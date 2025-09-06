using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Models;
using Entities.Request;
using Entities.Response;
using ReaderServices;
using Repositories;
using Microsoft.Extensions.Logging;

namespace WriterServices
{
    public class WriterService : IWriterService
    {
        private readonly IRepository _repository;
        private readonly IReaderService _readerService;
        private readonly ILogger<WriterService> _logger;

        public WriterService(IRepository repository, IReaderService readerService, ILogger<WriterService> logger)
        {
            _repository = repository;
            _readerService = readerService;
            _logger = logger;
        }

        #region Appointment
        public async Task<PostAppointmentResponse> PostAppointmentAsync(PostAppointmentRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "[WriterService.PostAppointmentAsync] Creating appointment for AgencyId={AgencyId}, CustomerId={CustomerId}, Date={Date}",
                    request.AgencyId, request.CustomerId, request.DesiredDate.ToString("yyyy-MM-dd")
                );

                // Check if agency exists
                var agencyExists = await _repository.AgencyExistsAsync(request.AgencyId);
                if (!agencyExists)
                {
                    _logger.LogWarning("[WriterService.PostAppointmentAsync] Agency not found. AgencyId={AgencyId}", request.AgencyId);
                    throw new KeyNotFoundException($"Agency with id {request.AgencyId} was not found.");
                }

                // Check if customer exists
                var customerExists = await _repository.CustomerExistsAsync(request.CustomerId);
                if (!customerExists)
                {
                    _logger.LogWarning("[WriterService.PostAppointmentAsync] Customer not found. CustomerId={CustomerId}", request.CustomerId);
                    throw new KeyNotFoundException($"Customer with id {request.CustomerId} was not found.");
                }

                // Check if date is a holiday
                if (await _readerService.IsHolidayAsync(request.AgencyId, request.DesiredDate))
                {
                    _logger.LogInformation(
                        "[WriterService.PostAppointmentAsync] Requested date {Date} is holiday, finding next available date...",
                        request.DesiredDate.ToString("yyyy-MM-dd")
                    );
                    request.DesiredDate = await FindNextAvailableDateAsync(request.AgencyId, request.DesiredDate);
                }

                // Get agency settings
                var agencySetting = await _readerService.GetAgencySettingAsync(request.AgencyId);
                int maxAppointments = agencySetting?.MaxAppointments ?? 10;

                // Check daily quota - only count active appointments (not cancelled)
                var appointments = await _repository.GetAppointmentsByDateAsync(request.AgencyId, request.DesiredDate);
                var activeAppointments = appointments.Where(a => a.Status != "Cancelled").ToList();

                if (activeAppointments.Count >= maxAppointments)
                {
                    _logger.LogInformation(
                        "[WriterService.PostAppointmentAsync] Daily quota reached for {Date}, searching next available date...",
                        request.DesiredDate.ToString("yyyy-MM-dd")
                    );

                    request.DesiredDate = await FindNextAvailableDateAsync(request.AgencyId, request.DesiredDate.AddDays(1));
                    appointments = await _repository.GetAppointmentsByDateAsync(request.AgencyId, request.DesiredDate);
                    activeAppointments = appointments.Where(a => a.Status != "Cancelled").ToList();
                }

                // Generate token number
                var datePrefix = request.DesiredDate.ToString("yyyyMMdd");
                int nextSeq = 1;

                // Use only active appointments to determine next sequence number
                if (activeAppointments.Any())
                {
                    var lastToken = activeAppointments.Max(a => a.TokenNumber);
                    var lastSeq = lastToken % 100;
                    nextSeq = lastSeq + 1;
                }

                int tokenNumber = int.Parse($"{datePrefix}{nextSeq:D2}");

                // Create appointment
                var appointment = new Appointment
                {
                    AgencyId = request.AgencyId,
                    CustomerId = request.CustomerId,
                    ApptDate = request.DesiredDate,
                    TokenNumber = tokenNumber,
                    Status = "Booked",
                    Notes = request.Notes,
                    CreatedBy = 1, // Replace with actual user ID from authentication
                    CreatedOn = DateTime.UtcNow
                };

                var createdAppointment = await _repository.PostAppointmentAsync(appointment);

                _logger.LogInformation(
                    "[WriterService.PostAppointmentAsync] Appointment created successfully with Id={AppointmentId}, Token={TokenNumber}",
                    createdAppointment.Id, createdAppointment.TokenNumber
                );

                return new PostAppointmentResponse
                {
                    AppointmentId = createdAppointment.Id,
                    AgencyId = createdAppointment.AgencyId,
                    CustomerId = createdAppointment.CustomerId,
                    AppointmentDate = createdAppointment.ApptDate,
                    TokenNumber = createdAppointment.TokenNumber,
                    Status = createdAppointment.Status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[WriterService.PostAppointmentAsync] Error creating appointment");
                throw;
            }
        }
        public async Task<PostAppointmentResponse> UpdateAppointmentAsync(int appointmentId, UpdateAppointmentRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "[WriterService.UpdateAppointmentAsync] Updating appointment with Id={AppointmentId}",
                    appointmentId
                );

                // Get existing appointment
                var existingAppointment = await _repository.GetAppointmentByIdAsync(appointmentId);
                if (existingAppointment == null)
                {
                    _logger.LogWarning("[WriterService.UpdateAppointmentAsync] Appointment not found. Id={AppointmentId}", appointmentId);
                    throw new KeyNotFoundException($"Appointment with id {appointmentId} was not found.");
                }

                DateTime newAppointmentDate = existingAppointment.ApptDate;
                bool dateChanged = false;

                // If appointment date is being changed
                if (request.AppointmentDate.HasValue && request.AppointmentDate.Value != existingAppointment.ApptDate)
                {
                    dateChanged = true;
                    newAppointmentDate = request.AppointmentDate.Value;

                    // Check if new date is a holiday
                    if (await _readerService.IsHolidayAsync(existingAppointment.AgencyId, newAppointmentDate))
                    {
                        _logger.LogInformation(
                            "[WriterService.UpdateAppointmentAsync] Requested date {Date} is holiday, finding next available date...",
                            newAppointmentDate.ToString("yyyy-MM-dd")
                        );
                        newAppointmentDate = await FindNextAvailableDateAsync(existingAppointment.AgencyId, newAppointmentDate);
                    }

                    // Get agency settings
                    var agencySetting = await _readerService.GetAgencySettingAsync(existingAppointment.AgencyId);
                    int maxAppointments = agencySetting?.MaxAppointments ?? 10;

                    // Check daily quota for new date
                    var appointments = await _repository.GetAppointmentsByDateAsync(existingAppointment.AgencyId, newAppointmentDate);
                    var activeAppointments = appointments.Where(a => a.Status != "Cancelled").ToList();

                    if (activeAppointments.Count >= maxAppointments)
                    {
                        _logger.LogInformation(
                            "[WriterService.UpdateAppointmentAsync] Daily quota reached for {Date}, searching next available date...",
                            newAppointmentDate.ToString("yyyy-MM-dd")
                        );

                        newAppointmentDate = await FindNextAvailableDateAsync(existingAppointment.AgencyId, newAppointmentDate.AddDays(1));
                    }

                    // Generate new token number if date changed
                    if (newAppointmentDate != existingAppointment.ApptDate)
                    {
                        var datePrefix = newAppointmentDate.ToString("yyyyMMdd");
                        int nextSeq = 1;

                        var newDateAppointments = await _repository.GetAppointmentsByDateAsync(existingAppointment.AgencyId, newAppointmentDate);
                        if (newDateAppointments.Any())
                        {
                            var lastToken = newDateAppointments.Max(a => a.TokenNumber);
                            var lastSeq = lastToken % 100;
                            nextSeq = lastSeq + 1;
                        }

                        existingAppointment.TokenNumber = int.Parse($"{datePrefix}{nextSeq:D2}");
                    }
                }

                // Update appointment fields
                if (dateChanged)
                {
                    existingAppointment.ApptDate = newAppointmentDate;
                }

                if (!string.IsNullOrEmpty(request.Status))
                {
                    existingAppointment.Status = request.Status;
                }

                if (request.Notes != null)
                {
                    existingAppointment.Notes = request.Notes;
                }

                existingAppointment.ModifiedOn = DateTime.UtcNow;
                existingAppointment.ModifiedBy = 1; // Replace with actual user ID

                // Save changes
                var updatedAppointment = await _repository.UpdateAppointmentAsync(existingAppointment);

                _logger.LogInformation(
                    "[WriterService.UpdateAppointmentAsync] Appointment updated successfully with Id={AppointmentId}",
                    updatedAppointment.Id
                );

                return new PostAppointmentResponse
                {
                    AppointmentId = updatedAppointment.Id,
                    AgencyId = updatedAppointment.AgencyId,
                    CustomerId = updatedAppointment.CustomerId,
                    AppointmentDate = updatedAppointment.ApptDate,
                    TokenNumber = updatedAppointment.TokenNumber,
                    Status = updatedAppointment.Status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[WriterService.UpdateAppointmentAsync] Error updating appointment");
                throw;
            }
        }

        private async Task<DateTime> FindNextAvailableDateAsync(int agencyId, DateTime startDate)
        {
            try
            {
                var date = startDate;
                int maxAttempts = 20;

                for (int i = 0; i < maxAttempts; i++)
                {
                    if (await _readerService.IsHolidayAsync(agencyId, date))
                    {
                        _logger.LogInformation("[WriterService.FindNextAvailableDateAsync] {Date} is holiday, checking next day...", date.ToString("yyyy-MM-dd"));
                        date = date.AddDays(1);
                        continue;
                    }

                    // Check daily quota - only count active appointments (not cancelled)
                    var appointments = await _repository.GetAppointmentsByDateAsync(agencyId, date);
                    var activeAppointments = appointments.Where(a => a.Status != "Cancelled").ToList();

                    var agencySetting = await _readerService.GetAgencySettingAsync(agencyId);
                    int maxAppointments = agencySetting?.MaxAppointments ?? 10;

                    if (activeAppointments.Count < maxAppointments)
                    {
                        _logger.LogInformation("[WriterService.FindNextAvailableDateAsync] Found available date: {Date}", date.ToString("yyyy-MM-dd"));
                        return date;
                    }

                    _logger.LogInformation("[WriterService.FindNextAvailableDateAsync] {Date} is full, checking next day...", date.ToString("yyyy-MM-dd"));
                    date = date.AddDays(1);
                }

                throw new Exception("No available appointment slots found in the next 20 days");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[WriterService.FindNextAvailableDateAsync] Error finding available date for AgencyId={AgencyId}", agencyId);
                throw;
            }
        }
        #endregion

        #region Holiday
        public async Task<HolidayResponse> CreateHolidayAsync(CreateHolidayRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "[WriterService.CreateHolidayAsync] Creating holiday for AgencyId={AgencyId}, Date={Date}",
                    request.AgencyId, request.OffDate.ToString("yyyy-MM-dd")
                );


                // Check if agency exists
                var agencyExists = await _repository.AgencyExistsAsync(request.AgencyId);
                if (!agencyExists)
                {
                    _logger.LogWarning("[WriterService.CreateHolidayAsync] Agency not found. AgencyId={AgencyId}", request.AgencyId);
                    throw new KeyNotFoundException($"Agency with id {request.AgencyId} was not found.");
                }

                // Check if holiday already exists
                var existingHoliday = await _repository.GetHolidayByDateAsync(request.AgencyId, request.OffDate);
                if (existingHoliday != null)
                {
                    _logger.LogWarning(
                        "[WriterService.CreateHolidayAsync] Holiday already exists for AgencyId={AgencyId}, Date={Date}",
                        request.AgencyId, request.OffDate.ToString("yyyy-MM-dd")
                    );
                    throw new Exception("Holiday already exists for this date");
                }

                // Create holiday
                var holiday = new Holiday
                {
                    AgencyId = request.AgencyId,
                    OffDate = request.OffDate,
                    Reason = request.Reason,
                    CreatedBy = 1, 
                    CreatedOn = DateTime.UtcNow
                };

                var createdHoliday = await _repository.CreateHolidayAsync(holiday);

                _logger.LogInformation(
                    "[WriterService.CreateHolidayAsync] Holiday created successfully with Id={HolidayId}",
                    createdHoliday.Id
                );

                return new HolidayResponse
                {
                    Id = createdHoliday.Id,
                    AgencyId = createdHoliday.AgencyId,
                    OffDate = createdHoliday.OffDate,
                    Reason = createdHoliday.Reason ?? string.Empty,
                    CreatedOn = createdHoliday.CreatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[WriterService.CreateHolidayAsync] Error creating holiday");
                throw;
            }
        }

        public async Task<bool> DeleteHolidayAsync(int holidayId)
        {
            try
            {
                _logger.LogInformation("[WriterService.DeleteHolidayAsync] Deleting holiday with Id={HolidayId}", holidayId);

                return await _repository.DeleteHolidayAsync(holidayId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[WriterService.DeleteHolidayAsync] Error deleting holiday with Id={HolidayId}", holidayId);
                throw;
            }
        }
        #endregion

        #region Agency Settings
        public async Task<AgencySettingResponse> UpdateAgencySettingsAsync(int agencyId, UpdateAgencySettingsRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "[WriterService.UpdateAgencySettingsAsync] Updating settings for AgencyId={AgencyId}, MaxAppointments={MaxAppointments}",
                    agencyId, request.MaxAppointments
                );

                var agencyExists = await _repository.AgencyExistsAsync(agencyId);
                if (!agencyExists)
                {
                    _logger.LogWarning("[WriterService.UpdateAgencySettingsAsync] Agency not found. AgencyId={AgencyId}", agencyId);
                    throw new KeyNotFoundException($"Agency with id {agencyId} was not found.");
                }

                var updatedSetting = await _repository.UpdateAgencySettingsAsync(agencyId, request.MaxAppointments);

                _logger.LogInformation(
                    "[WriterService.UpdateAgencySettingsAsync] Settings updated successfully for AgencyId={AgencyId}",
                    agencyId
                );

                return new AgencySettingResponse
                {
                    AgencyId = updatedSetting.AgencyId,
                    MaxAppointments = updatedSetting.MaxAppointments
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[WriterService.UpdateAgencySettingsAsync] Error updating settings for AgencyId={AgencyId}", agencyId);
                throw;
            }
        }
        #endregion
    }
}

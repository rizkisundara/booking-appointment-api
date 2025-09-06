using Entities.Models;
using Entities.Response;
using Repositories;
using Microsoft.Extensions.Logging; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReaderServices
{
    public class ReaderService : IReaderService
    {
        private readonly IRepository _repository;
        private readonly ILogger<ReaderService> _logger;

        public ReaderService(IRepository repository, ILogger<ReaderService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        #region Appointment
        public async Task<IReadOnlyList<GetAppointmentsByDateResponse>> GetAppointmentsByDateAsync(int agencyId, DateTime date)
        {
            try
            {
                _logger.LogInformation("[ReaderService.GetAppointmentsByDateAsync] Fetching appointments for AgencyId={AgencyId}, Date={Date}",
                    agencyId, date.ToString("yyyy-MM-dd"));

                var appointments = await _repository.GetAppointmentsByDateAsync(agencyId, date);

                if (appointments == null || !appointments.Any())
                {
                    _logger.LogInformation("[ReaderService.GetAppointmentsByDateAsync] No appointments found for AgencyId={AgencyId}, Date={Date}",
                        agencyId, date.ToString("yyyy-MM-dd"));
                    return new List<GetAppointmentsByDateResponse>();
                }

                return appointments.Select(a => new GetAppointmentsByDateResponse
                {
                    AppointmentId = a.Id,
                    AgencyId = a.AgencyId,
                    AgencyName = a.Agency?.Name ?? string.Empty,
                    CustomerId = a.CustomerId,
                    CustomerName = a.Customer?.FullName ?? string.Empty,
                    CustomerPhone = a.Customer?.Phone ?? string.Empty,
                    AppointmentDate = a.ApptDate,
                    TokenNumber = a.TokenNumber,
                    Status = a.Status,
                    Notes = a.Notes ?? string.Empty,
                    CreatedOn = a.CreatedOn
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReaderService.GetAppointmentsByDateAsync] Error for AgencyId={AgencyId}, Date={Date}",
                    agencyId, date.ToString("yyyy-MM-dd"));
                throw;
            }
        }

        public async Task<IReadOnlyList<GetAllAppointmentResponse>> GetAllAppointmentsByDateAsync(DateTime date)
        {
            try
            {
                _logger.LogInformation("[ReaderService.GetAllAppointmentsByDateAsync] Fetching all appointments for Date={Date}", date.ToString("yyyy-MM-dd"));

                var appointments = await _repository.GetAllAppointmentsByDateAsync(date);

                if (appointments == null || !appointments.Any())
                {
                    _logger.LogInformation("[ReaderService.GetAllAppointmentsByDateAsync] No appointments found for Date={Date}", date.ToString("yyyy-MM-dd"));
                    return new List<GetAllAppointmentResponse>();
                }

                return appointments.Select(a => new GetAllAppointmentResponse
                {
                    AppointmentId = a.Id,
                    AgencyId = a.AgencyId,
                    AgencyName = a.Agency?.Name ?? string.Empty,
                    CustomerId = a.CustomerId,
                    CustomerName = a.Customer?.FullName ?? string.Empty,
                    CustomerPhone = a.Customer?.Phone ?? string.Empty,
                    AppointmentDate = a.ApptDate,
                    TokenNumber = a.TokenNumber,
                    Status = a.Status,
                    Notes = a.Notes ?? string.Empty,
                    CreatedOn = a.CreatedOn
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReaderService.GetAllAppointmentsByDateAsync] Error for Date={Date}", date.ToString("yyyy-MM-dd"));
                throw;
            }
        }
        #endregion

        #region Agency Settings
        public async Task<AgencySetting> GetAgencySettingAsync(int agencyId)
        {
            try
            {
                _logger.LogInformation("[ReaderService.GetAgencySettingAsync] Fetching agency setting for AgencyId={AgencyId}", agencyId);

                var agencySetting = await _repository.GetAgencySettingAsync(agencyId);

                if (agencySetting == null)
                {
                    _logger.LogWarning("[ReaderService.GetAgencySettingAsync] No agency setting found for AgencyId={AgencyId}. Using default values.", agencyId);

                    return new AgencySetting
                    {
                        AgencyId = agencyId,
                        MaxAppointments = 10
                    };
                }

                return agencySetting;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReaderService.GetAgencySettingAsync] Error for AgencyId={AgencyId}", agencyId);
                throw;
            }
        }
        #endregion

        #region Holiday
        public async Task<bool> IsHolidayAsync(int agencyId, DateTime date)
        {
            try
            {
                _logger.LogInformation("[ReaderService.IsHolidayAsync] Checking holiday for AgencyId={AgencyId}, Date={Date}",agencyId, date.ToString("yyyy-MM-dd"));

                return await _repository.IsHolidayAsync(agencyId, date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReaderService.IsHolidayAsync] Error for AgencyId={AgencyId}, Date={Date}", agencyId, date.ToString("yyyy-MM-dd"));
                throw;
            }
        }
        public async Task<IReadOnlyList<HolidayResponse>> GetHolidaysAsync(int agencyId)
        {
            try
            {
                _logger.LogInformation("[ReaderService.GetHolidaysAsync] Fetching holidays for AgencyId={AgencyId}", agencyId);

                var holidays = await _repository.GetHolidaysAsync(agencyId);

                if (holidays == null || !holidays.Any())
                {
                    _logger.LogInformation("[ReaderService.GetHolidaysAsync] No holidays found for AgencyId={AgencyId}", agencyId);
                    return new List<HolidayResponse>();
                }

                return holidays.Select(h => new HolidayResponse
                {
                    Id = h.Id,
                    AgencyId = h.AgencyId,
                    OffDate = h.OffDate,
                    Reason = h.Reason ?? string.Empty,
                    CreatedOn = h.CreatedOn
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReaderService.GetHolidaysAsync] Error for AgencyId={AgencyId}", agencyId);
                throw;
            }
        }
        #endregion
    }
}

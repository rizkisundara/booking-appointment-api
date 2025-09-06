using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories
{
    public class Repository : IRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<Repository> _logger;

        public Repository(ApplicationDbContext db, ILogger<Repository> logger)
        {
            _db = db;
            _logger = logger;
        }

        #region Appointment
        public async Task<List<Appointment>> GetAppointmentsByDateAsync(int agencyId, DateTime date)
        {
            try
            {
                return await _db.Appointments
                    .AsNoTracking()
                    .Include(a => a.Customer)
                    .Include(a => a.Agency)
                    .Where(a => a.AgencyId == agencyId &&
                                a.ApptDate.Date == date.Date &&
                                !a.IsDelete)
                    .OrderBy(a => a.TokenNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Repository:GetAppointmentsByDateAsync] Error retrieving appointments for AgencyId={AgencyId} on {Date}", agencyId, date);
                throw;
            }
        }

        public async Task<List<Appointment>> GetAllAppointmentsByDateAsync(DateTime date)
        {
            try
            {
                return await _db.Appointments
                    .AsNoTracking()
                    .Include(a => a.Customer)
                    .Include(a => a.Agency)
                    .Where(a => a.ApptDate.Date == date.Date && !a.IsDelete)
                    .OrderBy(a => a.AgencyId)
                    .ThenBy(a => a.TokenNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Repository:GetAllAppointmentsByDateAsync] Error retrieving appointments for {Date}", date);
                throw;
            }
        }

        public async Task<Appointment> PostAppointmentAsync(Appointment appointment)
        {
            try
            {
                _db.Appointments.Add(appointment);
                await _db.SaveChangesAsync();
                return appointment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Repository:CreateAppointmentAsync] Error creating appointment for AgencyId={AgencyId}", appointment.AgencyId);
                throw;
            }
        }

        public async Task<Appointment> GetAppointmentByIdAsync(int appointmentId)
        {
            try
            {
                return await _db.Appointments
                    .Include(a => a.Customer)
                    .Include(a => a.Agency)
                    .FirstOrDefaultAsync(a => a.Id == appointmentId && !a.IsDelete);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Repository:GetAppointmentByIdAsync] Error retrieving appointment with Id={AppointmentId}", appointmentId);
                throw;
            }
        }

        public async Task<Appointment> UpdateAppointmentAsync(Appointment appointment)
        {
            try
            {
                _db.Appointments.Update(appointment);
                await _db.SaveChangesAsync();
                return appointment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Repository:UpdateAppointmentAsync] Error updating appointment with Id={AppointmentId}", appointment.Id);
                throw;
            }
        }
        #endregion

        #region Agency Settings
        public async Task<AgencySetting> GetAgencySettingAsync(int agencyId)
        {
            try
            {
                return await _db.AgencySettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.AgencyId == agencyId && !a.IsDelete);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Repository:GetAgencySettingAsync] Error retrieving settings for AgencyId={AgencyId}", agencyId);
                throw;
            }
        }
        public async Task<bool> AgencyExistsAsync(int agencyId)
        {
            try
            {
                return await _db.Agencies
                    .AsNoTracking()
                    .AnyAsync(a => a.Id == agencyId && !a.IsDelete && a.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Repository:AgencyExistsAsync] Error checking agency existence for AgencyId={AgencyId}", agencyId);
                throw;
            }
        }
        public async Task<AgencySetting> UpdateAgencySettingsAsync(int agencyId, int maxAppointments)
        {
            try
            {
                var existingSetting = await _db.AgencySettings
                    .FirstOrDefaultAsync(a => a.AgencyId == agencyId && !a.IsDelete);

                if (existingSetting != null)
                {
                    existingSetting.MaxAppointments = maxAppointments;
                    existingSetting.ModifiedOn = DateTime.UtcNow;
                    existingSetting.ModifiedBy = 1; // This should be replaced with actual user ID from authentication
                }
                else
                {
                    var newSetting = new AgencySetting
                    {
                        AgencyId = agencyId,
                        MaxAppointments = maxAppointments,
                        CreatedBy = 1, // This should be replaced with actual user ID from authentication
                        CreatedOn = DateTime.UtcNow
                    };
                    _db.AgencySettings.Add(newSetting);
                    await _db.SaveChangesAsync();
                    return newSetting;
                }

                await _db.SaveChangesAsync();
                return existingSetting;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Repository:UpdateAgencySettingsAsync] Error updating settings for AgencyId={AgencyId}", agencyId);
                throw;
            }
        }
        #endregion

        #region Holiday
        public async Task<bool> IsHolidayAsync(int agencyId, DateTime date)
        {
            try
            {
                return await _db.Holidays
                    .AsNoTracking()
                    .AnyAsync(h => h.AgencyId == agencyId &&
                                  h.OffDate.Date == date.Date &&
                                  !h.IsDelete);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Repository:IsHolidayAsync] Error checking holiday for AgencyId={AgencyId} on {Date}", agencyId, date);
                throw;
            }
        }
        public async Task<List<Holiday>> GetHolidaysAsync(int agencyId)
        {
            try
            {
                return await _db.Holidays
                    .AsNoTracking()
                    .Where(h => h.AgencyId == agencyId && !h.IsDelete)
                    .OrderBy(h => h.OffDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Repository:GetHolidaysAsync] Error retrieving holidays for AgencyId={AgencyId}", agencyId);
                throw;
            }
        }

        public async Task<Holiday> GetHolidayByDateAsync(int agencyId, DateTime date)
        {
            try
            {
                return await _db.Holidays
                    .FirstOrDefaultAsync(h => h.AgencyId == agencyId &&
                                             h.OffDate.Date == date.Date &&
                                             !h.IsDelete);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Repository:GetHolidayByDateAsync] Error retrieving holiday for AgencyId={AgencyId}, Date={Date}",
                    agencyId, date.ToString("yyyy-MM-dd"));
                throw;
            }
        }

        public async Task<Holiday> CreateHolidayAsync(Holiday holiday)
        {
            try
            {
                _db.Holidays.Add(holiday);
                await _db.SaveChangesAsync();
                return holiday;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Repository:CreateHolidayAsync] Error creating holiday for AgencyId={AgencyId}", holiday.AgencyId);
                throw;
            }
        }

        public async Task<bool> DeleteHolidayAsync(int holidayId)
        {
            try
            {
                var holiday = await _db.Holidays.FindAsync(holidayId);
                if (holiday == null || holiday.IsDelete)
                {
                    return false;
                }

                holiday.IsDelete = true;
                holiday.DeletedOn = DateTime.UtcNow;
                holiday.DeletedBy = 1; 

                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Repository:DeleteHolidayAsync] Error deleting holiday with Id={HolidayId}", holidayId);
                throw;
            }
        }
        #endregion

        #region Customer
        public async Task<bool> CustomerExistsAsync(int customerId)
        {
            try
            {
                return await _db.Customers
                    .AsNoTracking()
                    .AnyAsync(c => c.Id == customerId && !c.IsDelete);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Repository:CustomerExistsAsync] Error checking customer existence for CustomerId={CustomerId}", customerId);
                throw;
            }
        }
        #endregion
    }
}

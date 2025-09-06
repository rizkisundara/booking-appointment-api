using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IRepository
    {

        #region Appointment
        Task<List<Appointment>> GetAppointmentsByDateAsync(int agencyId, DateTime date);
        Task<List<Appointment>> GetAllAppointmentsByDateAsync(DateTime date);
        Task<Appointment> PostAppointmentAsync(Appointment appointment);
        Task<Appointment> GetAppointmentByIdAsync(int appointmentId);
        Task<Appointment> UpdateAppointmentAsync(Appointment appointment);
        #endregion

        #region Agency Settings
        Task<AgencySetting> GetAgencySettingAsync(int agencyId);
        Task<bool> AgencyExistsAsync(int agencyId);
        Task<AgencySetting> UpdateAgencySettingsAsync(int agencyId, int maxAppointments);
        #endregion

        #region Holiday
        Task<bool> IsHolidayAsync(int agencyId, DateTime date);
        Task<List<Holiday>> GetHolidaysAsync(int agencyId);
        Task<Holiday> GetHolidayByDateAsync(int agencyId, DateTime date);
        Task<Holiday> CreateHolidayAsync(Holiday holiday);
        Task<bool> DeleteHolidayAsync(int holidayId);
        #endregion

        #region Customer 
        Task<bool> CustomerExistsAsync(int customerId);
        #endregion
    }
}

using Entities.Models;
using Entities.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReaderServices
{
    public interface IReaderService
    {
        #region Appointment
        Task<IReadOnlyList<GetAppointmentsByDateResponse>> GetAppointmentsByDateAsync(int agencyId, DateTime date);
        Task<IReadOnlyList<GetAllAppointmentResponse>> GetAllAppointmentsByDateAsync(DateTime date);
        #endregion

        #region Agency Settings
        Task<AgencySetting> GetAgencySettingAsync(int agencyId);
        #endregion

        #region Holiday
        Task<bool> IsHolidayAsync(int agencyId, DateTime date);
        Task<IReadOnlyList<HolidayResponse>> GetHolidaysAsync(int agencyId);
        #endregion
    }
}

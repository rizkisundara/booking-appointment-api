using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Request;  
using Entities.Response;  

namespace WriterServices
{
    public interface IWriterService
    {
        #region Appointment
        Task<PostAppointmentResponse> PostAppointmentAsync(PostAppointmentRequest request);
        Task<PostAppointmentResponse> UpdateAppointmentAsync(int appointmentId, UpdateAppointmentRequest request);
        #endregion

        #region Holiday
        Task<HolidayResponse> CreateHolidayAsync(CreateHolidayRequest request);
        Task<bool> DeleteHolidayAsync(int holidayId);
        #endregion

        #region Agency Setting
        Task<AgencySettingResponse> UpdateAgencySettingsAsync(int agencyId, UpdateAgencySettingsRequest request);
        #endregion
    }
}

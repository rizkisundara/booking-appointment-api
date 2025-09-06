using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Request
{
    public class UpdateAppointmentRequest
    {
        public DateTime? AppointmentDate { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }
}

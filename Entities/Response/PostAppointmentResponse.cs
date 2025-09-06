using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Response
{
    public class PostAppointmentResponse
    {
        public int AppointmentId { get; set; }
        public int AgencyId { get; set; }
        public string? AgencyName { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; } 
        public DateTime AppointmentDate { get; set; }
        public int TokenNumber { get; set; }
        public string Status { get; set; }
    }
}

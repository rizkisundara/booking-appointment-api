using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Request
{
    public class PostAppointmentRequest
    {
        [Required]
        public int AgencyId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public DateTime DesiredDate { get; set; }

        public string Notes { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Request
{
    public class CreateHolidayRequest
    {
        public int AgencyId { get; set; }
        public DateTime OffDate { get; set; }
        public string Reason { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Response
{
    public class HolidayResponse
    {
        public int Id { get; set; }
        public int AgencyId { get; set; }
        public DateTime OffDate { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}

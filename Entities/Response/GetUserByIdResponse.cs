using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Response
{
    public class GetUserByIdResponse
    {
        public long Id { get; set; }
        public string? Email { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}

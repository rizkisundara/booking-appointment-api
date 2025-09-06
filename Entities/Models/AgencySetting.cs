using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    [Table("agency_setting")]
    public class AgencySetting
    {
        [Key]
        [Column("agency_id")]
        public int AgencyId { get; set; }

        [Column("max_appointments")]
        public int MaxAppointments { get; set; } = 10;

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("created_on")]
        public DateTime CreatedOn { get; set; }

        [Column("modified_by")]
        public int? ModifiedBy { get; set; }

        [Column("modified_on")]
        public DateTime? ModifiedOn { get; set; }

        [Column("deleted_by")]
        public int? DeletedBy { get; set; }

        [Column("deleted_on")]
        public DateTime? DeletedOn { get; set; }

        [Column("is_delete")]
        public bool IsDelete { get; set; } = false;
    }
}

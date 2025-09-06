using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    [Table("agency_quota_override", Schema = "dbo")]
    public class AgencyQuotaOverride
    {
        [Column("agency_id")]
        [Key]
        public int AgencyId { get; set; }

        [Column("appt_date")]
        public DateTime ApptDate { get; set; }

        [Column("max_appointments")]
        public int MaxAppointments { get; set; }

        [Column("created_on")]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [Column("modified_on")]
        public DateTime? ModifiedOn { get; set; }

        [Column("deleted_on")]
        public DateTime? DeletedOn { get; set; }

        [Column("is_delete")]
        public bool IsDelete { get; set; } = false;
    }
}

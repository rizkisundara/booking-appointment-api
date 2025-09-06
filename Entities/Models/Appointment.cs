using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    [Table("appointment")]
    public class Appointment
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("agency_id")]
        public int AgencyId { get; set; }

        [Column("customer_id")]
        public int CustomerId { get; set; }

        [Column("appt_date")]
        public DateTime ApptDate { get; set; }

        [Column("token_number")]
        public int TokenNumber { get; set; }

        [Required, MaxLength(30)]
        [Column("status")]
        public string Status { get; set; } = "Booked";

        [MaxLength(500)]
        [Column("notes")]
        public string? Notes { get; set; }

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

        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; } = default!;

        [ForeignKey(nameof(AgencyId))]
        public Agency Agency { get; set; } = default!;
    }
}

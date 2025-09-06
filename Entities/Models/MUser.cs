using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    [Table("m_user")]
    public class MUser
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }                           // BIGINT -> long

        [Column("biodata_id")]
        public long? BiodataId { get; set; }                   // BIGINT -> long?

        [Column("role_id")]
        public long? RoleId { get; set; }                      // BIGINT -> long?

        [Column("email")]
        public string? Email { get; set; }                     // VARCHAR NULL -> string?

        [Column("password")]
        public string? Password { get; set; }                  // VARCHAR NULL -> string?

        [Column("login_attempt")]
        public int? LoginAttempt { get; set; }                 // INT -> int?

        [Column("is_locked")]
        public bool? IsLocked { get; set; }                    // BIT NULL -> bool?

        [Column("last_login")]
        public DateTime? LastLogin { get; set; }               // DATETIME NULL -> DateTime?

        [Column("created_by")]
        public long CreatedBy { get; set; }                    // BIGINT NOT NULL -> long

        [Column("created_on")]
        public DateTime CreatedOn { get; set; }                // DATETIME NOT NULL -> DateTime

        [Column("modified_by")]
        public long? ModifiedBy { get; set; }                  // BIGINT NULL -> long?

        [Column("modified_on")]
        public DateTime? ModifiedOn { get; set; }              // DATETIME NULL -> DateTime?

        [Column("deleted_by")]
        public long? DeletedBy { get; set; }                   // BIGINT NULL -> long?

        [Column("deleted_on")]
        public DateTime? DeletedOn { get; set; }               // DATETIME NULL -> DateTime?

        [Column("is_delete")]
        public bool IsDelete { get; set; }                     // BIT NOT NULL -> bool
    }
}

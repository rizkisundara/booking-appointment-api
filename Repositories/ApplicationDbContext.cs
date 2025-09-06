using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }

        // DbSet for entity
        public DbSet<Agency> Agencies { get; set; } = default!;
        public DbSet<Customer> Customers { get; set; } = default!;
        public DbSet<AgencySetting> AgencySettings { get; set; } = default!;
        public DbSet<Holiday> Holidays { get; set; } = default!;
        public DbSet<Appointment> Appointments { get; set; } = default!;
        public DbSet<AgencyQuotaOverride> AgencyQuotaOverrides { get; set; } = default!;

    }
}

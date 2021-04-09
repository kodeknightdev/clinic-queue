using ClinicQueue.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicQueue.Web.Data
{
    public class ClinicDbContext : DbContext
    {
        public ClinicDbContext(DbContextOptions<ClinicDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }

        public DbSet<QueueEntry> QueueEntries { get; set; }
    }
}

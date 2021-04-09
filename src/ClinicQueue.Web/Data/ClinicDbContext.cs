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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>(b =>
            {
                b.Property(p => p.FullName).IsRequired().HasMaxLength(100);
                b.Property(p => p.Phone).HasMaxLength(20);
                b.Property(p => p.DateOfBirth).HasColumnType("date");
            });

            modelBuilder.Entity<QueueEntry>(b =>
            {
                b.Property(e => e.Reason).HasMaxLength(200);

                b.HasOne(e => e.Patient)
                    .WithMany(p => p.QueueEntries)
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

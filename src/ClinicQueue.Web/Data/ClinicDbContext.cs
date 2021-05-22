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

        public DbSet<StaffUser> StaffUsers { get; set; }

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
                b.Property(e => e.QueueDate).HasColumnType("date");
                b.Property(e => e.LookupCode).HasMaxLength(12);

                b.HasIndex(e => new { e.QueueDate, e.TicketNumber }).IsUnique();
                b.HasIndex(e => e.LookupCode).IsUnique();

                b.HasOne(e => e.Patient)
                    .WithMany(p => p.QueueEntries)
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StaffUser>(b =>
            {
                b.Property(u => u.Username).IsRequired().HasMaxLength(50);
                b.Property(u => u.DisplayName).IsRequired().HasMaxLength(100);
                b.Property(u => u.PasswordHash).IsRequired();

                b.HasIndex(u => u.Username).IsUnique();
            });
        }
    }
}

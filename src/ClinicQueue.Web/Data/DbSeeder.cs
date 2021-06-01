using ClinicQueue.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ClinicQueue.Web.Data
{
    /// <summary>
    /// Development-only: applies migrations and makes sure there's a staff
    /// account to sign in with.
    /// </summary>
    public class DbSeeder
    {
        private readonly ClinicDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<DbSeeder> _logger;

        public DbSeeder(ClinicDbContext db, IConfiguration config, ILogger<DbSeeder> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            await _db.Database.MigrateAsync();

            var username = _config["Seed:StaffUsername"];
            var password = _config["Seed:StaffPassword"];
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return;
            }

            if (await _db.StaffUsers.AnyAsync(u => u.Username == username))
            {
                return;
            }

            var user = new StaffUser
            {
                Username = username,
                DisplayName = _config["Seed:StaffDisplayName"] ?? "Front Desk",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            user.PasswordHash = new PasswordHasher<StaffUser>().HashPassword(user, password);

            _db.StaffUsers.Add(user);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Seeded staff user {Username}", username);
        }
    }
}

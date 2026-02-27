using Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seeding;

public class AdminSeeder : ISeeder
{
  private readonly DataContext _db;
  private readonly IConfiguration _config;

  public AdminSeeder(DataContext db, IConfiguration config)
  {
    _db = db;
    _config = config;
  }

  public async Task SeedAsync()
  {
    var username = _config["AdminSettings:DefaultUsername"] ?? "admin";
    var email = _config["AdminSettings:DefaultEmail"] ?? "admin@clinic.com";
    var plainPassword = _config["AdminSettings:DefaultPassword"] ?? "P@ssword123";

    if (await _db.Admins.AnyAsync(a => a.Username == username))
    {
      return;
    }

    var hasher = new PasswordHasher<Admin>();
    var admin = new Admin
    {
      Id = Guid.NewGuid(),
      Username = username,
      Email = email,
      PasswordHash = string.Empty
    };

    admin.PasswordHash = hasher.HashPassword(admin, plainPassword);

    _db.Admins.Add(admin);
    await _db.SaveChangesAsync();
  }
}
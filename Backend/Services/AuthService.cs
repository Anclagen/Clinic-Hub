using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthService
{
  private readonly DataContext _dataContext;
  private readonly JwtSettings _jwtSettings;

  public AuthService(DataContext dataContext, JwtSettings jwtSettings)
  {
    _dataContext = dataContext;
    _jwtSettings = jwtSettings;
  }

  public async Task<bool> ValidateUserAsync(Patient patient, string password)
  {

    var passwordHasher = new PasswordHasher<Patient>();
    var result = passwordHasher.VerifyHashedPassword(patient, patient.PasswordHash, password);

    return result == PasswordVerificationResult.Success;
  }

  public async Task<bool> ValidateAdminUserAsync(Admin admin, string password)
  {

    var passwordHasher = new PasswordHasher<Admin>();
    var result = passwordHasher.VerifyHashedPassword(admin, admin.PasswordHash, password);

    return result == PasswordVerificationResult.Success;
  }

  public async Task<bool> RegisterUserAsync(string email, string password, string first, string last, DateOnly dob)
  {
    var normalizedEmail = email.Trim().ToLower();

    var existingPatient = await _dataContext.Patients
        .FirstOrDefaultAsync(p => p.Email == normalizedEmail && !p.IsDeleted);

    if (existingPatient != null)
    {
      // If they are already a registered user (not a guest), we can't register again
      if (!existingPatient.IsGuest) return false;

      // guest to registered memember logic
      // Verify PII matches to ensure this is the same person
      bool piiMatches = existingPatient.Firstname.Equals(first, StringComparison.OrdinalIgnoreCase) &&
                        existingPatient.Lastname.Equals(last, StringComparison.OrdinalIgnoreCase) &&
                        existingPatient.DateOfBirth == dob;

      if (!piiMatches)
      {
        // Security: Email exists but details don't match. 
        return false;
      }

      // If it's the same person, set IsGuest false and add the password.
      var hasher = new PasswordHasher<Patient>();
      existingPatient.PasswordHash = hasher.HashPassword(existingPatient, password);
      existingPatient.IsGuest = false;

      await _dataContext.SaveChangesAsync();
      return true;
    }

    var newPatient = new Patient
    {
      Email = normalizedEmail,
      Firstname = first.Trim(),
      Lastname = last.Trim(),
      DateOfBirth = dob,
      IsGuest = false,
      IsDeleted = false
    };

    var newHasher = new PasswordHasher<Patient>();
    newPatient.PasswordHash = newHasher.HashPassword(newPatient, password);

    _dataContext.Patients.Add(newPatient);
    await _dataContext.SaveChangesAsync();
    return true;
  }

  public async Task<Patient> GetUserByEmailAsync(string email)
  {
    return await _dataContext.Patients.SingleOrDefaultAsync(u => u.Email == email);
  }

  public async Task<Admin> GetAdminUserByUsernameAsync(string username)
  {
    return await _dataContext.Admins.SingleOrDefaultAsync(u => u.Username == username);
  }

  private string CreateToken(IEnumerable<Claim> claims)
  {
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _jwtSettings.Issuer,
        audience: _jwtSettings.Audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  public string GenerateToken(Patient patient)
  {
    var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, patient.Id.ToString()),
        new Claim(ClaimTypes.Role, "Patient"),
        new Claim(JwtRegisteredClaimNames.Email, patient.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    return CreateToken(claims);
  }

  public string GenerateAdminToken(Admin admin)
  {
    var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, admin.Id.ToString()),
        new Claim(ClaimTypes.Role, "Admin"),
        new Claim(JwtRegisteredClaimNames.UniqueName, admin.Username),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    return CreateToken(claims);
  }
}


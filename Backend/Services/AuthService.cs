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

  public async Task<bool> ValidateUserAsync(string email, string password)
  {
    var patient = await _dataContext.Patients.SingleOrDefaultAsync(p => p.Email == email);
    if (patient == null)
      return false;


    var passwordHasher = new PasswordHasher<Patient>();
    var result = passwordHasher.VerifyHashedPassword(patient, patient.PasswordHash, password);

    return result == PasswordVerificationResult.Success;
  }

  public async Task<bool> RegisterUserAsync(string email, string password, string firstname, string lastname, DateOnly dateOfBirth)
  {
    //Check if email already exists
    if (await _dataContext.Patients.AnyAsync(p => p.Email == email))
      return false;

    //Create Patient
    var passwordHasher = new PasswordHasher<Patient>();
    var patient = new Patient
    {
      Email = email,
      PasswordHash = passwordHasher.HashPassword(null, password),
      Firstname = firstname,
      Lastname = lastname,
      IsGuest = false,
      IsDeleted = false,
      DateOfBirth = dateOfBirth
    };

    _dataContext.Patients.Add(patient);
    await _dataContext.SaveChangesAsync();
    return true;
  }

  //Used to retrieve the Username if it exists, when logging in
  public async Task<Patient> GetUserByEmailAsync(string email)
  {
    return await _dataContext.Patients.SingleOrDefaultAsync(u => u.Email == email);
  }

  public string GenerateToken(Patient patient, int role = 1)
  {
    string[]? roles = new[] { "Admin", "Patient" };
    var claims = new[]
    {
                new Claim(JwtRegisteredClaimNames.Sub, patient.Id.ToString()),
                new Claim(ClaimTypes.Role, roles[role]),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _jwtSettings.Issuer,
        audience: _jwtSettings.Audience,
        claims: claims,
        expires: DateTime.Now.AddMinutes(_jwtSettings.ExpiryMinutes),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}
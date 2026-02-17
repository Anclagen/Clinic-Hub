using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seeding;

public class SpecialitySeeder : ISeeder
{
  private readonly DataContext _db;
  public SpecialitySeeder(DataContext db) => _db = db;

  public async Task SeedAsync()
  {
    var existingNames = await _db.Specialities
        .Select(s => s.SpecialityName)
        .ToListAsync();

    var existingSet = existingNames.ToHashSet(StringComparer.OrdinalIgnoreCase);

    var toInsert = _specialities
        .Where(s => !existingSet.Contains(s.SpecialityName))
        .ToList();

    if (toInsert.Count > 0)
    {
      _db.Specialities.AddRange(toInsert);
      await _db.SaveChangesAsync();
    }
  }
  private static readonly List<Speciality> _specialities = new()
{
    new() { SpecialityName = "General Practice", Description = "Primary care and general medical consultations." },
    new() { SpecialityName = "Internal Medicine", Description = "Diagnosis and treatment of adult diseases." },
    new() { SpecialityName = "Pediatrics", Description = "Medical care for infants, children, and adolescents." },
    new() { SpecialityName = "Dermatology", Description = "Skin, hair, and nail conditions." },
    new() { SpecialityName = "Cardiology", Description = "Heart and cardiovascular system disorders." },
    new() { SpecialityName = "Orthopedics", Description = "Musculoskeletal system and joint conditions." },
    new() { SpecialityName = "Neurology", Description = "Disorders of the brain and nervous system." },
    new() { SpecialityName = "Psychiatry", Description = "Mental health assessment and treatment." },
    new() { SpecialityName = "Gynecology", Description = "Women's reproductive health." },
    new() { SpecialityName = "Ophthalmology", Description = "Eye health and vision care." },
    new() { SpecialityName = "ENT", Description = "Ear, nose, and throat medicine." },
    new() { SpecialityName = "Radiology", Description = "Medical imaging and diagnostic interpretation." },
    new() { SpecialityName = "Endocrinology", Description = "Hormonal and metabolic disorders." },
    new() { SpecialityName = "Gastroenterology", Description = "Digestive system disorders." },
    new() { SpecialityName = "Pulmonology", Description = "Respiratory system and lung diseases." }
};
}

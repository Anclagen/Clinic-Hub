using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seeding;

public class CategorySeeder : ISeeder
{
  private readonly DataContext _db;
  public CategorySeeder(DataContext db) => _db = db;

  public async Task SeedAsync()
  {
    // unique index on CategoryName, so use that as your natural key
    var existingNames = await _db.Categories
        .Select(c => c.CategoryName)
        .ToListAsync();

    var existingSet = existingNames.ToHashSet(StringComparer.OrdinalIgnoreCase);

    var toInsert = _categories
        .Where(c => !existingSet.Contains(c.CategoryName))
        .ToList();

    if (toInsert.Count > 0)
    {
      _db.Categories.AddRange(toInsert);
      await _db.SaveChangesAsync();
    }
  }
  private static readonly List<Category> _categories = new()
  {
      new() { CategoryName = "General Consultation", DefaultDuration = 20, Description = "Routine health consultation." },
      new() { CategoryName = "Follow-up Visit", DefaultDuration = 15, Description = "Follow-up after previous appointment." },
      new() { CategoryName = "Dental Checkup", DefaultDuration = 30, Description = "Routine dental examination." },
      new() { CategoryName = "Emergency Visit", DefaultDuration = 30, Description = "Urgent medical consultation." },
      new() { CategoryName = "Vaccination", DefaultDuration = 10, Description = "Immunization appointment." },
      new() { CategoryName = "Blood Test", DefaultDuration = 10, Description = "Routine lab sample collection." },
      new() { CategoryName = "Physical Examination", DefaultDuration = 30, Description = "Comprehensive health exam." },
      new() { CategoryName = "Dermatology Consultation", DefaultDuration = 25, Description = "Skin-related concerns." },
      new() { CategoryName = "Cardiology Consultation", DefaultDuration = 30, Description = "Heart-related assessment." },
      new() { CategoryName = "ENT Consultation", DefaultDuration = 20, Description = "Ear, nose, and throat assessment." },
      new() { CategoryName = "Orthopedic Consultation", DefaultDuration = 30, Description = "Bone and joint concerns." },
      new() { CategoryName = "Pediatric Consultation", DefaultDuration = 20, Description = "Child health consultation." },
      new() { CategoryName = "Gynecology Consultation", DefaultDuration = 30, Description = "Women's health consultation." },
      new() { CategoryName = "Mental Health Consultation", DefaultDuration = 40, Description = "Psychological support session." },
      new() { CategoryName = "Chronic Disease Review", DefaultDuration = 25, Description = "Ongoing condition management." },
      new() { CategoryName = "Prescription Renewal", DefaultDuration = 10, Description = "Medication renewal appointment." },
      new() { CategoryName = "Nutrition Consultation", DefaultDuration = 30, Description = "Dietary planning session." },
      new() { CategoryName = "Allergy Testing", DefaultDuration = 30, Description = "Allergy diagnostic appointment." },
      new() { CategoryName = "Minor Procedure", DefaultDuration = 40, Description = "Small in-clinic procedure." },
      new() { CategoryName = "Wound Care", DefaultDuration = 20, Description = "Treatment and dressing of wounds." },
      new() { CategoryName = "Post-Surgery Follow-up", DefaultDuration = 25, Description = "Follow-up after surgery." },
      new() { CategoryName = "Pre-Surgery Assessment", DefaultDuration = 30, Description = "Pre-operative check." },
      new() { CategoryName = "Telemedicine Consultation", DefaultDuration = 20, Description = "Remote consultation session." },
      new() { CategoryName = "Lab Result Review", DefaultDuration = 15, Description = "Review and explanation of lab results." },
      new() { CategoryName = "Health Screening", DefaultDuration = 30, Description = "Preventive screening appointment." }
  };
}

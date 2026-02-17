using System.Text;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seeding;

public class DoctorSeeder : ISeeder
{
  private readonly DataContext _db;

  public DoctorSeeder(DataContext db) => _db = db;

  public async Task SeedAsync()
  {
    var clinics = await _db.Clinics.AsNoTracking()
        .OrderBy(c => c.ClinicName)
        .ToListAsync();

    var specs = await _db.Specialities.AsNoTracking().ToListAsync();

    if (clinics.Count == 0 || specs.Count == 0)
      return;

    var specByName = specs.ToDictionary(s => s.SpecialityName, s => s, StringComparer.OrdinalIgnoreCase);

    if (!specByName.ContainsKey(GeneralPractice))
      throw new InvalidOperationException($"Missing required speciality '{GeneralPractice}'. Seed specialities first.");

    var needed = clinics.Count * DoctorsPerClinic;
    if (DoctorSeeds.Length < needed)
      throw new InvalidOperationException($"DoctorSeeds has {DoctorSeeds.Length} but needs {needed}. Add more doctors.");

    // avoid duplicates by (ClinicId + Firstname + Lastname)
    var existing = await _db.Doctors.AsNoTracking()
        .Select(d => new { d.ClinicId, d.Firstname, d.Lastname })
        .ToListAsync();

    var existingSet = new HashSet<string>(
        existing.Select(x => $"{x.ClinicId}|{x.Firstname}|{x.Lastname}"),
        StringComparer.OrdinalIgnoreCase
    );

    var toInsert = new List<Doctor>();
    var seedIndex = 0;

    for (int clinicIndex = 0; clinicIndex < clinics.Count; clinicIndex++)
    {
      var clinic = clinics[clinicIndex];

      for (int i = 0; i < DoctorsPerClinic; i++)
      {
        var seed = DoctorSeeds[seedIndex++];

        var specialityName = GetSpecialityFor(clinicIndex, i);
        if (!specByName.TryGetValue(specialityName, out var spec))
          spec = specByName[GeneralPractice]; // safe fallback

        var key = $"{clinic.Id}|{seed.Firstname}|{seed.Lastname}";
        if (existingSet.Contains(key))
          continue;

        toInsert.Add(new Doctor
        {
          Firstname = seed.Firstname,
          Lastname = seed.Lastname,
          ImageUrl = $"/images/doctors/{ToSlug(seed.Firstname)}_{ToSlug(seed.Lastname)}.jpg",
          ClinicId = clinic.Id,
          SpecialityId = spec.Id
        });

        existingSet.Add(key);
      }
    }

    if (toInsert.Count > 0)
    {
      _db.Doctors.AddRange(toInsert);
      await _db.SaveChangesAsync();
    }
  }

  private const int DoctorsPerClinic = 5;
  private const string GeneralPractice = "General Practice";

  private static readonly string[] SpecialityRotation =
  {
        GeneralPractice,
        "Internal Medicine",
        "Pediatrics",
        "Dermatology",
        "Cardiology",
        "Orthopedics",
        "Neurology",
        "ENT",
        "Gynecology",
        "Psychiatry",
        "Ophthalmology",
        "Endocrinology",
        "Gastroenterology",
        "Pulmonology",
        "Radiology"
    };
  private static readonly DoctorSeed[] DoctorSeeds =
  {
        new("Ingrid","Haugen"),
        new("Eirik","Berg"),
        new("Silje","Solberg"),
        new("Jonas","Hansen"),
        new("Nora","Johansen"),
        new("Anders","Olsen"),
        new("Mari","Larsen"),
        new("Henrik","Andersen"),
        new("Ida","Nilsen"),
        new("Sindre","Pedersen"),
        new("Thea","Moen"),
        new("Thomas","Kristiansen"),
        new("Camilla","Lie"),
        new("Kristian","Eide"),
        new("Emilie","Hovland"),
        new("Lars","Aas"),
        new("Hanne","Dahl"),
        new("Marius","Sæther"),
        new("Kari","Strand"),
        new("Ole","Bakke"),
        new("Ane","Bjerke"),
        new("Martin","Lunde"),
        new("Sara","Gundersen"),
        new("Magnus","Myhre"),
        new("Helene","Stensland"),
        new("Julie","Moe"),
        new("Petter","Iversen"),
        new("Cecilie","Rønning"),
        new("Sofia","Tangen"),
        new("Fredrik","Sundby"),
        new("Tone","Kvam"),
        new("Simen","Foss"),
        new("Ragnhild","Hjelle"),
        new("Daniel","Vik"),
        new("Malin","Hegg"),
        new("Linnea","Sørensen"),
        new("Stian","Nygård"),
        new("Hedda","Mikkelsen"),
        new("Bjørn","Gran"),
        new("Amalie","Bratt"),
        new("Ellen","Høgåsen"),
        new("Vilde","Skjelstad"),
        new("Arne","Haga"),
        new("Mina","Kleveland"),
        new("Tobias","Løken"),
        new("Aurora","Skaar"),
        new("Sebastian","Askeland"),
        new("Hilda","Ødegård"),
        new("Isak","Hjorth"),
        new("Frida","Meland"),
        new("Tuva","Høvik"),
        new("Adrian","Roald"),
        new("Maja","Aune"),
        new("Kasper","Sande"),
        new("Live","Fjeld"),
        new("Håkon","Holm"),
        new("Sanna","Nerhus"),
        new("Elias","Breivik"),
        new("Selma","Klepp"),
        new("Filip","Østby"),
        new("Oda","Skog"),
        new("Noah","Børresen"),
        new("Alma","Helle"),
        new("Liam","Rødsjø"),
        new("Ella","Lervik"),
        new("Iben","Vangen"),
        new("Pål","Tveit"),
        new("Astrid","Solli"),
        new("Sivert","Bøe"),
        new("Mathilde","Gjerstad"),
        new("Kaja","Haugland"),
        new("Even","Rasmussen"),
        new("Leah","Nygaard"),
        new("Joakim","Sjøberg"),
        new("Solveig","Brekke"),
    };

  private static string GetSpecialityFor(int clinicIndex, int doctorIndexWithinClinic)
  {
    // 1 GP per clinic
    if (doctorIndexWithinClinic == 0) return GeneralPractice;
    var idx = (clinicIndex * DoctorsPerClinic + doctorIndexWithinClinic) % SpecialityRotation.Length;
    var name = SpecialityRotation[idx];

    return name;
  }

  private static string ToSlug(string s)
  {
    s = s.Trim().ToLowerInvariant()
        .Replace("æ", "ae")
        .Replace("ø", "oe")
        .Replace("å", "aa");

    var sb = new StringBuilder(s.Length);
    foreach (var ch in s)
    {
      if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9'))
        sb.Append(ch);
    }
    return sb.ToString();
  }

  private readonly record struct DoctorSeed(string Firstname, string Lastname);
}

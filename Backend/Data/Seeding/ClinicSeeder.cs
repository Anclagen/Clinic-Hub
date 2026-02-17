using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seeding;

public class ClinicSeeder : ISeeder
{
    private readonly DataContext _db;
    public ClinicSeeder(DataContext db) => _db = db;

    public async Task SeedAsync()
    {
        // unique index on CategoryName, so use that as your natural key
        var existingNames = await _db.Clinics
            .Select(c => c.ClinicName)
            .ToListAsync();

        var existingSet = existingNames.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var toInsert = _clinics
            .Where(c => !existingSet.Contains(c.ClinicName))
            .ToList();

        if (toInsert.Count > 0)
        {
            _db.Clinics.AddRange(toInsert);
            await _db.SaveChangesAsync();
        }
    }
    private static readonly List<Clinic> _clinics = new()
  {
      new()
      {
          ClinicName = "Fjordbyen Legeklinikk",
          Address = "Bryggen 12, 5003 Bergen",
          ImageUrl = "/images/fjordbyen-legeklinikk.jpg",
          ImageAlt = "Main entrance of Fjordbyen Legeklinikk located near the Bergen harbourfront."
      },
      new()
      {
          ClinicName = "Nordlys Helsesenter",
          Address = "Storgata 18, 9008 Tromsø",
          ImageUrl = "/images/nordlys-helsesenter.jpg",
          ImageAlt = "Modern glass-front clinic building in Tromsø with snowy mountains in the background."
      },
      new()
      {
          ClinicName = "Lillehammer Medisinske Senter",
          Address = "Kirkegata 5, 2609 Lillehammer",
          ImageUrl = "/images/lillehammer-medisinske-senter.jpg",
          ImageAlt = "Clinic entrance beside a traditional wooden building in Lillehammer."
      },
      new()
      {
          ClinicName = "Sandvika Helsehus",
          Address = "Rådhusgata 22, 1337 Sandvika",
          ImageUrl = "/images/sandvika-helsehus.jpg",
          ImageAlt = "Contemporary health centre building near the Sandvika town centre."
      },
      new()
      {
          ClinicName = "Ålesund Familieklinikk",
          Address = "Keiser Wilhelms gate 14, 6003 Ålesund",
          ImageUrl = "/images/aalesund-familieklinikk.jpg",
          ImageAlt = "Clinic entrance overlooking Ålesund's coastal architecture."
      },
      new()
      {
          ClinicName = "Stavanger Sentrum Legesenter",
          Address = "Øvre Holmegate 9, 4006 Stavanger",
          ImageUrl = "/images/stavanger-sentrum-legesenter.jpg",
          ImageAlt = "Street-level clinic entrance in Stavanger city centre."
      },
      new()
      {
          ClinicName = "Trondheim Torg Helse",
          Address = "Kongens gate 11, 7013 Trondheim",
          ImageUrl = "/images/trondheim-torg-helse.jpg",
          ImageAlt = "Modern medical clinic located near Trondheim Torg shopping district."
      },
      new()
      {
          ClinicName = "Bodø Strandklinikk",
          Address = "Sjøgata 3, 8006 Bodø",
          ImageUrl = "/images/bodoe-strandklinikk.jpg",
          ImageAlt = "Clinic building close to the waterfront in Bodø."
      },
      new()
      {
          ClinicName = "Kristiansand Helsepunkt",
          Address = "Markens gate 21, 4611 Kristiansand",
          ImageUrl = "/images/kristiansand-helsepunkt.jpg",
          ImageAlt = "City centre clinic entrance in Kristiansand with pedestrian street access."
      },
      new()
      {
          ClinicName = "Drammen Elveklinikk",
          Address = "Bragernes Torg 2, 3017 Drammen",
          ImageUrl = "/images/drammen-elveklinikk.jpg",
          ImageAlt = "Clinic building overlooking the Drammen river."
      },
      new()
      {
          ClinicName = "Hamar Helsesenter",
          Address = "Strandgata 45, 2317 Hamar",
          ImageUrl = "/images/hamar-helsesenter.jpg",
          ImageAlt = "Medical centre near Lake Mjøsa in Hamar."
      },
      new()
      {
          ClinicName = "Molde Fjordklinikk",
          Address = "Storgata 33, 6413 Molde",
          ImageUrl = "/images/molde-fjordklinikk.jpg",
          ImageAlt = "Clinic building with fjord and mountain surroundings in Molde."
      },
      new()
      {
          ClinicName = "Fredrikstad Familiehelse",
          Address = "Nygaardsgata 16, 1607 Fredrikstad",
          ImageUrl = "/images/fredrikstad-familiehelse.jpg",
          ImageAlt = "Ground-floor clinic entrance in central Fredrikstad."
      },
      new()
      {
          ClinicName = "Alta Helseklinikk",
          Address = "Markedsgata 10, 9510 Alta",
          ImageUrl = "/images/alta-helseklinikk.jpg",
          ImageAlt = "Clinic building in Alta with northern landscape surroundings."
      },
      new()
      {
          ClinicName = "Arendal Medisinske Senter",
          Address = "Torvgaten 7, 4836 Arendal",
          ImageUrl = "/images/arendal-medisinske-senter.jpg",
          ImageAlt = "Coastal clinic building in Arendal with traditional southern architecture."
      }
  };
}

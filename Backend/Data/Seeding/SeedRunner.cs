namespace Backend.Data.Seeding;

public class SeedRunner
{
  private readonly IEnumerable<ISeeder> _seeders;

  public SeedRunner(IEnumerable<ISeeder> seeders)
  {
    _seeders = seeders;
  }

  public async Task RunAsync()
  {
    foreach (var seeder in _seeders)
      await seeder.SeedAsync();
  }
}
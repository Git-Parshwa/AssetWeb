using System.Text.Json;
using AssetWeb.Models.DTO;
#pragma warning disable CS8618

namespace AssetWeb.Services
{
    public static class CountryDataCache
    {
        public static List<CountryDto> Countries { get; private set; }

        public static void Load(IWebHostEnvironment env)
        {
            var path = Path.Combine(env.WebRootPath, "data", "countries+states+cities.json");
            var json = System.IO.File.ReadAllText(path);
            Countries = JsonSerializer.Deserialize<List<CountryDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AssetWeb.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace AssetWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly IWebHostEnvironment env;

        public LocationController(IWebHostEnvironment env)
        {
            this.env = env;
        }

        [HttpGet("countries")]
        public async Task<IActionResult> GetCountries()
        {
            var path = Path.Combine(env.WebRootPath, "data", "countries+states+cities.json");
            if (!System.IO.File.Exists(path))
            {
                return NotFound("countries.json file not found.");
            }
            var json = await System.IO.File.ReadAllTextAsync(path);
            var countries = JsonSerializer.Deserialize<List<CountryDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (countries == null)
            {
                return StatusCode(500, "Failed to parse json file");
            }
            var response = new { countries = countries.Select(x => x.Name) };
            return new JsonResult(response);
        }

        [HttpGet("states")]
        public async Task<IActionResult> GetStates(string selectedCountry)
        {
            var path = Path.Combine(env.WebRootPath, "data", "countries+states+cities.json");
            if (!System.IO.File.Exists(path))
            {
                return NotFound("json file not found.");
            }
            var json = await System.IO.File.ReadAllTextAsync(path);
            var countries = JsonSerializer.Deserialize<List<CountryDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (countries != null)
            {
                var country = countries.FirstOrDefault(x => x.Name.Equals(selectedCountry, StringComparison.OrdinalIgnoreCase));
                if (country != null)
                {
                    var response = new { states = country.States.Select(x => x.Name) };
                    return new JsonResult(response);
                }
                return NotFound("Country Not Found");
            }
            return StatusCode(500, "Failed to parse json file");
        }

        [HttpGet("cities")]
        public async Task<IActionResult> GetCities(string selectedCountry, string selectedState)
        {
            var path = Path.Combine(env.WebRootPath, "data", "countries+states+cities.json");
            if (!System.IO.File.Exists(path))
            {
                return NotFound("json file not found");
            }
            var json = await System.IO.File.ReadAllTextAsync(path);
            var countries = JsonSerializer.Deserialize<List<CountryDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (countries != null)
            {
                var country = countries.FirstOrDefault(x => x.Name.Equals(selectedCountry, StringComparison.OrdinalIgnoreCase));
                if (country != null)
                {
                    var state = country.States.FirstOrDefault(x => x.Name.Equals(selectedState, StringComparison.OrdinalIgnoreCase));
                    if (state != null)
                    {
                        var response = new { cities = state.Cities.Select(x => x.Name) };
                        return new JsonResult(response);
                    }
                    return NotFound("State Not Found");
                }
                return NotFound("Country Not Found");
            }
            return StatusCode(500, "Failed to parse json file");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AssetWeb.Models.DTO;
using AssetWeb.Services;
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
        public IActionResult GetCountries()
        {
            var response = new { countries = CountryDataCache.Countries.Select(x => x.Name) };
            return new JsonResult(response);
        }

        [HttpGet("states")]
        public IActionResult GetStates(string selectedCountry)
        {
            var country = CountryDataCache.Countries
                .FirstOrDefault(x => x.Name.Equals(selectedCountry, StringComparison.OrdinalIgnoreCase));

            if (country == null)
                return NotFound("Country not found");

            var response = new { states = country.States.Select(x => x.Name) };
            return new JsonResult(response);
        }

        [HttpGet("cities")]
        public IActionResult GetCities(string selectedCountry, string selectedState)
        {
            var country = CountryDataCache.Countries
                .FirstOrDefault(x => x.Name.Equals(selectedCountry, StringComparison.OrdinalIgnoreCase));

            if (country == null)
                return NotFound("Country not found");

            var state = country.States
                .FirstOrDefault(x => x.Name.Equals(selectedState, StringComparison.OrdinalIgnoreCase));

            if (state == null)
                return NotFound("State not found");

            var response = new { cities = state.Cities.Select(x => x.Name) };
            return new JsonResult(response);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#pragma warning disable CS8618

namespace AssetWeb.Models.DTO
{
    public class SiteDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
         public int Locations { get; set; } = 0;
        public int Assets { get; set; } = 0;
        public string? DepreciationMethod { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string PinCode { get; set; }
        public CompanyDto Company { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#pragma warning disable CS8618

namespace AssetWeb.Models.DTO
{
    public class StateDto
    {
        public string Name { get; set; }
        public List<CityDto> Cities { get; set; }
    }
}
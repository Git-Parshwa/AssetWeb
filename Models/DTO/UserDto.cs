using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetWeb.Models.Domain;
#pragma warning disable CS8618

namespace AssetWeb.Models.DTO
{
    public class UserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public CompanyDto Company { get; set; }
    }
}
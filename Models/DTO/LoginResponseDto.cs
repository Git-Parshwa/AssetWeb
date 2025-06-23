using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#pragma warning disable CS8618

namespace AssetWeb.Models.DTO
{
    public class LoginResponseDto
    {
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
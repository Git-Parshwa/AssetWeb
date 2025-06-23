using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
#pragma warning disable CS8618

namespace AssetWeb.Models.DTO
{
    public class CompanyProfileDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; }

        [Required]
        [MaxLength(500)]
        public string Address { get; set; }

        [Required]
        [MaxLength(15)]
        public string FinancialMonth { get; set; }

        [Required]
        [Range(1, 31)]
        public int FinancialDate { get; set; }

        [Required]
        [MaxLength(3, ErrorMessage = "Currency Code should be atmost of Length 3")]
        public string Currency { get; set; }
        
        [Required]
        public string? CompanyImageUrl { get; set; }
    }
}
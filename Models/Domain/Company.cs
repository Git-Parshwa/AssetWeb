using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#pragma warning disable CS8618

namespace AssetWeb.Models.Domain
{
    public class Company
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string FinancialMonth { get; set; }
        public int FinancialDate { get; set; }
        public string Currency { get; set; }
        public string? CompanyImageUrl { get; set; }
    }
}
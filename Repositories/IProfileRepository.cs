using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetWeb.Models.Domain;

namespace AssetWeb.Repositories
{
    public interface IProfileRepository
    {
        Task<User?> GetUserByIdAsync(string id);
        Task<Company?> CompanyProfileAsync(User user, Company addCompany);
        Task<Company?> UpdateCompanyProfileAsync(Guid id, Company updateCompany);
    }
}
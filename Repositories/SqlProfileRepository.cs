using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AssetWeb.Data;
using AssetWeb.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AssetWeb.Repositories
{
    public class SqlProfileRepository : IProfileRepository
    {
        private readonly AssetWebAuthDbContext dbContext;
        private readonly UserManager<User> userManager;

        public SqlProfileRepository(AssetWebAuthDbContext dbContext, UserManager<User> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }
        public async Task<Company?> CompanyProfileAsync(User user, Company addCompany)
        {
            var company = await dbContext.Companies.FirstOrDefaultAsync(x => x.Name == addCompany.Name && x.City == addCompany.City);
            if (company != null)
            {
                return null;
            }
            await dbContext.Companies.AddAsync(addCompany);
            await dbContext.SaveChangesAsync();
            company = await dbContext.Companies.FirstOrDefaultAsync(x => x.Name == addCompany.Name && x.City == addCompany.City);
            user.CompanyId = company!.Id;
            await dbContext.SaveChangesAsync();
            return company;
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {

            var user = await dbContext.Users.Include("Company").FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                return null;
            }
            return user;
        }

        public async Task<Company?> UpdateCompanyProfileAsync(Guid id, Company updateCompany)
        {
            var company = await dbContext.Companies.FirstOrDefaultAsync(x => x.Id == id);
            if (company == null)
            {
                return null;
            }
            company.Name = updateCompany.Name;
            company.Address = updateCompany.Address;
            company.Country = updateCompany.Country;
            company.State = updateCompany.State;
            company.City = updateCompany.City;
            company.Currency = updateCompany.Currency;
            company.CompanyImageUrl = updateCompany.CompanyImageUrl;
            company.FinancialDate = updateCompany.FinancialDate;
            company.FinancialMonth = updateCompany.FinancialMonth;
            await dbContext.SaveChangesAsync();
            return company;
        }
    }
}
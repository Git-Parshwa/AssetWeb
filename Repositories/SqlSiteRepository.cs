using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetWeb.Data;
using AssetWeb.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace AssetWeb.Repositories
{
    public class SqlSiteRepository : ISiteRepository
    {
        private readonly AssetWebAuthDbContext dbContext;

        public SqlSiteRepository(AssetWebAuthDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Site> AddSiteAsync(Site addSite)
        {
            await dbContext.Sites.AddAsync(addSite);
            await dbContext.SaveChangesAsync();
            return addSite;
        }

        public async Task<Site?> DeleteSiteAsync(Guid id)
        {
            var site = await dbContext.Sites.FirstOrDefaultAsync(x => x.Id == id);
            if (site == null)
            {
                return null;
            }
            dbContext.Sites.Remove(site);
            await dbContext.SaveChangesAsync();
            return site;
        }

        public async Task<List<Site>> GetAllSitesAsync(Guid id, string? filterOn, string? filterQuery, string? sortBy, bool isAscending, int pageSize, int pageNumber)
        {
            var sites = dbContext.Sites.Include("Company").AsQueryable();
            sites = sites.Where(x => x.CompanyId == id);
            if (string.IsNullOrWhiteSpace(filterOn) == false && string.IsNullOrWhiteSpace(filterQuery) == false)
            {
                if (filterOn.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    sites = sites.Where(x => x.Name.Contains(filterQuery));
                }
                else if (filterOn.Equals("Country", StringComparison.OrdinalIgnoreCase))
                {
                    sites = sites.Where(x => x.Country == filterQuery);
                }
                else if (filterOn.Equals("State", StringComparison.OrdinalIgnoreCase))
                {
                    sites = sites.Where(x => x.State == filterQuery);
                }
                else if (filterOn.Equals("City", StringComparison.OrdinalIgnoreCase))
                {
                    sites = sites.Where(x => x.City == filterQuery);
                }
            }

            if (string.IsNullOrWhiteSpace(sortBy) == false)
            {
                if (sortBy.Equals("Country", StringComparison.OrdinalIgnoreCase))
                {
                    if (isAscending == true)
                    {
                        sites = sites.OrderBy(x => x.Country);
                    }
                    else
                    {
                        sites = sites.OrderByDescending(x => x.Country);
                    }
                }
                else if (sortBy.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    if (isAscending == true)
                    {
                        sites = sites.OrderBy(x => x.Name);
                    }
                    else
                    {
                        sites = sites.OrderByDescending(x => x.Name);
                    }
                }
            }
            else
            {
                sites = sites.OrderBy(x => x.Name);
            }

            int skipResults = pageSize * (pageNumber - 1);
            sites = sites.Skip(skipResults).Take(pageSize);
            return await sites.ToListAsync();
        }

        public async Task<Site?> UpdateSiteAsync(Guid id, Site updateSite)
        {
            var site = await dbContext.Sites.FirstOrDefaultAsync(x => x.Id == id);
            if (site == null)
            {
                return null;
            }
            site.Name = updateSite.Name;
            site.Description = updateSite.Description;
            site.Country = updateSite.Country;
            site.State = updateSite.State;
            site.City = updateSite.City;
            site.PinCode = updateSite.PinCode;
            await dbContext.SaveChangesAsync();
            return site;
        }
    }
}
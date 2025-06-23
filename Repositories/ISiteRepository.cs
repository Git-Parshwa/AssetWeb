using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetWeb.Models.Domain;

namespace AssetWeb.Repositories
{
    public interface ISiteRepository
    {
        Task<Site> AddSiteAsync(Site addSite);
        Task<List<Site>> GetAllSitesAsync(Guid id, string? filterOn, string? filterQuery, string? sortBy, bool isAscending,
        int pageSize, int pageNumber);
        Task<Site?> UpdateSiteAsync(Guid id, Site updateSite);
        Task<Site?> DeleteSiteAsync(Guid id);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetWeb.Repositories
{
    public interface IFileRepository
    {
        Task<MemoryStream> ExportSitesAsync(Guid companyId);
        Task<bool> ImportSitesAsync(IFormFile file, Guid CompanyId);
    }
}
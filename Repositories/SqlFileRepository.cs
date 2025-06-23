using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetWeb.Data;
using AssetWeb.Models.Domain;
using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
#pragma warning disable CS1998

namespace AssetWeb.Repositories
{
    public class SqlFileRepository : IFileRepository
    {
        private readonly string? connectionString;
        private readonly AssetWebAuthDbContext dbContext;

        public SqlFileRepository(IConfiguration configuration, AssetWebAuthDbContext dbContext)
        {
            connectionString = configuration.GetConnectionString("AssetWebAuthConnectionString");
            this.dbContext = dbContext;
        }

        public async Task<MemoryStream> ExportSitesAsync(Guid companyId)
        {
            var dataTable = new DataTable();
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            using var cmd = new SqlCommand("Select Name, Assets, Locations, City, State from Sites where CompanyId=@CompanyId", conn);
            cmd.Parameters.AddWithValue("@CompanyId", companyId);
            using var adpater = new SqlDataAdapter(cmd);
            adpater.Fill(dataTable);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(dataTable, $"sitelist_{DateTime.UtcNow:ddMMyyHHmmss}");
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream;

            // var sb = new StringBuilder();
            // sb.AppendLine("Site Name,Assets Count,Location Count,City,State");

            // using var conn = new SqlConnection(connectionString);
            // conn.Open();
            // var cmd = new SqlCommand("Select Name, Assets, Locations, City, State from Sites where CompanyId=@CompanyId", conn);
            // cmd.Parameters.AddWithValue("@CompanyId", companyId);
            // using var reader = await cmd.ExecuteReaderAsync();
            // while (await reader.ReadAsync())
            // {
            //     sb.AppendLine($"{reader["Name"]},{reader["Assets"]},{reader["Locations"]},{reader["City"]},{reader["State"]}");
            // }

            // var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            // return new MemoryStream(bytes);
        }

        public async Task<bool> ImportSitesAsync(IFormFile file, Guid companyId)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }
            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            var expectedHeaders = new[] { "Site Name", "Assets Count", "Location Count", "City", "State" };
            var headerRow = worksheet.FirstRow();
            for (int i = 0; i < expectedHeaders.Length; i++)
            {
                var actualHeader = headerRow.Cell(i + 1).GetString().Trim();
                if (actualHeader.Equals(expectedHeaders[i], StringComparison.OrdinalIgnoreCase) == false)
                {
                    return false;
                }
            }

            var rows = worksheet.RangeUsed()!.RowsUsed().Skip(1);
            var sites = new List<Site>();
            foreach (var row in rows)
            {
                try
                {
                    var site = new Site
                    {
                        Name = row.Cell(1).GetString(),
                        Assets = row.Cell(2).GetValue<int>(),
                        Locations = row.Cell(3).GetValue<int>(),
                        City = row.Cell(4).GetString(),
                        State = row.Cell(5).GetString(),
                        CompanyId=companyId
                    };
                    sites.Add(site);
                }
                catch
                {
                    continue;
                }
            }
            await dbContext.Sites.AddRangeAsync(sites);
            await dbContext.SaveChangesAsync();
            return true;
        }
    }
}
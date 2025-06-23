using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AssetWeb.Models.Domain;
using AssetWeb.Repositories;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AssetWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IFileRepository fileRepository;
        private readonly IProfileRepository profileRepository;

        public FileController(IFileRepository fileRepository, IProfileRepository profileRepository)
        {
            this.fileRepository = fileRepository;
            this.profileRepository = profileRepository;
        }

        [HttpGet("sitelist")]
        public async Task<IActionResult> ExportSites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("Invalid Token");
            }
            var user = await profileRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized("Invalid User");
            }
            var companyId = user.CompanyId;
            if (companyId == null)
            {
                return BadRequest("Company Not Found!");
            }

            var stream = await fileRepository.ExportSitesAsync(companyId.Value);
            var content = stream.ToArray();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"sitelist_{DateTime.UtcNow:ddMMyyyyHHmmss}.xlsx");
        }

        [HttpPost("sitelist")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportSites(IFormFile file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("Invalid Token");
            }
            var user = await profileRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized("Invalid User");
            }
            var companyId = user.CompanyId;
            if (companyId == null)
            {
                return BadRequest("Company Not Found!");
            }
            var result = await fileRepository.ImportSitesAsync(file, companyId.Value);
            if (result == false)
            {
                return BadRequest("Invalid User or File");
            }
            return Ok("Sites Imported Successfully!");
        }
    }
}
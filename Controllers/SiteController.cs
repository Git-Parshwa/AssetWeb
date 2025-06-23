using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AssetWeb.CustomActionFilters;
using AssetWeb.Models.Domain;
using AssetWeb.Models.DTO;
using AssetWeb.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace AssetWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SiteController : ControllerBase
    {
        private readonly ISiteRepository siteRepository;
        private readonly IMapper mapper;
        private readonly IProfileRepository profileRepository;

        public SiteController(ISiteRepository siteRepository, IMapper mapper, IProfileRepository profileRepository)
        {
            this.siteRepository = siteRepository;
            this.mapper = mapper;
            this.profileRepository = profileRepository;
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> AddSite([FromBody] AddSiteDto addSiteDto)
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

            var addSite = mapper.Map<Site>(addSiteDto);
            addSite.CompanyId = companyId!.Value;
            var site = await siteRepository.AddSiteAsync(addSite);
            var siteDto = mapper.Map<SiteDto>(site);
            return Ok(siteDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSites([FromQuery] string? filterOn, string? filterQuery, string? sortBy,
        bool? isAscending, int pageSize = 10, int pageNumber = 1)
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

            var sites = await siteRepository.GetAllSitesAsync(companyId!.Value, filterOn, filterQuery, sortBy, isAscending ?? true, pageSize, pageNumber);
            var sitesDto = mapper.Map<List<SiteDto>>(sites);
            return Ok(sitesDto);
        }

        [HttpPut("{id:Guid}")]
        [ValidateModel]
        public async Task<IActionResult> UpdateSite([FromRoute] Guid id, [FromBody] AddSiteDto updateSiteDto)
        {
            var updateSite = mapper.Map<Site>(updateSiteDto);
            var site = await siteRepository.UpdateSiteAsync(id, updateSite);
            if (site == null)
            {
                return NotFound();
            }

            var siteDto = mapper.Map<SiteDto>(site);
            return Ok(siteDto);
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> DeleteSite([FromRoute] Guid id)
        {
            var site = await siteRepository.DeleteSiteAsync(id);
            if (site == null)
            {
                return BadRequest("Site Not Found!");
            }
            var siteDto = mapper.Map<SiteDto>(site);
            return Ok(siteDto);
        }
    }
}
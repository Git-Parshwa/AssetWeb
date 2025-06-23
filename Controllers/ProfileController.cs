using System.Security.Claims;
using AssetWeb.CustomActionFilters;
using AssetWeb.Models.Domain;
using AssetWeb.Models.DTO;
using AssetWeb.Repositories;
using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AssetWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IProfileRepository profileRepository;

        public ProfileController(IMapper mapper, IProfileRepository profileRepository)
        {
            this.mapper = mapper;
            this.profileRepository = profileRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("Invalid Token");
            }

            var user = await profileRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User Not Found!");
            }

            var profileDto = mapper.Map<ProfileDto>(user);
            return Ok(profileDto);
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> CompanyProfile([FromBody] CompanyProfileDto companyProfileDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("Invalid Token");
            }

            var user = await profileRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User Not Found!");
            }

            var company = mapper.Map<Company>(companyProfileDto);
            company = await profileRepository.CompanyProfileAsync(user, company);
            if (company == null)
            {
                return BadRequest("Company Already Exists, ask your Admin for further queries!");
            }
            var companyDto = mapper.Map<CompanyDto>(company);
            return Ok(companyDto);
        }

        [HttpPut]
        [ValidateModel]
        public async Task<IActionResult> UpdateCompanyProfile([FromBody] CompanyProfileDto companyProfileDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("Invalid Token");
            }

            var user = await profileRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User Not Found!");
            }
            var companyId = user.CompanyId;
            if (companyId == null)
            {
                return NotFound("Company Not Found!");
            }
            var companyProfile = mapper.Map<Company>(companyProfileDto);

            var company = await profileRepository.UpdateCompanyProfileAsync(companyId.Value, companyProfile);
            var companyDto = mapper.Map<CompanyDto>(company);
            return Ok(companyDto);
        }
    }
}
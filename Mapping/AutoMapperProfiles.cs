using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetWeb.Models.Domain;
using AssetWeb.Models.DTO;
using AutoMapper;

namespace AssetWeb.Mapping
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, ProfileDto>().ReverseMap();
            CreateMap<CompanyProfileDto, Company>().ReverseMap();
            CreateMap<Company, CompanyDto>().ReverseMap();
            CreateMap<AddSiteDto, Site>().ReverseMap();
            CreateMap<Site, SiteDto>().ReverseMap();
        }
    }
}
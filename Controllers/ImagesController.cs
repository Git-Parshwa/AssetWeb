using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetWeb.CustomActionFilters;
using AssetWeb.Models.Domain;
using AssetWeb.Models.DTO;
using AssetWeb.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AssetWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly IImageRepository imageRepository;
        private readonly IMapper mapper;

        public ImagesController(IImageRepository imageRepository, IMapper mapper)
        {
            this.imageRepository = imageRepository;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage([FromForm] UploadImageDto uploadImageDto)
        {
            ValidateFileUpload(uploadImageDto);
            if (ModelState.IsValid)
            {
                var uploadImage = new Image
                {
                    File = uploadImageDto.File,
                    FileName = uploadImageDto.FileName,
                    FileDescription = uploadImageDto.FileDescription,
                    FileExtension = Path.GetExtension(uploadImageDto.File.FileName),
                    FileSize = uploadImageDto.File.Length
                };
                uploadImage = await imageRepository.UploadImageAsync(uploadImage);
                return Ok(uploadImage);
            }
            return BadRequest(ModelState);
        }

        private void ValidateFileUpload(UploadImageDto uploadImageDto)
        {
            var allowedExtensions = new string[] { ".jpg", ".jpeg", ".png" };
            if (allowedExtensions.Contains(Path.GetExtension(uploadImageDto.File.FileName)) == false)
            {
                ModelState.AddModelError("file", "Unsupported File");
            }
            if (uploadImageDto.File.Length > 10485760)
            {
                ModelState.AddModelError("file", "File exceeds maximum file size");
            }
        }
    }
}
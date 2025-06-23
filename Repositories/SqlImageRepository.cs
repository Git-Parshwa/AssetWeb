using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetWeb.Data;
using AssetWeb.Models.Domain;

namespace AssetWeb.Repositories
{
    public class SqlImageRepository : IImageRepository
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly AssetWebAuthDbContext dbContext;

        public SqlImageRepository(IWebHostEnvironment webHostEnvironment,IHttpContextAccessor httpContextAccessor,AssetWebAuthDbContext dbContext)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
            this.dbContext = dbContext;
        }
        public async Task<Image> UploadImageAsync(Image image)
        {
            var localFilePath = Path.Combine(webHostEnvironment.ContentRootPath, "Images", $"{image.FileName}{image.FileExtension}");

            using var stream = new FileStream(localFilePath, FileMode.Create);
            await image.File.CopyToAsync(stream);

            var urlFilePath = $"{httpContextAccessor.HttpContext!.Request.Scheme}://{httpContextAccessor.HttpContext!.Request.Host}{httpContextAccessor.HttpContext!.Request.PathBase}/Images/{image.FileName}{image.FileExtension}";
            image.FilePath = urlFilePath;

            await dbContext.Images.AddAsync(image);
            await dbContext.SaveChangesAsync();
            return image;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetWeb.Models.Domain;

namespace AssetWeb.Repositories
{
    public interface IImageRepository
    {
        Task<Image> UploadImageAsync(Image uploadImage);
    }
}
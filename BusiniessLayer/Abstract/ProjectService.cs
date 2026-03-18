using EntityLayer.Concrete;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusiniessLayer.Abstract
{
    // BusinessLayer/Abstract/IProjectService.cs
    public interface ProjectService
    {
        Task<List<Project>> GetAllWithImagesAsync();
        Task<List<Project>> GetLatestProjectsAsync(int count = 3);
        Task AddProjectAsync(Project project, IFormFile coverImage, List<IFormFile> galleryImages, string wwwRootPath);
        Task UpdateProjectAsync(Project project, IFormFile coverImage, string wwwRootPath);
        Task DeleteProjectAsync(int id, string wwwRootPath);
    }
}

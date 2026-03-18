using BusiniessLayer.Abstract;
using DataAcsessLayer.Abstract;
using EntityLayer.Concrete;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusiniessLayer.Concrete
{
    // BusinessLayer/Concrete/ProjectManager.cs
    public class ProjectManager : ProjectService
    {
        private readonly IProjectDal _projectDal;
        private readonly IProjectImageDal _projectImageDal;

        public ProjectManager(IProjectDal projectDal, IProjectImageDal projectImageDal)
        {
            _projectDal = projectDal;
            _projectImageDal = projectImageDal;
        }

        public async Task<List<Project>> GetAllWithImagesAsync()
        {
            return (await _projectDal.GetAllFilterIncludeAsync(x => true, p => p.ProjectImages)).ToList();
        }

        public async Task<List<Project>> GetLatestProjectsAsync(int count = 3)
        {
            var all = await GetAllWithImagesAsync();
            return all.OrderByDescending(x => x.Id).Take(count).ToList();
        }

        public async Task AddProjectAsync(Project project, IFormFile coverImage, List<IFormFile> galleryImages, string wwwRootPath)
        {
            await _projectDal.InsertAsync(project);

            string folderPath = Path.Combine(wwwRootPath, "img", "projects");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            if (coverImage?.Length > 0)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(coverImage.FileName);
                await SaveFileAsync(coverImage, Path.Combine(folderPath, fileName));
                await _projectImageDal.InsertAsync(new ProjectImage
                {
                    ImageUrl = "/img/projects/" + fileName,
                    ProjectId = project.Id,
                    IsCover = true,
                    Order = 1
                });
            }

            if (galleryImages?.Count > 0)
            {
                foreach (var img in galleryImages.Where(i => i.Length > 0))
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                    await SaveFileAsync(img, Path.Combine(folderPath, fileName));
                    await _projectImageDal.InsertAsync(new ProjectImage
                    {
                        ImageUrl = "/img/projects/" + fileName,
                        ProjectId = project.Id,
                        IsCover = false,
                        Order = 2
                    });
                }
            }
        }

        public async Task UpdateProjectAsync(Project project, IFormFile coverImage, string wwwRootPath)
        {
            var existing = await _projectDal.GetByIdAsync(project.Id)
                ?? throw new Exception("Proje bulunamadı");

            existing.Title = project.Title;
            existing.Description = project.Description;
            existing.Category = project.Category;
            existing.Year = project.Year;

            if (coverImage?.Length > 0)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(coverImage.FileName);
                string folderPath = Path.Combine(wwwRootPath, "img", "projects");
                await SaveFileAsync(coverImage, Path.Combine(folderPath, fileName));

                var oldImage = await _projectImageDal.GetByFilterAsync(x => x.ProjectId == project.Id && x.IsCover);
                if (oldImage != null)
                {
                    oldImage.ImageUrl = "/img/projects/" + fileName;
                    await _projectImageDal.UpdateAsync(oldImage);
                }
                else
                {
                    await _projectImageDal.InsertAsync(new ProjectImage
                    {
                        ImageUrl = "/img/projects/" + fileName,
                        ProjectId = project.Id,
                        IsCover = true
                    });
                }
            }

            await _projectDal.UpdateAsync(existing);
        }

        public async Task DeleteProjectAsync(int id, string wwwRootPath)
        {
            var project = await _projectDal.GetByIdAsync(id)
                ?? throw new Exception("Proje bulunamadı");

            var images = await _projectImageDal.GetAllFilterAsync(x => x.ProjectId == id);

            foreach (var img in images)
            {
                string filePath = Path.Combine(wwwRootPath, img.ImageUrl.TrimStart('/'));
                if (File.Exists(filePath)) File.Delete(filePath);
                await _projectImageDal.DeleteAsync(img);
            }

            await _projectDal.DeleteAsync(project);
        }

        // Yardımcı private method
        private async Task SaveFileAsync(IFormFile file, string fullPath)
        {
            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
        }
    }
}

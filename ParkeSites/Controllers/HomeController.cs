using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ParkeSites.Models;
using DataAcsessLayer.Abstract;
using EntityLayer.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting; // Dosya yükleme (wwwroot yolu) için gerekli
using Microsoft.AspNetCore.Http; // IFormFile için gerekli
using System.IO;

namespace ParkeSites.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProjectDal _projectDal;
        private readonly IProjectImageDal _projectImageDal;
        private readonly IWebHostEnvironment _webHostEnvironment; // wwwroot'a ulaþmak için

        // Bütün servisleri (Dependency Injection) içeri alýyoruz
        public HomeController(ILogger<HomeController> logger, IProjectDal projectDal, IProjectImageDal projectImageDal, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _projectDal = projectDal;
            _projectImageDal = projectImageDal;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            // Ýleride buraya son 3 projeyi getiren kodu yazacaðýz.
            return View();
        }

        public async Task<IActionResult> Projeler()
        {
            // Veritabanýndaki tüm projeleri View'a gönderiyoruz
            var projects = await _projectDal.GetAllFilterIncludeAsync(
         x => true,
         p => p.ProjectImages
     );
            return View(projects);
        }

        // --- YENÝ PROJE EKLEME ---
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddProject(Project project, IFormFile CoverImage, List<IFormFile> GalleryImages)
        {
            try
            {
                await _projectDal.InsertAsync(project);
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string folderPath = Path.Combine(wwwRootPath, "img", "projects");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // 1. Kapak Resmini Kaydet
                if (CoverImage != null && CoverImage.Length > 0)
                {
                    string coverFileName = Guid.NewGuid().ToString() + Path.GetExtension(CoverImage.FileName);
                    string exactCoverPath = Path.Combine(folderPath, coverFileName);

                    using (var fileStream = new FileStream(exactCoverPath, FileMode.Create))
                    {
                        await CoverImage.CopyToAsync(fileStream);
                    }

                    await _projectImageDal.InsertAsync(new ProjectImage
                    {
                        ImageUrl = "/img/projects/" + coverFileName,
                        ProjectId = project.Id,
                        IsCover = true,
                        Order = 1
                    });
                }

                // 2. Galeri Resimlerini Kaydet (Çoklu)
                if (GalleryImages != null && GalleryImages.Count > 0)
                {
                    foreach (var img in GalleryImages)
                    {
                        if (img.Length > 0)
                        {
                            string gFileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                            string gExactPath = Path.Combine(folderPath, gFileName);

                            using (var fileStream = new FileStream(gExactPath, FileMode.Create))
                            {
                                await img.CopyToAsync(fileStream);
                            }

                            await _projectImageDal.InsertAsync(new ProjectImage
                            {
                                ImageUrl = "/img/projects/" + gFileName,
                                ProjectId = project.Id,
                                IsCover = false, // Bunlar kapak deðil, slider'da dönecek
                                Order = 2
                            });
                        }
                    }
                }

                TempData["Success"] = "Proje ve galerisi baþarýyla eklendi!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Proje eklenirken hata: " + ex.Message;
            }

            return RedirectToAction("Projeler");
        }

        // --- PROJE GÜNCELLEME ---
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditProject(Project project, IFormFile CoverImage)
        {
            try
            {
                var existingProject = await _projectDal.GetByIdAsync(project.Id);
                if (existingProject == null) return NotFound();

                // Metin alanlarýný güncelle
                existingProject.Title = project.Title;
                existingProject.Description = project.Description;
                existingProject.Category = project.Category;
                existingProject.Year = project.Year;

                // Eðer yeni bir resim yüklendiyse
                if (CoverImage != null && CoverImage.Length > 0)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(CoverImage.FileName);
                    string folderPath = Path.Combine(wwwRootPath, "img", "projects");
                    string exactPath = Path.Combine(folderPath, fileName);

                    using (var fileStream = new FileStream(exactPath, FileMode.Create))
                    {
                        await CoverImage.CopyToAsync(fileStream);
                    }

                    // Eski kapak resmini bul ve yeni URL ile deðiþtir
                    var oldImage = await _projectImageDal.GetByFilterAsync(x => x.ProjectId == project.Id && x.IsCover == true);
                    if (oldImage != null)
                    {
                        oldImage.ImageUrl = "/img/projects/" + fileName;
                        await _projectImageDal.UpdateAsync(oldImage);
                    }
                    else
                    {
                        // Hiç resmi yoksa yeni oluþtur
                        await _projectImageDal.InsertAsync(new ProjectImage
                        {
                            ImageUrl = "/img/projects/" + fileName,
                            ProjectId = project.Id,
                            IsCover = true
                        });
                    }
                }

                await _projectDal.UpdateAsync(existingProject);
                TempData["Success"] = "Proje baþarýyla güncellendi!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Güncelleme hatasý: " + ex.Message;
            }

            return RedirectToAction("Projeler");
        }

        // --- PROJE SÝLME ---
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            try
            {
                var project = await _projectDal.GetByIdAsync(id);
                if (project != null)
                {
                    await _projectDal.DeleteAsync(project);
                    TempData["Success"] = "Proje baþarýyla silindi!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Silinirken hata oluþtu: " + ex.Message;
            }

            return RedirectToAction("Projeler");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
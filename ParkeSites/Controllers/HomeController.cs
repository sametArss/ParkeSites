using BusiniessLayer.Abstract;
using EntityLayer.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkeSites.Models;
using System.Diagnostics;

namespace ParkeSites.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ProjectService _projectService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, ProjectService projectService, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _projectService = projectService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var latestProjects = await _projectService.GetLatestProjectsAsync(3);
            return View(latestProjects);
        }

        public async Task<IActionResult> Projeler()
        {
            var projects = await _projectService.GetAllWithImagesAsync();
            return View(projects);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddProject(Project project, IFormFile CoverImage, List<IFormFile> GalleryImages)
        {
            try
            {
                await _projectService.AddProjectAsync(project, CoverImage, GalleryImages, _webHostEnvironment.WebRootPath);
                TempData["Success"] = "Proje ve galerisi başarıyla eklendi!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Proje eklenirken hata: " + ex.Message;
            }

            return RedirectToAction("Projeler");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditProject(Project project, IFormFile CoverImage)
        {
            try
            {
                await _projectService.UpdateProjectAsync(project, CoverImage, _webHostEnvironment.WebRootPath);
                TempData["Success"] = "Proje başarıyla güncellendi!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Güncelleme hatası: " + ex.Message;
            }

            return RedirectToAction("Projeler");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            try
            {
                await _projectService.DeleteProjectAsync(id, _webHostEnvironment.WebRootPath);
                TempData["Success"] = "Proje ve tüm resimleri silindi!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Silinirken hata oluştu: " + ex.Message;
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
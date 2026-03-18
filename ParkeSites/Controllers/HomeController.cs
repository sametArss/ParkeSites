using BusiniessLayer.Abstract;
using EntityLayer.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkeSites.Models;
using System.Diagnostics;
using System.Security.Claims; // Ekledik

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

        // Kullanıcı IP ve Mailini bulan yardımcı metotlar
        private string GetIp() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Bilinmeyen IP";
        private string GetEmail() => User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name ?? "Bilinmeyen Kullanıcı";

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
                
                // YENİ: İşlemi Logla
                _logger.LogInformation("[PROJE EKLENDİ] Başlık: {Title} | Ekleyen: {Email} | IP: {Ip}", project.Title, GetEmail(), GetIp());
                
                TempData["Success"] = "Proje ve galerisi başarıyla eklendi!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HATA - PROJE EKLEME] Ekleyen: {Email} | IP: {Ip}", GetEmail(), GetIp());
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
                
                // YENİ: İşlemi Logla
                _logger.LogInformation("[PROJE GÜNCELLENDİ] ID: {Id}, Başlık: {Title} | Güncelleyen: {Email} | IP: {Ip}", project.Id, project.Title, GetEmail(), GetIp());
                
                TempData["Success"] = "Proje başarıyla güncellendi!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HATA - PROJE GÜNCELLEME] Güncelleyen: {Email} | IP: {Ip}", GetEmail(), GetIp());
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
                
                // YENİ: İşlemi Logla
                _logger.LogInformation("[PROJE SİLİNDİ] ID: {Id} | Silen: {Email} | IP: {Ip}", id, GetEmail(), GetIp());
                
                TempData["Success"] = "Proje ve tüm resimleri silindi!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HATA - PROJE SİLME] Silen: {Email} | IP: {Ip}", GetEmail(), GetIp());
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
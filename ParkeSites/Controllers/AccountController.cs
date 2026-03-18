using BusiniessLayer.Abstract;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; // Claim'ler için eklendi

namespace ParkeSites.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IUserService userService, ILogger<AccountController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]// Sadece Admin rolündeki kullanıcılar erişebilir)]
        [HttpGet]
        public IActionResult Register() => View();
        [Authorize(Roles = "Admin")]// Sadece Admin rolündeki kullanıcılar erişebilir)]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            try
            {
                await _userService.RegisterAsync(dto);
                TempData["Success"] = "Hesabınız başarıyla oluşturuldu! Lütfen giriş yapınız.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Register");
            }
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            try
            {
                // 1. Artık token dönmüyoruz, kullanıcının nesnesini (User) dönüyoruz.
                // (UserManager.cs içerisindeki LoginAsync metodunu Task<User> dönecek şekilde güncellediğini varsayıyoruz)
                var user = await _userService.LoginAsync(dto);

                // 2. Cookie içine gömülecek kimlik bilgilerini (Claim) oluştur
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FullName ?? ""),
                    new Claim(ClaimTypes.Role, user.Role ?? "User") // İleride rol bazlı yetkilendirme (Admin vs) için lazım olur
                };

                // 3. Kimliği oluştur
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // 4. Çerez (Cookie) ayarlarını yapılandır
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // "Beni Hatırla" mantığı, tarayıcı kapansa da silinmez
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) // 7 gün boyunca geçerli
                };

                // 5. Sistemi standart MVC metoduyla giriş yaptır
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                TempData["Success"] = "Giriş başarılı ...";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Hata mesajını loglayabilirsin ama kullanıcıya sadece "hatalı" demek daha güvenlidir.
                TempData["Error"] = "E-posta veya şifre hatalı.";
                return RedirectToAction("Login");
            }
        }

        // --- YENİ EKLENDİ: Çıkış Yapma Metodu ---
        
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            string email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name ?? "Bilinmeyen";
            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Bilinmeyen IP";

            _logger.LogInformation("[AUTH] Logout success | Email={Email} IP={Ip}", email, ip);

            // Cookie'yi silerek oturumu güvenli bir şekilde kapatır
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
        [Authorize(Roles = "Admin")] // Sadece Admin rolündeki kullanıcılar erişebilir)]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            try
            {
                // 1. Giriş yapan kullanıcının ID'sini al
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

                // 2. Servise gönder (Bütün işi UserService yapacak)
                await _userService.ChangePasswordAsync(userId, dto);

                // 3. Başarılı mesajı
                TempData["Success"] = "Şifreniz başarıyla güncellendi!";
            }
            catch (Exception ex)
            {
                // 4. Hata mesajı (Şifre yanlışsa vs. buraya düşecek)
                TempData["Error"] = ex.Message;
            }

            // Her durumda Profil sayfasına geri dön
            return RedirectToAction("Profile");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // Giriş yapan kullanıcının ID'sini Cookie'den çekiyoruz
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            // Bilgileri getir
            var user = await _userService.GetUserByIdAsync(userId);

            return View(user);
        }
    }
}
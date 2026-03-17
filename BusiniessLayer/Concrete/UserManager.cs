using BusiniessLayer.Abstract;
using BusiniessLayer.Security;
using DataAcsessLayer.Abstract;
using EntityLayer.Concrete;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusiniessLayer.Concrete
{
    public class UserManager : IUserService
    {
        private readonly IUserDal _userRepo;
        private readonly ILogger<UserManager> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserManager(IUserDal userRepo,  ILogger<UserManager> logger, IHttpContextAccessor httpContextAccessor)
        {
            _userRepo = userRepo;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetIp() => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        public async Task RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _userRepo.GetByFilterAsync(x => x.Email == dto.Email);

            PasswordHasher.Create(dto.Password, out var hash, out var salt);
            string verificationToken = Guid.NewGuid().ToString();

            // --- KULLANICI KONTROL MANTIĞI ---
            if (existingUser != null)
            {
                throw new Exception("Bu email zaten kayıtlı.");
            }
            else
            {
                // Yeni kayıt
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = dto.Email,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    FullName = dto.FullName,
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    
                };
                await _userRepo.InsertAsync(user);
                _logger.LogInformation("[AUTH] Register success | Email={Email} IP={Ip}", dto.Email, GetIp());
            }
            // ----------------------------------

           
        }

        public async Task<User> LoginAsync(LoginDto dto)
        {
            var user = await _userRepo.GetByFilterAsync(x => x.Email == dto.Email);
            if (user == null)
            {
                _logger.LogWarning("[AUTH] Login failed - user not found | Email={Email} IP={Ip}", dto.Email, GetIp());
                throw new Exception("Kullanıcı veya şifre hatalı."); // Güvenlik için spesifik hata vermiyoruz
            }

            if (!PasswordHasher.Verify(dto.Password, user.PasswordHash, user.PasswordSalt))
            {
                _logger.LogWarning("[AUTH] Login failed - wrong password | Email={Email} IP={Ip}", dto.Email, GetIp());
                throw new Exception("Kullanıcı veya şifre hatalı.");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user);

            _logger.LogInformation("[AUTH] Login success | UserId={UserId} Email={Email} IP={Ip}", user.Id, user.Email, GetIp());

            // Token oluşturmak yerine kullanıcı nesnesini doğrudan döndürüyoruz
            return user;
        }


        public async Task<User> GetUserByIdAsync(Guid id)
        {
            return await _userRepo.GetByIdAsync(id);
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            // 1. Kullanıcıyı getir
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            // 2. Eski şifre doğru mu kontrol et
            if (!PasswordHasher.Verify(dto.OldPassword, user.PasswordHash, user.PasswordSalt))
            {
                throw new Exception("Mevcut şifreniz hatalı.");
            }

            // 3. Yeni şifreler uyuşuyor mu?
            if (dto.NewPassword != dto.ConfirmPassword)
            {
                throw new Exception("Yeni şifreler birbiriyle uyuşmuyor.");
            }

            // 4. Yeni şifrenin güvenliği (İstersen buraya karakter sayısı kontrolü ekleyebilirsin)
            if (dto.NewPassword.Length < 6)
            {
                throw new Exception("Yeni şifreniz en az 6 karakter olmalıdır.");
            }

            // 5. Yeni şifreyi Hashle
            PasswordHasher.Create(dto.NewPassword, out var newHash, out var newSalt);

            // 6. Bilgileri güncelle
            user.PasswordHash = newHash;
            user.PasswordSalt = newSalt;

            // 7. Veritabanına kaydet
            await _userRepo.UpdateAsync(user);
            _logger.LogInformation("[AUTH] Password changed | UserId={UserId} Email={Email} IP={Ip}", userId, user.Email, GetIp());
        }


    }
}

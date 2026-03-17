using System;
using System.ComponentModel.DataAnnotations;

namespace EntityLayer.Concrete
{
    public class User
    {
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; }

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; }

        // 🔐 HASH + SALT
        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string PasswordSalt { get; set; }

        // 🔑 AUTH
        [MaxLength(50)]
        public string Role { get; set; } = "Admin";

        public bool IsActive { get; set; } = true;

       
        // 🕒 TARİHLER
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
    }
}
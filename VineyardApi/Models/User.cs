using System;
using System.ComponentModel.DataAnnotations;

namespace VineyardApi.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; }

        public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
    }
}

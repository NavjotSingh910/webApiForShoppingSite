using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class User
    {
        [Key]
        public string Id { get; set; }= Guid.NewGuid().ToString();

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string? ResetPasswordToken { get; set; }

        public DateTime? ResetPasswordExpires { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        [Required]
        public string Role { get; set; } = "User";
    }
}

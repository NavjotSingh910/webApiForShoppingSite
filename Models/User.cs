﻿using System.Data;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class User
    {
        [Key]
       public String GuId { get; set; }

        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
         public string status { get; set; } = "Pending";

         [Required]
         public string Role { get; set; } = "User";




    }
}

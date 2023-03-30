using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Dto
{
    public class ForgetPassword
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Email { get; set; }

    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApplication3.Data;
using WebApplication3.Dto;
using WebApplication3.Helper;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PasswordResetController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserContext _context;
        private readonly MailHelper _mailHelper;

        public PasswordResetController(IConfiguration configuration, UserContext context, MailHelper mailHelper)
        {
            _configuration = configuration;
            _context = context;
            _mailHelper = mailHelper;
        }

        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> Post([FromBody] ForgetPassword model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email && u.Username == model.Username);

            if (user == null)
            {
                return NotFound();
            }

            var token = Guid.NewGuid().ToString();

            user.ResetPasswordToken = token;
            user.ResetPasswordExpires = DateTime.UtcNow.AddHours(1);

            _context.SaveChanges();

            var callbackUrl = $"{_configuration["AppUrl"]}passwordreset/reset-password?token={token}";


            Email mail = new Email();
            mail.From="navjotsandhu910@outlook.com";
            mail.To=user.Email;
            mail.Subject=$"{user.Username} want to resigter";
            mail.Body=$"Please reset your password by clicking this link: <a href='{callbackUrl}'>link</a>";

            // Send email to admin about new registration


            await _mailHelper.Send(mail);

            return Ok();
        }

        [HttpPut("ResetPasword")]
        public IActionResult Put([FromBody] ResetPassword model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = _context.Users.FirstOrDefault(u => u.ResetPasswordToken == model.Token && u.ResetPasswordExpires >= DateTime.UtcNow);

            if (user == null)
            {
                return NotFound();
            }

            user.Password = model.Password;
            user.ResetPasswordToken = null;
            user.ResetPasswordExpires = null;

            _context.SaveChanges();

            return Ok();
        }
    }
}

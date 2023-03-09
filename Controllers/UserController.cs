using System.Net.Mail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApplication3.Data;
using WebApplication3.Dto;
using WebApplication3.Models;
using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;

namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserContext _context;
        private readonly IConfiguration _configuration;

        public UserController(UserContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (_context.Users.Any(u => u.Username == user.Username))
            {
                return BadRequest("Username already exists");
            }

            if (_context.Users.Any(u => u.Email == user.Email))
            {
                return BadRequest("Email already exists");
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            SendEmail(user);//call the function and also send the values that is persent in restration page
            return Ok(user);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDTO user)
        {
            var existingUser = _context.Users.SingleOrDefault(u => u.Username == user.Username && u.Password == user.Password);
            if (existingUser == null)
            {
                return BadRequest("Invalid username or password");
            }
            // Generate JWT token
            string token = CreateToken(existingUser);
            return Ok(token);
        }

        private string CreateToken(User user)
        {
            // List<Claim> claims = new List<Claim>
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        [HttpPost]
        private IActionResult SendEmail(User user)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("navjotsandhu910@outlook.com"));//from section
            email.To.Add(MailboxAddress.Parse("navjotsandhu910@outlook.com"));//want to send email address of that person
            email.Subject="Test mail ";//subject of mail
            email.Body=new TextPart(TextFormat.Html){Text=$"Hi Admin,{user.Username} want to register in our web site please check on web site and mail address is {user.Email}"};//text of mail
            
            //now we want to make a connection with our stmpclient using mailKit
            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            smtp.Connect("smtp.office365.com",587,MailKit.Security.SecureSocketOptions.StartTls);
            smtp.Authenticate("navjotsandhu910@outlook.com","tes");//user name ,password
            smtp.Send(email);//send mail by passing our mail variable
            smtp.Disconnect(true);//now disconnect to server
            return Ok();

        }

    }
}

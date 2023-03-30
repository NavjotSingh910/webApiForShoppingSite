
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
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebApplication3.Helper;//for access the MailHelper
namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserContext _context;
        private readonly IConfiguration _configuration;

        private readonly MailHelper _helper;

        public UserController(UserContext context, IConfiguration configuration, MailHelper helper)
        {
            _context = context;
            _configuration = configuration;
            _helper = helper;
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
            await SendEmailToAdmin(user);
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
            else if (existingUser.status == "Active")
            {
                string token = CreateToken(existingUser);
                return Ok(token);
            }
            else if (existingUser.status == "Pending")
            {
                return BadRequest("Registertion Request Pending Please Wait.");
            }
            return BadRequest("Your Blocked");
        }

        private string CreateToken(User user)
        {
            List<Claim> claims;

            if (user.Role == "Admin")
            {
                claims = new()
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, "Admin")
                };
            }
            else
            {
                claims = new()
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, "User")
                };
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        [HttpPost("ForgetPassword")]
        private async Task<IActionResult> ForgetPassword([FromBody] ForgetPassword user)
        {
            var existUser = await _context.Users.Where(u => u.Username == user.Username && u.Email == user.Email).FirstOrDefaultAsync();

            if (existUser == null)
            {
                return NotFound();
            }

            await ForgetPasswordEmail(existUser);
            return Ok(existUser);
        }


        private async Task<IActionResult> ForgetPasswordEmail(User user)
        {
            string password = user.Password;

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("navjotsandhu910@outlook.com"));
            email.To.Add(MailboxAddress.Parse(user.Email));
            email.Subject = "Password of Your Account.";
            email.Body = new TextPart(TextFormat.Html) { Text = $"This is Your password: <h1>{password}</h1> and mail is this go to site https://localhost:7207/ and enjoy shopping." };

            string result = _helper.Send(email);

            if (result == "Done")
            {
                return Ok();
            }

            return NotFound();
        }

        [HttpGet("GetUsers")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var Users = await _context.Users.ToListAsync();
            return Users;
        }


        [HttpGet("GetUsersByStatus")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersByStatus(string status)
        {
            var Users = await _context.Users.Where(u => u.status == status).ToListAsync();
            return Users;
        }


        [HttpPost("EditStatus")]
        public async Task<IActionResult> EditStatusOfUserByAdmin(int id, string status)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.status = status;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return Ok();
        }

        private async Task<IActionResult> SendEmailToAdmin(User user)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("navjotsandhu910@outlook.com"));
            email.To.Add(MailboxAddress.Parse("navjotsandhu910@outlook.com"));
            email.Subject = "Test mail ";
            email.Body = new TextPart(TextFormat.Html) { Text = $"Hi Admin,{user.Username} want to register in our web site please check on web site and mail address is {user.Email} go to site https://localhost:7207/" };
            string result = _helper.Send(email);//here i  call mail helper for send the Mail

            if (result == "Done")
            {
                return Ok();
            }

            return NotFound();
        }
    }
}

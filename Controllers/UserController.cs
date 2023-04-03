
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
using WebApplication3.Helper;// Include MailHelper
namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserContext _context;
        private readonly IConfiguration _configuration;

        private readonly MailHelper _helper;// Create instance of MailHelper

        public UserController(UserContext context, IConfiguration configuration, MailHelper helper)
        {
            _context = context;
            _configuration = configuration;
            _helper = helper;// Assign MailHelper instance to _helper variable
        }

        // Register a new user
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            // Check if username or email already exists
            if (_context.Users.Any(u => u.Username == user.Username))
            {
                return BadRequest("Username already exists");
            }

            if (_context.Users.Any(u => u.Email == user.Email))
            {
                return BadRequest("Email already exists");
            }

            // Add user to database
            Email mail = new Email();
            mail.From="navjotsandhu910@outlook.com";
            mail.To="navjotsandhu910@outlook.com";
            mail.Subject=$"{user.Username} want to resigter";
            mail.Body=$"Hi Admin,{user.Username} want to register in our web site please check on web site and mail address is {user.Email} go to site https://localhost:7207/";
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            // Send email to admin about new registration
            await _helper.Send(mail);
            return Ok(user);
        }

        // Login an existing user
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDTO user)
        {
            // Check if user with given username and password exists

            var existingUser = _context.Users.SingleOrDefault(u => u.Username == user.Username && u.Password == user.Password);

            if (existingUser == null)
            {
                return BadRequest("Invalid username or password");
            }

            else if (existingUser.status == "Active")
            {
                // Create token for authenticated user
                string token = CreateToken(existingUser);
                return Ok(token);
            }

            else if (existingUser.status == "Pending")
            {
                return BadRequest("Registertion Request Pending Please Wait.");
            }
            return BadRequest("Your Blocked");
        }

        // Helper function to create a JWT token for authenticated users
        private string CreateToken(User user)
        {
            List<Claim> claims;
            // Create claims for admin or normal user
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

            // Set token key and signing credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // Create JWT token
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        // This API endpoint is used to handle the request when a user forgets their password
        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPassword user)
        {
            // Check if a user with the provided username and email exists in the database
            var existUser = await _context.Users.Where(u => u.Username == user.Username && u.Email == user.Email).FirstOrDefaultAsync();
            // If user does not exist, return a NotFound response
            if (existUser == null)
            {
                return NotFound();
            }
             Email mail = new Email();
            mail.From="navjotsandhu910@outlook.com";
            mail.To=$"{user.Email}";
            mail.Subject=$"Forget Password";
            mail.Body= $"This is Your password: <h1></h1> and mail is this go to site https://localhost:7207/ and enjoy shopping." ;
            // Send the user's password via email
            await _helper.Send(mail);
            return Ok();
        }

        // API endpoint to retrieve all users in the database
        [HttpGet("GetUsers")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var Users = await _context.Users.ToListAsync();
            return Users;
        }

        // API endpoint to retrieve all users with a specific status in the database
        [HttpGet("GetUsersByStatus")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersByStatus(string status)
        {
            var Users = await _context.Users.Where(u => u.status == status).ToListAsync();
            return Users;
        }

        // API endpoint to edit the status of a user by an admin
        [HttpPost("EditStatus")]
        public async Task<IActionResult> EditStatusOfUserByAdmin(int id, string status)
        {
            // Find the user with the provided ID
            var user = await _context.Users.FindAsync(id);
            // If user does not exist, return a NotFound response
            if (user == null)
            {
                return NotFound();
            }

            // Update the user's status and save changes to the database
            user.status = status;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            // Return an OK response
            return Ok();
        }

    }
}

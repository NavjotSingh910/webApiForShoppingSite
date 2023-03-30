using WebApplication3.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApplication3.Helper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure UserContext to use the SQL Server database using a connection string from the appsettings.json file.
builder.Services.AddDbContext<UserContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("UserDb1")));

// Add the MailHelper service to the container.
builder.Services.AddScoped<MailHelper>();

// Add services to the container.
builder.Services.AddControllers();

// Configure JWT authentication options
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Configure the options for token validation
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// Add authorization services to the container
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Redirect HTTP requests to HTTPS.
app.UseHttpsRedirection();

// Use JWT authentication middleware.
app.UseAuthentication();

// Use authorization middleware.
app.UseAuthorization();

// Map controller routes.
app.MapControllers();

// Start the application.
app.Run();

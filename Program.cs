using Microsoft.EntityFrameworkCore;
using KozoskodoAPI.Models;
using KozoskodoAPI.Data;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Buffers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using KozoskodoAPI.Auth;

namespace KozoskodoAPI
{
    public class Program 
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Az appsettings.json fájlból olvasson be adatot
            builder.Services.AddDbContext<DBContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("MediaDB"),
                    ServerVersion.Parse("10.4.6-mariadb")));

            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(jwt =>
                {
                    var key = builder.Configuration.GetValue<string>("JwtConfig:Key");
                    var keyBytes = Encoding.ASCII.GetBytes(key!);
                    jwt.SaveToken = true;
                    jwt.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                        ValidateLifetime = true,
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ClockSkew = TimeSpan.Zero
                    };
                });
            builder.Services.AddAuthorization();

            builder.Services.AddControllers().AddNewtonsoftJson();
            builder.Services.AddControllers();
            builder.Services.AddScoped<IJwtTokenManager, JwtTokenManager>();

            var app = builder.Build();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            
      
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.MapControllers();
            app.Run();
        }
    }
}
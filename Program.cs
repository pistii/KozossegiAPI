using Microsoft.EntityFrameworkCore;
using KozoskodoAPI.Models;
using KozoskodoAPI.Data;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Buffers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc;

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

            builder.Services.AddControllers().AddNewtonsoftJson();
            builder.Services.AddControllers();

            var app = builder.Build();

            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.MapControllers();
            app.Run();
        }
    }
}
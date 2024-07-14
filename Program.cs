using Microsoft.EntityFrameworkCore;
using KozoskodoAPI.Data;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using KozoskodoAPI.Auth;
using KozoskodoAPI.Realtime;
using KozoskodoAPI.Auth.Helpers;
using KozoskodoAPI.Controllers;
using KozoskodoAPI.Realtime.Connection;
using KozoskodoAPI.Controllers.Cloud;
using KozoskodoAPI.Repo;
using KozoskodoAPI.SMTP;
using KozoskodoAPI.Security;
using KozoskodoAPI.Services;
using KozoskodoAPI.Models;
using KozoskodoAPI.DTOs;
using KozossegiAPI.SMTP;
using KozossegiAPI.Controllers.Cloud;
using KozossegiAPI.Storage;
using KozossegiAPI.Controllers.Cloud.Helpers;
using Serilog;

namespace KozoskodoAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var services = builder.Services;
            // Az appsettings.json fájlból olvasson be adatot
            services.AddDbContext<DBContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("MediaDB"),
                    ServerVersion.Parse("10.4.20-mariadb"))); //10.4.6-mariadb

            services.AddAuthentication(options =>
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
                        ValidateIssuer = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });
            
            services.AddAuthorization();
            services.AddAuthentication();
            
            services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

            services.AddScoped<IJwtUtils, JwtUtils>();
            services.AddScoped<IJwtTokenManager, JwtTokenManager>();
            services.AddScoped<IUserService, UserService>();
            services.AddSingleton<IMapConnections,  ConnectionMapping>();
            services.AddScoped<StorageController>();
            services.AddScoped<IStorageController, StorageController>();
            services.AddScoped<IFriendRepository, FriendRepository>();
            services.AddScoped<IUserRepository<user>, UserRepository>();
            services.AddScoped<IChatRepository<ChatRoom, Personal>, ChatRepository>();
            services.AddScoped<IPostRepository<PostDto>, PostRepository>();
            services.AddScoped<IPostRepository<Comment>, PostRepository>();
            services.AddScoped<IPersonalRepository, PersonalRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IImageRepository, ImageController>();
            services.AddScoped<IMailSender, SendMail>();
            services.AddSingleton<IVerificationCodeCache, VerificationCodeCache>();
            services.AddScoped<IEncodeDecode, EncodeDecode>();
            services.AddScoped<IFileHandlerService, FileHandlerService>();
            services.AddSingleton<IChatStorage, ChatStorage>();
            services.AddHostedService<NotificationService>();

            services.AddMemoryCache();

            services.AddHttpContextAccessor();

            services.AddMvc();

            //services.AddLogging(builder => builder.AddConsole());
            services.AddSerilog();
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

            services.AddSignalR(options =>
            {
                options.MaximumReceiveMessageSize = 102400000;
                options.EnableDetailedErrors = true;
                
            });

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            services.AddControllers().AddNewtonsoftJson().AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())); ;

            var origins = "http://localhost:5173";
            services.AddCors(options => options.AddPolicy("AllowAll", p => p
            .WithOrigins(origins)
            .AllowAnyMethod()
            .WithHeaders("content-type", "authorization")
            .AllowCredentials()
            ));
            
            var app = builder.Build();

            app.UseRouting();
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();

            app.Use(async (context, next) =>
            {
                Thread.CurrentPrincipal = context.User;
                await next(context);
            });

            app.UseMiddleware<JwtMiddleware>().UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.MapHub<NotificationHub>("/Notification");
            app.MapHub<ChatHub>("/Chat");
            
            app.MapControllers();
            app.Run();

        }
    }
}
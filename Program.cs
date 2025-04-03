using Microsoft.EntityFrameworkCore;
using KozossegiAPI.Data;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using KozossegiAPI.Auth;
using KozossegiAPI.Realtime;
using KozossegiAPI.Auth.Helpers;
using KozossegiAPI.Realtime.Connection;
using KozossegiAPI.SMTP;
using KozossegiAPI.Controllers.Cloud;
using KozossegiAPI.Repo;
using KozossegiAPI.Security;
using KozossegiAPI.Services;
using KozossegiAPI.Models;
using KozossegiAPI.DTOs;
using KozossegiAPI.Storage;
using Serilog;
using System.Threading.RateLimiting;
using AutoMapper;
using KozoskodoAPI.Repo;
using KozossegiAPI.Interfaces;

namespace KozossegiAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var services = builder.Services;
            
            // Az appsettings.json f�jlb�l olvasson be adatot
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

                    jwt.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                (path.StartsWithSegments("/Chat")))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;


                            }
                            return Task.CompletedTask;
                        }
                    };
                });
            
            services.AddAuthorization();
            services.AddAuthentication();
            
            services.Configure<Auth.Helpers.AppSettings>(builder.Configuration.GetSection("AppSettings"));
            services.Configure<KozossegiAPI.SMTP.Helpers.AppSettings>(builder.Configuration.GetSection("SMTP"));

            services.AddScoped<IJwtUtils, JwtUtils>();
            services.AddScoped<IJwtTokenManager, JwtTokenManager>();
            services.AddScoped<IUserService, UserService>();
            services.AddSingleton<IMapConnections,  ConnectionMapping>();
            services.AddScoped<StorageRepository>();
            services.AddScoped<IStorageRepository, StorageRepository>();
            services.AddScoped<IFriendRepository, FriendRepository>();
            services.AddScoped<IUserRepository<user>, UserRepository>();
            services.AddScoped<IChatRepository<ChatRoom, Personal>, ChatRepository>();
            services.AddScoped<IPostRepository<PostDto>, PostRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IPersonalRepository, PersonalRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IImageRepository, ImageRepository>();
            services.AddScoped<IMailSender, SendMail>();
            services.AddSingleton<IVerificationCodeCache, VerificationCodeCache>();
            services.AddScoped<IEncodeDecode, EncodeDecode>();
            services.AddSingleton<IChatStorage, ChatStorage>();
            services.AddScoped<ISettingRepository, SettingRepository>();
            services.AddScoped<IStudyRepository, StudyRepository>();
            services.AddScoped<IMobileRepository<user>, MobileRepository>();

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

            var config = new MapperConfiguration(cfg =>
            {

                //cfg.CreateMap<SettingDto, UserDto>()
                //    .ForMember(dest => dest.personal, opt => opt.MapFrom(src => src.User.personal))
                //    .ForMember(dest => dest.StudiesDto, opt => opt.MapFrom(src => src.User.StudiesDto));

                //cfg.CreateMap<UserDto, user>()
                //    .ForMember(dest => dest.personal, opt => opt.MapFrom(src => src.personal))
                //    .ForMember(dest => dest.Studies, opt => opt.MapFrom(src => src.StudiesDto));
                cfg.CreateMap<UserDto, user>().ForMember(p => p.email, opt => opt.MapFrom(src => src.email));
                cfg.CreateMap<SettingDto, StudyDto>();
//cfg.CreateMap<UserDto, user>();
                
            });
            services.AddRateLimiter(options =>
            {
                // General limiter
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: context.Request.Headers.Host.ToString(),
                        factory: partition => new SlidingWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 50,
                            SegmentsPerWindow = 15,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                // Password changing
                options.AddPolicy("password_reset", context =>
                  RateLimitPartition.GetFixedWindowLimiter(context.Request.Headers.Host.ToString(),
                  partition => new FixedWindowRateLimiterOptions
                  {
                      PermitLimit = 3,
                      Window = TimeSpan.FromMinutes(30)
                  }));

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = 429;

                    // Set CORS headers
                    context.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");

                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        await context.HttpContext.Response.WriteAsync(
                            $"Too many requests. Try again in a few seconds", cancellationToken: token);
                    }
                    else
                    {
                        await context.HttpContext.Response.WriteAsync(
                            "Too many requests. Try again later.", cancellationToken: token);
                    }
                };
            });



            var mapper = config.CreateMapper();
            services.AddAutoMapper(typeof(Program));

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            services.AddControllers().AddNewtonsoftJson().AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            var origins = "http://localhost:5173;http://192.168.0.16:8888";
            services.AddCors(options => options.AddPolicy("AllowAll", p => p
            .WithOrigins(origins)
            .AllowAnyMethod()
            .WithHeaders("content-type", "authorization")
            .AllowCredentials()
            ));
            
            
            var app = builder.Build();

            app.UseRouting();
            app.UseSession();


            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<AuthenticateMiddleware>().UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseMiddleware<JwtMiddleware>().UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseRateLimiter();


            //app.Use(async (context, next) =>
            //{
            //    Thread.CurrentPrincipal = context.User;
            //    await next(context);
            //});

            //websockets
            app.MapHub<NotificationHub>("/Notification");
            app.MapHub<ChatHub>("/Chat");
            
            app.MapControllers();
            app.Run();

        }
    }
}
using EstateAccessManagement.Application.Interfaces.Email;
using EstateAccessManagement.Application.Interfaces.Messaging;
using EstateAccessManagement.Application.Interfaces.Services;
using EstateAccessManagement.Core.Email;
using EstateAccessManagement.Core.Entities;
using EstateAccessManagement.Core.Messaging;
using EstateAccessManagement.Infrastructure.BackgroundJobs;
using EstateAccessManagement.Infrastructure.Email;
using EstateAccessManagement.Infrastructure.Messaging;
using EstateAccessManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EstateAccessManagement.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("RedisCache");
            });

            services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            services.AddSingleton<IMessageQueueClient>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value;
                var logger = sp.GetRequiredService<ILogger<RabbitMqClient>>();
                return RabbitMqClient.CreateAsync(
                    options.HostName,
                    options.Port,
                    options.UserName,
                    options.Password,
                    options.VirtualHost,
                    logger).GetAwaiter().GetResult();
            });

            services.AddSingleton(sp =>
                sp.GetRequiredService<IOptions<EmailSettings>>().Value);
            services.AddTransient<IEmailService, EmailService>();
            services.AddScoped<NotificationWorker>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAccessCodeService, AccessCodeService>();
            services.AddScoped<IUserService, UserService>();
            services.AddHostedService<RabbitMqInitializer>();
            services.AddHostedService<NotificationBackgroundService>();

            services.AddIdentityCore<AppUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!);

            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };
            });

            return services;
        }
    }
}

using EstateAccessManagement.API.Filters;
using EstateAccessManagement.Application;
using EstateAccessManagement.Infrastructure;
using Microsoft.OpenApi.Models;
using Serilog;

namespace EstateAccessManagement.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, loggerConfig) =>
               loggerConfig.ReadFrom.Configuration(context.Configuration));

            // Add services to the container.
            builder.Services.AddControllers(options =>
            {
               options.Filters.Add<ApiExceptionFilter>();
            });

            builder.Services.AddHealthChecks();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Estate Access Management System API",
                    Version = "v1.0",
                    Description = "API for managing residential estate access control",
                });
            });

            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);

            var app = builder.Build();


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "EAMS API v1");
                    options.RoutePrefix = "swagger";
                    options.DocumentTitle = "EAMS API Documentation";
                });
            }

            app.MapHealthChecks("/health");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSerilogRequestLogging();
            app.MapControllers();

            app.Run();
        }
    }
}

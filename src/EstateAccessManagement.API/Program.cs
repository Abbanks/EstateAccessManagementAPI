using EstateAccessManagement.Infrastructure;
using EstateAccessManagement.Application;
using Microsoft.OpenApi.Models;
using Serilog;

namespace EstateAccessManagement.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, loggerConfig) =>
               loggerConfig.ReadFrom.Configuration(context.Configuration));

            // Add services to the container.
            builder.Services.AddControllers(options =>
            {
              //  options.Filters.Add<ApiExceptionFilter>();
            });

            builder.Services.AddOpenApi();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Estate Access Management System API",
                    Version = "v1.0",
                    Description = "API for managing residential estate access control",
                });
            });

            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddApplicationServices();
            builder.Services.AddControllers();

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

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.MapHealthChecks("/health");
            app.UseSerilogRequestLogging();

            app.Run();
        }
    }
}

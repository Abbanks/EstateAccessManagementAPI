using EstateAccessManagement.API.Filters;
using EstateAccessManagement.Application;
using EstateAccessManagement.Infrastructure;

namespace EstateAccessManagement.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting API...");
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<ApiExceptionFilter>();
            });
            builder.Services.AddHealthChecks();
            builder.Services.AddOpenApi();
            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddAuthorization();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var configuration = app.Configuration;

                try
                {
                    await serviceProvider.InitializeAsync(configuration);
                }
                catch (Exception ex)
                {
                    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();

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
            app.MapControllers();
            app.Run();
        }
    }
}

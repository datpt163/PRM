using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Builder;

namespace Capstone.Application
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddApplicationLocalization();

            var app = builder.Build();

            var supportedCultures = new[]
            {
                new CultureInfo("en"),
                new CultureInfo("vi")
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

            app.Run();
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationLocalization(this IServiceCollection services)
        {
            services.AddLocalization(options =>
                options.ResourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../Capstone.Application/Resources")
            );
            return services;
        }
    }
}

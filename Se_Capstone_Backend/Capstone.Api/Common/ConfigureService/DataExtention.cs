using Capstone.Domain.Entities;
using Capstone.Infrastructure.DbContexts;
using Capstone.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Api.Common.ConfigureService
{
    public static class DataExtention
    {
        public static void AddDataService(this IServiceCollection services, IConfiguration configuration)
        {
            var dbConnectionString = configuration.GetConnectionString("value");
            services.AddDbContext<SeCapstoneContext>(options => options.UseNpgsql(configuration.GetConnectionString("value")));
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddTransient<IRepository<Job>, Repository<Job>>();
            services.AddTransient<IRepository<Project>, Repository<Project>>();
            services.AddTransient<IRepository<Skill>, Repository<Skill>>();
            services.AddTransient<IRepository<Position>, Repository<Position>>();
            services.AddScoped<IRepository<User>, Repository<User>>();
            services.AddScoped<IRepository<Role>, Repository<Role>>();
            services.AddScoped<IRepository<Applicant>, Repository<Applicant>>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}

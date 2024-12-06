using Capstone.Api.Common.ConfigureService;
using Capstone.Api.Middleware;
using Capstone.Api.Module.Auth.Validator;
using Capstone.Api.Module.Statuses.SignalR;
using Capstone.Application;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.DbContexts;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationLocalization();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

#region Fluent Validation
builder.Services.AddFluentValidationAutoValidation(); 
builder.Services.AddFluentValidationClientsideAdapters(); 
builder.Services.AddValidatorsFromAssemblyContaining<RegisterCommandValidator>();
builder.Services.AddControllers();
#endregion

builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();

#region CORS
var corsUrls = builder.Configuration.GetSection("Cors:Urls").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
           
            policy.WithOrigins(corsUrls)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                .AllowCredentials(); 
        });
});
#endregion

builder.Services.AddSignalR();
builder.Services.AddSwaggerService();
builder.Services.AddDataService(builder.Configuration);

#region MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AssemblyReference.Assembly));
builder.Services.AddIdentity<User, Capstone.Domain.Entities.Role>()
    .AddEntityFrameworkStores<SeCapstoneContext>()
    .AddDefaultTokenProviders();
#endregion

builder.Services.AddAuthSerivce(builder.Configuration);
builder.Services.AddMassTransitService(builder.Configuration);

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
        options.TokenLifespan = TimeSpan.FromDays(1));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

#region Redis Cache
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(
    builder.Configuration.GetConnectionString("Redis") ?? "", true);
    configuration.ResolveDns = true;
    return ConnectionMultiplexer.Connect(configuration);
});
#endregion

builder.Services.AddGreetingService(builder.Configuration);
var app = builder.Build();
app.UseCors("AllowAll");
#region Cultures
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
#endregion

#region MiddleWare
app.UseMiddleware<BlacklistedTokenMiddleware>();
#endregion

#region Environment
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
app.UseDeveloperExceptionPage();
//}
#endregion


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

#region MigrateDbContext
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SeCapstoneContext>();
    dbContext.Database.Migrate();
}
#endregion



app.MapHub<StatusHub>("/statusHub");
app.MapHub<NotificationHub>("/notificationHub");
app.Run();

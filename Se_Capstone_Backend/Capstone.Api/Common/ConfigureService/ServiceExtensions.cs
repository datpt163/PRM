using Capstone.Application;
using Capstone.Application.Common.Cloudinaries;
using Capstone.Application.Common.Cohere;
using Capstone.Application.Common.Email;
using Capstone.Application.Common.FileService;
using Capstone.Application.Common.Gpt;
using Capstone.Application.Common.Helper;
using Capstone.Application.Common.HuggingFace;
using Capstone.Application.Common.ProjectAuthorize;
using Capstone.Application.Common.RabbitMQ;
using Capstone.Application.Common.TokenService;
using Capstone.Domain.Module.Auth.TokenBlackList;
using Capstone.Infrastructure.Redis;
using Microsoft.Extensions.Options;

namespace Capstone.Api.Common.ConfigureService
{
    public static class ServiceExtensions
    {
        public static void AddGreetingService(this IServiceCollection services, IConfiguration configuration)
        {
            #region ServiceCommon
            services.AddScoped<IManagePermissionProject, ManagePermissionProject>();
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
            services.AddScoped<IEmailService, EmailService>();
            //Rabbit
            services.Configure<RabbitMQSettings>(configuration.GetSection("RabbitMQSettings"));

            services.AddSingleton(sp =>
            {
                var rabbitMQSettings = sp.GetRequiredService<IOptions<RabbitMQSettings>>().Value;
                return new RabbitMQProducer(rabbitMQSettings.HostName, rabbitMQSettings.QueueName);
            });

            services.AddScoped<IFileService, FileService>();

            services.AddSingleton(sp =>
            {
                var rabbitMQSettings = sp.GetRequiredService<IOptions<RabbitMQSettings>>().Value;
                var emailService = sp.GetRequiredService<IEmailService>();
                var logger = sp.GetRequiredService<ILogger<RabbitMQConsumer>>();
                return new RabbitMQConsumer(emailService, rabbitMQSettings.HostName, rabbitMQSettings.QueueName, logger);
            });

            services.Configure<RedisSettings>(configuration.GetSection("RedisDBSettings"));
            services.AddSingleton<RedisContext, RedisContext>();
            services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();
            services.AddHttpClient<IChatGPTService, ChatGPTService>();
            services.AddHttpClient<IHuggingFaceService, HuggingFaceService>();
            services.AddHttpClient<ICohereService, CohereService>();
            services.AddScoped<PdfReaderService>();
            services.AddSingleton<CloudinaryService>();
            services.AddScoped<ITokenRevocationService, TokenRevocationService>();

            #endregion

            #region Localization
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            #endregion

            #region Middleware
            #endregion

        }
    }
}

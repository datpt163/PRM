using Capstone.Application.Common.Email.EmailQueue;
using Capstone.Application.Module.Issues.ConsumerRabbitMq;
using Capstone.Application.Module.Issues.ConsumerRabbitMq.Message;
using Capstone.Application.Module.Status.ConsumerRabbitMq;
using MassTransit;

namespace Capstone.Api.Common.ConfigureService
{
    public static class MassTransitExtention
    {
        public static void AddMassTransitService(this IServiceCollection services, IConfiguration configuration)
        {
            var RabbitMqSetting = configuration.GetSection("MessageBroker");
            string host = RabbitMqSetting["host"] ?? string.Empty;
            string userName = RabbitMqSetting["userName"] ?? string.Empty;
            string password = RabbitMqSetting["password"] ?? string.Empty;

            services.AddMassTransit(busConfiguration =>
            {
                //busConfiguration.SetKebabCaseEndpointNameFormatter();

                busConfiguration.AddConsumer<OrderStatusConsumer>();
                busConfiguration.AddConsumer<AddIssueConsumer>();
                busConfiguration.AddConsumer<OrderIssueConsumer>();
                busConfiguration.AddConsumer<SendEmailConsumer>();


                busConfiguration.UsingRabbitMq((context, configuration) =>
                {
                    configuration.Host(new Uri(host), h =>
                    {
                        h.Username(userName);
                        h.Password(password);
                    });

                    configuration.ReceiveEndpoint("order-status-endpoint", e =>
                    {
                        e.ConfigureConsumer<OrderStatusConsumer>(context);
                    });

                    configuration.ReceiveEndpoint("add-issue-endpoint", e =>
                    {
                        e.ConfigureConsumer<AddIssueConsumer>(context);
                    });

                    configuration.ReceiveEndpoint("order-issue-endpoint", e =>
                    {
                        e.ConfigureConsumer<OrderIssueConsumer>(context);
                    });

                    configuration.ReceiveEndpoint("send-email-endpoint", e =>
                    {
                        e.ConfigureConsumer<SendEmailConsumer>(context);
                    });
                    //configuration.ConfigureEndpoints(context);
                });

                //busConfiguration.AddRequestClient<AddIssueMessage>();
            });
        }
    }
}

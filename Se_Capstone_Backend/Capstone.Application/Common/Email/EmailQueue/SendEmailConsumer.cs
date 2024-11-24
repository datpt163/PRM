using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Common.Email.EmailQueue
{
    public class SendEmailConsumer : IConsumer<SendEmailMessage>
    {
        private readonly IEmailService _emailService;

        public SendEmailConsumer(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task Consume(ConsumeContext<SendEmailMessage> context)
        {
            if (!string.IsNullOrEmpty(context.Message.ToEmail))
                await _emailService.SendEmailAsync(context.Message.ToEmail, context.Message.Subject, context.Message.Body);
        }
    }
}

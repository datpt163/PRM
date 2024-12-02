using Capstone.Application.Module.Issues.ConsumerRabbitMq;
using Capstone.Application.Module.Status.ConsumerRabbitMq.Message;
using Capstone.Infrastructure.Repository;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Application.Module.Status.ConsumerRabbitMq
{
    public class OrderStatusConsumer : IConsumer<OrderStatusMessage2>
    {
        private readonly IUnitOfWork _unitOfWork;
        public OrderStatusConsumer(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Consume(ConsumeContext<OrderStatusMessage2> context)
        {
            var position = context.Message.Position;
            var status = _unitOfWork.Statuses.Find(x => x.Id == context.Message.Status.Id).Include(c => c.Project).ThenInclude(c => c.Statuses).FirstOrDefault();
            if (status != null)
            {
                if (position > status.Position)
                {
                    foreach (var s in status.Project.Statuses)
                        if (s.Position > status.Position && s.Position <= position)
                            s.Position -= 1;
                }
                else
                {
                    foreach (var s in status.Project.Statuses)
                        if (s.Position < status.Position && s.Position >= position)
                            s.Position += 1;
                }
                status.Position = position;
                _unitOfWork.Statuses.Update(status);
                await _unitOfWork.SaveChangesAsync();
                await context.RespondAsync(new UserResponse() { UserId = "" });
            }
        }
    }
}

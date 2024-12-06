using Capstone.Application.Common.Email.EmailQueue;
using Capstone.Application.Common.EmailHTML;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Issues.NotiDate
{
    public class ScheduledTaskService : BackgroundService
    {
        private readonly TimeSpan _scheduledTime = new TimeSpan(14, 50, 0);
        private DateTime _nextRunTime;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ScheduledTaskService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _nextRunTime = GetNextRunTime();
        }

        private DateTime GetNextRunTime()
        {
            Console.WriteLine("vao roi");
            var now = DateTime.UtcNow;
            var nextRunDate = now.Date.Add(_scheduledTime);

            // Nếu thời gian hiện tại đã qua 8h sáng, lên lịch cho ngày tiếp theo
            if (now >= nextRunDate)
            {
                nextRunDate = nextRunDate.AddDays(1);
            }

            return nextRunDate;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var timeUntilNextRun = _nextRunTime - DateTime.UtcNow;

                // Nếu thời gian cho đến lần chạy tiếp theo còn nhỏ hơn 0, thì tính lại
                if (timeUntilNextRun <= TimeSpan.Zero)
                {
                    _nextRunTime = GetNextRunTime();
                    timeUntilNextRun = _nextRunTime - DateTime.UtcNow;
                }

                await Task.Delay(timeUntilNextRun, stoppingToken);
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var publisher = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                    var users = unitOfWork.Users.GetQuery().Include(c => c.LeadProjects).ThenInclude(c => c.Statuses).ThenInclude(c => c.Issues)
                                                            .Include(c => c.UserProjects).ThenInclude(c => c.Project).ThenInclude(c => c.Statuses).ThenInclude(c => c.Issues).ToList();

                    if (users != null) {
                        foreach (var u in users)
                        {

                            var issues = new List<Issue>();
                            issues.AddRange(u.LeadProjects.SelectMany(x => x.Statuses).Where(x => x.IsDone == false).SelectMany(x => x.Issues).Where(x => (x.AssigneeId != null && x.AssigneeId.Value == u.Id)));
                            issues.AddRange(u.UserProjects.Select(x => x.Project).SelectMany(x => x.Statuses).Where(x => x.IsDone == false).SelectMany(x => x.Issues).Where(x => (x.AssigneeId != null && x.AssigneeId.Value == u.Id)));

                            if(issues.Count() > 0)
                            {
                              
                                var requests = new List<NotificationIssue>();
                                foreach (var i in issues)
                                {
                                    var issue = unitOfWork.Issues.Find(x => x.Id == i.Id).Include(c => c.Status).ThenInclude(c => c.Project).FirstOrDefault();
                                    if(issue != null)
                                        requests.Add(new NotificationIssue() { ProjectName = issue.Status.Project.Name, IssueIndex = issue.Index, IssueName = issue.Title, StatusName = issue.Status.Name, DueDate = issue.DueDate, IssueId = issue.Id + "", ProjectId = issue.Status.ProjectId + "" });
                                }
                                Console.WriteLine(u.Email);
                                await publisher.Publish(new SendEmailMessage() { ToEmail = u.Email == null ? "" : u.Email, Body = EmailMessage.NotificationIssue(requests), Subject = $"Unfinished tasks" });
                            }
                        }
                    }
                    _nextRunTime = GetNextRunTime();
                }
                // Cập nhật thời gian chạy tiếp theo
                _nextRunTime = GetNextRunTime();
            }
        }
    }
}

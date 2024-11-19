using Capstone.Application.Module.Projects.Query;
using Capstone.Application.Module.Projects.QueryHandle;
using Capstone.Application.Module.Projects.Request;
using Capstone.Application.Module.Projects.Response;
using Capstone.Domain.Helpers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Capstone.Test.Module.Projects.QueryHandle
{
    public class CalculateEffortMetricsHandlerTest
    {
        private readonly CalculateEffortMetricsHandler _handler;

        public CalculateEffortMetricsHandlerTest()
        {
            _handler = new CalculateEffortMetricsHandler();
        }

        [Fact]
        public async Task Handle_ValidData_CalculatesMetricsCorrectly()
        {
            var tasks = new List<TaskEffort>
            {
                new TaskEffort { UserId = Guid.NewGuid(), UserName = "User1", EstimatedTime = 10, ActualTime = 8 },
                new TaskEffort { UserId = Guid.NewGuid(), UserName = "User2", EstimatedTime = 20, ActualTime = 15 },
                new TaskEffort { UserId = Guid.NewGuid(), UserName = "User1", EstimatedTime = 5, ActualTime = 6 }
            };

            var query = new CalculateEffortMetricsQuery
            {
                ProjectName = "Project A",
                Tasks = tasks,
                IsCalculateDetails = true
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.Equal("Project A", result.ProjectName);
            Assert.Equal(35, result.TotalEstimatedTime);
            Assert.Equal(29, result.TotalActualTime);

            Assert.NotNull(result.UserPerformance);

            var user1Performance = result.UserPerformance.FirstOrDefault(up => up.UserId == tasks[0].UserId);
            var user2Performance = result.UserPerformance.FirstOrDefault(up => up.UserId == tasks[1].UserId);

            Assert.NotNull(user1Performance);
            Assert.NotNull(user2Performance);
        }

        [Fact]
        public async Task Handle_NoTasks_ReturnsZeroMetrics()
        {
            var query = new CalculateEffortMetricsQuery
            {
                ProjectName = "Project A",
                Tasks = new List<TaskEffort>(),
                IsCalculateDetails = true
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.Equal("Project A", result.ProjectName);
            Assert.Equal(0, result.TotalEstimatedTime);
            Assert.Equal(0, result.TotalActualTime);
            Assert.Equal(0, result.CompletionRate);
            Assert.Empty(result.UserPerformance);
        }

        [Fact]
        public async Task Handle_CalculateDetailsFalse_DoesNotCalculateUserPerformance()
        {
            var tasks = new List<TaskEffort>
            {
                new TaskEffort { UserId = Guid.NewGuid(), UserName = "User1", EstimatedTime = 10, ActualTime = 8 },
                new TaskEffort { UserId = Guid.NewGuid(), UserName = "User2", EstimatedTime = 20, ActualTime = 15 }
            };

            var query = new CalculateEffortMetricsQuery
            {
                ProjectName = "Project A",
                Tasks = tasks,
                IsCalculateDetails = false
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.Equal("Project A", result.ProjectName);
            Assert.Equal(30, result.TotalEstimatedTime);
            Assert.Equal(23, result.TotalActualTime);

        }
    }
}

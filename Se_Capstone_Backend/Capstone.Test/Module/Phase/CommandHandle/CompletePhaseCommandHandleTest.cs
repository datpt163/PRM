using Capstone.Application.Module.Projects.Command;
using Capstone.Application.Module.Projects.CommandHandle;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Projects.Response;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Capstone.Application.Common.FileService;
using System.Linq.Expressions;
using Capstone.Application.Module.Phase.Command;
using Capstone.Application.Module.Phase.CommandHandle;

namespace Capstone.Test.Module.Phases.CommandHandle
{
    public class CompletePhaseCommandHandleTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IFileService> _mockFileService;
        private readonly CompletePhaseCommandHandle _handler;

        public CompletePhaseCommandHandleTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUserManager = new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
            _mockMapper = new Mock<IMapper>();
            _mockFileService = new Mock<IFileService>();
            _handler = new CompletePhaseCommandHandle(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_PhaseDoesNotExist_ReturnsError()
        {
            // Arrange
            var command = new CompletePhaseCommand
            {
                ProjectId = Guid.NewGuid(),
            };

            var project = new Project("Test Project", "PROJ001", "Test Description", DateTime.Now, DateTime.Now.AddDays(10), Guid.NewGuid(), true);
            _mockUnitOfWork.Setup(u => u.Projects.Find(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(new[] { project }.AsQueryable());

            _mockUnitOfWork.Setup(u => u.Phases.Find(It.IsAny<Expression<Func<Phase, bool>>>()))
                .Returns(Enumerable.Empty<Phase>().AsQueryable());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("Project does not have any phase.", result.ErrorMessage);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task Handle_PhaseAlreadyCompleted_ReturnsError()
        {
            // Arrange
            var command = new CompletePhaseCommand
            {
                ProjectId = Guid.NewGuid(),
            };

            var phase = new Phase
            {
                Title = "Phase 1",
                ExpectedStartDate = DateTime.Now.AddDays(-5),
                ExpectedEndDate = DateTime.Now.AddDays(5),
                ActualStartDate = DateTime.Now.AddDays(-5),
                ActualEndDate = DateTime.Now.AddDays(5)
            };
            var project = new Project("Test Project", "PROJ001", "Test Description", DateTime.Now, DateTime.Now.AddDays(10), Guid.NewGuid(), true);
            project.Phases.Add(phase);

            _mockUnitOfWork.Setup(u => u.Projects.Find(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(new[] { project }.AsQueryable());

            _mockUnitOfWork.Setup(u => u.Phases.Find(It.IsAny<Expression<Func<Phase, bool>>>()))
                .Returns(new[] { phase }.AsQueryable());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(string.Empty, result.ErrorMessage);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task Handle_NomorePhase_ReturnsError()
        {
            // Arrange
            var command = new CompletePhaseCommand
            {
                ProjectId = Guid.NewGuid(),
            };

            var phase = new Phase
            {
                Title = "Phase 1",
                ExpectedStartDate = DateTime.Now.AddDays(-5),
                ExpectedEndDate = DateTime.Now.AddDays(5),
                ActualStartDate = DateTime.Now.AddDays(-5),
                ActualEndDate = null // Phase not completed yet
            };
            var project = new Project("Test Project", "PROJ001", "Test Description", DateTime.Now, DateTime.Now.AddDays(10), Guid.NewGuid(), true);
            project.Phases.Add(phase);

            _mockUnitOfWork.Setup(u => u.Projects.Find(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(new[] { project }.AsQueryable());

            _mockUnitOfWork.Setup(u => u.Phases.Find(It.IsAny<Expression<Func<Phase, bool>>>()))
                .Returns(new[] { phase }.AsQueryable());

            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("No more phase.", result.ErrorMessage);
            Assert.Null(result.Data);
        }
    }
}

using Capstone.Application.Module.Phase.CommandHandle;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Resources;
using Capstone.Domain.Entities;
using Capstone.Domain.Enums;
using Capstone.Infrastructure.Repository;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Capstone.Application.Module.Phase.Command;
using System.Linq.Expressions;

namespace Capstone.Test.Module.Phases.CommandHandle
{
    public class CreatePhaseCommandHandleTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly CreatePhaseCommandHandle _handler;

        public CreatePhaseCommandHandleTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _handler = new CreatePhaseCommandHandle(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_ProjectNotFound_ReturnsError()
        {
            var command = new CreatePhaseCommand
            {
                ProjectId = Guid.NewGuid(),
                Title = "New Phase",
                ExpectedStartDate = DateTime.Now.AddDays(1),
                ExpectedEndDate = DateTime.Now.AddDays(2)
            };

            _mockUnitOfWork.Setup(u => u.Projects.Find(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(Enumerable.Empty<Project>().AsQueryable());

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal("Project not found.", result.ErrorMessage);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task Handle_EmptyTitle_ReturnsError()
        {
            var command = new CreatePhaseCommand
            {
                ProjectId = Guid.NewGuid(),
                Title = "",
                ExpectedStartDate = DateTime.Now.AddDays(1),
                ExpectedEndDate = DateTime.Now.AddDays(2)
            };

            var project = new Project { Id = command.ProjectId, Phases = new List<Phase>() };
            _mockUnitOfWork.Setup(u => u.Projects.Find(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(new[] { project }.AsQueryable());

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal("Title is empty.", result.ErrorMessage);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task Handle_PhaseTitleAlreadyExists_ReturnsError()
        {
            var command = new CreatePhaseCommand
            {
                ProjectId = Guid.NewGuid(),
                Title = "Existing Phase",
                ExpectedStartDate = DateTime.Now.AddDays(1),
                ExpectedEndDate = DateTime.Now.AddDays(2)
            };

            var project = new Project
            {
                Id = command.ProjectId,
                Phases = new List<Phase>
                {
                    new Phase { Title = "Existing Phase", ProjectId = command.ProjectId }
                }
            };

            _mockUnitOfWork.Setup(u => u.Projects.Find(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(new[] { project }.AsQueryable());

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal("This title phase is available.", result.ErrorMessage);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task Handle_EndDateEarlierThanStartDate_ReturnsError()
        {
            var command = new CreatePhaseCommand
            {
                ProjectId = Guid.NewGuid(),
                Title = "New Phase",
                ExpectedStartDate = DateTime.Now.AddDays(2),
                ExpectedEndDate = DateTime.Now.AddDays(1)
            };

            var project = new Project { Id = command.ProjectId, Phases = new List<Phase>() };
            _mockUnitOfWork.Setup(u => u.Projects.Find(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(new[] { project }.AsQueryable());

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal("End date must be greater or equal to the start date.", result.ErrorMessage);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task Handle_SuccessfulPhaseCreation_ReturnsPhase()
        {
            var command = new CreatePhaseCommand
            {
                ProjectId = Guid.NewGuid(),
                Title = "New Phase",
                ExpectedStartDate = DateTime.Now.AddDays(1),
                ExpectedEndDate = DateTime.Now.AddDays(2)
            };

            var project = new Project { Id = command.ProjectId, Phases = new List<Phase>() };

            _mockUnitOfWork.Setup(u => u.Projects.Find(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(new[] { project }.AsQueryable());

            _mockUnitOfWork.Setup(u => u.Phases.Add(It.IsAny<Phase>()))
                .Callback<Phase>((phase) => project.Phases.Add(phase));

            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(string.Empty, result.ErrorMessage);
            Assert.NotNull(result.Data);

            var createdPhase = result.Data as Phase;
            Assert.NotNull(createdPhase);
            Assert.Equal(command.Title, createdPhase?.Title);
        }

    }
}

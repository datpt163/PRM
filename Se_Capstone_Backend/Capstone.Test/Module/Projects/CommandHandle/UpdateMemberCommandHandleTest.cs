using Capstone.Application.Module.Projects.Command;
using Capstone.Application.Module.Projects.CommandHandle;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Infrastructure.Repository;
using Moq;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Capstone.Domain.Entities;

namespace Capstone.Test.Module.Projects.CommandHandle
{
    public class UpdateMemberCommandHandleTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly UpdateMemberCommandHandle _handler;

        public UpdateMemberCommandHandleTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _handler = new UpdateMemberCommandHandle(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_UserProjectNotFound_ReturnsNotFoundError()
        {
            var command = new UpdateMemberCommand
            {
                ProjectId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                PositionId = null,
                IsProjectConfigurator = true,
                IsIssueConfigurator = false,
                IsCommentConfigurator = false
            };

            // Mocking the UserProjects repository to return no user project for the given ProjectId and UserId
            _mockUnitOfWork.Setup(uow => uow.UserProjects.Find(It.IsAny<Expression<Func<UserProject, bool>>>()))
                .Returns(Enumerable.Empty<UserProject>().AsQueryable());

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal("User project not found", result.ErrorMessage);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task Handle_PositionNotFound_ReturnsNotFoundError()
        {
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var positionId = Guid.NewGuid();
            var command = new UpdateMemberCommand
            {
                ProjectId = projectId,
                UserId = userId,
                PositionId = positionId,
                IsProjectConfigurator = true,
                IsIssueConfigurator = false,
                IsCommentConfigurator = false
            };

            var userProject = new UserProject { ProjectId = projectId, UserId = userId };

            // Mocking the UserProjects repository to return the user project for the given ProjectId and UserId
            _mockUnitOfWork.Setup(uow => uow.UserProjects.Find(It.IsAny<Expression<Func<UserProject, bool>>>()))
                .Returns(new[] { userProject }.AsQueryable());

            // Mocking the Positions repository to return no position for the given PositionId
            _mockUnitOfWork.Setup(uow => uow.Positions.Find(It.IsAny<Expression<Func<Position, bool>>>()))
                .Returns(Enumerable.Empty<Position>().AsQueryable());

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal("", result.ErrorMessage);
        }

        [Fact]
        public async Task Handle_SuccessfulUpdate_ReturnsSuccess()
        {
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var positionId = Guid.NewGuid();
            var command = new UpdateMemberCommand
            {
                ProjectId = projectId,
                UserId = userId,
                PositionId = positionId,
                IsProjectConfigurator = true,
                IsIssueConfigurator = false,
                IsCommentConfigurator = true
            };

            var userProject = new UserProject { ProjectId = projectId, UserId = userId };

            // Mocking the UserProjects repository to return the user project for the given ProjectId and UserId
            _mockUnitOfWork.Setup(uow => uow.UserProjects.Find(It.IsAny<Expression<Func<UserProject, bool>>>()))
                .Returns(new[] { userProject }.AsQueryable());

            var position = new Position { Id = positionId };

            // Mocking the Positions repository to return the position for the given PositionId
            _mockUnitOfWork.Setup(uow => uow.Positions.Find(It.IsAny<Expression<Func<Position, bool>>>()))
                .Returns(new[] { position }.AsQueryable());

            // Mocking Update method to simulate the repository update
            _mockUnitOfWork.Setup(uow => uow.UserProjects.Update(It.IsAny<UserProject>()));

            // Mocking SaveChangesAsync method to return a successful task
            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync())
                .Returns(Task.FromResult(1));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(string.Empty, result.ErrorMessage);
            Assert.NotNull(result.Data);
            var updatedUserProject = result.Data as UserProject;
            Assert.Equal(positionId, updatedUserProject.PositionId);
            Assert.True(updatedUserProject.IsProjectConfigurator);
            Assert.False(updatedUserProject.IsIssueConfigurator);
            Assert.True(updatedUserProject.IsCommentConfigurator);

            // Verify that Update and SaveChangesAsync were called exactly once
            _mockUnitOfWork.Verify(uow => uow.UserProjects.Update(It.Is<UserProject>(up => up.ProjectId == projectId && up.UserId == userId)), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }
    }
}

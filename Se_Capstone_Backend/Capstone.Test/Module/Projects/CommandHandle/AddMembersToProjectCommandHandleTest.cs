using Capstone.Application.Module.Projects.Command;
using Capstone.Application.Module.Projects.CommandHandle;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using System.Linq.Expressions;

namespace Capstone.Test.Module.Projects.CommandHandle
{
    public class AddMembersToProjectCommandHandleTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly AddMembersToProjectCommandHandle _handler;

        public AddMembersToProjectCommandHandleTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _handler = new AddMembersToProjectCommandHandle(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_ProjectNotFound_ReturnsError()
        {
            // Arrange
            var command = new AddMembersToProject
            {
                ProjectId = Guid.NewGuid(),
                MemberIds = new List<Guid> { Guid.NewGuid() }
            };

            _mockUnitOfWork.Setup(u => u.Projects.Find(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(Enumerable.Empty<Project>().AsQueryable());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("Project not found", result.ErrorMessage);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task Handle_EmptyMemberList_ReturnsError()
        {
            // Arrange
            var command = new AddMembersToProject
            {
                ProjectId = Guid.NewGuid(),
                MemberIds = new List<Guid>() // Empty list
            };

            var project = new Project("Test Project", "PROJ001", "Description", DateTime.Now, DateTime.Now.AddDays(10), Guid.NewGuid(), false);
            _mockUnitOfWork.Setup(u => u.Projects.Find(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(new[] { project }.AsQueryable());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("List member empty", result.ErrorMessage);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task Handle_MemberNotFound_ReturnsError()
        {
            // Arrange
            var command = new AddMembersToProject
            {
                ProjectId = Guid.NewGuid(),
                MemberIds = new List<Guid> { Guid.NewGuid() }
            };

            var project = new Project("Test Project", "PROJ001", "Description", DateTime.Now, DateTime.Now.AddDays(10), Guid.NewGuid(), false);
            _mockUnitOfWork.Setup(u => u.Projects.Find(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(new[] { project }.AsQueryable());

            _mockUnitOfWork.Setup(u => u.Users.FindOne(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns((User)null); // Member not found

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("Member not found", result.ErrorMessage);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task Handle_AddMembersToProject_Success()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var command = new AddMembersToProject
            {
                ProjectId = projectId,
                MemberIds = new List<Guid> { memberId }
            };

            var project = new Project("Test Project", "PROJ001", "Description", DateTime.Now, DateTime.Now.AddDays(10), Guid.NewGuid(), false);
            var user = new User { Id = memberId, FullName = "John Doe" };

            _mockUnitOfWork.Setup(u => u.Projects.Find(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(new[] { project }.AsQueryable());
            _mockUnitOfWork.Setup(u => u.Users.FindOne(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(user);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(string.Empty, result.ErrorMessage);
            Assert.Null(result.Data);
            _mockUnitOfWork.Verify(u => u.Projects.Update(It.IsAny<Project>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}

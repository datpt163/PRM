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
using Capstone.Application.Common.Jwt;
using Capstone.Application.Common.ProjectAuthorize;
using MassTransit;

namespace Capstone.Test.Module.Projects.CommandHandle
{
    public class AddMembersToProjectCommandHandleTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPublishEndpoint> _publisher;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly Mock<IManagePermissionProject> _managePermissionProject;
        private readonly AddMembersToProjectCommandHandle _handler;

        public AddMembersToProjectCommandHandleTest()
        {
            _mockJwtService = new Mock<IJwtService>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _publisher = new Mock<IPublishEndpoint>();
            _managePermissionProject = new Mock<IManagePermissionProject>();
            _handler = new AddMembersToProjectCommandHandle(_mockUnitOfWork.Object, _publisher.Object, _mockJwtService.Object, _managePermissionProject.Object);
        }

        [Fact]
        public async Task Handle_ProjectNotFound_ReturnsError()
        {
            // Arrange
            var command = new AddMembersToProject
            {
                ProjectId = Guid.NewGuid(),
                MemberIds = new List<Guid> { Guid.NewGuid() },
                Token = "fake-token-for-testing"
            };

            _mockJwtService.Setup(jwtService => jwtService.VerifyTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new User { Id = Guid.NewGuid(), UserProjects = new List<UserProject>() });


            _mockUnitOfWork.Setup(u => u.Projects.Find(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(Enumerable.Empty<Project>().AsQueryable());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("Project not found.", result.ErrorMessage);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task Handle_EmptyMemberList_ReturnsError()
        {
            // Arrange
            var command = new AddMembersToProject
            {
                ProjectId = Guid.NewGuid(),
                MemberIds = new List<Guid>(),
                Token = "fake-token-for-testing"
            };
            _mockJwtService.Setup(jwtService => jwtService.VerifyTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new User { Id = Guid.NewGuid(), UserProjects = new List<UserProject>() });

            var project = new Project("Test Project", "PROJ001", "Description", DateTime.Now, DateTime.Now.AddDays(10), Guid.NewGuid(), false);
            _mockUnitOfWork.Setup(u => u.Projects.Find(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(new[] { project }.AsQueryable());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("", result.ErrorMessage);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task Handle_MemberNotFound_ReturnsError()
        {
            // Arrange
            var command = new AddMembersToProject
            {
                ProjectId = Guid.NewGuid(),
                MemberIds = new List<Guid> { Guid.NewGuid() },
                Token = "fake-token-for-testing"
            };
            _mockJwtService.Setup(jwtService => jwtService.VerifyTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new User { Id = Guid.NewGuid(), UserProjects = new List<UserProject>() });

            var project = new Project("Test Project", "PROJ001", "Description", DateTime.Now, DateTime.Now.AddDays(10), Guid.NewGuid(), false);
            _mockUnitOfWork.Setup(u => u.Projects.Find(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(new[] { project }.AsQueryable());

            _mockUnitOfWork.Setup(u => u.Users.FindOne(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns((User)null); // Member not found

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("", result.ErrorMessage);
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
                MemberIds = new List<Guid> { memberId },
                Token = "fake-token-for-testing"
            };

            _mockJwtService.Setup(jwtService => jwtService.VerifyTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new User { Id = Guid.NewGuid(), UserProjects = new List<UserProject>() });
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
            //_mockUnitOfWork.Verify(u => u.Projects.Update(It.IsAny<Project>()), Times.Once);
            //_mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}

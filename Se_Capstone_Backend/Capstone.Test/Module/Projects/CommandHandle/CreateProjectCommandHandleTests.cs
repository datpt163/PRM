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

namespace Capstone.Test.Module.Projects.CommandHandle
{
    public class CreateProjectCommandHandleTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IFileService> _mockFileService;
        private readonly CreateProjectCommandHandle _handler;

        public CreateProjectCommandHandleTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUserManager = new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
            _mockMapper = new Mock<IMapper>();
            _mockFileService = new Mock<IFileService>();
            _handler = new CreateProjectCommandHandle(_mockUnitOfWork.Object, _mockUserManager.Object, _mockMapper.Object, _mockFileService.Object);
        }

        [Fact]
        public async Task Handle_ProjectCodeAlreadyExists_ReturnsError()
        {
            // Arrange
            var command = new CreateProjectCommand
            {
                Name = "Project A",
                Code = "PROJ001",
                Description = "Test Project",
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(10),
                LeadId = Guid.NewGuid()
            };

            var existingProject = new Project("Existing Project", "PROJ001", "Existing Description", DateTime.Now, DateTime.Now.AddDays(10), null, false);
            _mockUnitOfWork.Setup(u => u.Projects.Find(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(new[] { existingProject }.AsQueryable());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("Project code is exist", result.ErrorMessage);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task Handle_EndDateBeforeStartDate_ReturnsError()
        {
            // Arrange
            var command = new CreateProjectCommand
            {
                Name = "Project B",
                Code = "PROJ002",
                Description = "Test Project B",
                StartDate = DateTime.Now.AddDays(10),
                EndDate = DateTime.Now.AddDays(5),
                LeadId = Guid.NewGuid()
            };

            var user = new User { Id = command.LeadId.Value, FullName = "John Doe" };
            _mockUnitOfWork.Setup(u => u.Users.Find(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(new[] { user }.AsQueryable());

            _mockUnitOfWork.Setup(u => u.Users.Find(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(new[] { user }.AsQueryable());

            _mockUnitOfWork.Setup(u => u.Projects.Find(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(Enumerable.Empty<Project>().AsQueryable());

            _mockUnitOfWork.Setup(u => u.Projects.Add(It.IsAny<Project>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("End date must be greater or equal than the start date", result.ErrorMessage);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task Handle_ValidProjectCreation_ReturnsSuccess()
        {
            var command = new CreateProjectCommand
            {
                Name = "Project C",
                Code = "PROJ003",
                Description = "Test Project C",
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(10),
                LeadId = Guid.NewGuid()
            };

            var user = new User { Id = command.LeadId.Value, FullName = "John Doe" };
            _mockUnitOfWork.Setup(u => u.Users.Find(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(new[] { user }.AsQueryable());

            _mockUnitOfWork.Setup(u => u.Projects.Find(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(Enumerable.Empty<Project>().AsQueryable());

            _mockUnitOfWork.Setup(u => u.Projects.Add(It.IsAny<Project>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var projectCreate = new Project(command.Name, command.Code, command.Description, command.StartDate, command.EndDate, command.LeadId, false);
            _mockMapper.Setup(m => m.Map<ProjectDTO>(It.IsAny<Project>())).Returns(new ProjectDTO
            {
                Id = projectCreate.Id,
                Name = projectCreate.Name,
                LeadName = user.FullName
            });

            _mockFileService.Setup(fs => fs.ReadFileAsync(It.IsAny<string>()))
                .ReturnsAsync("[{\"Name\":\"Status1\",\"Position\":1,\"Description\":\"Description1\",\"Color\":\"#FFFFFF\"}]");

            var mockStatuses = new Mock<IRepository<Status>>();
            _mockUnitOfWork.Setup(u => u.Statuses).Returns(mockStatuses.Object);

            mockStatuses.Setup(s => s.AddRange(It.IsAny<IEnumerable<Status>>())).Verifiable();
            var mockLabels = new Mock<IRepository<Label>>();
            _mockUnitOfWork.Setup(u => u.Labels).Returns(mockLabels.Object);

            mockLabels.Setup(l => l.AddRange(It.IsAny<IEnumerable<Label>>())).Verifiable();
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(string.Empty, result.ErrorMessage);
            Assert.NotNull(result.Data);
            Assert.Equal("Project C", ((ProjectDTO)result.Data).Name);
            _mockUnitOfWork.Verify(u => u.Projects.Add(It.IsAny<Project>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Exactly(2));
            mockStatuses.Verify(s => s.AddRange(It.IsAny<IEnumerable<Status>>()), Times.Once);
        }


    }
}

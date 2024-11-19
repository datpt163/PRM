using Capstone.Application.Module.Projects.Command;
using Capstone.Application.Module.Projects.CommandHandle;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Infrastructure.Repository;
using Moq;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Capstone.Domain.Entities;

namespace Capstone.Test.Module.Projects.CommandHandle
{
    public class DeleteProjectCommandHandleTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IRepository<Project>> _mockProjectRepository;
        private readonly DeleteProjectCommandHandle _handler;

        public DeleteProjectCommandHandleTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockProjectRepository = new Mock<IRepository<Project>>();

            _mockUnitOfWork.Setup(u => u.Projects).Returns(_mockProjectRepository.Object);

            _handler = new DeleteProjectCommandHandle(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_SuccessfulDeletion_ReturnsSuccess()
        {
            var projectId = Guid.NewGuid();
            var command = new DeleteProjectCommand { Id = projectId };
            var project = new Project("Test Project", "PROJ001", "Description", DateTime.Now, DateTime.Now.AddDays(10), Guid.NewGuid(), false)
            {
                Id = projectId 
            };

            _mockProjectRepository.Setup(u => u.FindOne(It.Is<Expression<Func<Project, bool>>>(expr => expr.Compile().Invoke(new Project { Id = projectId }))))
                .Returns(project);

            _mockProjectRepository.Setup(u => u.Remove(It.Is<Project>(p => p.Id == projectId))).Verifiable();

            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .Returns(Task.FromResult(1))  
                .Verifiable();

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(string.Empty, result.ErrorMessage); 
            Assert.Null(result.Data); 

            _mockProjectRepository.Verify(u => u.Remove(It.Is<Project>(p => p.Id == projectId)), Times.Once);

            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}

using Capstone.Application.Module.Skills.Command;
using Capstone.Application.Module.Skills.CommandHandle;
using Capstone.Application.Module.Skills.Response;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using Moq;
using Xunit;

namespace Capstone.Test.Module.Skills.CommandHandle
{
    public class CreateSkillCommandHandlerTest
    {
        private readonly Mock<IRepository<Skill>> _skillRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly CreateSkillCommandHandler _handler;

        public CreateSkillCommandHandlerTest()
        {
            _skillRepositoryMock = new Mock<IRepository<Skill>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _handler = new CreateSkillCommandHandler(_skillRepositoryMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_CreatesSkillAndReturnsSkillDto()
        {
            // Arrange
            var command = new CreateSkillCommand
            {
                Title = "Programming",
                Description = "Skill related to coding"
            };

            _skillRepositoryMock.Setup(repo => repo.Add(It.IsAny<Skill>()));
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _skillRepositoryMock.Verify(repo => repo.Add(It.Is<Skill>(s =>
                s.Title == command.Title &&
                s.Description == command.Description &&
                !s.IsDeleted
            )), Times.Once);

            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            Assert.NotNull(result);
            Assert.Equal(command.Title, result.Title);
            Assert.Equal(command.Description, result.Description);
        }

        [Fact]
        public async Task Handle_EmptyTitle_ThrowsException()
        {
            // Arrange
            var command = new CreateSkillCommand
            {
                Title = string.Empty,
                Description = "Skill with no title"
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Skill title cannot be empty or null.", exception.Message);
        }
    }
}

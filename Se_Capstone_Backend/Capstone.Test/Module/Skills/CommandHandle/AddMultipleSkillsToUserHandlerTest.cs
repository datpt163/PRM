using Capstone.Application.Module.Skills.Command;
using Capstone.Application.Module.Skills.CommandHandle;
using Capstone.Application.Module.Skills.Response;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Capstone.Test.Module.Skills.CommandHandle
{
    public class AddMultipleSkillsToUserHandlerTest
    {
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly Mock<IRepository<Skill>> _skillRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly AddMultipleSkillsToUserHandler _handler;

        public AddMultipleSkillsToUserHandlerTest()
        {
            _userRepositoryMock = new Mock<IRepository<User>>();
            _skillRepositoryMock = new Mock<IRepository<Skill>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new AddMultipleSkillsToUserHandler(
                _userRepositoryMock.Object,
                _skillRepositoryMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Fact]
        public async Task Handle_ValidSkills_AddsSkillsToUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var skillId1 = Guid.NewGuid();
            var skillId2 = Guid.NewGuid();
            var skill1 = new Skill { Id = skillId1, Title = "C#" };
            var skill2 = new Skill { Id = skillId2, Title = "SQL" };

            var user = new User
            {
                Id = userId,
                Skills = new List<Skill>()
            };

            var users = new List<User> { user }.AsQueryable().BuildMock();
            var skills = new List<Skill> { skill1, skill2 }.AsQueryable().BuildMock();

            _userRepositoryMock.Setup(repo => repo.GetQuery()).Returns(users);
            _skillRepositoryMock.Setup(repo => repo.GetQueryNoTracking()).Returns(skills);

            var command = new AddMultipleSkillsToUserCommand
            {
                UserId = userId,
                SkillIds = new List<Guid> { skillId1, skillId2 }
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _userRepositoryMock.Verify(repo => repo.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            Assert.True(result.Success);
            Assert.DoesNotContain("not found", result.Message);
        }

        [Fact]
        public async Task Handle_SkillNotFound_ReturnsMessage()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var skillId1 = Guid.NewGuid();
            var skillId2 = Guid.NewGuid();
            var skill1 = new Skill { Id = skillId1, Title = "C#" };

            var user = new User
            {
                Id = userId,
                Skills = new List<Skill>()
            };

            var users = new List<User> { user }.AsQueryable().BuildMock();
            var skills = new List<Skill> { skill1 }.AsQueryable().BuildMock();

            _userRepositoryMock.Setup(repo => repo.GetQuery()).Returns(users);
            _skillRepositoryMock.Setup(repo => repo.GetQueryNoTracking()).Returns(skills);

            var command = new AddMultipleSkillsToUserCommand
            {
                UserId = userId,
                SkillIds = new List<Guid> { skillId1, skillId2 }
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _userRepositoryMock.Verify(repo => repo.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            Assert.True(result.Success);
            Assert.Contains("Skills not found", result.Message);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new AddMultipleSkillsToUserCommand
            {
                UserId = userId,
                SkillIds = new List<Guid> { Guid.NewGuid() }
            };

            var users = new List<User>().AsQueryable().BuildMock();

            _userRepositoryMock.Setup(repo => repo.GetQuery()).Returns(users);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}

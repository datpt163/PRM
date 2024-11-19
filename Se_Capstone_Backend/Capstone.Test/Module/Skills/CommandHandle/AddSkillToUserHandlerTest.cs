using Capstone.Application.Module.Skills.Command;
using Capstone.Application.Module.Skills.CommandHandle;
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
    public class AddSkillToUserHandlerTest
    {
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly Mock<IRepository<Skill>> _skillRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly AddSkillToUserHandler _handler;

        public AddSkillToUserHandlerTest()
        {
            _userRepositoryMock = new Mock<IRepository<User>>();
            _skillRepositoryMock = new Mock<IRepository<Skill>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new AddSkillToUserHandler(
                _userRepositoryMock.Object,
                _skillRepositoryMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Fact]
        public async Task Handle_UserNotFound_ReturnsFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new AddSkillToUserCommand { UserId = userId, SkillId = Guid.NewGuid() };
            var users = new List<User>().AsQueryable().BuildMock();
            _userRepositoryMock.Setup(repo => repo.GetQuery()).Returns(users);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Handle_SkillNotFound_ReturnsFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var skillId = Guid.NewGuid();
            var user = new User { Id = userId, Skills = new List<Skill>() };
            var users = new List<User> { user }.AsQueryable().BuildMock();
            var skills = new List<Skill>().AsQueryable().BuildMock();

            _userRepositoryMock.Setup(repo => repo.GetQuery()).Returns(users);
            _skillRepositoryMock.Setup(repo => repo.GetQueryNoTracking()).Returns(skills);

            var command = new AddSkillToUserCommand { UserId = userId, SkillId = skillId };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Handle_SkillAlreadyAssigned_ReturnsFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var skillId = Guid.NewGuid();
            var skill = new Skill { Id = skillId, Title = "C#" };
            var user = new User
            {
                Id = userId,
                Skills = new List<Skill> { skill }
            };

            var users = new List<User> { user }.AsQueryable().BuildMock();
            var skills = new List<Skill> { skill }.AsQueryable().BuildMock();

            _userRepositoryMock.Setup(repo => repo.GetQuery()).Returns(users);
            _skillRepositoryMock.Setup(repo => repo.GetQueryNoTracking()).Returns(skills);

            var command = new AddSkillToUserCommand { UserId = userId, SkillId = skillId };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Handle_ValidSkill_AssignsSkillAndReturnsTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var skillId = Guid.NewGuid();
            var skill = new Skill { Id = skillId, Title = "C#" };
            var user = new User
            {
                Id = userId,
                Skills = new List<Skill>()
            };

            var users = new List<User> { user }.AsQueryable().BuildMock();
            var skills = new List<Skill> { skill }.AsQueryable().BuildMock();

            _userRepositoryMock.Setup(repo => repo.GetQuery()).Returns(users);
            _skillRepositoryMock.Setup(repo => repo.GetQueryNoTracking()).Returns(skills);
            _userRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1); 


            var command = new AddSkillToUserCommand { UserId = userId, SkillId = skillId };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.True(result);
        }
    }
}

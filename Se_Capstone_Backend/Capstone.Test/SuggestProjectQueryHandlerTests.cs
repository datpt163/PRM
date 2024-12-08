using Capstone.Application.Common.Cohere;
using Capstone.Application.Common.Gpt;
using Capstone.Application.Common.HuggingFace;
using Capstone.Application.Module.Projects.Query;
using Capstone.Application.Module.Projects.QueryHandle;
using Capstone.Application.Module.Projects.Response;
using Capstone.Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Suggest
{
    public class SuggestProjectQueryHandlerTests
    {
        private readonly Mock<IChatGPTService> _chatGptServiceMock;
        private readonly Mock<IHuggingFaceService> _huggingFaceServiceMock;
        private readonly Mock<ICohereService> _cohereServiceMock;
        private readonly SuggestProjectQueryHandler _handler;

        public SuggestProjectQueryHandlerTests()
        {
            _chatGptServiceMock = new Mock<IChatGPTService>();
            _huggingFaceServiceMock = new Mock<IHuggingFaceService>();
            _cohereServiceMock = new Mock<ICohereService>();

            _handler = new SuggestProjectQueryHandler(_chatGptServiceMock.Object, _huggingFaceServiceMock.Object, _cohereServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuggestMappings_WhenCohereResponseIsValid()
        {
            var testUserStatistics = new List<UserStatistic>
            {
                new UserStatistic { Id = Guid.NewGuid(), FullName = "Alice" },
                new UserStatistic { Id = Guid.NewGuid(), FullName = "Bob" },
                new UserStatistic { Id = Guid.NewGuid(), FullName = "Charlie" }
            };

            var request = new SuggestProjectQuery
            {
                UserStatistics = testUserStatistics,TotalUsersNeed = 2
            };

            var expectedIds = new List<Guid> { testUserStatistics[0].Id, testUserStatistics[1].Id };
            var cohereResponseJson = JsonSerializer.Serialize(expectedIds);

            _cohereServiceMock.Setup(service => service.GetResponseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(cohereResponseJson);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.UserId == testUserStatistics[0].Id && x.Name == "Alice");
            Assert.Contains(result, x => x.UserId == testUserStatistics[1].Id && x.Name == "Bob");
        }

        [Fact]
        public async Task Handle_ShouldThrowArgumentException()
        {
            var request = new SuggestProjectQuery
            {
                UserStatistics = new List<UserStatistic>(),TotalUsersNeed = 0
            };

            _cohereServiceMock.Setup(service => service.GetResponseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync("Invalid response");

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _handler.Handle(request, CancellationToken.None));
        }
    }
}

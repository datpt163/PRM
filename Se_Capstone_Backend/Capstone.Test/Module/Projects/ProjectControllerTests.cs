using Capstone.Api.Common.ResponseApi.Model;
using Capstone.Api.Module.Projects.Controlers;
using Capstone.Api.Module.Projects.Request;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Common.Paging;
using Capstone.Application.Module.Projects.Command;
using Capstone.Application.Module.Projects.Query;
using Capstone.Application.Module.Projects.Response;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Capstone.Application.Module.Auths.Command;
using Capstone.Application.Module.Projects.Request;

namespace Capstone.Api.Module.Projects.Controllers.Tests
{
    public class ProjectControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly ProjectController _controller;

        public ProjectControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new ProjectController(_mediatorMock.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task CreateProject_Success_ReturnsOkResult()
        {
            // Arrange
            var command = new CreateProjectCommand();
            var response = new ResponseMediator("", new ProjectDTO());

            _mediatorMock.Setup(x => x.Send(It.IsAny<CreateProjectCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateProject(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<ResponseMediator>(okResult.Value);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(value.Data);
        }

        [Fact]
        public async Task CreateProject_NotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var command = new CreateProjectCommand();
            var response = new ResponseMediator("Not found", null, 404);

            _mediatorMock.Setup(x => x.Send(It.IsAny<CreateProjectCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateProject(command);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var value = Assert.IsType<ResponseMediator>(notFoundResult.Value);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Null(value.Data);
        }

        [Fact]
        public async Task UpdateProject_Success_ReturnsOkResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new UpdateProjectRequest();
            var response = new ResponseMediator("", new ProjectDTO());

            _mediatorMock.Setup(x => x.Send(It.IsAny<UpdateProjectCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateProject(id, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<ResponseSuccess<ProjectDTO>>(okResult.Value);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetListProject_Success_ReturnsOkResult()
        {
            // Arrange
            var projects = new List<ProjectDTO>();
            var paging = new PagingSP(0, 1, 10);
            var response = new PagingResultSP<ProjectDTO>(projects, paging.TotalCount, paging.PageIndex, paging.PageSize);

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetListProjectQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetListProject(1, 10, true, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<ResponseSuccess<List<ProjectDTO>>>(okResult.Value);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteProject_Success_ReturnsNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            var response = new ResponseMediator("", null);

            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteProjectCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.DeleteProject(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetProject_Success_ReturnsOkResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            var response = new ResponseMediator("", new ProjectDTO());

            _mediatorMock.Setup(x => x.Send(It.IsAny<GetDetailProjectQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetProject(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<ResponseSuccess<ProjectDTO>>(okResult.Value);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task ToggleVisible_Success_ReturnsOkResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            var response = new ResponseMediator("", new ProjectDTO());

            _mediatorMock.Setup(x => x.Send(It.IsAny<ToggleProjectCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.ToggleVisible(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<ResponseSuccess<ProjectDTO>>(okResult.Value);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task AddMember_Success_ReturnsNoContent()
        {
            // Arrange
            var request = new AddMembersToProject();
            var response = new ResponseMediator("", null);

            _mediatorMock.Setup(x => x.Send(It.IsAny<AddMembersToProject>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.AddMember(request);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Fact]
        public async Task UpdateMember_Success_ReturnsOkResult()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var request = new UpdateMemberRequest();
            var response = new ResponseMediator("", new ProjectDTO());

            _mediatorMock.Setup(x => x.Send(It.IsAny<UpdateMemberCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateMember(projectId, memberId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<ResponseSuccess<ProjectDTO>>(okResult.Value);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task CalculateEffort_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new ProjectEffortCalculationRequest();

            // Act
            var result = await _controller.CalculateEffort(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }
    }
}

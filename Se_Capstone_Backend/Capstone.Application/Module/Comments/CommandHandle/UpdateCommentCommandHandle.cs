    using AutoMapper;
using Capstone.Application.Common.Email.EmailQueue;
using Capstone.Application.Common.EmailHTML;
using Capstone.Application.Common.Jwt;
using Capstone.Application.Common.ProjectAuthorize;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Comments.Command;
using Capstone.Application.Module.Comments.CommentDTOs;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using CloudinaryDotNet.Core;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Comments.CommandHandle
{
    public class UpdateCommentCommandHandle : IRequestHandler<UpdateCommentCommand, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IPublishEndpoint _publisher;
        private readonly IManagePermissionProject _managePermissionProject;

        public UpdateCommentCommandHandle(IUnitOfWork unitOfWork, IJwtService jwtService, IMapper mapper, UserManager<User> userManager, IPublishEndpoint publishEndpoint, IManagePermissionProject managePermissionProject)
        {
            _managePermissionProject = managePermissionProject;
            _publisher = publishEndpoint;
            _userManager = userManager;
            _mapper = mapper;
            _jwtService = jwtService;
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseMediator> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = _unitOfWork.Comments.Find(x => x.Id == request.Id)
                                              .Include(c => c.Issue).ThenInclude(c => c.Reporter)
                                              .Include(c => c.Issue).ThenInclude(c => c.Assignee)
                                                .Include(c => c.Issue).ThenInclude(c => c.Status)
                                               .Include(c => c.Issue).ThenInclude(c => c.Comments).ThenInclude(c => c.User)
                                              .FirstOrDefault();
            if (comment == null)
                return new ResponseMediator("Comment not found", null, 404);
            var user = await _jwtService.VerifyTokenAsync(request.Token);
            if (user == null)
                return new ResponseMediator("User not found", null, 404);

            (bool isAuthorize, int status) = await _managePermissionProject.IsAuthorizedAsync(request.Token, "IsCommentConfigurator", commentId: request.Id);
            if (comment.UserId != user.Id && isAuthorize == false)
                return new ResponseMediator("", null, 403);

            var roles = await _userManager.GetRolesAsync(user);
            var role = _unitOfWork.Roles.Find(x => x.Name != null && x.Name == (roles.FirstOrDefault() == null ? "" : roles.FirstOrDefault())).Include(c => c.Permissions).FirstOrDefault();

            if (string.IsNullOrEmpty(request.Content))
                return new ResponseMediator("Content empty", null);
            var users = new List<User>();
            users.Add(comment.Issue.Reporter);
            if (comment.Issue.Assignee != null)
                users.Add(comment.Issue.Assignee);
            users.AddRange(comment.Issue.Comments.Select(x => x.User).ToList());
            users = users
                .GroupBy(u => u.Id)
                .Select(g => g.First())
                .ToList();
            users.RemoveAll(x => x.Id == user.Id);
            comment.Content = request.Content;
            comment.UpdatedAt = DateTime.Now;
            _unitOfWork.Comments.Update(comment);
            foreach (var u in users)
            {
                _unitOfWork.Notifications.Add(new Notification() { CreatedAt = DateTime.Now, UserId = u.Id, Type = "createComment", Data = JsonSerializer.Serialize(new { type = "updateComment", projectId = comment.Issue.Status.ProjectId, issueId = comment.Issue.Id, commentId = request.Id, issueName = comment.Issue.Title, issueIndex = comment.Issue.Index, issueStatusName = comment.Issue.Status.Name, commenterName = user.FullName, commenterUsername = user.UserName, commenterAvatar = user.Avatar }) });
                await _unitOfWork.SaveChangesAsync();
                await _publisher.Publish(new SendEmailMessage() { ToEmail = u.Email == null ? "" : u.Email, Body = EmailMessage.CreateComment(comment.Issue.Title, comment.Issue.Id + "", comment.Issue.Status.ProjectId + ""), Subject = $"[{comment.Issue.Title}]Comment on issue" });

            }

            await _unitOfWork.SaveChangesAsync();
            var response = _mapper.Map<CommentDTO>(comment);
            return new ResponseMediator(JsonSerializer.Serialize(users.Select(x => x.Id)), response, 200);

        }
    }
}

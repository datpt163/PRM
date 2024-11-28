using AutoMapper;
using Capstone.Application.Common.Email.EmailQueue;
using Capstone.Application.Common.EmailHTML;
using Capstone.Application.Common.Jwt;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Comments.Command;
using Capstone.Application.Module.Comments.CommentDTOs;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using MassTransit;
using MassTransit.RabbitMqTransport;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pipelines.Sockets.Unofficial.Arenas;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Comments.CommandHandle
{
    public class AddCommentCommandHandle : IRequestHandler<AddCommentCommand, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publisher;
        public AddCommentCommandHandle(IUnitOfWork unitOfWork, IJwtService jwtService, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _publisher = publishEndpoint;
            _mapper = mapper;
            _jwtService = jwtService;
            _unitOfWork = unitOfWork;
        }   

        public async Task<ResponseMediator> Handle(AddCommentCommand request, CancellationToken cancellationToken)
        {
            var responseSuccess = 200;
            var user = await _jwtService.VerifyTokenAsync(request.Token);
            if(user == null)
                return new ResponseMediator("User not found", null);

            if (_unitOfWork.Users.FindOne(x => x.Id == user.Id) == null)
                return new ResponseMediator("User not found", null);

            var issue = _unitOfWork.Issues.Find(x => x.Id == request.IssueId).Include(c => c.Comments).ThenInclude(c => c.User).Include(c => c.Status).ThenInclude(c => c.Project).Include(c => c.Reporter).Include(c => c.Assignee).FirstOrDefault();
            if(issue  == null)
                return new ResponseMediator("Issue not found", null);

            if (string.IsNullOrEmpty(request.Content))
                return new ResponseMediator("Content empty", null);

            var users = new List<User>();
            users.Add(issue.Reporter);
            if (issue.Assignee != null)
                users.Add(issue.Assignee);
            users.AddRange(issue.Comments.Select(x => x.User).ToList());
            users = users
                .GroupBy(u => u.Id)
                .Select(g => g.First())
                .ToList();
            users.RemoveAll(x => x.Id == user.Id);
            var comment = new Comment() { Content = request.Content, UserId = user.Id, IssueId = request.IssueId, CreatedAt = DateTime.Now };
            _unitOfWork.Comments.Add(comment);
            await _unitOfWork.SaveChangesAsync();
            foreach(var u in users)
            {
                responseSuccess = 205;
                _unitOfWork.Notifications.Add(new Notification() { CreatedAt = DateTime.Now, UserId = u.Id, Type = "createComment", Data = JsonSerializer.Serialize(new { type = "createComment", projectId  = issue.Status.ProjectId, issueId = issue.Id, commentId = comment.Id, issueName = issue.Title, issueIndex = issue.Index, issueStatusName = issue.Status.Name, commenterName = user.FullName, commenterUsername = user.UserName, commenterAvatar  = user.Avatar}) });
                await _unitOfWork.SaveChangesAsync();
                await _publisher.Publish(new SendEmailMessage() { ToEmail = u.Email == null ? "" : u.Email, Body = EmailMessage.CreateComment(issue.Title, issue.Id + "", issue.Status.ProjectId + ""), Subject = $"[{issue.Title}]Comment on issue" });

            }
            var response = _mapper.Map<CommentDTO>(comment);
            return new ResponseMediator(JsonSerializer.Serialize(users.Select(x => x.Id)), response, responseSuccess);

        }
    }
}

using Capstone.Application.Common.Jwt;
using Capstone.Application.Common.ProjectAuthorize;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Comments.Command;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Comments.CommandHandle
{
    public class DeleteCommentCommandHandle : IRequestHandler<DeleteCommentCommand, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly UserManager<User> _userManager;
        private readonly IManagePermissionProject _managePermissionProject;
        public DeleteCommentCommandHandle(IUnitOfWork unitOfWork, IJwtService jwtService, UserManager<User> userManager, IManagePermissionProject managePermissionProject)
        {
            _managePermissionProject = managePermissionProject;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
        }

        public async Task<ResponseMediator> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = _unitOfWork.Comments.FindOne(x => x.Id == request.Id);
            if (comment == null)
                return new ResponseMediator("Comment not found", null, 404);
            var user = await _jwtService.VerifyTokenAsync(request.Token);
            if (user == null)
                return new ResponseMediator("User not found", null, 404);
            (bool isAuthorize, int status) = await _managePermissionProject.IsAuthorizedAsync(request.Token, "IsCommentConfigurator", commentId: request.Id);
            if(comment.UserId != user.Id && isAuthorize == false )
                return new ResponseMediator("", null, 403);

            var roles = await _userManager.GetRolesAsync(user);
            var role = _unitOfWork.Roles.Find(x => x.Name != null && x.Name == (roles.FirstOrDefault() == null ? "" : roles.FirstOrDefault())).Include(c => c.Permissions).FirstOrDefault();

            _unitOfWork.Comments.Remove(comment);
            await _unitOfWork.SaveChangesAsync();
            return new ResponseMediator("", null);
        }
    }
}

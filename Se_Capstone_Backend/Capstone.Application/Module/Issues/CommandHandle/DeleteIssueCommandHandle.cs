using Capstone.Application.Common.Jwt;
using Capstone.Application.Common.ProjectAuthorize;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Issues.Command;
using Capstone.Application.Resources;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace Capstone.Application.Module.Issues.CommandHandle
{
    public class DeleteIssueCommandHandle : IRequestHandler<DeleteIssueCommand, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IManagePermissionProject _managePermissionProject;
        private readonly IJwtService _jwtService;
        public DeleteIssueCommandHandle(IUnitOfWork unitOfWork, IManagePermissionProject managePermissionProject, IJwtService jwtService)
        {
            _jwtService = jwtService;
            _managePermissionProject = managePermissionProject;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseMediator> Handle(DeleteIssueCommand request, CancellationToken cancellationToken)
        {
            var issue = _unitOfWork.Issues.Find(x => x.Id == request.Id).Include(c => c.Status).ThenInclude(d => d.Issues).FirstOrDefault();
            if (issue == null)
                return new ResponseMediator(Messages.parent_issue_not_found, null, 404);

            var user = await _jwtService.VerifyTokenAsync(request.Token);
            if (user == null)
                return new ResponseMediator("", null, 403);

            (bool isAuthorized, int statusCode) = await _managePermissionProject.IsAuthorizedAsync(request.Token, "IsIssueConfigurator", issueId: request.Id);
            if (issue.ReporterId != user.Id && !isAuthorized)
                return new ResponseMediator("", null, 403);

            foreach (var iss in issue.Status.Issues)
                if (iss.Position > issue.Position)
                    iss.Position--;

            _unitOfWork.Issues.Update(issue);
            _unitOfWork.Issues.Remove(issue);
            await _unitOfWork.SaveChangesAsync();
            return new ResponseMediator("", null);
        }
    }
}

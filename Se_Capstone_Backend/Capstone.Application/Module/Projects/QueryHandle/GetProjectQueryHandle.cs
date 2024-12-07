using AutoMapper;
using Capstone.Application.Common.Jwt;
using Capstone.Application.Common.Paging;
using Capstone.Application.Module.Projects.Query;
using Capstone.Application.Module.Projects.Response;
using Capstone.Domain.Entities;
using Capstone.Domain.Enums;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Application.Module.Projects.QueryHandle
{
    public class GetProjectQueryHandle : IRequestHandler<GetListProjectQuery, PagingResultSP<ProjectDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;
        private readonly UserManager<User> _userManager;

        public GetProjectQueryHandle(IUnitOfWork unitOfWork, IMapper mapper, IJwtService jwtService, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _jwtService = jwtService;
            _userManager = userManager;
        }

        public async Task<PagingResultSP<ProjectDTO>> Handle(GetListProjectQuery request, CancellationToken cancellationToken)
        {
            if (request.Status.HasValue)
                if (!(request.Status == ProjectStatus.NotStarted || request.Status == ProjectStatus.InProgress || request.Status == ProjectStatus.Finished || request.Status == ProjectStatus.Canceled))
                    return new PagingResultSP<ProjectDTO>() { ErrorMessage = "Status must more than 0 or less than 5" };

            var projectsQuery = new List<Project>();

            var user = await _jwtService.VerifyTokenAsync(request.Token);
            if (user != null)
            {     
                var roles = await _userManager.GetRolesAsync(user);
                var role = _unitOfWork.Roles.Find(x => x.Name != null && x.Name == (roles.FirstOrDefault() == null ? "" : roles.FirstOrDefault())).Include(c => c.Permissions).FirstOrDefault();

                if (role != null && role.Name != null && role.Permissions.Select(x => x.Name).Contains("READ_ALL_PROJECTS"))
                {
                    projectsQuery = await _unitOfWork.Projects.GetQuery().Include(c => c.Lead).ToListAsync();
                }
                else
                {
                    if (user != null)
                    {
                        projectsQuery = user.UserProjects.Select(x => x.Project).Where(x => x.IsVisible).ToList();
                        projectsQuery.AddRange(user.LeadProjects.ToList().Where(x => x.IsVisible));
                    }
                }
            }

            var ListProject = _mapper.Map<List<ProjectDTO>>(projectsQuery);

            if (request.IsVisible.HasValue)
                ListProject = ListProject.Where(x => x.IsVisible == request.IsVisible).ToList();
            if (request.Status.HasValue)
                ListProject = ListProject.Where(x => x.Status == request.Status).ToList();
            if (request.StartDate != null)
            {
                ListProject = ListProject.Where(x => x.StartDate != null && x.StartDate.Value.Date >= request.StartDate.Value.Date).ToList();
            }

            if (request.EndDate != null)
            {
                ListProject = ListProject.Where(x => x.EndDate != null && x.EndDate.Value.Date <= request.EndDate.Value.Date).ToList();
            }

            if (!string.IsNullOrEmpty(request.Search))
            {
                ListProject = ListProject.Where(x => (x.Name.Trim().ToUpper().Contains(request.Search.Trim().ToUpper()) || x.Code.Trim().ToUpper().Contains(request.Search.Trim().ToUpper()))).ToList();
            }

            if (request.PageIndex.HasValue && request.PageSize.HasValue)
            {
                if (request.PageIndex.Value < 1 || request.PageSize.Value < 0)
                    return new PagingResultSP<ProjectDTO>() { ErrorMessage = "PageIndex, PageSize must >= 0" };

                int skip = (request.PageIndex.Value - 1) * request.PageSize.Value;
                var projectPaing = ListProject.OrderByDescending(c => c.CreatedAt).Skip(skip).Take(request.PageSize.Value).ToList();
                var totalCount = ListProject.Count();
                var result = new PagingResultSP<ProjectDTO>(projectPaing, totalCount, request.PageIndex.Value, request.PageSize.Value);
                return result;
            }
            return new PagingResultSP<ProjectDTO>() { Data = ListProject.OrderByDescending(c => c.CreatedAt).ToList() };
        }
    }
}

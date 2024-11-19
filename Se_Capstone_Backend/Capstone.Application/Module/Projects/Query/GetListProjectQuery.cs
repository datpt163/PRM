using Capstone.Application.Common.Paging;
using Capstone.Application.Module.Projects.Response;
using Capstone.Domain.Enums;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Capstone.Application.Module.Projects.Query
{
    public class GetListProjectQuery : IRequest<PagingResultSP<ProjectDTO>>
    {
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
        public bool? IsVisible {  get; set; }
        public ProjectStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Search { get; set; } 

        public string Token { get; set; } = string.Empty;

        public GetListProjectQuery(int? pageIndex, int? pageSize, bool? isVisible, ProjectStatus? status, string token, string? search)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            IsVisible = isVisible;
            Status = status;
            Token = token;
            Search = search;    
        }
    }
}

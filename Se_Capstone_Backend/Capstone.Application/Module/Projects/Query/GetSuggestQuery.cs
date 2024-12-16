using Capstone.Application.Module.Projects.Response;
using MediatR;

namespace Capstone.Application.Module.Projects.Query
{
    public class GetSuggestQuery : IRequest<SuggestionResult>
    {
        public string SearchTerm { get; set; } = string.Empty;
    }
}

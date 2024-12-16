using Capstone.Application.Common.Cohere;
using Capstone.Application.Common.Gpt;
using Capstone.Application.Common.HuggingFace;
using Capstone.Application.Module.Projects.Query;
using Capstone.Application.Module.Projects.Response;
using Capstone.Application.Resources;
using MediatR;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Projects.QueryHandle
{
    public class GetSuggestQueryHandler : IRequestHandler<GetSuggestQuery, SuggestionResult>
    {
        private readonly IChatGPTService _chatGptService;
        private readonly IHuggingFaceService _huggingFaceService;
        private readonly ICohereService _cohereService;

        public GetSuggestQueryHandler(IChatGPTService chatGptService, IHuggingFaceService huggingFaceService, ICohereService cohereService)
        {
            _chatGptService = chatGptService;
            _huggingFaceService = huggingFaceService;
            _cohereService = cohereService;
        }

        public async Task<SuggestionResult> Handle(GetSuggestQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = new SuggestionResult();

                string systemMessage = Messages.analysis_project;

                var response = await _chatGptService.GetChatGptResponseAsync(request.SearchTerm, systemMessage, 4096);

                // var response = await _huggingFaceService.GetResponseAsync(requestJson, systemMessage, 4096);
                // var response = await _cohereService.GetResponseAsync(requestJson, systemMessage, 4096);

                if (!string.IsNullOrEmpty(response))
                {
                    result.Description = response.Replace("\n", "<br>");
                }

                return result;
            }
            catch (Exception e)
            {
                return new SuggestionResult
                {
                    Description = $"An error occurred while processing your request: {e.Message}",
                };
            }
        }
    }
}

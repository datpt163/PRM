using Capstone.Application.Common.Cohere;
using Capstone.Application.Common.Gpt;
using Capstone.Application.Common.HuggingFace;
using Capstone.Application.Module.Projects.Query;
using Capstone.Application.Module.Projects.Response;
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

                string systemMessage = "You are an expert in project management, specializing in analyzing various project data and providing actionable insights and suggestions for overall project improvement. " +
                "Given the input data in 'SearchTerm', analyze the project's current state and identify key areas for improvement and optimization. " +
                "Your response should be concise, actionable, and organized in a numbered or bullet-point list. Focus on practical recommendations in the following areas: \n" +
                "- Task management and tracking (e.g., task completion rates, backlog management)\n" +
                "- Effort estimation and allocation (e.g., estimating resources, tracking effort)\n" +
                "- Project performance monitoring (e.g., completion rates, overall performance metrics)\n" +
                "- Resource management and allocation (e.g., team capacity, task assignment)\n" +
                "- Communication and collaboration (e.g., team interactions, updates)\n" +
                "- Risk management (e.g., identifying potential risks, mitigating issues)\n" +
                "Ensure that your advice is directly applicable to improving the project's processes, team efficiency, and overall performance. Provide solutions that are easy to implement and can immediately improve project outcomes. Avoid generic advice and make sure to tailor the recommendations to the specific data provided in 'SearchTerm'.";


                var response = await _chatGptService.GetChatGptResponseAsync(request.SearchTerm, systemMessage, 4096);

                // var response = await _huggingFaceService.GetResponseAsync(requestJson, systemMessage, 4096);
                // var response = await _cohereService.GetResponseAsync(requestJson, systemMessage, 4096);

                if (!string.IsNullOrEmpty(response))
                {
                    result.Description = response; 
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

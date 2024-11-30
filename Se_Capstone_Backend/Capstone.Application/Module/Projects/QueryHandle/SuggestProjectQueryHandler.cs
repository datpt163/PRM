using Capstone.Application.Common.Cohere;
using Capstone.Application.Common.Gpt;
using Capstone.Application.Common.HuggingFace;
using Capstone.Application.Module.Projects.Query;
using Capstone.Application.Module.Projects.Response;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Projects.QueryHandle
{
    public class SuggestProjectQueryHandler : IRequestHandler<SuggestProjectQuery, List<SuggestMapping>>
    {
        private readonly IChatGPTService _chatGptService;
        private readonly IHuggingFaceService _huggingFaceService;
        private readonly ICohereService _cohereService;
        public SuggestProjectQueryHandler(IChatGPTService chatGptService, IHuggingFaceService huggingFaceService, ICohereService cohereService)
        {
            _chatGptService = chatGptService;
            _huggingFaceService = huggingFaceService;
            _cohereService = cohereService;

        }

        public async Task<List<SuggestMapping>> Handle(SuggestProjectQuery request, CancellationToken cancellationToken)
        {
            try
            {
                string systemMessage = "You are a helpful assistant that provides potential employee suggestions based on project requirements. " +
                "Please just return the result of top 3 suggest UID UserStatistics as a JSON array of GUIDs example [\"\", \"\", \"\"]. You need to return just result, don't add anything superfluous";
                var maxTokens = 4096;
                var requestJson = JsonSerializer.Serialize(request);
                //var response = await _chatGptService.GetChatGptResponseAsync(requestJson, systemMessage, maxTokens);
                //var response = await _huggingFaceService.GetResponseAsync(requestJson, systemMessage, maxTokens);
                var response = await _cohereService.GetResponseAsync(requestJson, systemMessage, maxTokens);

                var potentialEmployeeIds = ParseGptResponse(response);

                var suggestMappings = request.UserStatistics
                      .Where(us => potentialEmployeeIds.Contains(us.Id))
                      .Select(us => new SuggestMapping
                      {
                          UserId = us.Id,
                          Name = us.FullName ?? string.Empty,
                      })
                      .ToList();

                return suggestMappings;
            }catch(Exception e)
            {
                await Console.Out.WriteLineAsync(e.Message);
                var suggestMappings = request.UserStatistics.OrderBy(x=> x.ActiveProjectCount)
                        .Take(3)
                      .Select(us => new SuggestMapping
                      {
                          UserId = us.Id,
                          Name = us.FullName ?? string.Empty,
                      })
                      .ToList();

                return suggestMappings;
            }
            
        }
        private List<Guid> ParseGptResponse(string gptResponse)
        {
            if (!gptResponse.Contains('[') || !gptResponse.Contains(']'))
            {
                throw new ArgumentException("The response does not contain valid JSON array brackets.");
            }

            int startIndex = gptResponse.IndexOf('[');
            int endIndex = gptResponse.LastIndexOf(']');

            if (startIndex < 0 || endIndex < 0 || endIndex < startIndex)
            {
                throw new ArgumentException("Invalid format: Could not find a valid JSON array.");
            }

            string jsonArrayString = gptResponse.Substring(startIndex, endIndex - startIndex + 1);

            return JsonSerializer.Deserialize<List<Guid>>(jsonArrayString) ?? new List<Guid>();
        }


    }
}

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
            if (request.TotalUsersNeed < 1)
            {
                throw new ArgumentException("Total Users need to be at least 1.");
            }

            try
            {   
                string systemMessage = "You are a specialized assistant capable of analyzing project requirements and employee data. " +
                       $"Your role is to carefully evaluate the 'ProjectDetail' and 'Skill' field in the provided data and select exactly {request.TotalUsersNeed} unique user IDs (UIDs) " +
                       "that best align with the described requirements. " +
                       "The selection criteria are based on relevance and fit to the ProjectDetail field. " +
                       "The output must be a valid JSON array of exactly 3 GUID strings, strictly formatted as [\"GUID1\", \"GUID2\", \"GUID3\"]. " +
                       "Do not include any comments, explanations, or additional text in your response. Return only the JSON array. " +
                       "If there is insufficient data to select 3 UIDs, return an empty JSON array: [].";

                var maxTokens = 4096;
                var requestJson = JsonSerializer.Serialize(request);
                var response = await _chatGptService.GetChatGptResponseAsync(requestJson, systemMessage, maxTokens);
                //var response = await _huggingFaceService.GetResponseAsync(requestJson, systemMessage, maxTokens);
                //var response = await _cohereService.GetResponseAsync(requestJson, systemMessage, maxTokens);

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

            }
            catch (Exception e)
            {
                await Console.Out.WriteLineAsync(e.Message);
                var req = RemoveCommonWords(request.ProjectDetail);

                var keywords = req.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(k => k.Trim())
                                  .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var matchedUsers = request.UserStatistics
                    .Select(us => new
                    {
                        User = us,
                        MatchCount = us.Skills.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                              .Count(skill => keywords.Contains(skill.Trim()))
                    })
                    .OrderByDescending(x => x.MatchCount)
                    .ThenBy(x => x.User.ActiveProjectCount)
                    .Take(request.TotalUsersNeed)
                    .Select(x => new SuggestMapping
                    {
                        UserId = x.User.Id,
                        Name = x.User.FullName ?? string.Empty,
                    })
                    .ToList();

                if (!matchedUsers.Any())
                {
                    matchedUsers = request.UserStatistics
                        .OrderByDescending(us => us.ActiveProjectCount)
                        .Take(request.TotalUsersNeed)
                        .Select(us => new SuggestMapping
                        {
                            UserId = us.Id,
                            Name = us.FullName ?? string.Empty,
                        })
                        .ToList();
                }

                return matchedUsers;

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

        private string RemoveCommonWords(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            var stopWords = new List<string>
            {
                "i", "me", "my", "myself", "we", "our", "ours", "ourselves", "you", "your", "yours", "yourself", "yourselves",
                "he", "him", "his", "himself", "she", "her", "hers", "herself", "it", "its", "itself", "they", "them", "their", "theirs", "themselves",
                "what", "which", "who", "whom", "this", "that", "these", "those", "am", "is", "are", "was", "were", "be", "been", "being", "have", "has", "had",
                "having", "do", "does", "did", "doing", "a", "an", "the", "and", "but", "if", "or", "because", "as", "until", "while", "of", "at", "by", "for",
                "with", "about", "against", "between", "into", "through", "during", "before", "after", "above", "below", "to", "from", "up", "down", "in",
                "out", "on", "off", "over", "under", "again", "further", "then", "once", "here", "there", "when", "where", "why", "how", "all", "any", "both",
                "each", "few", "more", "most", "other", "some", "such", "no", "nor", "not", "only", "own", "same", "so", "than", "too", "very", "s", "t", "can",
                "will", "just", "don", "should", "now"
            };

            var words = input.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            var filteredWords = words.Where(word => !stopWords.Contains(word, StringComparer.OrdinalIgnoreCase));

            return string.Join(" ", filteredWords);
        }

    }
}

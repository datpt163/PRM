using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Capstone.Application.Common.Gpt
{
    public class ChatGPTService : IChatGPTService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;

        public ChatGPTService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = !string.IsNullOrEmpty(configuration["OpenAI:ApiKey"]) ? configuration["OpenAI:ApiKey"] : Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<string> GetChatGptResponseAsync(string prompt, string systemMessage = "You are a helpful assistant.", int maxTokens = 4096)
        {
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
            new { role = "system", content = systemMessage },
            new { role = "user", content = prompt }
        },
                max_tokens = maxTokens
            };

            var requestJson = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error: {response.StatusCode}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            var chatGptResponse = JsonConvert.DeserializeObject<ChatGptResponse>(responseJson);

            if (chatGptResponse?.Choices != null && chatGptResponse.Choices.Length > 0)
            {
                return chatGptResponse.Choices[0].Message.Content.ToString();
            }

            return "No response from GPT";
        }
    }
}

namespace Capstone.Application.Common.Gpt
{
    public class ChatGptResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Object { get; set; } = string.Empty;
        public int Created { get; set; }
        public string Model { get; set; } = string.Empty;
        public UsageInfo Usage { get; set; } = new UsageInfo();
        public Choice[] Choices { get; set; } = new Choice[0];

        public class UsageInfo
        {
            public int PromptTokens { get; set; }
            public int CompletionTokens { get; set; }
            public int TotalTokens { get; set; }
        }

        public class Choice
        {
            public ChoiceMessage Message { get; set; } = new ChoiceMessage();
            public string FinishReason { get; set; } = string.Empty;
            public int Index { get; set; }

            public class ChoiceMessage
            {
                public string Role { get; set; } = string.Empty;
                public string Content { get; set; } = string.Empty;
            }
        }
    }
}

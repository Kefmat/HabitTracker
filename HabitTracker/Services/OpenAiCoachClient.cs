using OpenAI.Chat;

namespace HabitTracker.Services;

/// Ekte AI via OpenAI sin offisielle .NET SDK.
public class OpenAiCoachClient : ICoachClient
{
    private readonly ChatClient _chat;

    public bool IsAiEnabled => true;

    public OpenAiCoachClient(ChatClient chatClient)
    {
        _chat = chatClient;
    }

    public async Task<string> AskAsync(string systemPrompt, string userPrompt, CancellationToken ct = default)
    {
        // Vi bruker ChatClient fra OpenAI .NET SDK.
        // Her sender vi system + user som en enkel samtale.
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        ChatCompletion completion = await _chat.CompleteChatAsync(messages, cancellationToken: ct);

        // Tar første tekst-svar
        return completion.Content.FirstOrDefault()?.Text?.Trim() ?? "";
    }
}

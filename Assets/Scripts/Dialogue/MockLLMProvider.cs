using System.Threading.Tasks;

public class MockLLMProvider : ILLMProvider
{
    public Task<string> GenerateTextAsync(string prompt)
    {
        string mockReply = "Мне бы чего-нибудь... Даже не знаю...";
        return Task.FromResult(mockReply);
    }
}
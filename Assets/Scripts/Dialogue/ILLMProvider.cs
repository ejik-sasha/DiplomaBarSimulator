using System.Threading.Tasks;

public interface ILLMProvider
{
    Task<string> GenerateTextAsync(string prompt);
}
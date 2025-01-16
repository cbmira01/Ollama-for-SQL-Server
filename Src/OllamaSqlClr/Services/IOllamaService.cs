using System.Collections;
using System.Data.SqlTypes;

public interface IOllamaService
{
    string CompletePrompt(string modelName, string askPrompt, string morePrompt);

    IEnumerable CompleteMultiplePrompts(string modelName, string askPrompt, string morePrompt, int numCompletions);

    IEnumerable GetAvailableModels();

    IEnumerable QueryFromPrompt(string modelName, string prompt);

    string ExamineImage(string modelName, string askPrompt, byte[] imageData);

}

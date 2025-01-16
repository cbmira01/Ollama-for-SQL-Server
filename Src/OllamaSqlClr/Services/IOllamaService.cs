using System.Collections;
using System.Data.SqlTypes;

public interface IOllamaService
{
    string CompletePrompt(string modelName, string askPrompt, string morePrompt);

    IEnumerable CompleteMultiplePrompts(SqlString modelName, SqlString askPrompt, SqlString morePrompt, SqlInt32 numCompletions);

    IEnumerable GetAvailableModels();

    IEnumerable QueryFromPrompt(SqlString modelName, SqlString prompt);

    SqlString ExamineImage(SqlString modelName, SqlString askPrompt, SqlBytes imageData);

}

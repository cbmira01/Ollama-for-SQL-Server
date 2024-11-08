using System.Collections;
using System.Data.SqlTypes;

public interface IOllamaService
{
    SqlString CompletePrompt(SqlString modelName, SqlString askPrompt, SqlString morePrompt);
    IEnumerable CompleteMultiplePrompts(SqlString modelName, SqlString askPrompt, SqlString morePrompt, SqlInt32 numCompletions);
    IEnumerable GetAvailableModels();
    //SqlString QueryFromPrompt(SqlString modelName, SqlString askPrompt);
}

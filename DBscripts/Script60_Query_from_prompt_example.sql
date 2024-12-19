
/**
    This script contains tests of the 'QueryFromPrompt' feature, in progress.

    Make sure your local Ollama API server is running.
**/

----------------------------------------------------
--
----------------------------------------------------
DECLARE @modelName NVARCHAR(MAX) = 'llama3.2';
DECLARE @prompt NVARCHAR(MAX) = 'Find all entries in support_emails where sentiment is glad.';

SELECT * FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO


----------------------------------------------------
--
----------------------------------------------------

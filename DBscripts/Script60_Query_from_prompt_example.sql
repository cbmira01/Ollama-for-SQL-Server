

-- Testing 'QueryFromPrompt' feature

DECLARE @modelName NVARCHAR(MAX) = 'llama3.2';
DECLARE @prompt NVARCHAR(MAX) = 'Find all entries in support_emails where sentiment is glad.';

SELECT * FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

SELECT * FROM support_emails WHERE sentiment = 'glad'




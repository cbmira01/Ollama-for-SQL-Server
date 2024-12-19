/**
    This script will test completions of sentiment analysis from support emails.

    Use Script20 to populate the demonstration data.

    Make sure your local Ollama API server is running.
**/

Use [TEST];
GO

DECLARE @modelName NVARCHAR(MAX) = 'llama3.2';
DECLARE @prompt NVARCHAR(MAX) = 'Provide a single word sentiment label:';
SELECT 
    id,
    email_content,
	sentiment,
    dbo.CompletePrompt(@modelName, @prompt, email_content) AS sentiment_analysis
FROM 
    support_emails;
GO

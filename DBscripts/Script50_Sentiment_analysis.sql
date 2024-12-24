﻿/**
    This script will test completions of sentiment and keyword analysis from support emails.

    Use Script20 to populate the support_emails table.

    The email_content and initial sentiment field was originally generated by ChatGPT.

    Make sure your local Ollama API server is running.
**/

Use [TEST];
GO

--------------------------------------------------------------------
-- Sentiment analysis
--------------------------------------------------------------------
DECLARE @modelName NVARCHAR(MAX) = 'llama3.2';
DECLARE @prompt NVARCHAR(MAX) = '
You are providing a sentiment label for a support email. 
Respond with ONLY one word from this list: glad, sad, angry, neutral.
Do not include punctuation or explanation. 
';

SELECT 
    id,
    email_content,
	sentiment,
    dbo.CompletePrompt(@modelName, @prompt, email_content) AS sentiment_analysis
FROM 
    support_emails;
GO


--------------------------------------------------------------------
-- Keyword discovery
--------------------------------------------------------------------
DECLARE @modelName NVARCHAR(MAX) = 'llama3.2';
DECLARE @prompt NVARCHAR(MAX) = '
You are an expert in extracting keywords from support emails.
Respond only with a comma-separated list of keywords you discover in this email.
Do not include any further explanation. 
';

SELECT 
    id,
    email_content,
    dbo.CompletePrompt(@modelName, @prompt, email_content) AS keywords
FROM 
    support_emails;
GO


--------------------------------------------------------------------
-- Email routing
--------------------------------------------------------------------
DECLARE @modelName NVARCHAR(MAX) = 'llama3.2';
DECLARE @prompt NVARCHAR(MAX) = '
You are an expert in routing customer support emails
Route this email to either "factory", "sales", "warranty", or "marketing".
Provide one-word answers without any further explanation. 
';

SELECT 
    id,
    email_content,
    dbo.CompletePrompt(@modelName, @prompt, email_content) AS keywords
FROM 
    support_emails;
GO



USE [Test];
GO


CREATE TABLE QueryPromptLog (
    LogID INT IDENTITY(1,1) PRIMARY KEY,
    Prompt NVARCHAR(MAX) NOT NULL,
    GeneratedQuery NVARCHAR(MAX) NOT NULL,
    Timestamp DATETIME NOT NULL DEFAULT(getdate())
);

DECLARE @modelName NVARCHAR(MAX) = 'llama3.2';
DECLARE @prompt NVARCHAR(MAX) = 'Find all entries in support_emails where sentiment is glad.';

SELECT dbo.QueryFromPrompt(@modelName, @prompt);
GO


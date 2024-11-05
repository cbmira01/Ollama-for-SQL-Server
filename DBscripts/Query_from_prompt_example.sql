

USE [Test];
GO


CREATE TABLE QueryPromptLog (
    LogID INT IDENTITY(1,1) PRIMARY KEY,
    Prompt NVARCHAR(MAX) NOT NULL,
    ProposedQuery NVARCHAR(MAX) NOT NULL,
    ErrorNumber NVARCHAR(10) NULL,
    ErrorMessage NVARCHAR(100) NULL,
    ErrorLine NVARCHAR(10) NULL,

    Timestamp DATETIME NOT NULL DEFAULT(getdate())
);
GO


DECLARE @modelName NVARCHAR(MAX) = 'llama3.2';
DECLARE @prompt NVARCHAR(MAX) = 'Find all entries in support_emails where sentiment is glad.';

SELECT dbo.QueryFromPrompt(@modelName, @prompt);
GO



/**
    This script contains tests of the 'QueryFromPrompt' feature, in progress.

	This function is most successful when using the Mistral model.

    Make sure your local Ollama API server is running.
**/

USE [TEST];

---------------------------------------------------------------------------------
-- This query returns support email rows with glad sentiment
---------------------------------------------------------------------------------
DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = '
Find all entries in support_emails where sentiment is glad.
';

SELECT * FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

---------------------------------------------------------------------------------
-- Query to exhibit foreign keys, conditionals, and aggregate
---------------------------------------------------------------------------------
DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = '
How much has John Doe spent in total?
';

SELECT * FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

---------------------------------------------------------------------------------
-- Query to find existence of a customer
---------------------------------------------------------------------------------
DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = '
Is there a customer by the name John Doe?
';

SELECT * FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

---------------------------------------------------------------------------------
-- Query to count the number of sales
---------------------------------------------------------------------------------
DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = '
How many times has John Doe purchased anything?
';

SELECT * FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

---------------------------------------------------------------------------------
-- Query to test string searching
---------------------------------------------------------------------------------
DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = '
What customers have first names that start with "J"?';

SELECT * FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO


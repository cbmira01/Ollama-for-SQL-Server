
/**
    This script contains tests of the 'QueryFromPrompt' feature.

	This function is most successful when the Mistral model is used.

    Make sure your local Ollama API server is running.
**/

USE [TEST];

---------------------------------------------------------------------------------
-- This query returns support email rows with glad sentiment
---------------------------------------------------------------------------------
DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = 'Find all entries in support_emails where sentiment is glad.';

SELECT * FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

---------------------------------------------------------------------------------
-- Query to exhibit foreign keys, conditionals, and aggregate
---------------------------------------------------------------------------------
DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = 'How much has John Doe spent in total?';

SELECT * FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

---------------------------------------------------------------------------------
-- Query to find existence of a customer
---------------------------------------------------------------------------------
DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = 'Tell me yes or no, is there a customer by the name of John Doe?';

SELECT * FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

---------------------------------------------------------------------------------
-- Query to find existence of a customer
---------------------------------------------------------------------------------
DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = 'Tell me yes or no, is there a customer by the name of Jason Argonaut?';

SELECT * FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

---------------------------------------------------------------------------------
-- Query to count the number of sales
---------------------------------------------------------------------------------
DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = 'How many times has John Doe purchased anything?';

SELECT * FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

---------------------------------------------------------------------------------
-- Query to test string searching
---------------------------------------------------------------------------------
DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = 'What customers have first names that start with the letter J?';

SELECT * FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

---------------------------------------------------------------------------------
-- Query to produce sorted list
---------------------------------------------------------------------------------
DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = 'Give me a list of items sold, sorted in reverse alphabetical order by the name of the item.';

SELECT * FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

---------------------------------------------------------------------------------
-- Query to test date/time handling
---------------------------------------------------------------------------------
DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = 'What was the date and time of the earliest purchase?';

SELECT * FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

---------------------------------------------------------------------------------
-- Query to test date/time handling
---------------------------------------------------------------------------------
DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = 'What was the time of day of the latest purchase?';

SELECT * FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

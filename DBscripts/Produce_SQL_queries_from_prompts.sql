
/*************************************************************************************************

    This script contains tests of the 'QueryFromPrompt' feature, to demonstrate production
        of SQL queries from natural-language prompts.

    Behind the scenes, the LLM is provided with a database schema, along with heavy prompting 
        to ensure the best SQL production.

    Of the LLMs I've tested, the Mistral model appears to perform the best at this coding task.

    Make sure your local Ollama API server is running.

*************************************************************************************************/


---------------------------------------------------------------------------------
-- This query returns support email rows with glad sentiment
---------------------------------------------------------------------------------
USE [AI_Lab];

DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = 'Find all entries in support_emails where sentiment is glad.';

SELECT [ProposedQuery], [Result] FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

---------------------------------------------------------------------------------
-- Query to exhibit foreign keys, conditionals, and aggregate
---------------------------------------------------------------------------------
USE [AI_Lab];

DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = 'How much has John Doe spent in total?';

SELECT [ProposedQuery], [Result] FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

---------------------------------------------------------------------------------
-- Query to find existence of a customer
---------------------------------------------------------------------------------
USE [AI_Lab];

DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = 'Is there a customer with last name of Doe? Answer yes or no.';

SELECT [ProposedQuery], [Result] FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

---------------------------------------------------------------------------------
-- Query to find existence of a customer
---------------------------------------------------------------------------------
USE [AI_Lab];

DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = 'Tell me yes or no, is there a customer by the name of Jason Argonaut?';

SELECT [ProposedQuery], [Result] FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

---------------------------------------------------------------------------------
-- Query to count the number of sales
---------------------------------------------------------------------------------
USE [AI_Lab];

DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = 'How many times has John Doe purchased anything?';

SELECT [ProposedQuery], [Result] FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

---------------------------------------------------------------------------------
-- Query to test string searching
---------------------------------------------------------------------------------
USE [AI_Lab];

DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = 'What customers have first names that start with the letter J?';

SELECT [ProposedQuery], [Result] FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

---------------------------------------------------------------------------------
-- Query to produce sorted list
---------------------------------------------------------------------------------
USE [AI_Lab];

DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = 'Give me a list of items sold, sorted in reverse alphabetical order by the name of the item.';

SELECT [ProposedQuery], [Result] FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

---------------------------------------------------------------------------------
-- Query to test date/time handling
---------------------------------------------------------------------------------
USE [AI_Lab];

DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = 'What was the date and time of the earliest purchase?';

SELECT [ProposedQuery], [Result] FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

---------------------------------------------------------------------------------
-- Query to test date/time handling
---------------------------------------------------------------------------------
USE [AI_Lab];

DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = 'What was the time of day of the latest purchase?';

SELECT [ProposedQuery], [Result] FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

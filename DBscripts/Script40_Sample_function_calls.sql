

/*************************************************************************************************

    Demonstrations of LLM completions in SQL Server.

    Make sure your local Ollama API server is running.

*************************************************************************************************/


-----------------------------------------------------------
-- Call 'CompletePrompt' to get completions as projections
-----------------------------------------------------------
USE [TEST];

DECLARE @modelName NVARCHAR(MAX) = 'llama3.2';
DECLARE @ask NVARCHAR(MAX) = 'Replying only on the words ''happy'' or ''not happy'', describe this sentiment: ';

SELECT     
    dbo.CompletePrompt(@modelName, @ask, Sentence) AS Response,
	Sentence
FROM 
    (VALUES 
        ('The sun rises on a new day.'),
        ('Tears fall in the quiet night.'),
        ('A warm embrace after a long journey.'),
        ('The laughter faded too soon.'),
        ('Dancing in the rain with no worries.'),
        ('An empty chair at the dinner table.'),
        ('A smile shared across the room.'),
        ('Memories linger like a heavy cloud.'),
        ('Joy fills the air with every word.'),
        ('The farewell came too quickly.')
    ) AS Sentences(Sentence);
GO

------------------------------------------------------------------
-- Call 'CompleteMultiplePrompts' to get completions in a table

--      note: this function demonstrates context linkage
------------------------------------------------------------------
USE [TEST];

DECLARE @modelName NVARCHAR(MAX) = 'llama3.2';
DECLARE @numCompletions INT = 10;

SELECT * 
FROM dbo.CompleteMultiplePrompts(
    @modelName,
    N'Give me the name of a tree. It must be fruit-bearing. ', 
    N'Answer in 20 words or less.', 
    @numCompletions);
GO

-------------------------------------------------------
-- Marketing proposals
-------------------------------------------------------
USE [TEST];

DECLARE @modelName NVARCHAR(MAX) = 'llama3.2';
DECLARE @task NVARCHAR(MAX) = '
You are a marketing expert for a home appliance company.
Give me a marketing proposal for some type of home appliance that our company will build and sell.
';

DECLARE @numCompletions INT = 5;

SELECT 
	OllamaCompletion AS Proposal
FROM dbo.CompleteMultiplePrompts(
    @modelName,
	@task,
    N'Answer in 50 words or less.', 
    @numCompletions);
GO

-------------------------------------------------------
-- Discover all models currently hosted on Ollama
-------------------------------------------------------
USE [TEST];

SELECT *
FROM dbo.GetAvailableModels()
GO


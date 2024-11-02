
-- Demonstration of LLM completion in SQL Server

-- Example of calling CompletePrompt to get completions as projections:
DECLARE @modelName NVARCHAR(MAX) = 'llama3.2';
DECLARE @ask NVARCHAR(MAX) = 'Replying only ''happy'' or ''not happy'', is the sentiment generally happy?';

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

-- Example of calling CompleteMultiplePrompts to get completions in a table:
DECLARE @modelName NVARCHAR(MAX) = 'llama3.2';
DECLARE @numCompletions INT = 10;

SELECT * 
FROM dbo.CompleteMultiplePrompts(
    @modelName,
    N'Give me the name of a tree. It must be fruit-bearing. ', 
    N'Answer in 20 words or less.', 
    @numCompletions);
GO

-- Discover all the models currently hosted on Ollama
SELECT *
FROM dbo.GetAvailableModels()
GO

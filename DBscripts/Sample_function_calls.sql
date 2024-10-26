
-- Demonstration of LLM completion in SQL Server


-- Example of calling the CompletePrompt function in SQL Server:
DECLARE @ask NVARCHAR(MAX) = 'Return an integer 1 if the sentiment is generally happy, 0 otherwise';

SELECT dbo.CompletePrompt(@ask, N'The sun rises on a new day.');
SELECT dbo.CompletePrompt(@ask, N'Tears fall in the quiet night.');
SELECT dbo.CompletePrompt(@ask, N'A warm embrace after a long journey.');
SELECT dbo.CompletePrompt(@ask, N'The laughter faded too soon.');
SELECT dbo.CompletePrompt(@ask, N'Dancing in the rain with no worries.');
SELECT dbo.CompletePrompt(@ask, N'An empty chair at the dinner table.');
SELECT dbo.CompletePrompt(@ask, N'A smile shared across the room.');
SELECT dbo.CompletePrompt(@ask, N'Memories linger like a heavy cloud.');
SELECT dbo.CompletePrompt(@ask, N'Joy fills the air with every word.');
SELECT dbo.CompletePrompt(@ask, N'The farewell came too quickly.');
GO

-- Example of calling the CompleteMultiplePrompts function to get the completions as rows:
DECLARE @numCompletions INT = 10;

SELECT * 
FROM dbo.CompleteMultiplePrompts(
    N'Give me the name of a tree.', 
    N'It must be fruit-bearing', 
    @numCompletions);
GO
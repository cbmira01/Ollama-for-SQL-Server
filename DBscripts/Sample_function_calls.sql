



-- Demonstration of LLM completion in SQL Server
DECLARE @apiUrl NVARCHAR(MAX) = 'http://localhost:11434'; -- Ollama running llama3.2


-- Example of calling the CompletePrompt function in SQL Server:
DECLARE @ask NVARCHAR(MAX) = 'Return an integer 1 if the sentiment is generally happy, 0 otherwise';

SELECT dbo.CompletePrompt(@apiUrl, @ask, 'The sun rises on a new day.');
SELECT dbo.CompletePrompt(@apiUrl, @ask, 'Tears fall in the quiet night.');
SELECT dbo.CompletePrompt(@apiUrl, @ask, 'A warm embrace after a long journey.');
SELECT dbo.CompletePrompt(@apiUrl, @ask, 'The laughter faded too soon.');
SELECT dbo.CompletePrompt(@apiUrl, @ask, 'Dancing in the rain with no worries.');
SELECT dbo.CompletePrompt(@apiUrl, @ask, 'An empty chair at the dinner table.');
SELECT dbo.CompletePrompt(@apiUrl, @ask, 'A smile shared across the room.');
SELECT dbo.CompletePrompt(@apiUrl, @ask, 'Memories linger like a heavy cloud.');
SELECT dbo.CompletePrompt(@apiUrl, @ask, 'Joy fills the air with every word.');
SELECT dbo.CompletePrompt(@apiUrl, @ask, 'The farewell came too quickly.');
GO

-- Example of calling the CompleteMultiplePrompts function to get the completions as rows:
DECLARE @numCompletions INT = 10;

SELECT * 
FROM dbo.CompleteMultiplePrompts(@apiUrl, 'Give me the name of a kitchen utensil or appliance', '', @numCompletions);
GO

-- Example of calling the ExecuteApiCommand function:
SELECT dbo.ExecuteApiCommand('https://official-joke-api.appspot.com/random_joke', '');

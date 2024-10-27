
-- Demonstration of LLM completion in SQL Server


DECLARE @ask NVARCHAR(MAX) = 'Replying only yes or no, is the sentiment generally happy?';

SELECT     
    dbo.CompletePrompt(@ask, Sentence) AS Response,
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

-- Example of calling the CompleteMultiplePrompts function to get the completions as rows:
DECLARE @numCompletions INT = 10;

SELECT * 
FROM dbo.CompleteMultiplePrompts(
    N'Give me the name of a tree.', 
    N'It must be fruit-bearing', 
    @numCompletions);
GO
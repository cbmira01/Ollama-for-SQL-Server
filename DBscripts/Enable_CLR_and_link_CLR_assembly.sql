
-- Enable CLR, drop and re-create functions and assembly link, and run a short test
-- Must be done everytime the SQL CLR project is rebuilt

sp_configure 'clr enabled', 1;
RECONFIGURE;
GO

-- Drop functions from the assembly
SELECT * FROM sys.assemblies WHERE name = 'SqlClrApiExecutor';
GO

DROP FUNCTION dbo.CompletePrompt;
DROP FUNCTION dbo.CompleteMultiplePrompts;

Drop ASSEMBLY SqlClrApiExecutor;
GO

-- Create the assembly link
CREATE ASSEMBLY SqlClrApiExecutor
FROM 'C:\Users\cmirac2\Source\PrivateRepos\ApiCommandSqlExecutor\Src\SqlClrApiExecutor\bin\Release\SqlClrApiExecutor.dll'
WITH PERMISSION_SET = UNSAFE;
GO

-- Create the functions
CREATE FUNCTION dbo.CompletePrompt(
  @askPrompt NVARCHAR(MAX), 
  @additionalPrompt NVARCHAR(MAX)
)
RETURNS NVARCHAR(MAX)
AS EXTERNAL NAME SqlClrApiExecutor.[SqlClrApiExecutor.ApiExecutor].CompletePrompt;
GO

CREATE FUNCTION dbo.CompleteMultiplePrompts(
  @askPrompt NVARCHAR(MAX), 
  @additionalPrompt NVARCHAR(MAX)
)
RETURNS NVARCHAR(MAX)
AS EXTERNAL NAME SqlClrApiExecutor.[SqlClrApiExecutor.ApiExecutor].CompleteMultiplePrompts;
GO

-- List all CLR functions and their associated assemblies
SELECT 
    asm.name AS AssemblyName,
    asm.permission_set_desc AS AssemblyPermissionSet,
    obj.name AS FunctionName,
    obj.type_desc AS ObjectType,
    mod.assembly_class AS AssemblyClass,
    mod.assembly_method AS AssemblyMethod
FROM sys.assembly_modules mod
JOIN sys.objects obj ON mod.object_id = obj.object_id
JOIN sys.assemblies asm ON mod.assembly_id = asm.assembly_id
WHERE obj.type IN ('FN', 'TF', 'IF'); -- FN = Scalar function, TF = Table-valued function, IF = Inline function
GO

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

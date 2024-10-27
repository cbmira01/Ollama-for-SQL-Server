
-- Enable CLR, drop and re-create functions and assembly link, and run a short test.
-- Must be done everytime the SQL CLR project is rebuilt.
-- These functions depend on the Ollama API service running on localhost.

sp_configure 'clr enabled', 1;
RECONFIGURE;
GO

SELECT * FROM sys.assemblies WHERE name = 'SqlClrApiExecutor';
GO

-- Drop functions from the assembly, then the assembly link
DROP FUNCTION dbo.CompletePrompt;
DROP FUNCTION dbo.CompleteMultiplePrompts;
GO

DROP ASSEMBLY SqlClrApiExecutor;
GO

-- Create the assembly link
CREATE ASSEMBLY SqlClrApiExecutor
FROM 'C:\Users\cmirac2\Source\PrivateRepos\OllamaCompletionsForSqlServer\Src\SqlClrApiExecutor\bin\Release\SqlClrApiExecutor.dll'
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
    @additionalPrompt NVARCHAR(MAX),
    @numCompletions INT
)
RETURNS TABLE (
    CompletionGuid UNIQUEIDENTIFIER,
    OllamaCompletion NVARCHAR(MAX)
)
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
--WHERE obj.type IN ('FN', 'TF', 'IF'); -- FN = Scalar function, TF = Table-valued function, IF = Inline function
GO

-- Example of calling the CompletePrompt function in SQL Server:
DECLARE @ask NVARCHAR(MAX) = 'Do Ollama, Llama3.2, and SQL Server make a good team?';

SELECT dbo.CompletePrompt(@ask, N'Tell me in fourty words or less!');
GO


-- Enable CLR, drop and re-create functions and assembly link, and run a short test.
-- Must be done everytime the SQL CLR project is rebuilt.
-- These functions depend on the Ollama API service running on 127.0.0.1.
--
-- Make sure a TEST database is available on your database server.
-- Modify @repositoryName as needed for your assembly location.
-- Modify @sqlConnection AND @APIuRL as needed for your environment.

-- Use target database; enable CLR if needed; 
USE Test;
GO

sp_configure 'clr enabled', 1;
RECONFIGURE;
SELECT * FROM sys.assemblies WHERE is_user_defined = 1;
GO

-- Drop all CLR functions and the CLR assembly
BEGIN
	IF OBJECT_ID('dbo.CompletePrompt', 'FS') IS NOT NULL
		DROP FUNCTION dbo.CompletePrompt;

	IF OBJECT_ID('dbo.CompleteMultiplePrompts', 'FT') IS NOT NULL
		DROP FUNCTION dbo.CompleteMultiplePrompts;

	IF OBJECT_ID('dbo.GetAvailableModels', 'FT') IS NOT NULL
		DROP FUNCTION dbo.GetAvailableModels;

	IF OBJECT_ID('dbo.QueryFromPrompt', 'FT') IS NOT NULL
		DROP FUNCTION dbo.QueryFromPrompt;

	-- Drop the assembly only after all dependent objects are removed
	IF EXISTS (SELECT * FROM sys.assemblies WHERE name = 'OllamaSqlClr')
		DROP ASSEMBLY OllamaSqlClr;
END
GO

-- Re-link to the most recently released CLR assembly
-- Declare variables for repository and release name; alter as needed
BEGIN
	DECLARE @repositoryName NVARCHAR(200) = 'C:\Users\cmirac2\Source\PrivateRepos\Ollama-for-SQL-Server';
	DECLARE @releaseName NVARCHAR(200) = 'Src\OllamaSqlClr\bin\Release\OllamaSqlClr.dll';
	DECLARE @fullPath NVARCHAR(400) = @repositoryName + '\' + @releaseName;

	-- Create the assembly link
	CREATE ASSEMBLY OllamaSqlClr
	FROM @fullPath
	WITH PERMISSION_SET = UNSAFE;
END
GO

-- Create links for the CLR functions
CREATE FUNCTION dbo.CompletePrompt(
    @modelName NVARCHAR(MAX), 
    @askPrompt NVARCHAR(MAX), 
    @additionalPrompt NVARCHAR(MAX)
)
RETURNS NVARCHAR(MAX)
AS EXTERNAL NAME [OllamaSqlClr].[OllamaSqlClr.SqlClrFunctions].[CompletePrompt];
GO

CREATE FUNCTION dbo.CompleteMultiplePrompts(
    @modelName NVARCHAR(MAX), 
    @askPrompt NVARCHAR(MAX), 
    @additionalPrompt NVARCHAR(MAX),
    @numCompletions INT
)
RETURNS TABLE (
    [CompletionGuid] UNIQUEIDENTIFIER,
    [CompletedBy] NVARCHAR(MAX),
    [OllamaCompletion] NVARCHAR(MAX)
)
AS EXTERNAL NAME [OllamaSqlClr].[OllamaSqlClr.SqlClrFunctions].[CompleteMultiplePrompts];
GO

CREATE FUNCTION dbo.GetAvailableModels()
RETURNS TABLE 
(
    [ModelGuid] UNIQUEIDENTIFIER,
    [Name] NVARCHAR(30),
    [Model] NVARCHAR(30),
    [ReferToName] NVARCHAR(30),
    [ModifiedAt] DATETIME,
    [Size] BIGINT,
    [Family] NVARCHAR(30),
    [ParameterSize] NVARCHAR(20),
    [QuantizationLevel] NVARCHAR(20),
    [Digest] NVARCHAR(100)
)
AS EXTERNAL NAME [OllamaSqlClr].[OllamaSqlClr.SqlClrFunctions].[GetAvailableModels]
GO

-- QueryFromPrompt function in progress
CREATE FUNCTION dbo.QueryFromPrompt(
     @modelName NVARCHAR(MAX), 
     @prompt NVARCHAR(MAX)
)
RETURNS TABLE 
(
    [QueryGuid] UNIQUEIDENTIFIER,
    [ModelName] NVARCHAR(MAX),
    [Prompt] NVARCHAR(MAX),
    [ProposedQuery] NVARCHAR(MAX),
    [Result] NVARCHAR(MAX),
    [Timestamp] DATETIME
)
AS EXTERNAL NAME [OllamaSqlClr].[OllamaSqlClr.SqlClrFunctions].[QueryFromPrompt];
GO

-- List all user-defined assemblies and all CLR functions
SELECT * FROM sys.assemblies WHERE is_user_defined = 1;
GO

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
DECLARE @modelName NVARCHAR(MAX) = 'llama3.2';
DECLARE @ask NVARCHAR(MAX) = 'Do Ollama, Llama3.2, and SQL Server make a good team?';

SELECT dbo.CompletePrompt(@modelName, @ask, N'Tell me in fourty words or less!');
GO

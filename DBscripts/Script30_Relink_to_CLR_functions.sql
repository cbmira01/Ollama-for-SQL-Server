

/**
    
    This script will:
    - drop all CLR functions and the existing CLR assembly reference
    - recreate the link to the most currently released CLR assembly
    - recreate links to the CLR functions
    - dump a list of all user-defined assemblies and all CLR functions
    - run a short query to the Ollama API server

    This script must be run every time the SQL/CLR project is rebuilt.

    These functions depend on the local Ollama API service being available...
        make sure your local Ollama API server is running.

    Use Script10 to ensure that a TEST database is available on your database 
        server with permissions for CLR integration.

    Use Script20 to populate the TEST database with demonstration data.

    Make sure the @RepositoryPath symbol is set to your local repository location.

    After running this script, take a look at the demonstration scripts:
    - Script40: Various sample function calls
    - Script50: A study of scored sentiment analysis
    - Script60: A test of 'QueryFromPrompt' (in progress)
**/

DECLARE @RepositoryPath NVARCHAR(200) = 'C:\Users\cmirac2\Source\PrivateRepos\Ollama-for-SQL-Server';

----------------------------------------------
-- Use the target database with CLR enabled 
----------------------------------------------
USE [TEST];

----------------------------------------------
-- Drop all CLR functions and the CLR assembly
----------------------------------------------
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

------------------------------------------------------
-- Relink to the most recently released CLR assembly
------------------------------------------------------
BEGIN
	DECLARE @ReleaseName NVARCHAR(200) = 'Src\OllamaSqlClr\bin\Release\OllamaSqlClr.dll';
	DECLARE @fullPath NVARCHAR(200) = @RepositoryPath + '\' + @ReleaseName;

	-- Create the assembly link
	CREATE ASSEMBLY OllamaSqlClr
	FROM @fullPath
	WITH PERMISSION_SET = UNSAFE;
END
GO

----------------------------------------------
-- Create CLR function links
----------------------------------------------
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

-----------------------------------------------------------
-- List all user-defined assemblies and CLR functions
-----------------------------------------------------------
SELECT * FROM sys.assemblies WHERE is_user_defined = 1;

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

----------------------------------------------------
-- Sanity check: Call the 'CompletePrompt' function
----------------------------------------------------
DECLARE @modelName NVARCHAR(200) = 'llama3.2';
DECLARE @ask NVARCHAR(200) = 'Do Ollama, Llama3.2, and SQL Server make a good team?';

SELECT dbo.CompletePrompt(@modelName, @ask, N'Tell me in forty words or less!');
GO

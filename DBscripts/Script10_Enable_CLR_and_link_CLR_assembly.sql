
-- Enable CLR, drop and re-create functions and assembly link, and run a short test.
-- Must be done everytime the SQL CLR project is rebuilt.
-- These functions depend on the Ollama API service running on localhost.
--
-- Be sure to modify @repositoryName as needed.

sp_configure 'clr enabled', 1;
RECONFIGURE;
GO

SELECT * FROM sys.assemblies WHERE is_user_defined = 1;
GO

USE Test;
GO

-- Drop functions from the assembly, then drop the assembly link
IF OBJECT_ID('dbo.CompletePrompt', 'FS') IS NOT NULL
    DROP FUNCTION dbo.CompletePrompt;

IF OBJECT_ID('dbo.CompleteMultiplePrompts', 'FT') IS NOT NULL
    DROP FUNCTION dbo.CompleteMultiplePrompts;

IF OBJECT_ID('dbo.GetAvailableModels', 'FT') IS NOT NULL
    DROP FUNCTION dbo.GetAvailableModels;

IF OBJECT_ID('dbo.QueryFromPrompt', 'FS') IS NOT NULL
    DROP FUNCTION dbo.QueryFromPrompt;

IF EXISTS (SELECT * FROM sys.assemblies WHERE name = 'OllamaSqlClr')
    DROP ASSEMBLY OllamaSqlClr;
GO

-- Declare variables for repository and release name, alter as needed
DECLARE @repositoryName NVARCHAR(200) = 'C:\Users\cmirac2\Source\PrivateRepos\Ollama-for-SQL-Server';
DECLARE @releaseName NVARCHAR(200) = 'Src\OllamaSqlClr\bin\Release\OllamaSqlClr.dll';
DECLARE @fullPath NVARCHAR(400) = @repositoryName + '\' + @releaseName;

-- Create the assembly link
CREATE ASSEMBLY OllamaSqlClr
FROM @fullPath
WITH PERMISSION_SET = UNSAFE;
GO

-- Create the functions
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
    @askPrompt NVARCHAR(MAX)
)
RETURNS NVARCHAR(MAX)
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

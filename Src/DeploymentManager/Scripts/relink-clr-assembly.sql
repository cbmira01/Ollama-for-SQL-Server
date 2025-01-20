
-------------------------------------------------------------------------------------
PRINT '[CHECK]: scriptName is ''relink-clr-assembly.sql'' ';
PRINT '[CHECK]: Ensure the @RepoRootDirectory symbol is declared in this script.';
-------------------------------------------------------------------------------------

-------------------------------------------------------------------------------------
PRINT '[STEP]: Persist declared symbols across GO batches';
-------------------------------------------------------------------------------------
IF OBJECT_ID('tempdb..#variables', 'U') IS NOT NULL
    DROP TABLE #variables;

CREATE TABLE #variables
(
    VarName NVARCHAR(200) PRIMARY KEY,
    VarValue NVARCHAR(200)
);

-- Insert the initial value of @MySymbol into the temporary table
INSERT INTO #variables (VarName, VarValue) VALUES ('@RepoRootDirectory', @RepoRootDirectory);
GO

-------------------------------------------------------------------------------------
PRINT '[STEP]: Determine if the AI_Lab database has been established';
-------------------------------------------------------------------------------------
USE [master];

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'AI_Lab')
BEGIN
    PRINT '[ERROR]: The AI_Lab database does not exist; it must be established.';
    SET NOEXEC ON;
END

GO

USE [AI_Lab];

-------------------------------------------------------------------------------------
PRINT '[STEP]: Drop all CLR functions';
-------------------------------------------------------------------------------------
BEGIN
    IF OBJECT_ID('dbo.CompletePrompt', 'FS') IS NOT NULL
        DROP FUNCTION dbo.CompletePrompt;

    IF OBJECT_ID('dbo.CompleteMultiplePrompts', 'FT') IS NOT NULL
        DROP FUNCTION dbo.CompleteMultiplePrompts;

    IF OBJECT_ID('dbo.GetAvailableModels', 'FT') IS NOT NULL
        DROP FUNCTION dbo.GetAvailableModels;

    IF OBJECT_ID('dbo.QueryFromPrompt', 'FT') IS NOT NULL
        DROP FUNCTION dbo.QueryFromPrompt;

    IF OBJECT_ID('dbo.ExamineImage', 'FS') IS NOT NULL
        DROP FUNCTION dbo.ExamineImage;
END

-------------------------------------------------------------------------------------
PRINT '[STEP]: Drop the old CLR assembly';
-------------------------------------------------------------------------------------
-- Drop the assembly only after all dependent objects are removed
IF EXISTS (SELECT * FROM sys.assemblies WHERE name = 'OllamaSqlClr')
    DROP ASSEMBLY OllamaSqlClr;

-------------------------------------------------------------------------------------
PRINT '[STEP]: Link to the most recent release of the CLR assembly';
-------------------------------------------------------------------------------------
DECLARE @RepoRootDirectory NVARCHAR(200) = (SELECT VarValue FROM #variables WHERE VarName = '@RepoRootDirectory');

BEGIN
    DECLARE @ReleaseName NVARCHAR(200) = 'Src\OllamaSqlClr\bin\Release\OllamaSqlClr.dll';
    DECLARE @fullPath NVARCHAR(200) = @RepoRootDirectory + '\' + @ReleaseName;

    -- Create the assembly link
    CREATE ASSEMBLY OllamaSqlClr
    FROM @fullPath
    WITH PERMISSION_SET = UNSAFE;
END
GO

-------------------------------------------------------------------------------------
PRINT '[STEP]: Create CLR function links';
GO
-------------------------------------------------------------------------------------
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

CREATE FUNCTION dbo.ExamineImage(
    @modelName NVARCHAR(MAX), 
    @prompt NVARCHAR(MAX), 
    @imageData VARBINARY(MAX)
)
RETURNS NVARCHAR(MAX)
AS EXTERNAL NAME [OllamaSqlClr].[OllamaSqlClr.SqlClrFunctions].[ExamineImage];
GO

-------------------------------------------------------------------------------------
PRINT '[STEP]: List of all user-defined assemblies and CLR functions';
-------------------------------------------------------------------------------------
SELECT 
    [name],
    [clr_name],
    [create_date]
FROM sys.assemblies WHERE is_user_defined = 1;

SELECT 
    asm.[name] AS AssemblyName,
    -- asm.permission_set_desc AS AssemblyPermissionSet,
    obj.name AS FunctionName,
    obj.type_desc AS ObjectType,
    mod.assembly_class AS AssemblyClass
    --mod.assembly_method AS AssemblyMethod
FROM sys.assembly_modules mod
JOIN sys.objects obj ON mod.object_id = obj.object_id
JOIN sys.assemblies asm ON mod.assembly_id = asm.assembly_id;
GO

-------------------------------------------------------------------------------------
PRINT '[STEP]: Sanity check: get a fast completion from a hosted model';
-------------------------------------------------------------------------------------
DECLARE @modelName NVARCHAR(200) = 'llama3.2';
DECLARE @ask NVARCHAR(200) = 'Do Ollama, Llama3.2, and SQL Server make a good team?';
DECLARE @morePrompt NVARCHAR(200) = 'Tell me in forty words or less!';

PRINT '    modelName = ' + @modelName;
PRINT '    ask = ' + @ask;
PRINT '    morePrompt = ' + @morePrompt;

SELECT dbo.CompletePrompt(@modelName, @ask, @morePrompt) as Response;
GO

-------------------------------------------------------------------------------------
PRINT '[STEP]: Cleanup';
-------------------------------------------------------------------------------------
DROP TABLE #variables;
GO

PRINT 'Goodbye...';

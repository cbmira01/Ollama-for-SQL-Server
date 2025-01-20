-------------------------------------------------------------------------------------
PRINT '[CHECK]: scriptName is ''revert-this-deployment.sql'' ';
-------------------------------------------------------------------------------------

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
PRINT '[STEP]: Drop all CLR functions and the CLR assembly';
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

    -- Drop the assembly only after all dependent objects are removed
    IF EXISTS (SELECT * FROM sys.assemblies WHERE name = 'OllamaSqlClr')
        DROP ASSEMBLY OllamaSqlClr;
END

GO

USE [master];

-------------------------------------------------------------------------------------
PRINT '[STEP]: Delete this project''s trusted assemblies';
-------------------------------------------------------------------------------------
DECLARE @hash VARBINARY(64);

-- Remove OllamaSqlClr trusted assembly if it exists
SELECT @hash = [hash]
FROM sys.trusted_assemblies
WHERE description = 'OllamaSqlClr';

IF @hash IS NOT NULL
BEGIN
    EXEC sys.sp_drop_trusted_assembly @hash = @hash;
END

-- Remove JsonClrLibrary if it exists
SELECT @hash = [hash]
FROM sys.trusted_assemblies
WHERE description = 'JsonClrLibrary';

IF @hash IS NOT NULL
BEGIN
    EXEC sys.sp_drop_trusted_assembly @hash = @hash;
END

-------------------------------------------------------------------------------------
PRINT '[STEP]: Drop the AI_Lab database';
-------------------------------------------------------------------------------------
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'AI_Lab')
BEGIN
    -- Set database to single-user mode to terminate active connections
    ALTER DATABASE [AI_Lab] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [AI_Lab];
END

PRINT 'Goodbye...';

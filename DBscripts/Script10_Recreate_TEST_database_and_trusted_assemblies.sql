

USE [master];

----------------------------------------------
-- Drop and recreate the TEST database
----------------------------------------------
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'TEST')
BEGIN
    -- Set database to single-user mode to terminate active connections
    ALTER DATABASE [TEST] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [TEST];
END

CREATE DATABASE [TEST];
ALTER DATABASE [TEST] SET TRUSTWORTHY ON;
GO

------------------------------------------------------------------------
-- Enable CLR integration, disable strict security for UNSAFE assemblies
------------------------------------------------------------------------
EXEC sp_configure 'clr enabled', 1;
RECONFIGURE;

EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;

EXEC sp_configure 'clr strict security', 0; 
RECONFIGURE;

EXEC sp_configure 'show advanced options', 0;
RECONFIGURE;
GO

----------------------------------------------
-- Delete this project's trusted assemblies
----------------------------------------------
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

----------------------------------------------
-- Define trusted assemblies for this project
----------------------------------------------
DECLARE @RepositoryPath NVARCHAR(MAX) = 'C:\Users\cmirac2\Source\PrivateRepos\Ollama-for-SQL-Server';
DECLARE @OllamaSqlClrRelease NVARCHAR(MAX) = 'Src\OllamaSqlClr\bin\Release\OllamaSqlClr.dll';
DECLARE @JsonClrLibraryRelease NVARCHAR(MAX) = 'Src\OllamaSqlClr\bin\Release\JsonClrLibrary.dll';

DECLARE @OllamaSqlClrAssemblyPath NVARCHAR(MAX) = @RepositoryPath + '\' + @OllamaSqlClrRelease;
DECLARE @JsonClrLibraryAssemblyPath NVARCHAR(MAX) = @RepositoryPath + '\' + @JsonClrLibraryRelease;

DECLARE @AssemblyPath NVARCHAR(MAX);
DECLARE @AssemblyHash VARBINARY(64);
DECLARE @sql NVARCHAR(MAX);

--------------------------------------
-- Trust the OllamaSqlClr assembly
--------------------------------------
SET @AssemblyPath = @OllamaSqlClrAssemblyPath;

SET @sql = N'
SELECT @AssemblyHashOut = HASHBYTES(''SHA2_512'', BulkColumn)
FROM OPENROWSET(BULK ''' + @AssemblyPath + ''', SINGLE_BLOB) AS A;
';

-- Execute the dynamic SQL and capture the assembly hash
EXEC sp_executesql @sql, N'@AssemblyHashOut VARBINARY(64) OUTPUT', @AssemblyHashOut=@AssemblyHash OUTPUT;

-- Add the assembly’s hash to the trusted assemblies
EXEC sys.sp_add_trusted_assembly @hash = @AssemblyHash, @description = N'OllamaSqlClr';

--------------------------------------
-- Trust the JsonClrLibrary assembly
--------------------------------------
SET @AssemblyPath = @JsonClrLibraryAssemblyPath;

SET @sql = N'
SELECT @AssemblyHashOut = HASHBYTES(''SHA2_512'', BulkColumn)
FROM OPENROWSET(BULK ''' + @AssemblyPath + ''', SINGLE_BLOB) AS A;
';

EXEC sp_executesql @sql, N'@AssemblyHashOut VARBINARY(64) OUTPUT', @AssemblyHashOut=@AssemblyHash OUTPUT;

EXEC sys.sp_add_trusted_assembly @hash = @AssemblyHash, @description = N'JsonClrLibrary';

-- Verify the assembly is now trusted
SELECT * FROM sys.trusted_assemblies;

--------------------------------

-- Below is a cleanup script that removes all entries from sys.trusted_assemblies 
-- by iterating through each trusted assembly and calling sp_drop_trusted_assembly.
--
-- Ensure you have the appropriate permissions to run sp_drop_trusted_assembly.
-- This script removes all trusted assemblies, so use it with caution.
-- 

--DECLARE @hash VARBINARY(64);

--DECLARE hash_cursor CURSOR LOCAL FORWARD_ONLY READ_ONLY FOR
--SELECT [hash]
--FROM sys.trusted_assemblies;

--OPEN hash_cursor;
--FETCH NEXT FROM hash_cursor INTO @hash;

--WHILE @@FETCH_STATUS = 0
--BEGIN
--    EXEC sys.sp_drop_trusted_assembly @hash = @hash;
--    FETCH NEXT FROM hash_cursor INTO @hash;
--END

--CLOSE hash_cursor;
--DEALLOCATE hash_cursor;

---- Verify all trusted assemblies have been removed
--SELECT * FROM sys.trusted_assemblies;

--------------------------------

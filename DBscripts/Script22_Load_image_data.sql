
/**

    This script populates the Images table for image classification studies.

    Use Script10 to ensure that a TEST database is available on your database 
        server with permissions for CLR integration.

    Use Script20 to populate the TEST database with demonstration data.

    Make sure the @RepositoryPath symbol is set to your local repository location.

    Tests of image classification can be found in Script70.
**/

-- DECLARE @RepositoryPath NVARCHAR(200) = 'C:\Users\cmirac2\Source\PrivateRepos\Ollama-for-SQL-Server';
DECLARE @RepositoryPath NVARCHAR(200) = 'C:\Temp\';
DECLARE @ImagesPath NVARCHAR(100) = 'Images\';

USE [TEST];

---------------------------------------------------------------------------------
-- Recreate Images table if it exists
---------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Images', 'U') IS NOT NULL
    DROP TABLE dbo.Images;

CREATE TABLE Images (
    Id INT PRIMARY KEY IDENTITY,
    FileName NVARCHAR(255) NOT NULL,
    ImageData VARBINARY(MAX) NOT NULL
);

---------------------------------------------------------------------------------
-- Enable xp_cmdshell for image loading
---------------------------------------------------------------------------------
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;

EXEC sp_configure 'xp_cmdshell', 1;
RECONFIGURE;

EXEC sp_configure 'xp_cmdshell';

---------------------------------------------------------------------------------
-- Bulk Insert Images from Files
---------------------------------------------------------------------------------
DECLARE @FolderPath NVARCHAR(255) = @RepositoryPath + '\' + @ImagesPath ;
DECLARE @FileName NVARCHAR(255);
DECLARE @FullFilePath NVARCHAR(255);

CREATE TABLE #FileList (FileName NVARCHAR(255));

DECLARE @Command NVARCHAR(255) = 'dir   ' + @FolderPath + '   /b';
INSERT INTO #FileList (FileName)
EXEC xp_cmdshell @Command;

DECLARE FileCursor CURSOR FOR
SELECT FileName FROM #FileList WHERE FileName IS NOT NULL;

OPEN FileCursor;

FETCH NEXT FROM FileCursor INTO @FileName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @FullFilePath = @FolderPath + @FileName;

    BEGIN TRY
        -- Use dynamic SQL for OPENROWSET
        DECLARE @DynamicSQL NVARCHAR(MAX) = N'
            INSERT INTO Images (FileName, ImageData)
            SELECT ''' + @FileName + ''', BulkColumn
            FROM OPENROWSET(BULK ''' + @FullFilePath + ''', SINGLE_BLOB) AS ImageData;';
        
        PRINT @DynamicSQL;
        EXEC sp_executesql @DynamicSQL;
    END TRY
    BEGIN CATCH
        PRINT 'Error processing file: ' + @FileName;
        PRINT ERROR_MESSAGE();
    END CATCH;

    FETCH NEXT FROM FileCursor INTO @FileName;
END;

CLOSE FileCursor;
DEALLOCATE FileCursor;

-- Drop the temporary table
DROP TABLE #FileList;

-- Confirm inserted rows
SELECT * FROM Images;

---------------------------------------------------------------------------------
-- Disable xp_cmdshell after we are done using it
---------------------------------------------------------------------------------
EXEC sp_configure 'xp_cmdshell', 0;
RECONFIGURE;

EXEC sp_configure 'show advanced options', 0;
RECONFIGURE;

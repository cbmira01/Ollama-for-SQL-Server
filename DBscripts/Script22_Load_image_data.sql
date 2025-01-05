
/**

    This script recreates  the Images table for image classification studies.

    To populate the Images table with test data, you can either:
    - Run console program 'LoadImageFiles'
    - Uncomment SQL code below to use 'xp_cmdshell' (requires copying image files to an accessible directory)

    Use Script10 to ensure that a TEST database is available on your database server.

    Use Script20 to populate the TEST database with demonstration data.

    Tests of image classification can be found in Script70.
**/

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


/********************************************************************************
-- The code commented here is an alternative way to load image data,
-- other than by using the 'LoadImageFiles' console program.

-- To use 'xp_cmdshell' to populate image data, follow these steps:

--   1. Uncomment this block of SQL code.

--   2. Make sure the @ImagesPath symbol is set to a directory accessible by 'xp_cmdshell'.
--      The C:\Temp\Images folder should work quite well.

--   3. Copy JPEG files from <repository_root>\Images directory into C:\Temp\Images\

--   4. Run the entire script to drop/create the table and bulk-insert the image files.

DECLARE @ImagesPath NVARCHAR(200) = 'C:\Temp\Images\';

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
DECLARE @FileName NVARCHAR(255);
DECLARE @FullFilePath NVARCHAR(255);

CREATE TABLE #FileList (FileName NVARCHAR(255));

DECLARE @Command NVARCHAR(255) = 'dir   ' + @ImagesPath + '   /b';
INSERT INTO #FileList (FileName)
EXEC xp_cmdshell @Command;

DECLARE FileCursor CURSOR FOR
SELECT FileName FROM #FileList WHERE FileName IS NOT NULL;

OPEN FileCursor;

FETCH NEXT FROM FileCursor INTO @FileName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @FullFilePath = @ImagesPath + @FileName;

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

********************************************************************************/
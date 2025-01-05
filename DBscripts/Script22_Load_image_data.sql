
/*************************************************************************************************

    This script recreates the Images table for image classification studies.

    The Images table can be populated with image files in two ways:
        - Run console program 'LoadImageFiles' (without using xp_cmdshell).
        - Enable xp_cmdshell (requires copying image files into an accessible directory).

    To optionally use xp_cmdshell to load image files, follow these steps:

        1. Set the @EnableXpCmdshell symbol to '1' to enable the code section.

        2. Create a folder that xp_cmdshell is able to read.
            xp_cmdshell should be able to read files in the C:\Temp\Images folder.

        3. Set the @ImagesPath symbol to the readable directory.

        4. Copy JPEG files from <repository_root>\Images folder into the readable folder.

        5. Run the entire script to drop/create the Images table and bulk-insert the image files.

    Before running this script, run Script10 to ensure that a TEST database 
        is available on your database server.

    Before running this script, it is recommended to run Script20 to populate the 
        TEST database with other demonstration data.

    Demonstrations of image classifications by model completions can be found in Script70.

*************************************************************************************************/

----------------------------------------------------------------------------------------
-- Ensure these symbols are set properly:
--      - Set @EnableXpCmdshell to 1 to enable the xp_cmdshell section, or 0 to skip it.
--      - Set @ImagesPath to a directory readable by xp_cmdshell.
DECLARE @EnableXpCmdshell BIT = 0;
DECLARE @ImagesPath NVARCHAR(200) = 'C:\Temp\Images\';
----------------------------------------------------------------------------------------

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
-- Optionally run xp_cmdshell to load image files
---------------------------------------------------------------------------------
IF @EnableXpCmdshell = 1
BEGIN
    PRINT 'xp_cmdshell section enabled. Proceeding with image loading.';

    ---------------------------------------------------------------------------------
    -- Enable permission to run xp_cmdshell
    ---------------------------------------------------------------------------------
    EXEC sp_configure 'show advanced options', 1;
    RECONFIGURE;

    EXEC sp_configure 'xp_cmdshell', 1;
    RECONFIGURE;

    ---------------------------------------------------------------------------------
    -- Bulk-insert Images from files
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
    -- Disable permission to run xp_cmdshell
    ---------------------------------------------------------------------------------
    EXEC sp_configure 'xp_cmdshell', 0;
    RECONFIGURE;

    EXEC sp_configure 'show advanced options', 0;
    RECONFIGURE;
END
ELSE
BEGIN
    PRINT 'xp_cmdshell section skipped.';
END;

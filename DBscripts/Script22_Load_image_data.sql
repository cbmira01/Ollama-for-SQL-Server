
/*************************************************************************************************

    The Images table can be populated with image files in one of two ways:

        - Via console program 'LoadImageFiles' (this script not required).

        - Bulk-insert via this script (requires image files copied into an accessible folder).

    To bulk-insert image files, follow these steps:

        1. Set the @EnableXpCmdshell symbol to '1' to enable the code section.

        2. Create a folder that xp_cmdshell is able to read.
            xp_cmdshell should be able to read files in the C:\Temp\Images folder.

        3. Set the @ImagesPath symbol to the readable folder.

        4. Copy image files from the <repository_root>\Images folder into the readable folder.

        5. Run this script.

    Before running this script, note that:

        - Script10 establishes the TEST database along with its CLR permissions.

        - Script20 recreates the Images table.

    Demonstrations of image classifications by model completions can be found in Script70.

*************************************************************************************************/

----------------------------------------------------------------------------------------
-- Ensure these symbols are set properly:

--      - Set @EnableXpCmdshell to 1 to enable the xp_cmdshell section, or 0 to skip it.
DECLARE @EnableXpCmdshell BIT = 0;

--      - Set @ImagesPath to a directory readable by xp_cmdshell.
DECLARE @ImagesPath NVARCHAR(200) = 'C:\Temp\Images\';
----------------------------------------------------------------------------------------

USE [TEST];

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

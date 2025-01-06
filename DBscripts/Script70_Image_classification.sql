
/*************************************************************************************************

    This script contains demonstrations of image classification.

    Run the 'LoadImageFiles' console job, or Script22, to load image data into the TEST database.

    Image classification depends on a model that can work with JPEG images.
    The Llava model is a good choice for image classification.

    Make sure your local Ollama API server is running.

*************************************************************************************************/

USE [TEST];
 
---------------------------------------------------------------------------------
-- Run a prompt against a chosen image
---------------------------------------------------------------------------------

-- Choose a filename
DECLARE @FileName NVARCHAR(100) = 'pexels-brunoscramgnon-596134-moon_resized.jpg';
-- DECLARE @FileName NVARCHAR(100) = 'pexels-anntarazevich-5910755-motivation_resized.jpg';
-- DECLARE @FileName NVARCHAR(100) = 'pexels-anntarazevich-6230961-scrabble-words_resized.jpg';
-- DECLARE @FileName NVARCHAR(100) = 'pexels-tdcat-59523-puppy_resized.jpg';
-- DECLARE @FileName NVARCHAR(100) = 'pexels-pixabay-45201-cat_resized.jpg';

DECLARE @Prompt NVARCHAR(100) = 'Do you recognize anything in this image?';
DECLARE @ModelName NVARCHAR(100) = 'llava';
DECLARE @ImageData VARBINARY(MAX);

SELECT @ImageData = ImageData FROM Images WHERE FileName = @FileName;

SELECT dbo.ExamineImage(@ModelName, @Prompt, @ImageData) AS Result;
GO

---------------------------------------------------------------------------------
-- Run a prompt against all images
---------------------------------------------------------------------------------

-- Choose a prompt
DECLARE @Prompt NVARCHAR(100) = 'Do you recognize anything in this image?';
-- DECLARE @Prompt NVARCHAR(100) = 'Answering only YES or NO, are there any animals depicted in this image?';
-- DECLARE @Prompt NVARCHAR(100) = 'Answering only YES or NO, are there any letters or numbers depicted in this image?';

DECLARE @ModelName NVARCHAR(100) = 'llava';

SELECT 
    NEWID() AS RequestGUID,
    FileName,
    dbo.ExamineImage(@ModelName, @Prompt, ImageData) AS Result
FROM Images;
GO

--------------------------------------------------------------------------------------
-- Run a prompt against all images using a cursor, send output to the Messages panel
--------------------------------------------------------------------------------------

-- Choose a prompt
DECLARE @Prompt NVARCHAR(100) = 'Do you recognize anything in this image?';
-- DECLARE @Prompt NVARCHAR(100) = 'Answering only YES or NO, are there any animals depicted in this image?';
-- DECLARE @Prompt NVARCHAR(100) = 'Answering only YES or NO, are there any letters or numbers depicted in this image?';

DECLARE @ModelName NVARCHAR(100) = 'llava';

-- Declare variables to store individual row data
DECLARE @FileName NVARCHAR(255);
DECLARE @ImageData VARBINARY(MAX);
DECLARE @Result NVARCHAR(MAX);
DECLARE @RequestGUID UNIQUEIDENTIFIER;

-- Declare a cursor for the Images table
DECLARE ImageCursor CURSOR FOR
SELECT FileName, ImageData
FROM Images;

-- Open the cursor
OPEN ImageCursor;

-- Fetch the first row
FETCH NEXT FROM ImageCursor INTO @FileName, @ImageData;

-- Loop through the cursor
WHILE @@FETCH_STATUS = 0
BEGIN
    -- Generate a unique identifier for each request
    SET @RequestGUID = NEWID();

    -- Process the current row and call the ExamineImage function
    SELECT @Result = dbo.ExamineImage(@ModelName, @Prompt, @ImageData);

    -- Output the results
    PRINT 'RequestGUID: ' + CAST(@RequestGUID AS NVARCHAR(36));
    PRINT 'FileName: ' + @FileName;
    PRINT 'Result: ' + @Result;
    PRINT '';

    -- Fetch the next row
    FETCH NEXT FROM ImageCursor INTO @FileName, @ImageData;
END;

-- Close and deallocate the cursor
CLOSE ImageCursor;
DEALLOCATE ImageCursor;
GO

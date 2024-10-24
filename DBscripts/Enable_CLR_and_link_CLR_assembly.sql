

sp_configure 'clr enabled', 1;
RECONFIGURE;
GO

CREATE ASSEMBLY ApiCommandLineExecutor
FROM 'C:\path\to\CommandExecutor.dll'
WITH PERMISSION_SET = EXTERNAL_ACCESS;
GO

CREATE FUNCTION ExecuteApiCommand (
    @apiUrl NVARCHAR(MAX), 
    @requestBody NVARCHAR(MAX))
RETURNS NVARCHAR(MAX)
AS EXTERNAL NAME ApiCommandLineExecutor.CommandExecutor.ExecuteApiCommand;
GO

-- Create the CompletePrompt function
CREATE FUNCTION dbo.CompletePrompt(
  @apiUrl NVARCHAR(4000), 
  @ask NVARCHAR(MAX), 
  @body NVARCHAR(MAX))
RETURNS NVARCHAR(MAX)
AS EXTERNAL NAME ApiCommandLineExecutor.CommandExecutor.CompletePrompt;
GO

-- Create the CompleteMultiplePrompts function
CREATE FUNCTION dbo.CompleteMultiplePrompts(
  @apiUrl NVARCHAR(4000), 
  @ask NVARCHAR(MAX), 
  @body NVARCHAR(MAX), 
  @numCompletions INT)
RETURNS TABLE (Completion NVARCHAR(MAX))
AS EXTERNAL NAME ApiCommandLineExecutor.CommandExecutor.CompleteMultiplePrompts;
GO


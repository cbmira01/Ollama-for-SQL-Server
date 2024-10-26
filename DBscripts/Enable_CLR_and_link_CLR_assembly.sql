

sp_configure 'clr enabled', 1;
RECONFIGURE;
GO

CREATE ASSEMBLY SqlClrApiExecutor
FROM 'C:\Users\cmirac2\Source\PrivateRepos\ApiCommandSqlExecutor\Src\SqlClrApiExecutor\bin\Release\SqlClrApiExecutor.dll'
WITH PERMISSION_SET = EXTERNAL_ACCESS;
GO

CREATE FUNCTION ExecuteApiCommand (
    @apiUrl NVARCHAR(MAX), 
    @requestBody NVARCHAR(MAX))
RETURNS NVARCHAR(MAX)
AS EXTERNAL NAME SqlClrApiExecutor.CommandExecutor.ExecuteApiCommand;
GO

-- Create the CompletePrompt function
CREATE FUNCTION dbo.CompletePrompt(
  @apiUrl NVARCHAR(4000), 
  @ask NVARCHAR(MAX), 
  @body NVARCHAR(MAX))
RETURNS NVARCHAR(MAX)
AS EXTERNAL NAME SqlClrApiExecutor.CommandExecutor.CompletePrompt;
GO

-- Create the CompleteMultiplePrompts function
CREATE FUNCTION dbo.CompleteMultiplePrompts(
  @apiUrl NVARCHAR(4000), 
  @ask NVARCHAR(MAX), 
  @body NVARCHAR(MAX), 
  @numCompletions INT)
RETURNS TABLE (Completion NVARCHAR(MAX))
AS EXTERNAL NAME SqlClrApiExecutor.CommandExecutor.CompleteMultiplePrompts;
GO

-- Check if CLR is enabled
SELECT 
    name, 
    value_in_use 
FROM sys.configurations 
WHERE name = 'clr enabled';
GO

-- List all CLR functions and their associated assemblies
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
WHERE obj.type IN ('FN', 'TF', 'IF'); -- FN = Scalar function, TF = Table-valued function, IF = Inline function
GO

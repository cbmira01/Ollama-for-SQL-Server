
PRINT 'check-this-deployment.sql';

USE [TEST];

-----------------------------------------------------------
PRINT 'List of user-defined CLR assemblies';
-----------------------------------------------------------
SELECT 
    [name],
    [clr_name],
    [create_date]
FROM sys.assemblies WHERE is_user_defined = 1;
GO

-----------------------------------------------------------
PRINT 'List of all external CLR functions';
-----------------------------------------------------------
SELECT 
    -- asm.name AS AssemblyName,
    -- asm.permission_set_desc AS AssemblyPermissionSet,
    obj.name AS FunctionName,
    obj.type_desc AS ObjectType,
    mod.assembly_class AS AssemblyClass,
    mod.assembly_method AS AssemblyMethod
FROM sys.assembly_modules mod
JOIN sys.objects obj ON mod.object_id = obj.object_id
JOIN sys.assemblies asm ON mod.assembly_id = asm.assembly_id
--WHERE obj.type IN ('FN', 'TF', 'IF'); -- FN = Scalar function, TF = Table-valued function, IF = Inline function
GO

----------------------------------------------------------------------------------
PRINT 'Dump of the database schema (required for code-generation demonstrations)';
----------------------------------------------------------------------------------
SELECT TOP 1 
    [Key], [Value]
FROM KeyValuePairs
WHERE [Key] = N'schemaJson'
ORDER BY [ID] DESC;
GO

PRINT 'Goodbye...';

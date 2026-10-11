USE [FaultDB];
GO

IF OBJECT_ID('dbo.AppUsers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AppUsers
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AppUsers PRIMARY KEY,
        ServiceId NVARCHAR(6) NOT NULL,
        Name NVARCHAR(120) NOT NULL,
        AzureAccount NVARCHAR(200) NOT NULL,
        RoleName NVARCHAR(40) NOT NULL,
        Department NVARCHAR(120) NULL,
        Phone NVARCHAR(40) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_AppUsers_IsActive DEFAULT(1),
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_AppUsers_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        CreatedBy NVARCHAR(200) NULL
    );
END;
GO

IF COL_LENGTH('dbo.AppUsers', 'ServiceId') IS NULL
    ALTER TABLE dbo.AppUsers ADD ServiceId NVARCHAR(6) NULL;
GO

IF COL_LENGTH('dbo.AppUsers', 'Name') IS NULL
    ALTER TABLE dbo.AppUsers ADD Name NVARCHAR(120) NULL;
GO

IF COL_LENGTH('dbo.AppUsers', 'AzureAccount') IS NULL
    ALTER TABLE dbo.AppUsers ADD AzureAccount NVARCHAR(200) NULL;
GO

IF COL_LENGTH('dbo.AppUsers', 'UserName') IS NOT NULL
   OR COL_LENGTH('dbo.AppUsers', 'DisplayName') IS NOT NULL
   OR COL_LENGTH('dbo.AppUsers', 'Email') IS NOT NULL
BEGIN
    EXEC sp_executesql N'
    UPDATE u
    SET
        Name = COALESCE(NULLIF(u.Name, ''''), NULLIF(u.DisplayName, ''''), NULLIF(u.UserName, ''''), NULLIF(u.Email, '''')),
        AzureAccount = COALESCE(NULLIF(u.AzureAccount, ''''), NULLIF(u.Email, ''''), NULLIF(u.UserName, '''')),
        ServiceId = COALESCE(
            NULLIF(u.ServiceId, ''''),
            CASE
                WHEN CHARINDEX(''@'', COALESCE(NULLIF(u.AzureAccount, ''''), NULLIF(u.Email, ''''), NULLIF(u.UserName, ''''))) > 1
                     AND LEN(LEFT(COALESCE(NULLIF(u.AzureAccount, ''''), NULLIF(u.Email, ''''), NULLIF(u.UserName, '''')), CHARINDEX(''@'', COALESCE(NULLIF(u.AzureAccount, ''''), NULLIF(u.Email, ''''), NULLIF(u.UserName, ''''))) - 1)) >= 6
                    THEN LOWER(LEFT(LEFT(COALESCE(NULLIF(u.AzureAccount, ''''), NULLIF(u.Email, ''''), NULLIF(u.UserName, '''')), CHARINDEX(''@'', COALESCE(NULLIF(u.AzureAccount, ''''), NULLIF(u.Email, ''''), NULLIF(u.UserName, ''''))) - 1), 6))
                WHEN CHARINDEX(''@'', COALESCE(NULLIF(u.AzureAccount, ''''), NULLIF(u.Email, ''''), NULLIF(u.UserName, ''''))) = 0
                     AND LEN(COALESCE(NULLIF(u.AzureAccount, ''''), NULLIF(u.Email, ''''), NULLIF(u.UserName, ''''))) >= 6
                    THEN LOWER(LEFT(COALESCE(NULLIF(u.AzureAccount, ''''), NULLIF(u.Email, ''''), NULLIF(u.UserName, '''')), 6))
                ELSE NULL
            END
        )
    FROM dbo.AppUsers u;';
END;
GO

IF EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_AppUsers_UserName'
      AND object_id = OBJECT_ID('dbo.AppUsers')
)
BEGIN
    DROP INDEX UX_AppUsers_UserName ON dbo.AppUsers;
END;
GO

IF EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_AppUsers_Email'
      AND object_id = OBJECT_ID('dbo.AppUsers')
)
BEGIN
    DROP INDEX UX_AppUsers_Email ON dbo.AppUsers;
END;
GO

IF COL_LENGTH('dbo.AppUsers', 'UserName') IS NOT NULL
    ALTER TABLE dbo.AppUsers DROP COLUMN UserName;
GO

IF COL_LENGTH('dbo.AppUsers', 'DisplayName') IS NOT NULL
    ALTER TABLE dbo.AppUsers DROP COLUMN DisplayName;
GO

IF COL_LENGTH('dbo.AppUsers', 'Email') IS NOT NULL
    ALTER TABLE dbo.AppUsers DROP COLUMN Email;
GO

UPDATE dbo.AppUsers
SET ServiceId = NULL
WHERE ServiceId IS NOT NULL AND LTRIM(RTRIM(ServiceId)) = '';
GO

UPDATE dbo.AppUsers
SET AzureAccount = NULL
WHERE AzureAccount IS NOT NULL AND LTRIM(RTRIM(AzureAccount)) = '';
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_AppUsers_ServiceId'
      AND object_id = OBJECT_ID('dbo.AppUsers')
)
BEGIN
    CREATE UNIQUE INDEX UX_AppUsers_ServiceId ON dbo.AppUsers(ServiceId)
    WHERE ServiceId IS NOT NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_AppUsers_AzureAccount'
      AND object_id = OBJECT_ID('dbo.AppUsers')
)
BEGIN
    CREATE UNIQUE INDEX UX_AppUsers_AzureAccount ON dbo.AppUsers(AzureAccount)
    WHERE AzureAccount IS NOT NULL;
END;
GO

SELECT
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length,
    c.is_nullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.AppUsers')
ORDER BY c.column_id;
GO
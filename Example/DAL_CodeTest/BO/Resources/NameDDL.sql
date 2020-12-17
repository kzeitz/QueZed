-- This resource isn't needed.
-- It's just an example of the DDL used to generate the table and supporting app.udi objects
IF OBJECT_ID(N'[dbo].Name', N'U') IS NULL
BEGIN
	PRINT 'Create Table [dbo].[Name]'
	CREATE TABLE [dbo].[Name] (
		[ID]		INT IDENTITY(1, 1) NOT FOR REPLICATION NOT NULL,
		[FirstName]	VARCHAR(255) NOT NULL,
		[LastName]	VARCHAR(255) NOT NULL,
		CONSTRAINT [PK_Name] PRIMARY KEY CLUSTERED (ID),
	) ON [PRIMARY]
END
GO

EXEC [app.udi].AuditColumns  '[dbo].[Name]'
EXEC [app.udi].AuditTrigger	 '[dbo].[Name]'
EXEC [app.udi].DeleteTrigger '[dbo].[Name]'
EXEC [app.udi].UDI			 '[dbo].[Name]'
EXEC [app.udi].[View]        '[dbo].[Name]'
GO

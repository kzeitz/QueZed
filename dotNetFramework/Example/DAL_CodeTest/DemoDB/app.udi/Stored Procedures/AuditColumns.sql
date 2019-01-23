CREATE PROCEDURE [app.udi].AuditColumns(@tableName SYSNAME) AS SET NOCOUNT ON BEGIN
	PRINT '[app.udi].AuditColumns'
	EXEC [app.udi.utility].[column] @tableName, 'added', 'DATETIME2', 'GETDATE()'
	EXEC [app.udi.utility].[column] @tableName, 'edited', 'DATETIME2'	
	EXEC [app.udi.utility].[column] @tableName, 'editor', 'VARCHAR(64)'
	EXEC [app.udi.utility].[column] @tableName, 'deleted', 'INT', '0'
	EXEC [app.udi.utility].[column] @tableName, 'version', 'ROWVERSION'
END
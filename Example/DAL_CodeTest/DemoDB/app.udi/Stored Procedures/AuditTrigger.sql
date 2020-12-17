CREATE PROCEDURE [app.udi].AuditTrigger(@tableName SYSNAME) AS SET NOCOUNT ON BEGIN
DECLARE 
	@schemaName SYSNAME,
	@triggerName SYSNAME,
	@pkColumnName SYSNAME,
	@sql VARCHAR(max)

	PRINT '[app.udi].AuditTrigger'
	SET @schemaName = [app.utility].schemaName(@tableName)
	SET @triggerName = @schemaName + '.AIUTr_' + [app.utility].tableName(@tableName)
	EXEC [app.udi.utility].checkSchema @schemaName
	IF EXISTS(
		SELECT o.[Name] FROM sys.objects o
		INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
		WHERE o.[Name] = [app.utility].tableName(@triggerName) AND s.[name] = @schemaName
	) EXEC('DROP TRIGGER ' + @triggerName)
	SELECT @pkColumnName = pkColumnName FROM [app.udi.utility].pkColumns(@TableName)
	SET @sql = 'CREATE TRIGGER ' + @triggerName + ' ON ' + @tableName + ' AFTER INSERT, UPDATE' + CHAR(13) + 
		'AS' + CHAR(13) +
		'BEGIN ' + CHAR(13) +
		'SET NOCOUNT ON;' + CHAR(13) +
		CHAR(9) + 'UPDATE '+ @tableName + ' SET added = GETDATE(), editor = SYSTEM_USER WHERE ' + @pkColumnName + ' IN (SELECT ' + @pkColumnName + ' FROM inserted) AND ' + @pkColumnName + ' NOT IN (SELECT ' + @pkColumnName + ' FROM deleted)' + CHAR(13) +
		CHAR(9) + 'UPDATE '+ @tableName + ' SET edited = GETDATE(), editor = SYSTEM_USER WHERE ID IN (SELECT ID FROM deleted)' + CHAR(13) +
		'END'
	EXEC(@sql)
END
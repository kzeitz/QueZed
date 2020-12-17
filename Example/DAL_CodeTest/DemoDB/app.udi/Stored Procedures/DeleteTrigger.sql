CREATE PROCEDURE [app.udi].DeleteTrigger(@tableName SYSNAME) AS SET NOCOUNT ON BEGIN
DECLARE 
	@schemaName SYSNAME,
	@pkColumnName SYSNAME,
	@triggerName SYSNAME,
	@tempTableName SYSNAME,
	@sql VARCHAR(max)

	PRINT '[app.udi].DeleteTrigger'
	BEGIN TRY
	SELECT @pkColumnName = pkColumnName FROM [app.udi.utility].pkColumns(@TableName)
	IF (@pkColumnName IS NULL) RAISERROR('The Table ''%s'' does not have a Primary Key Defined. Please define and re-run.', 16, 1, @TableName)

	SET @schemaName = [app.utility].schemaName(@tableName)
	SET @triggerName = @schemaName + '.IODTr_' + [app.utility].tableName(@tableName)

	SELECT @tempTableName = [app.utility].tempTableName(@tableName)
	IF EXISTS(
		SELECT o.[Name] FROM sys.objects o
		INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
		WHERE o.[Name] = [app.utility].tableName(@triggerName) AND s.[name] = @schemaName
	) EXEC('DROP TRIGGER ' + @triggerName)
	SET @sql =
		'CREATE TRIGGER ' + @triggerName + ' ON ' + @tableName + ' INSTEAD OF DELETE AS ' + CHAR(13) + 
		'BEGIN ' + CHAR(13) +
		CHAR(9) + 'SET NOCOUNT ON;' + CHAR(13) +
		CHAR(9) + 'UPDATE ' + @tableName + ' SET deleted = 1, edited = GETDATE(), editor = SYSTEM_USER WHERE ' + @pkColumnName + ' IN (SELECT ' + @pkColumnName + ' FROM deleted) AND deleted <> 1' + CHAR(13) +
		CHAR(9) + 'CREATE TABLE ' + @tempTableName + ' (ID INT)' + CHAR(13) +
		CHAR(9) + 'INSERT ' + @tempTableName + ' SELECT ' + @pkColumnName + ' FROM deleted' + CHAR(13) +
		CHAR(9) + 'EXEC [app.utility].CascadeDelete ' + QUOTENAME(@tableName, '''') + CHAR(13) + 
		'END'
	EXEC (@sql)
	END TRY
	BEGIN CATCH
		SELECT ERROR_MESSAGE() AS ErrorMessage;
	END CATCH
END
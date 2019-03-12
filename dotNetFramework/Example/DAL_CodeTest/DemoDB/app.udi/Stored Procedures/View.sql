CREATE PROCEDURE [app.udi].[View](@tableName SYSNAME, @ignoreError BIT = 0) AS SET NOCOUNT ON BEGIN
DECLARE 
	@schemaName SYSNAME,
	@viewName SYSNAME,
	@selectFilter VARCHAR(max),
	@sql VARCHAR(max),
	@deleted INT
	PRINT '[app.udi].[View]'
	-- Build table of columns name and generate formal parameter names.
	-- Gives the ability to select comma delimited strings built out of column values.
	EXEC [app.udi.utility].tableColumns @tableName
	
	SET @schemaName = [app.utility].schemaName(@tableName)
	SET @viewName = '[' + @schemaName + '.select].' + QUOTENAME([app.utility].tableName(@tableName))

	SET @schemaName = [app.utility].schemaName(@viewName)
	EXEC [app.udi.utility].checkSchema @schemaName
	-- Remove old definition of the view
	EXEC('IF OBJECT_ID(''' + @viewName + ''', ''V'') IS NOT NULL DROP VIEW ' + @viewName)

	EXEC [app.udi.utility].delimitedColumns 'ColumnName', @selectFilter OUTPUT, NULL, NULL
	SELECT @deleted = COALESCE(COL_LENGTH(@tableName, 'deleted'), 0)
	IF (0 = @ignoreError AND 0 = @deleted) RAISERROR('The Table ''%s'' does not have a ''deleted'' column. Please define and re-run.', 16, 1, @tableName)
	IF (1 = @ignoreError AND 0 = @deleted) Print('The Table ''' + @tableName + ''' does not have a ''deleted'' column.  Ignore error is true.')

	SET @sql = 'CREATE VIEW ' + @viewName + CHAR(13) + 
		'AS' + CHAR(13) +
		'SELECT ' + @selectFilter + CHAR(13) +
		'FROM ' + @tableName + CHAR(13) +
		CASE WHEN @deleted > 0 THEN 'WHERE deleted = 0' ELSE '' END
	EXEC(@sql)
	EXEC [app.udi.utility].columnsTable 0
END
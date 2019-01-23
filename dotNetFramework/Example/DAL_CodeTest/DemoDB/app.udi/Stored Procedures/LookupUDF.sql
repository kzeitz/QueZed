CREATE PROCEDURE [app.udi].LookupUDF(@tableName SYSNAME) AS SET NOCOUNT ON BEGIN
DECLARE 
	@schemaName SYSNAME,
	@typeName SYSNAME,
	@funcName SYSNAME,
	@param SYSNAME,
	@paramType SYSNAME,
	@returnType SYSNAME,
	@returnVar SYSNAME,
	@selectCol SYSNAME,
	@whereCol SYSNAME,
	@sql VARCHAR(max)

	PRINT '[app.udi].LookupUDF'
	EXEC [app.udi.utility].tableColumns @tableName
	SET @schemaName = [app.utility].schemaName(@tableName)
	SET @typeName = 'Lookup_' + @schemaName + '_' + [app.utility].tableName(@tableName)
	IF TYPE_ID(@typeName) IS NULL EXEC sp_addtype @typeName, 'INT', 'NOT NULL'

	SET @funcName = @schemaName + '.Lookup' + [app.utility].tableName(@tableName)
	SELECT @whereCol = ColumnName FROM [app.udi.utility].[columns] WHERE ColumnUserTypeName = 'LOOKUPNAME' 
	EXEC [app.udi.utility].checkSchema @schemaName
	-- Remove old function
	EXEC('IF OBJECT_ID(''' + @funcName + ''', ''FN'') IS NOT NULL DROP FUNCTION ' + @funcName)
	SET @param = '@name'
	SET @paramType = '[app.udi.utility].LOOKUPNAME'
	SET @returnType = '[app.udi.utility].LOOKUPID'
	SET @returnVar = '@id'
	SET @selectCol = 'ID'
	SET @sql =
	'CREATE FUNCTION ' + @funcName + '(' + @param + ' ' + @paramType + ')' + CHAR(13) +
	'RETURNS ' + @returnType + CHAR(13) +
	'AS' + CHAR(13) +
	'BEGIN' + CHAR(13) +
	CHAR(9) + 'DECLARE ' + @returnVar + ' ' + @returnType + CHAR(13) +
	CHAR(9) + 'SELECT ' + @returnVar + ' = ' + @selectCol + ' FROM ' + @tableName + ' WHERE ' + @whereCol + ' = '  + @param + CHAR(13) +
	CHAR(9) + 'RETURN ' + @returnVar + CHAR(13) +
	'END' + CHAR(13)
	EXEC (@sql)
	EXEC [app.udi.utility].columnsTable 0
END
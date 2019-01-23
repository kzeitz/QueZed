CREATE PROCEDURE [app.udi].UDI (@tableName SYSNAME) AS SET NOCOUNT ON BEGIN
DECLARE
	@procedure VARCHAR(max),
	@oldDboProcName SYSNAME,
	@oldProcName SYSNAME,
	@procName SYSNAME,
	@pkColumnName SYSNAME,
	@schemaName SYSNAME,
	@isIdentity BIT,
	@selectFilter VARCHAR(max),
	@whereFilter VARCHAR(max),
	@parameterListSql VARCHAR(max),
	@insertCheckSql VARCHAR(max),
	@insertColumnsSql VARCHAR(max),
	@insertSelectSql VARCHAR(max),
	@updateCheckSql VARCHAR(max),
	@updateSql VARCHAR(max)

	PRINT '[app.udi].UDI'
	BEGIN TRY
	-- Build table of columns name and generate formal parameter names.
	-- Gives the ability to select comma delimited strings built out of column values.
	EXEC [app.udi.utility].tableColumns @tableName

	SET @schemaName = [app.utility].schemaName(@tableName)
	SET @oldDboProcName = 'dbo.' + QUOTENAME('UDI_' + [app.utility].tableName(@tableName))
	SET @oldProcName = @schemaName + '.' + QUOTENAME('UDI_' + [app.utility].tableName(@tableName))
	-- Build declaration for store procedure
	SET @procName = '[dom.udi]' + '.' + QUOTENAME([app.utility].tableName(@tableName))

	-- Build Parameter list including data types and length for data types that require length. All parameters
	-- are optional so an ' = NULL' is appended.
	SET @selectFilter = 'CASE WHEN ColumnUserTypeName IS NOT NULL AND CHARINDEX(''lookup_'', LOWER(ColumnUserTypeName)) = 1 THEN ParameterName + ''LookupName'' + '' '' + ''LOOKUPNAME'' ELSE ParameterName + '' '' + ColumnTypeName END + CASE WHEN CHARINDEX(''var'', ColumnTypeName) > 0 THEN CASE WHEN ColumnLength = -1 THEN + ''(max)'' ELSE + ''('' + CONVERT(VARCHAR, ColumnLength) + '')'' END ELSE + '''' END + CASE WHEN ColumnDefaultValue IS NOT NULL AND PATINDEX(''%(%)%'', ColumnDefaultValue) = 0 THEN + '' = '' + ColumnDefaultValue ELSE + '' = NULL'' END'
	EXEC [app.udi.utility].delimitedColumns @selectFilter, @parameterListSql OUTPUT, NULL, NULL

	-- Get the primary key column name this parameter value will control insert vs update.
	-- If the primary key parameter value is null then its an insert otherwise its an update.
	SELECT @pkColumnName = pkColumnName FROM [app.udi.utility].pkColumns(@tableName)
	IF (@pkColumnName IS NULL) RAISERROR('The Table ''%s'' does not have a Primary Key Defined. Please define and re-run.', 16, 1, @tableName)
	SET @isIdentity = (SELECT IsIdentity = COLUMNPROPERTY(OBJECT_ID(@tableName), @pkColumnName, 'IsIdentity'))
	
	-- Build insert section
	SET @insertCheckSql = CASE WHEN @isIdentity = 1 THEN '@' + @pkColumnName + ' IS NULL' ELSE '(SELECT COUNT(*) FROM ' + @tableName + ' WHERE ' + @pkColumnName + ' = @' + @pkColumnName + ') = 0' END

	---- Build insert column list . Format of "INSERT <table> ([col one], [col two], ...)"
	SET @whereFilter = CASE WHEN @isIdentity = 1 THEN 'WHERE ColumnName NOT IN (SELECT QUOTENAME(pkColumnName) FROM [app.udi.utility].pkColumns(''' + @tableName + '''))' ELSE '' END
	EXEC [app.udi.utility].delimitedColumns 'ColumnName', @insertColumnsSql OUTPUT, @whereFilter, NULL

	---- Build insert value list. Format of "VALUES( @col_one, @col_two, ...'
	SET @selectFilter = 'CASE WHEN ColumnUserTypeName IS NOT NULL AND CHARINDEX(''lookup_'', LOWER(ColumnUserTypeName)) = 1 THEN REPLACE(RIGHT(ColumnUserTypeName, LEN(ColumnUserTypeName) - LEN(''lookup_'')), ''_'', ''.Lookup'') + ''('' + ParameterName + ''LookupName'' + '')'' ELSE ParameterName END'
	SET @whereFilter = CASE WHEN @isIdentity = 1 THEN 'WHERE ParameterName NOT IN (SELECT ''@'' + pkColumnName FROM [app.udi.utility].pkColumns(''' + @tableName + '''))' ELSE '' END
	EXEC [app.udi.utility].delimitedColumns @selectFilter, @insertSelectSql OUTPUT, @whereFilter, NULL
	IF (@insertSelectSql IS NULL) RAISERROR('The Table ''%s'' does not have any non Key columns Defined. Please define and re-run.', 16, 1, @tableName)

	-- Build update section
	SET @selectFilter = 'CASE WHEN ColumnUserTypeName IS NOT NULL AND CHARINDEX(''lookup_'', LOWER(ColumnUserTypeName)) = 1 THEN ColumnName + ''='' + REPLACE(RIGHT(ColumnUserTypeName, LEN(ColumnUserTypeName) - LEN(''lookup_'')), ''_'', ''.Lookup'') + ''('' + ParameterName + ''LookupName'' + '')'' ELSE ColumnName + ''='' + ParameterName END'
	SET @whereFilter = 'WHERE ColumnName NOT IN (SELECT QUOTENAME(pkColumnName) FROM [app.udi.utility].pkColumns(''' + @tableName + '''))'
	EXEC [app.udi.utility].delimitedColumns @selectFilter, @updateSql OUTPUT, @whereFilter, NULL 

	-- Build parameter NULL check list'
--	SET @selectFilter = 'CASE WHEN ColumnUserTypeName IS NOT NULL AND CHARINDEX(''lookup_'', LOWER(ColumnUserTypeName)) = 1 THEN ParameterName + ''LookupName'' WHEN CHARINDEX(''date'', ColumnTypeName) > 0 OR CHARINDEX(''uniqueidentifier'', ColumnTypeName) > 0 OR CHARINDEX(''bit'', ColumnTypeName) > 0 THEN ''CONVERT(VARCHAR, '' + ParameterName + '')'' ELSE ParameterName END'
	SET @selectFilter = 'CASE WHEN ColumnUserTypeName IS NOT NULL AND CHARINDEX(''lookup_'', LOWER(ColumnUserTypeName)) = 1 THEN ParameterName + ''LookupName'' WHEN NOT CHARINDEX(''image'', ColumnTypeName) > 0 THEN ''CONVERT(VARCHAR, '' + ParameterName + '')'' ELSE ParameterName END'
	SET @whereFilter = 'WHERE ParameterName NOT IN (SELECT ''@'' + pkColumnName FROM [app.udi.utility].pkColumns(''' + @tableName + '''))'
	EXEC [app.udi.utility].delimitedColumns @selectFilter, @updateCheckSql OUTPUT, @whereFilter, NULL
	IF CHARINDEX(', ', @updateCheckSql) > 0 SET @updateCheckSql = 'COALESCE(NULL, ' + @updateCheckSql + ')' 

	-- Remove old definitions of the procedure
	EXEC('IF OBJECT_ID(''' + @oldDboProcName + ''', ''P'') IS NOT NULL DROP PROCEDURE ' + @oldDboProcName)
	EXEC('IF OBJECT_ID(''' + @oldProcName + ''', ''P'') IS NOT NULL DROP PROCEDURE ' + @oldProcName)
	EXEC('IF OBJECT_ID(''' + @procName + ''', ''P'') IS NOT NULL DROP PROCEDURE ' + @procName)

	SET @schemaName = [app.utility].schemaName(@procName)
	EXEC [app.udi.utility].checkSchema @schemaName
	-- Need this scalar because EXEC doesn't like calling functions
	SET @procedure = 'CREATE PROCEDURE ' + @procName + CHAR(13) +
		CHAR(9) + '(' + @parameterListSql + ') AS ' + CHAR(13) +
		'SET NOCOUNT ON' + CHAR(13) +
		'IF ' + @insertCheckSql + ' BEGIN' + CHAR(13) +
		CHAR(9) + 'INSERT ' + @tableName + ' (' + @insertColumnsSql + ') VALUES (' + @insertSelectSql + ')' + CHAR(13) +
		CHAR(9) + 'SELECT SCOPE_IDENTITY() ' + CHAR(13) +
		'END' + CHAR(13) +
		'ELSE BEGIN' + CHAR(13) +
		CHAR(9) + 'IF ' + @updateCheckSql + ' IS NOT NULL' + CHAR(13) +
		CHAR(9) + CHAR(9) + 'UPDATE ' + @tableName + ' SET ' + @updateSql + ' WHERE ' + @pkColumnName + ' = @' + @pkColumnName + CHAR(13) +
		CHAR(9) + 'ELSE DELETE '+ @tableName + ' WHERE ' + @pkColumnName + ' = @' + @pkColumnName + CHAR(13) +
		CHAR(9) + 'SELECT @@ROWCOUNT' + CHAR(13) +
		'END' 
	--PRINT @procedure
	EXEC(@procedure)
	SET @updateCheckSql = 'SET NOCOUNT ON; SELECT c.text FROM sys.objects o INNER JOIN syscomments c ON c.id = object_id INNER JOIN sys.schemas s ON s.schema_id = o.schema_id WHERE o.[name] = ''' + [app.utility].tableName(@procName) + ''' AND s.[name] = ''' + [app.utility].schemaName(@procName) + ''''
	EXEC(@updateCheckSql)
	END TRY
	BEGIN CATCH
		SELECT ERROR_MESSAGE() AS ErrorMessage;
		PRINT 'Probable source of error -->' + CHAR(13) + @procedure
	END CATCH
	EXEC [app.udi.utility].columnsTable 0
END
-- Dependantcies
-- schemas
IF SCHEMA_ID('app.utility') IS NULL EXEC('CREATE SCHEMA [app.utility] AUTHORIZATION dbo')
GO
IF SCHEMA_ID('app.udi') IS NULL EXEC('CREATE SCHEMA [app.udi] AUTHORIZATION dbo')
GO
IF SCHEMA_ID('app.udi.utility') IS NULL EXEC('CREATE SCHEMA [app.udi.utility] AUTHORIZATION dbo')
GO
-- types
IF TYPE_ID('[app.udi.utility].LookupId') IS NULL CREATE TYPE [app.udi.utility].LookupId FROM INT NOT NULL
GO
IF TYPE_ID('[app.udi.utility].LookupName') IS NULL CREATE TYPE [app.udi.utility].LookupName FROM VARCHAR(150) NOT NULL
GO
IF TYPE_ID('[app.udi.utility].DefaultValue') IS NULL CREATE TYPE [app.udi.utility].DefaultValue FROM VARCHAR(50) NOT NULL
GO

-- [app.utility] schema
--------------------------------------------------------------------------------------------------------
-- These functions parse out the parts of a table name if they exist
--------------------------------------------------------------------------------------------------------
IF OBJECT_ID('[app.utility].tableName', 'FN') IS NOT NULL DROP FUNCTION [app.utility].tableName
GO
CREATE FUNCTION [app.utility].tableName(@fullName SYSNAME) RETURNS SYSNAME AS BEGIN	RETURN PARSENAME(@fullName, 1) END
GO

IF OBJECT_ID('[app.utility].schemaName', 'FN') IS NOT NULL DROP FUNCTION [app.utility].schemaName
GO
CREATE FUNCTION [app.utility].schemaName(@fullName SYSNAME)	RETURNS SYSNAME AS BEGIN RETURN ISNULL(PARSENAME(@fullName, 2), SCHEMA_NAME()) END
GO

IF OBJECT_ID('[app.utility].databaseName', 'FN') IS NOT NULL DROP FUNCTION [app.utility].databaseName
GO
CREATE FUNCTION [app.utility].databaseName(@fullName SYSNAME) RETURNS SYSNAME AS BEGIN RETURN ISNULL(PARSENAME(@fullName, 3), DB_NAME()) END
GO

IF OBJECT_ID('[app.utility].serverName', 'FN') IS NOT NULL DROP FUNCTION [app.utility].serverName
GO
CREATE FUNCTION [app.utility].serverName(@fullName SYSNAME)	RETURNS SYSNAME AS BEGIN RETURN ISNULL(PARSENAME(@fullName, 4), @@SERVERNAME) END
GO

--------------------------------------------------------------------------------------------------------
-- This function trims white space from left AND right sides of a string
--------------------------------------------------------------------------------------------------------
IF OBJECT_ID('[app.utility].trim', 'FN') IS NOT NULL DROP FUNCTION [app.utility].trim
GO
CREATE FUNCTION [app.utility].trim(@string VARCHAR(max)) RETURNS VARCHAR(max) AS BEGIN RETURN LTRIM(RTRIM(@string)) END
GO

--------------------------------------------------------------------------------------------------------
-- This function recursively removes enclosing character pairs.
--------------------------------------------------------------------------------------------------------
IF OBJECT_ID('[app.utility].removeEnclosingCharacterPair', 'FN') IS NOT NULL DROP FUNCTION [app.utility].removeEnclosingCharacterPair
GO
CREATE FUNCTION [app.utility].removeEnclosingCharacterPair(@string VARCHAR(max), @open CHAR = '(', @close CHAR = ')') RETURNS VARCHAR(max) AS
BEGIN
	DECLARE @ret VARCHAR(max)
	IF (LEFT(@string, 1) = @open AND RIGHT(@string, 1) = @close)
		SET @ret = [app.utility].removeEnclosingCharacterPair(SUBSTRING(@string, 2, LEN(@string) - 2), @open, @close)
	ELSE SET @ret = @string
	RETURN @ret	
END
GO

--------------------------------------------------------------------------------------------------------
-- This function performs the inverse of the built-in QUOTENAME function.
--------------------------------------------------------------------------------------------------------
IF OBJECT_ID('[app.utility].unquoteName', 'FN') IS NOT NULL DROP FUNCTION [app.utility].unquoteName
GO
CREATE FUNCTION [app.utility].unquoteName(@name SYSNAME, @quoteChar CHAR = N']') RETURNS SYSNAME AS
BEGIN
   DECLARE @ret SYSNAME
   DECLARE @openQuote CHAR = '[', @closeQuote CHAR = ']'
   IF @quoteChar IN ('''', '"') BEGIN 
	  SET @openQuote = @quoteChar 
	  SET @closeQuote = @quoteChar
   END	
   RETURN [app.utility].removeEnclosingCharacterPair(@name, @openQuote, @closeQuote)
END
GO

--------------------------------------------------------------------------------------------------------
-- This procedure takes in a pattern match parameter (see syntax for PATINDEX) and removes any characters that do not adhere to that pattern.
--------------------------------------------------------------------------------------------------------
IF OBJECT_ID('[app.utility].stripPattern', 'FN') IS NOT NULL DROP FUNCTION [app.utility].stripPattern
GO
CREATE FUNCTION [app.utility].stripPattern(@Input VARCHAR(max), @Pattern VARCHAR(100)) RETURNS VARCHAR(max) AS
BEGIN
	WHILE PATINDEX(@Pattern, @Input) != 0
		BEGIN
			SET @Input = REPLACE(@Input, SUBSTRING(@Input, PATINDEX(@Pattern, @Input), 1), '')
		END
	RETURN @Input
END
GO

--------------------------------------------------------------------------------------------------------
-- This procedure returns a name for a tempoarary table, based on the name passed.
--------------------------------------------------------------------------------------------------------
IF OBJECT_ID('[app.utility].tempTableName', 'FN') IS NOT NULL DROP FUNCTION [app.utility].tempTableName
GO
CREATE FUNCTION [app.utility].tempTableName(@tableName SYSNAME) RETURNS SYSNAME AS
BEGIN
	RETURN  '#' + [app.utility].unquoteName(RIGHT(@tableName, LEN(@tableName) - CHARINDEX('.', @tableName)), NULL);
END
GO

-- [app.utility] schema
--------------------------------------------------------------------------------------------------------
-- This procedure is [app.utility] because it depends on the 'DeleteTrigger' to create a working table for this procedure to use
-- and it is called at runtime
--------------------------------------------------------------------------------------------------------
IF OBJECT_ID('[app.utility].CascadeDelete', 'P') IS NOT NULL DROP PROCEDURE [app.utility].CascadeDelete
GO
CREATE PROCEDURE [app.utility].CascadeDelete(@ParentTableName SYSNAME) AS SET NOCOUNT ON BEGIN
-- This procedure is to be called by INSTEAD OF DELETE trigger of tables w/ foreign keys into them.
-- The huge assumption is that the INSTEAD OF DELETE trigger will create a temporary table w/ name
-- #tablename - where tablename is SAME value passed in as @ParentTableName parameter of this 
-- stored procedure.
DECLARE @refCsr CURSOR
DECLARE 
	@childTableName SYSNAME,
	@pKeyColumnName SYSNAME,
	@fKeyColumnName SYSNAME,
	@tempTableName SYSNAME

	SET @refCsr = CURSOR FOR
	SELECT 
		s.[name] + '.' + OBJECT_NAME(fk.parent_object_id) AS ChildTable,
		COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS PKeyColumn,
		COL_NAME(fkc.parent_object_id, fkc.parent_column_id) As FKeyColumn
	FROM 
		sys.foreign_keys fk
		INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
		INNER JOIN sys.schemas s ON s.schema_id = fk.schema_id
	WHERE
		fk.referenced_object_id = OBJECT_ID(@ParentTableName, 'U')
	OPEN @refCsr
	FETCH NEXT FROM @refCsr INTO @childTableName, @pKeyColumnName, @fKeyColumnName
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT @tempTableName = [app.utility].tempTableName(@ParentTableName)
		EXEC('DELETE ' + @childTableName + ' WHERE ' + @fKeyColumnName + ' IN  ( SELECT ' + @pKeyColumnName + ' FROM ' + @tempTableName + ')')
		FETCH NEXT FROM @refCsr INTO @childTableName, @pKeyColumnName, @fKeyColumnName
	END
	CLOSE @refCsr
	DEALLOCATE @refCsr
END
GO

-- [app.udi] schema

-- udi utility
--------------------------------------------------------------------------------------------------------
-- This utility function Returns the primary key columns (name, datatype, length) for the specified table.
-- It returns the rows in a table variable. The primary key value is used in the UDI
-- stored procedure of the table to determine which insert / update / delete functionality is aapropriate
--------------------------------------------------------------------------------------------------------
IF OBJECT_ID('[app.udi.utility].pkColumns', 'TF') IS NOT NULL DROP FUNCTION [app.udi.utility].pkColumns
GO
CREATE FUNCTION [app.udi.utility].pkColumns(@tableName SYSNAME) RETURNS @PKColumns TABLE (pkColumnName SYSNAME, pkColumnType SYSNAME, pkColumnLength INT) AS
BEGIN
	INSERT @PKColumns
	SELECT c.[name], st.[name], c.length
	FROM 
		sysobjects pk,
		sysindexes i,
		sysindexkeys k,
		syscolumns c,
		systypes st 
	WHERE 
		OBJECT_ID(@tableName, 'U') = pk.parent_obj AND 
		pk.xtype = 'PK' AND
		pk.[name] = i.[name] AND 
		i.id = k.id AND 
		i.indid = k.indid AND
		k.colid = c.colid AND 
		c.id =pk.parent_obj AND
		c.xtype = st.xtype AND 
		c.type = st.type AND
		st.xusertype & 256 = 0
RETURN 
END
GO

--------------------------------------------------------------------------------------------------------
-- This utility stored procedure drops and if directed re-creates the [app.udi.utility].columns table.
-- [app.udi.utility].columns table is defined as:
--	ColumnName		- the quoted ([name]) name of the actual column.
--	ParameterName	- a generated value based on the column name. Any spaces in the column name are replaced w/ under-scores.
--	ColumnTypeName	- the systypes name value for the specified colum type.
--	ColumnLength	- used of varying length data types.

IF OBJECT_ID('[app.udi.utility].columns', 'U') IS NOT NULL DROP TABLE [app.udi.utility].columns
IF OBJECT_ID('[app.udi.utility].columnsTable', 'P') IS NOT NULL DROP PROCEDURE [app.udi.utility].columnsTable
GO
CREATE PROCEDURE [app.udi.utility].columnsTable(@createTable BIT) AS SET NOCOUNT ON BEGIN
	EXEC('IF OBJECT_ID(''[app.udi.utility].[columns]'', ''U'') IS NOT NULL DROP TABLE [app.udi.utility].[columns]')
	IF (1 = @createTable) EXEC('CREATE TABLE [app.udi.utility].[columns] (ColumnName SYSNAME, ParameterName SYSNAME, ColumnTypeName SYSNAME, ColumnLength INT, ColumnId INT, ColumnUserTypeName SYSNAME NULL, ColumnDefaultValue VARCHAR(max) NULL)')
END
GO

--------------------------------------------------------------------------------------------------------
-- This utility stored procedure populates the table '[app.udi.utility].columns' with the column definitions 
-- for all the columns in the specified table.
-- The columns of the table are defined:
--------------------------------------------------------------------------------------------------------
IF OBJECT_ID('[app.udi.utility].tableColumns', 'P') IS NOT NULL DROP PROCEDURE [app.udi.utility].tableColumns
GO
CREATE PROCEDURE [app.udi.utility].tableColumns(@tableName SYSNAME) AS SET NOCOUNT ON BEGIN
	EXEC [app.udi.utility].columnsTable 1
	INSERT [app.udi.utility].[columns]
	SELECT QUOTENAME(c.[name]), '@'+ REPLACE(c.[name], ' ', '_'), 	st.[name], c.length, c.colid, ut.[name], [app.utility].removeEnclosingCharacterPair(sc.[text], DEFAULT, DEFAULT)
	FROM sys.syscolumns c
	INNER JOIN (SELECT * FROM sys.systypes WHERE (xusertype & 256) = 0) st ON st.xtype = c.xtype
	LEFT OUTER JOIN (SELECT * FROM sys.systypes WHERE (xusertype & 256) > 0) ut ON ut.xusertype = c.xusertype
	LEFT OUTER JOIN syscomments sc ON sc.id = c.cdefault
	WHERE 
	c.id = OBJECT_ID(@tableName, 'U') AND
	c.[name] NOT IN ('added', 'editor', 'edited', 'deleted', 'version')
	ORDER BY c.colid
END
GO

--------------------------------------------------------------------------------------------------------
-- This utility stored procedure produces a comma delimited string (as output parameter) from the specified
-- column of the [app.udi.utility].columns table. The specified column can be a computed column to accomplish 
-- inserting literal strings into each section of the comma delimited string.
--------------------------------------------------------------------------------------------------------
IF OBJECT_ID('[app.udi.utility].delimitedColumns', 'P') IS NOT NULL DROP PROCEDURE [app.udi.utility].delimitedColumns
GO
CREATE PROCEDURE [app.udi.utility].delimitedColumns(@columnName VARCHAR(2000), @columnList VARCHAR(max) OUTPUT, @columnFilter VARCHAR(2000), @columnDelimeter VARCHAR(10)) AS SET NOCOUNT ON BEGIN
DECLARE
	@columnValue SYSNAME,
	@csrSql NVARCHAR(4000),
	@whereClause VARCHAR(1000)
	
	SET @columnFilter = ISNULL(@columnFilter, '')
	SET @columnDelimeter = ISNULL(@columnDelimeter, ', ')
	SET @columnList = ''
	SELECT @csrSql = ('DECLARE csr CURSOR FOR SELECT ' + @columnName + ' FROM [app.udi.utility].columns ' + @columnFilter + ' ORDER BY ColumnId')
	EXEC sp_executesql @csrSql

	OPEN csr
	FETCH NEXT FROM csr INTO @columnValue
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @columnList = @columnList + @columnValue
		FETCH NEXT FROM csr INTO @columnValue
		IF @@FETCH_STATUS = 0 SET @columnList = @columnList + @columnDelimeter
	END
	CLOSE csr
	DEALLOCATE csr
END
GO

--------------------------------------------------------------------------------------------------------
-- This utility stored procedure adds a column to a table using the passed information.
--------------------------------------------------------------------------------------------------------
IF OBJECT_ID('[app.udi.utility].[column]', 'P') IS NOT NULL DROP PROCEDURE [app.udi.utility].[column]
GO
CREATE PROCEDURE [app.udi.utility].[column](@tableName SYSNAME, @columnName SYSNAME, @dataType SYSNAME, @defaultValue [app.udi.utility].DefaultValue = NULL) AS SET NOCOUNT ON BEGIN
DECLARE
	@defaultClause VARCHAR(1000)
	IF @defaultValue IS NULL SET @defaultClause = ' NULL' --Set to nullable since 
	ELSE SET @defaultClause = ' NOT NULL CONSTRAINT [DF_' + [app.utility].schemaName(@tableName) + '.' + [app.utility].tableName(@tableName) + '_' + @columnName + '] DEFAULT ' + @defaultValue + ' WITH VALUES'
	SET NOCOUNT ON;
	IF NOT EXISTS(SELECT 1 FROM sys.columns c WHERE OBJECT_ID(@tableName, 'U') = c.object_id AND c.[name] = @columnName)
	EXEC('ALTER TABLE ' + @tableName + ' ADD ' + @columnName + ' ' + @dataType + @defaultClause)
END
GO

--------------------------------------------------------------------------------------------------------
-- This utility stored procedure checks if a schema exists, and creates it if it doesn't
--------------------------------------------------------------------------------------------------------
IF OBJECT_ID('[app.udi.utility].checkSchema', 'P') IS NOT NULL DROP PROCEDURE [app.udi.utility].checkSchema
GO
CREATE PROCEDURE [app.udi.utility].checkSchema(@schemaName SYSNAME) AS SET NOCOUNT ON BEGIN 
DECLARE	@sql VARCHAR(max)
	IF SCHEMA_ID(@schemaName) IS NULL BEGIN
		SET @sql = 'CREATE SCHEMA ' + QUOTENAME(@schemaName) + ' AUTHORIZATION dbo'
		EXEC(@sql)
	END
END
GO

--------------------------------------------------------------------------------------------------------
-- This stored procedures adds the audit columns, 'deleted' and version columns
--------------------------------------------------------------------------------------------------------
IF OBJECT_ID('[app.udi].AuditColumns', 'P') IS NOT NULL DROP PROCEDURE [app.udi].AuditColumns
GO
CREATE PROCEDURE [app.udi].AuditColumns(@tableName SYSNAME) AS SET NOCOUNT ON BEGIN
	PRINT '[app.udi].AuditColumns'
	EXEC [app.udi.utility].[column] @tableName, 'added', 'DATETIME2', 'GETDATE()'
	EXEC [app.udi.utility].[column] @tableName, 'edited', 'DATETIME2'	
	EXEC [app.udi.utility].[column] @tableName, 'editor', 'VARCHAR(64)'
	EXEC [app.udi.utility].[column] @tableName, 'deleted', 'INT', '0'
	EXEC [app.udi.utility].[column] @tableName, 'version', 'ROWVERSION'
END
GO

--------------------------------------------------------------------------------------------------------
-- Creates the audit trigger
-- This trigger automatically updates the 'added', 'edited' and 'editor' columns
--------------------------------------------------------------------------------------------------------
IF OBJECT_ID('[app.udi].AuditTrigger', 'P') IS NOT NULL DROP PROCEDURE [app.udi].AuditTrigger
GO
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
GO

--------------------------------------------------------------------------------------------------------
-- Creates the delete trigger
-- This trigger when fired marks the record as deleted and cascades to child tables
--------------------------------------------------------------------------------------------------------
IF OBJECT_ID('[app.udi].DeleteTrigger', 'P') IS NOT NULL DROP PROCEDURE [app.udi].DeleteTrigger
GO
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
GO

--------------------------------------------------------------------------------------------------------
-- Creates a stock 'lookup' function for a table where columns are of type [app.udi.utility].LOOKUPID and [app.udi.utility].LOOKUPNAME
-- The 'lookup' support needs work 
--------------------------------------------------------------------------------------------------------
IF OBJECT_ID('[app.udi].LookupUDF', 'P') IS NOT NULL DROP PROCEDURE [app.udi].LookupUDF
GO
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
GO

--------------------------------------------------------------------------------------------------------
-- Creates the UDI procedure for a given table
--------------------------------------------------------------------------------------------------------
IF OBJECT_ID('[app.udi].UDI', 'P') IS NOT NULL DROP PROCEDURE [app.udi].UDI
GO
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
	SET @procName = '[dbo.udi]' + '.' + QUOTENAME([app.utility].tableName(@tableName))

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
GO

--------------------------------------------------------------------------------------------------------
-- Creates a stock view of the table filtering out records marked as 'deleted'
-- Set ignorError to true to generate a stock view on a table that doesn't have a 'deleted' column
--------------------------------------------------------------------------------------------------------
IF OBJECT_ID('[app.udi].[View]', 'P') IS NOT NULL DROP PROCEDURE [app.udi].[View]
GO
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
GO

CREATE PROCEDURE [app.udi.utility].[column](@tableName SYSNAME, @columnName SYSNAME, @dataType SYSNAME, @defaultValue [app.udi.utility].DefaultValue = NULL) AS SET NOCOUNT ON BEGIN
DECLARE
	@defaultClause VARCHAR(1000)
	IF @defaultValue IS NULL SET @defaultClause = ' NULL' --Set to nullable since 
	ELSE SET @defaultClause = ' NOT NULL CONSTRAINT [DF_' + [app.utility].schemaName(@tableName) + '.' + [app.utility].tableName(@tableName) + '_' + @columnName + '] DEFAULT ' + @defaultValue + ' WITH VALUES'
	SET NOCOUNT ON;
	IF NOT EXISTS(SELECT 1 FROM sys.columns c WHERE OBJECT_ID(@tableName, 'U') = c.object_id AND c.[name] = @columnName)
	EXEC('ALTER TABLE ' + @tableName + ' ADD ' + @columnName + ' ' + @dataType + @defaultClause)
END
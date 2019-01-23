CREATE PROCEDURE [app.udi.utility].checkSchema(@schemaName SYSNAME) AS SET NOCOUNT ON BEGIN 
DECLARE	@sql VARCHAR(max)
	IF SCHEMA_ID(@schemaName) IS NULL BEGIN
		SET @sql = 'CREATE SCHEMA ' + QUOTENAME(@schemaName) + ' AUTHORIZATION dbo'
		EXEC(@sql)
	END
END
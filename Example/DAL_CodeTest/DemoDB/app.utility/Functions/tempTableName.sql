CREATE FUNCTION [app.utility].tempTableName(@tableName SYSNAME) RETURNS SYSNAME AS
BEGIN
	RETURN  '#' + [app.utility].unquoteName(RIGHT(@tableName, LEN(@tableName) - CHARINDEX('.', @tableName)), NULL);
END
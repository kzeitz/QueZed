CREATE PROCEDURE [app.udi.utility].columnsTable(@createTable BIT) AS SET NOCOUNT ON BEGIN
	EXEC('IF OBJECT_ID(''[app.udi.utility].[columns]'', ''U'') IS NOT NULL DROP TABLE [app.udi.utility].[columns]')
	IF (1 = @createTable) EXEC('CREATE TABLE [app.udi.utility].[columns] (ColumnName SYSNAME, ParameterName SYSNAME, ColumnTypeName SYSNAME, ColumnLength INT, ColumnId INT, ColumnUserTypeName SYSNAME NULL, ColumnDefaultValue VARCHAR(max) NULL)')
END
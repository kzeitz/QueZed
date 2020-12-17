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
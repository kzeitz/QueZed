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
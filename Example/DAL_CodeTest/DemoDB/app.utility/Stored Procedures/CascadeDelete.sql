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
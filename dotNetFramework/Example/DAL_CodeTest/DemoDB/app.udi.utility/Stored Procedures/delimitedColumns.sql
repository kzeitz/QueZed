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
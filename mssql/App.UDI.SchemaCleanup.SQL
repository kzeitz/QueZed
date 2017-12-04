-- cleanup old names
IF OBJECT_ID('dal.CascadeDelete','P') IS NOT NULL DROP PROCEDURE dal.CascadeDelete
IF OBJECT_ID('gen.tableName','FN') IS NOT NULL DROP FUNCTION gen.tableName
IF OBJECT_ID('gen.schemaName','FN') IS NOT NULL DROP FUNCTION gen.schemaName
IF OBJECT_ID('gen.databaseName','FN') IS NOT NULL DROP FUNCTION gen.databaseName
IF OBJECT_ID('gen.serverName','FN') IS NOT NULL DROP FUNCTION gen.serverName
IF OBJECT_ID('gen.trim','FN') IS NOT NULL DROP FUNCTION gen.trim
IF OBJECT_ID('gen.stripPattern','FN') IS NOT NULL DROP FUNCTION gen.stripPattern
IF OBJECT_ID('gen.BuildTempTableName','FN') IS NOT NULL	DROP FUNCTION gen.BuildTempTableName
IF OBJECT_ID('gen.getPKColumns','TF') IS NOT NULL DROP FUNCTION gen.getPKColumns
IF OBJECT_ID('gen.TableColumns','U') IS NOT NULL DROP TABLE gen.TableColumns
IF OBJECT_ID('gen.fillTableColumns','P') IS NOT NULL DROP PROCEDURE gen.fillTableColumns
IF OBJECT_ID('gen.columnCommaDelimited','P') IS NOT NULL DROP PROCEDURE gen.columnCommaDelimited
IF OBJECT_ID('gen.addColumn','P') IS NOT NULL DROP PROCEDURE gen.addColumn
IF OBJECT_ID('gen.AddAuditColumns','P') IS NOT NULL DROP PROCEDURE gen.AddAuditColumns
IF OBJECT_ID('gen.AddUDI','P') IS NOT NULL DROP PROCEDURE gen.AddUDI
IF OBJECT_ID('gen.AddView','P') IS NOT NULL	DROP PROCEDURE gen.AddView
IF OBJECT_ID('gen.AddAuditTriggers','P') IS NOT NULL DROP PROCEDURE gen.AddAuditTriggers
IF OBJECT_ID('gen.AddDeleteTrigger','P') IS NOT NULL DROP PROCEDURE gen.AddDeleteTrigger
IF OBJECT_ID('gen.AddLookupUDF','P') IS NOT NULL DROP PROCEDURE gen.AddLookupUDF
GO
IF SCHEMA_ID('gen') IS NOT NULL EXEC('DROP SCHEMA gen')
GO
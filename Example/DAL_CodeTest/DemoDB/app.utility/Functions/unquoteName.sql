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
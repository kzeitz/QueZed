CREATE FUNCTION [app.utility].removeEnclosingCharacterPair(@string VARCHAR(max), @open CHAR = '(', @close CHAR = ')') RETURNS VARCHAR(max) AS
BEGIN
	DECLARE @ret VARCHAR(max)
	IF (LEFT(@string, 1) = @open AND RIGHT(@string, 1) = @close)
		SET @ret = [app.utility].removeEnclosingCharacterPair(SUBSTRING(@string, 2, LEN(@string) - 2), @open, @close)
	ELSE SET @ret = @string
	RETURN @ret	
END
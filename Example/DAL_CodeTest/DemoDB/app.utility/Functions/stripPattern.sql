CREATE FUNCTION [app.utility].stripPattern(@Input VARCHAR(max), @Pattern VARCHAR(100)) RETURNS VARCHAR(max) AS
BEGIN
	WHILE PATINDEX(@Pattern, @Input) != 0
		BEGIN
			SET @Input = REPLACE(@Input, SUBSTRING(@Input, PATINDEX(@Pattern, @Input), 1), '')
		END
	RETURN @Input
END
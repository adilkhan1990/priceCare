-- =============================================
-- Author:		Jonathan Hubeau
-- Create date: 2014-10-24
-- Description:	Get list of dimension types
-- =============================================
CREATE FUNCTION GetDimensionType
(	
	@Dimension nvarchar(max)
)
RETURNS @DimensionType TABLE 
(
	Id int,
	Name nvarchar(max),
	ShortName nvarchar(max)
)
AS
BEGIN
	INSERT INTO @DimensionType
	(
		Id,
		Name,
		ShortName
	)
	SELECT
		Id,
		Name,
		ShortName
	FROM
		DimensionType WHERE Dimension=@Dimension
RETURN 
END
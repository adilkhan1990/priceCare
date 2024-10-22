-- =============================================
-- Author:		Jonathan Hubeau
-- Create date: 2014-10-23
-- Description:	Get Id for not-specified dimension type of specified dimension
-- =============================================
CREATE FUNCTION GetNotSpecifiedDimensionId
(
	@Dimension nvarchar(max)
)
RETURNS int
AS
BEGIN
	DECLARE @DimensionId int
	SELECT @DimensionId=Id from DimensionType WHERE Dimension=@Dimension AND ShortName='XX'
	RETURN @DimensionId
END
-- =============================================
-- Author:		Jonathan Hubeau
-- Create date: 2014-10-23
-- Description:	Get Id for save type (data or forecast)
-- =============================================
CREATE FUNCTION GetSaveTypeId
(
	@SaveType nvarchar(max)
)
RETURNS int
AS
BEGIN
	DECLARE @SaveTypeId int
	SELECT @SaveTypeId=Id from DimensionType WHERE Dimension='Save' AND ShortName=@SaveType
	RETURN @SaveTypeId
END
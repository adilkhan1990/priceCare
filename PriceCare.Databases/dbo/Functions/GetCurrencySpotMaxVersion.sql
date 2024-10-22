-- =============================================
-- Author:		Jonathan Hubeau
-- Create date: 2014-10-16
-- Description:	Get GprmBasket for specific version and dimensions
-- =============================================
CREATE FUNCTION [dbo].[GetCurrencySpotMaxVersion]
(	
	@Date DateTime
)
RETURNS @GprmBasket TABLE
(
	CurrencyId int,
	VersionId int
)
AS
BEGIN
	
	insert into @GprmBasket
	(CurrencyId,
	 VersionId
	)

	SELECT CS.[CurrencyId]
         ,max([VersionId])
    FROM [dbo].[CurrencySpot] CS,
       [dbo].[Version] V
    where V.Id=CS.VersionId and 
	      V.VersionTime<=@Date
    group by [CurrencyId]

	RETURN
END
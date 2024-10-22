-- =============================================
-- Author:		Jonathan Hubeau
-- Create date: 2014-10-16
-- Description:	Get list of PriceMap versions
-- =============================================
CREATE FUNCTION [dbo].[GetPriceMapVersion]
(	
	@geographyId int,
	@productId int,
	@gprmRuleTypeId int
)
RETURNS @GprmRuleVersion TABLE
(
	VersionId int,
	Information nvarchar(max),
	VersionData nvarchar(max),
	VersionTime datetime,
	UserName nvarchar(max)
)
AS
BEGIN
	DECLARE @saveTypeId int
	DECLARE @DefaultProduct int
	DECLARE @Version0 TABLE
	(
		VersionId int,
		VersionData nvarchar(max)
	)
	DECLARE @Version TABLE
	(
		VersionId int,
		VersionData nvarchar(max)
	)
	
	SET @saveTypeId=dbo.GetSaveTypeId('Data')
	SELECT @DefaultProduct=Id from Product WHERE ShortName='XX'
	
	INSERT INTO @Version0 (VersionId,VersionData)
	SELECT DISTINCT VersionId,'Reviewed Price Map' AS VersionData FROM GprmReviewedPrice
	WHERE
		SaveTypeId=@SaveTypeId AND
		GeographyId=CASE @geographyId WHEN 0 THEN GeographyId ELSE @geographyId END AND
		(ProductId=@DefaultProduct OR ProductId=CASE @productId WHEN 0 THEN ProductId ELSE @productId END) AND
		GprmRuleTypeId=CASE @gprmRuleTypeId WHEN 0 THEN GprmRuleTypeId ELSE @gprmRuleTypeId END
	UNION ALL
	SELECT DISTINCT VersionId,'Referenced Price Map' AS VersionData FROM GprmReferencedPrice
	WHERE
		SaveTypeId=@SaveTypeId AND
		GeographyId=CASE @geographyId WHEN 0 THEN GeographyId ELSE @geographyId END AND
		(ProductId=@DefaultProduct OR ProductId=CASE @productId WHEN 0 THEN ProductId ELSE @productId END) AND
		GprmRuleTypeId=CASE @gprmRuleTypeId WHEN 0 THEN GprmRuleTypeId ELSE @gprmRuleTypeId END
	
	INSERT INTO @Version (VersionId,VersionData)
	SELECT
		V.VersionId,
		STUFF((SELECT ', '+VersionData FROM @Version0 WHERE VersionId=V.VersionId FOR XML PATH('')),1,1,'')
	FROM @Version0 AS V
	GROUP BY V.VersionId

	INSERT @GprmRuleVersion
	(
		VersionId,
		Information,
		VersionData,
		VersionTime,
		UserName
	)
	SELECT
		V.VersionId,
		VDb.Information,
		V.VersionData,
		VDb.VersionTime,
		U.FirstName + ' ' + U.LastName
	FROM
		@Version AS V
			INNER JOIN Version AS VDb ON V.VersionId=VDb.Id
			INNER JOIN [PriceCare.Databases.Users].[dbo].[AspNetUsers] AS U ON VDb.UserId=U.Id
	
	RETURN
END
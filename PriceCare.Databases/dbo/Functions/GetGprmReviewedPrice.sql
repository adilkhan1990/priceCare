-- =============================================
-- Author:		Jonathan Hubeau
-- Create date: 2014-10-16
-- Description:	Get GprmReviewedPrice for specific version and dimensions
-- =============================================
CREATE FUNCTION [dbo].[GetGprmReviewedPrice]
(	
	@isData bit,
	@saveId int,
	@geographyId int,
	@productId int,
	@gprmRuleTypeId int,
	@versionId int
)
RETURNS @GprmReviewedPrice TABLE
(
	Geography nvarchar(max),
	ReviewedPriceType nvarchar(max),
	ApplicableFrom datetime,
	GprmRuleTypeId int,
	GeographyId int,
	ProductId int,
	ReviewedPriceTypeId int,
	ReviewedPriceAdjustment float
)
AS
BEGIN
	DECLARE @saveTypeId int
	DECLARE @DefaultProduct int
	DECLARE @GprmReviewedPriceIdDefault TABLE (Id int)
	DECLARE @GprmReviewedPriceIdProduct TABLE (Id int)
	DECLARE @GprmReviewedPriceTmpDefault TABLE
	(
		Id int IDENTITY(1,1),
		GeographyId int,
		ProductId int,
		GprmRuleTypeId int,
		ReviewedPriceTypeId int,
		ReviewedPriceAdjustment float,
		VersionId int,
		Active bit,
		ApplicableFrom datetime
	)
	DECLARE @GprmReviewedPriceTmpProduct TABLE
	(
		Id int IDENTITY(1,1),
		GeographyId int,
		ProductId int,
		GprmRuleTypeId int,
		ReviewedPriceTypeId int,
		ReviewedPriceAdjustment float,
		VersionId int,
		Active bit,
		ApplicableFrom datetime
	)

	SELECT @DefaultProduct=Id from Product WHERE ShortName='XX'

	IF (@isData=1) BEGIN SET @saveTypeId=dbo.GetSaveTypeId('Data') END ELSE BEGIN SET @saveTypeId=dbo.GetSaveTypeId('Forecast') END

	IF (@versionId=0) BEGIN SELECT @versionId=MAX(Id) FROM dbo.Version END

	INSERT INTO @GprmReviewedPriceTmpDefault
	(
		GeographyId,
		ProductId,
		GprmRuleTypeId,
		ReviewedPriceTypeId,
		ReviewedPriceAdjustment,
		VersionId,
		Active,
		ApplicableFrom
	)
	SELECT
		GeographyId,
		0,
		GprmRuleTypeId,
		ReviewedPriceTypeId,
		ReviewedPriceAdjustment,
		VersionId,
		Active,
		ApplicableFrom
	FROM
		dbo.GprmReviewedPrice
	WHERE
		SaveTypeId=@saveTypeId AND
		SaveId=CASE @saveId WHEN 0 THEN SaveId ELSE @saveId END AND
		GeographyId=CASE @geographyId WHEN 0 THEN GeographyId ELSE @geographyId END AND
		ProductId=@DefaultProduct AND
		GprmRuleTypeId=CASE @gprmRuleTypeId WHEN 0 THEN GprmRuleTypeId ELSE @gprmRuleTypeId END AND
		VersionId<=@versionId
	ORDER BY
		VersionId
	
	INSERT INTO @GprmReviewedPriceTmpProduct
	(
		GeographyId,
		ProductId,
		GprmRuleTypeId,
		ReviewedPriceTypeId,
		ReviewedPriceAdjustment,
		VersionId,
		Active,
		ApplicableFrom
	)
	SELECT
		GeographyId,
		ProductId,
		GprmRuleTypeId,
		ReviewedPriceTypeId,
		ReviewedPriceAdjustment,
		VersionId,
		Active,
		ApplicableFrom
	FROM
		dbo.GprmReviewedPrice
	WHERE
		SaveTypeId=@saveTypeId AND
		SaveId=CASE @saveId WHEN 0 THEN SaveId ELSE @saveId END AND
		GeographyId=CASE @geographyId WHEN 0 THEN GeographyId ELSE @geographyId END AND
		ProductId<>@DefaultProduct AND ProductId=CASE @productId WHEN 0 THEN ProductId ELSE @productId END AND
		GprmRuleTypeId=CASE @gprmRuleTypeId WHEN 0 THEN GprmRuleTypeId ELSE @gprmRuleTypeId END AND
		VersionId<=@versionId
	ORDER BY
		VersionId

	INSERT INTO @GprmReviewedPriceIdDefault (Id)
	SELECT MAX(Id)
	FROM @GprmReviewedPriceTmpDefault
	GROUP BY
		GeographyId,
		ProductId,
		GprmRuleTypeId,
		ApplicableFrom

	INSERT INTO @GprmReviewedPriceIdProduct (Id)
	SELECT MAX(Id)
	FROM @GprmReviewedPriceTmpProduct
	GROUP BY
		GeographyId,
		ProductId,
		GprmRuleTypeId,
		ApplicableFrom
	
	INSERT INTO @GprmReviewedPrice
	(
		Geography,
		ReviewedPriceType,
		ApplicableFrom,
		GprmRuleTypeId,
		GeographyId,
		ProductId,
		ReviewedPriceTypeId,
		ReviewedPriceAdjustment
	)
	SELECT
		G.Name,
		PT.ShortName,
		ApplicableFrom,
		GprmRuleTypeId,
		GeographyId,
		ProductId,
		ReviewedPriceTypeId,
		ReviewedPriceAdjustment
	FROM
		@GprmReviewedPriceTmpDefault AS GRP INNER JOIN @GprmReviewedPriceIdDefault AS V ON GRP.Id=V.Id
		INNER JOIN Geography AS G ON GRP.GeographyId=G.Id
		INNER JOIN PriceType AS PT ON GRP.ReviewedPriceTypeId=PT.Id
	WHERE
		GRP.Active=1
	
	UNION ALL

	SELECT
		G.Name,
		PT.ShortName,
		ApplicableFrom,
		GprmRuleTypeId,
		GeographyId,
		ProductId,
		ReviewedPriceTypeId,
		ReviewedPriceAdjustment
	FROM
		@GprmReviewedPriceTmpProduct AS GRP INNER JOIN @GprmReviewedPriceIdProduct AS V ON GRP.Id=V.Id
		INNER JOIN Geography AS G ON GRP.GeographyId=G.Id
		INNER JOIN PriceType AS PT ON GRP.ReviewedPriceTypeId=PT.Id
	WHERE
		GRP.Active=1
	RETURN
END
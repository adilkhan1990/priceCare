-- =============================================
-- Author:		Jonathan Hubeau
-- Create date: 2014-10-16
-- Description:	Get GprmReferencedPrice for specific version and dimensions
-- =============================================
CREATE FUNCTION [dbo].[GetGprmReferencedPrice]
(	
	@isData bit,
	@saveId int,
	@geographyId int,
	@productId int,
	@gprmRuleTypeId int,
	@versionId int
)
RETURNS @GprmReferencedPrice TABLE
(
	Geography nvarchar(max),
	ReferencedGeography nvarchar(max),
	ReferencedPriceType nvarchar(max),
	ApplicableFrom datetime,
	GprmRuleTypeId int,
	GeographyId int,
	ProductId int,
	SubRuleIndex int,
	ReferencedGeographyId int,
	ReferencedPriceTypeId int,
	ReferencedPriceAdjustment float
)
AS
BEGIN
	DECLARE @saveTypeId int
	DECLARE @DefaultProduct int
	DECLARE @GprmReferencedPriceIdDefault TABLE (Id int)
	DECLARE @GprmReferencedPriceIdProduct TABLE (Id int)
	DECLARE @GprmReferencedPriceTmpDefault TABLE
	(
		Id int IDENTITY(1,1),
		GeographyId int,
		ProductId int,
		SubRuleIndex int,
		GprmRuleTypeId int,
		ReferencedGeographyId int,
		ReferencedPriceTypeId int,
		ReferencedPriceAdjustment float,
		VersionId int,
		Active bit,
		ApplicableFrom datetime
	)
	DECLARE @GprmReferencedPriceTmpProduct TABLE
	(
		Id int IDENTITY(1,1),
		GeographyId int,
		ProductId int,
		SubRuleIndex int,
		GprmRuleTypeId int,
		ReferencedGeographyId int,
		ReferencedPriceTypeId int,
		ReferencedPriceAdjustment float,
		VersionId int,
		Active bit,
		ApplicableFrom datetime
	)

	SELECT @DefaultProduct=Id from Product WHERE ShortName='XX'

	IF (@isData=1) BEGIN SET @saveTypeId=dbo.GetSaveTypeId('Data') END ELSE BEGIN SET @saveTypeId=dbo.GetSaveTypeId('Forecast') END

	IF (@versionId=0) BEGIN SELECT @versionId=MAX(Id) FROM dbo.Version END

	INSERT INTO @GprmReferencedPriceTmpDefault
	(
		GeographyId,
		ProductId,
		SubRuleIndex,
		GprmRuleTypeId,
		ReferencedGeographyId,
		ReferencedPriceTypeId,
		ReferencedPriceAdjustment,
		VersionId,
		Active,
		ApplicableFrom
	)
	SELECT
		GeographyId,
		0,
		SubRuleIndex,
		GprmRuleTypeId,
		ReferencedGeographyId,
		ReferencedPriceTypeId,
		ReferencedPriceAdjustment,
		VersionId,
		Active,
		ApplicableFrom
	FROM
		dbo.GprmReferencedPrice
	WHERE
		SaveTypeId=@saveTypeId AND
		SaveId=CASE @saveId WHEN 0 THEN SaveId ELSE @saveId END AND
		GeographyId=CASE @geographyId WHEN 0 THEN GeographyId ELSE @geographyId END AND
		ProductId=@DefaultProduct AND
		GprmRuleTypeId=CASE @gprmRuleTypeId WHEN 0 THEN GprmRuleTypeId ELSE @gprmRuleTypeId END AND
		VersionId<=@versionId
	ORDER BY
		VersionId
	
	INSERT INTO @GprmReferencedPriceTmpProduct
	(
		GeographyId,
		ProductId,
		SubRuleIndex,
		GprmRuleTypeId,
		ReferencedGeographyId,
		ReferencedPriceTypeId,
		ReferencedPriceAdjustment,
		VersionId,
		Active,
		ApplicableFrom
	)
	SELECT
		GeographyId,
		ProductId,
		SubRuleIndex,
		GprmRuleTypeId,
		ReferencedGeographyId,
		ReferencedPriceTypeId,
		ReferencedPriceAdjustment,
		VersionId,
		Active,
		ApplicableFrom
	FROM
		dbo.GprmReferencedPrice
	WHERE
		SaveTypeId=@saveTypeId AND
		SaveId=CASE @saveId WHEN 0 THEN SaveId ELSE @saveId END AND
		GeographyId=CASE @geographyId WHEN 0 THEN GeographyId ELSE @geographyId END AND
		ProductId<>@DefaultProduct AND ProductId=CASE @productId WHEN 0 THEN ProductId ELSE @productId END AND
		GprmRuleTypeId=CASE @gprmRuleTypeId WHEN 0 THEN GprmRuleTypeId ELSE @gprmRuleTypeId END AND
		VersionId<=@versionId
	ORDER BY
		VersionId

	INSERT INTO @GprmReferencedPriceIdDefault (Id)
	SELECT MAX(Id)
	FROM @GprmReferencedPriceTmpDefault
	GROUP BY
		GeographyId,
		ProductId,
		SubRuleIndex,
		GprmRuleTypeId,
		ReferencedGeographyId,
		ApplicableFrom

	INSERT INTO @GprmReferencedPriceIdProduct (Id)
	SELECT MAX(Id)
	FROM @GprmReferencedPriceTmpProduct
	GROUP BY
		GeographyId,
		ProductId,
		SubRuleIndex,
		GprmRuleTypeId,
		ReferencedGeographyId,
		ApplicableFrom
	
	INSERT INTO @GprmReferencedPrice
	(
		Geography,
		ReferencedGeography,
		ReferencedPriceType,
		ApplicableFrom,
		GprmRuleTypeId,
		GeographyId,
		ProductId,
		SubRuleIndex,
		ReferencedGeographyId,
		ReferencedPriceTypeId,
		ReferencedPriceAdjustment
	)
	SELECT
		G.Name,
		RG.Name,
		PT.ShortName,
		ApplicableFrom,
		GprmRuleTypeId,
		GeographyId,
		ProductId,
		SubRuleIndex,
		ReferencedGeographyId,
		ReferencedPriceTypeId,
		ReferencedPriceAdjustment
	FROM
		@GprmReferencedPriceTmpDefault AS GRP INNER JOIN @GprmReferencedPriceIdDefault AS V ON GRP.Id=V.Id
		INNER JOIN Geography AS G ON GRP.GeographyId=G.Id
		INNER JOIN PriceType AS PT ON GRP.ReferencedPriceTypeId=PT.Id
		INNER JOIN Geography AS RG ON GRP.ReferencedGeographyId=RG.Id
	WHERE
		GRP.Active=1
	
	UNION ALL

	SELECT
		G.Name,
		RG.Name,
		PT.ShortName,
		ApplicableFrom,
		GprmRuleTypeId,
		GeographyId,
		ProductId,
		SubRuleIndex,
		ReferencedGeographyId,
		ReferencedPriceTypeId,
		ReferencedPriceAdjustment
	FROM
		@GprmReferencedPriceTmpProduct AS GRP INNER JOIN @GprmReferencedPriceIdProduct AS V ON GRP.Id=V.Id
		INNER JOIN Geography AS G ON GRP.GeographyId=G.Id
		INNER JOIN PriceType AS PT ON GRP.ReferencedPriceTypeId=PT.Id
		INNER JOIN Geography AS RG ON GRP.ReferencedGeographyId=RG.Id
	WHERE
		GRP.Active=1
	RETURN
END
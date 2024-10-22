-- =============================================
-- Author:		Jonathan Hubeau
-- Create date: 2014-10-16
-- Description:	Get GprmBasket for specific version and dimensions
-- =============================================
CREATE FUNCTION [dbo].[GetGprmBasket]
(	
	@isData bit,
	@saveId int,
	@geographyId int,
	@productId int,
	@gprmRuleTypeId int,
	@versionId int
)
RETURNS @GprmBasket TABLE
(
	Geography nvarchar(max),
	ReferencedGeography nvarchar(max),
	ApplicableFrom datetime,
	GprmRuleTypeId int,
	GeographyId int,
	ProductId int,
	SubRuleIndex int,
	ReferencedGeographyId int
)
AS
BEGIN
	DECLARE @saveTypeId int
	DECLARE @DefaultProduct int
	DECLARE @GprmBasketIdDefault TABLE (Id int)
	DECLARE @GprmBasketIdProduct TABLE (Id int)
	DECLARE @GprmBasketTmpDefault TABLE
	(
		Id int IDENTITY(1,1),
		GeographyId int,
		ProductId int,
		SubRuleIndex int,
		GprmRuleTypeId int,
		ReferencedGeographyId int,
		VersionId int,
		Active bit,
		ApplicableFrom datetime
	)
	DECLARE @GprmBasketTmpProduct TABLE
	(
		Id int IDENTITY(1,1),
		GeographyId int,
		ProductId int,
		SubRuleIndex int,
		GprmRuleTypeId int,
		ReferencedGeographyId int,
		VersionId int,
		Active bit,
		ApplicableFrom datetime
	)

	SELECT @DefaultProduct=Id from Product WHERE ShortName='XX'

	IF (@isData=1) BEGIN SET @saveTypeId=dbo.GetSaveTypeId('Data') END ELSE BEGIN SET @saveTypeId=dbo.GetSaveTypeId('Forecast') END
	
	IF (@versionId=0) BEGIN SELECT @versionId=MAX(Id) FROM dbo.Version END

	INSERT INTO @GprmBasketTmpDefault
	(
		GeographyId,
		ProductId,
		SubRuleIndex,
		GprmRuleTypeId,
		ReferencedGeographyId,
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
		VersionId,
		Active,
		ApplicableFrom
	FROM
		dbo.GprmBasket
	WHERE
		SaveTypeId=@saveTypeId AND
		SaveId=CASE @saveId WHEN 0 THEN SaveId ELSE @saveId END AND
		GeographyId=CASE @geographyId WHEN 0 THEN GeographyId ELSE @geographyId END AND
		ProductId=@DefaultProduct AND
		GprmRuleTypeId=CASE @gprmRuleTypeId WHEN 0 THEN GprmRuleTypeId ELSE @gprmRuleTypeId END AND
		VersionId<=@versionId
	ORDER BY
		VersionId
	
	INSERT INTO @GprmBasketTmpProduct
	(
		GeographyId,
		ProductId,
		SubRuleIndex,
		GprmRuleTypeId,
		ReferencedGeographyId,
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
		VersionId,
		Active,
		ApplicableFrom
	FROM
		dbo.GprmBasket
	WHERE
		SaveTypeId=@saveTypeId AND
		SaveId=CASE @saveId WHEN 0 THEN SaveId ELSE @saveId END AND
		GeographyId=CASE @geographyId WHEN 0 THEN GeographyId ELSE @geographyId END AND
		ProductId<>@DefaultProduct AND ProductId=CASE @productId WHEN 0 THEN ProductId ELSE @productId END AND
		GprmRuleTypeId=CASE @gprmRuleTypeId WHEN 0 THEN GprmRuleTypeId ELSE @gprmRuleTypeId END AND
		VersionId<=@versionId
	ORDER BY
		VersionId

	INSERT INTO @GprmBasketIdDefault (Id)
	SELECT MAX(Id)
	FROM @GprmBasketTmpDefault
	GROUP BY
		GeographyId,
		ProductId,
		SubRuleIndex,
		GprmRuleTypeId,
		ReferencedGeographyId,
		ApplicableFrom

	INSERT INTO @GprmBasketIdProduct (Id)
	SELECT MAX(Id)
	FROM @GprmBasketTmpProduct
	GROUP BY
		GeographyId,
		ProductId,
		SubRuleIndex,
		GprmRuleTypeId,
		ReferencedGeographyId,
		ApplicableFrom
	
	INSERT INTO @GprmBasket
	(
		Geography,
		ReferencedGeography,
		ApplicableFrom,
		GprmRuleTypeId,
		GeographyId,
		ProductId,
		SubRuleIndex,
		ReferencedGeographyId
	)
	SELECT
		G.Name,
		RG.Name,
		ApplicableFrom,
		GprmRuleTypeId,
		GeographyId,
		ProductId,
		SubRuleIndex,
		ReferencedGeographyId
	FROM
		@GprmBasketTmpDefault AS GRP INNER JOIN @GprmBasketIdDefault AS V ON GRP.Id=V.Id
		INNER JOIN Geography AS G ON GRP.GeographyId=G.Id
		INNER JOIN Geography AS RG ON GRP.ReferencedGeographyId=RG.Id
	WHERE
		GRP.Active=1
	
	UNION ALL

	SELECT
		G.Name,
		RG.Name,
		ApplicableFrom,
		GprmRuleTypeId,
		GeographyId,
		ProductId,
		SubRuleIndex,
		ReferencedGeographyId
	FROM
		@GprmBasketTmpProduct AS GRP INNER JOIN @GprmBasketIdProduct AS V ON GRP.Id=V.Id
		INNER JOIN Geography AS G ON GRP.GeographyId=G.Id
		INNER JOIN Geography AS RG ON GRP.ReferencedGeographyId=RG.Id
	WHERE
		GRP.Active=1
	RETURN
END
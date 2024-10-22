-- =============================================
-- Author:		Jonathan Hubeau
-- Create date: 2014-10-16
-- Description:	Get GprmSubRule for specific version and dimensions
-- =============================================
CREATE FUNCTION [dbo].[GetGprmSubRule]
(	
	@isData bit,
	@saveId int,
	@geographyId int,
	@productId int,
	@GprmSubRuleTypeId int,
	@versionId int
)
RETURNS @GprmSubRule TABLE
(
	ApplicableFrom datetime,
	GeographyId int,
	ProductId int,
	SubRuleIndex int,
	GprmMathId int,
	Argument int,
	WeightTypeId int,
	GprmRuleTypeId int
)
AS
BEGIN
	DECLARE @saveTypeId int
	DECLARE @DefaultProduct int
	DECLARE @GprmSubRuleIdDefault TABLE (Id int)
	DECLARE @GprmSubRuleIdProduct TABLE (Id int)
	DECLARE @GprmSubRuleTmpDefault TABLE
	(
		Id int IDENTITY(1,1),
		GeographyId int,
		ProductId int,
		SubRuleIndex int,
		GprmMathId int,
		Argument int,
		WeightTypeId int,
		GprmRuleTypeId int,
		SaveTypeId int,
		SaveId int,
		VersionId int,
		Active bit,
		ApplicableFrom datetime
	)
	DECLARE @GprmSubRuleTmpProduct TABLE
	(
		Id int IDENTITY(1,1),
		GeographyId int,
		ProductId int,
		SubRuleIndex int,
		GprmMathId int,
		Argument int,
		WeightTypeId int,
		GprmRuleTypeId int,
		SaveTypeId int,
		SaveId int,
		VersionId int,
		Active bit,
		ApplicableFrom datetime
	)

	SELECT @DefaultProduct=Id from Product WHERE ShortName='XX'

	IF (@isData=1) BEGIN SET @saveTypeId=dbo.GetSaveTypeId('Data') END ELSE BEGIN SET @saveTypeId=dbo.GetSaveTypeId('Forecast') END
	
	IF (@versionId=0) BEGIN SELECT @versionId=MAX(Id) FROM dbo.Version END

	INSERT INTO @GprmSubRuleTmpDefault
	(
		GeographyId,
		ProductId,
		SubRuleIndex,
		GprmMathId,
		Argument,
		WeightTypeId,
		GprmRuleTypeId,
		SaveTypeId,
		SaveId,
		VersionId,
		Active,
		ApplicableFrom
	)
	SELECT
		GeographyId,
		0,
		SubRuleIndex,
		GprmMathId,
		Argument,
		WeightTypeId,
		GprmRuleTypeId,
		SaveTypeId,
		SaveId,
		VersionId,
		Active,
		ApplicableFrom
	FROM
		dbo.GprmSubRule
	WHERE
		SaveTypeId=@saveTypeId AND
		SaveId=CASE @saveId WHEN 0 THEN SaveId ELSE @saveId END AND
		GeographyId=CASE @geographyId WHEN 0 THEN GeographyId ELSE @geographyId END AND
		GprmRuleTypeId=CASE @GprmSubRuleTypeId WHEN 0 THEN GprmRuleTypeId ELSE @GprmSubRuleTypeId END AND
		ProductId=@DefaultProduct AND
		VersionId<=@versionId
	ORDER BY
		VersionId
	
	INSERT INTO @GprmSubRuleTmpProduct
	(
		GeographyId,
		ProductId,
		SubRuleIndex,
		GprmMathId,
		Argument,
		WeightTypeId,
		GprmRuleTypeId,
		SaveTypeId,
		SaveId,
		VersionId,
		Active,
		ApplicableFrom
	)
	SELECT
		GeographyId,
		ProductId,
		SubRuleIndex,
		GprmMathId,
		Argument,
		WeightTypeId,
		GprmRuleTypeId,
		SaveTypeId,
		SaveId,
		VersionId,
		Active,
		ApplicableFrom
	FROM
		dbo.GprmSubRule
	WHERE
		SaveTypeId=@saveTypeId AND
		SaveId=CASE @saveId WHEN 0 THEN SaveId ELSE @saveId END AND
		GeographyId=CASE @geographyId WHEN 0 THEN GeographyId ELSE @geographyId END AND
		ProductId<>@DefaultProduct AND ProductId=CASE @productId WHEN 0 THEN ProductId ELSE @productId END AND
		GprmRuleTypeId=CASE @GprmSubRuleTypeId WHEN 0 THEN GprmRuleTypeId ELSE @GprmSubRuleTypeId END AND
		VersionId<=@versionId
	ORDER BY
		VersionId

	INSERT INTO @GprmSubRuleIdDefault (Id)
	SELECT MAX(Id)
	FROM @GprmSubRuleTmpDefault
	GROUP BY
		GeographyId,
		ProductId,
		SubRuleIndex,
		GprmRuleTypeId,
		ApplicableFrom

	INSERT INTO @GprmSubRuleIdProduct (Id)
	SELECT MAX(Id)
	FROM @GprmSubRuleTmpProduct
	GROUP BY
		GeographyId,
		ProductId,
		SubRuleIndex,
		GprmRuleTypeId,
		ApplicableFrom
	
	INSERT INTO @GprmSubRule
	(
		ApplicableFrom,
		GeographyId,
		ProductId,
		SubRuleIndex,
		GprmMathId,
		Argument,
		WeightTypeId,
		GprmRuleTypeId
	)
	SELECT
		ApplicableFrom,
		GeographyId,
		ProductId,
		SubRuleIndex,
		GprmMathId,
		Argument,
		WeightTypeId,
		GprmRuleTypeId
	FROM
		@GprmSubRuleTmpDefault AS GR INNER JOIN @GprmSubRuleIdDefault AS V ON GR.Id=V.Id
	WHERE
		Active=1
	
	UNION ALL

	SELECT
		ApplicableFrom,
		GeographyId,
		ProductId,
		SubRuleIndex,
		GprmMathId,
		Argument,
		WeightTypeId,
		GprmRuleTypeId
	FROM
		@GprmSubRuleTmpProduct AS GR INNER JOIN @GprmSubRuleIdProduct AS V ON GR.Id=V.Id
	WHERE
		Active=1
	RETURN
END
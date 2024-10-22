-- =============================================
-- Author:		Jonathan Hubeau
-- Create date: 2014-10-16
-- Description:	Get GprmRule for specific version and dimensions
-- =============================================
CREATE FUNCTION [dbo].[GetGprmRule]
(	
	@isData bit,
	@saveId int,
	@geographyId int,
	@productId int,
	@gprmRuleTypeId int,
	@versionId int
)
RETURNS @GprmRule TABLE
(
	ApplicableFrom datetime,
	GeographyId int,
	ProductId int,
	Regular bit,
	GprmMathId int,
	Argument int,
	WeightTypeId int,
	IrpRuleListId int,
	GprmRuleTypeId int,
	LookBack int,
	EffectiveLag int,
	AllowIncrease bit
)
AS
BEGIN
	DECLARE @saveTypeId int
	DECLARE @DefaultProduct int
	DECLARE @GprmRuleIdDefault TABLE (Id int)
	DECLARE @GprmRuleIdProduct TABLE (Id int)
	DECLARE @GprmRuleTmpDefault TABLE
	(
		Id int IDENTITY(1,1),
		GeographyId int,
		ProductId int,
		Regular bit,
		GprmMathId int,
		Argument int,
		WeightTypeId int,
		IrpRuleListId int,
		GprmRuleTypeId int,
		LookBack int,
		EffectiveLag int,
		AllowIncrease bit,
		SaveTypeId int,
		SaveId int,
		VersionId int,
		Active bit,
		ApplicableFrom datetime
	)
	DECLARE @GprmRuleTmpProduct TABLE
	(
		Id int IDENTITY(1,1),
		GeographyId int,
		ProductId int,
		Regular bit,
		GprmMathId int,
		Argument int,
		WeightTypeId int,
		IrpRuleListId int,
		GprmRuleTypeId int,
		LookBack int,
		EffectiveLag int,
		AllowIncrease bit,
		SaveTypeId int,
		SaveId int,
		VersionId int,
		Active bit,
		ApplicableFrom datetime
	)

	SELECT @DefaultProduct=Id from Product WHERE ShortName='XX'

	IF (@isData=1) BEGIN SET @saveTypeId=dbo.GetSaveTypeId('Data') END ELSE BEGIN SET @saveTypeId=dbo.GetSaveTypeId('Forecast') END
	
	IF (@versionId=0) BEGIN SELECT @versionId=MAX(Id) FROM dbo.Version END

	INSERT INTO @GprmRuleTmpDefault
	(
		GeographyId,
		ProductId,
		Regular,
		GprmMathId,
		Argument,
		WeightTypeId,
		IrpRuleListId,
		GprmRuleTypeId,
		LookBack,
		EffectiveLag,
		AllowIncrease,
		SaveTypeId,
		SaveId,
		VersionId,
		Active,
		ApplicableFrom
	)
	SELECT
		GeographyId,
		0,
		Regular,
		GprmMathId,
		Argument,
		WeightTypeId,
		IrpRuleListId,
		GprmRuleTypeId,
		LookBack,
		EffectiveLag,
		AllowIncrease,
		SaveTypeId,
		SaveId,
		VersionId,
		Active,
		ApplicableFrom
	FROM
		dbo.GprmRule
	WHERE
		SaveTypeId=@saveTypeId AND
		SaveId=CASE @saveId WHEN 0 THEN SaveId ELSE @saveId END AND
		GeographyId=CASE @geographyId WHEN 0 THEN GeographyId ELSE @geographyId END AND
		GprmRuleTypeId=CASE @gprmRuleTypeId WHEN 0 THEN GprmRuleTypeId ELSE @gprmRuleTypeId END AND
		ProductId=@DefaultProduct AND
		VersionId<=@versionId
	ORDER BY
		VersionId
	
	INSERT INTO @GprmRuleTmpProduct
	(
		GeographyId,
		ProductId,
		Regular,
		GprmMathId,
		Argument,
		WeightTypeId,
		IrpRuleListId,
		GprmRuleTypeId,
		LookBack,
		EffectiveLag,
		AllowIncrease,
		SaveTypeId,
		SaveId,
		VersionId,
		Active,
		ApplicableFrom
	)
	SELECT
		GeographyId,
		ProductId,
		Regular,
		GprmMathId,
		Argument,
		WeightTypeId,
		IrpRuleListId,
		GprmRuleTypeId,
		LookBack,
		EffectiveLag,
		AllowIncrease,
		SaveTypeId,
		SaveId,
		VersionId,
		Active,
		ApplicableFrom
	FROM
		dbo.GprmRule
	WHERE
		SaveTypeId=@saveTypeId AND
		SaveId=CASE @saveId WHEN 0 THEN SaveId ELSE @saveId END AND
		GeographyId=CASE @geographyId WHEN 0 THEN GeographyId ELSE @geographyId END AND
		ProductId<>@DefaultProduct AND ProductId=CASE @productId WHEN 0 THEN ProductId ELSE @productId END AND
		GprmRuleTypeId=CASE @gprmRuleTypeId WHEN 0 THEN GprmRuleTypeId ELSE @gprmRuleTypeId END AND
		VersionId<=@versionId
	ORDER BY
		VersionId

	INSERT INTO @GprmRuleIdDefault (Id)
	SELECT MAX(Id)
	FROM @GprmRuleTmpDefault
	GROUP BY
		GeographyId,
		ProductId,
		GprmRuleTypeId,
		ApplicableFrom

	INSERT INTO @GprmRuleIdProduct (Id)
	SELECT MAX(Id)
	FROM @GprmRuleTmpProduct
	GROUP BY
		GeographyId,
		ProductId,
		GprmRuleTypeId,
		ApplicableFrom
	
	INSERT INTO @GprmRule
	(
		ApplicableFrom,
		GeographyId,
		ProductId,
		Regular,
		GprmMathId,
		Argument,
		WeightTypeId,
		IrpRuleListId,
		GprmRuleTypeId,
		LookBack,
		EffectiveLag,
		AllowIncrease
	)
	SELECT
		ApplicableFrom,
		GeographyId,
		ProductId,
		Regular,
		GprmMathId,
		Argument,
		WeightTypeId,
		IrpRuleListId,
		GprmRuleTypeId,
		LookBack,
		EffectiveLag,
		AllowIncrease
	FROM
		@GprmRuleTmpDefault AS GR INNER JOIN @GprmRuleIdDefault AS V ON GR.Id=V.Id
	WHERE
		Active=1
	
	UNION ALL

	SELECT
		ApplicableFrom,
		GeographyId,
		ProductId,
		Regular,
		GprmMathId,
		Argument,
		WeightTypeId,
		IrpRuleListId,
		GprmRuleTypeId,
		LookBack,
		EffectiveLag,
		AllowIncrease
	FROM
		@GprmRuleTmpProduct AS GR INNER JOIN @GprmRuleIdProduct AS V ON GR.Id=V.Id
	WHERE
		Active=1
	RETURN
END
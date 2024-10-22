-- =============================================
-- Author:		Jonathan Hubeau
-- Create date: 2014-10-16
-- Description:	Get data for specific version and dimensions
-- =============================================
CREATE FUNCTION [dbo].[GetData]
(	
	@isData bit,
	@saveId int,
	@versionId int,
	@currencyBudgetVersionId int,
	@geographyId0 IdArray readonly,
	@productId0 IdArray readonly,
	@dataTypeId0 IdArray readonly
)
RETURNS @Data TABLE
(
	GeographyId int,
    ProductId int,
    CurrencySpotId int,
    CurrencySpotVersionId int,
    PriceTypeId int,
    DataTypeId int,
	EventTypeId int,
	UnitTypeId int,
    SegmentId int,
    DataTime datetime,
    Value float,
	Description nvarchar(max),
	ValueDisplay float,
	ValueUsdSpot float,
	ValueEurSpot float,
	ValueUsdBudget float,
	ValueEurBudget float
)
AS
BEGIN
	DECLARE @geographyId IdArray
	DECLARE @productId IdArray
	DECLARE @dataTypeId IdArray
	
	DECLARE @saveTypeId int
	DECLARE @CurrencyUnitTypeId int
	DECLARE @DataId TABLE (Id int)
	DECLARE @CurrencyBudgetId TABLE (Id int)
	DECLARE @DataTmp TABLE
	(
		Id int IDENTITY(1,1),
		GeographyId int,
		ProductId int,
		CurrencySpotId int,
		CurrencySpotVersionId int,
		PriceTypeId int,
		DataTypeId int,
		EventTypeId int,
		UnitTypeId int,
		SegmentId int,
		DataTime datetime,
		Value float,
		Description nvarchar(max),
		ValueDisplay float,
		ValueUsdSpot float,
		ValueEurSpot float,
		ValueUsdBudget float,
		ValueEurBudget float,
		VersionId int,
		Active bit
	)
	DECLARE @CurrencyBudget TABLE
	(
		CurrencyId int,
		USD float,
		EUR float
	)
	DECLARE @CurrencyBudgetTmp TABLE
	(
		Id int IDENTITY(1,1),
		CurrencyId int,
		USD float,
		EUR float,
		VersionId int
	)
	DECLARE @DisplayCurrency TABLE
	(
		GeographyId int,
		DisplayCurrencyId int
	)

	IF (@isData=1) BEGIN SET @saveTypeId=dbo.GetSaveTypeId('Data') END ELSE BEGIN SET @saveTypeId=dbo.GetSaveTypeId('Forecast') END
	SELECT @CurrencyUnitTypeId=Id FROM DimensionType WHERE Dimension='Unit' AND Name='Currency'

	IF (@versionId=0 OR @currencyBudgetVersionId=0)
	BEGIN
		DECLARE @MaxVersionId int
		SELECT @MaxVersionId=MAX(Id) FROM dbo.Version
		IF (@versionId=0) BEGIN SET @versionId=@MaxVersionId END
		IF (@currencyBudgetVersionId=0) BEGIN SET @currencyBudgetVersionId=@MaxVersionId END
	END

	IF (SELECT COUNT(Id) FROM @geographyId0)=0
	BEGIN
		INSERT INTO @geographyId (Id) SELECT Id FROM Geography
	END
	ELSE
	BEGIN
		INSERT INTO @geographyId (Id) SELECT Id FROM @geographyId0
	END

	IF (SELECT COUNT(Id) FROM @productId0)=0
	BEGIN
		INSERT INTO @productId (Id) SELECT Id FROM Product
	END
	ELSE
	BEGIN
		INSERT INTO @productId (Id) SELECT Id FROM @productId0
	END
	
	IF (SELECT COUNT(Id) FROM @dataTypeId0)=0
	BEGIN
		INSERT INTO @dataTypeId (Id) SELECT Id FROM dbo.GetDimensionType('Data')
	END
	ELSE
	BEGIN
		INSERT INTO @dataTypeId (Id) SELECT Id FROM @dataTypeId0
	END

	INSERT INTO @CurrencyBudgetTmp
	(
		CurrencyId,
		USD,
		EUR,
		VersionId
	)
	SELECT
		CurrencyId,
		USD,
		EUR,
		VersionId
	FROM
		dbo.CurrencyBudget
	WHERE
		VersionId<=@currencyBudgetVersionId
	ORDER BY
		VersionId

	INSERT INTO @CurrencyBudgetId (Id)
	SELECT MAX(Id)
	FROM @CurrencyBudgetTmp
	GROUP BY
		CurrencyId

	INSERT INTO @CurrencyBudget (CurrencyId, USD, EUR)
	SELECT
		CurrencyId,
		USD,
		EUR
	FROM
		@CurrencyBudgetTmp AS T INNER JOIN @CurrencyBudgetId AS I ON T.Id=I.Id

	INSERT INTO @DisplayCurrency
	(
		GeographyId,
		DisplayCurrencyId
	)
	SELECT
		Id,
		DisplayCurrencyId
	FROM Geography

	INSERT INTO @DataTmp
	(
		GeographyId,
		ProductId,
		CurrencySpotId,
		CurrencySpotVersionId,
		PriceTypeId,
		DataTypeId,
		EventTypeId,
		UnitTypeId,
		SegmentId,
		DataTime,
		Value,
		Description,
		ValueDisplay,
		ValueUsdSpot,
		ValueEurSpot,
		ValueUsdBudget,
		ValueEurBudget,
		VersionId,
		Active
	)
	SELECT
		D.GeographyId,
		D.ProductId,
		D.CurrencySpotId,
		D.CurrencySpotVersionId,
		D.PriceTypeId,
		D.DataTypeId,
		D.EventTypeId,
		D.UnitTypeId,
		D.SegmentId,
		D.DataTime,
		D.Value,
		D.Description,
		CASE D.UnitTypeId WHEN @CurrencyUnitTypeId THEN D.Value*CS.USD/CSD.USD ELSE D.Value END,
		CASE D.UnitTypeId WHEN @CurrencyUnitTypeId THEN D.Value*CS.USD ELSE D.Value END,
		CASE D.UnitTypeId WHEN @CurrencyUnitTypeId THEN D.Value*CS.EUR ELSE D.Value END,
		CASE D.UnitTypeId WHEN @CurrencyUnitTypeId THEN D.Value*CB.USD ELSE D.Value END,
		CASE D.UnitTypeId WHEN @CurrencyUnitTypeId THEN D.Value*CB.EUR ELSE D.Value END,
		D.VersionId,
		D.Active
	FROM
		dbo.Data AS D
			INNER JOIN CurrencySpot AS CS ON
				D.CurrencySpotId=CS.CurrencyId AND
				D.CurrencySpotVersionId=CS.VersionId
			INNER JOIN @CurrencyBudget AS CB ON
				D.CurrencySpotId=CB.CurrencyId
			INNER JOIN @DisplayCurrency AS DC ON
				D.GeographyId=DC.GeographyId
			INNER JOIN CurrencySpot AS CSD ON
				DC.DisplayCurrencyId=CSD.CurrencyId AND
				D.CurrencySpotVersionId=CSD.VersionId
			INNER JOIN @geographyId AS G ON
				D.GeographyId=G.Id
			INNER JOIN @productId AS P ON
				D.ProductId=P.Id
			INNER JOIN @dataTypeId AS DT ON
				D.DataTypeId=DT.Id
	WHERE
		D.SaveTypeId=@saveTypeId AND
		D.SaveId=CASE @saveId WHEN 0 THEN D.SaveId ELSE @saveId END AND
		D.VersionId<=@versionId
	ORDER BY
		D.VersionId
	
	INSERT INTO @DataId (Id)
	SELECT MAX(Id)
	FROM @DataTmp
	GROUP BY
		GeographyId,
		ProductId,
		PriceTypeId,
		DataTypeId,
		SegmentId,
		DataTime
	
	INSERT INTO @Data
	(	
		GeographyId,
		ProductId,
		CurrencySpotId,
		CurrencySpotVersionId,
		PriceTypeId,
		DataTypeId,
		EventTypeId,
		UnitTypeId,
		SegmentId,
		DataTime,
		Value,
		Description,
		ValueDisplay,
		ValueUsdSpot,
		ValueEurSpot,
		ValueUsdBudget,
		ValueEurBudget
	)
	SELECT
		GeographyId,
		ProductId,
		CurrencySpotId,
		CurrencySpotVersionId,
		PriceTypeId,
		DataTypeId,
		EventTypeId,
		UnitTypeId,
		SegmentId,
		DataTime,
		Value,
		Description,
		ValueDisplay,
		ValueUsdSpot,
		ValueEurSpot,
		ValueUsdBudget,
		ValueEurBudget
	FROM
		@DataTmp AS D INNER JOIN @DataId AS V ON D.Id=V.Id
	WHERE
		Active=1
	RETURN
END
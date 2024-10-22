-- =============================================
-- Author:		Jonathan Hubeau
-- Create date: 2014-10-16
-- Description:	Get list-to-sales impact for specific version and dimensions
-- =============================================
CREATE FUNCTION [dbo].[GetListToSalesImpact]
(	
	@isData bit,
	@saveId int,
	@versionId int,
	@geographyId0 IdArray readonly,
	@productId0 IdArray readonly
)
RETURNS @Data TABLE
(
	GeographyId int,
    ProductId int,
    SegmentId int,
    ImpactDelay int,
	ImpactPercentage float
)
AS
BEGIN
	DECLARE @geographyId IdArray
	DECLARE @productId IdArray
	
	DECLARE @saveTypeId int
	DECLARE @DataId TABLE (Id int)
	DECLARE @DataTmp TABLE
	(
		Id int IDENTITY(1,1),
		GeographyId int,
		ProductId int,
		SegmentId int,
		ImpactDelay int,
		ImpactPercentage float,
		VersionId int,
		Active bit
	)

	IF (@isData=1) BEGIN SET @saveTypeId=dbo.GetSaveTypeId('Data') END ELSE BEGIN SET @saveTypeId=dbo.GetSaveTypeId('Forecast') END

	IF (@versionId=0)
	BEGIN
		SELECT @versionId=MAX(Id) FROM dbo.Version
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

	INSERT INTO @DataTmp
	(
		GeographyId,
		ProductId,
		SegmentId,
		ImpactDelay,
		ImpactPercentage,
		VersionId,
		Active
	)
	SELECT
		D.GeographyId,
		D.ProductId,
		D.SegmentId,
		D.ImpactDelay,
		D.ImpactPercentage,
		D.VersionId,
		D.Active
	FROM
		dbo.ListToSalesImpact AS D
			INNER JOIN @geographyId AS G ON
				D.GeographyId=G.Id
			INNER JOIN @productId AS P ON
				D.ProductId=P.Id
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
		SegmentId
	
	INSERT INTO @Data
	(	
		GeographyId,
		ProductId,
		SegmentId,
		ImpactDelay,
		ImpactPercentage
	)
	SELECT
		GeographyId,
		ProductId,
		SegmentId,
		ImpactDelay,
		ImpactPercentage
	FROM
		@DataTmp AS D INNER JOIN @DataId AS V ON D.Id=V.Id
	WHERE
		Active=1
	RETURN
END
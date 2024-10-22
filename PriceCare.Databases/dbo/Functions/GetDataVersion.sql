-- =============================================
-- Author:		Jonathan Hubeau
-- Create date: 2014-10-16
-- Description:	Get list of GprmData versions
-- =============================================
CREATE FUNCTION [dbo].[GetDataVersion]
(	
	@geographyId0 IdArray readonly,
	@productId0 IdArray readonly,
	@dataTypeId0 IdArray readonly
)
RETURNS @GprmDataVersion TABLE
(
	VersionId int,
	Information nvarchar(max),
	VersionData nvarchar(max),
	VersionTime datetime,
	UserName nvarchar(max)
)
AS
BEGIN
	DECLARE @geographyId IdArray
	DECLARE @productId IdArray
	DECLARE @dataTypeId IdArray
	DECLARE @saveTypeId int
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
	DECLARE @DataType TABLE
	(
		Id int,
		Name nvarchar(max),
		ShortName nvarchar(max)
	)
	SET @saveTypeId=dbo.GetSaveTypeId('Data')
	
	INSERT INTO @DataType
	(
		Id,
		Name,
		ShortName
	)
	SELECT
		Id,
		Name,
		ShortName
	FROM
		dbo.GetDimensionType('Data')

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

	INSERT INTO @Version0 (VersionId,VersionData)
	SELECT DISTINCT
		D.VersionId,
		T.Name
	FROM
		Data AS D
			INNER JOIN @DataType AS T ON
				D.DataTypeId=T.Id
			INNER JOIN @geographyId AS G ON
				D.GeographyId=G.Id
			INNER JOIN @productId AS P ON
				D.ProductId=P.Id
			INNER JOIN @dataTypeId AS DT ON
				D.DataTypeId=DT.Id
	
	INSERT INTO @Version (VersionId,VersionData)
	SELECT
		V.VersionId,
		STUFF((SELECT ', '+VersionData FROM @Version0 WHERE VersionId=V.VersionId FOR XML PATH('')),1,1,'')
	FROM @Version0 AS V
	GROUP BY V.VersionId

	INSERT @GprmDataVersion
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
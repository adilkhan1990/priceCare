-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE LOAD_CALENDAR
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

  insert into [PriceCare.Databases.Central].[dbo].[Data]
  (  [GeographyId]
      ,[ProductId]
      ,[CurrencySpotId]
      ,[CurrencySpotVersionId]
      ,[PriceTypeId]
      ,[DataTypeId]
      ,[EventTypeId]
      ,[UnitTypeId]
      ,[SegmentId]
      ,[DataTime]
      ,[Value]
      ,[VersionId]
      ,[SaveTypeId]
      ,[SaveId]
      ,[Active])

SELECT DD.DimensionId as GeographyId
      ,DD2.DimensionId as ProductId
      ,118 as currspotid
	  ,1 as spotver
	  ,1 as pricetype
	  ,43 as datatype
	  ,DT.Id as EventTypeId
	  ,18 as unittypeid
	  ,15 as segid
	  ,DATEADD(year, [year]-1900, DATEADD(month, [month]-1, DATEADD(day, 1-1, 0)))
      ,[SIZE_OF_CUT_PCT]/100  as value
	  ,1
	  ,1
	  ,1
	  ,1
  FROM [PriceCare.Databases.Central].[dbo].[gco_gprm_cntry_price_cal_vw] cal inner join
       [PriceCare.Databases.Central].[dbo].[DimensionDictionary] DD on cal.[COUNTRY]=DD.Name  inner join
	   [PriceCare.Databases.Central].[dbo].[DimensionDictionary] DD2 on cal.[PRODUCT_FAMILY]=DD2.Name inner join
	   [PriceCare.Databases.Central].[dbo].[DimensionType] as DT on DT.[ShortName]=cal.[TYPE_OF_CUT]

where [PROBABILITY_OF_CUT_PCT]>=50
END
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[LOAD_PRICES]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

 
 declare @Prices table (iddx int identity,GeographyId int,ProductId int,PriceTypeId int,CurrencyId int,EffectiveDate datetime,ExpirationDate datetime, PricelistMcg float, CurrencySpotVersionId int)


 insert into @Prices
 (GeographyId,ProductId,PriceTypeId,CurrencyId,EffectiveDate,ExpirationDate,PricelistMcg)
 
  SELECT  (SELECT [DimensionId] FROM [PriceCare.Databases.Central].[dbo].[DimensionDictionary] where Name=[COUNTRY] and Dimension='Geography' and SystemId=16) CountryId
     
	  ,case [PRODUCT_FAMILY] when 'ARANESP' then 
	   case when [STRENGTH]<150 then 1
	        when [STRENGTH]=150 then 14
			when [STRENGTH]>150 then 2 end
	   else  (SELECT  [Id] FROM [PriceCare.Databases.Central].[dbo].[Product] where Name =[PRODUCT_FAMILY]) end  ProductId
     
	 ,(SELECT [Id]  FROM [PriceCare.Databases.Central].[dbo].[PriceType] where Name=[TYPE_TO_CHANNEL]+' '+[STATUS_TO_STATUS]+' '+[PRICELIST_CURRENCY_TYPE]) PriceTypeId
	 ,(SELECT [CurrencyId] FROM [PriceCare.Databases.Central].[dbo].[PriceType]where Name=[TYPE_TO_CHANNEL]+' '+[STATUS_TO_STATUS]+' '+[PRICELIST_CURRENCY_TYPE]) CurrencyId
      ,[EFFECTIVE_DATE]
      ,[EXPIRATION_DATE]
      ,[PRICELIST_PRICE_MCG]

   
  FROM [PriceCare.Databases.Central].[dbo].[gco_gprm_price_list_vw]
  where [EXPIRATION_DATE]>'2013-01-01' and [PRODUCT_FAMILY]<>'' and [PRODUCT_FAMILY]<>'sensipar' and [PRODUCT_FAMILY]<>'Kepivance' and [PRODUCT_FAMILY]<>'kineret' and [PRODUCT_FAMILY]<>'CORORA'


declare @date as datetime
set @date='2013-01-01'




  declare @i int
  set @i=1



  while @date <= '2014-09-01'
  begin

    insert into [PriceCare.Databases.Central].[dbo].[Data]
  ([GeographyId]
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

	select GeographyId
	      ,ProductId
		  ,P.CurrencyId
		  ,max(GC.VersionId) 
		  ,PriceTypeId
		  ,41
		  ,44
		  ,5 as [UnitTypeId]
		  ,15 as seg
		  ,@date
		  ,avg(PricelistMcg)
		  ,1
		  ,39 as savetypeid
		  ,1 as saveid
		  ,1
  from @Prices P inner join
      dbo.GetCurrencySpotMaxVersion(@date) as GC  on GC.CurrencyId=P.CurrencyId
  where 
       EffectiveDate<= dateadd(month,1,@date) and 
		ExpirationDate>=@date
		
  group by GeographyId,
           ProductId,
		   PriceTypeId,
		   P.CurrencyId


  set @date=DATEADD(month,1,@date)
  end





END
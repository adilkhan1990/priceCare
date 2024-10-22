-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[insert_new_skus_from_GCOODS]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

 insert into [PriceCare.Databases.Central].[dbo].[SKU]
  ([GeographyId]
      ,[ProductId]
      ,[Name]
      ,[Dosage]
      ,[PackSize]
      ,[FormulationId]
      ,[FactorUnit])


SELECT  
      distinct
	   (SELECT [DimensionId] FROM [PriceCare.Databases.Central].[dbo].[DimensionDictionary] where Name=[COUNTRY] and Dimension='Geography' and SystemId=16) CountryId
      --,[COUNTRY]
	 -- ,[PRODUCT_FAMILY]
	  ,case [PRODUCT_FAMILY] when 'ARANESP' then 
	   case when [STRENGTH]<150 then 1
	        when [STRENGTH]=150 then 14
			when [STRENGTH]>150 then 2 end
	   else  (SELECT  [Id] FROM [PriceCare.Databases.Central].[dbo].[Product] where Name =[PRODUCT_FAMILY]) end  ProductId
	  
	
    --  ,[PRODUCT_NUMBER]
      ,[PRODUCT_NAME]
	  ,[STRENGTH] as dosage
	  ,[PACK] as Packsize
	  ,17 as Formulationn
      ,case [UOM] when 'mg' then 1000 else 1 end as FactorUnit
     
     

  FROM [PriceCare.Databases.Central].[dbo].[gco_gprm_price_list_vw]
  where (SELECT top 1   [Id] FROM [PriceCare.Databases.Central].[dbo].[SKU] where [Name]=[PRODUCT_NAME]) is null
 and [EXPIRATION_DATE]>'2013-01-01' and [PRODUCT_FAMILY]<>'' and [PRODUCT_FAMILY]<>'sensipar' and [PRODUCT_FAMILY]<>'Kepivance' and [PRODUCT_FAMILY]<>'kineret'
END
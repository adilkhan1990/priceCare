-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[ReferencedP]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

  insert into [PriceCare.Databases.Central].[dbo].[GprmReferencedPrice]
  ([GeographyId]
      ,[ProductId]
      ,[GprmRuleTypeId]
      ,[ReferencedGeographyId]
      ,[ReferencedPriceTypeId]
      ,[ReferencedPriceAdjustment]
      ,[SaveId]
      ,[SaveTypeId]
      ,[VersionId]
      ,[Active]
      ,[ApplicableFrom])


SELECT C.Country_ID
	  ,P.Product_Family_ID
	  ,1
	  ,(select Country_ID from [GPRM_DB].[dbo].[DEF_GCPS_Country_TBL] C where C.GCPS_Name=R.[Referenced] )
	 ,case when (SELECT Id FROM [PriceCare.Databases.Central].[dbo].[PriceType] where [ShortName] = R.[PriceType])
      is null then 1 else (SELECT Id FROM [PriceCare.Databases.Central].[dbo].[PriceType] where [ShortName] = R.[PriceType]) end as [GPRM pricelist]
      ,0
	  ,1
	  ,1
	  ,1
	  ,1
	  ,'2014-10-17'
      --,[Referencing]
      --,[Referenced]
      --,[Product]
      --,[PriceType]
  FROM [PriceCare.Databases.Central].[dbo].[Referenced] R,
       [GPRM_DB].[dbo].[DEF_GCPS_Country_TBL] C,
	   [GPRM_DB].[dbo].[DEF_GCPS_Product_Family_TBL] P --,

	   where R.[Referencing]=C.GCPS_Name and R.[Product]=P.GCPS_Name
	   
	   
	   
	     insert into [PriceCare.Databases.Central].[dbo].[GprmReferencedPrice]
  ([GeographyId]
      ,[ProductId]
      ,[GprmRuleTypeId]
      ,[ReferencedGeographyId]
      ,[ReferencedPriceTypeId]
      ,[ReferencedPriceAdjustment]
      ,[SaveId]
      ,[SaveTypeId]
      ,[VersionId]
      ,[Active]
      ,[ApplicableFrom])
SELECT 
      distinct  [GeographyId] ,
                15,
                1,
               [ReferencedGeographyId],
                1,
                0,
                1,
                1,
                1,
                1,
                '2014-10-17' 
 
  FROM [PriceCare.Databases.Central].[dbo].[GprmReferencedPrice]
  where ProductId <>15 and cast([GeographyId] as nvarchar(max))+'_'+

    
      cast([ReferencedGeographyId] as nvarchar(max)) not in (SELECT 
       cast([GeographyId] as nvarchar(max))+'_'+

    
      cast([ReferencedGeographyId] as nvarchar(max))
 
  FROM [PriceCare.Databases.Central].[dbo].[GprmReferencedPrice]
  where ProductId=15)
	   
END
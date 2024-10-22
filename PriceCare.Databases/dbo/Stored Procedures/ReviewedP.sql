-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[ReviewedP]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

  insert into [PriceCare.Databases.Central].[dbo].[GprmReviewedPrice]
  ([GeographyId]
      ,[ProductId]
      ,[GprmRuleTypeId]
      ,[ReviewedPriceTypeId]
      ,[ReviewedPriceAdjustment]
      ,[SaveId]
      ,[SaveTypeId]
      ,[VersionId]
      ,[Active]
      ,[ApplicableFrom])


SELECT C.Country_ID
	  ,P.Product_Family_ID
	  ,1
	 ,case when (SELECT Id FROM [PriceCare.Databases.Central].[dbo].[PriceType] where [ShortName] = R.[GPRM pricelist])
      is null then 1 else (SELECT Id FROM [PriceCare.Databases.Central].[dbo].[PriceType] where [ShortName] = R.[GPRM pricelist]) end as [GPRM pricelist]
      ,0
	  ,1
	  ,1
	  ,1
	  ,1
	  ,'2014-10-17'
   --   ,R.Country
	  --,R.[Product Family]
	  --,R.[GPRM pricelist]
  FROM [PriceCare.Databases.Central].[dbo].[reviewed] R,
       [GPRM_DB].[dbo].[DEF_GCPS_Country_TBL] C,
	   [GPRM_DB].[dbo].[DEF_GCPS_Product_Family_TBL] P --,

	   where R.Country=C.GCPS_Name and R.[Product Family]=P.GCPS_Name
	   
	   
	   
	     insert into [PriceCare.Databases.Central].[dbo].[GprmReviewedPrice]
  ([GeographyId]
      ,[ProductId]
      ,[GprmRuleTypeId]
      ,[ReviewedPriceTypeId]
      ,[ReviewedPriceAdjustment]
      ,[SaveId]
      ,[SaveTypeId]
      ,[VersionId]
      ,[Active]
      ,[ApplicableFrom]) 
  SELECT distinct [GeographyId],15,1,1,0,1,1,1,1,'2014-10-17'
   
  FROM [PriceCare.Databases.Central].[dbo].[GprmReviewedPrice]
  where ProductId <>15 and 
       GeographyId not in (select GeographyId from [PriceCare.Databases.Central].[dbo].[GprmReviewedPrice] P2 where P2.ProductId=15)
	   
END
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE reviewed_price
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
declare @output as table (iddx int identity, [Country] nvarchar(max)
      ,Country_ID int
      ,[Product] nvarchar(max)
      ,Product_Family_ID int
      ,[GPRM_Output] nvarchar(max)
      ,[CPF_ID] int
      ,[GPRM_LP_ID] int)
      
insert into @output
([Country]
      ,Country_ID
      ,[Product]
      ,Product_Family_ID
      ,[GPRM_Output]
      ,[CPF_ID]
      ,[GPRM_LP_ID])      

SELECT  [Country]
      ,C.Country_ID
      ,[Product]
      ,P.Product_Family_ID
      ,[GPRM_Output]
      ,[CPF_ID]
      ,[GPRM_LP_ID]
  FROM [GPRM_DB].[dbo].[TMP_GPRM_OUTPUT_PRICE_TBL] OP,
      [GPRM_DB].[dbo].[DEF_GCPS_Country_TBL] C,
       [GPRM_DB].[dbo].[DB_Product_Family_TBL] P
   where OP.Country=C.GCPS_Name and OP.Product=P.Name and OP.GPRM_LP_ID is not null
 
 declare @existing_rule as table (iddx int  identity , geographyid int)

insert into @existing_rule
(geographyid)
SELECT [GeographyId]
     
  FROM [PriceCare.Databases.Central].[dbo].[GprmRule]
 
 
 
 select * from @output where Country_ID in (select geographyid from @existing_rule)
 
 
END
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[RulePeriod]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

  insert into [PriceCare.Databases.Central].[dbo].[GprmRulePeriodicity]
  ([GeographyId]
      ,[StartMonth]
      ,[Period]
      ,[SaveTypeId]
      ,[Active]
      ,[VersionId]
      ,[ValidFrom])
   
SELECT distinct C.Country_ID
      , case when (SELECT  [Month_ID] FROM [GPRM_DB].[dbo].[DB_Month_TBL] where Month =[Referencing Month]) is null then 0
        else (SELECT  [Month_ID] FROM [GPRM_DB].[dbo].[DB_Month_TBL] where Month =[Referencing Month]) end
      ,[Number of Months Between Referencing]
      ,1
      ,1
      ,1
      ,'2014-10-17'
  FROM [GPRM_DB].[dbo].[MR_Ref] R,
       [GPRM_DB].[dbo].[DEF_MR_Country_TBL] C
  where Frequency='Regular' and R.[Country  Name]=C.MR_Name
END
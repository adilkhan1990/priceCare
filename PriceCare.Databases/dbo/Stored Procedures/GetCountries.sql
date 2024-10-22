-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetCountries]

@RegionId int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

if @RegionId = 0 
begin
 SELECT G.[Id]
      ,G.[Name]
      ,[Iso2]
      ,C.Iso
      ,[DisplayCurrencyId]
      ,G.[Active]
      ,case when (SELECT [Name] FROM [dbo].[DimensionDictionary]  where Dimension='Geography' and SystemId=14 and DimensionId=G.[Id]) IS null
            then G.[Name] else (SELECT [Name] FROM [dbo].[DimensionDictionary]  where Dimension='Geography' and SystemId=14 and DimensionId=G.[Id]) end as ExportName
       ,GL.GeographyUpId as RegionId
  
  FROM [dbo].[Geography] G inner join
       [dbo].[Currency] C on G.DisplayCurrencyId=C.Id inner join
       GeographyLink GL on GL.GeographyId=G.Id
       
  where G.GeographyTypeId=3 and G.Name<>'Not Specified'
  order by G.Name
end
else
begin
  SELECT G.[Id]
      ,G.[Name]
      ,[Iso2]
       ,C.Iso
      ,[DisplayCurrencyId]
      ,G.[Active]
      ,case when (SELECT [Name] FROM [dbo].[DimensionDictionary]  where Dimension='Geography' and SystemId=14 and DimensionId=G.[Id]) IS null
            then G.[Name] else (SELECT [Name] FROM [dbo].[DimensionDictionary]  where Dimension='Geography' and SystemId=14 and DimensionId=G.[Id]) end as ExportName
            
       ,GL.GeographyUpId as RegionId
  
  FROM [dbo].[Geography] G inner join
       [dbo].[Currency] C on G.DisplayCurrencyId=C.Id inner join
       GeographyLink GL on GL.GeographyId=G.Id
       
  where G.GeographyTypeId=3 and G.Name<>'Not Specified' and GL.GeographyUpId=@RegionId
  order by G.Name

end

END
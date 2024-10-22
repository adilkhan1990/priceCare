-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Fill_Rules_MR_Extract]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    declare @GprmRule as table (idx int identity
                             , [GprmRulePermId] int
                              ,[GeograpgyId] int
							  ,[ProductId] int
							  ,[Regular] bit
							  ,[GprmMathId] int
							  ,[Argument] int
							  ,[IrpRuleListId] int
							  ,[GprmRuleTypeId] int
							  ,[LookBack] int
							  ,[EffectiveLag] int
							  ,[AllowIncrease] bit 
							  ,[SaveTypeId] int
							  ,[SaveId] int
							  ,[VersionId] int
							  ,[Active] bit)
							  
							  
	insert into @GprmRule (GeograpgyId,ProductId,Regular,GprmMathId,Argument,IrpRuleListId,GprmRuleTypeId,
	                      LookBack,EffectiveLag,
	                       AllowIncrease,SaveTypeId,SaveId,VersionId,Active)	
	
	SELECT distinct G.Id as GeographyId
	      ,15
	      ,case [Frequency] when 'Irregular' then 0 when '' then 0 else 1 end as Regular
		  ,Calc.IrpMathId as Mathdid
		  ,Calc.Argument
		  ,IRP.Id as rulelistid
		  ,1 as ruletype -- default
		  ,1 as back
		  ,0 as lag
		  ,0 as allow
		  ,1 as savetypeid
		  ,1 as Saveid
          ,1 as versionid
          ,1 as active
  FROM [GPRM_DB].[dbo].[MR_Ref] Ref,
       [GPRM_DB].[dbo].[DEF_MR_Country_TBL] MRC,
       [GPRM_DB].[dbo].[DB_Country_TBL] DBC,
       [PriceCare.Databases.Central].[dbo].[Geography] G,
       [PriceCare.Databases.Central].[dbo].[IrpRule] IRP,
       [PriceCare.Databases.Central].[dbo].[IrpRuleCalculation] Calc
  where REf.[Country  Name]=MRC.MR_Name and MRC.Country_ID=DBC.Country_ID and G.ShortName=DBC.Code_3D
      and [Formula Used by Govt ]<>'' and [Formula Used by Govt ]<>'Negotiation'
      and IRP.Description=Ref.[Formula Used by Govt ] and Calc.IrpRuleListId=IRP.Id and Calc.UpId is null	
      
      
      update @GprmRule
      set GprmRulePermId=idx
      
      
  --     insert into [PriceCare.Databases.Central].[dbo].[GprmRule]
  --([GprmRulePermId]
  --    ,[GeographyId]
  --    ,[ProductId]
  --    ,[Regular]
  --    ,[GprmMathId]
  --    ,[Argument]
  --    ,[WeightTypeId]
  --    ,[IrpRuleListId]
  --    ,[GprmRuleTypeId]
  --    ,[LookBack]
  --    ,[EffectiveLag]
  --    ,[AllowIncrease]
  --    ,[SaveTypeId]
  --    ,[SaveId]
  --    ,[VersionId]
  --    ,[Active]
  --    ,[ApplicableFrom])
      select GprmRulePermId,
             GeograpgyId,
             ProductId,
             Regular,
             GprmMathId,
             Argument,
             1,
             IrpRuleListId,
             GprmRuleTypeId,
             LookBack,
             EffectiveLag,
             AllowIncrease,
             SaveTypeId,
             SaveId,
             VersionId,
             Active,
             '2014-10-17'
       from @GprmRule
      
      
      
        declare @GprmSubRule as table (iddx int identity
                                  ,[ProductId] int
								  ,[GprmSubRulePermId] int
								  ,[GprmRulePermId] int
								  ,[GprmMathId] int
								  ,[Argument] int
								  ,[WeightTypeId] int
								  ,[SaveTypeId] int
								  ,[SaveId] int
								  ,[VersionId] int
								  ,[Active] int)
		
		declare @i int
		declare @j int
		set @i=1
		while @i <= (select MAX(idx) from @GprmRule)
		begin
		
		if (SELECT TOP 1 [Id] FROM [PriceCare.Databases.Central].[dbo].[IrpRuleCalculation] where IrpRuleListId=(select IrpRuleListId  from @GprmRule where idx=@i) and UpId is not null) is not null
		begin
		
		set @j=(SELECT min([Id]) FROM [PriceCare.Databases.Central].[dbo].[IrpRuleCalculation] where IrpRuleListId=(select IrpRuleListId  from @GprmRule where idx=@i) and UpId is not null)
		while @j<=(SELECT max([Id]) FROM [PriceCare.Databases.Central].[dbo].[IrpRuleCalculation] where IrpRuleListId=(select IrpRuleListId  from @GprmRule where idx=@i) and UpId is not null)
			begin
	
		    insert into @GprmSubRule
		      ([ProductId] 
			  ,[GprmRulePermId] 
			  ,[GprmMathId] 
			  ,[Argument] 
			  ,[WeightTypeId] 
			  ,[SaveTypeId] 
			  ,[SaveId] 
			  ,[VersionId] 
			  ,[Active] )
			  
			  select 15,
			         (select GprmRulePermId from @GprmRule where idx=@i),
		             (SELECT IrpMathId FROM [PriceCare.Databases.Central].[dbo].[IrpRuleCalculation] where IrpRuleListId=(select IrpRuleListId  from @GprmRule where idx=@i) and Id=@j),
		             (SELECT Argument FROM [PriceCare.Databases.Central].[dbo].[IrpRuleCalculation] where IrpRuleListId=(select IrpRuleListId  from @GprmRule where idx=@i) and Id=@j)
		             ,1
		             ,1
		             ,1
		             ,1
		             ,1
			set @j=@j+1
			end
		end
		
		set @i=@i+1
		end
      			
      
      update @GprmSubRule
      set GprmSubRulePermId=iddx
      
        
  --insert into [PriceCare.Databases.Central].[dbo].[GprmSubRule]
  --([ProductId]
  --    ,[GprmSubRulePermId]
  --    ,[GprmRulePermId]
  --    ,[GprmMathId]
  --    ,[Argument]
  --    ,[WeightTypeId]
  --    ,[SaveTypeId]
  --    ,[SaveId]
  --    ,[VersionId]
  --    ,[Active])
      select [ProductId] 
              ,[GprmSubRulePermId]
			  ,[GprmRulePermId] 
			  ,[GprmMathId] 
			  ,[Argument] 
			  ,[WeightTypeId] 
			  ,[SaveTypeId] 
			  ,[SaveId] 
			  ,[VersionId] 
			  ,[Active]
       from @GprmSubRule
      
        declare @GprmBasket as table (idddx int identity,
                               [ProductId] int
							  ,[GprmRulePermId] int
							  ,[GprmSubRulePermId] int
							  ,[GeographyId] int
							  ,[SaveTypeId] int
							  ,[SaveId] int
							  ,[VersionId] int
							  ,[Active] bit)
							  
		declare @ref as table (idd int identity,Referencing int,referenced int,GPRMPermRuleId int)					  
							  
		
		insert into @ref(Referencing,referenced)
		SELECT  G.Id as GeographyId
	      ,( select top 1 G.Id from   
      [GPRM_DB].[dbo].[DEF_MR_Country_TBL] MRC,
       [GPRM_DB].[dbo].[DB_Country_TBL] DBC,
       [PriceCare.Databases.Central].[dbo].[Geography] G
       where MRC.Country_ID=DBC.Country_ID and DBC.Code_3D=G.ShortName and MRC.MR_Name=Ref.[Referenced Country  Name])
  FROM [GPRM_DB].[dbo].[MR_Ref] Ref,
       [GPRM_DB].[dbo].[DEF_MR_Country_TBL] MRC,
       [GPRM_DB].[dbo].[DB_Country_TBL] DBC,
       [PriceCare.Databases.Central].[dbo].[Geography] G,
       [PriceCare.Databases.Central].[dbo].[IrpRule] IRP,
       [PriceCare.Databases.Central].[dbo].[IrpRuleCalculation] Calc
  where REf.[Country  Name]=MRC.MR_Name and MRC.Country_ID=DBC.Country_ID and G.ShortName=DBC.Code_3D
      and [Formula Used by Govt ]<>'' and [Formula Used by Govt ]<>'Negotiation'
      and IRP.Description=Ref.[Formula Used by Govt ] and Calc.IrpRuleListId=IRP.Id and Calc.UpId is null	
      
     
      
      
      update @ref
      set GPRMPermRuleId=(select GprmRulePermId from @GprmRule where GeograpgyId = ref.Referencing)
      from @ref as ref
      
     --  select * from @ref
       
        insert into @GprmBasket
				      ([ProductId]
					  ,[GprmRulePermId]
					--  ,[GprmSubRulePermId]
					  ,[GeographyId]
					  ,[SaveTypeId]
					  ,[SaveId]
					  ,[VersionId]
					  ,[Active])
	    select 15,
	           GPRMPermRuleId,
	           referenced,
	           1,
	           1,
	           1,
	           1 
	    from @ref
	    
	    select * from @GprmBasket
	    
	    insert into [PriceCare.Databases.Central].[dbo].[GprmBasket]
    ([GeographyId]
      ,[ProductId]
      ,[GprmRuleTypeId]
      ,[ReferencedGeographyId]
      ,[VersionId]
      ,[SaveTypeId]
      ,[SaveId]
      ,[Active]
      ,[ApplicableFrom])
	    select     (select GeograpgyId from @GprmRule as R where R.[GprmRulePermId]=B.[GprmRulePermId])  
	                ,[ProductId]
	                ,1
					  --,[GprmRulePermId]
					  --,[GprmSubRulePermId]
					  ,[GeographyId]
					   ,[VersionId]
					  ,[SaveTypeId]
					  ,[SaveId]
					  ,[Active]
					  ,'2014-10-17'
	   from @GprmBasket as B
	   
	 
      
      	  
END
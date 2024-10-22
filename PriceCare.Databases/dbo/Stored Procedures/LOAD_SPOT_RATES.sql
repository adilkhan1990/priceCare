-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[LOAD_SPOT_RATES]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

declare @i int
declare @date  date
declare @month int
declare @year int
declare @EUR_rate float
 declare @CurrencySpot as table (CurrencyId int,ISO nvarchar(max),VersionId int,USD float,EUR float)

set @date='2013-01-01'
set @i=1

while @date<'2014-10-01' 
begin

set @month=month(@date)
set @year=year(@date)


  insert into [Version]
  ([Information]
      ,[VersionTime]
      ,[UserId])
	  values('Spot Rate Load',@date,'8e4534d8-e32a-46f4-af94-2a9ea6194082')

--set @EUR_rate=(select  max(R2.[EXCHANGE_RATE]) from   [PriceCare.Databases.Central].[dbo].[gco_sap_curr_exchange_rate_vw] R2 where R2.[FROM_CURR]='EUR' and month(R2.[EFFECTIVE_DATE])=@month and year(R2.[EFFECTIVE_DATE])=@year and R2.[RATE_TYPE]='E' )

--select @EUR_rate,@month,@year
insert into @CurrencySpot
(CurrencyId,Iso,VersionId,USD,EUR)
SELECT  curr.[Id] as currencyId
      ,[Iso]
      ,(select max(id) from [Version] where [VersionTime]=@date and [Information]='Spot Rate Load' ) as VersionId
	  ,Rates.[EXCHANGE_RATE] as USD_SPOT
	  ,(Rates.[EXCHANGE_RATE]/(select  max(R2.[EXCHANGE_RATE]) from [gco_sap_curr_exchange_rate_vw] R2 where R2.[FROM_CURR]='EUR' and month(R2.[EFFECTIVE_DATE])=@month and year(R2.[EFFECTIVE_DATE])=@year and R2.[RATE_TYPE]='E' )) EUR_SPOT
	--  ,Rates.[TO_CURR]
  FROM [Currency] curr ,
       [gco_sap_curr_exchange_rate_vw] Rates
		where rates.[FROM_CURR]=curr.iso 
		and rates.[FROM_CURR]<>'USD' and month(Rates.[EFFECTIVE_DATE])=@month and year(Rates.[EFFECTIVE_DATE])=@year and Rates.[RATE_TYPE]='E'

union 
      select 30,
             'USD' ,
			 (select max(id) from [Version] where [VersionTime]=@date and [Information]='Spot Rate Load' ),
			  
			  1,
			  1/(select  max(R2.[EXCHANGE_RATE]) from [gco_sap_curr_exchange_rate_vw] R2 where R2.[FROM_CURR]='EUR' and month(R2.[EFFECTIVE_DATE])=@month and year(R2.[EFFECTIVE_DATE])=@year and R2.[RATE_TYPE]='E' )




set @date=DATEADD(month,1,@date)
end

  insert into [CurrencySpot]
  ([CurrencyId]
      ,[VersionId]
      ,[USD]
      ,[EUR])

select CurrencyId,VersionId,USD,EUR from @CurrencySpot



END
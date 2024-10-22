-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE UpdateCountries
@GeographyId bit,
@UpdateActiveStatus bit,
@UpdateExportName bit,
@UpdateCurrency bit,
@ActiveStatus bit,
@CurrencyId bit,
@ExportName nvarchar(max)


AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

if @UpdateCurrency =1
	begin
		update [dbo].[Geography]
		set [DisplayCurrencyId]=@CurrencyId
		where Id=@GeographyId
	end

if @UpdateActiveStatus =1
	begin
		update [dbo].[Geography]
		set [Active]=@ActiveStatus
		where Id=@GeographyId
	end


if @UpdateExportName=1
begin
if (SELECT [Name] FROM [dbo].[DimensionDictionary]  where Dimension='Geography' and SystemId=14 and DimensionId=@GeographyId) IS null
	begin

		insert into [dbo].[DimensionDictionary]
		           ([Dimension]
			       ,[DimensionId]
			       ,[SystemId]
			       ,[Name])
		values     ('Geography'
		           ,@GeographyId
		           ,14
		           ,@ExportName)

	end
	else
	begin

		update [dbo].[DimensionDictionary]
		set    Name=@ExportName
		where  Dimension='Geography' and 
		       SystemId=14 and 
		       DimensionId=@GeographyId

	end

end

 
END
CREATE TABLE [dbo].[Geography]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[Name] NVARCHAR(MAX) NOT NULL,
	[ShortName] NVARCHAR(MAX) NOT NULL,  
    [Iso2] NCHAR(2) NOT NULL,
	[GeographyTypeId] INT NOT NULL, 
    [DisplayCurrencyId] INT NOT NULL, 
    [Active] BIT NOT NULL
)

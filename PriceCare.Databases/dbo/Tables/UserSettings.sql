CREATE TABLE [dbo].[UserSettings]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [DefaultRegionId] INT NOT NULL, 
    [DefaultCountryId] INT NOT NULL, 
    [DefaultProductId] NCHAR(10) NOT NULL
)

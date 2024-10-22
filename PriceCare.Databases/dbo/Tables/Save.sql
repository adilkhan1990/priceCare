CREATE TABLE [dbo].[Save]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] NVARCHAR(MAX) NOT NULL, 
    [Comment] NVARCHAR(MAX) NOT NULL, 
    [SaveTypeId] INT NOT NULL, 
    [SaveTime] DATETIME NOT NULL, 
    [Active] BIT NOT NULL, 
    [isReference] BIT NULL, 
    [isBudget] BIT NULL 
)

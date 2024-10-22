CREATE TABLE [dbo].[IrpRule]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Description] NVARCHAR(MAX) NOT NULL, 
    [Active] BIT NOT NULL
)

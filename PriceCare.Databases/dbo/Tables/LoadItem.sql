CREATE TABLE [dbo].[LoadItem]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [LoadId] INT NOT NULL, 
    [Name] NVARCHAR(150) NOT NULL, 
    [Status] INT NOT NULL, 
    [LastUpdateDate] DATETIME NOT NULL, 
    [IsDimension] BIT NOT NULL, 
    CONSTRAINT [FK_LoadItem_ToLoadStatus] FOREIGN KEY ([Status]) REFERENCES [LoadStatus]([Id]), 
    CONSTRAINT [FK_LoadItem_ToLoad] FOREIGN KEY ([LoadId]) REFERENCES [Load]([Id])
)

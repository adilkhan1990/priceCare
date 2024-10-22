CREATE TABLE [dbo].[SaveVersion]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [SaveId] INT NOT NULL, 
    [SaveTypeId] INT NOT NULL, 
    [DataVersionId] INT NOT NULL, 
    [VersionId] INT NOT NULL
)

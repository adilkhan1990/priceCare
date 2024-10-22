CREATE TABLE [dbo].[GeographyLink]
(
    [GeographyUpId] INT NOT NULL, 
    [GeographyId] INT NOT NULL, 
    CONSTRAINT [PK_GeographyLink] PRIMARY KEY ([GeographyUpId], [GeographyId])
)

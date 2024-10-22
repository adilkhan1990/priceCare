CREATE TABLE [dbo].[ProductUnit] (
    [Id]           INT IDENTITY (1, 1) NOT NULL,
    [ProductId]    INT NOT NULL,
    [UnitId]       INT NOT NULL,
    [FactorScreen] INT NOT NULL,
    [Active]       BIT NOT NULL,
    [IsDefault]    BIT NOT NULL,
    CONSTRAINT [PK_ProductUnit] PRIMARY KEY CLUSTERED ([Id] ASC)
);




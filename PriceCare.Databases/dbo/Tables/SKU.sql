CREATE TABLE [dbo].[SKU] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [GeographyId]   INT            NOT NULL,
    [ProductId]     INT            NOT NULL,
    [Name]          NVARCHAR (MAX) NOT NULL,
    [Dosage]        FLOAT (53)     NOT NULL,
    [PackSize]      FLOAT (53)     NOT NULL,
    [FormulationId] INT            NOT NULL,
    [FactorUnit]    FLOAT (53)     NOT NULL,
    [Active]        BIT            NOT NULL,
    CONSTRAINT [PK__SKU__3214EC07F1530A0C] PRIMARY KEY CLUSTERED ([Id] ASC)
);



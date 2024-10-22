CREATE TABLE [dbo].[Unit] (
    [Id]              INT            IDENTITY (1, 1) NOT NULL,
    [DimensionTypeId] INT            NOT NULL,
    [Name]            NVARCHAR (MAX) NOT NULL,
    [Factor]          FLOAT (53)     NOT NULL,
    CONSTRAINT [PK_Unit] PRIMARY KEY CLUSTERED ([Id] ASC)
);


CREATE TABLE [dbo].[DimensionType] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [Dimension] NVARCHAR (MAX) NOT NULL,
    [Name]      NVARCHAR (MAX) NOT NULL,
    [ShortName] NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK__Dimensio__3214EC078F4944B3] PRIMARY KEY CLUSTERED ([Id] ASC)
);




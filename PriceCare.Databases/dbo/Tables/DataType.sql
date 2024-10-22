CREATE TABLE [dbo].[DataType] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [Data]       NVARCHAR (MAX) NOT NULL,
    [UnitTypeId] INT            NOT NULL,
    CONSTRAINT [PK__DataType__3214EC07C4DCBFB0] PRIMARY KEY CLUSTERED ([Id] ASC)
);



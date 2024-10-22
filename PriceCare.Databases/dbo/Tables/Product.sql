CREATE TABLE [dbo].[Product] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [Name]       NVARCHAR (MAX) NOT NULL,
    [ShortName]  NVARCHAR (MAX) NOT NULL,
    [UnitTypeId] INT            NOT NULL,
    [Active]     BIT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);



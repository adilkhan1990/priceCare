CREATE TABLE [dbo].[PriceType] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [Name]       NVARCHAR (MAX) NULL,
    [ShortName]  NVARCHAR (MAX) NULL,
    [CurrencyId] INT            NULL,
    [Active]     BIT            NULL,
    CONSTRAINT [PK_PriceType2] PRIMARY KEY CLUSTERED ([Id] ASC)
);





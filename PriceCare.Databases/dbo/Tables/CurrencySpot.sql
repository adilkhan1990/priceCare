CREATE TABLE [dbo].[CurrencySpot] (
    [CurrencyId] INT        NOT NULL,
    [VersionId]  INT        NOT NULL,
    [USD]        FLOAT (53) NOT NULL,
    [EUR]        FLOAT (53) NOT NULL,
    CONSTRAINT [PK_CurrencyData] PRIMARY KEY CLUSTERED ([CurrencyId] ASC, [VersionId] ASC)
);


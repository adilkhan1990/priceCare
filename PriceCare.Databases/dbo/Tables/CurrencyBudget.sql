CREATE TABLE [dbo].[CurrencyBudget] (
    [Id]         INT        IDENTITY (1, 1) NOT NULL,
    [CurrencyId] INT        NOT NULL,
    [VersionId]  INT        NOT NULL,
    [USD]        FLOAT (53) NOT NULL,
    [EUR]        FLOAT (53) NOT NULL,
    CONSTRAINT [PK_CurrencyBudget] PRIMARY KEY CLUSTERED ([Id] ASC)
);



CREATE TABLE [dbo].[ListToSales] (
    [Id]                    INT        IDENTITY (1, 1) NOT NULL,
    [GeographyId]           INT        NOT NULL,
    [ProductId]             INT        NOT NULL,
    [CurrencySpotId]        INT        NOT NULL,
    [CurrencySpotVersionId] INT        NOT NULL,
    [SegmentId]             INT        NOT NULL,
    [Asp]                   FLOAT (53) NOT NULL,
    [MarketPercentage]      FLOAT (53) NOT NULL,
    [ImpactPercentage]      FLOAT (53) NOT NULL,
    [VersionId]             INT        NOT NULL,
    [SaveId]                INT        NOT NULL,
    [SaveTypeId]            INT        NOT NULL,
    [Active]                BIT        NOT NULL,
    CONSTRAINT [PK__Segment__3214EC075214EEC0] PRIMARY KEY CLUSTERED ([Id] ASC)
);




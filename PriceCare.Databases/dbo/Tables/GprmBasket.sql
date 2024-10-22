CREATE TABLE [dbo].[GprmBasket] (
    [GeographyId]           INT      NOT NULL,
    [ProductId]             INT      NOT NULL,
    [SubRuleIndex]          INT      NOT NULL,
    [ReferencedGeographyId] INT      NOT NULL,
    [VersionId]             INT      NOT NULL,
    [GprmRuleTypeId]        INT      NOT NULL,
    [SaveTypeId]            INT      NOT NULL,
    [SaveId]                INT      NOT NULL,
    [Active]                BIT      NOT NULL,
    [ApplicableFrom]        DATETIME NOT NULL,
    CONSTRAINT [PK_GprmBasket] PRIMARY KEY CLUSTERED ([GeographyId] ASC, [ProductId] ASC, [SubRuleIndex] ASC, [ReferencedGeographyId] ASC)
);











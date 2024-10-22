CREATE TABLE [dbo].[GprmReferencedPrice] (
    [GeographyId]               INT        NOT NULL,
    [ProductId]                 INT        NOT NULL,
    [SubRuleIndex]              INT        NOT NULL,
    [ReferencedGeographyId]     INT        NOT NULL,
    [VersionId]                 INT        NOT NULL,
    [GprmRuleTypeId]            INT        NOT NULL,
    [ReferencedPriceTypeId]     INT        NOT NULL,
    [ReferencedPriceAdjustment] FLOAT (53) NOT NULL,
    [SaveId]                    INT        NOT NULL,
    [SaveTypeId]                INT        NOT NULL,
    [Active]                    BIT        NOT NULL,
    [ApplicableFrom]            DATETIME   NOT NULL,
    CONSTRAINT [PK_GprmReferencedPrice_1] PRIMARY KEY CLUSTERED ([GeographyId] ASC, [ProductId] ASC, [SubRuleIndex] ASC, [ReferencedGeographyId] ASC, [VersionId] ASC)
);










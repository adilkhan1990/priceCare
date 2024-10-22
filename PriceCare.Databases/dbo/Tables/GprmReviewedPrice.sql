CREATE TABLE [dbo].[GprmReviewedPrice] (
    [GeographyId]             INT        NOT NULL,
    [ProductId]               INT        NOT NULL,
    [VersionId]               INT        NOT NULL,
    [GprmRuleTypeId]          INT        NOT NULL,
    [ReviewedPriceTypeId]     INT        NOT NULL,
    [ReviewedPriceAdjustment] FLOAT (53) NOT NULL,
    [SaveId]                  INT        NOT NULL,
    [SaveTypeId]              INT        NOT NULL,
    [Active]                  BIT        NOT NULL,
    [ApplicableFrom]          DATETIME   NOT NULL,
    CONSTRAINT [PK_GprmReviewedPrice_1] PRIMARY KEY CLUSTERED ([GeographyId] ASC, [ProductId] ASC, [VersionId] ASC)
);








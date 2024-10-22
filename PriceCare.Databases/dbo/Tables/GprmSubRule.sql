CREATE TABLE [dbo].[GprmSubRule] (
    [GeographyId]    INT      NOT NULL,
    [ProductId]      INT      NOT NULL,
    [SubRuleIndex]   INT      NOT NULL,
    [VersionId]      INT      NOT NULL,
    [GprmRuleTypeId] INT      NOT NULL,
    [GprmMathId]     INT      NOT NULL,
    [Argument]       INT      NOT NULL,
    [WeightTypeId]   INT      NOT NULL,
    [SaveTypeId]     INT      NOT NULL,
    [SaveId]         INT      NOT NULL,
    [Active]         BIT      NOT NULL,
    [ApplicableFrom] DATETIME NOT NULL,
    CONSTRAINT [PK_GprmSubRule_1] PRIMARY KEY CLUSTERED ([GeographyId] ASC, [ProductId] ASC, [SubRuleIndex] ASC, [VersionId] ASC)
);







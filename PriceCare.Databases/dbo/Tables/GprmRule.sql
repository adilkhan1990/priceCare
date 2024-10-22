CREATE TABLE [dbo].[GprmRule] (
    [GeographyId]    INT      NOT NULL,
    [ProductId]      INT      NOT NULL,
    [VersionId]      INT      NOT NULL,
    [Regular]        BIT      NOT NULL,
    [GprmMathId]     INT      NOT NULL,
    [Argument]       INT      NOT NULL,
    [WeightTypeId]   INT      NOT NULL,
    [IrpRuleListId]  INT      NOT NULL,
    [GprmRuleTypeId] INT      NOT NULL,
    [LookBack]       INT      NOT NULL,
    [EffectiveLag]   INT      NOT NULL,
    [AllowIncrease]  BIT      NOT NULL,
    [SaveTypeId]     INT      NOT NULL,
    [SaveId]         INT      NOT NULL,
    [Active]         BIT      NOT NULL,
    [ApplicableFrom] DATETIME NOT NULL,
    CONSTRAINT [PK_GprmRule] PRIMARY KEY CLUSTERED ([GeographyId] ASC, [ProductId] ASC, [VersionId] ASC)
);









CREATE TABLE [dbo].[ListToSalesImpact] (
    [Id]               INT        IDENTITY (1, 1) NOT NULL,
    [GeographyId]      INT        NOT NULL,
    [ProductId]        INT        NOT NULL,
    [SegmentId]        FLOAT (53) NOT NULL,
    [ImpactDelay]      INT        NOT NULL,
    [ImpactPercentage] FLOAT (53) NOT NULL,
    [VersionId]        INT        NOT NULL,
    [SaveId]           INT        NOT NULL,
    [SaveTypeId]       INT        NOT NULL,
    [Active]           BIT        NOT NULL,
    CONSTRAINT [PK__SegmentI__3214EC07A8D66D36] PRIMARY KEY CLUSTERED ([Id] ASC)
);




CREATE TABLE [dbo].[GprmRulePeriodicity] (
    [Id]          INT      IDENTITY (1, 1) NOT NULL,
    [GeographyId] INT      NOT NULL,
    [ProductId]   INT      NULL,
    [StartMonth]  INT      NOT NULL,
    [Period]      INT      NOT NULL,
    [SaveTypeId]  INT      NOT NULL,
    [Active]      BIT      NOT NULL,
    [VersionId]   INT      NOT NULL,
    [ValidFrom]   DATETIME NOT NULL,
    CONSTRAINT [PK_GprmRulePeriodicity] PRIMARY KEY CLUSTERED ([Id] ASC)
);








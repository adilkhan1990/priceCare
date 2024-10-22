CREATE TABLE [dbo].[Data] (
    [Id]                    INT            IDENTITY (1, 1) NOT NULL,
    [GeographyId]           INT            NOT NULL,
    [ProductId]             INT            NOT NULL,
    [CurrencySpotId]        INT            NOT NULL,
    [CurrencySpotVersionId] INT            NOT NULL,
    [PriceTypeId]           INT            NOT NULL,
    [DataTypeId]            INT            NOT NULL,
    [EventTypeId]           INT            NOT NULL,
    [UnitTypeId]            INT            NOT NULL,
    [SegmentId]             INT            NOT NULL,
    [DataTime]              DATETIME       NOT NULL,
    [Value]                 FLOAT (53)     NOT NULL,
    [VersionId]             INT            NOT NULL,
    [SaveTypeId]            INT            NOT NULL,
    [SaveId]                INT            NOT NULL,
    [Active]                BIT            NOT NULL,
    [Description]           NVARCHAR (MAX) NULL,
    CONSTRAINT [PK__tmp_ms_x__3214EC076ED5FFF2] PRIMARY KEY CLUSTERED ([Id] ASC)
);









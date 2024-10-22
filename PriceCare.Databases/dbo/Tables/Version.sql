CREATE TABLE [dbo].[Version] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Information] NVARCHAR (MAX) NOT NULL,
    [VersionTime] DATETIME       NOT NULL,
    [UserId]      NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK__Version__3214EC0763677CF1] PRIMARY KEY CLUSTERED ([Id] ASC)
);


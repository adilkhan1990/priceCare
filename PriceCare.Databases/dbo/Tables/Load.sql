CREATE TABLE [dbo].[Load] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [Name]           NVARCHAR (250) NOT NULL,
    [CreationDate]   DATETIME       NOT NULL,
    [LastUpdateDate] DATETIME       NOT NULL,
    [Status]         INT            NOT NULL,
    [UserId]         NVARCHAR (128) NOT NULL,
    [Comment] NTEXT NULL, 
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Load_ToLoadStatus] FOREIGN KEY ([Status]) REFERENCES [dbo].[LoadStatus] ([Id])
);


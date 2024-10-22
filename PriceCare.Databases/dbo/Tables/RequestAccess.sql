CREATE TABLE [dbo].[RequestAccess] (
    [Id]     INT            IDENTITY (1, 1) NOT NULL,
    [Email]  NVARCHAR (MAX) NOT NULL,
    [Reason] NVARCHAR (MAX) NOT NULL,
    [Date]   DATETIME       NOT NULL,
    [Status] INT NOT NULL, 
    [DateStatusChanged] DATETIME NULL, 
    [UserStatusChanged] NVARCHAR(MAX) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


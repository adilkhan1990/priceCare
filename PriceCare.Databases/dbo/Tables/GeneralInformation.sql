CREATE TABLE [dbo].[GeneralInformation] (
    [Id]                   INT            IDENTITY (1, 1) NOT NULL,
    [ContactPerson]        NVARCHAR (MAX) NOT NULL,
    [ContactMail]          NVARCHAR (MAX) NOT NULL,
    [TechnicalSupportMail] NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_GeneralInformation] PRIMARY KEY CLUSTERED ([Id] ASC)
);


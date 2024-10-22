CREATE TABLE [dbo].[InvitationRole]
(
	[InvitationId] INT NOT NULL, 
    [RoleId] NVARCHAR(128) NOT NULL, 
    CONSTRAINT [PK_InvitationRole] PRIMARY KEY ([InvitationId],[RoleId]), 
    CONSTRAINT [FK_InvitationRole_ToInvitation] FOREIGN KEY ([InvitationId]) REFERENCES [Invitation]([Id])
)

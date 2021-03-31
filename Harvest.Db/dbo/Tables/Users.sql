CREATE TABLE [dbo].[Users] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [FirstName] NVARCHAR (50)  NOT NULL,
    [LastName]  NVARCHAR (50)  NOT NULL,
    [Email]     NVARCHAR (300) NOT NULL,
    [Iam]       NVARCHAR (10)  NULL,
    [Kerberos]  NVARCHAR (20)  NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_Users_Kerberos]
    ON [dbo].[Users]([Kerberos] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Users_Iam]
    ON [dbo].[Users]([Iam] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Users_Email]
    ON [dbo].[Users]([Email] ASC);


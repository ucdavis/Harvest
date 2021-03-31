CREATE TABLE [dbo].[Permissions] (
    [Id]     INT IDENTITY (1, 1) NOT NULL,
    [RoleId] INT NOT NULL,
    [UserId] INT NOT NULL,
    CONSTRAINT [PK_Permissions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Permissions_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([Id]),
    CONSTRAINT [FK_Permissions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Permissions_UserId]
    ON [dbo].[Permissions]([UserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Permissions_RoleId]
    ON [dbo].[Permissions]([RoleId] ASC);


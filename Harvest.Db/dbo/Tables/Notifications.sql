CREATE TABLE [dbo].[Notifications] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [ProjectId] INT            NOT NULL,
    [Email]     NVARCHAR (300) NOT NULL,
    [Subject]   NVARCHAR (300) NOT NULL,
    [Body]      NVARCHAR (MAX) NOT NULL,
    [Sent]      BIT            NOT NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Notifications_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[Projects] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Notifications_ProjectId]
    ON [dbo].[Notifications]([ProjectId] ASC);


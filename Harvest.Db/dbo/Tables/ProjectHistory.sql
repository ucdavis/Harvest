CREATE TABLE [dbo].[ProjectHistory] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [ProjectId]   INT            NOT NULL,
    [Action]      NVARCHAR (200) NULL,
    [Description] NVARCHAR (MAX) NULL,
    [Type]        NVARCHAR (50)  NULL,
    [Actor]       INT            NOT NULL,
    [ActorName]   NVARCHAR (50)  NULL,
    [ActionDate]  DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_ProjectHistory] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ProjectHistory_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[Projects] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_ProjectHistory_ProjectId]
    ON [dbo].[ProjectHistory]([ProjectId] ASC);


CREATE TABLE [dbo].[Fields] (
    [Id]        INT               IDENTITY (1, 1) NOT NULL,
    [ProjectId] INT               NOT NULL,
    [Crop]      NVARCHAR (450)    NULL,
    [Details]   NVARCHAR (MAX)    NULL,
    [Location]  [sys].[geography] NULL,
    [IsActive]  BIT               NOT NULL,
    CONSTRAINT [PK_Fields] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Fields_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[Projects] ([Id])
);
GO

CREATE NONCLUSTERED INDEX [IX_Fields_Crop]
    ON [dbo].[Fields]([Crop] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Fields_ProjectId]
    ON [dbo].[Fields]([ProjectId] ASC);
GO

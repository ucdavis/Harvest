CREATE TABLE [dbo].[Accounts] (
    [Id]           INT             IDENTITY (1, 1) NOT NULL,
    [ProjectId]    INT             NOT NULL,
    [Number]       NVARCHAR (100)  NULL,
    [Name]         NVARCHAR (200)  NULL,
    [Percentage]   DECIMAL (18, 2) NOT NULL,
    [ApprovedById] INT             NULL,
    [ApprovedOn]   DATETIME2 (7)   NULL,
    CONSTRAINT [PK_Accounts] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Accounts_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[Projects] ([Id]),
    CONSTRAINT [FK_Accounts_Users_ApprovedById] FOREIGN KEY ([ApprovedById]) REFERENCES [dbo].[Users] ([Id])
);




GO
CREATE NONCLUSTERED INDEX [IX_Accounts_ProjectId]
    ON [dbo].[Accounts]([ProjectId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Accounts_Number]
    ON [dbo].[Accounts]([Number] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Accounts_Name]
    ON [dbo].[Accounts]([Name] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Accounts_ApprovedById]
    ON [dbo].[Accounts]([ApprovedById] ASC);


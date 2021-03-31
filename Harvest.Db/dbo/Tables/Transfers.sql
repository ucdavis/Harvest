CREATE TABLE [dbo].[Transfers] (
    [Id]            INT             IDENTITY (1, 1) NOT NULL,
    [Amount]        DECIMAL (18, 2) NOT NULL,
    [Description]   NVARCHAR (40)   NULL,
    [FromAccountId] INT             NOT NULL,
    [ToAccountId]   INT             NOT NULL,
    CONSTRAINT [PK_Transfers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Transfers_Accounts_FromAccountId] FOREIGN KEY ([FromAccountId]) REFERENCES [dbo].[Accounts] ([Id]),
    CONSTRAINT [FK_Transfers_Accounts_ToAccountId] FOREIGN KEY ([ToAccountId]) REFERENCES [dbo].[Accounts] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Transfers_ToAccountId]
    ON [dbo].[Transfers]([ToAccountId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Transfers_FromAccountId]
    ON [dbo].[Transfers]([FromAccountId] ASC);


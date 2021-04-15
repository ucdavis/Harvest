CREATE TABLE [dbo].[Expenses] (
    [Id]          INT             IDENTITY (1, 1) NOT NULL,
    [Type]        NVARCHAR (15)   NOT NULL,
    [Description] NVARCHAR (250)  NOT NULL,
    [Price]       DECIMAL (18, 2) NOT NULL,
    [Quantity]    DECIMAL (18, 2) NOT NULL,
    [Total]       DECIMAL (18, 2) NOT NULL,
    [ProjectId]   INT             NOT NULL,
    [RateId]      INT             NOT NULL,
    [InvoiceId]   INT             NULL,
    [CreatedOn]   DATETIME2 (7)   NOT NULL,
    [CreatedById] INT             NULL,
    [Account]     NVARCHAR (50)   DEFAULT (N'') NOT NULL,
    CONSTRAINT [PK_Expenses] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Expenses_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [dbo].[Invoices] ([Id]),
    CONSTRAINT [FK_Expenses_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[Projects] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Expenses_Rates_RateId] FOREIGN KEY ([RateId]) REFERENCES [dbo].[Rates] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Expenses_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Expenses_RateId]
    ON [dbo].[Expenses]([RateId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Expenses_ProjectId]
    ON [dbo].[Expenses]([ProjectId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Expenses_InvoiceId]
    ON [dbo].[Expenses]([InvoiceId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Expenses_CreatedById]
    ON [dbo].[Expenses]([CreatedById] ASC);


CREATE TABLE [dbo].[Transfers] (
    [Id]        INT             IDENTITY (1, 1) NOT NULL,
    [Total]     DECIMAL (18, 2) NOT NULL,
    [InvoiceId] INT             NOT NULL,
    [Account]   NVARCHAR (50)   DEFAULT (N'') NOT NULL,
    [Type]      NVARCHAR (10)   DEFAULT (N'') NOT NULL,
    CONSTRAINT [PK_Transfers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Transfers_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [dbo].[Invoices] ([Id])
);




GO



GO
CREATE NONCLUSTERED INDEX [IX_Transfers_InvoiceId]
    ON [dbo].[Transfers]([InvoiceId] ASC);


CREATE TABLE [dbo].[Invoices] (
    [Id]                 INT             IDENTITY (1, 1) NOT NULL,
    [ProjectId]          INT             NOT NULL,
    [Total]              DECIMAL (18, 2) NOT NULL,
    [CreatedOn]          DATETIME2 (7)   NOT NULL,
    [Notes]              NVARCHAR (MAX)  NULL,
    [Status]             NVARCHAR (MAX)  NULL,
    [KfsTrackingNumber]  NVARCHAR (20)   NULL,
    [SlothTransactionId] NVARCHAR (50)   NULL,
    CONSTRAINT [PK_Invoices] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Invoices_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[Projects] ([Id]) ON DELETE CASCADE
);




GO
CREATE NONCLUSTERED INDEX [IX_Invoices_ProjectId]
    ON [dbo].[Invoices]([ProjectId] ASC);


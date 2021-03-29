CREATE TABLE [dbo].[Documents] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [QuoteId]    INT            NOT NULL,
    [Name]       NVARCHAR (50)  NULL,
    [Identifier] NVARCHAR (200) NOT NULL,
    CONSTRAINT [PK_Documents] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Documents_Quotes_QuoteId] FOREIGN KEY ([QuoteId]) REFERENCES [dbo].[Quotes] ([Id])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Documents_QuoteId]
    ON [dbo].[Documents]([QuoteId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Documents_Name]
    ON [dbo].[Documents]([Name] ASC);


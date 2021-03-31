CREATE TABLE [dbo].[Quotes] (
    [Id]                INT             IDENTITY (1, 1) NOT NULL,
    [ProjectId]         INT             NOT NULL,
    [Text]              NVARCHAR (MAX)  NULL,
    [Total]             DECIMAL (18, 2) NOT NULL,
    [InitatedById]      INT             NOT NULL,
    [CurrentDocumentId] INT             NULL,
    [ApprovedById]      INT             NULL,
    [ApprovedOn]        DATETIME2 (7)   NULL,
    [CreatedDate]       DATETIME2 (7)   NOT NULL,
    [Status]            NVARCHAR (50)   NULL,
    [InitiatedById]     INT             NULL,
    CONSTRAINT [PK_Quotes] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Quotes_Documents_CurrentDocumentId] FOREIGN KEY ([CurrentDocumentId]) REFERENCES [dbo].[Documents] ([Id]),
    CONSTRAINT [FK_Quotes_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[Projects] ([Id]),
    CONSTRAINT [FK_Quotes_Users_ApprovedById] FOREIGN KEY ([ApprovedById]) REFERENCES [dbo].[Users] ([Id]),
    CONSTRAINT [FK_Quotes_Users_InitiatedById] FOREIGN KEY ([InitiatedById]) REFERENCES [dbo].[Users] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Quotes_ProjectId]
    ON [dbo].[Quotes]([ProjectId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Quotes_InitiatedById]
    ON [dbo].[Quotes]([InitiatedById] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Quotes_InitatedById]
    ON [dbo].[Quotes]([InitatedById] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Quotes_CurrentDocumentId]
    ON [dbo].[Quotes]([CurrentDocumentId] ASC) WHERE ([CurrentDocumentId] IS NOT NULL);


GO
CREATE NONCLUSTERED INDEX [IX_Quotes_ApprovedById]
    ON [dbo].[Quotes]([ApprovedById] ASC);


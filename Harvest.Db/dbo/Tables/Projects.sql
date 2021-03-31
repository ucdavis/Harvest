CREATE TABLE [dbo].[Projects] (
    [Id]                      INT               IDENTITY (1, 1) NOT NULL,
    [Start]                   DATETIME2 (7)     NOT NULL,
    [End]                     DATETIME2 (7)     NOT NULL,
    [Crop]                    NVARCHAR (50)     NULL,
    [Requirements]            NVARCHAR (MAX)    NULL,
    [Name]                    NVARCHAR (200)    NULL,
    [PrincipalInvestigatorId] INT               NOT NULL,
    [Location]                [sys].[geography] NULL,
    [LocationCode]            NVARCHAR (50)     NULL,
    [QuoteId]                 INT               NULL,
    [QuoteTotal]              DECIMAL (18, 2)   NOT NULL,
    [ChargedTotal]            DECIMAL (18, 2)   NOT NULL,
    [CreatedById]             INT               NOT NULL,
    [Status]                  NVARCHAR (50)     NULL,
    [CurrentAccountVersion]   INT               NOT NULL,
    [IsActive]                BIT               NOT NULL,
    [CreatedOn]               DATETIME2 (7)     DEFAULT ('0001-01-01T00:00:00.0000000') NOT NULL,
    [QuoteId1]                INT               NULL,
    CONSTRAINT [PK_Projects] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Projects_Quotes_QuoteId1] FOREIGN KEY ([QuoteId1]) REFERENCES [dbo].[Quotes] ([Id]),
    CONSTRAINT [FK_Projects_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
    CONSTRAINT [FK_Projects_Users_PrincipalInvestigatorId] FOREIGN KEY ([PrincipalInvestigatorId]) REFERENCES [dbo].[Users] ([Id])
);




GO
CREATE NONCLUSTERED INDEX [IX_Projects_QuoteId1]
    ON [dbo].[Projects]([QuoteId1] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Projects_QuoteId]
    ON [dbo].[Projects]([QuoteId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Projects_Name]
    ON [dbo].[Projects]([Name] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Projects_CreatedById]
    ON [dbo].[Projects]([CreatedById] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Projects_PrincipalInvestigatorId]
    ON [dbo].[Projects]([PrincipalInvestigatorId] ASC);


CREATE TABLE [dbo].[Projects] (
    [Id]                      INT             IDENTITY (1, 1) NOT NULL,
    [Start]                   DATETIME2 (7)   NOT NULL,
    [End]                     DATETIME2 (7)   NOT NULL,
    [Crop]                    NVARCHAR (512)  NULL,
    [Requirements]            NVARCHAR (MAX)  NULL,
    [Name]                    NVARCHAR (200)  NULL,
    [PrincipalInvestigatorId] INT             NOT NULL,
    [QuoteId]                 INT             NULL,
    [QuoteTotal]              DECIMAL (18, 2) NOT NULL,
    [ChargedTotal]            DECIMAL (18, 2) NOT NULL,
    [CreatedById]             INT             NOT NULL,
    [Status]                  NVARCHAR (50)   NULL,
    [CurrentAccountVersion]   INT             NOT NULL,
    [IsActive]                BIT             NOT NULL,
    [CreatedOn]               DATETIME2 (7)   DEFAULT ('0001-01-01T00:00:00.0000000') NOT NULL,
    [AcreageRateId]           INT             NULL,
    [Acres]                   FLOAT (53)      DEFAULT ((0.0000000000000000e+000)) NOT NULL,
    [CropType]                NVARCHAR (50)   NULL,
    [IsApproved]              BIT             DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [OriginalProjectId]       INT             NULL,
    CONSTRAINT [PK_Projects] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Projects_Projects_OriginalProjectId] FOREIGN KEY ([OriginalProjectId]) REFERENCES [dbo].[Projects] ([Id]),
    CONSTRAINT [FK_Projects_Quotes_QuoteId] FOREIGN KEY ([QuoteId]) REFERENCES [dbo].[Quotes] ([Id]),
    CONSTRAINT [FK_Projects_Rates_AcreageRateId] FOREIGN KEY ([AcreageRateId]) REFERENCES [dbo].[Rates] ([Id]),
    CONSTRAINT [FK_Projects_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
    CONSTRAINT [FK_Projects_Users_PrincipalInvestigatorId] FOREIGN KEY ([PrincipalInvestigatorId]) REFERENCES [dbo].[Users] ([Id])
);
GO

CREATE NONCLUSTERED INDEX [IX_Projects_AcreageRateId]
    ON [dbo].[Projects]([AcreageRateId] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Projects_CreatedById]
    ON [dbo].[Projects]([CreatedById] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Projects_Name]
    ON [dbo].[Projects]([Name] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Projects_OriginalProjectId]
    ON [dbo].[Projects]([OriginalProjectId] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Projects_PrincipalInvestigatorId]
    ON [dbo].[Projects]([PrincipalInvestigatorId] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Projects_QuoteId]
    ON [dbo].[Projects]([QuoteId] ASC);
GO


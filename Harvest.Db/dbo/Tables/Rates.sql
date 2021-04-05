CREATE TABLE [dbo].[Rates] (
    [Id]          INT             IDENTITY (1, 1) NOT NULL,
    [IsActive]    BIT             NOT NULL,
    [Type]        NVARCHAR (15)   NOT NULL,
    [Description] NVARCHAR (250)  NOT NULL,
    [BillingUnit] NVARCHAR (50)   NULL,
    [Account]     NVARCHAR (50)   NOT NULL,
    [Price]       DECIMAL (18, 2) NOT NULL,
    [EffectiveOn] DATETIME2 (7)   NULL,
    [CreatedById] INT             NOT NULL,
    [UpdatedById] INT             NOT NULL,
    [CreatedOn]   DATETIME2 (7)   NOT NULL,
    [UpdatedOn]   DATETIME2 (7)   NOT NULL,
    [Unit]        NVARCHAR (50)   NULL,
    CONSTRAINT [PK_Rates] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO
CREATE NONCLUSTERED INDEX [IX_Rates_UpdatedById]
    ON [dbo].[Rates]([UpdatedById] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Rates_Type]
    ON [dbo].[Rates]([Type] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Rates_Description]
    ON [dbo].[Rates]([Description] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Rates_CreatedById]
    ON [dbo].[Rates]([CreatedById] ASC);


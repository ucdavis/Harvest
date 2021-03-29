CREATE TABLE [dbo].[TransferRequests] (
    [Id]                 INT              IDENTITY (1, 1) NOT NULL,
    [SlothTransactionId] UNIQUEIDENTIFIER NULL,
    [KfsTrackingNumber]  NVARCHAR (20)    NULL,
    [Description]        NVARCHAR (40)    NULL,
    [Status]             NVARCHAR (50)    NULL,
    [RequestedOn]        DATETIME2 (7)    NOT NULL,
    [IsDeleted]          BIT              NOT NULL,
    [RequestedById]      INT              NOT NULL,
    [ProjectId]          INT              NOT NULL,
    CONSTRAINT [PK_TransferRequests] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TransferRequests_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[Projects] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TransferRequests_Users_RequestedById] FOREIGN KEY ([RequestedById]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_TransferRequests_RequestedById]
    ON [dbo].[TransferRequests]([RequestedById] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TransferRequests_ProjectId]
    ON [dbo].[TransferRequests]([ProjectId] ASC);


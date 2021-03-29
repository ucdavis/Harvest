CREATE TABLE [dbo].[TransferHistory] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [TransferId]     INT            NOT NULL,
    [ActionDateTime] DATETIME2 (7)  NOT NULL,
    [Action]         NVARCHAR (50)  NULL,
    [ActorId]        NVARCHAR (50)  NULL,
    [ActorName]      NVARCHAR (250) NULL,
    [Status]         NVARCHAR (50)  NULL,
    [Notes]          NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_TransferHistory] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TransferHistory_TransferRequests_TransferId] FOREIGN KEY ([TransferId]) REFERENCES [dbo].[TransferRequests] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_TransferHistory_TransferId]
    ON [dbo].[TransferHistory]([TransferId] ASC);


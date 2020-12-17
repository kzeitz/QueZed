﻿CREATE TABLE [dbo].[Name] (
    [ID]        INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [FirstName] VARCHAR (255) NOT NULL,
    [LastName]  VARCHAR (255) NOT NULL,
    [added]     DATETIME2 (7) CONSTRAINT [DF_dbo.Name_added] DEFAULT (getdate()) NOT NULL,
    [edited]    DATETIME2 (7) NULL,
    [editor]    VARCHAR (64)  NULL,
    [deleted]   INT           CONSTRAINT [DF_dbo.Name_deleted] DEFAULT ((0)) NOT NULL,
    [version]   ROWVERSION    NULL,
    CONSTRAINT [PK_Name] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
CREATE TRIGGER dbo.IODTr_Name ON [dbo].[Name] INSTEAD OF DELETE AS 
GO
CREATE TRIGGER dbo.AIUTr_Name ON [dbo].[Name] AFTER INSERT, UPDATE
CREATE TABLE [dbo].[Name] (
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
CREATE TRIGGER dbo.IODTr_Name ON [dbo].[Name] INSTEAD OF DELETE AS BEGIN 	SET NOCOUNT ON;	UPDATE [dbo].[Name] SET deleted = 1, edited = GETDATE(), editor = SYSTEM_USER WHERE ID IN (SELECT ID FROM deleted) AND deleted <> 1	CREATE TABLE #Name (ID INT)	INSERT #Name SELECT ID FROM deleted	EXEC [app.utility].CascadeDelete '[dbo].[Name]'END
GO
CREATE TRIGGER dbo.AIUTr_Name ON [dbo].[Name] AFTER INSERT, UPDATEASBEGIN SET NOCOUNT ON;	UPDATE [dbo].[Name] SET added = GETDATE(), editor = SYSTEM_USER WHERE ID IN (SELECT ID FROM inserted) AND ID NOT IN (SELECT ID FROM deleted)	UPDATE [dbo].[Name] SET edited = GETDATE(), editor = SYSTEM_USER WHERE ID IN (SELECT ID FROM deleted)END
if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DS_AttachmentSynchroInfo]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DS_AttachmentSynchroInfo] (
	[ProviderName] [varchar](20) NOT NULL,
	[DocTBGuid] [uniqueidentifier] NOT NULL,
	[AttachmentID] [int] NOT NULL,
	[DocNamespace] [varchar](256) NULL CONSTRAINT DF_AttSynch_DocNamespace DEFAULT(''),
	[SynchStatus] [int] NULL CONSTRAINT DF_AttSynch_SynchStatus DEFAULT(31457280),
	[SynchDate] [datetime] NULL CONSTRAINT DF_AttSynch_SynchDate DEFAULT('17991231'),
	[SynchDirection] [int] NULL CONSTRAINT DF_AttSynch_SynchDirection DEFAULT(31522816),
	[LastAction] [int] NULL CONSTRAINT DF_AttSynch_LastAction DEFAULT(31588352),
	[WorkerID] [int] NULL CONSTRAINT DF_AttSynch_WorkerID DEFAULT(0),
 CONSTRAINT [PK_DS_AttachmentSynchroInfo] PRIMARY KEY NONCLUSTERED 
(
	[ProviderName] ASC,
	[DocTBGuid] ASC,
	[AttachmentID] ASC	
)) ON [PRIMARY]
END
GO
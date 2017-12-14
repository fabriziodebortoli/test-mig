if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_WFAttachments]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_WFAttachments](
	[AttachmentID] [int] NOT NULL,
	[ApprovalStatus] [int] NOT NULL CONSTRAINT DF_DMS_WFAttachments_ApprovedStatus DEFAULT (0),	
	[WorkerID] [int] NOT NULL,
	[RequestDate] [datetime] NOT NULL CONSTRAINT DF_DMS_WFAttachments_ApprovedDate DEFAULT (getdate()),
	[ApproverID] [int] NULL,
	[ApprovalDate] [datetime] NULL,
	[RequestComments] [varchar] (1024) NULL,
	[ApprovalComments] [varchar] (1024) NULL,
	CONSTRAINT [PK_DMS_WFAttachments] PRIMARY KEY CLUSTERED 
		(
			[AttachmentID] ASC,
			[WorkerID] ASC
		)
	) ON [PRIMARY]
END
GO
if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_ArchivedDocTextContent]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_ArchivedDocTextContent](
	[ArchivedDocID] [int] NOT NULL,
	[TextContent] [text] NOT NULL,
	CONSTRAINT [PK_DMS_ArchivedDocTextContent] PRIMARY KEY CLUSTERED 
	(
		[ArchivedDocID] ASC
	),
	CONSTRAINT [FK_DMS_ArchivedDocTextContent_DMS_ArchivedDocContent] FOREIGN KEY
	(
		[ArchivedDocID]
	) REFERENCES [DMS_ArchivedDocContent] (
		[ArchivedDocID]
	)) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
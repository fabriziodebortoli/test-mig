if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MA_EISelectedAttachment]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[MA_EISelectedAttachment](
	[ErpDocumentID] [int] NOT NULL,
	[DocNamespace] [varchar](256) NOT NULL,	
	[PrimaryKeyValue] [int] NOT NULL,
	[DescriptionValue] [varchar](256) NULL CONSTRAINT DF_EISelected_DescriptionValue DEFAULT(''),
	[IsSetByDefault] [char] (1) NULL CONSTRAINT DF_EISelected_IsSetByDefault DEFAULT ('0'),
	CONSTRAINT [PK_EISelectedAttachment] PRIMARY KEY CLUSTERED 
	(
		[ErpDocumentID],
		[DocNamespace],
		[PrimaryKeyValue]
	)) ON [PRIMARY] 
CREATE NONCLUSTERED INDEX [IX_EISelectedAttachment_ErpDocument1] ON [dbo].[MA_EISelectedAttachment]([DocNamespace] ASC,[PrimaryKeyValue] ASC) ON [PRIMARY]
END
GO
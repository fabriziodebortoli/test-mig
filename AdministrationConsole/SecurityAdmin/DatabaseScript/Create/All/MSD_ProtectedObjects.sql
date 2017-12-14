if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ProtectedObjects]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MSD_ProtectedObjects] (
	[CompanyId] [int] NOT NULL ,
	[ObjectId] [int] NOT NULL ,
	[Disabled] [bit] NOT NULL  DEFAULT (0),
	[FromGrant] [bit] NOT NULL  DEFAULT (0),
	CONSTRAINT [PK_MSD_ProtectedObjects] PRIMARY KEY  NONCLUSTERED 
	(
		[CompanyId],
		[ObjectId]
	),
	CONSTRAINT [FK_MSD_ProtectedObjects_Company] FOREIGN KEY 
	(
		[CompanyId]
	) REFERENCES [dbo].[MSD_Companies] (
		[CompanyId]
	),
	CONSTRAINT [FK_MSD_ProtectedObjects_Objects] FOREIGN KEY 
	(
		[ObjectId]
	) REFERENCES [dbo].[MSD_Objects] (
		[ObjectId]
	)
)
END
GO
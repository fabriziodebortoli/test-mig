if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ADVSecurity]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MSD_ADVSecurity] (
	[Code] [varchar] (256) NOT NULL ,
	[Adv] [varchar] (2048) NOT NULL DEFAULT (''),
	[HighDependency] [varchar] (2048) NOT NULL DEFAULT (''),
	CONSTRAINT [PK_MSD_ADVSecurity] PRIMARY KEY  NONCLUSTERED 
	(
		[Code]
	)
)
END
GO
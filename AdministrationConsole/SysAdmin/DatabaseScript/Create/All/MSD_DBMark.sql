if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_DBMark]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MSD_DBMark] (
	[Application] [varchar] (20) NOT NULL ,
	[AddOnModule] [varchar] (40) NOT NULL ,
	[DBRelease] [smallint] NOT NULL DEFAULT (-1),
	[ReleaseDate] [datetime] NOT NULL CONSTRAINT [DF_MSD_DBMark_RelDate] DEFAULT(GetDate()),
	[Status] [varchar] (1) NOT NULL DEFAULT (1),
	[ExecLevel3] [varchar] (1) NOT NULL DEFAULT (0),
	[UpgradeLevel] [smallint] NOT NULL DEFAULT (0),
	[Step] [smallint] NOT NULL DEFAULT (0),
	CONSTRAINT [PK_MSD_DBMark] PRIMARY KEY  NONCLUSTERED 
	(
		[Application],
		[AddOnModule]
	)
)
END
GO
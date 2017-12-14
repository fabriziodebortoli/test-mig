if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[TB_DBMark]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[TB_DBMark] (
	[Application] [varchar] (20) COLLATE Latin1_General_CI_AS  NOT NULL ,
	[AddOnModule] [varchar] (40) COLLATE Latin1_General_CI_AS NOT NULL ,
	[DBRelease] [smallint] NOT NULL CONSTRAINT [DF_TB_DBMark_DbRel] DEFAULT (-1),
	[ReleaseDate] [datetime] NOT NULL CONSTRAINT [DF_TB_DBMark_RelDate] DEFAULT(GetDate()),
	[Status] [char] (1) NOT NULL CONSTRAINT [DF_TB_DBMark_Status] DEFAULT (1),
	[ExecLevel3] [char] (1) NULL CONSTRAINT [DF_TB_DBMark_ExecLevel3] DEFAULT (0),
	[UpgradeLevel] [smallint] NOT NULL CONSTRAINT [DF_TB_DBMark_UpgradeLevel] DEFAULT (0),
	[Step] [smallint] NOT NULL CONSTRAINT [DF_TB_DBMark_Step] DEFAULT (0),
	CONSTRAINT [PK_TB_DBMark] PRIMARY KEY  NONCLUSTERED 
	(
		[Application],
		[AddOnModule]
	)
)
END
GO
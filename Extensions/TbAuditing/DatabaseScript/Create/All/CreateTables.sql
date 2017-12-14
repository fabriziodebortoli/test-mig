if not  exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[AUDIT_Namespaces]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[AUDIT_Namespaces] (
	[ID] [int] IDENTITY (1, 1) NOT NULL ,
	[Namespace] [varchar] (256) COLLATE Latin1_General_CI_AS NOT NULL ,
	[TypeNs] [smallint]  NOT NULL CONSTRAINT [DF_AUDIT_Namespaces_Type] DEFAULT (0)
	CONSTRAINT [PK_AUDIT_Namespaces] PRIMARY KEY  NONCLUSTERED 
	(
		[ID]
	)  ON [PRIMARY] 
) ON [PRIMARY]
END
GO

if not  exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[AUDIT_Tables]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[AUDIT_Tables] (
	[TableName] [varchar] (128) COLLATE Latin1_General_CI_AS NOT NULL ,
	[StartTrace] [datetime] NULL ,
	[Suspended] [char] (1)  NULL CONSTRAINT [DF_AUDIT_Tables_Suspended] DEFAULT (0),
	CONSTRAINT [PK_AUDIT_Tables] PRIMARY KEY  NONCLUSTERED 
	(
		[TableName]
	)  ON [PRIMARY] 
) ON [PRIMARY]
END
GO


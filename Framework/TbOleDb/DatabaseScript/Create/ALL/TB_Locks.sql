if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[TB_Locks]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[TB_Locks](
	[TableName] [varchar](56) NOT NULL,
	[LockKey] [varchar](512) NOT NULL,
	[Context] [varchar](10) NOT NULL,
	[AuthenticationToken] [varchar](36) NOT NULL,
	[AccountName] [varchar](128) NOT NULL,
	[ProcessName] [varchar](256) NOT NULL,
	[ProcessGuid] [varchar](36) NOT NULL,
	[LockDate] [datetime] NOT NULL,
 CONSTRAINT [PK_TB_Locks] PRIMARY KEY CLUSTERED 
(
	[TableName] ASC,
	[LockKey] ASC
))ON [PRIMARY] 


CREATE NONCLUSTERED INDEX [IX_TB_Locks] ON [dbo].[TB_Locks]
(
	[TableName] ASC,
	[LockKey] ASC,
	[AuthenticationToken] ASC
) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_TB_Locks_1] ON [dbo].[TB_Locks]
(
	[TableName] ASC,
	[AuthenticationToken] ASC,
	[Context] ASC
) ON [PRIMARY]


CREATE NONCLUSTERED INDEX [IX_TB_Locks_2] ON [dbo].[TB_Locks]
(
	[AuthenticationToken] ASC,
	[Context] ASC
) ON [PRIMARY]


CREATE NONCLUSTERED INDEX [IX_TB_Locks_3] ON [dbo].[TB_Locks]
(
	[AuthenticationToken] ASC
) ON [PRIMARY]


CREATE NONCLUSTERED INDEX [IX_TB_Locks_4] ON [dbo].[TB_Locks]
(
	[ProcessGuid] ASC
) ON [PRIMARY]

ALTER TABLE [dbo].[TB_Locks] ADD  CONSTRAINT [DF_TB_Locks_ProcessGuid]  DEFAULT ('') FOR [ProcessGuid]
ALTER TABLE [dbo].[TB_Locks] ADD  CONSTRAINT [DF_TB_Locks_LockDate]  DEFAULT (getdate()) FOR [LockDate]

END
GO
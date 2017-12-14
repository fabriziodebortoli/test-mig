if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_SlaveLogins]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MSD_SlaveLogins] (
	[SlaveId] [int] NOT NULL ,
	[LoginId] [int] NOT NULL ,
	[SlaveDBUser] [varchar] (50) NOT NULL CONSTRAINT DF_SlaveLogins_DBUser DEFAULT (''),
	[SlaveDBPassword] [varchar] (128) NOT NULL CONSTRAINT DF_SlaveLogins_DBPassword DEFAULT (''),
	[SlaveDBWindowsAuthentication] [bit] NOT NULL CONSTRAINT DF_SlaveLogins_WinAuth DEFAULT (0),
	CONSTRAINT [PK_MSD_SlaveLogins] PRIMARY KEY  NONCLUSTERED 
	(
		[SlaveId],
		[LoginId]
	),
	CONSTRAINT [FK_MSD_SlaveLogins_CompanyDBSlaves] FOREIGN KEY 
	(
		[SlaveId]
	) REFERENCES [dbo].[MSD_CompanyDBSlaves] (
		[SlaveId]
	),
	CONSTRAINT [FK_MSD_SlaveLogins_Logins] FOREIGN KEY 
	(
		[LoginId]
	) REFERENCES [dbo].[MSD_Logins] (
		[LoginId]
	)
)
END
GO
if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_CompanyDBSlaves]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MSD_CompanyDBSlaves] (
	[SlaveId] [int] IDENTITY (1, 1) NOT NULL ,
	[CompanyId] [int] NOT NULL ,
	[Signature] [varchar] (20) NULL CONSTRAINT DF_CompanyDBSlaves_Signature DEFAULT(''),
	[ServerName] [varchar] (255) NULL CONSTRAINT DF_CompanyDBSlaves_Server DEFAULT(''),
	[DatabaseName] [varchar] (255) NULL CONSTRAINT DF_CompanyDBSlaves_Database DEFAULT(''),
	[SlaveDBOwner] [int] NULL CONSTRAINT DF_CompanyDBSlaves_DBOwner DEFAULT (0),
	CONSTRAINT [PK_MSD_CompanyDBSlaves] PRIMARY KEY NONCLUSTERED 
	(
		[SlaveId]
	),
	CONSTRAINT [FK_MSD_CompanyDBSlaves_Companies] FOREIGN KEY 
	(
		[CompanyId]
	) REFERENCES [dbo].[MSD_Companies] (
		[CompanyId]
	),
	CONSTRAINT [FK_MSD_CompanyDBSlaves_Logins] FOREIGN KEY 
	(
		[SlaveDBOwner]
	) REFERENCES [dbo].[MSD_Logins] (
		[LoginId]
	)
)
END
GO
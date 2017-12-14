if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_SLAccessAttributes]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
	CREATE TABLE [dbo].[MSD_SLAccessAttributes] (
		[ObjectId] [int] NOT NULL ,
		[CompanyId] [int] NOT NULL DEFAULT(-1),
		[UserId] [int] NOT NULL DEFAULT(-1),
		[AttributesMask] [int] NULL CONSTRAINT [DF_SLAccessAttributes_AttributesMask] DEFAULT (1),
		CONSTRAINT [PK_MSD_SLAccessAttributes] PRIMARY KEY NONCLUSTERED 
		(
			[ObjectId],
			[CompanyId],
			[UserId]
		),
		CONSTRAINT [FK_MSD_SLAccessAttributes_Objects] FOREIGN KEY 
		(
			[ObjectId]
		) REFERENCES [dbo].[MSD_SLObjects] (
			[ObjectId]
		),
		CONSTRAINT [FK_MSD_SLAccessAttributes_Companies] FOREIGN KEY 
		(
			[CompanyId]
		) REFERENCES [dbo].[MSD_Companies] (
			[CompanyId]
		),
		CONSTRAINT [FK_MSD_SLAccessAttributes_Users] FOREIGN KEY 
		(
			[UserId]
		) REFERENCES [dbo].[MSD_Logins] (
			[LoginId]
		)
	)
END
GO

if exists (select dbo.sysobjects.name from dbo.sysobjects where dbo.sysobjects.name = 'FK_MSD_SLAccessAttributes_Companies')
	ALTER TABLE [dbo].[MSD_SLAccessAttributes] NOCHECK CONSTRAINT [FK_MSD_SLAccessAttributes_Companies]
GO

if exists (select dbo.sysobjects.name from dbo.sysobjects where dbo.sysobjects.name = 'FK_MSD_SLAccessAttributes_Users')
	ALTER TABLE [dbo].[MSD_SLAccessAttributes] NOCHECK CONSTRAINT [FK_MSD_SLAccessAttributes_Users]
GO
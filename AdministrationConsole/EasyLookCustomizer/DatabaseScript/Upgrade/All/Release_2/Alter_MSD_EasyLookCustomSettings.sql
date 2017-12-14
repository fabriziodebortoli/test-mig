if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_EasyLookCustomSettings]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
ALTER TABLE [dbo].[MSD_EasyLookCustomSettings] WITH NOCHECK 
	ADD CONSTRAINT [FK_MSD_EasyLookCustomSettings_CompanyLogins] FOREIGN KEY 
	(
		[CompanyId],
		[LoginId]
	) REFERENCES [MSD_CompanyLogins] (
		[CompanyId],
		[LoginId]
	)
GO

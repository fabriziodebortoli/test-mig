if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_EasyLookCustomSettings]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
	CREATE TABLE [dbo].[MSD_EasyLookCustomSettings] (
		[CompanyId] [int] NOT NULL ,
		[LoginId] [int] NOT NULL ,
		[LogoImageURL] [nvarchar] (255) NULL ,
		[AppPanelBkgndColor] [int] NULL ,
		[GroupsPanelBkgndColor] [int] NULL ,
		[GroupsPanelBkgndImageURL] [nvarchar] (255) NULL ,
		[MenuTreeBkgndColor] [int] NULL ,
		[CommandListBkgndColor] [int] NULL,
		[FontFamily] [varchar] (50) NULL ,
		[AllUsersReportTitleColor] [int] NULL ,
		[CurrentUserReportTitleColor] [int] NULL ,
		[MaxWrmHistoryNum] [int] NULL ,
		[WrmHistoryAutoDelEnabled] [bit] NULL ,
		[WrmHistoryAutoDelType] [int] NULL ,
		CONSTRAINT [PK_MSD_EasyLookCustomSettings] PRIMARY KEY  CLUSTERED 
		(
			[CompanyId],
			[LoginId]
		)  ON [PRIMARY] ,
		CONSTRAINT [FK_MSD_EasyLookCustomSettings_CompanyLogins] FOREIGN KEY 
		(
			[CompanyId],
			[LoginId]
		) REFERENCES [MSD_CompanyLogins] (
			[CompanyId],
			[LoginId]
		)
	)
END
GO

ALTER TABLE [dbo].[MSD_EasyLookCustomSettings] NOCHECK CONSTRAINT [FK_MSD_EasyLookCustomSettings_CompanyLogins]
GO

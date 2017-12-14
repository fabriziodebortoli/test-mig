if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_EasyLookCustomSettings]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
ALTER TABLE [dbo].[MSD_EasyLookCustomSettings]
ADD 
[AllUsersReportTitleColor] [int] NULL,
[CurrentUserReportTitleColor] [int] NULL
GO

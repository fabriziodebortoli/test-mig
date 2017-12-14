if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MSD_CompanyRoles' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name = 'Email')
ALTER TABLE [dbo].[MSD_CompanyRoles] 
ADD [Email] [varchar] (255) NOT NULL DEFAULT ('')
GO


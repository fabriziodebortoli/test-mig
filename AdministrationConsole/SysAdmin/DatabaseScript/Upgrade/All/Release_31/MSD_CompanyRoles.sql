if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MSD_CompanyRoles' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name IN ('ReadOnly'))
ALTER TABLE [dbo].[MSD_CompanyRoles] 
ADD [ReadOnly] [bit] NOT NULL DEFAULT (0)
GO
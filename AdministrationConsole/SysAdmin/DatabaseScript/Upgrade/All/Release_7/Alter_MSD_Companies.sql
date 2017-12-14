if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MSD_Companies' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name = 'ApplicationLanguage')
ALTER TABLE [dbo].[MSD_Companies] 
ADD [ApplicationLanguage] [varchar] (10) NOT NULL DEFAULT ('')
GO
if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MSD_CompanyLogins' and dbo.sysobjects.id = dbo.syscolumns.id and dbo.syscolumns.name = 'ssoid')
ALTER TABLE [dbo].[MSD_CompanyLogins] 
ADD ssoid [varchar] (40) NULL CONSTRAINT DF_CompanyLogins_ssoid DEFAULT ('')
GO


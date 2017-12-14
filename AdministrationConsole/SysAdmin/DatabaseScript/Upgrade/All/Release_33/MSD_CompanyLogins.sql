if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MSD_CompanyLogins' and dbo.sysobjects.id = dbo.syscolumns.id and dbo.syscolumns.name = 'MMMENUID')
ALTER TABLE [dbo].[MSD_CompanyLogins] 
ADD [MMMENUID] [varchar] (32) NULL CONSTRAINT DF_CompanyLogins_MMMENUID DEFAULT ('')
GO


if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MSD_CompanyLogins' and dbo.sysobjects.id = dbo.syscolumns.id and dbo.syscolumns.name = 'InfinityData')
ALTER TABLE [dbo].[MSD_CompanyLogins] 
ADD [InfinityData] [varchar] (256) NULL CONSTRAINT DF_CompanyLogins_InfinityData DEFAULT ('')
GO


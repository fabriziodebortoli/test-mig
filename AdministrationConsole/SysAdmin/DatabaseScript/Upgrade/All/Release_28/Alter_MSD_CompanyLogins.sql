if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MSD_CompanyLogins' and dbo.sysobjects.id = dbo.syscolumns.id and dbo.syscolumns.name = 'TB_Modified')
ALTER TABLE [dbo].[MSD_CompanyLogins] 
ADD [TB_Modified] [datetime] NULL
GO


if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MSD_CompanyLogins' and dbo.sysobjects.id = dbo.syscolumns.id and dbo.syscolumns.name = 'OtherData')
ALTER TABLE [dbo].[MSD_CompanyLogins] 
ADD [OtherData] [varchar] (128) NULL CONSTRAINT DF_CompanyLogins_OtherData DEFAULT ('')
GO


if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'XE_SiteParams' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name = 'ExportPath')

BEGIN
	ALTER TABLE [dbo].[XE_SiteParams] 
	ADD [ExportPath] [varchar] (256) NULL CONSTRAINT DF_SiteParam_ExpPath_00 DEFAULT('') 
END
GO
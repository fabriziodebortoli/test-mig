if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DS_Providers' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('SkipCrtValidation'))
ALTER TABLE [dbo].[DS_Providers] 
	ADD [SkipCrtValidation] [char](1) NULL CONSTRAINT DF_Providers_SkipCrtValidation DEFAULT ('0')
GO

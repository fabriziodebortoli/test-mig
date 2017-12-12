if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects where 
    dbo.sysobjects.name = 'MSD_ADVSecurity' and dbo.sysobjects.id = dbo.syscolumns.id 
    and dbo.syscolumns.name = 'HighDependency')
ALTER TABLE [dbo].[MSD_ADVSecurity] 
ADD [HighDependency] [varchar] (2048) NOT NULL DEFAULT ('')
GO

UPDATE [dbo].[MSD_ADVSecurity] SET [dbo].[MSD_ADVSecurity].[HighDependency] = '' 
WHERE [dbo].[MSD_ADVSecurity].[HighDependency] IS NULL
GO
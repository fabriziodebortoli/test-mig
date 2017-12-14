if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MSD_Logins' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name = 'PreferredLanguage')
UPDATE [MSD_Logins] SET [PreferredLanguage] = '' WHERE [PreferredLanguage] = 'Native'
GO
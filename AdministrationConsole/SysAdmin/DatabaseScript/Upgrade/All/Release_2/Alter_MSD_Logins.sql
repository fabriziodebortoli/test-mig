if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MSD_Logins' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name IN ('SmartClientAccess', 'WebAccess'))
ALTER TABLE [dbo].[MSD_Logins] 
ADD [SmartClientAccess] [bit] NOT NULL DEFAULT (1), 
[WebAccess] [bit] NOT NULL DEFAULT (1)
GO
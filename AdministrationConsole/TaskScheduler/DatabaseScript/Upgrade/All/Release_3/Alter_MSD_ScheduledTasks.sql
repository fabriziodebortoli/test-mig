if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MSD_ScheduledTasks' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name = 'AppBuild')
ALTER TABLE [dbo].[MSD_ScheduledTasks] 
DROP COLUMN [AppBuild]
GO


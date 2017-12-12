if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MSD_ScheduledTasks' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name = 'MessageContent')
ALTER TABLE [dbo].[MSD_ScheduledTasks] 
ADD [MessageContent] [varchar] (255) NULL
GO


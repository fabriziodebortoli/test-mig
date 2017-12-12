if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_SchedulerMailNotifications]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
  CREATE TABLE [dbo].[MSD_SchedulerMailNotifications] (
	[TaskId] [uniqueidentifier] NOT NULL, 
	[RecipientName] [varchar] (254) NOT NULL  DEFAULT (''),
	[SendCondition] [smallint] NOT NULL DEFAULT (0),
	CONSTRAINT [PK_MSD_SchedulerMailNotifications] PRIMARY KEY  NONCLUSTERED 
	(
		[TaskId],
		[RecipientName]
	)  ON [PRIMARY] ,
	CONSTRAINT [FK_MSD_SchedulerMailNotifications_MSD_ScheduledTasks] FOREIGN KEY 
	(
		[TaskId]
	) REFERENCES [MSD_ScheduledTasks] (
		[Id]
	) ON DELETE CASCADE  ON UPDATE CASCADE 
  ) ON [PRIMARY]
 END
GO



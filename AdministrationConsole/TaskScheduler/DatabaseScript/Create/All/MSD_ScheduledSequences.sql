if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ScheduledSequences]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
	CREATE TABLE [dbo].[MSD_ScheduledSequences] (
		[SequenceId] [uniqueidentifier] NOT NULL ,
		[TaskId] [uniqueidentifier] NOT NULL ,
		[TaskIndex] [smallint] NOT NULL ,
		[BlockingMode] [bit] NOT NULL DEFAULT (0),
		CONSTRAINT [PK_MSD_SchedulerSequences] PRIMARY KEY  NONCLUSTERED 
		(
			[SequenceId],
			[TaskId],
			[TaskIndex]
		)  ON [PRIMARY] ,
		CONSTRAINT [FK_MSD_ScheduledSequences_MSD_ScheduledTasks] FOREIGN KEY 
		(
			[TaskId]
		) REFERENCES [MSD_ScheduledTasks] (
			[Id]
		),
		CONSTRAINT [FK_MSD_ScheduledSequences_MSD_ScheduledTasks1] FOREIGN KEY 
		(
			[SequenceId]
		) REFERENCES [MSD_ScheduledTasks] (
			[Id]
		)
	) ON [PRIMARY]
 END
GO


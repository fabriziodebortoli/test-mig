if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ScheduledTasks]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
	CREATE TABLE [dbo].[MSD_ScheduledTasks] (
		[Id] [uniqueidentifier] NOT NULL DEFAULT (newid()),
		[Code] [varchar] (10) NOT NULL ,
		[CompanyId] [int] NOT NULL ,
		[LoginId] [int] NOT NULL ,
		[AppConfig] [varchar] (30) NULL ,
		[Type] [int] NOT NULL ,
		[RunningOptions] [int] NOT NULL DEFAULT (0),
		[Enabled] [bit] NULL ,
		[Command] [varchar] (255) NULL ,
		[Description] [varchar] (128) NULL ,
		[FrequencyType] [int] NOT NULL DEFAULT (0),
		[FrequencySubtype] [int] NOT NULL DEFAULT (0),
		[FrequencyInterval] [int] NULL ,
		[FrequencySubinterval] [int] NULL ,
		[FrequencyRelativeInterval] [int] NULL ,
		[FrequencyRecurringFactor] [int] NULL ,
		[ActiveStartDate] [datetime] NULL ,
		[ActiveEndDate] [datetime] NULL ,
		[LastRunDate] [datetime] NULL ,
		[LastRunRetries] [datetime] NULL ,
		[LastRunCompletitionLevel] [smallint] NOT NULL DEFAULT (0),
		[NextRunDate] [datetime] NULL ,
		[RetryAttempts] [int] NULL ,
		[RetryDelay] [int] NULL ,
		[RetryAttemptsActualCount] [int] NULL ,
		[SendMailUsingSMTP] [bit] NULL DEFAULT (1),
		[CyclicRepeat] [int] NULL ,
		[CyclicDelay] [int] NULL ,
		[CyclicTaskCode] [varchar] (10) NULL ,
		[ImpersonationDomain] [varchar] (255) NULL ,
		[ImpersonationUser] [varchar] (255) NULL ,
		[ImpersonationPassword] [nvarchar] (255) NULL ,
		[MessageContent] [varchar] (255) NULL ,
		CONSTRAINT [PK_MSD_ScheduledTasks] PRIMARY KEY  NONCLUSTERED 
		(
			[Id]
		)  ON [PRIMARY] ,
		CONSTRAINT [FK_MSD_ScheduledTasks_CompanyLogins] FOREIGN KEY 
		(
			[CompanyId],
			[LoginId]
		) REFERENCES [MSD_CompanyLogins] (
			[CompanyId],
			[LoginId]
		)
	) ON [PRIMARY]
	CREATE UNIQUE INDEX [IX_MSD_ScheduledTasks_Code] ON [dbo].[MSD_ScheduledTasks]([Code], [CompanyId], [LoginId])
 END
GO


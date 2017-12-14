if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[RM_Calendars]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[RM_Calendars] (
    [Calendar] [varchar] (8) NOT NULL,
    [Description] [varchar] (32) NULL CONSTRAINT [DF_RM_Calendars_Description] DEFAULT(''),
    [ExcludedDays] [smallint] NULL CONSTRAINT [DF_RM_Calendars_ExcludedDays] DEFAULT(0),
    [ExcludedMonths] [smallint] NULL CONSTRAINT [DF_RM_Calendars_ExcludedMonths] DEFAULT(0),
    [ShiftDays] [smallint] NULL CONSTRAINT [DF_RM_Calendars_ShiftDays] DEFAULT(0),
    [MoveShiftsOnExclDays] [char] (1) NULL CONSTRAINT [DF_RM_Calendars_MoveShifts] DEFAULT('0'),
    [Notes] [varchar] (64) NULL CONSTRAINT [DF_RM_Calendars_Notes] DEFAULT(''),
   CONSTRAINT [PK_RM_Calendars] PRIMARY KEY NONCLUSTERED 
    (
        [Calendar]
    ) ON [PRIMARY]
) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[RM_CalendarsShifts]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[RM_CalendarsShifts] (
    [Calendar] [varchar] (8) NOT NULL,
    [Line] [smallint] NOT NULL,
    [DayNo] [smallint] NULL CONSTRAINT [DF_RM_CalendarsShifts_DayNo] DEFAULT(0),
    [StartingHour] [smallint] NULL CONSTRAINT [DF_RM_CalendarsShifts_StartingHour] DEFAULT(0),
    [StartingMinute] [smallint] NULL CONSTRAINT [DF_RM_CalendarsShifts_StartingMinute] DEFAULT(0),
    [EndingHour] [smallint] NULL CONSTRAINT [DF_RM_CalendarsShifts_EndingHour] DEFAULT(0),
    [EndingMinute] [smallint] NULL CONSTRAINT [DF_RM_CalendarsShifts_EndingMinute] DEFAULT(0),
    [Notes] [varchar] (64) NULL CONSTRAINT [DF_RM_CalendarsShifts_Notes] DEFAULT(''),
   CONSTRAINT [PK_RM_CalendarsShifts] PRIMARY KEY NONCLUSTERED 
    (
        [Calendar],
        [Line]
    ) ON [PRIMARY],
   CONSTRAINT [FK_RM_CalendarsShifts_00] FOREIGN KEY
    (
        [Calendar]
    ) REFERENCES [dbo].[RM_Calendars] (
        [Calendar]
    )
) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[RM_CalendarsHolidays]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[RM_CalendarsHolidays] (
    [Calendar] [varchar] (8) NOT NULL,
    [Line] [smallint] NOT NULL,
    [StartingDay] [datetime] NULL CONSTRAINT [DF_RM_CalendarsHolidays_StartingDay] DEFAULT('17991231'),
    [EndingDay] [datetime] NULL CONSTRAINT [DF_RM_CalendarsHolidays_EndingDay] DEFAULT('17991231'),
    [ReasonOfExclusion] [varchar] (64) NULL CONSTRAINT [DF_RM_CalendarsHolidays_ReasonOfEx] DEFAULT(''),
    [Notes] [varchar] (64) NULL CONSTRAINT [DF_RM_CalendarsHolidays_Notes] DEFAULT(''),
   CONSTRAINT [PK_RM_CalendarsHolidays] PRIMARY KEY NONCLUSTERED 
    (
        [Calendar],
        [Line]
    ) ON [PRIMARY],
   CONSTRAINT [FK_RM_CalendarsHolidays_00] FOREIGN KEY
    (
        [Calendar]
    ) REFERENCES [dbo].[RM_Calendars] (
        [Calendar]
    )
) ON [PRIMARY]
END
GO


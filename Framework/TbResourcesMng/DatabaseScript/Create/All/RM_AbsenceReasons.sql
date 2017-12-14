if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[RM_AbsenceReasons]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[RM_AbsenceReasons] (
    [Reason] [varchar] (8) NOT NULL,
    [Description] [varchar] (32) NULL CONSTRAINT [DF_RM_AbsenceReasons_Description] DEFAULT(''),
    [Notes] [varchar] (64) NULL CONSTRAINT [DF_RM_AbsenceReasons_Notes] DEFAULT(''),
   CONSTRAINT [PK_RM_AbsenceReasons] PRIMARY KEY NONCLUSTERED 
    (
        [Reason]
    ) ON [PRIMARY]
) ON [PRIMARY]
END
GO


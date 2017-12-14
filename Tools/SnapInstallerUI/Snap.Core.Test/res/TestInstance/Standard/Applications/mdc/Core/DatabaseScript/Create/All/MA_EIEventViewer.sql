if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MA_EIEventViewer]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MA_EIEventViewer] (
	[DocCRType] [int] NOT NULL,
	[DocID] [int] NOT NULL,
	[Line] [smallint] NOT NULL,
	[EventDate] [datetime] NULL CONSTRAINT DF_EventVw_EventDate_00 DEFAULT('17991231'),
    [Event_Type] [int] NULL CONSTRAINT DF_EventVw_EventType_00 DEFAULT(32243712),
	[Event_Description] [varchar] (255) NULL CONSTRAINT DF_EventVw_EventDe_00 DEFAULT (''),
	[Event_XML] [ntext] NULL CONSTRAINT DF_EventVw_EventXml_00 DEFAULT (''),
	[Event_String1] [varchar] (15) NULL CONSTRAINT DF_EventVw_EventNRec_00 DEFAULT (''),
	[Event_String2] [varchar] (3) NULL CONSTRAINT DF_EventVw_EventSt_00 DEFAULT (''),
    CONSTRAINT [PK_EIEventViewer] PRIMARY KEY NONCLUSTERED 
    (
		[DocCRType],
        [DocID],
		[Line]
    ) ON [PRIMARY]
) ON [PRIMARY]

END
GO


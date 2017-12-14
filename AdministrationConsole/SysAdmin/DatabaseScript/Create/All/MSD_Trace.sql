if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_Trace]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[MSD_Trace]
GO

CREATE TABLE [dbo].[MSD_Trace] (
	[Company] [varchar] (50) NOT NULL ,
	[Login] [varchar] (50)  NOT NULL ,
	[Data] [datetime] NOT NULL ,
	[Type] [smallint] NOT NULL ,
	[ProcessName] [varchar] (50) NOT NULL ,
	[WinUser] [varchar] (50)  NOT NULL ,
	[Location] [varchar] (50) NOT NULL 
) ON [PRIMARY]
GO
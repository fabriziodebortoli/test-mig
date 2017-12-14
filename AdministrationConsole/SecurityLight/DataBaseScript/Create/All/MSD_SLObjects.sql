if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_SLObjects]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
	BEGIN
		CREATE TABLE [dbo].[MSD_SLObjects] (
			[ObjectId] [int] IDENTITY (1, 1) NOT NULL ,
			[NameSpace] [varchar] (512) NOT NULL ,
			[Type] [int] NOT NULL ,
			CONSTRAINT [PK_MSD_SLObjects] PRIMARY KEY  NONCLUSTERED 
			(
				[ObjectId]
			)
		)

	CREATE UNIQUE INDEX [IX_MSD_SLObjects_NameSpaceType] ON [dbo].[MSD_SLObjects]([NameSpace], [Type])
	CREATE INDEX [IX_MSD_SLObjects_Type] ON [dbo].[MSD_SLObjects]([Type])

	END
GO
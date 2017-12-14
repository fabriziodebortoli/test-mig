if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ObjectTypes]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MSD_ObjectTypes] (
	[TypeId] [int] IDENTITY (1, 1) NOT NULL ,
	[Type] [int] NOT NULL ,
	[TypeName] [varchar] (255) NOT NULL ,
	CONSTRAINT [PK_OSL_ObjectTypes] PRIMARY KEY  NONCLUSTERED 
	(
		[TypeId]
	)
)

END
GO


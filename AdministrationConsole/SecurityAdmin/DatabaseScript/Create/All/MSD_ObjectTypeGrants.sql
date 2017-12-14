if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ObjectTypeGrants]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MSD_ObjectTypeGrants] (
	[TypeId] [int] NOT NULL ,
	[GrantMask] [int] NOT NULL ,
	[GrantName] [varchar] (255) NOT NULL ,
	CONSTRAINT [PK_MSD_ObjectTypeGrants] PRIMARY KEY  NONCLUSTERED 
	(
		[TypeId],
		[GrantMask]
	)
)

BEGIN
	DECLARE @newtypeid as integer 

	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (3, 'Function')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')

	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (4, 'Report')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 256, 'Export')

	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (5, 'Data Entry')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 8, 'Delete')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 16, 'Browse')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 32, 'Customize Form')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 64, 'Edit Query')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 128, 'Import')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 256, 'Export')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 512, 'Silent Mode')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1024, 'Extended Browse')

	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (6, 'Child Window')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')


	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (7, 'Batch')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 32, 'Customize Form')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 512, 'Silent Mode')

	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (8, 'Tab')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')

	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (9, 'Constraint')
		SET @newtypeid = @@IDENTITY

	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (10, 'Table')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 256, 'Export')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 128, 'Import')

	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (11, 'HotLink')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 256, 'Export')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 64, 'Edit Query')

	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (13, 'View')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 256, 'Export')

	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (14, 'Row View')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 16, 'Add Row')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 8, 'Delete Row')


	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (15, 'Grid')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 8, 'Delete Row')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 16, 'Add Row')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 32, 'Show Row View')

	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (16, 'Grid Column')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')

	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (17, 'Control')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')

	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (21, 'Finder Documents')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 16, 'Browse')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 32, 'Customize Form')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 64, 'Edit Query')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1024, 'Extended Browse')


	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (23, 'Word Document')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		
	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (24, 'Excel Document')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')

	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (25, 'Word Template')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')

	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (26, 'Excel Template')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
	
	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (30, 'Tabber')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')

	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (31, 'TileManager')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')
	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (32, 'Tile')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')

	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (33, 'Toolbar')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')
	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (34, 'ToolbarButton')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')

	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (35, 'EmbeddedView')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')

	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (36, 'PropertyGrid')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')

	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (37, 'TilePanelTab')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')
		
	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (38, 'TilePanel')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')


END

END
GO



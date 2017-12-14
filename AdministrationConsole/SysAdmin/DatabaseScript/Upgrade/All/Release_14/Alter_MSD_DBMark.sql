if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MSD_DBMark' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('Release'))
ALTER TABLE [dbo].[MSD_DBMark] 
DROP COLUMN [Release]
GO

EXEC SP_RENAME 'MSD_DBMark.DataRelease', 'ReleaseDate', 'COLUMN'
GO

UPDATE [dbo].[MSD_DBMark] SET [dbo].[MSD_DBMark].[ReleaseDate] = GetDate() 
WHERE [dbo].[MSD_DBMark].[ReleaseDate] IS NULL
GO
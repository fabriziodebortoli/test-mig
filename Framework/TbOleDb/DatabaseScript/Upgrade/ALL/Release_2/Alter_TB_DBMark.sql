if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'TB_DBMark' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('Release'))
ALTER TABLE [dbo].[TB_DBMark] 
DROP COLUMN [Release]
GO

EXEC SP_RENAME 'TB_DBMark.DataRelease', 'ReleaseDate', 'COLUMN'
GO

UPDATE [dbo].[TB_DBMark] SET [dbo].[TB_DBMark].[ReleaseDate] = GetDate() 
WHERE [dbo].[TB_DBMark].[ReleaseDate] IS NULL
GO
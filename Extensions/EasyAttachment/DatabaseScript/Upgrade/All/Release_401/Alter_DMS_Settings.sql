if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_Settings]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
UPDATE [DMS_Settings]
SET Settings.modify('delete /SettingState/Options/RepositoryOptions/EnableToOpenRepositoryMng')
WHERE Settings IS NOT NULL
END
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_Settings]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
UPDATE [DMS_Settings]
SET Settings.modify('delete /SettingState/Options/RepositoryOptions/EnableCheckInCheckOut')
WHERE Settings IS NOT NULL
END
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_Settings]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
UPDATE [DMS_Settings]
SET Settings.modify('delete /SettingState/Options/BookmarksOptions/EnableOpenCategories')
WHERE Settings IS NOT NULL
END
GO
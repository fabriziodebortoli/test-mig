if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_CompanyLogins]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
UPDATE [dbo].[MSD_CompanyLogins] 
SET [dbo].[MSD_CompanyLogins].[Admin] = 1
WHERE [dbo].[MSD_CompanyLogins].[DBUser] = 'EasyLookSystem'
END
GO
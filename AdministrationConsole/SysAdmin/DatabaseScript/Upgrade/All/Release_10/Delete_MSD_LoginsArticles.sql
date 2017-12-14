
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_LoginsArticles]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 
BEGIN 
DELETE FROM MSD_LoginsArticles
END
GO
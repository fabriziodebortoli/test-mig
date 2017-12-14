if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_LoginsArticles]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MSD_LoginsArticles] (
	[LoginId] [int] NOT NULL ,
	[Article] [varchar] (50) NOT NULL ,
	CONSTRAINT [PK_MSD_LoginsArticles] PRIMARY KEY  NONCLUSTERED 
	(
		[LoginId],
		[Article]
	)
)
END

GO



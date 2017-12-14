if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_Providers]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MSD_Providers] (
	[ProviderId] [int] IDENTITY (1, 1) NOT NULL ,
	[Provider] [varchar] (50) NOT NULL ,
	[Description] [varchar] (255) NULL ,
	[UseConstParameter] [bit] NOT NULL  DEFAULT (0),
	[StripTrailingSpaces] [bit] NOT NULL  DEFAULT (1)
	CONSTRAINT [PK_MSD_Providers] PRIMARY KEY  NONCLUSTERED 
	(
		[ProviderId]
	)
)
END

GO 

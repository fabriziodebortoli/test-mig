ALTER TABLE [dbo].[MSD_Companies] DROP CONSTRAINT FK_MSD_Companies_Providers
GO

DROP TABLE [MSD_Providers]
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_Providers]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MSD_Providers] (
	[ProviderId] [int] IDENTITY (1, 1) NOT NULL ,
	[Provider] [varchar] (50) NOT NULL ,
	[Description] [varchar] (255) NULL ,
	[UseConstParameter] [bit] NOT NULL  DEFAULT (0),
	[StripTrailingSpaces] [bit] NOT NULL  DEFAULT (0)
	CONSTRAINT [PK_MSD_Providers] PRIMARY KEY  NONCLUSTERED 
	(
		[ProviderId]
	)
)
END
GO

ALTER TABLE MSD_Companies WITH NOCHECK
ADD CONSTRAINT FK_MSD_Companies_Providers FOREIGN KEY 
(
	[ProviderId]
) REFERENCES [dbo].[MSD_Providers] (
	[ProviderId]
)
GO

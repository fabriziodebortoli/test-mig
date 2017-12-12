if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_SOSConfiguration]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
DROP TABLE [dbo].[DMS_SOSConfiguration]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_SOSConfiguration]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_SOSConfiguration](
	[ParamID] [int] NOT NULL,
	[SubjectCode] [varchar](20) NOT NULL,
	[AncestorCode] [varchar](20) NOT NULL,
	[KeeperCode] [varchar](20) NOT NULL,
	[MySOSUser] [varchar](50) NOT NULL,
	[MySOSPassword] [varchar](40) NOT NULL,
	[DocClasses] [xml] NULL,
	[SOSWebServiceUrl] [varchar](128) NOT NULL,
	CONSTRAINT [PK_DMS_SOSConfiguration] PRIMARY KEY CLUSTERED 
	(
		[ParamID] ASC
	)) ON [PRIMARY]
END
GO
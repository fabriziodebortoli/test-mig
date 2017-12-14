if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[RM_ResourceTypes]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[RM_ResourceTypes] (
	[ResourceType] [varchar] (8) NOT NULL ,
    [Description] [varchar] (64) NULL CONSTRAINT [DF_RM_ResourceTypes_Descri] DEFAULT (''),
	[ImagePath] [varchar] (128) COLLATE Latin1_General_CI_AS NULL CONSTRAINT [DF_RM_ResourceTypes_ImagePath] DEFAULT (''),
   CONSTRAINT [PK_RM_ResourceTypes] PRIMARY KEY NONCLUSTERED 
    (
        [ResourceType]
    ) ON [PRIMARY]  
) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[RM_Resources]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[RM_Resources] (
	[ResourceType] [varchar] (8) NOT NULL ,
    [ResourceCode] [varchar] (8) NOT NULL ,
    [Description] [varchar] (64) NULL CONSTRAINT [DF_RM_Resources_Description] DEFAULT('') ,
    [Manager] [int] NULL CONSTRAINT [DF_RM_Resources_Manager] DEFAULT(0) ,
    [Notes] [varchar] (64) NULL CONSTRAINT [DF_RM_Resources_Notes] DEFAULT('') ,
    [ImagePath] [varchar] (128) COLLATE Latin1_General_CI_AS NULL CONSTRAINT [DF_RM_Resources_ImagePath] DEFAULT('') ,
    [CostCenter] [varchar] (8) NULL CONSTRAINT [DF_RM_Resources_CostCenter] DEFAULT('') ,
    [Disabled] [char] (1) NULL CONSTRAINT [DF_RM_Resources_Disabled] DEFAULT('') ,
	[HideOnLayout] [char] (1) NULL CONSTRAINT [DF_RM_Resources_HideOnLayout] DEFAULT('') ,
	[DomicilyAddress] [varchar](128) NULL CONSTRAINT [DF_RM_Resources_DomicilyAddress] DEFAULT (''),
    [DomicilyCity] [varchar] (64) NULL CONSTRAINT [DF_RM_Resources_DomicilyCity] DEFAULT (''),
	[DomicilyCounty] [varchar](3) NULL CONSTRAINT [DF_RM_Resources_DomicilyCounty] DEFAULT (''),
    [DomicilyZip] [varchar] (10) NULL CONSTRAINT [DF_RM_Resources_DomicilyZip] DEFAULT (''),
	[DomicilyCountry] [varchar](64) NULL CONSTRAINT [DF_RM_Resources_DomicilyCountry] DEFAULT (''),
	[Telephone1] [varchar](20) NULL CONSTRAINT [DF_RM_Resources_Telephone1] DEFAULT (''),
	[Telephone2] [varchar](20) NULL CONSTRAINT [DF_RM_Resources_Telephone2] DEFAULT (''),
	[Telephone3] [varchar](20) NULL CONSTRAINT [DF_RM_Resources_Telephone3] DEFAULT (''),
	[Telephone4] [varchar](20) NULL CONSTRAINT [DF_RM_Resources_Telephone4] DEFAULT (''),
	[Email] [varchar](64) NULL CONSTRAINT [DF_RM_Resources_Email] DEFAULT (''),
	[URL] [varchar](64) NULL CONSTRAINT [DF_RM_Resources_URL] DEFAULT (''),
	[SkypeID] [varchar](64) NULL CONSTRAINT [DF_RM_Resources_SkypeID] DEFAULT (''),
	[Branch] [varchar](8) NULL CONSTRAINT [DF_RM_Resources_Branch] DEFAULT (''),
	[Latitude] [varchar](16) NULL CONSTRAINT [DF_RM_Resources_Latitude] DEFAULT (''),
	[Longitude] [varchar](16) NULL CONSTRAINT [DF_RM_Resources_Longitude]  DEFAULT (''),
    [Address2] [varchar] (64) NULL CONSTRAINT [DF_RM_Resources_Address2] DEFAULT (''),
    [StreetNo] [varchar] (10) NULL CONSTRAINT [DF_RM_Resources_StreetNo] DEFAULT (''),
    [District] [varchar] (64) NULL CONSTRAINT [DF_RM_Resources_District] DEFAULT (''),
    [FederalState] [varchar] (2) NULL CONSTRAINT [DF_RM_Resources_FederalState] DEFAULT (''),
    [ISOCountryCode] [varchar] (2) NULL CONSTRAINT [DF_RM_Resources_ISOCountry] DEFAULT (''),
   CONSTRAINT [PK_RM_Resources] PRIMARY KEY NONCLUSTERED 
    (
        [ResourceType],
        [ResourceCode]
    ) ON [PRIMARY]
) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[RM_ResourcesDetails]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[RM_ResourcesDetails] (
	[ResourceType] [varchar] (8) NOT NULL,
    [ResourceCode] [varchar] (8) NOT NULL,
	[IsWorker] [char] (1) NOT NULL,
	[ChildResourceType] [varchar] (8) NOT NULL,
    [ChildResourceCode] [varchar] (8) NOT NULL,   
    [ChildWorkerID] [int] NOT NULL,
   CONSTRAINT [PK_RM_ResourcesDetails] PRIMARY KEY NONCLUSTERED 
    (
        [ResourceType],
        [ResourceCode],
		[IsWorker],
        [ChildResourceType],
        [ChildResourceCode],
        [ChildWorkerID]
    ) ON [PRIMARY],
   CONSTRAINT [FK_RM_ResourcesDetails_00] FOREIGN KEY
    (
        [ResourceType],
        [ResourceCode]
    ) REFERENCES [dbo].[RM_Resources] (
        [ResourceType],
        [ResourceCode]
    )    
) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[RM_ResourcesFields]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[RM_ResourcesFields](
	[ResourceType] [varchar] (8) NOT NULL ,
    [ResourceCode] [varchar] (8) NOT NULL ,
	[Line] [smallint] NOT NULL,
	[FieldName] [varchar](64) NULL CONSTRAINT [DF_RM_ResourcesFields_FieldName] DEFAULT (''),
	[FieldValue] [varchar](256) NULL CONSTRAINT [DF_RM_ResourcesFields_FieldValue] DEFAULT (''),
	[Notes] [varchar](64) NULL CONSTRAINT [DF_RM_ResourcesFields_Notes] DEFAULT (''),
	[HideOnLayout] [char] (1) NULL CONSTRAINT [DF_RM_ResourcesFields_HideOnLayout] DEFAULT('') 
   CONSTRAINT [PK_RM_ResourcesFields] PRIMARY KEY NONCLUSTERED 
    (
        [ResourceType],
        [ResourceCode],
        [Line]
    ) ON [PRIMARY],
   CONSTRAINT [FK_RM_ResourcesFields_00] FOREIGN KEY
    (
        [ResourceType],
        [ResourceCode]
    ) REFERENCES [dbo].[RM_Resources] (
        [ResourceType],
        [ResourceCode]
    )    
) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[RM_ResourcesAbsences]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[RM_ResourcesAbsences] (
	[ResourceType] [varchar] (8) NOT NULL ,
    [ResourceCode] [varchar] (8) NOT NULL ,
    [Reason] [varchar] (8) NOT NULL ,
	[StartingDate] [datetime] NOT NULL ,
    [EndingDate] [datetime] NULL CONSTRAINT [DF_RM_ResourcesAbsences_EndingDate] DEFAULT('17991231') ,   
    [Manager] [int] NULL CONSTRAINT [DF_RM_ResourcesAbsences_Manager] DEFAULT(0) ,
    [Notes] [varchar] (64) NULL CONSTRAINT [DF_RM_ResourcesAbsences_Notes] DEFAULT('') ,
   CONSTRAINT [PK_RM_ResourcesAbsences] PRIMARY KEY NONCLUSTERED 
    (
        [ResourceType],
        [ResourceCode],
        [Reason],
        [StartingDate]
    ) ON [PRIMARY],
   CONSTRAINT [FK_RM_ResourcesAbsences_00] FOREIGN KEY
    (
        [ResourceType],
        [ResourceCode]
    ) REFERENCES [dbo].[RM_Resources] (
        [ResourceType],
        [ResourceCode]
    )    
) ON [PRIMARY]
END
GO
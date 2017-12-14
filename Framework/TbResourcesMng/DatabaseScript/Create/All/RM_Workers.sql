if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[RM_Workers]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[RM_Workers](
	[WorkerID] [int] NOT NULL,
	[Password] [varchar](128) NULL CONSTRAINT [DF_RM_Workers_Password]  DEFAULT (''),
	[PasswordMustBeChanged] [char](1) NULL CONSTRAINT [DF_RM_Workers_PasswordMustBeChanged]  DEFAULT ('0'),
	[PasswordCannotChange] [char](1) NULL CONSTRAINT [DF_RM_Workers_PasswordCannotChange]  DEFAULT ('0'),
	[PasswordNeverExpire] [char](1) NULL CONSTRAINT [DF_RM_Workers_PasswordNeverExpire]  DEFAULT ('1'),
	[PasswordNotRenewable] [char](1) NULL CONSTRAINT [DF_RM_Workers_PasswordNotRenewable]  DEFAULT ('0'),
	[PasswordExpirationDate] [datetime] NULL CONSTRAINT [DF_RM_Workers_PasswordExpirationDate]  DEFAULT ('17991231'),
	[PasswordAttemptsNumber] [smallint] NULL CONSTRAINT [DF_RM_Workers_PasswordAttemptsNumber]  DEFAULT (0),
	[Title] [varchar](8) NULL CONSTRAINT [DF_RM_Workers_Title]  DEFAULT (''),
	[Name] [varchar](32) NULL CONSTRAINT [DF_RM_Workers_Name]  DEFAULT (''),
	[LastName] [varchar](64) NULL CONSTRAINT [DF_RM_Workers_LastName]  DEFAULT (''),
	[Gender] [int] NULL CONSTRAINT [DF_RM_Workers_Gender] DEFAULT(2097152),
	[CompanyLogin] [varchar](50) NULL CONSTRAINT [DF_RM_Workers_CompanyLogin]  DEFAULT (''),
	[DomicilyAddress] [varchar](128) NULL CONSTRAINT [DF_RM_Workers_DomicilyAddress]  DEFAULT (''),
    [DomicilyCity] [varchar] (64) NULL CONSTRAINT [DF_RM_Workers_DomicilyCity]  DEFAULT (''),
	[DomicilyCounty] [varchar](3) NULL CONSTRAINT [DF_RM_Workers_DomicilyCounty]  DEFAULT (''),
    [DomicilyZip] [varchar] (10) NULL CONSTRAINT [DF_RM_Workers_DomicilyZip]  DEFAULT (''),
	[DomicilyCountry] [varchar](64) NULL CONSTRAINT [DF_RM_Workers_DomicilyCountry]  DEFAULT (''),
    [DomicilyFC] [varchar] (20) NULL CONSTRAINT [DF_RM_Workers_DomicilyFC]  DEFAULT (''),
	[DomicilyISOCode] [varchar](2) NULL CONSTRAINT [DF_RM_Workers_DomicilyISOCode]  DEFAULT (''),
	[Telephone1] [varchar](20) NULL CONSTRAINT [DF_RM_Workers_Telephone1] DEFAULT (''),
	[Telephone2] [varchar](20) NULL CONSTRAINT [DF_RM_Workers_Telephone2] DEFAULT (''),
	[Telephone3] [varchar](20) NULL CONSTRAINT [DF_RM_Workers_Telephone3] DEFAULT (''),
	[Telephone4] [varchar](20) NULL CONSTRAINT [DF_RM_Workers_Telephone4] DEFAULT (''),
	[Email] [varchar](64) NULL CONSTRAINT [DF_RM_Workers_Email]  DEFAULT (''),
	[URL] [varchar](64) NULL CONSTRAINT [DF_RM_Workers_URL]  DEFAULT (''),
	[SkypeID] [varchar](64) NULL CONSTRAINT [DF_RM_Workers_SkypeID]  DEFAULT (''),
	[CostCenter] [varchar](8) NULL CONSTRAINT [DF_MR_Workers_CostCenter]  DEFAULT (''),
	[HourlyCost] [float] NULL CONSTRAINT [DF_RM_Workers_HourlyCost]  DEFAULT (0),
	[Notes] [varchar](64) NULL CONSTRAINT [DF_RM_Workers_Notes]  DEFAULT (''),
	[DateOfBirth] [datetime] NULL CONSTRAINT [DF_RM_Workers_DateOfBirthOfBirth]  DEFAULT ('17991231'),
	[CityOfBirth] [varchar](32) NULL CONSTRAINT [DF_RM_Workers_CityOfBirth]  DEFAULT (''),
	[CivilStatus] [varchar](16) NULL CONSTRAINT [DF_RM_Workers_CivilStatus]  DEFAULT (''),
	[RegisterNumber] [varchar](16) NULL CONSTRAINT [DF_RM_Workers_RegisterNumber]  DEFAULT (''),
	[EmploymentDate] [datetime] NULL CONSTRAINT [DF_RM_Workers_EmploymentDate]  DEFAULT ('17991231'),
	[ResignationDate] [datetime] NULL CONSTRAINT [DF_RM_Workers_ResignationDate]  DEFAULT ('17991231'),
	[ImagePath] [varchar](128) COLLATE Latin1_General_CI_AS NULL CONSTRAINT [DF_RM_Workers_ImagePath]  DEFAULT (''),
	[HideOnLayout] [char] (1) NULL CONSTRAINT [DF_RM_Workers_HideOnLayout] DEFAULT('0') ,
	[Disabled] [char](1) NULL CONSTRAINT [DF_RM_Workers_Disabled]  DEFAULT ('0'),
	[Latitude] [varchar](16) NULL CONSTRAINT [DF_RM_Workers_Latitude]  DEFAULT (''),
	[Longitude] [varchar](16) NULL CONSTRAINT [DF_RM_Workers_Longitude]  DEFAULT (''),
    [PIN] [varchar] (8) NULL CONSTRAINT [DF_RM_Workers_PIN] DEFAULT (''),
	[Branch] [varchar](8) NULL CONSTRAINT [DF_RM_Workers_Branch]  DEFAULT (''),
    [Address2] [varchar] (64) NULL CONSTRAINT [DF_RM_Workers_Address2] DEFAULT (''),
    [StreetNo] [varchar] (10) NULL CONSTRAINT [DF_RM_Workers_StreetNo] DEFAULT (''),
    [District] [varchar] (64) NULL CONSTRAINT [DF_RM_Workers_District] DEFAULT (''),
    [FederalState] [varchar] (2) NULL CONSTRAINT [DF_RM_Workers_FederalState] DEFAULT (''),
	[IsRSEnabled] [char] (1) NULL CONSTRAINT [DF_RM_Workers_IsRSEnabled] DEFAULT('0')
   CONSTRAINT [PK_RM_Workers] PRIMARY KEY NONCLUSTERED 
    (
        [WorkerID]
    ) ON [PRIMARY]
) ON [PRIMARY]

CREATE INDEX [IX_RM_Workers_1] ON [dbo].[RM_Workers] ([CompanyLogin], [WorkerID]) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[RM_WorkersDetails]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[RM_WorkersDetails] (
	[WorkerId] [int] NOT NULL,
	[IsWorker] [char] (1)  NOT NULL,
	[ChildResourceType] [varchar] (8) NOT NULL ,
    [ChildResourceCode] [varchar] (8) NOT NULL ,   
    [ChildWorkerId] [int] NOT NULL ,
   CONSTRAINT [PK_RM_WorkersDetails] PRIMARY KEY NONCLUSTERED 
    (
        [WorkerId],
		[IsWorker],
        [ChildResourceType],
        [ChildResourceCode],
        [ChildWorkerId]
    ) ON [PRIMARY],
   CONSTRAINT [FK_RM_Workers_Details_00] FOREIGN KEY
    (
        [WorkerId]
    ) REFERENCES [dbo].[RM_Workers] (
        [WorkerId]
    )    
) ON [PRIMARY]

END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[RM_WorkersFields]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[RM_WorkersFields](
	[WorkerID] [int] NOT NULL,
	[Line] [smallint] NOT NULL,
	[FieldName] [varchar](64) NULL CONSTRAINT [DF_RM_WorkersFields_FieldName]  DEFAULT (''),
	[FieldValue] [varchar](256) NULL CONSTRAINT [DF_RM_WorkersFields_FieldValue]  DEFAULT (''),
	[Notes] [varchar](64) NULL CONSTRAINT [DF_RM_WorkersFields_Notes]  DEFAULT (''),
	[HideOnLayout] [char] (1) NULL CONSTRAINT [DF_RM_WorkersFields_HideOnLayout] DEFAULT('') 
   CONSTRAINT [PK_RM_WorkersFields] PRIMARY KEY NONCLUSTERED 
    (
        [WorkerID],
        [Line]
    ) ON [PRIMARY],
   CONSTRAINT [FK_RM_WorkersFields_00] FOREIGN KEY
    (
        [WorkerID]
    ) REFERENCES [dbo].[RM_Workers] (
        [WorkerID]
    )    
) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[RM_WorkersArrangements]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[RM_WorkersArrangements](
	[WorkerID] [int] NOT NULL,
	[Line] [smallint] NOT NULL, 
	[Arrangement] [varchar](8) NULL CONSTRAINT [DF_RM_WorkersArrangements_Arrangement]  DEFAULT (''),
	[ArrangementLevel] [varchar](32) NULL CONSTRAINT [DF_RM_WorkersArrangements_ArrangementLevel]  DEFAULT (''),
	[BasicPay] [float] NULL CONSTRAINT [DF_RM_WorkersArrangements_BasicPay]  DEFAULT (0),
	[TotalPay] [float] NULL  CONSTRAINT [DF_RM_WorkersArrangements_TotalPay]  DEFAULT (0),
	[FromDate] [datetime] NULL CONSTRAINT [DF_RM_WorkersArrangements_FromDate]  DEFAULT ('17991231'),
	[ToDate] [datetime] NULL CONSTRAINT [DF_RM_WorkersArrangements_ToDate]  DEFAULT ('17991231'),
	[Notes] [varchar](64) NULL CONSTRAINT [DF_RM_WorkersArrangements_Notes]  DEFAULT ('')
   CONSTRAINT [PK_RM_WorkersArrangements] PRIMARY KEY NONCLUSTERED 
    (
        [WorkerID],
        [Line]
    ) ON [PRIMARY],
   CONSTRAINT [FK_RM_WorkersArrangements_00] FOREIGN KEY
    (
        [WorkerID]
    ) REFERENCES [dbo].[RM_Workers] (
        [WorkerID]
    )    
) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[RM_WorkersAbsences]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[RM_WorkersAbsences](
	[WorkerID] [int] NOT NULL,
	[Reason] [varchar](10) NOT NULL,
	[StartingDate] [datetime] NOT NULL,
	[EndingDate] [datetime] NULL  CONSTRAINT [DF_RM_WorkersAbsences_EndingDate]  DEFAULT ('17991231'),
	[Manager] [int] NULL  CONSTRAINT [DF_RM_WorkersAbsences_Manager]  DEFAULT (0),
	[Notes] [varchar](64) NULL  CONSTRAINT [DF_RM_WorkersAbsences_Notes]  DEFAULT (''),
 CONSTRAINT [PK_RM_WorkersAbsences] PRIMARY KEY CLUSTERED 
(
	[WorkerID] ,
	[Reason] ,
	[StartingDate] 
) ON [PRIMARY],
   CONSTRAINT [FK_RM_WorkersAbsences_00] FOREIGN KEY
    (
        [WorkerID]
    ) REFERENCES [dbo].[RM_Workers] (
        [WorkerID]
    )    
) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[RM_Arrangements]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[RM_Arrangements](
	[Arrangement] [varchar](8) NOT NULL,
	[Description] [varchar](32) NULL  CONSTRAINT [DF_RM_Arrangements_Description]  DEFAULT (''),
	[ArrangementLevel] [varchar](32) NULL CONSTRAINT [DF_RM_Arrangements_ArrangementLevel]  DEFAULT (''),
	[BasicPay] [float] NULL CONSTRAINT [DF_RM_Arrangements_BasicPay] DEFAULT ((0)),
	[TotalPay] [float] NULL CONSTRAINT [DF_RM_Arrangements_TotalPay] DEFAULT ((0)),
	[WorkingHours] [int] NULL CONSTRAINT [DF_RM_Arrangements_WorkingHours]  DEFAULT ((0)),
	[Notes] [varchar](64) NULL  CONSTRAINT [DF_RM_Arrangements_Notes]  DEFAULT (''),
 CONSTRAINT [PK_RM_Arrangements] PRIMARY KEY CLUSTERED 
(
	[Arrangement] 
) ON [PRIMARY]
) ON [PRIMARY]
END
GO

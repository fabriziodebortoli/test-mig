if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MA_EI_ITFatelWebParameters]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MA_EI_ITFatelWebParameters] (
    [ParameterId]           [int]       NOT NULL,
    [Url]					[varchar]  (256) NULL CONSTRAINT DF_EIITFatelw_Url_00 DEFAULT(''),
    [UserName]              [varchar]  (128) NULL CONSTRAINT DF_EIITFatelw_UserName_00   DEFAULT(''),
    [Password]              [varchar]  (32) NULL CONSTRAINT DF_EIITFatelw_Password_00   DEFAULT(''),
    [CompanyCode]           [varchar]   (8) NULL CONSTRAINT DF_EIITFatelw_CompanyCod_00 DEFAULT(''),
	[UseFatelWeb]			[char]		(1) NULL CONSTRAINT DF_EIITFatelw_UseFatelWe_00  DEFAULT ('0'),
	[XMLVersion]			[varchar]	(4) NULL CONSTRAINT DF_EIITFatelw_XMLVersion_00 DEFAULT (''),
	[ActivationDate]		[datetime]	NULL CONSTRAINT DF_EIITFatelw_Activation_00 DEFAULT ('17991231'),
	[ExpiringDate]			[datetime]	NULL CONSTRAINT DF_EIITFatelw_ExpiringD_00 DEFAULT ('17991231'),
	[ProgrCompanyCode]      [varchar]   (8) NULL CONSTRAINT DF_EIITFatelw_ProgrCompa_00 DEFAULT(''),
    [MaxNumFile]           	[int]       NULL CONSTRAINT DF_EIITFatelw_MaxNumFile_00 DEFAULT(0),
	[ManageSignature]       [char]      (1) NULL CONSTRAINT DF_EIITFatelw_ManageSign_00 DEFAULT('0'),
	[PortalUrl]				[varchar]  (256) NULL CONSTRAINT DF_EIITFatelw_PortalUrl_00 DEFAULT(''),
	[SOSManagement]		    [char]      (1) NULL CONSTRAINT DF_EIITFatelw_SOSMan_00 DEFAULT('1'),
   CONSTRAINT [PK_EI_ITFatelWebParameters] PRIMARY KEY NONCLUSTERED 
    (
        [ParameterId]
    ) ON [PRIMARY]
) ON [PRIMARY]

END
GO


if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MA_FEPAParameters]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MA_FEPAParameters] (
    [ParameterId]            [int]       NOT NULL,
    [ServerName]             [varchar]  (32) NULL CONSTRAINT DF_FEPAParame_ServerName_00	DEFAULT(''),
    [DBName]                 [varchar]  (32) NULL CONSTRAINT DF_FEPAParame_DBName_00		DEFAULT(''),
    [UserName]               [varchar]  (32) NULL CONSTRAINT DF_FEPAParame_UserName_00		DEFAULT(''),
    [Password]               [varchar]  (32) NULL CONSTRAINT DF_FEPAParame_Password_00		DEFAULT(''),
    [CompanyCode]            [varchar]   (8) NULL CONSTRAINT DF_FEPAParame_CompanyCod_00	DEFAULT(''),
	[DocStatusCheckError]	 [int]			 NULL CONSTRAINT DF_FEPAParame_DocStatusC_00	DEFAULT(10682368),
	[UseInternalTranscoding] [char]		 (1) NULL CONSTRAINT DF_FEPAParame_UseInterna_00	DEFAULT('0'),
	[ProcessCodeB2B]         [varchar]   (2) NULL CONSTRAINT DF_FEPAParame_ProcCodeB2B_00	DEFAULT(''),
   CONSTRAINT [PK_FEPAParameters] PRIMARY KEY NONCLUSTERED 
    (
        [ParameterId]
    ) ON [PRIMARY]
) ON [PRIMARY]

END
GO


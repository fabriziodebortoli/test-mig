if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_IMagoMenu]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[MSD_IMagoMenu](
	[MMMENUID] [char](32) NOT NULL,
	[MMLSTUPD] [char](32) NULL,
	[MM__VOCE] [char](254) NULL,
	[MM_LEVEL] [int] NULL DEFAULT ((0)),
	[MMPROCED] [text] NULL,
	[MMINFPRO] [text] NULL,
	[MMDIRECT] [int] NULL DEFAULT ((0)),
	[MMRIFPAD] [int] NULL DEFAULT ((0)),
	[MMELEMEN] [int] NULL DEFAULT ((0)),
	[MM_ROWID] [char](10) NULL DEFAULT ('          '),
	[MMENABLE] [bit] NULL,
	[MM_PROGR] [int] NULL DEFAULT ((0)),
	[MM_IMAGE] [char](254) NULL,
	[MMMODULO] [text] NULL,
	[MMINFMOD] [bit] NULL,
	[MMINSPNT] [char](254) NULL,
	[MM_POSIT] [char](6) NULL,
	[MMDELETE] [char](1) NULL,
	[MMLVLKEY] [char](100) NOT NULL,
	[MMINSERT] [char](254) NULL,
	[MM__NOTE] [char](254) NULL,
	[MM___MRU] [char](1) NULL,
	[MMTEAROF] [char](1) NULL,
	[cpccchk] [char](10) NULL,
 CONSTRAINT [PK_MSD_IMagoMenu] PRIMARY KEY CLUSTERED 
(
	[MMMENUID] ASC,
	[MMLVLKEY] ASC
)
)
END
GO




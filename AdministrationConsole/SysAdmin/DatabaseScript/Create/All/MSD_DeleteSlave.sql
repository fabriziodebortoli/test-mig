SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_DeleteSlave]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[MSD_DeleteSlave]
GO

CREATE PROCEDURE MSD_DeleteSlave
	(
		@par_slaveid int,
		@par_companyid int
	) AS
	BEGIN TRANSACTION
	DELETE MSD_SlaveLogins WHERE SlaveId = @par_slaveid
	DELETE MSD_CompanyDBSlaves WHERE CompanyId = @par_companyid AND SlaveId = @par_slaveid
	COMMIT TRANSACTION
RETURN(0)

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


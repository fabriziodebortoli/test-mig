SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_DeleteCompanyLogin]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[MSD_DeleteCompanyLogin]
GO

CREATE PROCEDURE MSD_DeleteCompanyLogin
	(
		@par_companyid int,
		@par_loginid int
	) AS
	BEGIN TRANSACTION
	IF exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ObjectGrants]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
		DELETE MSD_ObjectGrants WHERE CompanyId = @par_companyid AND LoginId = @par_loginid
	IF exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ScheduledTasks]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
	BEGIN
		IF exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ScheduledSequences]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
			DELETE MSD_ScheduledSequences WHERE TaskId IN (SELECT Id FROM MSD_ScheduledTasks WHERE CompanyId = @par_companyid AND LoginId = @par_loginid)
		DELETE MSD_ScheduledTasks WHERE CompanyId = @par_companyid AND LoginId = @par_loginid
	END
	IF exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_EasyLookCustomSettings]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
		DELETE MSD_EasyLookCustomSettings WHERE CompanyId = @par_companyid AND LoginId = @par_loginid
	DELETE MSD_CompanyRolesLogins WHERE CompanyId = @par_companyid AND LoginId = @par_loginid
	DELETE MSD_SlaveLogins WHERE SlaveId IN (SELECT SlaveId FROM MSD_CompanyDBSlaves WHERE CompanyId = @par_companyid) AND LoginId = @par_loginid
	DELETE MSD_CompanyLogins WHERE CompanyId = @par_companyid AND LoginId = @par_loginid
	COMMIT TRANSACTION
RETURN(0)

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


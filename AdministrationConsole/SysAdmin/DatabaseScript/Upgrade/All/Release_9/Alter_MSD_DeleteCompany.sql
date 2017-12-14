SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

ALTER PROCEDURE MSD_DeleteCompany
	(
			@par_companyid int
	) AS
	BEGIN TRANSACTION
	  	IF exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ProtectedObjects]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
		BEGIN
			DELETE MSD_ObjectGrants WHERE CompanyId = @par_companyid
			DELETE MSD_ProtectedObjects WHERE CompanyId = @par_companyid
		END
	  	IF exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ScheduledTasks]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
		BEGIN
		  	IF exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ScheduledSequences]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
				DELETE MSD_ScheduledSequences WHERE TaskId IN (SELECT Id FROM MSD_ScheduledTasks WHERE CompanyId = @par_companyid)
			DELETE MSD_ScheduledTasks WHERE CompanyId = @par_companyid
		END
		IF exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_EasyLookCustomSettings]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
			DELETE MSD_EasyLookCustomSettings WHERE CompanyId = @par_companyid
		DELETE MSD_CompanyRolesLogins WHERE CompanyId = @par_companyid
		DELETE MSD_CompanyRoles WHERE CompanyId = @par_companyid
		DELETE MSD_CompanyLogins WHERE CompanyId = @par_companyid
		DELETE MSD_Companies WHERE CompanyId = @par_companyid
	COMMIT TRANSACTION
RETURN (0)

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_DeleteCompanyRoleLogin]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[MSD_DeleteCompanyRoleLogin]
GO

CREATE PROCEDURE MSD_DeleteCompanyRoleLogin
	( 
		@par_companyid int,
		@par_loginid int,
		@par_roleid int
	) AS
BEGIN TRANSACTION

    IF exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ObjectGrants]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
		DELETE MSD_ObjectGrants 
			WHERE LoginId = @par_loginid AND  CompanyId = @par_companyid AND RoleId = @par_roleid
    DELETE MSD_CompanyRolesLogins 
		WHERE LoginId = @par_loginid AND CompanyId = @par_companyid AND  RoleId = @par_roleid
COMMIT TRANSACTION
RETURN (0)

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


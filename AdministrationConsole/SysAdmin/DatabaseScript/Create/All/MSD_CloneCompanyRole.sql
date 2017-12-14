SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_CloneCompanyRole]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[MSD_CloneCompanyRole]
GO

CREATE PROCEDURE MSD_CloneCompanyRole
		(
			@par_srccompanyid int, @par_srcroleid int,
            @par_dstcompanyid int, @par_dstrolename varchar(50)
         ) 
AS
DECLARE @retcode INTEGER
DECLARE @newroleid INTEGER

BEGIN TRANSACTION

	INSERT INTO MSD_CompanyRoles( CompanyId, Role, Description)
        SELECT
                @par_dstcompanyid,
                @par_dstrolename,
                Description
        FROM MSD_CompanyRoles
        WHERE CompanyId = @par_srccompanyid AND RoleId = @par_srcroleid
        
    IF @@error <> 0 OR @@IDENTITY IS NULL
        BEGIN
            ROLLBACK TRANSACTION
            RETURN (1)
        END
    SET @newroleid = @@IDENTITY
 
    IF @par_srccompanyid = @par_dstcompanyid
		BEGIN
			IF EXISTS 
			(
				SELECT CompanyId FROM MSD_CompanyRolesLogins
				WHERE CompanyId = @par_srccompanyid AND RoleId = @par_srcroleid
             )
				BEGIN
					INSERT INTO MSD_CompanyRolesLogins(CompanyId, RoleId, LoginId)
						SELECT
								CompanyId,
								@newroleid,
								LoginId
						FROM MSD_CompanyRolesLogins
						WHERE
								CompanyId = @par_srccompanyid AND
								RoleId = @par_srcroleid
								
					IF @@error <> 0
						BEGIN
							ROLLBACK TRANSACTION
							RETURN (3)
						END
                END
           END
           
  	IF exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ProtectedObjects]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
		BEGIN
			EXEC @retcode = MSD_CloneCompanyRoleGrants @par_srccompanyid, @par_srcroleid, @par_dstcompanyid, @newroleid
			IF @retcode <> 0
				BEGIN
					ROLLBACK TRANSACTION
					RETURN (2)
				END
     END 
     
COMMIT TRANSACTION
RETURN (0)

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


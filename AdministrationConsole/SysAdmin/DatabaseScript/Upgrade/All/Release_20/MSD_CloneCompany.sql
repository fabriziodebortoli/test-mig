SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_CloneCompany]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[MSD_CloneCompany]
GO

CREATE PROCEDURE MSD_CloneCompany
	(
		@par_srccompanyid int,
        @par_newcompanyname varchar(50),
        @par_newcompanydbserver varchar(50),
        @par_newcompanydbname varchar(50),
        @par_newcompanydbowner int
     ) AS

DECLARE @newcompanyid INTEGER

BEGIN TRANSACTION
	INSERT INTO MSD_Companies 
				(
					Company,
					Description,
					ProviderId,
					CompanyDBServer,
					CompanyDBName,
					CompanyDBOwner,
                    DBDefaultUser,
                    DBDefaultPassword,
                    CompanyDBWindowsAuthentication,
                    PreferredLanguage,
                    ApplicationLanguage,
                    UseTransaction,
                    UseKeyedUpdate,
                    UseSecurity,
                    UseAuditing,
                    Disabled,
                    DatabaseCulture,
                    SupportColumnCollation,
                    IsSecurityLightMigrated
                  )
        SELECT
               @par_newcompanyname,
               Description,
               ProviderId,
               @par_newcompanydbserver,
               @par_newcompanydbname,
               @par_newcompanydbowner,
               DBDefaultUser,
               DBDefaultPassword,
               CompanyDBWindowsAuthentication,
               PreferredLanguage,
               ApplicationLanguage,
               UseTransaction,
               UseKeyedUpdate,
               UseSecurity,
               UseAuditing,
               Disabled,
			   DatabaseCulture,
               SupportColumnCollation,
               IsSecurityLightMigrated
        FROM MSD_Companies
        WHERE CompanyId = @par_srccompanyid
        IF @@error <> 0 OR @@IDENTITY IS NULL
        BEGIN
               ROLLBACK TRANSACTION
               RETURN(1)
        END
	SET @newcompanyid = @@IDENTITY
        IF EXISTS (SELECT RoleId,Description FROM MSD_CompanyRoles
                   WHERE CompanyId = @par_srccompanyid
        )
	BEGIN
	         SET IDENTITY_INSERT MSD_CompanyRoles ON
             INSERT INTO MSD_CompanyRoles ( CompanyId, RoleId, Role,Description, LastModifyGrants)
             SELECT
                    @newcompanyid,
                    RoleId,
                    Role,
                    Description,
                    LastModifyGrants
             FROM MSD_CompanyRoles
             WHERE CompanyId = @par_srccompanyid
             IF @@error <> 0
             BEGIN
                 ROLLBACK TRANSACTION
                 RETURN(2)
             END
             SET IDENTITY_INSERT MSD_CompanyRoles OFF
        END

			/* inserisco il dbo */
		
        IF EXISTS (SELECT LoginId FROM MSD_CompanyLogins
                   WHERE CompanyId = @par_srccompanyid
        )
        BEGIN
             INSERT INTO MSD_CompanyLogins 
							(
								CompanyId,
								LoginId,
								DBUser,
								DBPassword,
								DBWindowsAuthentication,
								Admin,
								LastModifyGrants,
								Disabled
							)
             SELECT
                    @newcompanyid,
                    LoginId,
                    DBUser,
                    DBPassword,
                    DBWindowsAuthentication,
                    Admin,
                    LastModifyGrants,
                    Disabled
            FROM MSD_CompanyLogins
            WHERE CompanyId = @par_srccompanyid
             IF @@error <> 0
            BEGIN
                ROLLBACK TRANSACTION
                RETURN (3)
            END
        END
	
	
	IF EXISTS( SELECT LoginId FROM MSD_CompanyRolesLogins
                   WHERE CompanyId = @par_srccompanyid
        )
        BEGIN
            INSERT INTO MSD_CompanyRolesLogins (CompanyId,RoleId,LoginId)
            SELECT
                @newcompanyid,
                RoleId,
                LoginId
            FROM MSD_CompanyRolesLogins
            WHERE CompanyId = @par_srccompanyid
            IF @@error <> 0
            BEGIN
                ROLLBACK TRANSACTION
                RETURN (4)
            END
        END

		IF NOT EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ProtectedObjects]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
		BEGIN
			COMMIT TRANSACTION
			RETURN (0)
		END
		
        IF EXISTS ( SELECT ObjectId FROM MSD_ProtectedObjects
                    WHERE CompanyId = @par_srccompanyid
        )
        BEGIN
             INSERT INTO MSD_ProtectedObjects(CompanyId,ObjectId,Disabled)
             SELECT
                 @newcompanyid,
                 ObjectId,
                 Disabled
             FROM MSD_ProtectedObjects
             WHERE CompanyId = @par_srccompanyid
            IF @@error <> 0
            BEGIN
                ROLLBACK TRANSACTION
                RETURN (5)
            END
            
        END

        
        IF EXISTS ( SELECT ObjectId FROM MSD_ObjectGrants
                    WHERE CompanyId = @par_srccompanyid
        )
        BEGIN
             INSERT INTO MSD_ObjectGrants(CompanyId,ObjectId,LoginId,RoleId,Grants,InheritMask)
             SELECT
                 @newcompanyid,
                 ObjectId,
                 LoginId,
                 RoleId,
                 Grants,
                 InheritMask
             FROM MSD_ObjectGrants
             WHERE CompanyId = @par_srccompanyid
            IF @@error <> 0
            BEGIN
                ROLLBACK TRANSACTION
                RETURN (6)
            END
            
        END
COMMIT TRANSACTION
RETURN (0)

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


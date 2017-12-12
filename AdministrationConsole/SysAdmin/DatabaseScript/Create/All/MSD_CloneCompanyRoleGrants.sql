SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_CloneCompanyRoleGrants]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[MSD_CloneCompanyRoleGrants]
GO

CREATE PROCEDURE MSD_CloneCompanyRoleGrants
	(
		@par_srccompanyid int, @par_srcroleid int,
                @par_dstcompanyid int, @par_dstroleid int
        ) AS
IF @par_srccompanyid <> @par_dstcompanyid
BEGIN
	IF EXISTS ( 
			SELECT ObjectId
                        FROM MSD_ProtectedObjects AS T1
                        WHERE CompanyId = @par_srccompanyid AND
                        NOT EXISTS
                        (
				SELECT ObjectId
                                FROM MSD_ProtectedObjects AS T2
                                WHERE
                                   T2.CompanyId = @par_dstcompanyid AND
                                   T1.ObjectId = T2.ObjectId
                         )
         )
	 BEGIN
		INSERT INTO MSD_ProtectedObjects(CompanyId, ObjectId, Disabled)
                SELECT
                       @par_dstcompanyid,
                       ObjectId,
                       Disabled
                FROM MSD_ProtectedObjects AS T1
                WHERE
		      CompanyId = @par_srccompanyid AND
                      NOT EXISTS
                      (
			 SELECT ObjectId
                         FROM MSD_ProtectedObjects AS T2
                         WHERE 
                               T2.CompanyId = @par_dstcompanyid AND
                               T1.ObjectId = T2.ObjectId
                       )
		IF @@error <> 0
                BEGIN
                      RETURN (10)
                END
	END
END

IF EXISTS (
	SELECT ObjectId
                       FROM MSD_ObjectGrants AS T1
                        WHERE 
                              CompanyID = @par_srccompanyid AND
                              RoleId = @par_srcroleid 
                    )
	BEGIN
              INSERT INTO MSD_ObjectGrants
                          (CompanyId,
                           ObjectId,
                           LoginId,
                           RoleId,
                           Grants,
                           InheritMask)
              SELECT
                           @par_dstcompanyid,
                           ObjectId,
                           LoginId,
                           @par_dstroleid,
                           Grants,
                           InheritMask
              FROM MSD_ObjectGrants AS T1
              WHERE
                    CompanyId = @par_srccompanyid AND
                    RoleId = @par_srcroleid 
              IF @@error <> 0
              BEGIN
					RETURN (12)
			 END
END

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


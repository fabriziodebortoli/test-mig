SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_CloneCompanyLogin]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[MSD_CloneCompanyLogin]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_CloneCompanyLoginGrants]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[MSD_CloneCompanyLoginGrants]
GO

CREATE PROCEDURE MSD_CloneCompanyLoginGrants
	(
		@par_srccompanyid int, @par_srcloginid int,
        @par_dstcompanyid int, @par_dstloginid int
        ) AS
BEGIN
IF exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ProtectedObjects]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
	BEGIN
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
								LoginId = @par_srcloginid 
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
							@par_dstloginid,
							RoleId,
							Grants,
							InheritMask
				FROM MSD_ObjectGrants AS T1
				WHERE
						CompanyId = @par_srccompanyid AND
						LoginId = @par_srcloginid
				IF @@error <> 0
				BEGIN
				RETURN (12)
				END
			END
	END
END
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
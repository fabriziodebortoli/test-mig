
											if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ScheduledTasks]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'Framework.TbWoormEngine.TbWoormEngine.TbWoormAskParameters'
WHERE( Command = 'Framework.TbWoormEngine.TbWoormEngine.TbWoormRichiestaParametri' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
GO
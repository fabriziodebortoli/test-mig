BEGIN

UPDATE  dbo.MSD_ObjectTypes SET TypeName ='Function' WHERE Type = 3 
UPDATE  dbo.MSD_ObjectTypes SET TypeName ='Report' WHERE Type = 4 
UPDATE  dbo.MSD_ObjectTypes SET TypeName ='Data Entry' WHERE Type = 5 
UPDATE  dbo.MSD_ObjectTypes SET TypeName ='Child Window' WHERE Type = 6 
UPDATE  dbo.MSD_ObjectTypes SET TypeName ='Batch' WHERE Type = 7 
UPDATE  dbo.MSD_ObjectTypes SET TypeName ='Tab' WHERE Type = 8 
UPDATE  dbo.MSD_ObjectTypes SET TypeName ='Constraint' WHERE Type = 9 
UPDATE  dbo.MSD_ObjectTypes SET TypeName ='Table' WHERE Type = 10 
UPDATE  dbo.MSD_ObjectTypes SET TypeName ='HotLink' WHERE Type = 11 
UPDATE  dbo.MSD_ObjectTypes SET TypeName ='View' WHERE Type = 13 
UPDATE  dbo.MSD_ObjectTypes SET TypeName ='Row View' WHERE Type = 14 
UPDATE  dbo.MSD_ObjectTypes SET TypeName ='Grid' WHERE Type = 15 
UPDATE  dbo.MSD_ObjectTypes SET TypeName ='Grid Column' WHERE Type = 16 
UPDATE  dbo.MSD_ObjectTypes SET TypeName ='Control' WHERE Type = 17 
UPDATE  dbo.MSD_ObjectTypes SET TypeName ='Radar' WHERE Type = 21 
UPDATE  dbo.MSD_ObjectTypes SET TypeName ='Word Document' WHERE Type = 23 
UPDATE  dbo.MSD_ObjectTypes SET TypeName ='Excel Document' WHERE Type = 24 
UPDATE  dbo.MSD_ObjectTypes SET TypeName ='Word Template' WHERE Type = 25 
UPDATE  dbo.MSD_ObjectTypes SET TypeName ='Excel Template' WHERE Type = 26 

END 

GO
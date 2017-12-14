﻿using System.Collections;
using System.Xml;

namespace Microarea.TaskBuilderNet.Interfaces
{
	//===================================================================================
	public interface IDatabaseObjectsInfo
	{
		string FilePath { get; }
		string Signature { get; }
		int Release { get; }
		bool Valid { get; set; }
		string ParsingError { get; set; }
		bool Dms { get; }
		bool IsDevelopmentVersion { get; }
		string PreviousApplication { get; }
		string PreviousModule { get; }

		IBaseModuleInfo ParentModuleInfo { get; }

		ITableInfo GetTableInfoByName(string tableName);
		IDbObjectInfo GetViewInfoByName(string viewName);
		IDbObjectInfo GetProcedureInfoByName(string procedureName);
		
		IList TableInfoArray { get; }
		IList ViewInfoArray { get; }
		IList ProcedureInfoArray { get; }

		bool Parse();
		bool ParseSingleProcedure(XmlNodeList procedureNodes);
		bool ParseSingleTable(XmlNodeList tableNodes);
		bool ParseSingleView(XmlNodeList viewNodes);
	}
}

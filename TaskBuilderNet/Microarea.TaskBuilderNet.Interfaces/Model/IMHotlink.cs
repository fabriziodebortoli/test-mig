using System;
using System.Collections.Generic;


namespace Microarea.TaskBuilderNet.Interfaces.Model
{
	//=========================================================================
	public interface IMHotLink
	{
		String DBFieldName { get; }
		String TableName { get; }
		IList<IMHotLinkParam> Parameters { get; }
		INameSpace Namespace { get; }
		String SerializedType { get; }
		String ReturnType { get; }
		bool ExistData(IDataObj dataObj);
	}

	//=========================================================================
	public interface IMHotLinkParam
	{
		String Name { get; }
		String Description { get; }
	}
}

using System.Collections.Generic;

namespace Microarea.AdminServer.Services
{
	//================================================================================
	public class QueryInfo
	{
		private List<QueryField> fields;

		public List<QueryField> Fields { get => fields; set => fields = value; }

		//-----------------------------------------------------------------------------	
		public QueryInfo()
		{
			Fields = new List<QueryField>();
		}
	}

	//================================================================================
	public class QueryField
	{
		public string Name;
		public object Value;

		//-----------------------------------------------------------------------------	
		public QueryField(string name, object value)
		{
			Name = name;
			Value = value;
		}
	}
}
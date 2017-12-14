using System.Collections;
using System.Xml.Serialization;

namespace Microarea.TaskBuilderNet.Data.DatabaseWinControls
{
	//========================================================================
	// SQLServersCombo class which will be serialized
	//========================================================================
	[XmlRoot("SQLServersCombo")]
	public class SQLServersCombo
	{
		private ArrayList serversList;

		public int ItemsCount { get { return serversList.Count ;} }

		//---------------------------------------------------------------------
		public SQLServersCombo()
		{
			serversList = new ArrayList();
		}

		//---------------------------------------------------------------------
		[XmlElement("SQLServer")]
		public SQLServer[] Servers
		{
			get
			{
				SQLServer[] items = new SQLServer[serversList.Count];
				serversList.CopyTo(items);
				return items;
			}
			set
			{
				if (value == null)
					return;
				SQLServer[] items = (SQLServer[])value;
				serversList.Clear();
				foreach (SQLServer item in items)
					serversList.Add(item);
			}
		}

		//---------------------------------------------------------------------
		public int AddServer(SQLServer item)
		{
			return serversList.Add(item);
		}
	}

	//========================================================================
	public class SQLServer
	{
		//---------------------------------------------------------------------
		[XmlAttribute("name")]
		public string Name;

		//---------------------------------------------------------------------
		public SQLServer()
		{
		}

		//---------------------------------------------------------------------
		public SQLServer(string name)
		{
			this.Name = name;
		}
	}
}
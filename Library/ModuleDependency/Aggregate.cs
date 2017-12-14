using System;
using System.Diagnostics;
using System.Xml;

namespace Microarea.Library.ModuleDependency
{
	public class Aggregate
	{
		private readonly string dllName;
		private readonly string application;
		private readonly string module;
		private readonly string[] modules;

		public Aggregate(XmlElement aggregateElement)
		{
			if (aggregateElement == null)
				throw new ArgumentNullException();

			this.dllName = aggregateElement.GetAttribute("name");
			this.application = aggregateElement.GetAttribute("application");
			this.module = aggregateElement.GetAttribute("module");
			this.modules = aggregateElement.GetAttribute("modules").Split(',');
			Debug.Assert(this.dllName.Length != 0);
			Debug.Assert(this.application.Length != 0);
			Debug.Assert(this.modules.Length != 0);
		}

		public string DllName { get { return this.dllName; } }
		public string Application { get { return this.application; } }
		public string Module { get { return this.module; } }
		public string[] Modules { get { return this.modules; } }
	}
}

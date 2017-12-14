using System.Configuration;
using Microarea.Library.LogManager.Loggers;

namespace Microarea.Library.LogManager.Config
{
	//=========================================================================
	class TypeConfigElement : ConfigurationElement
	{
		//---------------------------------------------------------------------
		[ConfigurationProperty(BaseLogger.AssemblyAttribute, IsRequired = true)]
		public string Assembly
		{
			get { return this[BaseLogger.AssemblyAttribute] as string; }
		}

		//---------------------------------------------------------------------
		[ConfigurationProperty(BaseLogger.NameAttribute, IsRequired = true)]
		public string TypeName
		{
			get { return this[BaseLogger.NameAttribute] as string; }
		}
	}
}

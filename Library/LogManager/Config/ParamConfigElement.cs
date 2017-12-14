using System.Configuration;
using Microarea.Library.LogManager.Loggers;

namespace Microarea.Library.LogManager.Config
{
	//=========================================================================
	class ParamConfigElement : ConfigurationElement
	{
		//---------------------------------------------------------------------
		[ConfigurationProperty(BaseLogger.NameAttribute, IsRequired = true)]
		public string Name
		{
			get { return this[BaseLogger.NameAttribute] as string; }
		}

		//---------------------------------------------------------------------
		[ConfigurationProperty(BaseLogger.TypeAttribute, IsRequired = true)]
		public string Type
		{
			get { return this[BaseLogger.TypeAttribute] as string; }
		}

		//---------------------------------------------------------------------
		[ConfigurationProperty(BaseLogger.ValueAttribute, IsRequired = true)]
		public string Value
		{
			get { return this[BaseLogger.ValueAttribute] as string; }
		}
	}
}

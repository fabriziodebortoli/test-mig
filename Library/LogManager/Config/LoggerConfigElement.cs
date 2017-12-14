using System.Configuration;
using Microarea.Library.LogManager.Loggers;

namespace Microarea.Library.LogManager.Config
{
	//=========================================================================
	class LoggerConfigElement : ConfigurationElement
	{
		//---------------------------------------------------------------------
		[ConfigurationProperty(BaseLogger.NameAttribute, IsRequired = true)]
		public string Name
		{
			get
			{
				return this[BaseLogger.NameAttribute] as string;
			}
		}

		//---------------------------------------------------------------------
		[ConfigurationProperty(BaseLogger.TypeAttribute, IsRequired = true)]
		public TypeConfigElement Type
		{
			get
			{
				return this[BaseLogger.TypeAttribute] as TypeConfigElement;
			}
		}

		//---------------------------------------------------------------------
		[ConfigurationProperty(BaseLogger.ParamsTag, IsRequired = false)]
		public ParamConfigElementCollection Params
		{
			get
			{
				return this[BaseLogger.ParamsTag] as ParamConfigElementCollection;
			}
		}
	}
}

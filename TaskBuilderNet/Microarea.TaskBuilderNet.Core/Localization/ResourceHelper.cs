using System;
using System.Collections;
using System.Resources;

namespace Microarea.TaskBuilderNet.Core.Localization
{
	//=========================================================================
	class ResourceHelper
	{
		static Hashtable resManCache = CreateCache();

		//---------------------------------------------------------------------
		private static Hashtable CreateCache()
		{
			return Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCulture));
		}

		//---------------------------------------------------------------------
		public ResourceHelper()
		{}

		//---------------------------------------------------------------------
		public ResourceManager GetResourceManager(Type type)
		{
			ResourceManager resMan = resManCache[type] as ResourceManager;
			if (resMan == null)
			{
				resMan = new ResourceManager(type.FullName, type.Assembly);
				resManCache.Add(type, resMan);
			}

			return resMan;
		}
	}
}

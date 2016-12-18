using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Microarea.TbLoaderGate
{
	public class TBLoaderEngine
	{
		static ArrayList tbloaders = (ArrayList)ArrayList.Synchronized(new ArrayList());
		private static TBLoaderInstance GetTbLoader(string name)
		{
			foreach (TBLoaderInstance item in tbloaders.ToArray())
			{
				if (item.name == name)
					return item;
			}
			return null;
		}
		internal static TBLoaderInstance GetTbLoader(string name, bool create, out bool newInstance)
		{
			newInstance = false;
			var tbLoader = GetTbLoader(name);
			if (tbLoader == null && create)
			{
				tbLoader = new TBLoaderInstance();
				tbLoader.ExecuteAsync().Wait();
				tbloaders.Add(tbLoader);
				newInstance = true;
			}
			return tbLoader;
		}

		internal static void RemoveTbLoader(string name)
		{
			var tbLoader = GetTbLoader(name);
			tbloaders.Remove(tbLoader);
		}
	}
}
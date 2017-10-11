using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace Microarea.TbLoaderGate
{
	public class TBLoaderEngine
	{
		static ArrayList tbloaders = (ArrayList)ArrayList.Synchronized(new ArrayList());

		//-----------------------------------------------------------------------------------------
		private static TBLoaderInstance GetTbLoader(string name)
		{
			object[] items = tbloaders.ToArray();
			for (int i = items.Length - 1; i >= 0; i--)
			{
				TBLoaderInstance item = items[i] as TBLoaderInstance;
				if (item == null)
					continue;

				if (!IsProcessRunning(item.processId))
				{
					tbloaders.Remove(item);
					continue;
				}

				if (item.name == name)
					return item;
			}
			return null;
		}

		//-----------------------------------------------------------------------------------------
		private static bool IsProcessRunning(int processId)
		{
			try
			{
				Process process = Process.GetProcessById(processId);
				if (process == null)
					return false;
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}

		//-----------------------------------------------------------------------------------------
		internal static TBLoaderInstance GetTbLoader(string server, int port, string name, bool create, out bool newInstance)
		{
			newInstance = false;
			var tbLoader = GetTbLoader(name);
			if (tbLoader == null && create)
			{
				tbLoader = new TBLoaderInstance(server, port);
				tbLoader.ExecuteAsync().Wait();
				tbloaders.Add(tbLoader);
				newInstance = true;
			}
			return tbLoader;
		}

		//-----------------------------------------------------------------------------------------
		internal static void RemoveTbLoader(string name)
		{
			var tbLoader = GetTbLoader(name);
			tbloaders.Remove(tbLoader);
		}
	}
}
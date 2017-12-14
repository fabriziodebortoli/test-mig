using System;
using System.Collections;
using System.Text;


namespace Microarea.Tools.TBLocalizer.CommonUtilities
{
	//================================================================================
	public class SolutionCache
	{
		private Hashtable cache = new Hashtable();

		//--------------------------------------------------------------------------------
		public SolutionCache()
		{
			Functions.CurrentSolutionCache = this;	
		}

		//--------------------------------------------------------------------------------
		public void Clear()
		{
			cache.Clear();
		}

		//--------------------------------------------------------------------------------
		public void Init(params object[] initialObjects)
		{
			Clear();
			for (int i = 0; i < initialObjects.Length; i = i + 2)
			{
				this[initialObjects[i]].Object = initialObjects[i + 1];
			}
		}

		//--------------------------------------------------------------------------------
		private string GetComposedKey(params object[] keys)
		{
			if (keys.Length == 0) 
				throw new ArgumentException(CommonStrings.InvalidArgument, "keys");

			StringBuilder composedKey = new StringBuilder();
			foreach (object key in keys)
			{
				if (key == null) continue;
				composedKey.Append(key.ToString());
			}
			
			return composedKey.ToString();
		}

		//--------------------------------------------------------------------------------
		public SolutionCacheObject this[params object[] keys]
		{
			get
			{
				string key = GetComposedKey(keys);
				SolutionCacheObject o = cache[key] as SolutionCacheObject;
				if (o != null) return o;

				o = new SolutionCacheObject();
				cache[key] = o;
				return o;
			}
		}
	}

	//================================================================================
	public class SolutionCacheObject
	{
		object innerObject;

		//--------------------------------------------------------------------------------
		public object Object
		{
			get { return innerObject; }
			set { innerObject = value; }
		}
	}
}

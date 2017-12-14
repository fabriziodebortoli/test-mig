using System;
using System.Collections;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Core.XmlPersister;

namespace Microarea.Tools.TBLocalizer.SourceBinding
{
	//================================================================================
	public class BindingInfoCache
	{
		private static Hashtable bindingInfos = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
		
		//--------------------------------------------------------------------------------
		public static ProjectBindingInfo GetBindingInfo(string path)
		{
			ProjectBindingInfo bindingInfo = bindingInfos[path] as ProjectBindingInfo;
			if (bindingInfo != null) return bindingInfo;
			try
			{
				if (File.Exists(path))
				{
					XmlDocument doc = new XmlDocument();
					doc.Load(path);
					bindingInfo = SerializerUtility.DeserializeFromXmlNode(doc.DocumentElement, typeof(ProjectBindingInfo)) as ProjectBindingInfo;
					bindingInfo.InitParents();
				}

				if (bindingInfo == null)
					bindingInfo = new ProjectBindingInfo();

				bindingInfo.LoadLocalInfos(path);

			}
			catch
			{
				bindingInfo = new ProjectBindingInfo();
			}
			bindingInfos[path] = bindingInfo;
			return bindingInfo;				
		}

		//--------------------------------------------------------------------------------
		public static void ResetBindingInfo(string path)
		{
			ProjectBindingInfo bindingInfo = bindingInfos[path] as ProjectBindingInfo;
			if (bindingInfo != null)
			{
				bindingInfo.Dispose();
				bindingInfos[path] = null;
			}
		}

		//--------------------------------------------------------------------------------
		public static bool SaveBindingInfo(string path)
		{
			ProjectBindingInfo bindingInfo = bindingInfos[path] as ProjectBindingInfo;
			if (bindingInfo == null) throw new IOException(Strings.InvalidSourceControlFile);
			
			XmlNode n = SerializerUtility.SerializeToXmlNode(bindingInfo);

			if (n == null)
				return false;
			
			CommonUtilities.Functions.SafeDeleteFile(path);
			n.OwnerDocument.Save(path);

			bindingInfo.SaveLocalInfos(path);

			return true;
		}
	}
}

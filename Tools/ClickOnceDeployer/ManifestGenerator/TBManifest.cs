using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace ManifestGenerator
{
	//================================================================================
	[Serializable]
	public class TBManifest
	{
		public const string ManifestExtension = ".TbManifest";

		public List<TBManifestFile> Files = new List<TBManifestFile>();

		//---------------------------------------------------------------------------
		public void Save(string filePath)
		{
			try
			{
				XmlSerializer x = new XmlSerializer(GetType());
				using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
					x.Serialize(fs, this);
			}
			catch
			{
				try
				{
					if (File.Exists(filePath))
						File.Delete(filePath);
				}
				catch { }
			}
		}

		//---------------------------------------------------------------------------
		public static SerializedType Load<SerializedType>(string filePath) where SerializedType : new()
		{
			try
			{
				if (!File.Exists(filePath))
					return new SerializedType();

				XmlSerializer x = new XmlSerializer(typeof(SerializedType));
				using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read))
					return (SerializedType)x.Deserialize(fs);
			}
			catch
			{
				try
				{
					if (File.Exists(filePath))
						File.Delete(filePath);
				}
				catch { }

				return new SerializedType();
			}
		}

		//--------------------------------------------------------------------------------
		public string GetManifestFileName(string mainAssemblyName)
		{
			return mainAssemblyName + ManifestExtension;
		}
	}

	//================================================================================
	[Serializable]
	public class TBManifestFile
	{
		//--------------------------------------------------------------------------------
		[XmlAttribute]
		public string Name { get; set; }
	}
}

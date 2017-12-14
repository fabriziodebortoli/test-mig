using System;
using System.IO;
using System.Xml.Serialization;

namespace Microarea.TaskBuilderNet.Core.SerializableTypes
{
	public class Serializable
	{
		//---------------------------------------------------------------------------
		public void Save(string filePath, Type[]extraTypes)
		{
			try
			{
				XmlSerializer x = new XmlSerializer(GetType(), extraTypes);
				
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
		public static SerializedType Load<SerializedType>(string filePath, Type[]extraTypes) where SerializedType : new()
		{
			try
			{
				if (!File.Exists(filePath))
					return new SerializedType();

				XmlSerializer x = new XmlSerializer(typeof(SerializedType), extraTypes);
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
	}
}

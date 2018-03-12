﻿using Microarea.Common.NameSolver;
using System;
using System.IO;
using System.Xml.Serialization;

namespace Microarea.Common.SerializableTypes
{

    //TODO LARA
    //la lascio sul FS xche ora come ora e' usata solo da applicationcache
    //pero forse un domani dovrebbe essere parametrizzata x salvare sempre su fs o appoggiarsi al pathfinder
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
					if (PathFinder.PathFinderInstance.ExistFile(filePath))
                        PathFinder.PathFinderInstance.RemoveFile(filePath);
				}
				catch { }
			}
		}

		//---------------------------------------------------------------------------
		public static SerializedType Load<SerializedType>(string filePath, Type[]extraTypes) where SerializedType : new()
		{
			try
			{
				if (!PathFinder.PathFinderInstance.ExistFile(filePath))
					return new SerializedType();

				XmlSerializer x = new XmlSerializer(typeof(SerializedType), extraTypes);
				using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read))
					return (SerializedType)x.Deserialize(fs);
			}
			catch
			{
				try
				{
					if (PathFinder.PathFinderInstance.ExistFile(filePath))
                        PathFinder.PathFinderInstance.RemoveFile(filePath);
				}
				catch { }

				return new SerializedType();
			}
		}
	}
}

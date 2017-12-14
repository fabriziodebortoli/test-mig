using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Microarea.EasyBuilder.Licence
{
	public partial class Product
	{
		/// <remarks/>
		//---------------------------------------------------------------------
		public static Product FromFile(string filePath)
		{
			if (filePath == null)
				throw new ArgumentNullException("filePath");

			if (filePath.Trim().Length == 0)
				throw new ArgumentException("'filePath' is an empty string", "filePath");

			if (!File.Exists(filePath))
				throw new ArgumentException("'filePath' does not exist", "filePath");

			using (FileStream inputFile = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				return FromStream(inputFile);
		}

		/// <remarks/>
		//---------------------------------------------------------------------
		public static Product FromStream(Stream inputFile)
		{
			XmlReader xmlReader = XmlReader.Create(inputFile);
			XmlSerializer serializer = new XmlSerializer(typeof(Product));

			Product aProduct = serializer.Deserialize(xmlReader) as Product;

			xmlReader.Close();

			return aProduct;
		}

		/// <remarks/>
		//---------------------------------------------------------------------
		public void ToFile(string filePath, bool overrideIfAlreadyExists)
		{
			if (filePath == null)
				throw new ArgumentNullException("filePath");

			if (filePath.Trim().Length == 0)
				throw new ArgumentException("'filePath' is an empty string", "filePath");

			if (File.Exists(filePath) && !overrideIfAlreadyExists)
				throw new ArgumentException("'filePath' does not exist", "filePath");

			using (FileStream outputFile = new FileStream(filePath, FileMode.Create, FileAccess.Write))
				ToStream(outputFile);
		}

		/// <remarks/>
		//---------------------------------------------------------------------
		public void ToStream(Stream outputStream)
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.ConformanceLevel = ConformanceLevel.Document;
			settings.Encoding = Encoding.UTF8;
			settings.Indent = true;

			XmlSerializer serializer = new XmlSerializer(typeof(Product));
			serializer.Serialize(outputStream, this);			
		}
	}
}

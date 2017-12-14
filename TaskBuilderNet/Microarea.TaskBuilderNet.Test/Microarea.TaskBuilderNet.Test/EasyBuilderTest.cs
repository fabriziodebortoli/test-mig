using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Microarea.EasyBuilder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microarea.TaskBuilderNet.Test
{
	[TestClass]
	public class EasyBuilderTest
	{
		//[TestMethod]
		public void TestCustomizationSerializaion()
		{
			// Custom test files list 
			string[] customTestFiles = new string[] 
                { 
                  "Microarea.TaskBuilderNet.Test.Customization.ERP.Company.Documents.Company.Company.dat",
                  "Microarea.TaskBuilderNet.Test.Customization.ERP.Sales.Documents.Invoice.Invoice.dat",
                  "Microarea.TaskBuilderNet.Test.Customization.ERP.Sales.Documents.DebitNote.DebitNote.dat",
                  "Microarea.TaskBuilderNet.Test.Customization.NewApp.NewModule.DynamicDocuments.CustomTest.CustomTest.dat"};

			// Test
			foreach (string customTestFile in customTestFiles)
			{
				using (Stream st = Assembly.GetExecutingAssembly().GetManifestResourceStream(customTestFile))
				{
					BinaryFormatter formatter = new BinaryFormatter();
					CustomizationInfos i = formatter.Deserialize(st) as CustomizationInfos;
					CustomizationSerializer ser = new CustomizationSerializer();
					byte[] buff;
					using (MemoryStream fs = new MemoryStream())
					{
						using (TextWriter tw = new StreamWriter(fs))
						{
							ser.Serialize(i, tw);
						}
						buff = fs.ToArray();
					}

					//using (FileStream file = new FileStream("c:\\test.txt", FileMode.Create))
					//	file.Write(buff, 0, buff.Length);
					CustomizationInfos i1 = null;
					using (Stream fs = new MemoryStream(buff))
					{
						using (TextReader tr = new StreamReader(fs))
						{
							i1 = ser.Deserialize(tr) as CustomizationInfos;
						}
					}

					string c1 = GetCode(i);
					string c2 = GetCode(i1);
					if (!c1.Equals(c2))
						throw new ApplicationException("Errore nel matching degli oggetti dopo la deserializzazione");
				}
			}
		}

		//--------------------------------------------------------------------------------
		private string GetCode(CustomizationInfos customizationInfos)
		{
			if (customizationInfos == null)
				return string.Empty;

			using (CodeDomProvider compiler = new Microsoft.CSharp.CSharpCodeProvider())
			using (StringWriter sw = new StringWriter())
			{


				CodeGeneratorOptions options = new CodeGeneratorOptions();
				options.BracingStyle = "C";

				if (customizationInfos.UsingCodeNamespace != null)
					compiler.GenerateCodeFromNamespace(customizationInfos.UsingCodeNamespace, sw, options);
				if (customizationInfos.CodeNamespace != null)
					compiler.GenerateCodeFromNamespace(customizationInfos.CodeNamespace, sw, options);
				if (customizationInfos.CodeNamespaceMethods != null)
					compiler.GenerateCodeFromNamespace(customizationInfos.CodeNamespaceMethods, sw, options);

				return sw.ToString();
			}
		}
	}
}

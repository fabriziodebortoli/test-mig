using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using Microsoft.CSharp;

namespace Microarea.Tools.TBLocalizer
{
	public class SatelliteAssemblyBuilder
	{
		//-----------------------------------------------------------------------------
		public static void ProduceAssembly(string sourcePath, string outputPath, string culture, string assemblyFileName, string frameworkVersion)
		{
			Dictionary<string, string> options = new Dictionary<string, string>();
            if (frameworkVersion.StartsWith("v4."))
                frameworkVersion = "v4.0";
			options.Add("CompilerVersion", frameworkVersion);

			using (CodeDomProvider compiler = new CSharpCodeProvider(options))
			{
				CompilerParameters parameters = new CompilerParameters();
				parameters.IncludeDebugInformation = false;
				
				parameters.GenerateInMemory = false;
				parameters.OutputAssembly = Path.Combine(outputPath, assemblyFileName);


				foreach (string file in Directory.GetFiles(sourcePath, "*" + AllStrings.resxExtension))
				{
					string resourcesFile = Path.ChangeExtension(file, ".resources");
					try
					{
						using (IResourceWriter rw = new ResourceWriter(resourcesFile))
						{
							using (ResXResourceReader rr = new ResXResourceReader(file))
							{
								IDictionaryEnumerator de = rr.GetEnumerator();
								while (de.MoveNext())
								{
									if (!(de.Value is string))
										continue;
									rw.AddResource(de.Key as string, de.Value);
								}
							}
						}
						parameters.EmbeddedResources.Add(resourcesFile);
				
					}
					catch (Exception ex)
					{
						throw new ApplicationException(string.Format("Error parsing file {0}: {1}", file, ex.Message));
					}
				}
				string content = string.Format(@"
using System.Reflection;
using System.Runtime.CompilerServices;
[assembly: AssemblyCompany(""Microarea S.p.A."")]
[assembly: AssemblyProduct(""International Studio generated satellite assembly"")]
[assembly: AssemblyDescription(""Contains localization resources for TaskBuilder.Net products"")]
[assembly: AssemblyCopyright(""International Studio: (c) Microarea S.p.A.  All rights reserved."")]
[assembly: AssemblyVersion(""{0}"")]
[assembly: AssemblyCulture(""{1}"")]", Assembly.GetExecutingAssembly().GetName().Version.ToString(), culture);

				CompilerResults res = compiler.CompileAssemblyFromSource(parameters, content);
				if (res.Errors.HasErrors)
				{
					StringBuilder sb = new StringBuilder();
					foreach (CompilerError error in res.Errors)
					{
						sb.AppendLine(error.ErrorText);
					}

					throw new ApplicationException(string.Format("Error building assemby {0}:\r\n{1}", assemblyFileName, sb.ToString()));
				}

			}
		}
	
		//-----------------------------------------------------------------------------
		//VECCHIA VERSIONE (non permette di scegliere il framework dell'assembly)
	//    public static void ProduceAssembly(string sourcePath, string outputPath, string culture, string assemblyFileName, string frameworkVersion)
	//    {
	//        AppDomain myDomain = System.Threading.Thread.GetDomain();
	//        AssemblyName myAsmName = new AssemblyName();
	//        myAsmName.Name = Path.GetFileNameWithoutExtension(assemblyFileName);
	//        myAsmName.CultureInfo = new System.Globalization.CultureInfo(culture);
	//        AssemblyBuilder myAsmBuilder = myDomain.DefineDynamicAssembly(
	//            myAsmName,
	//            AssemblyBuilderAccess.Save,
	//            outputPath
	//            );

	//        ModuleBuilder mb = myAsmBuilder.DefineDynamicModule(assemblyFileName, assemblyFileName);
			
	//        foreach (string file in Directory.GetFiles(sourcePath))
	//        {
	//            string resourcesFile = Path.ChangeExtension(Path.GetFileName(file), ".resources");
	//            try
	//            {
	//                IResourceWriter rw = mb.DefineResource(resourcesFile, resourcesFile);
	//                using (ResXResourceReader rr = new ResXResourceReader(file))
	//                {
	//                    IDictionaryEnumerator de = rr.GetEnumerator();
	//                    while (de.MoveNext())
	//                    {
	//                        if (!(de.Value is string))
	//                            continue;
	//                        rw.AddResource(de.Key as string, de.Value);
	//                    }
	//                }
	//            }
	//            catch (Exception ex)
	//            {
	//                throw new ApplicationException(string.Format("Error parsing file {0}: {1}", file, ex.Message));
	//            }
	//        }

	//        myAsmBuilder.Save(assemblyFileName);
	//    }
	}
}

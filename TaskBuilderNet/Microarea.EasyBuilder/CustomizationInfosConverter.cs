using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.CodeDom.Compiler;
using System.IO;

using System.CodeDom;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;

using ICSharpCode.NRefactory.MonoCSharp;
using ICSharpCode.NRefactory.CSharp;
using Microsoft.CSharp;
using System.Linq;
using System.Globalization;

namespace Microarea.EasyBuilder
{
	//=========================================================================
	sealed class CustomizationInfosConverter
	{
		//--------------------------------------------------------------------------------
		private CustomizationInfosConverter()
		{

		}

		//--------------------------------------------------------------------------------
		public static NewCustomizationInfos ConvertDatToNewCustomizationInfos(
			Stream s,
			string documentName
            )
		{
			BinaryFormatter formatter = new BinaryFormatter();
			CustomizationInfos oldCustomizationInfo = formatter.Deserialize(s) as CustomizationInfos;

			using (CodeDomProvider compiler = new CSharpCodeProvider())
			{
				string folder = Path.Combine(
					Path.GetTempPath(),
					Path.GetFileNameWithoutExtension(Path.GetTempFileName())
					);
				if (!Directory.Exists(folder))
					Directory.CreateDirectory(folder);

				GenerateSources(oldCustomizationInfo, folder);

				return ParseFolder(folder, documentName);
			}
		}

		//--------------------------------------------------------------------------------
		public static NewCustomizationInfos ParseFolder(
			string folder,
			string documentName
			)
		{
			SyntaxTree userMethodsCompilationUnit = null;
			SyntaxTree metadataInfoCompilationUnit = null;
			SyntaxTree ebDesignerCompilationUnit = new SyntaxTree();
			List<SyntaxTree> additionalCompilationUnits = new List<SyntaxTree>();

			DirectoryInfo workingFolder = new DirectoryInfo(folder);
			SyntaxTree compilationUnit = null;

			string userMethodsCode = string.Empty;
		
            string controllerFileCode = Path.ChangeExtension(Path.Combine(workingFolder.FullName, documentName), NameSolverStrings.DllExtension);
            string sourcesPath = Sources.GetSourcesPath(controllerFileCode);
            string[] sourceFiles = Sources.GetSourcesFilePathForDocument(sourcesPath);
			if (sourceFiles == null)
				throw new NoSourcesOrDatException();

			foreach (var sourceFile in sourceFiles)
			{
				string text = string.Empty;
				using (StreamReader s = new StreamReader(sourceFile))
				{
					text = s.ReadToEnd();
				}
				compilationUnit = AstFacilities.Parse(text, sourceFile);

				string fileName = Path.GetFileName(sourceFile);
				if (
					fileName.Contains(
						NewCustomizationInfos.UserMethodsSourceFileExtension + "."
						)
					)
				{
					userMethodsCompilationUnit = compilationUnit;

                    bool oldAttributesFound = PurgeOldAttributes(userMethodsCompilationUnit);
                    userMethodsCode = oldAttributesFound ? AstFacilities.GetVisitedText(userMethodsCompilationUnit) : text;
				}
				else if (Sources.IsABusinessObjectSourceFile(compilationUnit))
				{
					additionalCompilationUnits.Add(compilationUnit);
				}
				else if (fileName.Contains(NewCustomizationInfos.MetaSourceFileExtension + "."))
				{
					metadataInfoCompilationUnit = compilationUnit;
				}
				else
				{
					UpdateCompilationUnit(ebDesignerCompilationUnit, compilationUnit);
				}

			}

			NewCustomizationInfos newCustomizationInfo = new NewCustomizationInfos(
				userMethodsCompilationUnit,
				metadataInfoCompilationUnit,
				ebDesignerCompilationUnit,
				additionalCompilationUnits
				);

			newCustomizationInfo.UserMethodsCode = userMethodsCode;
			return newCustomizationInfo;
		}

        //--------------------------------------------------------------------------------
        private static bool PurgeOldAttributes(SyntaxTree userMethodsCompilationUnit)
        {
            if (userMethodsCompilationUnit == null || userMethodsCompilationUnit.Children == null || userMethodsCompilationUnit.Children.Count() == 0)
            {
                return false;
            }

            bool oldAttributesFound = false;
            NamespaceDeclaration nsd = null;
            TypeDeclaration td = null;
            foreach (var child in userMethodsCompilationUnit.Children)
            {
                nsd = child as NamespaceDeclaration;
                if (nsd == null)
                {
                    continue;
                }

                var toBeRemovedAttributes = new List<ICSharpCode.NRefactory.CSharp.Attribute>();
                var toBeRemovedAttributeSections = new List<ICSharpCode.NRefactory.CSharp.AttributeSection>();
                foreach (var @class in nsd.Children)
                {
                    td = @class as TypeDeclaration;
                    if (td == null)
                    {
                        continue;
                    }

                    foreach (var member in td.Members)
                    {
                        if (member.Attributes == null || member.Attributes.Count == 0)
                        {
                            continue;
                        }
                        foreach (var @as in member.Attributes)
                        {
                            if (@as.Attributes == null || @as.Attributes.Count == 0)
                            {
                                continue;
                            }
                            foreach (var attr in @as.Attributes)
                            {
                                var memberType = attr.Type as MemberType;
                                if (memberType == null)
                                {
                                    continue;
                                }
                                if (String.Compare(memberType.MemberName, "UserCustomMethodAttribute", StringComparison.InvariantCulture) == 0)
                                {
                                    toBeRemovedAttributes.Add(attr);
                                }
                            }
                            toBeRemovedAttributeSections.Add(@as);
                        }
                    }
                }
                foreach (var attr in toBeRemovedAttributes)
                {
                    attr.Remove();
                    oldAttributesFound = true;
                }
                foreach (var attrSec in toBeRemovedAttributeSections)
                {
                    if (attrSec.Attributes.Count == 0)
                    {
                        attrSec.Remove();
                    }
                }
            }
            return oldAttributesFound;
        }

        //--------------------------------------------------------------------------------
        private static void UpdateCompilationUnit(
			SyntaxTree targetCompilationUnit,
			SyntaxTree sourceCompilationUnit
			)
		{
			NamespaceDeclaration sourceNs = null;
			NamespaceDeclaration targetNs = null;
			UsingDeclaration sourceUd = null;
			foreach (AstNode node in sourceCompilationUnit.Members)
			{
				sourceNs = node as NamespaceDeclaration;
				if (sourceNs != null)
				{
					if (!targetCompilationUnit.ContainsNamespaceDeclaration(sourceNs, out targetNs))
					{
						targetNs = new NamespaceDeclaration(sourceNs.Name);
						targetCompilationUnit.Members.Add(targetNs);
					}
					foreach (TypeDeclaration tdNode in sourceNs.Members)
					{
						if (!ContainsTypeDeclaration(targetNs, tdNode))
						{
							targetNs.Members.Add(tdNode.Clone() as TypeDeclaration);
						}
					}
					continue;
				}
				sourceUd = node as UsingDeclaration;
				if (sourceUd != null)
				{
					sourceUd = sourceUd.Clone() as UsingDeclaration;
					if (!ContainsUsingDeclaration(targetCompilationUnit, sourceUd?.Namespace))
						targetCompilationUnit.Members.Add(sourceUd);

					continue;
				}
			}
		}

		//--------------------------------------------------------------------------------
		private static bool ContainsUsingDeclaration(
			SyntaxTree targetCompilationUnit,
			string @using
			)
		{
			UsingDeclaration usingDeclaration = null;
			foreach (var item in targetCompilationUnit.Members)
			{
				usingDeclaration = item as UsingDeclaration;
				if (usingDeclaration == null)
					continue;

				if (string.Compare(usingDeclaration.Namespace, @using, StringComparison.InvariantCulture) == 0)
					return true;
			}

			return false;
		}

		//--------------------------------------------------------------------------------
		private static bool ContainsTypeDeclaration(NamespaceDeclaration targetNs, TypeDeclaration tdNode)
		{
			TypeDeclaration temp = null;
			foreach (var item in targetNs.Members)
			{
				temp = item as TypeDeclaration;
				if (temp == null)
					continue;

				if (String.Compare(temp.Name, tdNode.Name, StringComparison.InvariantCulture) == 0)
					return true;
			}

			return false;
		}

	
		//--------------------------------------------------------------------------------
		private static void GenerateSources(CustomizationInfos oldCustomizationInfo, string folder)
		{
			DirectoryInfo di = new DirectoryInfo(folder);
            string basePath = Path.Combine(folder, oldCustomizationInfo.Namespace.Leaf);

			string controllerCodeFile = Path.ChangeExtension(basePath, NewCustomizationInfos.CSSourceFileExtension);
			string controllerMethodsCodeFile = Path.ChangeExtension(basePath, NewCustomizationInfos.UserMethodsSourceFileExtension + NewCustomizationInfos.CSSourceFileExtension);

			//Creo il file contenente il codice serializzato del controller (niente metodi personalizzati)
			using (StreamWriter sw = new StreamWriter(controllerCodeFile))
			{
				sw.Write(GetCode(oldCustomizationInfo.CodeNamespace, oldCustomizationInfo));
			}

			//Creo il file contenente il codice degli user method del controller 
			using (StreamWriter sw = new StreamWriter(controllerMethodsCodeFile))
			{
				sw.Write(GetCode(oldCustomizationInfo.CodeNamespaceMethods, oldCustomizationInfo));
			}

			// mi preoccupo di togliere i cadaveri che mi appartengono
			string sourcesPath = Path.GetDirectoryName(controllerCodeFile);
			string additionalFilesPrefix = string.Format("{0}_", Path.GetFileNameWithoutExtension(controllerCodeFile));
			try
			{
				string searchPattern = string.Format("{0}*{1}", additionalFilesPrefix, NewCustomizationInfos.CSSourceFileExtension);

				string[] oldSources = Directory.GetFiles(sourcesPath, searchPattern);
				foreach (string source in oldSources)
					File.Delete(source);
			}
			catch
			{
				// se va in errore in questo non è un grosso problema, al massimo
				// rimane qualche sorgente in più nella directory
			}

			foreach (CodeNamespace additionalNs in oldCustomizationInfo.AdditionalNamespaces)
			{
				string sourceFileName = Path.Combine(sourcesPath, string.Format("{0}{1}{2}", additionalFilesPrefix, additionalNs.Name, NewCustomizationInfos.CSSourceFileExtension));
				using (StreamWriter sw = new StreamWriter(sourceFileName))
				{
					sw.Write(GetCode(additionalNs, oldCustomizationInfo));
				}
			}
		}

		//--------------------------------------------------------------------------------
		private static string GetCode(CodeNamespace ns, CustomizationInfos customizationInfo)
		{
			using (CodeDomProvider compiler = new CSharpCodeProvider())
			using (StringWriter sw = new StringWriter())
			{
				CodeGeneratorOptions options = new CodeGeneratorOptions();
				options.BracingStyle = "C";

				compiler.GenerateCodeFromNamespace(ns, sw, options);
				return sw.ToString();
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;

using Microarea.EasyBuilder.Localization;

using System.IO;
using System.Resources;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.EasyBuilder.Packager;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.EasyBuilder.Properties;
using Microsoft.Win32;
using System.Diagnostics;
using ICSharpCode.NRefactory.CSharp;

namespace Microarea.EasyBuilder
{
	//=========================================================================
	internal interface IBuildStrategy
	{
        EBCompilerResults Build(Sources sources);
	}

	//=========================================================================
	internal abstract class BuildStrategy : IBuildStrategy
	{
		public abstract EBCompilerResults Build(Sources sources);

		//--------------------------------------------------------------------------------
		protected BuildStrategy()
		{

		}

		//--------------------------------------------------------------------------------
		protected virtual void AddLocalizationFiles(
			Dictionaries dictionaries,
			NewCustomizationInfos customizationInfos,
			CompilerParameters parameters,
			bool saveFiles
			)
		{
			NamespaceDeclaration ns = EasyBuilderSerializer.GetNamespaceDeclaration(customizationInfos.EbDesignerCompilationUnit);
			foreach (string culture in dictionaries.Keys)
			{
				string resourceFile = Path.Combine
					(
					Path.GetTempPath(),
					LocalizationSources.GetResourceFileFromCulture(culture, ns.Name)
					);

				Dictionary dictionary = dictionaries[culture];

				using (ResourceWriter writer = new ResourceWriter(resourceFile))
				{
					foreach (KeyValuePair<string, string> pair in dictionary)
						writer.AddResource(pair.Key, pair.Value);
				}
				parameters.EmbeddedResources.Add(resourceFile);
			}
		}
	}
}

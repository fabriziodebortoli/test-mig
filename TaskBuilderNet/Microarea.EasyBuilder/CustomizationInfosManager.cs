using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using System;
using System.Collections.Generic;
using System.CodeDom;
using Microarea.EasyBuilder.Packager;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using ICSharpCode.NRefactory;

using System.Diagnostics;

namespace Microarea.EasyBuilder
{
	//=========================================================================
	internal class SourcesCustomizationInfoManager
	{
		/// <summary>
		/// Returns the file name where to embedd the code namespace during the build.
		/// </summary>
		/// <param name="customizationNamespace"></param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		private string GetCodeNamespaceResourceName(string customizationNamespace)
		{
			return ControllerSources.GetSafeSerializedNamespace(customizationNamespace) + ".dat";
		}

		//--------------------------------------------------------------------------------
		public NewCustomizationInfos LoadCustomizationInfos(
			INameSpace customizationNamespace,
			Assembly asm
			)
		{
			if (asm != null)
			{
				Stream s = asm.GetManifestResourceStream(GetCodeNamespaceResourceName(customizationNamespace.FullNameSpace));
				if (s != null)
					return CustomizationInfosConverter.ConvertDatToNewCustomizationInfos(s, customizationNamespace.Leaf);
			}

			string customizationPath =
				BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationType == ApplicationType.Customization
				? BasePathFinder.BasePathFinderInstance.GetCustomizationPath(customizationNamespace, CUtility.GetUser(), BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp)
				: BasePathFinder.BasePathFinderInstance.GetStandardDocumentPath(customizationNamespace);

			return CustomizationInfosConverter.ParseFolder(customizationPath, customizationNamespace.Leaf);
		}
	}
}

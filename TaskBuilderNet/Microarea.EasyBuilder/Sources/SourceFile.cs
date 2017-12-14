using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.CodeDom.Compiler;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.EasyBuilder
{
	//=========================================================================
	/// <summary>
	/// SourceFile
	/// </summary>
	internal class SourceFile
	{
		public string FileName { get; private set; }
		public string Name { get; private set; }
		public string Content { get; private set; }

		public SourceFile(string name, string content, NameSpace addNS)
		{
			this.Name = name;
			this.Content = content;
			this.FileName =	Sources.GetCustomizedFilesPath(addNS, name);
		}
	}

	//=========================================================================
	internal static class CodeDomProviderExtension
	{
		public static CompilerResults CompileAssemblyFromSource(
			this CodeDomProvider @this,
			CompilerParameters parameters,
			SourceFile[] sourceFiles
			)
		{
			List<string> sources = new List<string>();
			foreach (var sourceFile in sourceFiles)
			{
				sources.Add(sourceFile.Content);
			}

			return @this.CompileAssemblyFromSource(parameters, sources.ToArray());
		}
	}
}

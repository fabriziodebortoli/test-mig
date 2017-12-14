using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Documentation;
using ICSharpCode.NRefactory.TypeSystem;
using Microarea.EasyBuilder.CodeEditorProviders;
using Microarea.EasyBuilder.MVC;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.SoapCall;

namespace Microarea.EasyBuilder
{
	//=========================================================================
	internal class ProjectContentFacilities
	{
		static object lockObject = new object();
		static IProjectContent projectContent = null;
		public static IProjectContent GetDefaultProjectContent(string assemblyName)
		{
			lock (lockObject)
			{
				if (projectContent == null)
				{
					projectContent = new CSharpProjectContent();
					Init();
				}
			}
			return projectContent.SetAssemblyName(assemblyName);
		}

		//-------------------------------------------------------------------------------
		public static XmlDocumentationProvider GetXmlDocumentation(string dllPath)
		{
			if (string.IsNullOrEmpty(dllPath))
				return null;

			var xmlFileName = Path.GetFileNameWithoutExtension(dllPath) + ".xml";
			var localPath = Path.Combine(Path.GetDirectoryName(dllPath), xmlFileName);
			if (File.Exists(localPath))
				return new XmlDocumentationProvider(localPath);

			//if it's a .NET framework assembly it's in one of following folders
			var netPath = Path.Combine(@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0", xmlFileName);
			if (File.Exists(netPath))
				return new XmlDocumentationProvider(netPath);

			return null;
		}


		//-------------------------------------------------------------------------------
		public static readonly List<string> DefaultUsings = new List<string>()
		{
				typeof(System.IntPtr).Namespace,
				typeof(System.Data.DataColumn).Namespace,
				typeof(System.Drawing.Brush).Namespace,
				typeof(System.Xml.XmlDocument).Namespace,
				typeof(System.Windows.Forms.Form).Namespace,
				typeof(System.ComponentModel.INotifyPropertyChanged).Namespace,
				typeof(Microarea.TaskBuilderNet.Interfaces.Model.IRecord).Namespace,
				typeof(Microarea.Framework.TBApplicationWrapper.MSqlRecord).Namespace,
				typeof(Microarea.TaskBuilderNet.Core.Generic.NameSpace).Namespace,
				typeof(Microarea.TaskBuilderNet.Core.EasyBuilder.EventInfo).Namespace,
				typeof(Microarea.TaskBuilderNet.Core.CoreTypes.DataArray).Namespace,
				typeof(Microarea.TaskBuilderNet.Interfaces.IDocumentInfo).Namespace,
				typeof(Microarea.TaskBuilderNet.Interfaces.View.IWindowWrapperContainer).Namespace,
				typeof(Microarea.TaskBuilderNet.UI.WinControls.TBTabPage).Namespace,
				typeof(DocumentView).Namespace,
				typeof(ControllerEventArgs).Namespace,
				typeof(OleDbConnection).Namespace,
				typeof(SqlConnection).Namespace,
				typeof(DataSet).Namespace,
				typeof(DbConnection).Namespace,
				typeof(IList<string>).Namespace
		};

		//-------------------------------------------------------------------------------
		private static void Init()
		{
			List<Assembly> typeAssemblies = new List<Assembly>()
			{
				typeof(object).Assembly, // mscorlib
				typeof(System.Data.DataColumn).Assembly,
				typeof(System.Drawing.Brush).Assembly,
				typeof(System.Xml.XmlDocument).Assembly,
				typeof(System.Windows.Forms.Form).Assembly,
				typeof(System.ComponentModel.INotifyPropertyChanged).Assembly,
				typeof(Microarea.TaskBuilderNet.Interfaces.Model.IRecord).Assembly,
				typeof(Microarea.Framework.TBApplicationWrapper.MSqlRecord).Assembly,
				typeof(Microarea.TaskBuilderNet.Core.Generic.NameSpace).Assembly,
				typeof(Microarea.TaskBuilderNet.UI.WinControls.TBTabPage).Assembly,
				typeof(DocumentView).Assembly,
				typeof(IList<string>).Assembly
			};

			//aggiungo la reference ad una classe dell'assembly delle funzioni esterne così lo vedo nell'editor
			Assembly asmb = ServiceClientCache.GetServicesAssembly();
			if (asmb != null)
            {
                typeAssemblies.Add(asmb);

                var type = asmb.GetTypes().FirstOrDefault();
                if (type != null)
                {
                    var serviceClientCacheNamespace = type.Namespace;
                    if (!DefaultUsings.Contains(serviceClientCacheNamespace, StringComparer.InvariantCulture))
                    {
                        DefaultUsings.Add(serviceClientCacheNamespace);
                    }
                }
            }
				

			var unresolvedAssemblies = new IUnresolvedAssembly[typeAssemblies.Count];
			Parallel.For(
				0, typeAssemblies.Count,
				delegate (int i)
				{
					var loader = new CecilLoader();
					var path = typeAssemblies[i].Location;
					loader.DocumentationProvider = GetXmlDocumentation(typeAssemblies[i].Location);
					unresolvedAssemblies[i] = loader.LoadAssemblyFile(typeAssemblies[i].Location);
				});
			
			projectContent = projectContent.AddAssemblyReferences(unresolvedAssemblies);
		}
	}
}

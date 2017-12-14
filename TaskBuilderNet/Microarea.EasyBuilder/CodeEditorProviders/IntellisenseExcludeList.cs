using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.MonoCSharp;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using Microarea.EasyBuilder.CodeCompletion;
using Microarea.TaskBuilderNet.Core.EasyBuilder;

namespace Microarea.EasyBuilder.CodeEditorProviders
{
	//=========================================================================
	internal class IntellisenseExcludeList
	{
		static IntellisenseExcludeList instance;
		public static IntellisenseExcludeList Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new IntellisenseExcludeList();
					instance.PopulateIntellisenseExcludes();
				}

				return instance;
			}
		}

		private Dictionary<string, string> intellisenseExcludes = null;

		/// <summary>
		/// Gets a list of keyword excluded from the code completion functionality.
		/// </summary>
		//--------------------------------------------------------------------------------
		public Dictionary<string, string> IntellisenseExcludes { get { return intellisenseExcludes; } }

		//--------------------------------------------------------------------------------
		internal void PopulateIntellisenseExcludes()
		{
			List<Assembly> assemblyToExplore = new List<Assembly>()
			{
				typeof(Microarea.TaskBuilderNet.Interfaces.Model.IRecord).Assembly,
				typeof(Microarea.TaskBuilderNet.Core.Generic.NameSpace).Assembly,
				typeof(Microarea.TaskBuilderNet.UI.WinControls.TBTabPage).Assembly,
				typeof(Microarea.EasyBuilder.MVC.DocumentView).Assembly,
				typeof(Microarea.Framework.TBApplicationWrapper.MSqlRecord).Assembly,
			};

			//uso una lista temporanea di stringhe, il parallel foreach è in grado di lockare la lista 
			//solamente al termine della procedura di ricerca negli assembly.
			List<string> tempList = new List<string>();
			Parallel.ForEach(assemblyToExplore, (Assembly asm) =>
			{
				List<string> list = GetExcludeIntellisenseForAssembly(asm);
				lock (tempList)
				{
					tempList.AddRange(list);
				}
			});

			//una volta completata la lista, la travaso in un dictionary (dove il value è empty perchè non mi interessa)
			//tale dictionary è molto più veloce (fino a 6000 elementi per l'intellisense del ctrlspace) portando la ricerca
			//da 300ms a 0
			string outValue = string.Empty;
			intellisenseExcludes = new Dictionary<string, string>();
			foreach (string item in tempList)
			{
				if (!intellisenseExcludes.TryGetValue(item, out outValue))
					intellisenseExcludes.Add(item, "");
			}
		}

		//-------------------------------------------------------------------------------
		private List<string> GetExcludeIntellisenseForAssembly(Assembly asm)
		{
			List<string> list = new List<string>();
			foreach (Type type in asm.GetTypes())
			{
				if (type == null || !type.IsClass)
					continue;

				MemberInfo[] info = GetFilteredMemberInfo(type);
				if (info == null || info.Count() <= 0)
					continue;

				foreach (MemberInfo currentMember in info)
				{
					if (currentMember == null || currentMember.ReflectedType == null)
						continue;

					string excludedItem = string.Format("{0}.{1}", currentMember.ReflectedType.FullName, currentMember.Name);
					if (!list.Contains(excludedItem))
						list.Add(excludedItem);
				}
			}
			return list;
		}

		//-------------------------------------------------------------------------------
		private static MemberInfo[] GetFilteredMemberInfo(Type type)
		{
			if (type == null)
				return null;

			try
			{
				MemberInfo[] info = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
				if (info == null || info.Count() <= 0)
					return null;

				//mi interessano tutti i type che non risiedono in dll della gac e che hanno l'attributo ExcludeFromIntellisense
				var filteredInfo = from s in info
								   where
								   s.GetCustomAttributes(typeof(ExcludeFromIntellisenseAttribute), true).Count() > 0
								   select s;

				if (filteredInfo == null || !filteredInfo.Any())
					return null;

				return filteredInfo.ToArray();
			}
			catch
			{
				return null;
			}
		}

		
		//--------------------------------------------------------------------------------
		internal static IntellisenseExcludeList PopulateIntellisense(IEnumerable<IAssemblyReference> assemblyReferences)
		{
			if (instance == null)
			{
				instance = new IntellisenseExcludeList();
				instance.PopulateIntellisenseExcludes();
			}
			return instance;
		}
	}
}

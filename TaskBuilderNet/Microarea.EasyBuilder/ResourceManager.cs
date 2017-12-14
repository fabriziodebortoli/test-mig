using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using Microarea.EasyBuilder.Packager;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyBuilder
{
	//=========================================================================
	/// <summary>
	/// Controls localization loading from the actual assembly and not from the
	/// satellite assembly.
	/// </summary>
	public class CustomizationComponentResourceManager : ComponentResourceManager
	{
		Dictionary<CultureInfo, ResourceSet> resourceSets = new Dictionary<CultureInfo, ResourceSet>();
		string customizationNamespace;
		string commonAssemblyPath = "";
		string module = "";
		//---------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the CustomizationComponentResourceManager.
		/// </summary>
		/// <param name="t">The type used to load localizations.</param>
		/// <seealso cref="System.Type"/>
		//--------------------------------------------------------------------------------
		public CustomizationComponentResourceManager(Type t)
			: base(t)
		{
			customizationNamespace = t.FullName;
			string app;
			if (OwnerEasyBuilderAppAttribute.GetModuleInfo(t.Assembly, out app, out module))
				commonAssemblyPath = Path.GetDirectoryName(AssemblyPackager.GetEBModuleAssemblyPath(app, module));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="culture"></param>
		/// <param name="createIfNotExists"></param>
		/// <param name="tryParents"></param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		protected override ResourceSet InternalGetResourceSet(
			CultureInfo culture,
			bool createIfNotExists,
			bool tryParents
			)
		{
			ResourceSet set = GetExactResource(culture);
			if (set == null && tryParents && culture != CultureInfo.InvariantCulture)
				return InternalGetResourceSet(culture.Parent, createIfNotExists, true);
			return set;
		}

		//--------------------------------------------------------------------------------
		private ResourceSet GetExactResource(System.Globalization.CultureInfo culture)
		{
			ResourceSet set = null;
			if (resourceSets.TryGetValue(culture, out set))
				return set;

			set = GetCustomizedResourceSet(culture);
			resourceSets[culture] = set;


			return set;
		}

		/// <summary>
		/// Retrieves a localizaed string using the given name and culture.
		/// </summary>
		/// <param name="name">
		/// The name of the string to be translated.
		/// </param>
		/// <param name="culture">
		/// The culture used to retrieve the translation.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// The name is null.
		/// </exception>
		/// <remarks>Inherited from ComponentResourceManager</remarks>
		/// <seealso cref="System.ComponentModel.ComponentResourceManager"/>
		//--------------------------------------------------------------------------------
		public override string GetString(string name, CultureInfo culture)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (culture == null)
			{
				culture = CultureInfo.CurrentUICulture;
			}
			ResourceSet set = this.InternalGetResourceSet(culture, true, true);
			if (set != null)
			{
				string str = set.GetString(name, true);
				if (str != null)
				{
					return str;
				}
			}
			ResourceSet set2 = null;
			while (!culture.Equals(CultureInfo.InvariantCulture))
			{
				culture = culture.Parent;
				set = this.InternalGetResourceSet(culture, true, true);
				if (set == null)
				{
					break;
				}
				if (set != set2)
				{
					string str2 = set.GetString(name, true);
					if (str2 != null)
					{
						return str2;
					}
					set2 = set;
				}
			}
			return null;
		}
		
		//--------------------------------------------------------------------------------
		private ResourceSet GetCustomizedResourceSet(System.Globalization.CultureInfo culture)
		{	
			StringBuilder s = new StringBuilder();
			s.Append(customizationNamespace);
			if (culture.Name.Length > 0)
			{
				s.Append(".");
				s.Append(culture.Name);
			}
			s.Append(".resources");
			
			if (commonAssemblyPath.Length > 0 && culture != CultureInfo.InvariantCulture)
			{
				string file = Path.Combine(commonAssemblyPath, culture.Name, module + ".resources.dll");
				if (File.Exists(file))
				{
					ResourceSet rs = LoadResourceSet(string.Concat(module, ".", NameSolverStrings.EBSources, '.', s), AssembliesLoader.Load(file));
					if (rs != null)
						return rs;

				}
			}
			return LoadResourceSet(s.ToString(), MainAssembly);
		}

		//--------------------------------------------------------------------------------
		private ResourceSet LoadResourceSet(string s, Assembly asm)
		{
			using (Stream str = asm.GetManifestResourceStream(s))
			{
				if (str == null)
					return null;
				using (ResourceReader rr = new ResourceReader(str))
					return new ResourceSet(rr);
			}
		}
	}
}

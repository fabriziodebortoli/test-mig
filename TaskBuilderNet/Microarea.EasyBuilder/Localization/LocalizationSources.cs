using System;

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Linq;
using Microarea.EasyBuilder.UI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;

using Microarea.TaskBuilderNet.Core.EasyBuilder;
using ICSharpCode.NRefactory.CSharp;

namespace Microarea.EasyBuilder.Localization
{
	/// <summary>
	/// Collezione di stringhe associate ad una particolare culture
	/// </summary>
	//================================================================================
	public class Dictionary : Dictionary<string, string> { }
	/// <summary>
	/// Collezione di dizionari
	/// </summary>
	//================================================================================
	public class Dictionaries : Dictionary<string, Dictionary> { }

	/// <summary>
	/// Localization manager
	/// </summary>
	public class LocalizationSources
	{
		/// <remarks/>
		internal const string ResourceManagerClassName = "Strings";
		/// <remarks/>
		internal const string ResourceManagerFieldName = "ResourceManager";
		/// <remarks/>
		internal const string ResourcesExtension = ".resources";
		internal const string ResxExtension = ".resx";
		internal const string dummyString = "dummy";

		private Dictionaries dictionaries = new Dictionaries();

		/// <summary>
		/// Occurs when source code generate dby EasyBuilder changed.
		/// </summary>
		public event EventHandler<CodeChangedEventArgs> CodeChanged;

		//-------------------------------------------------------------------------------
		/// <summary>
		/// Fired when code changes
		/// </summary>
		protected virtual void OnCodeChanged()
		{
			if (CodeChanged != null)
				CodeChanged(this, new CodeChangedEventArgs(ChangeType.CodeChanged));
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Internal use: stuff for localozation.
		/// </summary>
		public Dictionaries Dictionaries
		{
			get { return dictionaries; }
		}

		/// <remarks/>
		public const string ResourceExtension = ".resources";

		

		//-------------------------------------------------------------------------------
		internal static void SaveLocalizationFiles(Dictionary dictionary, string resxFile)
		{
            using (ResXResourceWriter writer = new ResXResourceWriter(resxFile))
            {
                foreach (KeyValuePair<string, string> pair in dictionary)
                {
                    writer.AddResource(pair.Key, pair.Value);
                }
            }
        }


        //-------------------------------------------------------------------------------
        internal static string GetResourceFileFromCulture(string culture, string customizationNamespace)
        {
            return string.Concat(customizationNamespace, '.', ResourceManagerClassName,
                (string.IsNullOrEmpty(culture) ? "" : "." + culture),
                ResourcesExtension);
        }

        //-------------------------------------------------------------------------------
        internal static string GetResxFileFromCulture(string culture)
        {
            return string.Concat(ResourceManagerClassName,
                (string.IsNullOrEmpty(culture) ? "" : "." + culture),
                ResxExtension);
        }



        //-------------------------------------------------------------------------------
        internal static string GetCultureFromResourceFile(string resource, string customizationNamespace)
		{
			string prefix = customizationNamespace + "." + ResourceManagerClassName;
			resource = Path.GetFileNameWithoutExtension(resource);
			resource = resource.Substring(prefix.Length);

			return resource.TrimStart('.');
		}

		/// <summary>
		/// Legge le stringhe localizzate dall'assembly e le memorizza per poterle poi risalvare 
		/// quando l'assembly verrà rigenerato
		/// </summary>
		/// <param name="customizationNamespace"></param>
		/// <param name="asm"></param>
		//-------------------------------------------------------------------------------
		public void ReadLocalizedStringsFromAssembly(INameSpace customizationNamespace, Assembly asm)
		{
			foreach (string resource in asm.GetManifestResourceNames())
			{
				if (!resource.EndsWith(ResourcesExtension))
					continue;

				string culture = GetCultureFromResxFile(resource, ControllerSources.GetSafeSerializedNamespace(customizationNamespace));
				using (Stream str = asm.GetManifestResourceStream(resource))
				{
					ResourceReader rr = new ResourceReader(str);
					IDictionaryEnumerator en = rr.GetEnumerator();

					while (en.MoveNext())
						AddLocalizableString(culture, (string)en.Key, (string)en.Value, false);
					rr.Close();
				}
			}
		}

		//-------------------------------------------------------------------------------
		internal void ReadLocalizedStringsFromResx(INameSpace customizationNamespace)
		{
			//string basePath = BasePathFinder.BasePathFinderInstance.GetStandardEBSourcesPath(BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationName, BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleName);

			//if (!Directory.Exists(basePath))
			//    return;
			//TODO LUCA
			//string filter = string.Concat(customizationNamespace, '*', ResxExtension);
			//foreach (string resx in Directory.GetFiles(basePath, filter))
			//{
			//    string culture = GetCultureFromResourceFile(resx, ControllerSources.GetSafeSerializedNamespace(customizationNamespace));
			//    using (ResXResourceReader reader = new ResXResourceReader(resx))
			//    {
			//        IDictionaryEnumerator en = reader.GetEnumerator();
			//        while (en.MoveNext())
			//            AddLocalizableString(culture, (string)en.Key, (string)en.Value, false);
			//    }
			//}
		}
		//-------------------------------------------------------------------------------
		private static string GetCultureFromResxFile(string resource, string customizationNamespace)
		{
			string prefix = customizationNamespace + "." + ResourceManagerClassName;
			resource = Path.GetFileNameWithoutExtension(resource);
            if (prefix.Length >= resource.Length)
                return "";
            resource = resource.Substring(prefix.Length);

			return resource.TrimStart('.');
		}

		/// <summary>
		/// Adds a localizable string.
		/// </summary>
		/// <param name="culture">The culture identifying the dictionary where to add the string</param>
		/// <param name="name">The name of the string to be added</param>
		/// <param name="value">The value of the string to be added</param>
		/// <param name="onlyIfNotExisting">Treu to add the string only if it does not exist yet, otherwise false.</param>
		/// <remarks>If the string exists and onlyIfNotExisting is false, then the old string value is replaced.</remarks>
		//-------------------------------------------------------------------------------
		// Aggiunge una stringa localizzabile nel dizionario appropriato, che verrà poi 
		// inserito in un file di risorsa (.resources) quando viene compilato l'assembly
		public void AddLocalizableString(string culture, string name, string value, bool onlyIfNotExisting)
		{
			Dictionary dictionary;
			if (!dictionaries.TryGetValue(culture, out dictionary))
			{
				dictionary = new Dictionary();
				dictionaries[culture] = dictionary;
			}

			if (onlyIfNotExisting && dictionary.ContainsKey(name))
				return;

			dictionary[name] = value;

			if (culture.Length == 0)
				return;

			//Cerco la stessa stringa in invariant...
			string val = GetLocalizedStringFromCulture("", name);
			if (val != null)
				return;

			//...se non la trovo la devo aggiungere
			dictionary = null;
			dictionaries.TryGetValue("", out dictionary);

			if (dictionary != null)
				dictionary[name] = value;
		}

		// Cerca in ogni lingua del dizionario la vecchia stringa, ne recupera il valore, e inserisce
		// la nuova stringa con il relativo valore
		//-------------------------------------------------------------------------------
		internal void SubstituteDictionaryKey(string oldName, string newName)
		{
			//per ogni lingua di dizionario...
			foreach (Dictionary item in dictionaries.Values)
			{
				//cerco il value della stringa vecchia, dopodichè rimuovo il vecchio entry e metto il nuovo
				try
				{
					string val = item[oldName];
					item.Remove(oldName);
					item.Add(newName, val);
				}
				catch { }
			}
		}

		//-------------------------------------------------------------------------------
		internal string GetLocalizedStringFromCulture(string culture, string resource)
		{
			Dictionary dictionary;
			if (!dictionaries.TryGetValue(culture, out dictionary))
				return null;

			string value = null;
			dictionary.TryGetValue(resource, out value);
			return value;
		}

		// 
		/// <summary>
		/// Aggiunge una lingua localizzabile nel dizionario 
		/// </summary>
		/// <param name="culture"></param>
		//-------------------------------------------------------------------------------
		public void AddLocalizableLanguage(string culture)
		{
			Dictionary dictionary;
			if (!dictionaries.TryGetValue(culture, out dictionary))
			{
				dictionary = new Dictionary();
				dictionaries[culture] = dictionary;
			}
		}

		/// <summary>
		/// Aggiunge una lingua localizzabile nel dizionario 
		/// </summary>
		/// <param name="culture"></param>
		//-------------------------------------------------------------------------------
		public void RemoveLocalizableLanguage(string culture)
		{
			Dictionary dictionary;
			if (dictionaries.TryGetValue(culture, out dictionary))
				dictionaries.Remove(culture);
		}

		/// <summary>
		/// Removes a string from the given culture or from all culture based on removeFromAll parameter
		/// </summary>
		/// <param name="culture">The culture used to remove the string.</param>
		/// <param name="name">The name of the string to be removed.</param>
		/// <param name="removeFromAll">true if necessary to delete from invariant culture.</param>
		//-------------------------------------------------------------------------------
		public void RemoveLocalizableString(string culture, string name, bool removeFromAll)
		{
			Dictionary dictionary;
			if (!removeFromAll)
			{
				//se non devo rimuovere da tutte le culture, faccio una remove specifica
				if (!dictionaries.TryGetValue(culture, out dictionary))
					return;

				dictionary.Remove(name);
				return;
			}

			//altrimenti faccio un ciclo di remove su tutte le lingue
			foreach (Dictionary dict in dictionaries.Values)
			{
				string val = string.Empty;
				if (!dict.TryGetValue(name, out val))
					continue;

				dict.Remove(name);
			}
		}

		/// <summary>
		/// Ritorna la lista di lingue che la customizzazione già include (non include la invariant culture)
		/// </summary>
		//-----------------------------------------------------------------------------
		public List<CultureInfo> GetLocalizedLanguages()
		{
			List<CultureInfo> languages = new List<CultureInfo>();
			foreach (string language in dictionaries.Keys)
				languages.Add(new CultureInfo(language));

			return languages;
		}

		/// <summary>
		/// Ritorna la lista di lingue che l'utente può scegliere per aggiungere alla customizzazione 
		/// (Non include quelle che sono già presenti e la invariantculture)
		/// </summary>
		//-----------------------------------------------------------------------------
		public List<CultureInfo> GetAvailableLanguages()
		{
			List<CultureInfo> existingLanguages = GetLocalizedLanguages();
			List<CultureInfo> purgedLanguages = new List<CultureInfo>();
			foreach (CultureInfo item in CultureInfo.GetCultures(CultureTypes.AllCultures))
			{
				if (existingLanguages.Contains(item) || item.Name == CultureInfo.InvariantCulture.Name)
					continue;

				purgedLanguages.Add(item);
			}

			return purgedLanguages;
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public List<LocalizableString> GetLocalizableStrings(string culture)
		{
			List<LocalizableString> strings = new List<LocalizableString>();
			string stringId = ResourceManagerClassName + ".";
			Dictionary cultureDictionary = null, invariantDictionary = null;
			if (!dictionaries.TryGetValue("", out invariantDictionary))
				return strings;

			if (culture.IsNullOrEmpty())
			{
				foreach (string key in invariantDictionary.Keys)
				{
					if (key.CompareNoCase(dummyString))
						continue;

					LocalizableString ls = new LocalizableString();
					if (key.StartsWith(stringId))
						ls.Name = key.Substring(stringId.Length);
					else
					{
						ls.Name = key;
						ls.IsControl = true;
					}
					ls.Text = invariantDictionary[key];
					strings.Add(ls);
				}
				return strings;
			}
			else
			{
				if (!dictionaries.TryGetValue(culture, out cultureDictionary))
					return strings;

				//l'invariant contiene tutte la chiavi, uso quello come base
				foreach (string key in invariantDictionary.Keys)
				{
					
					LocalizableString ls = new LocalizableString();
					string specificVal;
					if (key.StartsWith(stringId))
					{
						ls.Name = key.Substring(stringId.Length);
						if (cultureDictionary.TryGetValue(key, out specificVal))
							ls.Text = specificVal;
						else
							ls.Text = invariantDictionary[key];
					}
					else
					{
						ls.Name = key;
						if (cultureDictionary.TryGetValue(key, out specificVal))
							ls.Text = specificVal;
						else
							ls.Text = invariantDictionary[key];
						ls.IsControl = true;

					}
					strings.Add(ls);
				}
			}
			return strings;
		}

		/// <summary>
		/// Annulla tutte le stringhe localizzabili di una data cultura
		/// in modo che non vengano serializzate
		/// </summary>
		//-----------------------------------------------------------------------------
		public void PurgeLocalizableStrings(List<LocalizableString> strings, string culture)
		{
			string stringId = ResourceManagerClassName + ".";
			Dictionary dictionary = null;
			if (dictionaries.TryGetValue(culture, out dictionary))
			{
				//per permettere i foreach e la rimozione
				List<string> clonedList = new List<string>(dictionary.Keys);
				foreach (string key in clonedList)
				{
					if (key.StartsWith(stringId))
					{
						//se la nuova collection di stringhe non contiene la chiave corrente, allora la cancello
						//da tutte le lingue
						Predicate<LocalizableString> pred = new Predicate<LocalizableString>((li) => li.Name.CompareNoCase(key.Substring(stringId.Length)));
						LocalizableString ls = strings.Find(pred);
						RemoveLocalizableString(culture, key, ls == null);
					}
				}
			}
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		internal void AddResourceManager(NewCustomizationInfos customizationInfos)
		{
			AddResourceManager(customizationInfos, customizationInfos.EbDesignerCompilationUnit);
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		internal void AddResourceManager(NewCustomizationInfos customizationInfos, SyntaxTree cu)
		{
			TypeDeclaration resourceClass = new TypeDeclaration();
			resourceClass.Modifiers = Modifiers.Public | Modifiers.Static;
			resourceClass.Name = ResourceManagerClassName;

			NamespaceDeclaration codeNamespace = EasyBuilderSerializer.GetNamespaceDeclaration(cu);

			codeNamespace.Members.Add(resourceClass);

			//VariableDeclarationStatement variableDeclaration = new VariableDeclarationStatement(
			//	new SimpleType(typeof(CustomizationComponentResourceManager).FullName),
			//	ResourceManagerFieldName,
			//	AstFacilities.GetObjectCreationExpression
			//	(
			//		new SimpleType(typeof(CustomizationComponentResourceManager).FullName),
			//		new TypeOfExpression(new SimpleType(ResourceManagerClassName))
			//	));


			VariableInitializer varInitializer = new VariableInitializer(
				ResourceManagerFieldName,
					AstFacilities.GetObjectCreationExpression
				(
					new SimpleType(typeof(CustomizationComponentResourceManager).FullName),
					new TypeOfExpression(new SimpleType(ResourceManagerClassName))
				));
			

			FieldDeclaration resourceManagerField = new FieldDeclaration();
			resourceManagerField.ReturnType = new SimpleType(typeof(CustomizationComponentResourceManager).FullName);
			resourceManagerField.Modifiers = Modifiers.Static | Modifiers.Public;
			resourceManagerField.Variables.Add(varInitializer);
			resourceClass.Members.Add(resourceManagerField);

			foreach (LocalizableString ls in GetLocalizableStrings(""))
				if (!ls.IsControl)
					AddStringProperty(resourceClass, ls.Name);

		
			/*
			 VariableDeclarationStatement^ varStmt = gcnew VariableDeclarationStatement(
		gcnew SimpleType(className),
		varName,
		gcnew PrimitiveExpression(nullptr)
		);

	newCollection->Add(varStmt);
	IdentifierExpression^ variableDeclExpression = gcnew IdentifierExpression(varName);

	//_DMAccounting = new DMAccounting(controller);
	newCollection->Add
	(
		AstFacilities::GetAssignmentStatement(
			variableDeclExpression,
			AstFacilities::GetObjectCreationExpression(
				className
			)
		)
	);
			 */

		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		internal void UpdateResourceManagerStrings(List<LocalizableString> strings, NewCustomizationInfos customizationInfos)
		{
			TypeDeclaration resourceClass = null;
			NamespaceDeclaration nsDecl = EasyBuilderSerializer.GetNamespaceDeclaration(customizationInfos.EbDesignerCompilationUnit);

			foreach (AstNode current in nsDecl.Members)
			{
				TypeDeclaration ctd = current as TypeDeclaration;
				if (ctd == null)
					continue;

				if (ctd.Name == ResourceManagerClassName)
				{
					resourceClass = ctd;
					break;
				}
			}
			if (resourceClass == null)
				return;

			//rimuovo tutte le properties
			var members = resourceClass.Members.ToList();
			PropertyDeclaration prop;
			for (int i = members.Count - 1; i >= 0; i--)
			{
				prop = members[i] as PropertyDeclaration;
				if (prop == null)
					continue;

				resourceClass.Members.Remove(prop);
			}

			PurgeLocalizableStrings(strings, Thread.CurrentThread.CurrentUICulture.Name);

			//aggiungo le property che recuperano le stringhe da risorsa
			foreach (LocalizableString ls in strings)
			{
				if (ls.Name.IsNullOrEmpty() || ls.Text.IsNullOrEmpty())
					continue;

				string stringId = AddStringProperty(resourceClass, ls.Name);

				AddLocalizableString(Thread.CurrentThread.CurrentUICulture.Name, stringId, ls.Text, false);
			}

			OnCodeChanged();
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public void UpdateResourceManagerControls(List<LocalizableString> strings)
		{
			foreach (LocalizableString item in strings)
			{
				if (!item.IsControl)
					continue;

				Dictionary dictionary = null;
				if (!dictionaries.TryGetValue(Thread.CurrentThread.CurrentUICulture.Name, out dictionary))
					continue;

				string value = string.Empty;
				if (!dictionary.TryGetValue(item.Name, out value))
					continue;

				dictionary[item.Name] = item.Text;
			}

			OnCodeChanged();
		}

		//-----------------------------------------------------------------------------
		internal static string AddStringProperty(TypeDeclaration resourceClass, string name)
		{
			string stringId = ResourceManagerClassName + '.' + name;
			PropertyDeclaration prop = new PropertyDeclaration();
			prop.Modifiers = Modifiers.Static | Modifiers.Public;
			prop.Name = name;
			prop.ReturnType = new SimpleType(typeof(string).FullName);
			prop.Getter = new Accessor();
			prop.Getter.Body = new BlockStatement();
			prop.Getter.Body.Statements.Add(new ReturnStatement(
				AstFacilities.GetInvocationExpression(
					new IdentifierExpression(ResourceManagerFieldName),
					"GetString",
					new PrimitiveExpression(stringId)
					)));

			resourceClass.Members.Add(prop);

			return stringId;
		}
	}
}

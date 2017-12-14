using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using Microarea.TaskBuilderNet.Core.XmlPersister;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.Tools.TBLocalizer.CommonUtilities;

namespace Microarea.Tools.TBLocalizer
{
	/// <summary>
	/// Informazioni relative alla solution salvate in locale (relative al PC in cui sono state generate).
	/// </summary>
	//================================================================================
	public class SolutionLocalInfo
	{
		[XmlIgnore]
		private const string extension = "tblinf";
		[XmlIgnore]
		private string infoFile;
		
		private bool								sourceControlEnabled			= false;
		private bool								rapidCreation					= false;
		private bool								countTemporaryAsTranslated		= false;
		private bool								showTranslationProgress			= false;
		private bool								sourceBindingInfoInSourceSafe	= false;
		private string								glossariesFolder				= AllStrings.GLOSSARYDEFAULTFOLDER;
		private ArrayList							hiddenDictionaries				= new ArrayList();	
		private EnvironmentSettings					environmentSettings				= new EnvironmentSettings();

		public bool UseSupportDictionaryWhenAvailable = true;
		public string SupportLanguage;
        bool batchBuild;
        ILogger logger;

		//--------------------------------------------------------------------------------
		[XmlIgnore]
		public AssemblyGenerator.ConfigurationType Configuration { get { return EnvironmentSettings.AssemblyConfiguration; } set { EnvironmentSettings.AssemblyConfiguration = value; } }

		#region PROPERTIES
		//--------------------------------------------------------------------------------
		public bool SourceControlEnabled { get { return sourceControlEnabled; } set { sourceControlEnabled = value; } }

		//--------------------------------------------------------------------------------
		public bool RapidCreation { get { return rapidCreation; } set { rapidCreation = value; } }

		//--------------------------------------------------------------------------------
		public bool CountTemporaryAsTranslated { get { return countTemporaryAsTranslated; } set { countTemporaryAsTranslated = value; } }

		//--------------------------------------------------------------------------------
		public bool SourceBindingInfoInSourceSafe { get { return sourceBindingInfoInSourceSafe; } set { sourceBindingInfoInSourceSafe = value; } }

		//--------------------------------------------------------------------------------
		public bool ShowTranslationProgress { get { return showTranslationProgress; } set { showTranslationProgress = value; } }

		//--------------------------------------------------------------------------------
		public EnvironmentSettings EnvironmentSettings { get { return environmentSettings; } set { environmentSettings = value; } }

		//--------------------------------------------------------------------------------
		public ArrayList HiddenDictionaries { get { return hiddenDictionaries; } set { hiddenDictionaries = value; } }
		
		//--------------------------------------------------------------------------------
		public string GlossariesFolder { get { return glossariesFolder; } set { glossariesFolder = value; GlossaryFunctions.GlossariesFolder =  value;} }

		#endregion

        //--------------------------------------------------------------------------------
		public SolutionLocalInfo() :
            this (false, null)
        {

        }

		//--------------------------------------------------------------------------------
		public SolutionLocalInfo(bool batchBuild, ILogger logger)
		{
            this.logger = logger;
            this.batchBuild = batchBuild;
			GlossaryFunctions.GlossariesFolder =  glossariesFolder;
		}

		//--------------------------------------------------------------------------------
		private static string InfoPathFromSolutionPath(string solutionPath)
		{
			return Path.ChangeExtension(solutionPath, extension);
		}

		//--------------------------------------------------------------------------------
		internal static SolutionLocalInfo Load(bool batchBuild, ILogger logger)
		{
			return Load (null, batchBuild, logger);
		}

		/// <summary>
		/// Carica le informazioni locali relative alla solution
		/// </summary>
		/// <param name="file">il file associato alla solution</param>
		//--------------------------------------------------------------------------------
        internal static SolutionLocalInfo Load(string file, bool batchBuild, ILogger logger)
		{
			SolutionLocalInfo info;
			string infoFile, serializedInfoFile;
			
			if (file == null)
			{
                infoFile = InfoPathFromSolutionPath(AllStrings.AppDataPath);
				serializedInfoFile = infoFile;
			}
			else
			{
				infoFile = InfoPathFromSolutionPath(file);
				serializedInfoFile = infoFile;
				if (!File.Exists(infoFile))
                    infoFile = InfoPathFromSolutionPath(AllStrings.AppDataPath);
			}
	
			try
			{
				if (!File.Exists(infoFile))
				{
					info = new SolutionLocalInfo(batchBuild, logger);
					info.infoFile = serializedInfoFile;
					return info;
				}

				XmlDocument d = new XmlDocument();
				d.Load(infoFile);
				info = SerializerUtility.DeserializeFromXmlNode(d.DocumentElement, typeof(SolutionLocalInfo)) as SolutionLocalInfo;
				info.infoFile = serializedInfoFile;
				return info;
			}
			catch
			{
                info = new SolutionLocalInfo(batchBuild, logger);
				info.infoFile = serializedInfoFile;;
				return info;
			}
		}

		/// <summary>
		/// Salva le informazioni locali relative alla solution
		/// </summary>
		/// <param name="file">il file associato alla solution</param>
		//--------------------------------------------------------------------------------
		internal void Save(string file)
		{
			
			try
			{
				infoFile = InfoPathFromSolutionPath(file);
				
				XmlNode n = SerializerUtility.SerializeToXmlNode(this);
				if (n != null)
				{
					if (File.Exists(infoFile))
						File.SetAttributes(infoFile, FileAttributes.Normal);
					n.OwnerDocument.Save(infoFile);
				}
			}
			catch (Exception ex)
			{
                if (!batchBuild)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);   
                }
				CommonUtilities.Functions.SafeDeleteFile(infoFile);
                if (logger != null)
                {
                    logger.WriteLog(
                        String.Concat(
                            "SolutionLocalInfo.Save(",
                            file,
                            "): error saving infoFile in ",
                            infoFile,
                            ", message: ",
                            ex.ToString()
                            )
                        );
                }

			}
			
		}

		//--------------------------------------------------------------------------------
		public void Save()
		{
			if (infoFile != null && infoFile != string.Empty)
				Save(infoFile);
		}
	}

	//================================================================================
	public class EnvironmentSettings
	{
		[XmlIgnore]
		public static readonly string Key = typeof(EnvironmentSettings).FullName;

		[XmlIgnore]
		private Hashtable ht = new Hashtable();

		//--------------------------------------------------------------------------------
		[XmlIgnore]
		public Hashtable EnvironmentVariables 
		{
			get
			{
				return ht;
			}
		}

		private bool binAsSatelliteAssemblies = true;
		public bool BinAsSatelliteAssemblies
		{
			get { return binAsSatelliteAssemblies; }
			set { binAsSatelliteAssemblies = value; }
		}

		private string installationPath = String.Empty;
		public string InstallationPath
		{
			get { return installationPath; }
			set 
			{
                if (String.Compare(installationPath, value, StringComparison.InvariantCulture) == 0)
                {
                    return;
                }

                ht["$(InstallationPath)"] = value;
                installationPath = value;
                Drive = Path.GetPathRoot(value);
                Installation = Path.GetFileName(value);
				InitReplacer(); 
			}
		}
		private string installation	= "Development";
		public string Installation 
		{ 
			get { return installation; }
			set { installation = value; ht["$(Installation)"] = value; InitReplacer(); }
		}

		private string drive = @"C:";
		public string Drive			
		{ 
			get { return drive; }
			set { drive = value.TrimEnd(Path.DirectorySeparatorChar); ht["$(Drive)"] = drive; InitReplacer(); }
		}
		
		//--------------------------------------------------------------------------------
		[XmlIgnore]
		private Regex logicalToPhysicalReplacer;
		[XmlIgnore]
		private Regex physicalToLogicalReplacer;

		public EnvironmentSettings()
		{
			foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
			{
				ht[string.Format("$({0})", de.Key)] = de.Value;
			}
		}

		//--------------------------------------------------------------------------------
		private void InitReplacer()
		{
			logicalToPhysicalReplacer = InitLogicalToPhysicalRepacer();
			physicalToLogicalReplacer = InitPhysicalLogicalRepacer();
		}
		
		
		//--------------------------------------------------------------------------------
		private Regex InitPhysicalLogicalRepacer()
		{
			//li ordino in modo da avere per primi i path più specifici
			List<string> paths = new List<string>();
			foreach (string k in EnvironmentVariables.Values)
				paths.Add(k);
			paths.Sort(new PathComparer());

			StringBuilder sb = new StringBuilder();
			sb.Append("(");

			foreach (string path in paths)
			{
				if (sb.Length > 1)
					sb.Append("|");
				sb.AppendFormat(@"({0})", Regex.Escape(path));
			}
			sb.Append(")");
			
			string pattern = string.Format(@"(?<=\\|/|^){0}(?=\\|/|$)", sb);

			return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
		}

		//--------------------------------------------------------------------------------
		private Regex InitLogicalToPhysicalRepacer()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("(");

			foreach (string var in EnvironmentVariables.Keys)
			{
				if (sb.Length > 1)
					sb.Append("|");
				sb.AppendFormat(@"({0})", Regex.Escape(var));
			}
			sb.Append(")");

			string pattern = string.Format(@"(?<=\\|/|^){0}(?=\\|/|$)", sb);

			return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
		}

		//--------------------------------------------------------------------------------
		internal string LogicalPathToPhysicalPath(string s)
		{
			if (logicalToPhysicalReplacer == null)
			{
				System.Diagnostics.Debug.Fail("Regular expression not initialized");
				return s;
			}
			return logicalToPhysicalReplacer.Replace(s, new MatchEvaluator(ReplacePhysical));

		}

		//--------------------------------------------------------------------------------
		internal string PhysicalPathToLogicalPath(string s)
		{
			if (physicalToLogicalReplacer == null)
			{
				System.Diagnostics.Debug.Fail("Regular expression not initialized");
				return s;
			}
			return physicalToLogicalReplacer.Replace(s, new MatchEvaluator(ReplaceLogical));

		}

		//--------------------------------------------------------------------------------
		private string ReplacePhysical (Match m)
		{
			return (string) EnvironmentVariables[m.Value];
		}

		//--------------------------------------------------------------------------------
		private string ReplaceLogical (Match m)
		{
			string val = m.Value;
			foreach (DictionaryEntry de in EnvironmentVariables)
				if (string.Compare((string)de.Value, val, true, CultureInfo.InvariantCulture) == 0)
					return de.Key as string;

			return val;
		}

		public string CompareExecutablePath = "";
		public bool BuildDictionary	= true;
		public AssemblyGenerator.ConfigurationType AssemblyConfiguration = AssemblyGenerator.ConfigurationType.CFG_RELEASE;
		public DictionaryDocument.StringComparisonFlags StringComparisonFlags = DictionaryDocument.StringComparisonFlags.IGNORE_ALL;
	}

	//================================================================================
	public class EnvironmentGlobalSettings
	{
		[XmlIgnore]
		static string path = Path.Combine(Application.StartupPath, "GlobalInfo.tbl");
		[XmlIgnore]
		public static EnvironmentGlobalSettings GlobalSettings = GetGlobalSettings();
		
		//--------------------------------------------------------------------------------
		public EnvironmentGlobalSettings()
		{
		
		}

		//--------------------------------------------------------------------------------
		public void Save()
		{
			try
			{
				XmlNode n = SerializerUtility.SerializeToXmlNode(this);
				if (n != null)
				{
					if (File.Exists(path))
						File.SetAttributes(path, FileAttributes.Normal);
					n.OwnerDocument.Save(path);
				}
			}
			catch
			{
				File.Delete(path);
			}			
		}

		//--------------------------------------------------------------------------------
		private static EnvironmentGlobalSettings GetGlobalSettings()
		{
			if (File.Exists(path))
			{
				try
				{
					XmlDocument d = new XmlDocument();
					d.Load(path);
					return SerializerUtility.DeserializeFromXmlNode(d.DocumentElement, typeof(EnvironmentGlobalSettings)) as EnvironmentGlobalSettings;			
				}
				catch
				{
				}
			}

			return new EnvironmentGlobalSettings();
		}
	}

	//--------------------------------------------------------------------------------
	class PathComparer : IComparer<string>
	{
		#region IComparer<string> Members

		public int Compare(string x, string y)
		{
			int ix = CountTokens(x);
			int iy = CountTokens(y);
			return iy.CompareTo(ix);
		}

		//--------------------------------------------------------------------------------
		private int CountTokens(string x)
		{
			int t = 0;
			foreach (char c in x)
				if (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar)
					t++;
			return t;
		}

		#endregion
	}
}

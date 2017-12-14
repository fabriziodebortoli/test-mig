using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml;
using Microsoft.Win32;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	//=========================================================================
	public abstract class RequirementError
	{
		protected readonly	string name = string.Empty;

		//---------------------------------------------------------------------
		public abstract string Message						{ get; }
		public abstract string ShortRequirementDescription	{ get; }

		//---------------------------------------------------------------------
		protected RequirementError(string name)
		{
			this.name = name;
		}
	}

	//=========================================================================
	public class RegistryError : RequirementError
	{
		private readonly RegistryKeyInfo	registryKey;
		private readonly KeyValueInfo		wrongKeyValue;
		public RegistryError(string requirementName, RegistryKeyInfo registryKey, KeyValueInfo wrongKeyValue)
			: base(requirementName)
		{
			this.registryKey	= registryKey;
			this.wrongKeyValue	= wrongKeyValue;
		}

		//---------------------------------------------------------------------
		public override string Message
		{
			get
			{
				if (registryKey == null)
					return string.Format(GenericStrings.MissingRequirement, this.name);

				if (wrongKeyValue != null &&
					string.Compare(wrongKeyValue.Type, KeyType.Version.ToString(), true, CultureInfo.InvariantCulture) == 0)
					return string.Format(GenericStrings.WrongRequirementVersion, this.name, wrongKeyValue.ExpectedValue);

				return string.Format(GenericStrings.RequirementNotMet, this.name, registryKey.ToString());
			}
		}

		//---------------------------------------------------------------------
		public override string ShortRequirementDescription
		{
			get
			{
				if (registryKey.KeyValues == null || registryKey.KeyValues.Count == 0)
					return this.name;
				return string.Concat(this.name, " (", registryKey.ToString(), ")");
			}
		}
	}

	/// <summary>
	/// oggetto che rappresenta il valore atteso di un dato nel registry
	/// </summary>
	//=========================================================================
	public class KeyValueInfo
	{
		private string name				= string.Empty;
		private string type				= "String"; 
		private string expectedValue	= string.Empty;
	
		//---------------------------------------------------------------------
		public string Name			{ get { return name;		} }
		public string Type			{ get { return type;		} }
		public string ExpectedValue	{ get { return expectedValue;	} }

		//---------------------------------------------------------------------
		public KeyValueInfo(string name, string type, string expectedValue)
		{
			this.name			= name;
			this.type			= type;
			this.expectedValue	= expectedValue;
		}

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return string.Concat(name, ": ", expectedValue);
		}
	}

	/// <summary>
	/// Oggetto che rappresenta una chiave di registry attesa
	/// </summary>
	//=========================================================================
	public class RegistryKeyInfo
	{
		private		readonly	string		requirementName;
		private					string		key			= string.Empty;
		internal				ArrayList	KeyValues	= new ArrayList();

		/// <summary>
		/// costruttore
		/// </summary>
		/// <param name="requirementName">nome parlante del prerequisito da controllare</param>
		//---------------------------------------------------------------------
		public RegistryKeyInfo(string requirementName)
		{
			this.requirementName = requirementName;
		}

		/// <summary>
		/// popola i campi dell'oggetto in base al parametro passato
		/// </summary>
		/// <param name="registryKeyElement"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public bool Parse(XmlElement registryKeyElement)
		{
			if (registryKeyElement == null)
			{
				Debug.Assert(false);
				return false;
			}

			this.key = registryKeyElement.GetAttribute("key");

			XmlNodeList keyValueElements = registryKeyElement.GetElementsByTagName("KeyValue");
			if (keyValueElements == null)
			{
				Debug.Assert(false);
				return false;
			}

			foreach (XmlElement keyValueElement in keyValueElements)
			{
				KeyValueInfo rvi = new KeyValueInfo
					(
					keyValueElement.GetAttribute("name"), 
					keyValueElement.GetAttribute("type"),
					keyValueElement.GetAttribute("expectedValue")
					);

				KeyValues.Add(rvi);
			}

			return true;
		}

		/// <summary>
		/// Controlla che i prerequisiti espressi per la chiave di registry siano soddisfatti
		/// </summary>
		/// <param name="reqError">dettaglio dell'eventuale errore riscontrato</param>
		/// <returns>booleano. true se i prerequisiti sono tutti soddisfatti</returns>
		//---------------------------------------------------------------------
		public bool Check(out RequirementError reqError)
		{
			reqError = null;
			
			if (key == string.Empty)
			{
				Debug.Assert(false);
				return true;
			}

			using(RegistryKey k = Registry.LocalMachine.OpenSubKey(key))
			{
				if (k == null)
				{
					reqError = new RegistryError(requirementName, this, null);
					return false;
				}
				
				foreach (KeyValueInfo keyValue in KeyValues)
				{
					//se non abbiamo specificato un valore di versione nell'xml mi accontento della presenza
					if (keyValue.ExpectedValue == string.Empty)
						continue;
					
					//leggo il valore dal registro
					object valFromRegistry = k.GetValue(keyValue.Name);
					if (valFromRegistry == null)
					{
						//nel registry non c'è il valore
						Debug.Assert(false);
						reqError = new RegistryError(requirementName, this, keyValue);
						return false;
					}

					KeyType keyType;
					try
					{
						keyType = (KeyType)Enum.Parse(typeof(KeyType), keyValue.Type, true);
					}
					catch (Exception ex)
					{
						//abbiamo scritto una cazzata nell'xml
						Debug.Fail(ex.Message);
						continue;
					}
					

					switch (keyType)
					{
						case KeyType.Version:
						{
							#region Version

							Version currentVer	= null;
							Version neededVer	= null;
            
							try
							{
								neededVer	= new Version(keyValue.ExpectedValue);
							}
							catch(Exception exc)
							{
								//abbiamo scritto una cazzata nell'xml
								Debug.Fail(exc.Message);
								continue;
							}

							try
							{
								currentVer	= new Version((string)valFromRegistry);
							}
							catch(Exception exc)
							{
								//nel registry c'è qualcosa di sbagliato
								Debug.Fail(exc.Message);
								reqError = new RegistryError(requirementName, this, keyValue);
								return false;
							}

							if (currentVer < neededVer)
							{
								reqError = new RegistryError(requirementName, this, keyValue);
								return false;
							}

							#endregion
							break;
						}

						case KeyType.Int:
						{
							#region Int

							int neededValue = 0;
							try
							{
								neededValue	= Int32.Parse(keyValue.ExpectedValue);
							}
							catch(Exception exc)
							{
								//abbiamo scritto una cazzata nell'xml
								Debug.Fail(exc.Message);
								continue;
							}

							int regValue = 0;
							try
							{
								regValue	= (int)valFromRegistry;
							}
							catch(Exception exc)
							{
								//nel registry c'è qualcosa di sbagliato
								Debug.Fail(exc.Message);
								reqError = new RegistryError(requirementName, this, keyValue);
								return false;
							}
							
							if (regValue < neededValue)
							{
								reqError = new RegistryError(requirementName, this, keyValue);
								return false;
							}

							#endregion
							break;
						}

						case KeyType.String:
						{
							#region String

							string regValue = string.Empty;
							try
							{
								regValue = (string)valFromRegistry;
							}
							catch(Exception exc)
							{
								//nel registry c'è qualcosa di sbagliato
								Debug.Fail(exc.Message);
								reqError = new RegistryError(requirementName, this, keyValue);
								return false;
							}
							
							if (string.Compare(regValue, keyValue.ExpectedValue, true, CultureInfo.InvariantCulture) != 0)
							{
								reqError = new RegistryError(requirementName, this, keyValue);
								return false;
							}

							#endregion
							break;
						}
					}
				}
			}

			return true;
		}

		//---------------------------------------------------------------------
		public override string ToString()
		{
			StringBuilder s = new StringBuilder();
			for (int i = 0; i < KeyValues.Count; i++)
			{
				if (i != 0) s.Append(' ');
				s.Append(KeyValues[i].ToString());
				if (i < KeyValues.Count - 1) s.Append(';');
			}
			return s.ToString();
		}

	}
	/// <summary>
	/// Classe per la gestione dei prerequisiti di un'applicazione
	/// </summary>
	//=========================================================================
	public class RequirementInfo
	{
		#region vars & properties
		
		//---------------------------------------------------------------------
		private string					name					= string.Empty;
		private RequirementLocation[]	requirementLocations	= null;
		private ArrayList				registryKeys			= new ArrayList();
	
		//---------------------------------------------------------------------
		public	string					Name					{ get { return name; } }
		public	RequirementLocation[]	RequirementLocations	{ get { return requirementLocations; } }
		public	ArrayList				RegistryKeys			{ get { return registryKeys; } }
		
		#endregion

		//---------------------------------------------------------------------
		public bool Parse(XmlElement requirementElem)
		{
			if (requirementElem == null)
			{
				Debug.Assert(false);
				return false;
			}

			name		= requirementElem.GetAttribute("name");
			string	tmp	= requirementElem.GetAttribute("location");

			if (tmp.Length == 0)
				return false;

			string[] locationsStr = tmp.Split(',');
			ArrayList locs = new ArrayList(tmp.Length);
			
			Type t = typeof(RequirementLocation);
			foreach (string s in locationsStr)
			{
				RequirementLocation loc;
				try
				{
					loc = (RequirementLocation)Enum.Parse(t, s, true);
					locs.Add(loc);
				}
				catch(Exception exc)
				{
					Debug.Fail(exc.Message);
				}
			}

			requirementLocations = (RequirementLocation[])locs.ToArray(typeof(RequirementLocation));

			XmlNodeList registryKeyList = requirementElem.GetElementsByTagName("RegistryKey");
			foreach (XmlElement registryKeyEl in registryKeyList)
			{
				RegistryKeyInfo registryKey = new RegistryKeyInfo(this.name);
				if (!registryKey.Parse(registryKeyEl))
				{
					Debug.Assert(false);
					continue;
				}

				registryKeys.Add(registryKey);
			}

			return true;
		}

		//---------------------------------------------------------------------
		public bool IsSideRequirement(RequirementLocation location)
		{
			foreach (RequirementLocation loc in requirementLocations)
				if (loc == location)
					return true;

			return false;
		}

		/// <summary>
		/// Verifica che il prerequisito sia soddisfatto
		/// </summary>
		/// <param name="platform">sys op</param>
		/// <param name="error">messaggio di errore formattato</param>
		/// <returns>true se il controllo ha avuto successo</returns>
		//---------------------------------------------------------------------
		public bool Check(out RequirementError error)
		{
			error = null;
			
			foreach (RegistryKeyInfo registryKey in registryKeys)
				if (!registryKey.Check(out error))
					return false;
			
			return true;
		}
	}

	/// <summary>
	/// Classe per la gestione di tutti i prerequisiti delle applicazioni
	/// </summary>
	//=========================================================================
	public class RequirementsCatalog
	{
		/// <summary>
		/// array contenente tutti i prerequisisti
		/// </summary>
		private ArrayList requirements = new ArrayList();

		//---------------------------------------------------------------------
		public bool Init(StringCollection requirementFiles)
		{
			if (requirementFiles == null)
			{
				Debug.Assert(false);
				return false;
			}

			Platform platform = Platform.WinXP2K;

			foreach (string requirementFile in requirementFiles)
			{
				XmlDocument doc = new XmlDocument();
				
				try
				{
					doc.Load(requirementFile);
				}
				catch(Exception exc)
				{
					Debug.Fail(exc.Message);
					return false;
				}

				string xQry = string.Format("Requirements/Platform[@name=\"{0}\"]", platform.ToString());

				XmlElement platEl = doc.SelectSingleNode(xQry) as XmlElement;
				if (platEl == null)
					continue;
						
				XmlNodeList reqInfoList = platEl.SelectNodes("Requirement");
				if (reqInfoList == null)
				{
					Debug.Assert(false);
					return false;
				}

				foreach (XmlElement reqInfoEl in reqInfoList)
				{
					RequirementInfo ri = new RequirementInfo();
					if (ri.Parse(reqInfoEl))
						requirements.Add(ri);
				}
			}

			return true;
		}

		/// <summary>
		/// Verifica che tutti i prerequisiti descritti siano soddisfatti
		/// </summary>
		/// <param name="platform">sys op per il quale effettuare la verifica</param>
		/// <param name="client">a true controlla solo i check del client a false quelli del server</param>
		/// <param name="error">stringa formattata contenente i prerequisiti non soddisfatti</param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public bool CheckAllRequirements(RequirementLocation location, out ArrayList errors)
		{
			bool errorFound = false;
			errors = new ArrayList();

			RequirementError tmpError = null;
			
			foreach (RequirementInfo ri in requirements)
			{
				if (!ri.IsSideRequirement(location))
					continue;	
				
				if (!ri.Check(out tmpError))
				{
					errors.Add(tmpError);
					errorFound = true;
				}
			}

			return !errorFound;
		}

		//---------------------------------------------------------------------
		public bool CheckRequirement(string name, out RequirementError error)
		{
			error = null;

			RequirementInfo ri = GetRequirement(name);

			if (ri == null)
				return true;
			
			return ri.Check(out error);
		}

		//---------------------------------------------------------------------
		private RequirementInfo GetRequirement(string name)
		{
			foreach (RequirementInfo ri in requirements)
				if (string.Compare(ri.Name, name, true, CultureInfo.InvariantCulture) == 0)
					return ri;

			return null;
		}

		//---------------------------------------------------------------------
		public static StringBuilder GetErrorsMessage(ArrayList errors)
		{
			StringBuilder sb = new StringBuilder();
			if (errors == null || errors.Count == 0)
				return sb;

			sb.Append(GenericStrings.RequirementMessage);
			sb.Append(Environment.NewLine);
			sb.Append(Environment.NewLine);

			foreach (RequirementError re in errors)
				sb.Append(re.Message + Environment.NewLine);

			return sb;
		}
	}

	/// <summary>
	/// Enumerativo che indica il tipo di dato espresso da un valore nel registry
	/// </summary>
	//=========================================================================
	public enum KeyType {Version, Int, String}

	/// <summary>
	/// Enumerativo che indica se i prerequisiti devono essere verificati sul server, sul client o su entrambi
	/// </summary>
	//============================================================================
	public enum RequirementLocation
	{
		Client,
		Server
	}

	//============================================================================
	public enum Platform
	{
		WinXP2K
	}
}

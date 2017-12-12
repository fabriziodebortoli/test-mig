using System;
using System.Globalization;

using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Licence.Activation
{

	#region Class ProductCode

	/// <summary>
	/// Incapsula le funzionalitá per trattare i codici dei prodotti.
	/// </summary>
	//=========================================================================
	public class ProductCode : IComparable
	{
		private	bool	isEmbedded;
		private	string	productCodeClass;
		private	string	productCodeProduct;
		private	string	productCodeEdition;
		private	string	productCodeModule;
		private	string	productCodeDatabase;
		private	string	productCodeOperatingSystem;

		private	string	productCodeValue;

		#region Public Properties

		public	string	Class				{ get { return productCodeClass; } }
		public	string	Product				{ get { return productCodeProduct; } }
		public	string	Edition				{ get { return productCodeEdition; } }
		public	string	Module				{ get { return productCodeModule; } }
		public	string	Database			{ get { return productCodeDatabase; } }
		public	string	OperatingSystem		{ get { return productCodeOperatingSystem; } }

		public	string	Value				{ get { return productCodeValue; } }

		public	bool	IsSFTClass			{ get { return productCodeClass.ToUpperInvariant().Equals("SFT"); } }
		public	bool	IsEmbedded			{ get { return isEmbedded; } set { isEmbedded = value; } }
		public	char	DatabaseChar		{ get { return SerialNumber.Encode(SerialNumber.GetDBChar(productCodeDatabase), productCodeOperatingSystem); } }
		public	string	ProductInfoChars	{ get { return String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", productCodeProduct, productCodeEdition.Substring(0,1), DatabaseChar); } }
		#endregion

		#region Constructors

		//---------------------------------------------------------------------
		public ProductCode(string productCode)
		{
			if (productCode == null)
				throw new ArgumentNullException("productCode");
			if (productCode.Length != 16)//18) vedi discorso su productCodeCountry
				throw new ArgumentException("Invalid lenght of \"productCode\".");

			isEmbedded					= false;
			productCodeClass			= productCode.Substring(0, 3).ToUpperInvariant();
			productCodeProduct			= productCode.Substring(3, 2).ToUpperInvariant();
			productCodeEdition			= productCode.Substring(5, 3).ToUpperInvariant();
			productCodeModule			= productCode.Substring(8, 4).ToUpperInvariant();
			productCodeDatabase			= productCode.Substring(12,3).ToUpperInvariant();
			productCodeOperatingSystem	= productCode.Substring(15,1).ToUpperInvariant();

			productCodeValue =	String.Format(
				CultureInfo.InvariantCulture,
				"{0}{1}{2}{3}{4}{5}",
				productCodeClass,	//{0}
				productCodeProduct,	//{1}
				productCodeEdition,	//{2}
				productCodeModule,	//{3}
				productCodeDatabase,//{4}
				productCodeOperatingSystem	//{5}
				);
		}

		//---------------------------------------------------------------------
		public ProductCode	(
								string codeClass,
								string codeProduct,
								string codeEdition,
								string codeModule,
								string codeDatabase,
								string codeOperatingSystem
							)
			:
			this(codeClass, codeProduct, codeEdition, codeModule, codeDatabase, codeOperatingSystem, false)
		{}

		//---------------------------------------------------------------------
		public ProductCode	(
								string codeClass,
								string codeProduct,
								string codeEdition,
								string codeModule,
								string codeDatabase,
								string codeOperatingSystem,
								bool isEmb
							)
		{
			if (codeClass.Length != 3)
				throw new ArgumentException("Invalid lenght of " + codeClass);
			productCodeClass = codeClass.ToUpperInvariant();

			if (codeProduct.Length != 2)
				throw new ArgumentException("Invalid lenght of " + codeProduct);
			productCodeProduct = codeProduct.ToUpperInvariant();

			if (codeEdition.Length != 3)
				throw new ArgumentException("Invalid lenght of " + codeEdition);
			productCodeEdition = codeEdition.ToUpperInvariant();

			if (codeModule.Length != 4)
				throw new ArgumentException("Invalid lenght of " + codeModule);
			productCodeModule = codeModule.ToUpperInvariant();

			if (codeDatabase.Length != 3)
				throw new ArgumentException("Invalid lenght of " + codeDatabase);
			productCodeDatabase = codeDatabase.ToUpperInvariant();

			if (codeOperatingSystem.Length != 1)
				throw new ArgumentException("Invalid lenght of " + codeOperatingSystem);
			productCodeOperatingSystem = codeOperatingSystem.ToUpperInvariant();

			isEmbedded = isEmb;

			productCodeValue =	String.Format(
				CultureInfo.InvariantCulture,
				"{0}{1}{2}{3}{4}{5}",
				productCodeClass,	//{0}
				productCodeProduct,	//{1}
				productCodeEdition,	//{2}
				productCodeModule,	//{3}
				productCodeDatabase,//{4}
				productCodeOperatingSystem	//{5}
				);
		}

		#endregion

		#region Public Methods

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return this.Value;
		}

		#region IComparable Members

		//---------------------------------------------------------------------
		public int CompareTo(object obj)
		{
			ProductCode aProductCode = obj as ProductCode;

			if (aProductCode == null)
				throw new ArgumentException("obj is not of ProductCode type");

			return String.Compare(this.Value, aProductCode.Value, false, CultureInfo.InvariantCulture);
		}

		#endregion

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			ProductCode aProductCode = obj as ProductCode;

			return (Object.ReferenceEquals(null, aProductCode)) ?
				false :
				(String.Compare(this.Value, aProductCode.Value, true, CultureInfo.InvariantCulture) == 0);
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		//----------------------- Overloading degli operatori -----------------
		public static bool operator == (ProductCode pc1, ProductCode pc2) 
		{
			if (Object.ReferenceEquals(null, pc1) &&  Object.ReferenceEquals(null, pc2))
				return true;
			if (Object.ReferenceEquals(null, pc1) ||  Object.ReferenceEquals(null, pc2))
				return false;
			return pc1.Equals(pc2);
		}

		//---------------------------------------------------------------------
		public static bool operator != (ProductCode pc1, ProductCode pc2) 
		{
			if (Object.ReferenceEquals(null, pc1) &&  Object.ReferenceEquals(null, pc2))
				return false;
			if (Object.ReferenceEquals(null, pc1) ||  Object.ReferenceEquals(null, pc2))
				return true;
			return !pc1.Equals(pc2);
		}

		//---------------------------------------------------------------------
		public static string GetProductInfoChar (string productCode)	
		{
			ProductCode pc = new ProductCode(productCode);
			return pc.ProductInfoChars;
		}

		//---------------------------------------------------------------------
		public static char GetDatabaseChar (string freeChars)	
		{
			return freeChars[freeChars.Length-1];
		}

//		// Commentato in seguito alla gestione di NDB, in attesa di verifica
//		//---------------------------------------------------------------------
//		public static char GetDatabaseChar (DatabaseLayer.DatabaseVersion db)	
//		{
//			return SerialNumber.Encode
//				(
//				SerialNumber.GetStringFromDatabaseVersion(db), 
//				SerialNumber.GetStringFromOperatingSystem(Microarea.Library.Activation.OperatingSystem.Windows)
//				);
//		}

		//---------------------------------------------------------------------
		public static DatabaseVersion GetDatabaseVersion (char db)	
		{
			return SerialNumber.GetDatabaseVersionFromString(SerialNumber.Decode(db)[0].ToString());
		}

		//---------------------------------------------------------------------
		public static int GetCalNumber (string productCode)	
		{
			ProductCode pc = new ProductCode(productCode);
			return SerialNumber.GetCalNumber(pc.Module);
		}

		#endregion
	}

	#endregion
























	#region Class ProductCodeFormatException

	/// <summary>
	/// Esprime il fatto che il formato del product code non é corretto.
	/// </summary>
	//=========================================================================
	public class ProductCodeFormatException : Exception
	{
		//---------------------------------------------------------------------
		public ProductCodeFormatException()
		{}

		//---------------------------------------------------------------------
		public ProductCodeFormatException(string errorMessage)
			: base(errorMessage)
		{}
	}

	#endregion
}

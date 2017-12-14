using System;
using System.Globalization;
//
using Microarea.Library.XmlPersister;

namespace Microarea.Library.CommonDeploymentFunctions.States
{
	/// <summary>
	/// Un'istanza di questa classe rappresenta una fotografia
	/// dello stato dell'installazione.
	/// Tutti i suoi membri sono public r/w perché così facendo si possono
	/// serializzare in formato XML utilizzando XmlSerializer.
	/// </summary>
	[Serializable]
	public class InstalledProductState : State, IComparable
	{
		public string	StorageName		= string.Empty;
		public string	BrandedProduct	= string.Empty;
		public ProductType Type;

		public ImageInfo DownloadImage	= new ImageInfo();
		public ImageInfo RunningImage	= new ImageInfo();

		#region IComparable Members

		public int CompareTo(object obj)
		{
			InstalledProductState aIps = obj as InstalledProductState;
			if (aIps == null)
				return 1; // This instance is greater than obj
			if (this.Type == ProductType.Master && aIps.Type != ProductType.Master)
				return -1; // Master product should always come first
			if (this.Type != ProductType.Master && aIps.Type == ProductType.Master)
				return 1; // Master product should always come first
			string aIpsName = aIps.BrandedProduct != null && aIps.BrandedProduct.Length != 0 ? aIps.BrandedProduct : aIps.StorageName;
			string thisName = this.BrandedProduct != null && this.BrandedProduct.Length != 0 ? this.BrandedProduct : this.StorageName;
			return string.Compare(thisName, aIpsName, false, CultureInfo.CurrentCulture); // used for UI Device
		}

		#endregion

		public override string ToString()
		{
			if (this.BrandedProduct == null || this.BrandedProduct.Length == 0)
				return this.StorageName;
			return BrandedProduct;
		}

	}

	//=========================================================================
	[Serializable]
	public class ImageInfo
	{
		public string	CurrentRelease			= string.Empty;
		public string	CurrentReleaseExtended	= string.Empty;
		public string	PreviousRelease			= string.Empty;
		public string	PreviousReleaseExtended	= string.Empty;
		public DateTime	CurrentLastUpdate	= DateTime.MinValue;
		public DateTime	PreviousLastUpdate	= DateTime.MinValue;
	}

	//=========================================================================
	public enum ProductType
	{
		Unknown = 0, // for backward compatibility
		Master,
		AddOn
	}
}
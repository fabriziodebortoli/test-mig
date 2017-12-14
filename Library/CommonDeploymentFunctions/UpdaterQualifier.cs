using System;
using System.Globalization;

namespace Microarea.Library.CommonDeploymentFunctions
{
	//=========================================================================
	[Serializable]
	public struct UpdaterQualifier
	{
		public readonly string Installation;
		public readonly string Product;
		private readonly string brandedProduct;

		//---------------------------------------------------------------------
		public UpdaterQualifier(string installationName, string productName) : this (installationName, productName, string.Empty){}
		public UpdaterQualifier(string installationName, string productName, string brandedProduct)
		{
			this.Installation	= installationName;
			this.Product		= productName;
			this.brandedProduct	= brandedProduct;
		}

		//---------------------------------------------------------------------
		public string BrandedProduct
		{
			get
			{
				if (brandedProduct == null || brandedProduct.Length == 0)
					return Product;
				return brandedProduct;
			}
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			UpdaterQualifier uk = (UpdaterQualifier)obj;
			bool equals =
				(string.Compare(this.Installation, uk.Installation, true, CultureInfo.InvariantCulture) == 0) && 
				(string.Compare(this.Product, uk.Product, true, CultureInfo.InvariantCulture) == 0);
			return equals;
		}

		//---------------------------------------------------------------------
		public override int GetHashCode() 
		{
			string compound = this.Installation + "," + this.Product;
			return compound.ToLower(CultureInfo.InvariantCulture).GetHashCode();
		}

		//---------------------------------------------------------------------
		public override string ToString() 
		{
			return String.Concat(this.Installation, "-", this.Product);
		}
	}
}

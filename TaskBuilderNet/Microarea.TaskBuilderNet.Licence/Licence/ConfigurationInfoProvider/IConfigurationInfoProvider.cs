using System.Collections;
using System.Xml;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Licence.Activation;

namespace Microarea.TaskBuilderNet.Licence.Licence.ConfigurationInfoProvider
{
	/// <summary>
	/// Summary description for IConfigurationInfoProvider.
	/// </summary>
	public interface IConfigurationInfoProvider
	{
		bool AddFunctional { get; set; }

		/// <summary>
		/// All managed product names, included the ones not licensed, and, in case,
		/// also the one being installed. Also zombie Licensed.config names are included.
		/// </summary>
		/// <remarks>
		/// ActivationObject constructor uses it to enumerate all the product and build the
		/// related ProductInfo.
		/// </remarks>
		/// <returns></returns>
		string[] GetProductNames();
		
		/// <summary>
		/// Product element of the product Licensed.config
		/// </summary>
		/// <remarks>
		/// The product Element is not the root element of the Licensed.config file.
		/// ActivationObject uses it to build the ProductInfo
		/// </remarks>
		/// <param name="product"></param>
		/// <returns></returns>
		XmlElement GetProductLicensed(string product);
		
		/// <summary>
		/// Collection of the root elements of the article description files of a product.
		/// </summary>
		/// <remarks>
		/// ActivationObject uses them to build the ArticleInfo
		/// </remarks>
		/// <param name="product"></param>
		/// <returns>The required elements. If no solution is available, null.</returns>
        Hashtable GetArticles(string product, Hashtable articlesDom); //Hashtable<XmlDocument>

		/// <summary>
		/// Country of the licensee. In case of MasterMaker provider, TODO
		/// </summary>
		/// <returns></returns>
		string GetCountry();

		/// <summary>
		/// States whether TODO
		/// </summary>
		bool ArticlesLicensedByDefault { get; }

		/// <summary>
		/// Given a product name, return the DOM of the related solution description.
		/// </summary>
		/// <remarks>
		/// ActivationObject uses them to build the ArticleInfo and matches its entries
		/// with the ones retrieved with GetProduct()
		/// </remarks>
		/// <param name="product"></param>
		/// <returns>The product solution document element if available, null otherwise.</returns>
		XmlElement GetProductSolution(string product);

		/// <summary>
		///torna il nome della cartella application che contiene la solution
		///</summary>
		string GetApplication(string product);

		/// <summary>
		/// UserInfo of the licensee. In case of MasterMaker provider, TODO
		/// </summary>
		/// <returns></returns>
		UserInfo GetUserInfo();

		void InvalidateCaches();

		// used for registration
		IBasePathFinder GetPathFinder();

		bool FilterByCountry { get; }

	}
}

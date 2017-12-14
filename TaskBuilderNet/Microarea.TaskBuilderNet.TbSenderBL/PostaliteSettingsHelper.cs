using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Reflection;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.TaskBuilderNet.TbSenderBL
{
	//===================================================================================
	//public class PostaliteSettingsHelper
	//{
	//    private static PostaliteSettingsList postaliteSettingsList;

	//    /// <summary>
	//    /// Ritorna la list di parameters per azienda per il tbsender
	//    /// </summary>
	//    //-------------------------------------------------------------------------------
	//    public static PostaliteSettingsList PostaliteSettingsList
	//    {
	//        get 
	//        {
	//            if (postaliteSettingsList != null)
	//                return postaliteSettingsList;

	//            return Load();
	//        }
	//    }

	//    /// <summary>
	//    /// Carica i parametri per azienda del tbsender: ne crea di default se non sono mai stati impostati
	//    /// oppure ritorna quelli scelti dall'utente;
	//    /// </summary>
	//    //-------------------------------------------------------------------------------
	//    public static PostaliteSettingsList Load()
	//    {
	//        IDateTimeProvider timeProvider = new DateTimeProvider();
	//        List<string> companies = LoginManagerConnector.GetSubscribedCompaniesDescriptors();

	//        //non c'è niente, creo i default
	//        postaliteSettingsList = new PostaliteSettingsList();

	//        foreach (string current in companies)
	//            postaliteSettingsList.Add(new PostaliteSettings(new PathFinder(current, "AllUsers"), timeProvider));

	//        return postaliteSettingsList;
	//    }
	//}

	public interface IPostaLiteSettingsProvider
	{
		PostaliteSettings GetSettings(string company);
		void Refresh();
		IDateTimeProvider TimeProvider { get; }
	}
	public class PostaLiteSettingsProvider : IPostaLiteSettingsProvider
	{
		private PostaliteSettingsList companySettings;
		public IDateTimeProvider TimeProvider { get; private set; }

		//---------------------------------------------------------------------
		public PostaLiteSettingsProvider()
		{
			this.TimeProvider = new DateTimeProvider(); // TODO use IoC
			this.companySettings = Load();
		}

		//---------------------------------------------------------------------
		private PostaliteSettingsList Load()
		{
			List<string> companies = LoginManagerConnector.GetSubscribedCompaniesDescriptors();

			//non c'è niente, creo i default
			PostaliteSettingsList postaliteSettingsList = new PostaliteSettingsList();
			foreach (string current in companies)
				postaliteSettingsList.Add(new PostaliteSettings(new PathFinder(current, "AllUsers"), this.TimeProvider));
			return postaliteSettingsList;
		}

		//---------------------------------------------------------------------
		public void Refresh()
		{
			lock (this)
				this.companySettings = Load();
		}

		//---------------------------------------------------------------------
		public PostaliteSettings GetSettings(string company)
		{
			return companySettings[company];
		}
	}
}

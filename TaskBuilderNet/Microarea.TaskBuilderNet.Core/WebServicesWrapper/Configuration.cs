using System;
using System.Collections.Generic;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.WebServicesWrapper
{
	//=========================================================================
	public class Configuration
	{
		#region Recognized properties

		public const string SoapServerUrl = "SoapServerUrl";
		public const string AuthenticationServiceUrl = "AuthenticationServiceUrl";
		public const string Domain = "Domain";
		public const string Username = "Username";
		public const string Password = "Password";
		public const string Installation = "Installation";
		public const string CompanyName = "CompanyName";
		public const string ProducerKey = "ProducerKey";
		public const string IntegratedWindowsAuthentication = "IntegratedWindowsAuthentication";
		public const string ConnectionTimeout = "ConnectionTimeout";

		#endregion

		private static ISessionFactory sessionFactory;
		private static readonly object lockTicket = new object();

		private IDictionary<string, string> properties = new Dictionary<string, string>();

		//---------------------------------------------------------------------
		public string this[string propertyName]
		{
			get { return GetPropertyValue(propertyName); }
			set { SetPropertyValue(propertyName, value); }
		}

		//---------------------------------------------------------------------
		public void AddProperties(IDictionary<string, string> properties)
		{
			if (properties == null || properties.Keys.Count == 0)
				return;

			foreach (KeyValuePair<string, string> pair in properties)
				SetPropertyValue(pair.Key, pair.Value);
		}

		//---------------------------------------------------------------------
		public string GetPropertyValue(string propertyName)
		{
			return properties[propertyName];
		}

		//---------------------------------------------------------------------
		public Configuration SetPropertyValue(string propertyName, string propertyValue)
		{
			if (propertyName == null)
				throw new ArgumentNullException("propertyName");

			if (propertyName.Trim().Length == 0)
				throw new ArgumentException(" empty property name");

			if (properties.ContainsKey(propertyName))
				properties[propertyName] = propertyValue;
			else
				properties.Add(propertyName, propertyValue);

			return this;
		}

		//---------------------------------------------------------------------
		public void RemoveProperty(string propertyName)
		{
			if (
				propertyName == null ||
				propertyName.Trim().Length == 0 ||
				!properties.ContainsKey(propertyName)
				)
				return;

			properties.Remove(propertyName);
		}

		//---------------------------------------------------------------------
		public ISessionFactory BuildSessionFactory()
		{
			lock (lockTicket)
			{
				if (sessionFactory == null)
					sessionFactory = new SessionFactory(this);

				return sessionFactory;
			}
		}
	}
}

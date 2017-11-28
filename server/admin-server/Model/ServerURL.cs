using System;
using System.Data;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using System.Collections.Generic;

namespace Microarea.AdminServer.Model
{
	//================================================================================
	public class ServerURL : IServerURL, IModelObject
	{
		string instanceKey;
		URLType urlType = URLType.API;
		string url;

		//---------------------------------------------------------------------
		public string InstanceKey { get { return this.instanceKey; } set { this.instanceKey = value; } }
		public URLType URLType { get { return this.urlType; } set { this.urlType = value; } }
		public string URL { get { return this.url; } set { this.url = value; } }

		//---------------------------------------------------------------------
		public ServerURL()
		{
			this.instanceKey = String.Empty;
			this.urlType = new URLType();
			this.url = String.Empty;
		}

		//---------------------------------------------------------------------
		public OperationResult Save(BurgerData burgerData)
		{
			OperationResult opRes = new OperationResult();

			List<BurgerDataParameter> burgerDataParameters = new List<BurgerDataParameter>();
			burgerDataParameters.Add(new BurgerDataParameter("@InstanceKey", this.instanceKey));
			burgerDataParameters.Add(new BurgerDataParameter("@URLType", (int)this.urlType));
			burgerDataParameters.Add(new BurgerDataParameter("@URL", this.url));

			BurgerDataParameter instanceKeyColumnParameter = new BurgerDataParameter("@InstanceKey", this.instanceKey);
			BurgerDataParameter urlTypeColumnParameter = new BurgerDataParameter("@URLType", this.urlType);
			BurgerDataParameter[] columnParameters = new BurgerDataParameter[] {
				instanceKeyColumnParameter,
				urlTypeColumnParameter
			};

			opRes.Result = burgerData.Save(ModelTables.ServerURLs, columnParameters, burgerDataParameters);
			return opRes;
		}

		//---------------------------------------------------------------------
		public IModelObject Fetch(IDataReader reader)
		{
			ServerURL serverUrl = new ServerURL();
			serverUrl.instanceKey = reader["InstanceKey"] as string;
			serverUrl.urlType = this.GetURLType((int)reader["UrlType"]);
			serverUrl.url = reader["URL"] as string;
			return serverUrl;
		}

		//---------------------------------------------------------------------
		private URLType GetURLType(int val)
		{
			switch (val)
			{
				case 0:
					return URLType.API;
				case 1:
					return URLType.APP;
				case 2:
					return URLType.TBLOADER;
				default:
					return URLType.APP;
			}
		}

		//---------------------------------------------------------------------
		public string GetKey()
		{
			throw new NotImplementedException();
		}

		//----------------------------------------------------------------------
		public OperationResult Delete(BurgerData burgerData)
		{
			throw new NotImplementedException();
		}
	}
}

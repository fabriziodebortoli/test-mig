using System;
using System.Diagnostics;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.ParametersManager;

namespace Microarea.EasyBuilder.BackendCommunication
{
	/// <summary>
	/// SNGeneratorWrapper.
	/// </summary>
	//=========================================================================
	internal class SNGeneratorWrapper : BaseWsWrapper, IDisposable
	{
		private SNGeneratorRef.SNGenerator30 snGenerator;
        private IParametersManagerFactory pmFactory = new ParametersManagerFactory();

		//---------------------------------------------------------------------
		public SNGeneratorWrapper
			(
			ProxySettings	proxySettings,
			string			aSNGeneratorUrl
			) : base(proxySettings)
		{
			string snGeneratorUrl = (aSNGeneratorUrl != null && aSNGeneratorUrl.Length > 0) ? aSNGeneratorUrl.Trim() : String.Empty;
			try
			{
				if (snGeneratorUrl == null || snGeneratorUrl.Length == 0)
				{
					string cfgSNGeneratorUrl = Settings.Default.Microarea_EasyBuilder_SNGeneratorRef_SNGenerator30;
					if (cfgSNGeneratorUrl != null && cfgSNGeneratorUrl.Trim().Length > 0)
						snGeneratorUrl = cfgSNGeneratorUrl.Trim();
				}
			}
			catch(Exception exception)
			{
				Debug.Fail("SNGeneratorWrapper constructor error: " + exception.Message);
			}
		
			if (snGeneratorUrl == null || snGeneratorUrl.Length == 0)
			{
				snGeneratorUrl = @"http://www.microarea.it/SNGenerator/SNGenerator.asmx";;
			}
			snGenerator = new SNGeneratorRef.SNGenerator30();
			snGenerator.Url = snGeneratorUrl;
		}

		//---------------------------------------------------------------------
		public string SNGeneratorUrl
		{
			get { return (snGenerator != null) ? snGenerator.Url : String.Empty; }
		}
		
		//---------------------------------------------------------------------
		public string[] GetSerialNumbers(
			string login,
			string password,
			string solutionFileName,
			string dummyChars,
			string country,
			int orderedQty
			) 
		{
            SerialNumberBag snb = new SerialNumberBag();
            snb.Login = login;
            snb.Password = password;
            snb.SolutionFileName = solutionFileName;
            snb.DummyChars = dummyChars;
            snb.Country = country;
            snb.OrderedQty = orderedQty;

            string temp = null;
            snb.GetXmlString(out temp);

            IParametersManager pm = pmFactory.GetParametersManager(SerialNumberBag.ProtocolVersion);

            string sessionGuid = Guid.NewGuid().ToString();
            string sessionToken = snb.Country;
            string originalSessionGuid = sessionGuid;
            string originalSessionToken = sessionToken;

            string toBeSent = pm.SetParameter(true, temp, sessionGuid, sessionToken);

            snGenerator.GetSerialNumbers(ref toBeSent, ref sessionGuid, ref sessionToken);

            temp = pm.GetParameter(false, toBeSent, originalSessionGuid, originalSessionToken);
            snb = (SerialNumberBag)SerialNumberBag.GetFromXmlString(temp, typeof(SerialNumberBag));

			return snb.GeneratedSerialNumbers;
		}

		#region IDisposable Members

		//---------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//---------------------------------------------------------------------
		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (snGenerator != null)
				{
					snGenerator.Dispose();
					snGenerator = null;
				}
			}
		}

		#endregion
	}
}

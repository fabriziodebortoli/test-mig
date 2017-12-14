using System;
using System.Collections;
using System.Globalization;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.Licence.Activation
{
	//=========================================================================
	public class ReturnValuesManager
	{
		private ErrorInfo[]			errors		= null;
		private ActivationKeyInfo[]	keyInfos	= null;
		private UserId[]			userIdInfos	= null;
		private bool showFormForMluChargeChoice = false;

		public bool ShowFormForMluChargeChoice  {get {return showFormForMluChargeChoice;} set {showFormForMluChargeChoice = value;}}
		public ErrorInfo[]			Errors		{get {return (errors == null)?		new ErrorInfo[]{}			: errors;}		set {errors		 = value;}}
		public ActivationKeyInfo[]	KeyInfos	{get {return (keyInfos == null)?	new ActivationKeyInfo[]{}	: keyInfos;}	set {keyInfos	 = value;}}
		public UserId[]				UserIdInfos	{get {return (userIdInfos == null)? new UserId[]{}				: userIdInfos;}	set {userIdInfos = value;}}

		//---------------------------------------------------------------------
		public int GetCodeFromString(string error)
		{
			//se errore nullo  ritorno OK
			if (error == null)
				return GetCodeFromString(ErrorMessage.Ok);

			foreach (char c in error)
				if (!char.IsDigit(c))
					return GetCodeFromString(ErrorMessage.ErrorNotValid);//rischio loop
			try
			{
				return Int32.Parse(error, CultureInfo.InvariantCulture);
			}
			catch (Exception)
			{
				return  GetCodeFromString(ErrorMessage.ErrorNotValid);//rischio loop
			}
		}

		//---------------------------------------------------------------------
		public void AddError(string errorString, string details)
		{
			AddError(errorString, details, 0);
		}

		//---------------------------------------------------------------------
		public void AddError(string errorString, string details, int id)
		{
			ErrorInfo error	 = new ErrorInfo(GetCodeFromString(errorString), id, details);
			ArrayList list = new ArrayList();
			list.AddRange(Errors);
			list.Add(error);
			Errors = (ErrorInfo[])list.ToArray(typeof(ErrorInfo));
		}
	}

	//=========================================================================
	public class ReturnValuesWriter : ReturnValuesManager
	{
		XmlDocument workDoc = null;

		//---------------------------------------------------------------------
		public static string GetErrorString(string stringCode)
		{			
			return GetErrorString(stringCode, null);
		}

		//---------------------------------------------------------------------
		public static string GetErrorString(string stringCode, string details)
		{			
			ReturnValuesWriter w = new ReturnValuesWriter();
			
			w.InitWorkDoc();
			w.WriteError(stringCode, details);
			return w.workDoc.OuterXml;
		}
		//---------------------------------------------------------------------
		public string GetXml()
		{			
			InitWorkDoc();
			WriteErrors();
			WriteKey();
			WriteUserId();
			WriteShowFormForMluChargeChoice();
			return workDoc.OuterXml;
		}

		//---------------------------------------------------------------------
		public void InitWorkDoc()
		{
			workDoc = new XmlDocument();
			XmlElement root = workDoc.CreateElement(WceStrings.Element.ReturnValues);
			workDoc.AppendChild(root);
		}

		//---------------------------------------------------------------------
		public void AddKeyInfo(ActivationKeyInfo keyInfo)
		{			
			ArrayList list = new ArrayList();
			list.AddRange(KeyInfos);
			list.Add(keyInfo);
			KeyInfos = (ActivationKeyInfo[])list.ToArray(typeof(ActivationKeyInfo));
		}

		//---------------------------------------------------------------------
		public void AddUserIdInfo(UserId userId)
		{			
			if (userId == null) return;
			ArrayList list = new ArrayList();
			list.AddRange(UserIdInfos);
			list.Add(userId);
			UserIdInfos = (UserId[])list.ToArray(typeof(UserId));
		}

		//---------------------------------------------------------------------
		public void WriteError(string error, string detail)
		{
			if (error == null || error.Length < 1)
				return; 
			XmlElement errors = workDoc.CreateElement(WceStrings.Element.Errors);
			workDoc.DocumentElement.AppendChild(errors);
			XmlElement n = workDoc.CreateElement(WceStrings.Element.Error);
			n.SetAttribute(WceStrings.Attribute.Value, error);
			if (detail != null && detail.Length > 0)
				n.InnerText = detail;
			errors.AppendChild(n);
		}

		//---------------------------------------------------------------------
		public static void WriteErrors(ErrorInfo[] Errors, XmlDocument workDoc)
		{
			XmlElement errors = workDoc.CreateElement(WceStrings.Element.Errors);
			workDoc.DocumentElement.AppendChild(errors);
			foreach (ErrorInfo ei in Errors)
			{
				if (ei == null) continue; 
				XmlElement n = workDoc.CreateElement(WceStrings.Element.Error);
				n.SetAttribute(WceStrings.Attribute.Value, ei.Code.ToString(CultureInfo.InvariantCulture));
				if (ei.Id >= 0)
					n.SetAttribute(WceStrings.Attribute.Id, ei.Id.ToString(CultureInfo.InvariantCulture));
				n.InnerText = ei.Details;
				errors.AppendChild(n);
			}
		}
		//---------------------------------------------------------------------
		public void WriteErrors()
		{
			if (Errors == null || Errors.Length < 1)
				return; 
		   WriteErrors(Errors, workDoc);
			
		}
		//---------------------------------------------------------------------
		public string GetErrorsXml(ErrorInfo[] errors)
		{
			if (errors == null || errors.Length < 1)
				return null; 
			InitWorkDoc();
			WriteErrors(errors, workDoc);
			return workDoc.OuterXml;
			
		}
		

		//---------------------------------------------------------------------
		public void WriteKey()
		{
			if (KeyInfos == null ||KeyInfos.Length <1)
				return;
			foreach (ActivationKeyInfo aki in KeyInfos)
			{
				XmlElement n = workDoc.CreateElement(WceStrings.Element.ActivationKey);
				n.SetAttribute(WceStrings.Attribute.Key, aki.Key);
				if (aki.InternalCode != null)
					n.SetAttribute(WceStrings.Attribute.InternalCode, aki.InternalCode);
				if (aki.Producer != null)
					n.SetAttribute(WceStrings.Attribute.Producer, aki.Producer);
				workDoc.DocumentElement.AppendChild(n);
			}
		}

		
		//---------------------------------------------------------------------
		public void WriteUserId()
		{
			if (UserIdInfos == null || KeyInfos.Length < 1)
				return;
			foreach (UserId ui in UserIdInfos)
			{
				if (ui == null) continue; 
				XmlElement n = workDoc.CreateElement(WceStrings.Element.UserId);
				n.SetAttribute(WceStrings.Attribute.Value, ui.Value);
				if(ui.InternalCode != null)
					n.SetAttribute(WceStrings.Attribute.InternalCode, ui.InternalCode);
				if(ui.Producer != null)
					n.SetAttribute(WceStrings.Attribute.Producer, ui.Producer);
				workDoc.DocumentElement.AppendChild(n);
			}
		}

		//---------------------------------------------------------------------
		public void WriteShowFormForMluChargeChoice()
		{
			XmlElement n = workDoc.CreateElement(WceStrings.Element.ShowFormForMluChargeChoice);
			n.SetAttribute(WceStrings.Attribute.Value, ShowFormForMluChargeChoice ? Boolean.TrueString: Boolean.FalseString);
			workDoc.DocumentElement.AppendChild(n);
		}

		
	}


	/// <summary>
	///Parsa la risposta del WebService che genera la chiave di attivazione.
	/// </summary>
	//=========================================================================
	public class ReturnValuesReader : ReturnValuesManager
	{
		private string sourcestring = null;
		//---------------------------------------------------------------------
		public ReturnValuesReader(string xmlString)
		{
			sourcestring = xmlString;
				
			ParseXml(xmlString);	
		}

		//---------------------------------------------------------------------
		public ReturnValuesReader(Parameters parameters)
		{
			if (parameters.HasErrors)
				sourcestring = parameters.Error;
			else
				sourcestring = parameters.ActivationKey;
			ParseXml(sourcestring);	
		}

		//---------------------------------------------------------------------
		private void ParseXml(string xmlString)
		{
			if (xmlString == null || xmlString.Length == 0)
				xmlString = ReturnValuesWriter.GetErrorString(ErrorMessage.ResponseEmpty);

			if (xmlString == ErrorMessage.Ok)
				xmlString = "<ReturnValues />";

			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(xmlString);
			}
			catch (Exception)
			{
				XmlElement el =  doc.CreateElement(WceStrings.Element.Error);
				el.SetAttribute(WceStrings.Attribute.Value, ErrorMessage.ResponseXmlNotValid);//messaggio di errore per xml non formattato.
				doc.AppendChild(el);
			}
			
			XmlNodeList errorNodes	= doc.SelectNodes("//" + WceStrings.Element.Error);
			LoadErrors(errorNodes);
			XmlNodeList userIdNodes = doc.SelectNodes("//" + WceStrings.Element.UserId);
			LoadUserId(userIdNodes);
			XmlNodeList keyNodes	= doc.SelectNodes("//" + WceStrings.Element.ActivationKey);
			LoadKeys(keyNodes);
			XmlNode ShowFormForMluChargeChoiceNode		= doc.SelectSingleNode("//" + WceStrings.Element.ShowFormForMluChargeChoice);
			LoadShowFormForMluChargeChoice(ShowFormForMluChargeChoiceNode);
		}


		//---------------------------------------------------------------------
		public void LoadKeys(XmlNodeList keyNodes)
		{
			ArrayList keys = new ArrayList();
			foreach (XmlElement keyNode in keyNodes)
			{
				string key		= keyNode.GetAttribute(WceStrings.Attribute.Key);
				string internalCode = keyNode.GetAttribute(WceStrings.Attribute.InternalCode);
				string producer = null;
				if (internalCode == null || internalCode.Length <= 0)
				{
					internalCode = null;
					producer = keyNode.GetAttribute(WceStrings.Attribute.Producer);
				}
				
				if (key != null && key != String.Empty)
				{
					ActivationKeyInfo keyInfo = new ActivationKeyInfo(producer, internalCode, key/*, GetUserIdFromProducer(internalCode==null?producer:internalCode)*/);
					keys.Add(keyInfo);
				}
			}
			KeyInfos = (ActivationKeyInfo[])keys.ToArray(typeof(ActivationKeyInfo));

		}

		//---------------------------------------------------------------------
		public void LoadErrors(XmlNodeList errorNodes)
		{
			ArrayList errors = new ArrayList();
			Hashtable table = new Hashtable();
			foreach (XmlElement errorNode in errorNodes)
			{
				string error = errorNode.GetAttribute(WceStrings.Attribute.Value);
				int errorCode = -1;
				if (error != null && error != String.Empty)
					errorCode = GetCodeFromString(error);
               
				string id = errorNode.GetAttribute(WceStrings.Attribute.Id);
				int idError = 0;
				if (id != null && id != String.Empty)
				{
					try 
					{
						idError = Int32.Parse(id, CultureInfo.InvariantCulture);
					}
					catch {}
				}
				
				string details = errorNode.InnerText;
				if (errorCode != -1)
				{
					if (table.Contains(errorCode))
					{
						foreach (DictionaryEntry entry in table)
						{
							if(((int)entry.Key) == errorCode)
								((ErrorInfo)entry.Value).Details += (Environment.NewLine + details);
						}
					}
					else
						table.Add(errorCode, new ErrorInfo(errorCode, idError, details ));
				}	
				
			}
			foreach (DictionaryEntry entry in table)
				errors.Add((ErrorInfo)entry.Value);
			Errors = (ErrorInfo[])errors.ToArray(typeof(ErrorInfo));
		}

		//---------------------------------------------------------------------
		public void LoadUserId(XmlNodeList userIdNodes)
		{
			ArrayList userIds = new ArrayList();
			foreach (XmlElement userIdNode in userIdNodes)
			{
				string aValue	= userIdNode.GetAttribute(WceStrings.Attribute.Value);
				string internalCode = userIdNode.GetAttribute(WceStrings.Attribute.InternalCode);
				string activationID = userIdNode.GetAttribute(WceStrings.Attribute.ActivationID);
				string producer = null;
				if (internalCode == null || internalCode.Length <= 0)
				{
					internalCode = null;
					producer = userIdNode.GetAttribute(WceStrings.Attribute.Producer);
				}
				
				if (aValue != null && aValue != String.Empty)
				{
					UserId userIdInfo = new UserId(producer, internalCode, aValue);
					userIdInfo.ActivationID = activationID;
					userIds.Add(userIdInfo);
				}
			}
			UserIdInfos = (UserId[])userIds.ToArray(typeof(UserId));
		}

		//---------------------------------------------------------------------
		public void LoadShowFormForMluChargeChoice(XmlNode showFormForMluChargeChoiceNode)
		{
			if (showFormForMluChargeChoiceNode == null) 
			{
				ShowFormForMluChargeChoice = false;
				return;
			}
			string aValue	= ((XmlElement)showFormForMluChargeChoiceNode).GetAttribute(WceStrings.Attribute.Value);
			ShowFormForMluChargeChoice = String.Compare(aValue, Boolean.TrueString, true, CultureInfo.InvariantCulture) == 0 ? true : false;
		}

		//---------------------------------------------------------------------
		public string GetKey()
		{
			if (KeyInfos == null || KeyInfos.Length <= 0)
				return String.Empty;
			foreach (ActivationKeyInfo keyInfo in KeyInfos)
			{
				foreach (string s in UserInfo.InternalCodes)
				{
					if (String.Compare(keyInfo.InternalCode, s, true, CultureInfo.InvariantCulture) == 0 || String.Compare(keyInfo.Producer, s, true, CultureInfo.InvariantCulture) == 0)
						return keyInfo.Key;
				}
			}
			return String.Empty;
		}

//		//---------------------------------------------------------------------
//		public string GetKeyFromProducer()
//		{
//
//			if (KeyInfos == null || KeyInfos.Length <= 0)
//				return String.Empty;
//			foreach (ActivationKeyInfo keyInfo in KeyInfos)
//			{
//				foreach (string s in UserInfo.InternalCodes)
//				{
//					if (String.Compare(keyInfo.InternalCode, s, true) == 0)
//						return keyInfo.Key;
//				}
//			}
//			return String.Empty;
//		}
/*
		//---------------------------------------------------------------------
		public string GetUserIdFromProducer(string internalCode)
		{
			if (UserIdInfos == null)
				return String.Empty;
			foreach (UserId userId in UserIdInfos)
			{
				if (String.Compare(userId.InternalCode, internalCode, true) == 0 || 
					String.Compare(userId.Producer, internalCode, true) == 0)
					return userId.Value;
			}
			return String.Empty;
		}
*/
//		public bool Success	
		//---------------------------------------------------------------------
		public bool IsAMessage
		{ 
			get 
			{
				if (Errors.Length == 0)
                    return true;

				return Errors[0].Code == GetCodeFromString(ErrorMessage.Ok);
			}
		}

//		public bool SuccessAsync	
		//---------------------------------------------------------------------
		public bool IsAMessageAsync
		{ 
			get 
			{
				return (Errors.Length > 0 && Errors[0].Code == GetCodeFromString(ErrorMessage.AsyncOk)) ;
			}
		}

		//---------------------------------------------------------------------
		public bool IsAForcedTimeRequest
		{
			get
			{
				if (IsAMessage || Errors.Length == 0)
					return false;

				foreach (ErrorInfo ei in Errors)
					if (
						string.Compare
							(ErrorMessage.PingToBeBlocked,
							ei.Code.ToString(CultureInfo.InvariantCulture),
							StringComparison.InvariantCultureIgnoreCase) == 0)
						return true;

				return false;
			}
		}

//		public bool IsActivationToBeBlocked
		//---------------------------------------------------------------------
		public bool IsARealTimeRequest
		{ 
			get 
			{
				if (IsAMessage || Errors.Length == 0)
					return false;

				foreach (ErrorInfo ei in Errors)
				{
					
					if (
						string.Compare(ErrorMessage.PingToBeBlocked, ei.Code.ToString(CultureInfo.InvariantCulture), true, CultureInfo.InvariantCulture) == 0			||
						string.Compare(ErrorMessage.PingRequestNotUnderstood, ei.Code.ToString(CultureInfo.InvariantCulture), true, CultureInfo.InvariantCulture) == 0	||
						string.Compare(ErrorMessage.PingNotEqualsParameters, ei.Code.ToString(CultureInfo.InvariantCulture), true, CultureInfo.InvariantCulture) == 0	||
						string.Compare(ErrorMessage.PongNotReadDll, ei.Code.ToString(CultureInfo.InvariantCulture), true, CultureInfo.InvariantCulture) == 0			||
						string.Compare(ErrorMessage.PongNotDecriptDll, ei.Code.ToString(CultureInfo.InvariantCulture), true, CultureInfo.InvariantCulture) == 0			||
						string.Compare(ErrorMessage.PongReflectionError, ei.Code.ToString(CultureInfo.InvariantCulture), true, CultureInfo.InvariantCulture) == 0		||
						string.Compare(ErrorMessage.ParamsDllNotFound, ei.Code.ToString(CultureInfo.InvariantCulture), true, CultureInfo.InvariantCulture) == 0
						)
						return true;
				}

				return false;
			}
		}

//		public int SuccessCode	
		//---------------------------------------------------------------------
		public int BaseIndex	
		{ 
			get 
			{
				return GetCodeFromString(ErrorMessage.Ok);
			}
		}

//		public int SuccessAsynchCode	
		//---------------------------------------------------------------------
		public int ComplexIndex	
		{ 
			get 
			{
				return GetCodeFromString(ErrorMessage.AsyncOk);
			}
		}

		//---------------------------------------------------------------------
		public override string ToString()
		{
			if (Errors != null && Errors.Length > 0)
			{
				ReturnValuesWriter writer = new ReturnValuesWriter();
				string s =  writer.GetErrorsXml(Errors);
				if (s != null)
					return s;
			}
			
			return sourcestring;
		}
		
	}

	//=========================================================================
	public class ErrorInfo
	{
		public int Id			= 0;
		public int Code			= 0;
		public string Details	= null;

		//---------------------------------------------------------------------
		public ErrorInfo(int errorCode, int id, string details)
		{
			Id		= id;
			Code	= errorCode;
			Details = details;
		}
	
		//---------------------------------------------------------------------
		public ErrorInfo(int errorCode, string details):this(errorCode, 0, details)
		{}
	}

}

using System;

namespace Microarea.TaskBuilderNet.Licence.Licence
{
	//=========================================================================
	public class WebServiceErrors
	{
		public enum Errors
		{
			ErrorNotValid						= 70,
			OK									= 80,
			AsyncOk								= 85,
			ResponseXmlNotValid					= 90,
			ResponseEmpty						= 95,
			UnknownError						= 100,
			Producer_SerialNumber_Not_Verified	= 105,
			Not_Verified						= 110, 
			Missing_SerialNumber				= 115,
			Error_Loading_XML_Data				= 120, 
			Invalid_Version_Format				= 125,
			Error_Signing_Data					= 130, 
			Error_Verifing_Data					= 140, 
			Error_Updating_DataBase				= 150, 
			Resource_Busy						= 155,
			Error_Initializing_Web_Service		= 160, 
			Country_Not_Valid					= 170, 
			Error_In_UserInfo_Element			= 180, 
			Error_In_LicensedFiles_Element		= 190, 
			No_Licensed_Found					= 195,
			Error_In_SerialNumber_Format		= 200, 
			No_SerialNumber_Found				= 210,
			Error_In_ActivationKey_Element		= 220, 
			ActivationKey_Not_Valid				= 230, 
			Configuration_Not_Changed			= 240,
			User_Not_Recognized					= 250,
			Incorrect_VatNumber					= 255,
			Error_Generating_ActivationKey		= 260,
			Invalid_Database_Value				= 270,
			Invalid_Edition_Value				= 280,
			Invalid_OpSys_Value					= 290,
			Invalid_Request						= 300,
			No_CAL_Found						= 310,
			Found_OSLS_And_OSLL					= 316,
			Found_OSLL_But_OSLS_Was_Previously_Activated = 317,
			No_Producer_Found					= 320,
			ProducerKey_Not_Found				= 325,
			ProducerKey_Without_License			= 326,
			ProductName_Not_Found				= 330,
			Duplicated_Product					= 335,
			User_Disabled						= 340,
			Activation_Disabled					= 345,
			Error_Contacting_Web_Service		= 350,
			Migration_Order_Not_Yet_Generated	= 360,
			HasSerialAttribute_Not_Allowed		= 370,
			Time_Error							= 380,
			NoTenCalEnt							= 385,
			Too_Much_CAL_Specified				= 390,
			Too_Little_Oracle_CAL_Specified		= 391,
			Invalid_License						= 395,
			Product_Disabled					= 396,
			Incompatible_Products				= 397,
            Error_Confirmating_User_Data        = 460,
            Cannot_Confirm_User_Data            = 470,
//errori lato client prima del ping
			ExceptionBeforeCall					= 2222,
			LoginManagerNotExisting				= 2223,
			WebExceptionPinging					= 2224,
			SoapExceptionPinging				= 2225,
			InvalidOperationExceptionPinging	= 2226,
			ExceptionPinging					= 2227,
			ExceptionInstantiatingProxy			= 2228,
			ExceptionDuringCommunication		= 2229,
			PongReflectionError					= 1070,
			ArgumentsNull						= 1090,
			ParamManagerException				= 1091,
			NullCountry							= 1092,
//errori Async-ping
			/*ErrorPackSynch						= 700,
			ErrorPackAsynch						= 701,
			ErrorWriteID						= 710,
			ErrorSerialize						= 720,
			ErrorEmptyID						= 730,
			ErrorNullRequest					= 740,
			ErrorNullResponse					= 741,
			ErrorUnpackReq						= 750,
			ErrorUnpackRes						= 751,
			RequestNotSavedCorrectly			= 760,
			ErrorInvalidRequest					= 770*/

		}

		public enum MessageType {Error, Warning, Info};

		//---------------------------------------------------------------------
		public static string GetTitleFromErrorCode(int errorCode)
		{ 
			if (IsInfo(errorCode))
				return LicenceStrings.MsgTitleInfo;
			return LicenceStrings.MsgTitleError;
		}

		//---------------------------------------------------------------------
		public static MessageType GetMessageTypeFromErrorCode(int errorCode)
		{ 
			if (IsInfo(errorCode))
				return MessageType.Info;
			return MessageType.Error;
		}

		//---------------------------------------------------------------------
		private static bool IsInfo(int errorCode)
		{ 
			switch (errorCode)
			{
				case (int)Errors.Configuration_Not_Changed:
				case (int)Errors.OK:
				case (int)Errors.AsyncOk:
					return true;
		
				default:
					return false;
			}
		}
	}

	//=========================================================================
	public class ActivationKeyErrors : WebServiceErrors
	{
		//---------------------------------------------------------------------
		public static string GetStringFromErrorCode(int errorCode, string details)
		{ 
			switch (errorCode)
			{
				case (int)Errors.ErrorNotValid:
					return LicenceStrings.ErrorNotValid;
					
				case (int)Errors.OK:
					return LicenceStrings.RegisterOK;

				/*case (int)Errors.AsyncOk:
					return LicenceStrings.AsyncOk;

				case (int)Errors.ResponseXmlNotValid:
					return LicenceStrings.ResponseXmlNotValid;

				case (int)Errors.ResponseEmpty:
					return LicenceStrings.ResponseEmpty;*/

                case (int)Errors.UnknownError://riporto i dettagli così se aggiungiamo errori si pu`osemrpe mostrarne i dettagli che mi vengono inviati anche se il programma non nè pronto a gestirlo
                    {
                        if(string.IsNullOrWhiteSpace(details))
                          return LicenceStrings.Unknown_Error + ": " + details;
                        return LicenceStrings.Unknown_Error;
                    } 

				case (int)Errors.Producer_SerialNumber_Not_Verified:
				{
					if (details == null || details == String.Empty)
						details = LicenceStrings.Undefined;
					return String.Format(LicenceStrings.Producer_SerialNumber_Not_Verified, details);
				}

				case (int)Errors.ArgumentsNull:
				return String.Format(LicenceStrings.ArgumentsNull, details);

				case (int)Errors.PongReflectionError:
				return String.Format(LicenceStrings.PongReflectionError, details);

				case (int)Errors.ParamManagerException:
				return String.Format(LicenceStrings.ParamManagerException, details);

				case (int)Errors.NullCountry:
				return String.Format(LicenceStrings.NullCountry, details);

				case (int)Errors.Not_Verified:
				return String.Format(LicenceStrings.Not_Verified, details);

				case (int)Errors.Missing_SerialNumber:
				return LicenceStrings.Missing_SerialNumber;

				case (int)Errors.Error_Loading_XML_Data:
				return LicenceStrings.Error_Loading_XML_Data;

				case (int)Errors.Invalid_Version_Format:
				return LicenceStrings.Invalid_Version_Format;

				case (int)Errors.Error_Signing_Data:
				return LicenceStrings.Error_Signing_Data;

				case (int)Errors.Error_Verifing_Data:
				return LicenceStrings.Error_Verifing_Data;

				case (int)Errors.Error_Updating_DataBase:
				return LicenceStrings.Error_Updating_DataBase;

				case (int)Errors.Resource_Busy:
				return LicenceStrings.Resource_Busy;

				case (int)Errors.Found_OSLS_And_OSLL:
				return LicenceStrings.Found_OSLS_And_OSLL;

				case (int)Errors.Found_OSLL_But_OSLS_Was_Previously_Activated:
				return LicenceStrings.Found_OSLL_But_OSLS_Was_Previously_Activated;

				case (int)Errors.Error_Initializing_Web_Service:
				return LicenceStrings.Error_Initializing_ActivationWebService;

				case (int)Errors.Country_Not_Valid:
				return LicenceStrings.Country_Not_Valid;

				case (int)Errors.Error_In_UserInfo_Element:
				return LicenceStrings.Error_In_UserInfo_Element;

				case (int)Errors.Error_In_LicensedFiles_Element:
				return LicenceStrings.Error_In_LicensedFiles_Element;

				case (int)Errors.ProducerKey_Not_Found:
				return LicenceStrings.ProducerKey_Not_Found;

				case (int)Errors.ProducerKey_Without_License:
				{
					if (details == null || details == String.Empty)
						details = LicenceStrings.Undefined;
					return String.Format(LicenceStrings.ProducerKey_Without_License, details);
				}

				case (int)Errors.No_Licensed_Found:
				return LicenceStrings.No_Licensed_Found;

				case (int)Errors.Error_In_SerialNumber_Format:
				return LicenceStrings.Error_In_SerialNumber_Format;

				case (int)Errors.No_SerialNumber_Found:
				return LicenceStrings.No_SerialNumber_Found;

				case (int)Errors.Error_In_ActivationKey_Element:
				return LicenceStrings.Error_In_ActivationKey_Element;

				case (int)Errors.ActivationKey_Not_Valid:
				return LicenceStrings.ActivationKey_Not_Valid;

				case (int)Errors.Configuration_Not_Changed:
				return LicenceStrings.Configuration_Not_Changed;

				case (int)Errors.User_Not_Recognized:
				return LicenceStrings.User_Not_Recognized;

				case (int)Errors.Incorrect_VatNumber:
				return LicenceStrings.Incorrect_VatNumber;

				case (int)Errors.Error_Generating_ActivationKey:
				return LicenceStrings.Error_Generating_ActivationKey;

				case (int)Errors.Invalid_Database_Value:
				return LicenceStrings.Invalid_Database_Value;

				case (int)Errors.Invalid_Edition_Value:
				return LicenceStrings.Invalid_Edition_Value;

				case (int)Errors.Invalid_OpSys_Value:
				return LicenceStrings.Invalid_OpSys_Value;

				case (int)Errors.Invalid_Request:
				return LicenceStrings.Invalid_Activation_Request;

				case (int)Errors.No_CAL_Found:
				return LicenceStrings.No_CAL_Found;

				case (int)Errors.No_Producer_Found:
				return LicenceStrings.No_Producer_Found;

				case (int)Errors.User_Disabled:
				return LicenceStrings.User_Disabled;

				case (int)Errors.Duplicated_Product:
				return LicenceStrings.Duplicated_Product;
					
				case (int)Errors.ProductName_Not_Found:
				return LicenceStrings.ProductName_Not_Found;

				case (int)Errors.Product_Disabled:
				return LicenceStrings.Product_Disabled;
				
				case (int)Errors.Incompatible_Products:
				return LicenceStrings.Incompatible_Products;

                case (int)Errors.Error_Confirmating_User_Data:
                return LicenceStrings.Error_Confirmating_User_Data;

                case (int)Errors.Cannot_Confirm_User_Data:
                return LicenceStrings.Cannot_Confirm_User_Data;

				case (int)Errors.Activation_Disabled:
				return LicenceStrings.Activation_Disabled;
				
				case (int)Errors.Error_Contacting_Web_Service:
				{
					if (details == null || details == String.Empty)
						details = LicenceStrings.AllProducers;
					return String.Format(LicenceStrings.Error_Contacting_Web_Service, details);

				}
				case (int)Errors.Migration_Order_Not_Yet_Generated	:
				return LicenceStrings.Migration_Order_Not_Yet_Generated;

				case (int)Errors.HasSerialAttribute_Not_Allowed	:
				return LicenceStrings.HasSerialAttribute_Not_Allowed;

				case (int)Errors.Time_Error	:
				return LicenceStrings.Time_Error;

				case (int)Errors.Too_Much_CAL_Specified	:
				return LicenceStrings.Too_Much_CAL_Specified;

				case (int)Errors.Too_Little_Oracle_CAL_Specified	:
				return LicenceStrings.Too_Little_Oracle_CAL_Specified;

				case (int)Errors.NoTenCalEnt:
				return LicenceStrings.TenCalNeeded;

				case (int)Errors.Invalid_License	:
				return LicenceStrings.Invalid_License;
//errori client pre-ping

				case (int)Errors.ExceptionBeforeCall:
				return String.Format(LicenceStrings.ExceptionBeforeCall, details);

				case (int)Errors.LoginManagerNotExisting:
				return String.Concat(String.Format(LicenceStrings.LoginManagerNotExisting, details), LicenceStrings.LoginManagerNotExistingDet);

				case (int)Errors.WebExceptionPinging:
				return String.Format(LicenceStrings.WebExceptionPinging, details);

				case (int)Errors.SoapExceptionPinging:
				return String.Format(LicenceStrings.SoapExceptionPinging, details);

				case (int)Errors.ExceptionDuringCommunication:
				return String.Format(LicenceStrings.ExceptionDuringCommunication, details);

				case (int)Errors.InvalidOperationExceptionPinging:
				return String.Format(LicenceStrings.InvalidOperationExceptionPinging, details);

				case (int)Errors.ExceptionInstantiatingProxy:
				return String.Format(LicenceStrings.ExceptionInstantiatingProxy, details);

				case (int)Errors.ExceptionPinging:
				return String.Format(LicenceStrings.ExceptionInstantiatingProxy, details);
				//errori client Async-ping
				/*case (int)Errors.ErrorPackSynch:
					return String.Format(LicenceStrings.ErrorPackSynch, details);

				case (int)Errors.ErrorPackAsynch:
					return String.Format(LicenceStrings.ErrorPackAsynch, details);

				case (int)Errors.ErrorWriteID:
					return String.Format(LicenceStrings.ErrorWriteID, details);

				case (int)Errors.ErrorSerialize:
					return String.Format(LicenceStrings.ErrorSerialize, details);

				case (int)Errors.ErrorEmptyID:
					return String.Format(LicenceStrings.ErrorEmptyID, details);

				case (int)Errors.ErrorNullRequest:
					return String.Format(LicenceStrings.ErrorNullRequest, details);

				case (int)Errors.ErrorNullResponse:
					return String.Format(LicenceStrings.ErrorNullResponse, details);

				case (int)Errors.ErrorUnpackReq:
					return String.Format(LicenceStrings.ErrorUnpackReq, details);

				case (int)Errors.ErrorUnpackRes:
					return String.Format(LicenceStrings.ErrorUnpackRes, details);

				case (int)Errors.RequestNotSavedCorrectly:
					return String.Format(LicenceStrings.RequestNotSavedCorrectly, details);

				case (int)Errors.ErrorInvalidRequest:
					return String.Format(LicenceStrings.ErrorInvalidRequest, details);
					*/
				default:
                {
                    if (string.IsNullOrWhiteSpace(details))
                        return LicenceStrings.Unknown_Error + ": " + details;
                    return LicenceStrings.Unknown_Error;
                } 
			}
		}
	}
}

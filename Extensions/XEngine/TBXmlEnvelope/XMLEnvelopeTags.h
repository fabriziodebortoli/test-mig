#pragma once

// namespace di XTech
#define XTECH_NAMESPACE							_T("http://www.microarea.it/XTech/1.0.0/XMLSchema")

// namespace di SmartXTech
#define SMART_NAMESPACE							_T("http://www.microarea.it/Schema/2004/Smart/")


#define ENV_XML_FILE_NAME						_T("Envelope")  //nome del file di envelope

//Definizioni di TAG XML usati dal parser dell'envelope
#define ENV_XML_ENVELOPE_ID_TAG					_T("Envelope")
#define ENV_XML_EXPORT_ID_TAG					_T("ExportID")
#define ENV_XML_DESCRIPTION_TAG					_T("Description")
#define ENV_XML_DOC_INFO_TAG					_T("DocumentInfo")
#define ENV_XML_DOMAIN_TAG						_T("Domain")
#define ENV_XML_SITE_TAG						_T("Site")
#define ENV_XML_SITE_CODE_TAG					_T("SiteCode")
#define ENV_XML_USER_TAG						_T("User")
#define ENV_XML_DATATIME_TAG					_T("Datatime")
#define ENV_XML_ROOT_NS_TAG						_T("RootDocNamespace")
#define ENV_XML_CONTENTS_TAG					_T("Contents")
#define ENV_XML_FILE_TAG						_T("File")
#define ENV_XML_PROFILE_TAG						_T("Profile")
#define ENV_XML_ENVCLASS_TAG					_T("EnvelopeClass")

#define ENV_XML_DATA_URL_ATTRIBUTE				_T("dataurl")
#define ENV_XML_ENVCLASS_ATTRIBUTE				_T("envelopeclass")
#define ENV_XML_DOC_NAME_ATTRIBUTE				_T("documentname")
#define ENV_XML_DOC_NUMB_ATTRIBUTE				_T("documentnumber")
#define ENV_XML_TYPE_ATTRIBUTE					_T("type")

#define ENV_XML_FILE_SCHEMA_TYPE				_T("Schema")
#define ENV_XML_FILE_ROOT_TYPE					_T("Root")
#define ENV_XML_FILE_NEXT_ROOT_TYPE				_T("NextRoot")
#define ENV_XML_FILE_XREF_TYPE					_T("XReference")
#define ENV_XML_FILE_ENV_TYPE					_T("Envelope")
#define ENV_XML_FILE_UNDEF_TYPE					_T("Undefined")

//Definizioni di TAG XML usati dal parser delle property del tender
#define TENDER_XML_ROOTH_TAG					_T("TenderParams")
#define TENDER_XML_SITENAME_TAG					_T("SiteName")
#define TENDER_XML_URLREP_TAG					_T("UrlRep")
#define TENDER_XML_TRYNUM_TAG					_T("TryNum")
#define TENDER_XML_ENCODING_TAG					_T("Encoding")
#define TENDER_XML_ENCODING_UTF8_TYPE			_T("UTF-8")
#define TENDER_XML_ENCODING_UTF16_TYPE			_T("UTF-16")
#define TENDER_XML_TIMEOUT_TAG					_T("TimeOut")		

#define NAMESPACE_PREFIX						_T("X:")
#define GET_NAMESPACE_PREFIX(pObj)				pObj->GetNamespaceURI().IsEmpty() ? _T("") : NAMESPACE_PREFIX



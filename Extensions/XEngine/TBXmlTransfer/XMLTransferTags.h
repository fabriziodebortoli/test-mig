#pragma once

//Definizioni di TAG XML usati dal parser dell'export/import di documento
#define DOC_XML_DOCUMENT_TAG					_T("Document")
#define DOC_XML_DOCINFO_TAG						_T("DocumentInfo")
#define DOC_XML_NAMESPACE_TAG					_T("Namespace")
#define DOC_XML_TITLE_TAG						_T("Title")
#define DOC_XML_PROFILE_TAG						_T("Profile")
#define DOC_XML_DESCRIPTION_TAG					_T("Description")
#define DOC_XML_NEXTFILE_TAG					_T("NextFile")
#define DOC_XML_CREATION_TAG					_T("Creation")
#define DOC_XML_DOMAIN_TAG						_T("Domain")
#define DOC_XML_SITE_TAG						_T("Site")
#define DOC_XML_USER_TAG						_T("User")
#define DOC_XML_DATETIME_TAG					_T("DateTime")
#define DOC_XML_DOCUMENTS_TAG					_T("Documents")
#define DOC_XML_MASTERS_TAG						_T("Masters")
#define DOC_XML_MASTER_TAG						_T("Master")
#define DOC_XML_SLAVES_TAG						_T("Slaves")
#define DOC_XML_SLAVEBUFF_TAG					_T("SlaveBuff")
#define DOC_XML_SLAVE_TAG						_T("Slave")
#define DOC_XML_EXTREF_TAG						XML_EXTERNAL_REFERENCE_TAG
#define DOC_XML_EXTREFS_TAG						XML_EXTERNAL_REFERENCES_TAG
#define DOC_XML_KEY_TAG							_T("Key")
#define DOC_XML_FIELDS_TAG						XML_FIELD_FIELDS_TAG
#define DOC_XML_MESSAGE_TAG						_T("Message")
#define DOC_XML_CODE_TAG						_T("Code")
#define DOC_XML_SOURCE_TAG						_T("Source")
#define DOC_XML_SOURCE_TAG						_T("Source")
#define DOC_XML_PK_ATTRIBUTE					_T("pk")	
#define DOC_XML_FK_ATTRIBUTE					_T("fk")
#define	DOC_XML_NAME_ATTRIBUTE					_T("name")
#define DOC_XML_DOCNAME_ATTRIBUTE				_T("documentname")
#define DOC_XML_TABLE_ATTRIBUTE					_T("table")
#define DOC_XML_ROWSNUMBER_ATTRIBUTE			_T("rowsnumber")
#define DOC_XML_NUMBER_ATTRIBUTE				_T("number")
#define DOC_XML_EXTREF_FILE_ATTRIBUTE			_T("exportedinfile")
#define DOC_XML_BOOKMARK_ATTRIBUTE				_T("bookmark")
#define DOC_XML_NAMESPACE_ATTRIBUTE				XML_NAMESPACE_ATTRIBUTE
#define DOC_XML_DELETED_ATTRIBUTE				_T("deleted")
#define DOC_XML_XTECHPROFILE_ATTRIBUTE			_T("xTechProfile")
#define DOC_XML_MASTER_ATTRIBUTE				_T("master")
#define DOC_XML_TBNAMESPACE_ATTRIBUTE			_T("tbNamespace")
#define DOC_XML_EXTREF_ATTRIBUTE				_T("externalReference")
#define DOC_XML_DATAINSTANCES_ATTRIBUTE			_T("instances")
#define DOC_XML_PROCESSED_ATTRIBUTE				_T("processed")
#define DOC_XML_VALUE_ATTRIBUTE					_T("value")
#define DOC_XML_POSTABLE_ATTRIBUTE				_T("postable")
#define DOC_XML_POSTBACK_ATTRIBUTE				_T("postBack")
#define DOC_XML_POSTYPE_ATTRIBUTE				_T("posType")
#define DOC_XML_UPDATE_ATTRIBUTE				_T("updateType")


#define DOC_XML_HOTKEYLINK_ATTRIBUTE			_T("hotLink")
#define DOC_XML_PRIMARYKEY_ATTRIBUTE			_T("primaryKey")
#define DOC_XML_ENUMERATION_ATTRIBUTE			_T("enum")
#define DOC_XML_ENUMERATIONNAMESPACE_ATTRIBUTE	_T("EnumNameSpace")
// tag utilizzati per lo smart document 
#define DOC_XML_DATA_TAG						_T("Data")
#define DOC_XML_ERROR_TAG						_T("Error")
#define DOC_XML_ERRORS_TAG						_T("Errors")
#define DOC_XML_WARNING_TAG						_T("Warning")
#define DOC_XML_WARNINGS_TAG					_T("Warnings")
#define DOC_XML_DIAGNOSTIC_TAG					_T("Diagnostic")

#define DOC_XML_CODING_NUMBER_PADDING			_T("NumberPadding")
#define DOC_XML_CODING_CODE_EXTENSION			_T("CodeExtension")
#define DOC_XML_CODING_SAME_CODE				_T("SameCode")
#define DOC_XML_CODING_TABLE_TAG				_T("Table")
#define DOC_XML_CODING_BEFORE_VALUE				_T("Before")

#define DOC_XML_CODING_INSERT_ATTRIBUTE			_T("insert")
#define DOC_XML_CODING_LENGTH_ATTRIBUTE			_T("length")

//Definizioni di TAG XML usati dal parser dei criteri di esportazione

#define CRITERIA_XML_SELECTIONS					_T("Selections")
#define CRITERIA_XML_PREFERENCES_CRITERIA		_T("Preferences")
#define CRITERIA_XML_PREFERENCES_USE			_T("UseCriteria")
#define CRITERIA_XML_PREFERENCES_USE_OSL		_T("OSLTrace")
#define CRITERIA_XML_PREFERENCES_USE_APP		_T("Predefined")
#define CRITERIA_XML_PREFERENCES_USE_USER		_T("User")
#define CRITERIA_XML_PRIORITY					_T("Priority")

#define CRITERIA_XML_PRIORITY_MODE_ATTRIBUTE	_T("mode")
#define CRITERIA_XML_PRIORITY_MODE_OSL			_T("OSLPriority")
#define CRITERIA_XML_PRIORITY_MODE_GES			_T("GesPriority")
#define CRITERIA_XML_PRIORITY_MODE_AUTOMATIC	_T("Automatic")

#define CRITERIA_XML_ENVFILE					_T("EnvelopeClass")
#define CRITERIA_XML_PREDEFINED_CRITERIA		_T("PredefinedCriteria")
#define CRITERIA_XML_OSLTRACE_CRITERIA			_T("OSLTraceCriteria")
#define CRITERIA_XML_OSLTRACE_STARTDATE			_T("StartDate")
#define CRITERIA_XML_OSLTRACE_ENDDATE			_T("EndDate")
#define CRITERIA_XML_OSLTRACE_UPDATED			_T("Updated")
#define CRITERIA_XML_OSLTRACE_INSERED			_T("Insered")
#define CRITERIA_XML_OSLTRACE_DELETED			_T("Deleted")
#define CRITERIA_XML_ASKEXPORT_RULES			_T("AskExportRules")
#define CRITERIA_XML_USER_CRITERIA_RULES		_T("UserCriteria")
#define CRITERIA_XML_USER_OVERRIDE_ATTRIBUTE	_T("override")

#define CRITERIA_XML_USER_CRITERIA_STRING		_T("String")
#define CRITERIA_XML_USER_VARIABLES				_T("UserVariables")
#define CRITERIA_XML_USER_GROUP					_T("UserGroup")
#define CRITERIA_XML_USER_VARIABLE				_T("UserVariable")

#define CRITERIA_XML_ENVFILE_NAME_ATTRIBUTE		_T("name")
#define CRITERIA_XML_USER_GROUP_NAME_ATTRIBUTE	_T("name")
#define CRITERIA_XML_USER_VAR_NAME_ATTRIBUTE	_T("name")
#define CRITERIA_XML_USER_VAR_VALUE_ATTRIBUTE	_T("value")

#define CRITERIA_XML_TRUE						XML_TRUE
#define CRITERIA_XML_FALSE						XML_FALSE


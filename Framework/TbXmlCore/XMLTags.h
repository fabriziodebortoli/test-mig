#pragma once

// definizione dei data types
#define XML_DATATYPE_STRING_VALUE		_T("string")
#define XML_DATATYPE_CI_STRING_VALUE	_T("ciString")
#define XML_DATATYPE_INT_VALUE			_T("integer")
#define XML_DATATYPE_INT2_VALUE			_T("int")
#define XML_DATATYPE_LONG_VALUE			_T("long")
#define XML_DATATYPE_DOUBLE_VALUE		_T("double")
#define XML_DATATYPE_PERC_VALUE			_T("percent")
#define XML_DATATYPE_QUANTITY_VALUE		_T("quantity")
#define XML_DATATYPE_MONEY_VALUE		_T("money")
#define XML_DATATYPE_UUID_VALUE			_T("uuid")
#define XML_DATATYPE_DATE_VALUE			_T("date")
#define XML_DATATYPE_TIME_VALUE			_T("time")
#define XML_DATATYPE_DATETIME_VALUE		_T("dateTime")
#define XML_DATATYPE_BOOLEAN_VALUE		_T("bool")
#define XML_DATATYPE_BOOLEAN2_VALUE		_T("boolean")
#define XML_DATATYPE_ENUM_VALUE			_T("enum")
#define XML_DATATYPE_ELAPSEDTIME_VALUE	_T("elapsedTime")
#define XML_DATATYPE_ARRAY_VALUE		_T("array")
#define XML_DATATYPE_VOID_VALUE			_T("void")
#define XML_DATATYPE_BLOB_VALUE			_T("blob")
#define XML_DATATYPE_TEXT_VALUE			_T("text")
#define XML_DATATYPE_IDENTITY_VALUE		_T("identity")
#define XML_DATATYPE_DATAARRAY_VALUE	_T("dataarray")
#define XML_DATATYPE_DATAENUM_VALUE		_T("dataenum")
#define XML_DATATYPE_VARIANT_VALUE		_T("variant")
#define XML_DATATYPE_OBJECT_VALUE		_T("object")

// more used tags 
#define XML_NAMESPACE_TAG				_T("Namespace")
#define XML_NAME_TAG					_T("Name")
#define XML_TITLE_TAG					_T("Title")
#define XML_FUNCTION_TAG				_T("Function")
#define XML_EVENT_TAG					_T("Event")
#define XML_REPORT_TAG					_T("Report")
#define XML_VALUE_TAG					_T("Value")
#define XML_VARIABLES_TAG				_T("Variables")
#define XML_VARIABLE_TAG				_T("Variable")


// more used attributes
#define XML_NAME_ATTRIBUTE				_T("name")
#define XML_NAMESPACE_ATTRIBUTE			_T("namespace")
#define XML_TBNAMESPACE_ATTRIBUTE		_T("tb_namespace")
#define XML_LIBRARYNS_ATTRIBUTE			_T("library_namespace")
#define XML_RELEASE_ATTRIBUTE			_T("release")
#define XML_LOCALIZE_ATTRIBUTE			_T("localize")
#define XML_DEFAULT_ATTRIBUTE			_T("defaultReport")
#define XML_DEFAULT_VALUE_ATTRIBUTE		_T("defaultvalue")
#define XML_CONTEXT_NAME_ATTRIBUTE		_T("contextName")
#define XML_RELEASE_ATTRIBUTE			_T("release")
#define XML_TYPE_ATTRIBUTE				_T("type")
#define XML_BASETYPE_ATTRIBUTE			_T("basetype")
#define XML_OPERATOR_ATTRIBUTE			_T("operator")
#define XML_COLLATESENSITIVE_ATTRIBUTE	_T("collatesensitive")
#define XML_TRUE_VALUE					_T("true")
#define XML_FALSE_VALUE					_T("false")
#define XML_DEFAULT_TAG					_T("DefaultReport")

// ReferenceObjects
#define XML_HOTKEYLINK_TAG				_T("HotKeyLink")
#define XML_DBFIELD_TAG					_T("DbField")
#define XML_COMBOBOX_TAG				_T("ComboBox")
#define XML_COLUMN_TAG					_T("Column")
#define XML_PARAMETER_TAG				_T("Param")
#define XML_DATATYPE_TAG				_T("DataType")
#define XML_PASS_INOUT_VALUE			_T("in out")
#define XML_PASS_IN_VALUE				_T("in")
#define XML_PASS_OUT_VALUE				_T("out")
#define XML_PASSINGMODE_ATTRIBUTE		_T("mode")
#define XML_DATATYPE_ATTRIBUTE			_T("data_type")
#define XML_TYPE_ATTRIBUTE				_T("type")
#define XML_LENGTH_ATTRIBUTE			_T("length")
#define XML_DATALENGTH_ATTRIBUTE		_T("data_length")
#define XML_SOURCE_ATTRIBUTE			_T("source")
#define XML_FORMATTER_ATTRIBUTE			_T("formatter")
#define XML_WHEN_ATTRIBUTE				_T("when")
#define XML_OPTIONAL_ATTRIBUTE			_T("optional")
#define XML_CLASSNAME_TAG				_T("ClassName")
#define XML_VISIBLE_ATTRIBUTE			_T("visible")

#define XML_DBTABLE_TAG					_T("DbTable")
#define XML_DBFIELDDESCRIPTION_TAG		_T("DbFieldDescription")
#define XML_CALLLINK_TAG				_T("CallLink")
#define XML_SELECTIONTYPES_TAG			_T("SelectionTypes")
#define XML_SELECTION_TAG				_T("Selection")
#define XML_SELECTIONMODES_TAG			_T("SelectionModes")
#define XML_MODE_TAG					_T("Mode")
#define XML_ASKRULES_TAG				_T("AskRules")

#define XML_AddOnFlyEnabled_ATTRIBUTE	_T("addOnFlyEnabled")
#define XML_MustExistData_ATTRIBUTE		_T("mustExistData")
#define XML_BrowseEnabled_ATTRIBUTE		_T("browseEnabled")
#define XML_Datafile_ATTRIBUTE			_T("datafile")
#define XML_LoadFullRecord_ATTRIBUTE	_T("loadFullRecord")

// FunctionObjects
#define XML_FUNCTIONOBJ_TAG					_T("FunctionObjects")
#define XML_FUNCTIONS_TAG					_T("Functions")
#define XML_EVENTS_TAG						_T("Events")

#define XML_REPORT_ATTRIBUTE				_T("report")
#define XML_SERVER_ATTRIBUTE				_T("server")
#define XML_SERVICE_ATTRIBUTE				_T("service")
#define XML_SERVICEPORT_ATTRIBUTE			_T("port")
#define XML_ALWAYSRECEIVEEVENT_ATTRIBUTE	_T("alwaysreceiveevent")
#define XML_TABLE_ATTRIBUTE					_T("table")
#define XML_PUBLISHED_ATTRIBUTE				_T("published")
#define XML_GROUP_ATTRIBUTE					_T("group")

#define XML_TBSCRIPT_TAG					_T("TbScript")

// ItemSourceObjects
#define XML_ITEMSOURCES_TAG					_T("ItemSources")
#define XML_ITEMSOURCE_TAG					_T("ItemSource")

// Reports
#define XML_REPORTOBJ_TAG				_T("ReportObjects")
#define XML_REPORTS_TAG					_T("Reports")
#define XML_ARGUMENTS_TAG				_T("Arguments")

// DatabaseObjects
#define XML_DBOBJECTS_TAG				_T("DatabaseObjects")
#define XML_DBOBJECTSBIN_TAG			_T("DBObjects")
#define XML_CREATE_TAG					_T("Create")
#define XML_DESCRIPTION_TAG				_T("Description")
#define XML_TABLES_TAG					_T("Tables")
#define XML_VIRTUAL_TABLES_TAG			_T("VirtualTables")
#define XML_VIEWS_TAG					_T("Views")
#define XML_PROCEDURES_TAG				_T("Procedures")
#define XML_PROCEDURE_TAG				_T("Procedure")
#define XML_TABLE_TAG					_T("Table")
#define XML_VIRTUAL_TABLE_TAG			_T("VirtualTable")
#define XML_VIEW_TAG					_T("View")
#define XML_CREATESTEP_ATTRIBUTE		_T("createstep")
#define XML_ADDCOLS_TAG					_T("ExtraAddedColumns")
#define XML_ADDCOL_TAG					_T("ExtraAddedColumn")
#define XML_MASTERTABLE_ATTRIBUTE		_T("mastertable")


// AddOnDatabaseObjects
#define XML_OLDADDCOLS_TAG				_T("AdditionalColumns")
#define XML_ADDONDBASEOBJECTS_TAG		_T("AddOnDatabaseObjects")
#define XML_ALTERTABLE_TAG				_T("AlterTable")

// DocumentObjects
#define XML_DOCUMENTOBJECTS_TAG			_T("DocumentObjects")
#define XML_DOCUMENTS_TAG				_T("Documents")
#define XML_DOCUMENT_TAG				_T("Document")
#define XML_INTERFACECLASS_TAG			_T("InterfaceClass")
#define XML_MODE_TAG					_T("Mode")
#define XML_VIEWMODES_TAG				_T("ViewModes") 
#define XML_CAPTION_TAG					_T("Caption") 
#define XML_CAPTION_FIELD_TAG			_T("Field")
#define XML_CAPTION_VALUE_ATTRIBUTE		_T("value")
#define XML_FRAME_ID_ATTRIBUTE			_T("frameID")
#define XML_MANAGED_TYPE_ATTRIBUTE		_T("managedType")

#define XML_ISO_SEPARATOR				_T(",")


#define XML_FRAMEID_ATTRIBUTE			_T("frameID")
#define XML_ACTIVATION_ATTRIBUTE		_T("activation")
#define XML_ALLOWISO_ATTRIBUTE			_T("allowISO")
#define XML_DENYISO_ATTRIBUTE			_T("denyISO")
#define XML_GROUPID_ATTRIBUTE			_T("groupid")
#define XML_SUBMENU_ATTRIBUTE			_T("submenu")
#define XML_CLASS_ATTRIBUTE				_T("class")
#define XML_CLASSHIERARCHY_ATTRIBUTE	_T("classhierarchy")
#define XML_MANAGEDHIERARCHY_ATTRIBUTE	_T("managedHierarchy")
#define XML_VIWMODETYPE_BATCH_VALUE		_T("batch")
#define XML_VIWMODETYPE_FINDER_VALUE	_T("finder")
#define XML_VIWMODETYPE_MANAGED_VALUE	_T("managed")
#define XML_VIWMODETYPE_SILENT_VALUE	_T("silent")
#define XML_SCHEDULABLE_ATTRIBUTE		_T("schedulable")
#define XML_REFERENCE_ATTRIBUTE			_T("reference")
#define XML_DYNAMIC_ATTRIBUTE			_T("dynamic")	
#define XML_DOC_TEMPLATE_ATTRIBUTE		_T("templateNamespace")	
#define XML_NOWEB_ATTRIBUTE				_T("noweb")
#define	XML_TRANSFERDISABLED_ATTRIBUTE	_T("transferdisabled")
#define XML_EXCLUDEFROMEXTREF_ATTRIBUTE	_T("excludeFromExtRef")
#define	XML_RUNNABLEALONE_ATTRIBUTE		_T("runnableAlone")

#define	XML_DESIGNABLE_ATTRIBUTE		_T("designable")

// ClientDocumentObjects
#define XML_CDDOCUMENTOBJECTS_TAG		_T("ClientDocumentObjects")
#define XML_CLIENTDOCS_TAG				_T("ClientDocuments")
#define XML_SERVERDOC_TAG				_T("ServerDocument")
#define XML_CLIENTDOC_TAG				_T("ClientDocument")
#define XML_TYPE_FAMILY_VALUE			_T("family")
#define XML_CLIENTFORMS_TAG				_T("ClientForms")
#define XML_CLIENTFORM_TAG				_T("ClientForm")


#define XML_CONTROLLER_ATTRIBUTE		_T("controller")
// OutDateObjects
#define XML_OUTDATEOBJECTS_TAG			_T("OutDateObjects")
#define XML_GROUPS_TAG					_T("Groups")
#define XML_GROUP_TAG					_T("Group")
#define XML_REPORTS_TAG					_T("Reports")
#define XML_REPORT_TAG					_T("Report")
#define XML_OWNER_ATTRIBUTE				_T("owner")

// ParametersSttings
#define XML_SETTINGS_TAG				_T("ParameterSettings")
#define XML_SECTION_TAG					_T("Section")
#define XML_SETTING_TAG					_T("Setting")

// EnvelopeObjects
#define XML_MODULEOBJECTS_TAG			_T("ModuleObjects")
#define XML_ENVELOPE_CLASSES_TAG		_T("EnvelopeClasses")
#define XML_ENVELOPE_CLASS_TAG			_T("EnvelopeClass")

// Smart Import\Export 
#define XML_PARAMETERS_TAG				_T("Parameters")
#define XML_TITLE_ATTRIBUTE				_T("title")
#define XML_DEFAULTDIALOG_TAG			_T("DefaultDialog")
#define XML_DEFAULTGROUP_TAG			_T("DefaultGroup")

// deployment policy attribute values
#define XML_DEPLOYPOLICY_BASE_VALUE		_T("base")
#define XML_DEPLOYPOLICY_FULL_VALUE		_T("full")
#define XML_DEPLOYPOLICY_ADDON_VALUE	_T("addon")



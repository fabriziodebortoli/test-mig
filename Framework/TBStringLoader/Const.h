
#pragma once

#define SERIALIZATION_VERSION	5
#define DICTIONARY_VERSION		1

	
#define DICTIONARY_LRU_CACHE_SIZE	30

#define RESOURCE_NODE		_T("resource")
#define DICTIONARY_NODE		_T("dictionary")

#define STRINGS_NODE		_T("Strings")
#define STRING_NODE			_T("String")

#define EXPECTED_ATTRIBUTE	_T("expectedSize")
#define ACTUAL_ATTRIBUTE	_T("actualSize")
#define DEVIATION_ATTRIBUTE	_T("deviation")

#define ID_ATTRIBUTE		_T("id")
#define NAME_ATTRIBUTE		_T("name") 
#define URL_ATTRIBUTE		_T("url")
#define BASE_ATTRIBUTE		_T("base")
#define TARGET_ATTRIBUTE	_T("target")
#define X_ATTRIBUTE			_T("x")
#define Y_ATTRIBUTE			_T("y") 
#define H_ATTRIBUTE			_T("height")
#define W_ATTRIBUTE			_T("width")
#define FONTNAME_ATTRIBUTE	_T("fontname")
#define FONTSIZE_ATTRIBUTE	_T("fontsize")
#define FONTBOLD_ATTRIBUTE  _T("bold")
#define FONTITALIC_ATTRIBUTE _T("italic") 
#define CURRENT_ATTRIBUTE	_T("current") 
#define STRUCTURE_NODE		_T("structure") 
#define VALID_ATTRIBUTE		_T("valid")

#define XML_TRUE			_T("true")
#define XML_FALSE			_T("false")

#define SLASH_CHAR					_T('\\')
#define OTHER_FOLDER				_T("other")
#define RESOURCE_INDEX_FILE 		_T("ResourceIndex.xml")

#define SOURCE_STRINGS_FILE			_T("sources.xml")
#define SOURCE_STRINGS_FOLDER		_T("source")
#define SOURCE_STRING_IDENTIFIER	_T("sources")

#define DATABASE_STRINGS_IDENTIFIER	_T("scripts")
#define DATABASE_STRINGS_FILE		_T("scripts.xml")
#define DATABASE_STRINGS_FOLDER 	_T("databasescript")

#define ENUM_STRINGS_IDENTIFIER		_T("enums")
#define ENUM_STRINGS_FILE			_T("enums.xml")
#define ENUM_STRINGS_FOLDER			OTHER_FOLDER

#define FORMATTER_STRINGS_FILE		_T("formats.xml")
#define FORMATTER_STRINGS_NAME		_T("formats")
#define FORMATTER_STRINGS_FOLDER	OTHER_FOLDER

#define FONT_STRINGS_FILE			_T("fonts.xml")
#define FONT_STRINGS_NAME			_T("fonts")
#define FONT_STRINGS_FOLDER			OTHER_FOLDER

#define LOAD_FAILED			_T("Failed to load %s.")


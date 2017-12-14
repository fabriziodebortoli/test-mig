
#pragma once

#include "tbstrings.h"

//includere alla fine degli include del .H
#include "beginh.dex"


//-----------------------------------------------------------------------------------------------------------
BEGIN_TB_STRING_MAP(FileExtension)
	/////////////////////////////////////////////////////////////////////////////////////////////////////////
	//costanti non soggette a traduzione
	/////////////////////////////////////////////////////////////////////////////////////////////////////////
	TB_STANDARD(BMP_EXT, "bmp")
	TB_STANDARD(TBF_EXT, "tbf")
	TB_STANDARD(WRM_EXT, "wrm")
	TB_STANDARD(WRMT_EXT, "wrmt")
	TB_STANDARD(BAK_EXT, "bak")
	TB_STANDARD(TMP_EXT, "$$$")
	TB_STANDARD(RDE_EXT, "rde")
	TB_STANDARD(TXT_EXT, "txt")
	TB_STANDARD(EMF_EXT, "emf")
	TB_STANDARD(HLP_EXT, "chm")
	TB_STANDARD(CSV_EXT, "txt")
	TB_STANDARD(EXCEL_EXT, "xls")
	TB_STANDARD(EXCELNET_EXT, "xlsx")
	TB_STANDARD(WORD_EXT, "doc")
	TB_STANDARD(WORDNET_EXT, "docx")
	TB_STANDARD(DIF_EXT, "dif")
	TB_STANDARD(PDF_EXT, "pdf")
	TB_STANDARD(RTF_EXT, "rtf")
	TB_STANDARD(ODS_EXT, "ods")
	TB_STANDARD(ODT_EXT, "odt")
	TB_STANDARD(HTML_EXT, "html")
	TB_STANDARD(XML_EXT, "xml")
	TB_STANDARD(XSL_EXT, "xsl")
	TB_STANDARD(JSON_EXT, "json")

	TB_STANDARD(ANY_WRM, "*.wrm")
	TB_STANDARD(ANY_WRMT, "*.wrmt")
	TB_STANDARD(ANY_BMP, "*.bmp;*.gif;*.exif;*.jpg;*.png;*.tiff;*.tif")
	TB_STANDARD(ANY_RDE, "*.rde")
	TB_STANDARD(ANY_WRMRDE, "*.wrm;*.rde")
	TB_STANDARD(ANY_TXT, "*.txt")
	TB_STANDARD(ANY_EMF, "*.emf")
	TB_STANDARD(ANY_HLP, "*.hlp;*.html;*.htm;*.mht;*.chm")
	TB_STANDARD(ANY_CSV, "*.txt;*.csv;*.tdt")
	TB_STANDARD(ANY_EXCEL, "*.xls;.xlt;*.xlsx")
	TB_STANDARD(ANY_EXCELNET, "*.xlsx;*.xltx;")
	TB_STANDARD(ANY_WORD, "*.doc;*.dot;*.docx")
	TB_STANDARD(ANY_WORDNET, "*.docx;*.dotx;*.doc;*.dot")
	TB_STANDARD(ANY_DIF,  "*.dif")
	TB_STANDARD(ANY_PDF,  "*.pdf")
	TB_STANDARD(ANY_RTF,  "*.rtf")
	TB_STANDARD(ANY_ODS, "*.ods")
	TB_STANDARD(ANY_ODT, "*.odt")
	TB_STANDARD(ANY_HTML, "*.htm;*.html;*.mht")
	TB_STANDARD(ANY_XML,  "*.xml")
	TB_STANDARD(ANY_XSL,  "*.xsl")
	TB_STANDARD(ANY_JSON, "*.json")

	/////////////////////////////////////////////////////////////////////////////////////////////////////////
	// costanti parzialmente oggetto di possibile traduzione
	// 
	/////////////////////////////////////////////////////////////////////////////////////////////////////////
	TB_GENERIC(WRM_FILTER, _TB("Woorm Report")		+ _T(" (*.wrm)|*.wrm||"))
	TB_GENERIC(BMP_FILTER, _TB("Images")				+ _T(" (*.bmp;*.gif;*.exif;*.jpg;*.png;*.tiff;*.tif)|*.bmp;*.gif;*.exif;*.jpg;*.png;*.tiff;*.tif||"))
	TB_GENERIC(TXT_FILTER, _TB("Text file")			+ _T(" (*.txt)|*.txt||"))
	TB_GENERIC(WRMT_FILTER, _TB("Woorm template")		+ _T(" (*.wrmt)|*.wrmt||"))
	TB_GENERIC(RDE_FILTER, _TB("Woorm with data")		+ _T(" (*.rde)|*.rde||"))
	TB_GENERIC(WRMRDE_FILTER, _TB("Woorm Report")	+ _T(" (*.wrm)|*.wrm|") + _TB("Woorm with data")+ _T(" (*.rde)|*.rde|") + _TB("Woorm template")		+ _T(" (*.wrmt)|*.wrmt||"))
	TB_GENERIC(EMF_FILTER, _TB("Enhanced MetaFiles")	+ _T(" (*.emf)|*.emf||"))
	TB_GENERIC(HLP_FILTER, _TB("Help File")			+ _T(" (*.hlp)|*.hlp|File di Help (*.html)|*.html|File di Help (*.htm)|*.htm||"))
	TB_GENERIC(CSV_FILTER, _TB("Text file")			+ _T(" (*.txt;*.csv;*.tdt)|*.txt;*.csv;*.tdt||"))
	TB_GENERIC(EXCEL_FILTER,_TB("Excel File")			+ _T(" (*.xls)|*.xls|*.xlt|*.xlsx|*.xltx||"))
	TB_GENERIC(EXCELNET_FILTER,_TB("ExcelNet File")			+ _T(" (*.xlsx)|*.xlsx|*.xltx||"))
	TB_GENERIC(WORD_FILTER, _TB("Word File ")			+ _T(" (*.doc)|*.doc|*.dot|*.docx|*.dotx||"))
	TB_GENERIC(WORDNET_FILTER, _TB("WordNet File ")			+ _T(" (*.docx)*.docx|*.dotx||"))
	TB_GENERIC(DIF_FILTER, _TB("DIF File")	+ _T(" (*.dif)|*.dif||"))
	TB_GENERIC(PDF_FILTER, _TB("Portable Document Format") + _T(" (*.pdf)|*.pdf||"))
	TB_GENERIC(RTF_FILTER, _TB("RTF Document Format") + _T(" (*.rtf)|*.rtf||"))
	TB_GENERIC(ODS_FILTER, _TB("Open Office Document Format") + _T(" (*.ods)|*.ods||"))
	TB_GENERIC(ODT_FILTER, _TB("Open Office Document Format") + _T(" (*.odt)|*.odt||"))
	TB_GENERIC(HTML_FILTER,_TB("HTML File")				+ _T(" (*.htm; *.html )| *.htm;*.html||"))
	TB_GENERIC(XML_FILTER,_TB("XML File")				+ _T(" (*.xml; )|*.xml||"))
	TB_GENERIC(TBJSON_FILTER, _TB("TBJSON File") + _T(" (*.tbjson; )|*.tbjson||"))
	TB_GENERIC(JSON_FILTER, _TB("JSON File") + _T(" (*.json; )|*.json||"))
END_TB_STRING_MAP()


//-----------------------------------------------------------------------------------------------------------
BEGIN_TB_STRING_MAP(OSLErrors)
	TB_LOCALIZED(MISSING_GRANT, "The Integrated Security system detected that you do not have sufficient privileges to use the required function: please contact the Security Administrator to obtain them {0-%s}");
END_TB_STRING_MAP()

#include "endh.dex"


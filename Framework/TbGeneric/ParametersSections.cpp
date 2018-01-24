#include "stdafx.h"

#include <TbNamesolver\ApplicationContext.h>
#include "ParametersSections.h"
#include "SettingsTable.h"
#include "DataObj.h"
#include "GeneralFunctions.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

const TCHAR szTbDefaultSettingFileName[] = _T("Settings.config");

//==============================================================================
const TCHAR szTbGenericNamespace[]	= _T("Module.Framework.TbGeneric");
const CTBNamespace snsTbGeneric		= szTbGenericNamespace;
//==============================================================================

//.RdeProtocol........................................................
const TCHAR szRdeProtocol[]			= _T("RdeProtocol");
const TCHAR szRdeBuffered[]			= _T("RdeBuffered");
const TCHAR szRdeBufferSize[]		= _T("RdeBufferSize");

//==============================================================================
const TCHAR szTbGenlibNamespace[]	= _T("Module.Framework.TbGenlib");
const CTBNamespace snsTbGenlib		= szTbGenlibNamespace;
//==============================================================================

//.Forms.............................................................. 
const TCHAR szFormsSection[]				= _T("Forms");
const TCHAR szMaxComboBoxItems[]			= _T("MaxComboBoxItems");
const TCHAR szImmediateBrowsing[]			= _T("ImmediateBrowsing");
const TCHAR szDisplayBrowsingLimits[]		= _T("DisplayBrowsingLimits");
const TCHAR szHotlinkComboDefaultFields[]	= _T("HotlinkComboDefaultFields");
const TCHAR szUpdateTBFFiles[]				= _T("UpdateTBFFiles");
const TCHAR szAllowHotlinkOnQueryAskDialog[]= _T("AllowHotlinkOnQueryAskDialog");
const TCHAR szEnableCenterControls[]		= _T("EnableCenterControls");

//.Environment........................................................
const TCHAR szEnvironment[]					= _T("Environment");
const TCHAR szBarCodeType[]					= _T("BarCodeType");
const TCHAR szLoadLibrariesOnDemand[]		= _T("LoadLibrariesOnDemand");
const TCHAR szShowPrintSetup[]				= _T("ShowPrintSetup");
const TCHAR szSingleThread[]				= _T("SingleThreaded");
const TCHAR szEnableLockTrace[]				= _T("EnableLockTrace");
const TCHAR szUseGdiPlus[]					= _T("UseGdiPlus");
const TCHAR szLogExitInstance[]				= _T("LogExitInstance");
const TCHAR szEnableAssertionsInRelease[]	= _T("EnableAssertionsInRelease");
const TCHAR szDumpAssertionsIfNoCrash[]		= _T("DumpAssertionsIfNoCrash");
const TCHAR szExpect100Continue[]			= _T("Expect100Continue");

const TCHAR szLogDiagnosticInEventViewerUnattendedMode[] = _T("LogDiagnosticInEventViewerUnattendedMode");

//.Data Type Epsilon .................................................
TCHAR szDataTypeEpsilon[]					= _T("Data Type Epsilons");
TCHAR szDataDblEpsilon[]					= _T("Double Decimals");
TCHAR szDataMonEpsilon[]					= _T("Monetary Decimals");
TCHAR szDataPercEpsilon[]					= _T("Percentage Decimals");
TCHAR szDataQuantityEpsilon[]				= _T("Quantity Decimals");

//.Preference........................................................
const TCHAR szPreferenceSection[]			= _T("Preference");
const TCHAR szUseLargeToolBarButtons[]		= _T("UseLargeToolBarButtons");
const TCHAR szUseWoormRadar[]				= _T("UseWoormRadar");
const TCHAR szRepeatableNew[]				= _T("RepeatableNew");
const TCHAR szBackgroudColor[]				= _T("BackgroundColor");
const TCHAR szUseXPStyle[]					= _T("UseXPStyle");
const TCHAR szAlternateColor[]				= _T("AlternateColor");
const TCHAR szCurrentRowColor[]				= _T("CurrentRowColor");
const TCHAR szSepLineColor[]				= _T("SepLineColor");

const TCHAR szShowZerosInInput[]			= _T("ShowZerosInInput");
const TCHAR szTreatEnterAsTab[]				= _T("TreatEnterAsTab");
const TCHAR szShowColoredControlFocused[]	= _T("ShowColoredControlFocused");
const TCHAR szControlFocusedColor[]			= _T("ControlFocusedColor");
const TCHAR szUseEasyReading[]				= _T("UseEasyReading"); 
const TCHAR szUseEasyBrowsing[]				= _T("UseEasyBrowsing"); 
const TCHAR szUseEasyBrowsingInEasyLookOnly[]= _T("UseEasyBrowsingInEasyLookOnly"); 
const TCHAR szFindSlave[]					= _T("EnableFindOnSlaveFields"); 
const TCHAR szUpdateDefaultReport[]			= _T("UpdateDefaultReport"); 
const TCHAR szShowAdminCustomSaveDialog[]	= _T("ShowAdminCustomSaveDialog");
const TCHAR szUseDocPerformanceAnalyzer[]	= _T("UseDocPerformanceAnalyzer");

const TCHAR szAllowBodyeditColumnHeaderSmallFont[]  = _T("AllowBodyeditColumnHeaderSmallFont"); 
const TCHAR szAddBodyeditColumnHeaderExtraSpace[]	= _T("AddBodyeditColumnHeaderExtraSpace"); 

const TCHAR szDataTipDelay[]						= _T("DataTipDelay"); 
const TCHAR szDataTipLevelOnBodyedit[]				= _T("DataTipLevelOnBodyedit"); 
const TCHAR szDataTipMaxWidth[]						= _T("DataTipMaxWidth"); 
const TCHAR szDataTipMaxHeight[]					= _T("DataTipMaxHeight"); 

const TCHAR szMultiSelTextColor[]					= _T("MultiSelTextColor"); 
const TCHAR szMultiSelBackColor[]					= _T("MultiSelBackColor"); 

//.Culture........................................................
const TCHAR szCultureSection[]						= _T("Culture");
const TCHAR szUpperLimit[]							= _T("UpperLimit");
const TCHAR szLowerLimit[]							= _T("LowerLimit");
const TCHAR szUseVCenterBottomAlignInWoormFields[]	= _T("UseVCenterBottomAlignInWoormFields");
const TCHAR szSizeOfDescriptionFont[]				= _T("SizeOfDescripionFont");
const TCHAR szCharSetSample[]						= _T("CharSetSample");

const TCHAR szFormFontFace[]						= _T("FormFontFace");
const TCHAR szControlsFont[]						= _T("ControlsFont");
const TCHAR szTileDialogTitle[]						= _T("TileDialogTitle");
const TCHAR szWizardStepper[]						= _T("WizardStepper");
const TCHAR szTileStrip[]							= _T("TileStrip");
const TCHAR szFilterTileTitle[]						= _T("FilterTileTitle");
const TCHAR szStaticWithLineFont[]					= _T("StaticWithLineFont");

const TCHAR szRadarSearchFont[]						= _T("RadarSearchFont");

//==============================================================================
const CTBNamespace snsTbGes = _T("Module.Framework.TbGes");
//==============================================================================

//==============================================================================
const CTBNamespace snsTbOleDb				= _T("Module.Framework.TbOleDb");
//==============================================================================

//.Connection............................................................
const TCHAR szConnectionSection[]				= _T("Connection");
const TCHAR szDebugSqlTrace[]					= _T("DebugSqlTrace");
const TCHAR szEnableEventViewerLog[]			= _T("EnableEventViewerLog");

//.Caching............................................................
const TCHAR szDataCaching[]					= _T("Caching");
const TCHAR szOptimizeHotLinkQuery[]		= _T("OptimizeHotLinkQuery");
const TCHAR szDataCachingEnable[]			= _T("CachingEnable");
const TCHAR szDataCacheScope[]				= _T("CacheScope");
const TCHAR szDataCacheExpirationSeconds[]	= _T("CacheExpirationSeconds");
const TCHAR szDataCacheMaxKbSize[]			= _T("CacheMaxKbSize");
const TCHAR szDataCacheReductionPerc[]		= _T("CacheReductionPerc");
const TCHAR szDataCacheCheckSeconds[]		= _T("CacheCheckSeconds");

//.Recovery System............................................................
const TCHAR szRecoverySystem[]					= _T("RecoverySystem");
const TCHAR szRecoverySystemEnable[]			= _T("Enabled");
const TCHAR szRecoverySystemLevel[]				= _T("RecoveryLevel");
const TCHAR szRecoverySystemRetries[]			= _T("RecoveryRetries");
const TCHAR szRecoverySystemRetriesInterval[]	= _T("RecoveryRetriesInterval");

//.LockManager........................................................
const TCHAR szLockManager[]				= _T("LockManager");
const TCHAR szUseOptimisticLock[]			= _T("UseOptimisticLock");
const TCHAR szDisableLockRetry[]		= _T("DisableLockRetry");
const TCHAR szDisableBeep[]				= _T("DisableBeep");
const TCHAR szDisableBatchBeep[]		= _T("DisableBatchBeep");
const TCHAR szDisableBatchLockRetry[]	= _T("DisableBatchLockRetry");
const TCHAR szMaxBatchLockRetry[]		= _T("MaxBatchLockRetry");
const TCHAR szMaxBatchLockTime[]		= _T("MaxBatchLockTime");
const TCHAR szMaxLockRetry[]			= _T("MaxLockRetry");
const TCHAR szMaxLockTime[]				= _T("MaxLockTime");
const TCHAR szUseLockManager[]			= _T("UseLockManager");
const TCHAR szMaxReportLockRetry[]		= _T("MaxReportLockRetry");
const TCHAR szMaxReportLockTime[]		= _T("MaxReportLockTime");

//.PerformanceAnalizer................................................
const TCHAR szPerformanceAnalizer[]	= _T("Performance Analyzer"); 
const TCHAR szAnalizeDocPerformance[]  = _T("AnalyzeDocPerformance");


//==============================================================================
const CTBNamespace snsTbWoormViewer			= _T("Module.Framework.TbWoormViewer");
const TCHAR szTbWoormViewerSettingFileName[]= _T("Woorm.config");
//.Barcode2D..........................................................
const TCHAR szBarcode2DFileName[] = _T("Barcode2D.config");
//==============================================================================

//.WoormGeneralOptions................................................
const TCHAR szWoormGeneralOptions[]		= _T("WoormGeneralOptions");
const TCHAR szRectHRatio[]				= _T("RectHRatio");
const TCHAR szRectVRatio[]				= _T("RectVRatio");
const TCHAR szGridX[]					= _T("GridX");
const TCHAR szGridY[]					= _T("GridY");
const TCHAR szSortGap[]					= _T("SortGap");
const TCHAR szDefaultTableRows[]		= _T("DefaultTableRows");
const TCHAR szIncludeTotal[]			= _T("IncludeTotal");
const TCHAR szMouseSensibility[]		= _T("MouseSensibility");
const TCHAR szAddColumnBefore[]			= _T("AddColumnBefore");
const TCHAR szAutoColumnTotal[]			= _T("AutoColumnTotal");
const TCHAR szTotalOnNewPage[]			= _T("TotalOnNewPage");
const TCHAR szResetOnNewPage[]			= _T("ResetOnNewPage");
const TCHAR szAlwaysHidden[]			= _T("AlwaysHidden");
const TCHAR szDoBakupFile[]				= _T("DoBakupFile");
const TCHAR szEnableTrackCross[]		= _T("EnableTrackCross");
const TCHAR szTrackLineColor[]			= _T("TrackLineColor");
const TCHAR szTrackLineSize[]			= _T("TrackLineSize");
const TCHAR szTrackLineStyle[]			= _T("TrackLineStyle");
const TCHAR szEnableNewObjectSelection[]= _T("EnableNewObjectSelection");
const TCHAR szObjectSelectionColor[]	= _T("ObjectSelectionColor");
const TCHAR szObjectSelectionSize[]		= _T("ObjectSelectionSize");
const TCHAR szObjectSelectionStyle[]	= _T("ObjectSelectionStyle");
const TCHAR szTrackInside[]				= _T("TrackInside");
const TCHAR szShowGrid[]				= _T("ShowGrid");
const TCHAR szLineGrid[]				= _T("LineGrid");
const TCHAR szGridColor[]				= _T("GridColor");
const TCHAR szHiddenBorderColor[]		= _T("HiddenBorderColor");
const TCHAR szHiddenBorderStyle[]		= _T("HiddenBorderStyle");
const TCHAR szSnapToGrid[]				= _T("SnapToGrid");
const TCHAR szSizeInGridUnits[]			= _T("SizeInGridUnits");
const TCHAR szShowPrintableArea[]		= _T("ShowPrintableArea");
const TCHAR szTransparentCreate[]		= _T("TransparentCreate");
const TCHAR szNoBorderCreate[]			= _T("NoBorderCreate");
const TCHAR szNoLabelCreate[]			= _T("NoLabelCreate");
const TCHAR szNoShowMargins[]			= _T("NoShowMargins");
const TCHAR szSortObjects[]				= _T("SortObjects");
const TCHAR szBottomAlign[]				= _T("BottomAlign");
const TCHAR szUseDocProp[]				= _T("UseDocProp");
const TCHAR szToClipboard[]				= _T("ToClipboard");
const TCHAR szUndoLevel[]				= _T("UndoLevel");
const TCHAR szShow[]					= _T("Show");
const TCHAR szCopyType[]				= _T("CopyType");
const TCHAR szOptimizedLineBreak[]		= _T("OptimizedLineBreak");
const TCHAR szColumnWidthPercentage[]	= _T("ColumnWidthPercentage");
const TCHAR szCheckBarcodeSize[]		= _T("CheckBarcodeSize");
const TCHAR szForceVerticalAlignLabelRelative[] = _T("ForceVerticalAlignLabelRelative");
const TCHAR szShowAdvancedForms[]		= _T("ShowAdvancedForms");
const TCHAR szShowReportTree[]			= _T("ShowReportTree");
const TCHAR szShowAllToolbars[]			= _T("ShowAllToolbars");
const TCHAR szTimeAutoSave[]			= _T("TimeAutoSave");

//.DraftPrint.........................................................
const TCHAR szDraftPrint[]		= _T("DraftPrint");
const TCHAR szCharSet[]			= _T("CharSet");
const TCHAR szOutPrecision[]	= _T("OutPrecision");
const TCHAR szClipPrecision[]	= _T("ClipPrecision");
const TCHAR szQuality[]			= _T("Quality");
const TCHAR szPitchAndFamily[]	= _T("PitchAndFamily");
const TCHAR szFaceName[]		= _T("FaceName");

//.WoormRunningOptions.........................................................
const TCHAR szWoormRunningOptions[] = _T("WoormRunningOptions");
const TCHAR szUseMultithreading[] = _T("UseMultithreading");

//
//.Default Barcode 2D parameters...............................................
const TCHAR szDefaultBarcode2DEncoding[] = _T("DefaultBarcode2DEncoding");
const TCHAR szDefaultBarcode2DVersion[] = _T("DefaultBarcode2DVersion");
const TCHAR szDefaultBarcode2DErrCorrLevel[] = _T("DefaultBarcode2DErrCorrLevel");
const TCHAR* szBracodeTypes[] =
{
	_T("UPCA"),
	_T("UPCE"),
	_T("EAN13"),
	_T("EAN8"),
	_T("CODE39"),
	_T("EXT39"),
	_T("INT25"),
	_T("CODE128"),
	_T("CODABAR"),
	_T("ZIP"),
	_T("MSIPLESSEY"),
	_T("CODE93"),
	_T("EXT93"),
	_T("UCC128"),
	_T("HIBC"),
	_T("PDF417"),
	_T("UPCE0"),
	_T("UPCE1"),
	_T("CODE128A"),
	_T("CODE128B"),
	_T("CODE128C"),
	_T("EAN128"),
	_T("DataMatrix"),
	_T("MicroQR"),
	_T("QR")

};

//==============================================================================
const TCHAR szMailConnectorConfigFile[]		= _T("MailConnector.config");

//.MailConnector......................................................
static const TCHAR szMailConnector[]				= _T("MailConnector");
static const TCHAR szUseMapi[]						= _T("UseMAPI");
static const TCHAR szUseSmtp[]						= _T("UseSMTP");
static const TCHAR szOutlookProfile[]				= _T("OutlookProfile");
static const TCHAR szMailCompress[]					= _T("MailCompress");
static const TCHAR szSupportOutlookExpress[]		= _T("SupportOutlookExpress");
static const TCHAR szCryptFile[]					= _T("CryptFile");
static const TCHAR szCryptingType[]					= _T("CryptingType");
static const TCHAR szPassword[]						= _T("Password");
static const TCHAR szRequestDeliveryNotifications[] = _T("RequestDeliveryNotifications");
static const TCHAR szRequestReadNotifications[]		= _T("RequestReadNotifications");
static const TCHAR szReplyToAddress[]				= _T("ReplyToAddress");
static const TCHAR szRedirectToAddress[]			= _T("RedirectToAddress");
static const TCHAR szTrackingAddressForSentEmails[]	= _T("TrackingAddressForSentEmails");
static const TCHAR szPrinterTemplate[]				= _T("PrinterTemplate");
static const TCHAR szFAXFormatTemplate[]			= _T("FaxFormatTemplate");
static const TCHAR szPdfSplitPages[]				= _T("PdfSplitPages");


//=============================================================================        
//						Sezioni di Parametri
//=============================================================================        

//-----------------------------------------------------------------------------
MailConnectorParams::MailConnectorParams ()
	: 
	TbBaseSettings(_T("Module.Extensions.TbMailer"), szMailConnectorConfigFile)
{
}

//-----------------------------------------------------------------------------
CString MailConnectorParams::GetOutlookProfile () 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, szMailConnector, szOutlookProfile, DataStr(), szMailConnectorConfigFile);

	if (!pSetting || pSetting->GetDataType() != DataType::String)
		return _T("");

	return pSetting->Str();
}

//-----------------------------------------------------------------------------
void MailConnectorParams::SetOutlookProfile (CString s) 
{
	AfxSetSettingValue(m_Owner, szMailConnector, szOutlookProfile, DataStr(s), szMailConnectorConfigFile);
}

//-----------------------------------------------------------------------------
BOOL MailConnectorParams::GetUseSmtp() 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, szMailConnector, szUseSmtp, DataBool(FALSE));

	if (!pSetting) 
		return FALSE;

	if (pSetting->GetDataType() == DataType::Bool)
		return *((DataBool*) pSetting);

	if (pSetting->GetDataType() == DataType::Integer)
		return *((DataInt*) pSetting);

	return FALSE;
}

//-----------------------------------------------------------------------------
void MailConnectorParams::SetUseSmtp (BOOL b) 
{
	AfxSetSettingValue(m_Owner, szMailConnector, szUseSmtp, DataBool(b), szMailConnectorConfigFile);
	AfxSetSettingValue(m_Owner, szMailConnector, szUseMapi, DataBool(!b), szMailConnectorConfigFile);
}

//-----------------------------------------------------------------------------
BOOL MailConnectorParams::GetUseMapi() 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, szMailConnector, szUseMapi, DataBool(FALSE));

	if (!pSetting) 
		return FALSE;

	if (pSetting->GetDataType() == DataType::Bool)
		return *((DataBool*) pSetting);

	if (pSetting->GetDataType() == DataType::Integer)
		return *((DataInt*) pSetting);

	return FALSE;
}

//-----------------------------------------------------------------------------
void MailConnectorParams::SetUseMapi (BOOL b) 
{
	AfxSetSettingValue(m_Owner, szMailConnector, szUseMapi, DataBool(b), szMailConnectorConfigFile);
	AfxSetSettingValue(m_Owner, szMailConnector, szUseSmtp, DataBool(!b), szMailConnectorConfigFile);
}

//-----------------------------------------------------------------------------
BOOL MailConnectorParams::GetRequestReadNotifications() 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, szMailConnector, szRequestReadNotifications, DataBool(FALSE));

	if (!pSetting || pSetting->GetDataType() != DataType::Bool)
		return FALSE;

	return *((DataBool*) pSetting);
}

//-----------------------------------------------------------------------------
void MailConnectorParams::SetRequestReadNotifications (BOOL b) 
{
	AfxSetSettingValue(m_Owner, szMailConnector, szRequestReadNotifications, DataBool(b), szMailConnectorConfigFile);
}

//-----------------------------------------------------------------------------
BOOL MailConnectorParams::GetRequestDeliveryNotifications() 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, szMailConnector, szRequestDeliveryNotifications, DataBool(FALSE));

	if (!pSetting || pSetting->GetDataType() != DataType::Bool)
		return FALSE;

	return *((DataBool*) pSetting);
}


//-----------------------------------------------------------------------------
void MailConnectorParams::SetRequestDeliveryNotifications (BOOL b) 
{
	AfxSetSettingValue(m_Owner, szMailConnector, szRequestDeliveryNotifications, DataBool(b), szMailConnectorConfigFile);
}

//-----------------------------------------------------------------------------
BOOL MailConnectorParams::GetMailCompress() 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, szMailConnector, szMailCompress, DataBool(TRUE));

	if (!pSetting || pSetting->GetDataType() != DataType::Bool)
		return TRUE;

	return *((DataBool*) pSetting);
}

//-----------------------------------------------------------------------------
void MailConnectorParams::SetMailCompress (BOOL b) 
{
	AfxSetSettingValue(m_Owner, szMailConnector, szMailCompress, DataBool(b), szMailConnectorConfigFile);
}

//-----------------------------------------------------------------------------
BOOL MailConnectorParams::GetSupportOutlookExpress() 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, szMailConnector, szSupportOutlookExpress, DataBool(FALSE));

	if (!pSetting || pSetting->GetDataType() != DataType::Bool)
		return FALSE;

	return *((DataBool*) pSetting);
}

//-----------------------------------------------------------------------------
void MailConnectorParams::SetSupportOutlookExpress (BOOL b) 
{
	AfxSetSettingValue(m_Owner, szMailConnector, szSupportOutlookExpress, DataBool(b), szMailConnectorConfigFile);
}

//-----------------------------------------------------------------------------
BOOL MailConnectorParams::GetCryptFile() 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, szMailConnector, szCryptFile, DataBool(TRUE));

	if (!pSetting || pSetting->GetDataType() != DataType::Bool)
		return TRUE;

	return *((DataBool*) pSetting);
}

//-----------------------------------------------------------------------------
void MailConnectorParams::SetCryptFile (BOOL b) 
{
	AfxSetSettingValue(m_Owner, szMailConnector, szCryptFile, DataBool(b), szMailConnectorConfigFile);
}
//-----------------------------------------------------------------------------
CString MailConnectorParams::GetPassword() 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, szMailConnector, szPassword, DataStr());

	if (!pSetting || pSetting->GetDataType() != DataType::String)
		return _T("");

	return TbDecryptString(pSetting->Str());
}

//-----------------------------------------------------------------------------
void MailConnectorParams::SetPassword (CString s) 
{
	CString encrypted = TbCryptString(s);
	AfxSetSettingValue(m_Owner, szMailConnector, szPassword, DataStr(encrypted), szMailConnectorConfigFile);
}

//-----------------------------------------------------------------------------
CString MailConnectorParams::GetReplyToAddress() 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, szMailConnector, szReplyToAddress, DataStr());

	if (!pSetting || pSetting->GetDataType() != DataType::String)
		return _T("");

	return pSetting->Str();
}

//-----------------------------------------------------------------------------
void MailConnectorParams::SetReplyToAddress (CString s) 
{
	AfxSetSettingValue(m_Owner, szMailConnector, szReplyToAddress, DataStr(s), szMailConnectorConfigFile);
}
//-----------------------------------------------------------------------------
CString MailConnectorParams::GetRedirectToAddress() 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, szMailConnector, szRedirectToAddress, DataStr());

	if (!pSetting || pSetting->GetDataType() != DataType::String)
		return _T("");

	return pSetting->Str();
}

//-----------------------------------------------------------------------------
void MailConnectorParams::SetRedirectToAddress (CString s) 
{
	AfxSetSettingValue(m_Owner, szMailConnector, szRedirectToAddress, DataStr(s), szMailConnectorConfigFile);
}

//-----------------------------------------------------------------------------
int MailConnectorParams::GetPdfSplitPages()
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, szMailConnector, szPdfSplitPages, DataInt());

	if (!pSetting || pSetting->GetDataType() != DataType::Integer)
		return 1000;

	return  (int)*((DataInt*)pSetting);
}

//-----------------------------------------------------------------------------
void MailConnectorParams::SetPdfSplitPages(int n)
{
	AfxSetSettingValue(m_Owner, szMailConnector, szPdfSplitPages, DataInt(n), szMailConnectorConfigFile);
}

//-----------------------------------------------------------------------------
CString MailConnectorParams::GetTrackingAddressForSentEmails() 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, szMailConnector, szTrackingAddressForSentEmails, DataStr());

	if (!pSetting || pSetting->GetDataType() != DataType::String)
		return _T("");

	return pSetting->Str();
}

//-----------------------------------------------------------------------------
void MailConnectorParams::SetTrackingAddressForSentEmails (CString s) 
{
	AfxSetSettingValue(m_Owner, szMailConnector, szTrackingAddressForSentEmails, DataStr(s), szMailConnectorConfigFile);
}

//-----------------------------------------------------------------------------
CString MailConnectorParams::GetPrinterTemplate() 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, szMailConnector, szPrinterTemplate, DataStr());

	if (!pSetting || pSetting->GetDataType() != DataType::String)
		return _T("");

	return pSetting->Str();
}

//-----------------------------------------------------------------------------
void MailConnectorParams::SetPrinterTemplate (CString s) 
{
	AfxSetSettingValue(m_Owner, szMailConnector, szPrinterTemplate, DataStr(s), szMailConnectorConfigFile);
}

//-----------------------------------------------------------------------------
CString MailConnectorParams::GetFAXFormatTemplate() 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, szMailConnector, szFAXFormatTemplate, DataStr(_T("[FAX: ##]")));

	if (!pSetting || pSetting->GetDataType() != DataType::String)
		return _T("");

	return pSetting->Str();
}

//-----------------------------------------------------------------------------
void MailConnectorParams::SetFAXFormatTemplate (CString s) 
{
	AfxSetSettingValue(m_Owner, szMailConnector, szFAXFormatTemplate, DataStr(s), szMailConnectorConfigFile);
}

//=============================================================================        
//					Useful General Functions 
//=============================================================================        

//-----------------------------------------------------------------------------
BOOL IsEventViewerTraceEnabled ()
{
	DataObj* pSetting = NULL;
	if (AfxGetApplicationContext()->IsInUnattendedMode())
		pSetting = AfxGetSettingValue	(
											snsTbGenlib, 
											szEnvironment, 
											szLogDiagnosticInEventViewerUnattendedMode, 
											DataBool(TRUE), 
											szTbDefaultSettingFileName
										);
	
	// event viewer trace has been disabled
	if (!pSetting || pSetting->GetDataType() != DATA_BOOL_TYPE)
		return FALSE;

	return *((DataBool*) pSetting);
}

//.SmtpMailConnector......................................................
static const TCHAR szSmtpMailConnector[]	= _T("MailConnector-Smtp");
static const TCHAR szSmtpSettingsFile[]		= _T("Smtp.config");

static const TCHAR szUserName[]				= _T("UserName");
//static const TCHAR szPassword[]				= _T("Password");
static const TCHAR szHostName[]				= _T("HostName");
static const TCHAR szPort[]					= _T("Port");
static const TCHAR szHtmlEncoding[]			= _T("HtmlEncoding");
static const TCHAR szMimeEncoding[]			= _T("MimeEncoding");
static const TCHAR szFromName[]				= _T("FromName");
static const TCHAR szFromAddress[]			= _T("FromAddress");
static const TCHAR szReplyToName[]			= _T("ReplyToName");
//static const TCHAR szReplyToAddress[]		= _T("ReplyToAddress");
static const TCHAR szUseSSL[]				= _T("UseSSL");
static const TCHAR szUseImplicitSSL[]		= _T("UseImplicitSSL");
static const TCHAR szTimeout[]				= _T("Timeout");
static const TCHAR szAuthenticationType[]	= _T("AuthenticationType");
static const TCHAR szConfiguration[]		= _T("Configuration");

//=============================================================================        
//						SmtpMailConnectorParams
//=============================================================================        
//-----------------------------------------------------------------------------
SmtpMailConnectorParams::SmtpMailConnectorParams (LPCTSTR szCurrentSection/* = NULL*/)
	: 
	TbBaseSettings(_T("Module.Extensions.TbMailer"), szSmtpSettingsFile),
	m_sCurrentSection (szSmtpMailConnector)
{
	if (szCurrentSection && *szCurrentSection)
		m_sCurrentSection = szCurrentSection;
}

//-----------------------------------------------------------------------------
void SmtpMailConnectorParams::SetCurrentSection (LPCTSTR szCurrentSection/* = NULL*/) 
{
	if (szCurrentSection && *szCurrentSection)
		m_sCurrentSection = szCurrentSection;
	else
		m_sCurrentSection = szSmtpMailConnector;
}

//-----------------------------------------------------------------------------
BOOL SmtpMailConnectorParams::AddNewSection (LPCTSTR szSection) 
{
	ASSERT(szSection && *szSection);

	//TODO
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL SmtpMailConnectorParams::RemoveSection (LPCTSTR szSection) 
{
	ASSERT(szSection && *szSection);
	if (CString(szSection).CompareNoCase(szSmtpMailConnector) == 0)
		return FALSE;

	//TODO
	return FALSE;
}

//-----------------------------------------------------------------------------
void SmtpMailConnectorParams::EnumSections (CStringArray& arSections) 
{
	//TODO
}

//-----------------------------------------------------------------------------
CString SmtpMailConnectorParams::GetFromName () 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, m_sCurrentSection, szFromName, DataStr(), szSmtpSettingsFile);

	if (!pSetting || pSetting->GetDataType() != DataType::String)
		return _T("");

	return pSetting->Str();
}

//-----------------------------------------------------------------------------
CString SmtpMailConnectorParams::GetFromAddress () 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, m_sCurrentSection, szFromAddress, DataStr(), szSmtpSettingsFile);

	if (!pSetting || pSetting->GetDataType() != DataType::String)
		return _T("");

	return pSetting->Str();
}

//-----------------------------------------------------------------------------
CString SmtpMailConnectorParams::GetReplyToName () 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, m_sCurrentSection, szReplyToName, DataStr(), szSmtpSettingsFile);

	if (!pSetting || pSetting->GetDataType() != DataType::String)
		return _T("");

	return pSetting->Str();
}

//-----------------------------------------------------------------------------
CString SmtpMailConnectorParams::GetReplyToAddress () 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, m_sCurrentSection, szReplyToAddress, DataStr(), szSmtpSettingsFile);

	if (!pSetting || pSetting->GetDataType() != DataType::String)
		return _T("");

	return pSetting->Str();
}

//-----------------------------------------------------------------------------
int SmtpMailConnectorParams::GetPort() 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, m_sCurrentSection, szPort, DataLng(25), szSmtpSettingsFile);

	if (!pSetting || (pSetting->GetDataType() != DataType::Long && pSetting->GetDataType() != DataType::Integer))
		return 25;
	if (pSetting->GetDataType() == DataType::Integer)
		return *((DataInt*) pSetting);

	return *((DataLng*) pSetting);
}
//-----------------------------------------------------------------------------
void SmtpMailConnectorParams::SetPort (int n) 
{
	AfxSetSettingValue(m_Owner, m_sCurrentSection, szPort, DataLng(n), szSmtpSettingsFile);
}

//-----------------------------------------------------------------------------
int SmtpMailConnectorParams::GetTimeout() 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, m_sCurrentSection, szTimeout, DataLng(200000), szSmtpSettingsFile);

	if (!pSetting || (pSetting->GetDataType() != DataType::Long))
		return 200000;

	return *((DataLng*) pSetting);
}
//-----------------------------------------------------------------------------
void SmtpMailConnectorParams::SetTimeout (int n) 
{
	AfxSetSettingValue(m_Owner, m_sCurrentSection, szTimeout, DataLng(n), szSmtpSettingsFile);
}

//-----------------------------------------------------------------------------
BOOL SmtpMailConnectorParams::GetMimeEncoding() 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, m_sCurrentSection, szMimeEncoding, DataBool(FALSE), szSmtpSettingsFile);

	if (!pSetting || pSetting->GetDataType() != DataType::Bool)
		return FALSE;

	return *((DataBool*) pSetting);
}
//-----------------------------------------------------------------------------
void SmtpMailConnectorParams::SetMimeEncoding (BOOL b) 
{
	AfxSetSettingValue(m_Owner, m_sCurrentSection, szMimeEncoding, DataBool(b), szSmtpSettingsFile);
}

//-----------------------------------------------------------------------------
BOOL SmtpMailConnectorParams::GetHtmlEncoding() 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, m_sCurrentSection, szHtmlEncoding, DataBool(FALSE), szSmtpSettingsFile);

	if (!pSetting || pSetting->GetDataType() != DataType::Bool)
		return FALSE;

	return *((DataBool*) pSetting);
}
//-----------------------------------------------------------------------------
void SmtpMailConnectorParams::SetHtmlEncoding (BOOL b) 
{
	AfxSetSettingValue(m_Owner, m_sCurrentSection, szHtmlEncoding, DataBool(b), szSmtpSettingsFile);
}

//-----------------------------------------------------------------------------
CString SmtpMailConnectorParams::GetHostName () 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, m_sCurrentSection, szHostName, DataStr(), szSmtpSettingsFile);

	if (!pSetting || pSetting->GetDataType() != DataType::String)
		return _T("");

	return pSetting->Str();
}
//-----------------------------------------------------------------------------
void SmtpMailConnectorParams::SetHostName (const CString& s) 
{
	AfxSetSettingValue(m_Owner, m_sCurrentSection, szHostName, DataStr(s), szSmtpSettingsFile);
}

//-----------------------------------------------------------------------------
CString SmtpMailConnectorParams::GetUserName () 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, m_sCurrentSection, szUserName, DataStr(), szSmtpSettingsFile);

	if (!pSetting || pSetting->GetDataType() != DataType::String)
		return _T("");

	return pSetting->Str();
}
//-----------------------------------------------------------------------------
void SmtpMailConnectorParams::SetUserName (const CString& s) 
{
	AfxSetSettingValue(m_Owner, m_sCurrentSection, szUserName, DataStr(s), szSmtpSettingsFile);
}

//-----------------------------------------------------------------------------
CString SmtpMailConnectorParams::GetPassword () 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, m_sCurrentSection, szPassword, DataStr(), szSmtpSettingsFile);

	if (!pSetting || pSetting->GetDataType() != DataType::String)
		return _T("");

	return TbDecryptString(pSetting->Str());
}
//-----------------------------------------------------------------------------
void SmtpMailConnectorParams::SetPassword (const CString& s) 
{
	CString encrypted = TbCryptString(s);
	AfxSetSettingValue(m_Owner, m_sCurrentSection, szPassword, DataStr(encrypted), szSmtpSettingsFile);
}

//-----------------------------------------------------------------------------
void SmtpMailConnectorParams::SetFromName (const CString& s) 
{
	AfxSetSettingValue(m_Owner, m_sCurrentSection, szFromName, DataStr(s), szSmtpSettingsFile);
}

//-----------------------------------------------------------------------------
void SmtpMailConnectorParams::SetFromAddress (const CString& s) 
{
	AfxSetSettingValue(m_Owner, m_sCurrentSection, szFromAddress, DataStr(s), szSmtpSettingsFile);
}

//-----------------------------------------------------------------------------
void SmtpMailConnectorParams::SetReplyToName (const CString& s) 
{
	AfxSetSettingValue(m_Owner, m_sCurrentSection, szReplyToName, DataStr(s), szSmtpSettingsFile);
}

//-----------------------------------------------------------------------------
void SmtpMailConnectorParams::SetReplyToAddress (const CString& s) 
{
	AfxSetSettingValue(m_Owner, m_sCurrentSection, szReplyToAddress, DataStr(s), szSmtpSettingsFile);
}

//-----------------------------------------------------------------------------
BOOL SmtpMailConnectorParams::GetUseExplicitSSL() 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, m_sCurrentSection, szUseSSL, DataBool(FALSE), szSmtpSettingsFile);

	if (!pSetting || pSetting->GetDataType() != DataType::Bool)
		return FALSE;

	return *((DataBool*) pSetting);
}
//-----------------------------------------------------------------------------
void SmtpMailConnectorParams::SetUseExplicitSSL (BOOL b) 
{
	AfxSetSettingValue(m_Owner, m_sCurrentSection, szUseSSL, DataBool(b), szSmtpSettingsFile);
}

//-----------------------------------------------------------------------------
BOOL SmtpMailConnectorParams::GetUseImplicitSSL() 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, m_sCurrentSection, szUseImplicitSSL, DataBool(FALSE), szSmtpSettingsFile);

	if (!pSetting || pSetting->GetDataType() != DataType::Bool)
		return FALSE;

	return *((DataBool*) pSetting);
}
//-----------------------------------------------------------------------------
void SmtpMailConnectorParams::SetUseImplicitSSL (BOOL b) 
{
	AfxSetSettingValue(m_Owner, m_sCurrentSection, szUseImplicitSSL, DataBool(b), szSmtpSettingsFile);
}

//-----------------------------------------------------------------------------
short SmtpMailConnectorParams::GetConfiguration() 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, m_sCurrentSection, szConfiguration, DataInt(0), szSmtpSettingsFile);

	if (!pSetting || pSetting->GetDataType() != DataType::Integer)
		return FALSE;

	return *((DataInt*) pSetting);
}
//-----------------------------------------------------------------------------
void SmtpMailConnectorParams::SetConfiguration (short c) 
{
	AfxSetSettingValue(m_Owner, m_sCurrentSection, szConfiguration, DataInt(c), szSmtpSettingsFile);
}

//-----------------------------------------------------------------------------
CString SmtpMailConnectorParams::GetAuthenticationType () 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, m_sCurrentSection, szAuthenticationType, DataStr(_T("NTLM")), szSmtpSettingsFile);

	if (!pSetting || pSetting->GetDataType() != DataType::String)
		return _T("NTLM");

	return pSetting->Str();
}

//-----------------------------------------------------------------------------
void SmtpMailConnectorParams::SetAuthenticationType (const CString& s) 
{
	AfxSetSettingValue(m_Owner, m_sCurrentSection, szAuthenticationType, DataStr(s), szSmtpSettingsFile);
}

//=============================================================================

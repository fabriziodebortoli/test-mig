#pragma once

#include <TbNameSolver\TbNamespaces.h>

//includere alla fine degli include del .H
#include "beginh.dex"

extern const TB_EXPORT TCHAR szTbDefaultSettingFileName[];

//-------------------------------------------.TbGeneric.------------ @@TODO BAUZI (aspetto risposta da RICKY)
extern const TB_EXPORT TCHAR szTbGenericNamespace[];
extern const TB_EXPORT CTBNamespace snsTbGeneric;

extern const TB_EXPORT TCHAR szRdeProtocol[];				// RDE Protocol
extern const TB_EXPORT TCHAR szRdeBuffered[];
extern const TB_EXPORT TCHAR szRdeBufferSize[];

//-------------------------------------------.TbGenlib.------------
extern const TB_EXPORT TCHAR szTbGenlibNamespace[];
extern const TB_EXPORT CTBNamespace snsTbGenlib;

extern const TB_EXPORT TCHAR szEnvironment[];				// Environment
extern const TB_EXPORT TCHAR szManageCompanyDatabaseCulture[];
extern const TB_EXPORT TCHAR szEnableAssertionsInRelease[];
extern const TB_EXPORT TCHAR szDumpAssertionsIfNoCrash[];
extern const TB_EXPORT TCHAR szLogDiagnosticInEventViewerUnattendedMode[];
extern const TB_EXPORT TCHAR szSingleThreaded[];


extern const TB_EXPORT TCHAR szFormsSection[];				// Forms
extern const TB_EXPORT TCHAR szImmediateBrowsing[];
extern const TB_EXPORT TCHAR szDisplayBrowsingLimits[];
extern const TB_EXPORT TCHAR szHotlinkComboDefaultFields[];
extern const TB_EXPORT TCHAR szRadarEndForward[] ;
extern const TB_EXPORT TCHAR szUpdateTBFFiles[];
extern const TB_EXPORT TCHAR szTabbedDocuments[];
extern const TB_EXPORT TCHAR szChildManagement[];
extern const TB_EXPORT TCHAR szMaxComboBoxItems[];
extern const TB_EXPORT TCHAR szShowZerosInInput[];
extern const TB_EXPORT TCHAR szAllowBodyeditColumnHeaderSmallFont[];
extern const TB_EXPORT TCHAR szAddBodyeditColumnHeaderExtraSpace[];
extern const TB_EXPORT TCHAR szDataTipDelay[];
extern const TB_EXPORT TCHAR szDataTipLevelOnBodyedit[];
extern const TB_EXPORT TCHAR szDataTipMaxHeight[];
extern const TB_EXPORT TCHAR szDataTipMaxWidth[];
extern const TB_EXPORT TCHAR szTreatEnterAsTab[];
extern const TB_EXPORT TCHAR szEnableCenterControls[];

extern const TB_EXPORT TCHAR szEnableSuspendOnIdle[];

extern const TB_EXPORT TCHAR szPreferenceSection[];			// Preference
extern const TB_EXPORT TCHAR szShowAdminCustomSaveDialog[];
extern const TB_EXPORT TCHAR szAllActive[];
extern const TB_EXPORT TCHAR szUseWoormRadar[];
extern const TB_EXPORT TCHAR szRepeatableNew[];
extern const TB_EXPORT TCHAR szUseEasyBrowsing[];
extern const TB_EXPORT TCHAR szEnableFindOnSlaveFields[];
extern const TB_EXPORT TCHAR szTBLoaderDefaultSOAPPort[];


extern const TB_EXPORT TCHAR szDataTypeEpsilonSection[];			// DataTypeEpsilon 
extern const TB_EXPORT TCHAR szDoubleDecimals[];
extern const TB_EXPORT TCHAR szMonetaryDecimals[];
extern const TB_EXPORT TCHAR szPercentageDecimals[];
extern const TB_EXPORT TCHAR szQuantityDecimals[];

extern const TB_EXPORT TCHAR szReportSection[];		// Report
extern const TB_EXPORT TCHAR szShowPrintSetup[];
extern const TB_EXPORT TCHAR szUpdateDefaultReport[];


extern const TB_EXPORT TCHAR szDevelopmentSection[];		// Development
extern const TB_EXPORT TCHAR szEnableLockTrace[];
extern const TB_EXPORT TCHAR szLogExitInstance[];
extern const TB_EXPORT TCHAR szExpect100Continue[];
extern const TB_EXPORT TCHAR szChromeDebuggingPort[];
extern const TB_EXPORT TCHAR szRefreshDelay[];

extern const TB_EXPORT TCHAR szCultureSection[];			// Culture
extern const TB_EXPORT TCHAR szLowerLimit[];
extern const TB_EXPORT TCHAR szUpperLimit[];
extern const TB_EXPORT TCHAR szUseVCenterBottomAlignInWoormFields[];
extern const TB_EXPORT TCHAR szSizeOfDescriptionFont[];
extern const TB_EXPORT TCHAR szExcelDateFormat[];
extern const TB_EXPORT TCHAR szExcelTimeFormat[];
extern const TB_EXPORT TCHAR szExcelDateTimeFormat[];
extern const TB_EXPORT TCHAR szCharSetSample[];


extern const TB_EXPORT TCHAR szSchedulerSection[];			//Scheduler
extern const TB_EXPORT TCHAR szTaskIsolation[];

//-------------------------------------------.TbGes.------------
extern const TB_EXPORT CTBNamespace snsTbGes;

//-------------------------------------------.TbOleDb.------------

extern const TB_EXPORT CTBNamespace snsTbOleDb;
extern const TB_EXPORT TCHAR szConnectionSection[];
extern const TB_EXPORT TCHAR szDebugSqlTrace[];
extern const TB_EXPORT TCHAR szDebugSqlTraceActions[];
extern const TB_EXPORT TCHAR szDebugSqlTraceTables[];

extern const TB_EXPORT TCHAR szEnableEventViewerLog[];


extern const TB_EXPORT TCHAR szDataCaching[];					// Caching
extern const TB_EXPORT TCHAR szOptimizeHotLinkQuery[];
extern const TB_EXPORT TCHAR szDataCachingEnable[];
extern const TB_EXPORT TCHAR szDataCacheScope[];
extern const TB_EXPORT TCHAR szDataCacheExpirationSeconds[];
extern const TB_EXPORT TCHAR szDataCacheMaxKbSize[];
extern const TB_EXPORT TCHAR szDataCacheReductionPerc[];
extern const TB_EXPORT TCHAR szDataCacheCheckSeconds[];


extern const TB_EXPORT TCHAR szRecoverySystem[];				// Recovery System
extern const TB_EXPORT TCHAR szRecoverySystemEnable[];
extern const TB_EXPORT TCHAR szRecoverySystemLevel[];
extern const TB_EXPORT TCHAR szRecoverySystemRetries[];
extern const TB_EXPORT TCHAR szRecoverySystemRetriesInterval[];

extern const TB_EXPORT TCHAR szLockManager[];					// LockManager
extern const TB_EXPORT TCHAR szUseOptimisticLock[];
extern const TB_EXPORT TCHAR szDisableLockRetry[];
extern const TB_EXPORT TCHAR szDisableBeep[];
extern const TB_EXPORT TCHAR szDisableBatchBeep[];
extern const TB_EXPORT TCHAR szDisableBatchLockRetry[];
extern const TB_EXPORT TCHAR szMaxBatchLockRetry[];
extern const TB_EXPORT TCHAR szMaxBatchLockTime[];
extern const TB_EXPORT TCHAR szMaxLockRetry[];
extern const TB_EXPORT TCHAR szMaxLockTime[];
extern const TB_EXPORT TCHAR szUseLockManager[];
extern const TB_EXPORT TCHAR szMaxReportLockRetry[];
extern const TB_EXPORT TCHAR szMaxReportLockTime[];

extern const TB_EXPORT TCHAR szPerformanceAnalizer[];			// PerformanceAnalizer
extern const TB_EXPORT TCHAR szAnalizeDocPerformance[];


//-------------------------------------------.TbWoormViewer.------------
extern const TB_EXPORT CTBNamespace snsTbWoormViewer;

extern const TB_EXPORT TCHAR szTbWoormViewerSettingFileName[];
extern const TB_EXPORT TCHAR szWoormGeneralOptions[];			// WoormGeneralOptions
extern const TB_EXPORT TCHAR szRectHRatio[];
extern const TB_EXPORT TCHAR szRectVRatio[];
extern const TB_EXPORT TCHAR szGridX[];
extern const TB_EXPORT TCHAR szGridY[];
extern const TB_EXPORT TCHAR szSortGap[];
extern const TB_EXPORT TCHAR szDefaultTableRows[];
extern const TB_EXPORT TCHAR szIncludeTotal[];
extern const TB_EXPORT TCHAR szMouseSensibility[];
extern const TB_EXPORT TCHAR szAddColumnBefore[];
extern const TB_EXPORT TCHAR szAutoColumnTotal[];
extern const TB_EXPORT TCHAR szTotalOnNewPage[];
extern const TB_EXPORT TCHAR szResetOnNewPage[];
extern const TB_EXPORT TCHAR szAlwaysHidden[];
extern const TB_EXPORT TCHAR szDoBakupFile[];
extern const TB_EXPORT TCHAR szEnableTrackCross[];
extern const TB_EXPORT TCHAR szTrackLineColor[];
extern const TB_EXPORT TCHAR szTrackLineSize[];
extern const TB_EXPORT TCHAR szTrackLineStyle[];
extern const TB_EXPORT TCHAR szEnableNewObjectSelection[];
extern const TB_EXPORT TCHAR szObjectSelectionColor[];
extern const TB_EXPORT TCHAR szObjectSelectionSize[];
extern const TB_EXPORT TCHAR szObjectSelectionStyle[];
extern const TB_EXPORT TCHAR szTrackInside[];
extern const TB_EXPORT TCHAR szShowGrid[];
extern const TB_EXPORT TCHAR szLineGrid[];
extern const TB_EXPORT TCHAR szGridColor[];
extern const TB_EXPORT TCHAR szHiddenBorderColor[];
extern const TB_EXPORT TCHAR szHiddenBorderStyle[];
extern const TB_EXPORT TCHAR szSnapToGrid[];
extern const TB_EXPORT TCHAR szSizeInGridUnits[];
extern const TB_EXPORT TCHAR szShowPrintableArea[];
extern const TB_EXPORT TCHAR szTransparentCreate[];
extern const TB_EXPORT TCHAR szNoBorderCreate[];
extern const TB_EXPORT TCHAR szNoLabelCreate[];
extern const TB_EXPORT TCHAR szNoShowMargins[];
extern const TB_EXPORT TCHAR szSortObjects[];
extern const TB_EXPORT TCHAR szBottomAlign[];
extern const TB_EXPORT TCHAR szUseDocProp[];
extern const TB_EXPORT TCHAR szToClipboard[];
extern const TB_EXPORT TCHAR szUndoLevel[];
extern const TB_EXPORT TCHAR szShow[];
extern const TB_EXPORT TCHAR szCopyType[];
extern const TB_EXPORT TCHAR szTimeAutoSave[];

extern const TB_EXPORT TCHAR szOptimizedLineBreak[];
extern const TB_EXPORT TCHAR szColumnWidthPercentage[];
extern const TB_EXPORT TCHAR szForceVerticalAlignLabelRelative[];
extern const TB_EXPORT TCHAR szCheckBarcodeSize[];
extern const TB_EXPORT TCHAR szShowAdvancedForms[];
extern const TB_EXPORT TCHAR szShowReportTree[];
extern const TB_EXPORT TCHAR szShowAllToolbars[];

extern const TB_EXPORT TCHAR szDraftPrint[];					//	DraftPrint
extern const TB_EXPORT TCHAR szCharSet[];
extern const TB_EXPORT TCHAR szOutPrecision[];
extern const TB_EXPORT TCHAR szClipPrecision[];
extern const TB_EXPORT TCHAR szQuality[];
extern const TB_EXPORT TCHAR szPitchAndFamily[];
extern const TB_EXPORT TCHAR szFaceName[];


extern const TB_EXPORT TCHAR szWoormRunningOptions[];			// WoormRunningOptions
extern const TB_EXPORT TCHAR szUseMultithreading[];


//-------------------------------------------.Barcode.------------
extern const TB_EXPORT TCHAR szBarcodeConnectorConfigFile[];

extern const TB_EXPORT TCHAR 	szDefaultBarcodeSection[];
extern const TB_EXPORT TCHAR	szBarCodeType[];

extern const TB_EXPORT TCHAR 	szDefaultBarcode2DEncoding[];
extern const TB_EXPORT TCHAR*	szBarcodeTypes[];

extern const TB_EXPORT TCHAR 	szDefaultBarcode2DVersion[];
extern const TB_EXPORT TCHAR 	szDefaultBarcode2DErrCorrLevel[];
extern const TB_EXPORT TCHAR	szBarcode2DFileName[];

extern const TB_EXPORT TCHAR	szDataMatrix[];
extern const TB_EXPORT TCHAR	szMicroQR[];
extern const TB_EXPORT TCHAR	szQR[];
extern const TB_EXPORT TCHAR	szPDF417[];


//-------------------------------------------.TbMailer.------------

extern const TB_EXPORT TCHAR szMailConnectorConfigFile[];		



//Theme
extern const TB_EXPORT TCHAR szAlternateColor[];
extern const TB_EXPORT TCHAR szFormFontFace[];
extern const TB_EXPORT TCHAR szControlsFont[];
extern const TB_EXPORT TCHAR szTileDialogTitle[];
extern const TB_EXPORT TCHAR szWizardStepper[];
extern const TB_EXPORT TCHAR szRadarSearchFont[];
extern const TB_EXPORT TCHAR szTileStrip[];
extern const TB_EXPORT TCHAR szStaticWithLineFont[];

//=============================================================================        
//						TbBaseSettings
//============================================================================= 
class TB_EXPORT TbBaseSettings : public CObject
{
protected:
	CTBNamespace	m_Owner;
	CString			m_sFileName;
public:
	TbBaseSettings (LPCTSTR szNs, LPCTSTR szFilename) : m_Owner (szNs), m_sFileName(szFilename) {}

	CTBNamespace	GetOwner		() { return m_Owner;}
	CString			GetFileName		() { return m_sFileName; }	
};

// questo insieme di parametri viene sempre letto in blocco, quindi �
// comodo avere la sezione di lettura vecchio stile.
//=============================================================================        
class TB_EXPORT MailConnectorParams : public TbBaseSettings
{
public:
	MailConnectorParams ();

public:
	CString GetOutlookProfile		();
	void	SetOutlookProfile		(CString s);

	BOOL	GetUseMapi				();
	void	SetUseMapi				(BOOL b);
	BOOL	GetUseSmtp				();
	void	SetUseSmtp				(BOOL b);

	BOOL	GetRequestReadNotifications		();
	void	SetRequestReadNotifications		(BOOL b);

	BOOL	GetRequestDeliveryNotifications	();
	void	SetRequestDeliveryNotifications	(BOOL b);

	BOOL	GetMailCompress			();
	void	SetMailCompress			(BOOL b);

	BOOL	GetSupportOutlookExpress();	
	void	SetSupportOutlookExpress(BOOL b);	

	BOOL	GetCryptFile			();	
	void	SetCryptFile			(BOOL b);

	CString	GetPassword				();
	void	SetPassword				(CString s);

	CString	GetReplyToAddress		();
	void	SetReplyToAddress		(CString s);

	CString	GetRedirectToAddress	();
	void	SetRedirectToAddress	(CString s);

	CString	GetTrackingAddressForSentEmails		();
	void	SetTrackingAddressForSentEmails		(CString s);

	CString GetPrinterTemplate();
	void SetPrinterTemplate (CString s);

	CString GetFAXFormatTemplate();
	void SetFAXFormatTemplate (CString s);

	int		GetPdfSplitPages		();
	void	SetPdfSplitPages		(int n);
};

// questo insieme di parametri viene sempre letto in blocco, quindi �
// comodo avere la sezione di lettura vecchio stile.
//=============================================================================        
class TB_EXPORT SmtpMailConnectorParams : public TbBaseSettings
{
protected:
	CString	m_sCurrentSection;
public:
	enum ESecurityProtocolType { //System::Net::SecurityProtocolType
				SP_Default = 0, 
				SP_SSL3 = 48, SP_TLS10 = 192, SP_TLS11 = 768, SP_TLS12 = 3072, 
				SP_All = (SP_SSL3 | SP_TLS10 | SP_TLS11 | SP_TLS12)
	};
	static const int st_SecurityProtocolSize = 6;
	static LPCTSTR st_SecurityProtocolDescr[];
	static int st_SecurityProtocolValue[];

	SmtpMailConnectorParams (LPCTSTR szCurrentSection = NULL);
	
	const CString& GetCurrentSection () const { return m_sCurrentSection; }
	void SetCurrentSection (LPCTSTR szCurrentSection/* = NULL*/);
	
	BOOL AddNewSection (LPCTSTR szSection);
	void EnumSections (CStringArray& arSections);
	BOOL RemoveSection (LPCTSTR szSection);

	//----------------------------------
	CString GetFromName		();
	CString GetFromAddress	();

	CString GetReplyToName		();
	CString GetReplyToAddress	();

	CString GetHostName ();
	void	SetHostName (const CString&); 
	int		GetPort ();
	void	SetPort (int); 

	//CString GetBoundIP ();
	//void	SetBoundIP (const CString&); 

	CString GetUserName ();
	void	SetUserName (const CString&); 
	CString GetPassword ();
	void	SetPassword (const CString&); 

	//BOOL	GetAutoDial ();
	//void	SetAutoDial (BOOL); 

	// get functions in BaseSmtpMailConnectorParams class
	void	SetFromName (const CString&); 
	void	SetFromAddress (const CString&); 
	void	SetReplyToName (const CString&); 
	void	SetReplyToAddress (const CString&); 

	//int		GetPriorityType ();
	//void	SetPriorityType (int); 

	//CString GetEncodingCharset ();
	//void	SetEncodingCharset (const CString&); 

	//CString GetEncodingFriendly ();
	//void	SetEncodingFriendly (const CString&); 

	BOOL	GetMimeEncoding ();
	void	SetMimeEncoding (BOOL); 

	BOOL	GetHtmlEncoding ();
	void	SetHtmlEncoding (BOOL); 

	BOOL	GetUseExplicitSSL ();	//rename of GetUseSSL
	void	SetUseExplicitSSL (BOOL b);
	BOOL	GetUseImplicitSSL ();
	void	SetUseImplicitSSL (BOOL b);

	//BOOL	GetSupportedByExtendedMapi ();
	//void	SetSupportedByExtendedMapi (BOOL b);

	int		GetTimeout ();
	void	SetTimeout (int); 

	//int	GetAuthenticationType ();
	//void	SetAuthenticationType (int); 
	CString GetAuthenticationType ();
	void	SetAuthenticationType (const CString& s);

	int		GetSecurityProtocolType();
	void	SetSecurityProtocolType(int);

	short	GetConfiguration ();
	void	SetConfiguration (short);

	BOOL	UseSystemWebMail () { return GetUseImplicitSSL () || GetConfiguration () == 2; }
};

//=============================================================================        
TB_EXPORT BOOL IsEventViewerTraceEnabled ();

#include "endh.dex"

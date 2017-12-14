#pragma once

#include <tbges\extdoc.h>
#include <tbges\dbt.h>

#include "beginh.dex"

//////////////////////////////////////////////////////////////////////////////
//			       class CExtensionsToScanItemSource definition
//////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
class CExtensionsToScanItemSource : public CItemSource
{
	DECLARE_DYNCREATE(CExtensionsToScanItemSource)

public:
	CExtensionsToScanItemSource();
	~CExtensionsToScanItemSource();

private:
	CStringArray* m_pExtensionsList;

protected:
	virtual void GetData(DataArray& values, CStringArray& descriptions, CString strCurrentValue);
};

///////////////////////////////////////////////////////////////////////////////
//					class BDAcquisitionFromDevice							 //
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT BDAcquisitionFromDevice : public CAbstractFormDoc
{
	DECLARE_DYNCREATE(BDAcquisitionFromDevice)

public:
	BDAcquisitionFromDevice();
	~BDAcquisitionFromDevice();

private:
	gcroot<Microarea::TBPicComponents::TBPicImaging^>	tbPicImaging;
	gcroot<Microarea::TBPicComponents::TBPicPDF^>		tbPicPDF;
	gcroot<System::IntPtr>								dlgHandle;

	// variabili JSON
	DataStr		m_sSource;
	DataBool	m_bChangeSource;
	DataStr		m_sFile;
	DataStr		m_sExtension;
	DataBool	m_bSeparateFiles;
	DataBool	m_bSplitPage;
	DataBool	m_bSplitBarcode;
	//

	CString		m_sPDFImageName;
	DataInt		m_nTIFFID;
	DataInt		m_nTIFFImageCount;

	CAbstractFormDoc* m_pCallingDoc;

	BOOL m_bIsScanning;

public:
	CStringArray*	m_pAcquiredFiles;

private:
	void EnableControls	();
	void RunScan		();
	void InitSelections	();

	CTBPicViewerAdvWrapper::E_TBPICTURESTATUS SaveAsTIFF(int imageID, CString acquiredFilePath);
	CTBPicViewerAdvWrapper::E_TBPICTURESTATUS SaveAsPDF	(int imageID, CString acquiredFilePath);

protected:
	virtual BOOL OnAttachData	();
	virtual BOOL OnOpenDocument	(LPCTSTR);

public:
	//{{AFX_MSG(BDAcquisitionFromDevice)
	afx_msg void OnScanClick				();
	afx_msg void OnCancelClick				();
	afx_msg void OnChangeSourceClick		();
	afx_msg	void OnExtensionChanged			();
	afx_msg	void OnCreateSeparateFileChanged();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"

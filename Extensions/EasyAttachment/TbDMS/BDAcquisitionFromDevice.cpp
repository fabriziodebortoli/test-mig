#include "stdafx.h"

// local declarations
#include "TBRepositoryManager.h"
#include "CommonObjects.h"
#include "BDAcquisitionFromDevice.h"
#include "UIAcquisitionFromDevice.h"

#include "EasyAttachment\JsonForms\UIAcquisitionFromDevice\IDD_ACQUISITION_FROM_DEVICE_TOOLBAR.hjson"
#include "EasyAttachment\JsonForms\UIAcquisitionFromDevice\IDD_ACQUISITION_FROM_DEVICE.hjson"

using namespace System;
using namespace System::Collections::Generic;
using namespace Microarea::EasyAttachment::Components;
using namespace Microarea::EasyAttachment::Core;
using namespace Microarea::TBPicComponents;

//////////////////////////////////////////////////////////////////////////////
//			 class CExtensionsToScanItemSource implementation
//////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CExtensionsToScanItemSource, CItemSource)

//-----------------------------------------------------------------------------
CExtensionsToScanItemSource::CExtensionsToScanItemSource()
	:
	CItemSource			(),
	m_pExtensionsList	(NULL)
{
	if (!m_pExtensionsList)
		m_pExtensionsList = new CStringArray();

	IList<System::String^>^ extList = Utils::GetExtensionsForScan();
	for each (String^ extension in extList)
		m_pExtensionsList->Add((LPCTSTR)(CString)extension->ToUpperInvariant());
}

//-----------------------------------------------------------------------------
CExtensionsToScanItemSource::~CExtensionsToScanItemSource()
{
	SAFE_DELETE(m_pExtensionsList);
}

//-----------------------------------------------------------------------------
void CExtensionsToScanItemSource::GetData(DataArray& values, CStringArray& descriptions, CString strCurrentValue)
{
	if (!m_pExtensionsList)
		return;

	for (int i = 0; i <= m_pExtensionsList->GetUpperBound(); i++)
	{
		CString extension = m_pExtensionsList->GetAt(i);
		values.Add(new DataStr(extension));
		descriptions.Add(extension);
	}
}

//////////////////////////////////////////////////////////////////////////////
//               class BDAcquisitionFromDevice implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(BDAcquisitionFromDevice, CAbstractFormDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(BDAcquisitionFromDevice, CAbstractFormDoc)
	//{{AFX_MSG_MAP(BDAcquisitionFromDevice)
	ON_CBN_SELCHANGE(IDC_DEVICE_COMBO_EXTENSIONS,	OnExtensionChanged)

	ON_BN_CLICKED(IDC_DEVICE_CREATE_SEPARATE_FILE,	OnCreateSeparateFileChanged)
	ON_BN_CLICKED(IDC_DEVICE_BTN_SCAN,				OnScanClick)
	ON_BN_CLICKED(IDC_DEVICE_BTN_CANCEL,			OnCancelClick)
	ON_BN_CLICKED(IDC_DEVICE_BTN_CHANGE_SOURCE,		OnChangeSourceClick)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
BDAcquisitionFromDevice::BDAcquisitionFromDevice() 
	:
	tbPicImaging		(nullptr),
	tbPicPDF			(nullptr),
	m_sPDFImageName		(_T("")),
	m_nTIFFID			(-1),
	m_nTIFFImageCount	(0),
	m_bIsScanning		(FALSE),
	m_pCallingDoc		(NULL)
{
	DisableFamilyClientDoc(TRUE);

	tbPicImaging		= gcnew TBPicImaging();
	tbPicPDF			= gcnew TBPicPDF();
	dlgHandle			= IntPtr::Zero;
	m_pAcquiredFiles	= new CStringArray();
}

//-----------------------------------------------------------------------------
BDAcquisitionFromDevice::~BDAcquisitionFromDevice()
{
	tbPicImaging->TwainCloseSource();
	tbPicImaging->TwainCloseSourceManager(dlgHandle);
	tbPicImaging->TwainUnloadSourceManager(dlgHandle);

	if (tbPicImaging)
	{
		delete tbPicImaging;
		tbPicImaging = nullptr;
	}

	if (tbPicPDF)
	{
		delete tbPicPDF;
		tbPicPDF = nullptr;
	}

	SAFE_DELETE(m_pAcquiredFiles);
}

//-----------------------------------------------------------------------------
BOOL BDAcquisitionFromDevice::OnAttachData()
{                                                   
	SetFormTitle (_TB("Acquisition from device"));

	InitSelections();

	DECLARE_VAR_JSON(sSource);
	DECLARE_VAR_JSON(bChangeSource);
	DECLARE_VAR_JSON(sFile);
	DECLARE_VAR_JSON(sExtension);
	DECLARE_VAR_JSON(bSeparateFiles);
	DECLARE_VAR_JSON(bSplitPage);
	DECLARE_VAR_JSON(bSplitBarcode);

	return TRUE;
}  

//-----------------------------------------------------------------------------
void BDAcquisitionFromDevice::InitSelections()
{
	if (!tbPicImaging->TwainIsAvailable())
	{
		AfxMessageBox(_TB("TWAIN is non installed on the system! Unable to proceed!"), MB_OK, MB_ICONEXCLAMATION);
		CloseDocument();
	}

	// apro subito il source di default
	if (tbPicImaging->TwainOpenDefaultSource(dlgHandle))
	{
		CString source = tbPicImaging->TwainGetDefaultSourceName(dlgHandle);
		if (!source.IsEmpty())
			m_sSource = DataStr(source);
	}

	m_sFile = (CString)Utils::GetFileNameToScan();
	m_sExtension = szPdf; // estensione di default e' PDF
	
	// inizializzo i valori a video
	m_bSeparateFiles = FALSE;
	m_bSeparateFiles.SetReadOnly(FALSE);
	m_bSplitPage	= TRUE;
	m_bSplitBarcode = FALSE;
	m_bSplitPage.SetReadOnly(!m_bSeparateFiles);
	m_bSplitBarcode.SetReadOnly(!AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_EnableBarcode || !m_bSeparateFiles);
	
	UpdateDataView();
}

//----------------------------------------------------------------------------
BOOL BDAcquisitionFromDevice::OnOpenDocument(LPCTSTR pParam)
{
	// mi tengo da parte il puntatore del documento chiamante
	if (pParam)
		m_pCallingDoc = (CAbstractFormDoc*)GET_AUXINFO(pParam);

	if (m_pCallingDoc)
		m_pCallingDoc->GetMasterFrame()->EnableWindow(FALSE);

	return CAbstractFormDoc::OnOpenDocument(pParam);
}

//-----------------------------------------------------------------------------
void BDAcquisitionFromDevice::OnCreateSeparateFileChanged()
{
	EnableControls();
}

//-----------------------------------------------------------------------------
void BDAcquisitionFromDevice::OnExtensionChanged()
{
	EnableControls();
}

//-----------------------------------------------------------------------------
void BDAcquisitionFromDevice::EnableControls()
{
	BOOL isPdfOrTiff = (m_sExtension.CompareNoCase(szPdf) == 0 || m_sExtension.CompareNoCase(szTiff) == 0);

	if (isPdfOrTiff)
	{
		m_bSeparateFiles.SetReadOnly(FALSE);
		m_bSplitPage.SetReadOnly(!m_bSeparateFiles);
		m_bSplitBarcode.SetReadOnly(!AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_EnableBarcode || !m_bSeparateFiles);
	}
	else
	{
		m_bSeparateFiles = TRUE;
		m_bSeparateFiles.SetReadOnly(TRUE);
		m_bSplitPage = TRUE;
		m_bSplitBarcode = FALSE;
		m_bSplitPage.SetReadOnly(TRUE);
		m_bSplitBarcode.SetReadOnly(TRUE);
	}

	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDAcquisitionFromDevice::OnScanClick()
{
	if (m_bIsScanning)
	{
		AfxMessageBox(_TB("Please wait previous scanner operation completion!"), MB_OK, MB_ICONEXCLAMATION);
		return;
	}

	if (m_sSource.IsEmpty())
	{
		AfxMessageBox(_TB("You have to select a valid source first!"), MB_OK, MB_ICONEXCLAMATION);
		return;
	}

	// se il nome del file e' vuoto compongo al volo un nome temporaneo
	if (m_sFile.IsEmpty())
		m_sFile = ((CString)Utils::GetFileNameToScan());

	// richiamo la procedura di acquisizione dei file da scanner
	RunScan();

	// intercettato dai documenti chiamanti
	m_pCallingDoc->GetMasterFrame()->SendMessage(WM_COMMAND, ID_SCANPROCESS_ENDED);

	CloseDocument();
}

//-----------------------------------------------------------------------------
void BDAcquisitionFromDevice::OnCancelClick()
{
	if (m_bIsScanning)
	{
		AfxMessageBox(_TB("Please close scanner window first!"), MB_OK, MB_ICONEXCLAMATION);
		return;
	}
	CloseDocument();
}

// apre la dialog predefinita di TWAIN che consente di scegliere tra i source disponibili
//-----------------------------------------------------------------------------
void BDAcquisitionFromDevice::OnChangeSourceClick()
{
	if (!tbPicImaging->TwainSelectSource(dlgHandle))
	{
		if (tbPicImaging->TwainGetLastResultCode() == TbPicTwainResultCode::TWRC_FAILURE)
		{
			AfxMessageBox(_TB("Unable to select source!"), MB_OK, MB_ICONEXCLAMATION);
			return;
		}
	}

	CString source = tbPicImaging->TwainGetDefaultSourceName(dlgHandle);
	if (!source.IsEmpty())
	{
		if (!tbPicImaging->TwainOpenSource(dlgHandle, gcnew String(source)))
			return; //  un messaggio viene visualizzato in automatico dal driver
		m_sSource = DataStr(source);
	}
}

//-----------------------------------------------------------------------------
void BDAcquisitionFromDevice::RunScan()
{
	m_pAcquiredFiles->RemoveAll();

	// gestione in caso di multipagina
	if (m_bSplitBarcode && !m_bSplitBarcode.IsReadOnly())
	{
		if (m_sExtension.CompareNoCase(szPdf) == 0 || m_sExtension.CompareNoCase(szTiff) == 0)
			AfxGetTbRepositoryManager()->MultipleScanWithBarcode(m_sFile, m_sExtension, m_pAcquiredFiles);
		return;
	}

	CString acquiredFilePath;
	TBPictureStatus gdStatus = TBPictureStatus::GenericError;

	TRY
	{
		if (tbPicImaging->TwainOpenDefaultSource(dlgHandle))
		{
			// N.B. il source deve essere aperto per impostare queste proprieta'!!!!
			tbPicImaging->TwainSetAutoFeed(true); // Set AutoFeed Enabled
			tbPicImaging->TwainSetAutoScan(true); // To achieve the maximum scanning rate
			tbPicImaging->TwainSetHideUI(false); // mostro la finestra dei parametri del driver
			tbPicImaging->TwainSetIndicators(true);

			int nImage = 0; // serve per la gestione della numerazione in caso di salvataggio multiplo di file (laddove il driver lo consenta)
			do
			{
				m_bIsScanning = TRUE;

				int imageId = tbPicImaging->TwainAcquireToGdPictureImage(dlgHandle);
				
				m_bIsScanning = FALSE;
				
				// se l'immagine e' stata acquisita con successo procedo al suo salvataggio su file system
				if (imageId != 0)
				{
					nImage++;

					if (m_sExtension.CompareNoCase(szBmp) == 0)
					{
						acquiredFilePath = AfxGetTbRepositoryManager()->GetEasyAttachmentTempPath() + SLASH_CHAR + m_sFile;
						if (nImage > 1) // nel caso di acquisizione di piu' pagine aggiungo il numero in fondo dalla seconda pagina in poi
							acquiredFilePath += (_T("_") + (CString)nImage.ToString());
						acquiredFilePath += szDotBMP;

						gdStatus = tbPicImaging->SaveAsBMP(imageId, gcnew String(acquiredFilePath));
						if (gdStatus == TBPictureStatus::OK)
							m_pAcquiredFiles->Add(acquiredFilePath);
					}

					if (m_sExtension.CompareNoCase(szGif) == 0)
					{
						acquiredFilePath = AfxGetTbRepositoryManager()->GetEasyAttachmentTempPath() + SLASH_CHAR + m_sFile;
						if (nImage > 1) // nel caso di acquisizione di piu' pagine aggiungo il numero in fondo dalla seconda pagina in poi
							acquiredFilePath += (_T("_") + (CString)nImage.ToString());
						acquiredFilePath += szDotGIF;

						gdStatus = tbPicImaging->SaveAsGIF(imageId, gcnew String(acquiredFilePath));
						if (gdStatus == TBPictureStatus::OK)
							m_pAcquiredFiles->Add(acquiredFilePath);
					}

					if (m_sExtension.CompareNoCase(szJpeg) == 0)
					{
						acquiredFilePath = AfxGetTbRepositoryManager()->GetEasyAttachmentTempPath() + SLASH_CHAR + m_sFile;
						if (nImage > 1) // nel caso di acquisizione di piu' pagine aggiungo il numero in fondo dalla seconda pagina in poi
							acquiredFilePath += (_T("_") + (CString)nImage.ToString());
						acquiredFilePath += szDotJPEG;

						gdStatus = tbPicImaging->SaveAsJPEG(imageId, gcnew String(acquiredFilePath));
						if (gdStatus == TBPictureStatus::OK)
							m_pAcquiredFiles->Add(acquiredFilePath);
					}

					if (m_sExtension.CompareNoCase(szPng) == 0)
					{
						acquiredFilePath = AfxGetTbRepositoryManager()->GetEasyAttachmentTempPath() + SLASH_CHAR + m_sFile;
						if (nImage > 1) // nel caso di acquisizione di piu' pagine aggiungo il numero in fondo dalla seconda pagina in poi
							acquiredFilePath += (_T("_") + (CString)nImage.ToString());
						acquiredFilePath += szDotPNG;

						gdStatus = tbPicImaging->SaveAsPNG(imageId, gcnew String(acquiredFilePath));
						if (gdStatus == TBPictureStatus::OK)
							m_pAcquiredFiles->Add(acquiredFilePath);
					}

					if (m_sExtension.CompareNoCase(szPdf) == 0)
					{
						if (m_sPDFImageName.IsEmpty())
						{
							// il nome del path lo rigenero solo se pdfImageName e' empty, ovvero si tratta della prima pagina
							acquiredFilePath = AfxGetTbRepositoryManager()->GetEasyAttachmentTempPath() + SLASH_CHAR + m_sFile;
							if (nImage > 1)
								acquiredFilePath += (_T("_") + (CString)nImage.ToString());
							acquiredFilePath += szDotPDF;
						}
						gdStatus = (TBPictureStatus)SaveAsPDF(imageId, acquiredFilePath);
					}

					if (m_sExtension.CompareNoCase(szTiff) == 0)
					{
						if (m_nTIFFID == -1)
						{
							// il nome del path lo rigenero solo se tiffID e' -1, ovvero si tratta della prima pagina
							acquiredFilePath = AfxGetTbRepositoryManager()->GetEasyAttachmentTempPath() + SLASH_CHAR + m_sFile;
							if (nImage > 1)
								acquiredFilePath += (_T("_") + (CString)nImage.ToString());
							acquiredFilePath += szDotTIFF;
						}
						m_nTIFFImageCount++;
						gdStatus = (TBPictureStatus)SaveAsTIFF(imageId, acquiredFilePath);
					}
				}
			}// loop sullo stato (nel caso di multipagina e' impostato con TWAIN_TRANSFER_READY)
			while (tbPicImaging->TwainGetState() > TBPicTwainStatus::TWAIN_SOURCE_ENABLED);

			// se lo stato e' OK e l'estensione e' PDF o TIFF salvo l'eventuale multipagina e aggiungo il file acquisito alla lista
			// (non lo faccio prima perche' se qualcuno ha scelto il multipagina devo prima fare il loop su tutte le pagine memorizzate dal driver)
			if (gdStatus == TBPictureStatus::OK)
			{
				if (FileExtensions::IsPdfString(gcnew String(m_sExtension.Str())))
				{
					// se pdfImageName e' valorizzata allora devo salvare il file PDF come multipagina
					if (!m_sPDFImageName.IsEmpty())
					{
						tbPicPDF->SaveToFile(gcnew String(acquiredFilePath));
						m_pAcquiredFiles->Add(acquiredFilePath);
					}
				}

				if (FileExtensions::IsTifString(gcnew String(m_sExtension.Str())))
				{
					if (m_nTIFFID > 0) // se tiffID e' buono allora devo salvare il file TIFF come multipagina
					{
						tbPicImaging->TiffCloseMultiPageFile(m_nTIFFID);
						m_pAcquiredFiles->Add(acquiredFilePath);
					}
				}
			}
		}
	}
	CATCH(CException, e)
	{
		TCHAR szError[1024];
		AfxMessageBox(e->GetErrorMessage(szError, 1024));
	}
	END_CATCH

	tbPicImaging->TwainCloseSource();
}

// Salvataggio di un file TIFF a pagina singola oppure, se richiesto, gestione del multipagina con append delle pagine
// successive alla prima
//-----------------------------------------------------------------------------
CTBPicViewerAdvWrapper::E_TBPICTURESTATUS BDAcquisitionFromDevice::SaveAsTIFF(int imageID, CString acquiredFilePath)
{
	TBPictureStatus gdStatus = TBPictureStatus::OK;

	// se nTIFFImageCount == 1 si tratta della prima pagina
	if (m_nTIFFImageCount == 1)
	{
		m_nTIFFID = imageID;
		// creo il file TIFF e aggiungo la prima pagina
		gdStatus = tbPicImaging->TiffSaveAsMultiPageFile(m_nTIFFID, gcnew String(acquiredFilePath), TBPicTiffCompression::TiffCompressionAUTO);

		if (m_bSeparateFiles)
		{
			// se l'utente ha scelto di creare un singolo file per immagine
			// salvo il TIFF e reimposto il tiffID = -1 (cosi' non lo risalvo piu' alla fine del loop)
			// e ri-azzero il count delle pagine
			gdStatus = tbPicImaging->TiffCloseMultiPageFile(m_nTIFFID);
			// aggiungo il file alla lista
			m_pAcquiredFiles->Add(acquiredFilePath);

			m_nTIFFID = -1;
			m_nTIFFImageCount = 0;
		}
	}
	else
	{
		// dalla seconda volta in poi passo a qui e vado in append con le pagine
		// si tratta di un documento multipagina
		gdStatus = tbPicImaging->TiffAddToMultiPageFile(m_nTIFFID, imageID);
		tbPicImaging->ReleaseGdPictureImage(imageID);
	}

	return (CTBPicViewerAdvWrapper::E_TBPICTURESTATUS)gdStatus;
}

// Salvataggio di un file PDF a pagina singola oppure, se richiesto, gestione del multipagina con append delle pagine
// successive alla prima
//-----------------------------------------------------------------------------
CTBPicViewerAdvWrapper::E_TBPICTURESTATUS BDAcquisitionFromDevice::SaveAsPDF(int imageID, CString acquiredFilePath)
{
	TBPictureStatus gdStatus = TBPictureStatus::OK;

	// se pdfImageName e' empty e' la prima volta che passo di qui
	if (m_sPDFImageName.IsEmpty())
	{
		// creo il file PDF (formato PDF/A 1-b compatibile)
		gdStatus = tbPicPDF->NewPDF(true);

		// aggiungo l'immagine appena acquisita al file
		//@@TODOPORTING8: controllare il terzo parametro: If true, begin a new page and draw the added image on its whole surface.
		m_sPDFImageName = tbPicPDF->AddImageFromGdPictureImage(imageID, false, true);
		tbPicImaging->ReleaseGdPictureImage(imageID); // rilascio la memoria facendo la release dell'immagine

		if (m_bSeparateFiles)
		{
			// se l'utente ha scelto di creare un singolo file per immagine
			// salvo il PDF e reimposto il pdfImageName a empty (cosi' non lo risalvo piu' alla fine del loop)
			tbPicPDF->SaveToFile(gcnew String(acquiredFilePath));
			// aggiungo il file alla lista (ma solo se lo status e' a true??)
			m_pAcquiredFiles->Add(acquiredFilePath);

			m_sPDFImageName.Empty();
		}

		return (CTBPicViewerAdvWrapper::E_TBPICTURESTATUS)gdStatus;
	}
	else
	{
		// dalla seconda volta in poi passo a qui e vado in append con le pagine
		// si tratta di un documento multipagina
		//@@TODOPORTING8: controllare il terzo parametro: If true, begin a new page and draw the added image on its whole surface.
		m_sPDFImageName = tbPicPDF->AddImageFromGdPictureImage(imageID, false, true);
		tbPicImaging->ReleaseGdPictureImage(imageID); // rilascio la memoria facendo la release dell'immagine
	}

	return (CTBPicViewerAdvWrapper::E_TBPICTURESTATUS)gdStatus;
}

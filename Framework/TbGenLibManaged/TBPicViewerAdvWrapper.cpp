#include "stdafx.h"

#include "afxwinforms.h"
#include "atlimage.h"

#include <TbNameSolver\FileSystemFunctions.h>
#include <TbGeneric\Globals.h>
#include <TbGeneric\TBThemeManager.h>
#include <TbGeneric\FontsTable.h>
#include <TbGenlib\Generic.h>

#include "TBPicViewerAdvWrapper.h"

#include "UserControlHandlers.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;

using namespace Microarea::TBPicComponents;

//===========================================================================
// macro in order to identify Control used
//===========================================================================

#define ENSURE_PICVIEWERADV_CONTROL(r) ENSURE_USER_CONTROL(r, TBPicViewerAdv, picViewer, szPicViewerAdvNotInitialized)

#define VOID_ENSURE_PICVIEWERADV_CONTROL() VOID_ENSURE_USER_CONTROL(TBPicViewerAdv, picViewer, szPicViewerAdvNotInitialized)

#define DECLARE_PICVIEWERADV_HANDLER() DECLARE_CTRL_HANDLER(TBPicViewerAdv, picViewerHandler) 

//---------------------------------------------------------------------------
TBPicViewerAdv^ GetUserControl(CUserControlWrapperObj* pWrapper)
{
	if (pWrapper == NULL || pWrapper->GetHandler() == NULL)
	{
		ASSERT_TRACE (FALSE, _T("TbPicViewerAdv Managed User Control not initialized!"));
		return nullptr;
	}
	CUserControlHandler<TBPicViewerAdv>* p =  (CUserControlHandler<TBPicViewerAdv>*) pWrapper->GetHandler();
	return p->GetControl();
}

//---------------------------------------------------------------------------
static const TCHAR szPicViewerAdvNotInitialized [] = _T("TbPicViewerAdv control not initialized. Cannot call requested method");

//===========================================================================
//							TBPicViewerAdvEventsHandler
//===========================================================================
ref class TBPicViewerAdvEventsHandler : public ManagedEventsHandlerObj
{
	CTBPicViewerAdvWrapper* m_pWrapper;

public:
	//---------------------------------------------------------------------------------------
	TBPicViewerAdvEventsHandler(CTBPicViewerAdvWrapper* p) : m_pWrapper(p)
	{
	}

	//------------------------------------------------------------------------------------------------------------
	virtual void MapEvents(Object^ pControl) override
	{
		TBPicViewerAdv^ pTBPicViewerControl = (TBPicViewerAdv^)pControl;

		pTBPicViewerControl->DragDrop += gcnew DragEventHandler(this, &TBPicViewerAdvEventsHandler::OnDragDrop);
		pTBPicViewerControl->DragEnter += gcnew DragEventHandler(this, &TBPicViewerAdvEventsHandler::OnDragEnter);
	}

	//------------------------------------------------------------------------------------------------------------
	void TBPicViewerAdvEventsHandler::OnDragDrop(Object ^sender, DragEventArgs ^e)
	{
		if (e->Data->GetDataPresent(DataFormats::FileDrop))
		{
			Object^ obj = e->Data->GetData(DataFormats::FileDrop);

			array<String^>^ ar = dynamic_cast<array<String^>^>(obj);
			if (ar != nullptr)
				for each (String^ file in ar)
					m_pWrapper->m_sFilePathToLoad = file;
		}

		SendAsControl(UM_PICVIEWER_DRAG_DROP);
	}

	//------------------------------------------------------------------------------------------------------------
	void TBPicViewerAdvEventsHandler::OnDragEnter(Object ^sender, DragEventArgs ^e)
	{
		e->Effect = DragDropEffects::None;

		if (e->Data->GetDataPresent(DataFormats::FileDrop) || e->Data->GetDataPresent("FileGroupDescriptor"))
			e->Effect = DragDropEffects::Copy;
	}
};

//===========================================================================
//								CTBPicViewerAdvWrapper
//===========================================================================
//---------------------------------------------------------------------------------------
CTBPicViewerAdvWrapper::CTBPicViewerAdvWrapper()
{
	m_pManHandler = new CUserControlHandler<TBPicViewerAdv>(gcnew TBPicViewerAdvEventsHandler(this));
}

//---------------------------------------------------------------------------------------
CTBPicViewerAdvWrapper::~CTBPicViewerAdvWrapper()
{
	delete m_pManHandler;
}

//---------------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::OnInitControl()
{
	// set default values
	SetDocumentAlignmentMiddleLeft();
	SetDocumentPositionMiddleLeft();
	SetViewerMouseWheelModeVerticalScroll();
	SetViewerZoomModeFitToViewer();
	SetRectBorder();
	// imposta il colore di sfondo previsto dal tema
	SetBackColorTBPicViewer();
}

//---------------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::AllowDrop(const BOOL& bValue /*= FALSE*/)
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->AllowDrop = (bValue == TRUE);
}

// Defines if by default the viewer must animate loaded multiframe GIF images. True by default.
//---------------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::AnimateGIF(const BOOL& bValue /*= FALSE*/)
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->AnimateGIF = (bValue == TRUE);
}

// Clears the viewer
//---------------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::Clear()
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->Clear();
}

// For multipage documents viewing, determines if the viewer displays automatically the next or the previous page when the user scroll the current page.
//---------------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::ContinuousViewMode(const BOOL& bValue /*= TRUE*/)
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->ContinuousViewMode = (bValue == TRUE);
}

// Current page of the displayed document. (Starts from 1).
//---------------------------------------------------------------------------------------
int CTBPicViewerAdvWrapper::CurrentPage()
{
	ENSURE_PICVIEWERADV_CONTROL(1)
	return picViewer->CurrentPage;
}

//---------------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::Enabled(BOOL bValue /*TRUE*/)
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->Enabled = (bValue == TRUE);

	//CTBPicViewerAdvWrapper::E_TBPICTURESTATUS status = DisplayFromFile(_T("C:\\Shared\\GeneveSoundSystem.JPG"));
}

// To enable or disable the viewer contextual menu.
//---------------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::EnableMenu(const BOOL& bValue /*= TRUE*/)
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->EnableMenu = (bValue == TRUE);
}

//---------------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::EnablePreviewNotAvailable(const BOOL& bValue /*= TRUE*/)
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->EnablePreviewNotAvailable = (bValue == TRUE);
}


// Turn this property to True to force the viewer to always displaying scrollbars.
//---------------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::ForceScrollBars(const BOOL& bValue /*= TRUE*/)
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->ForceScrollBars = (bValue == TRUE);
}

// Number of pages in the displayed document.
//---------------------------------------------------------------------------------------
int CTBPicViewerAdvWrapper::PageCount()
{
	ENSURE_PICVIEWERADV_CONTROL(0)
	return picViewer->PageCount;
}

// Defines if scrollbars can be displayed or not within the viewer. If true, scrollbars appears when the area to render is larger than the control.
//---------------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::ScrollBars(const BOOL& bValue /*= TRUE*/)
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->ScrollBars = (bValue == TRUE);
}

// Turn this property to True to deactivate error reporting through MessageBox.
//---------------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::SilentMode(const BOOL& bValue /*= TRUE*/)
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->SilentMode = (bValue == TRUE);
}

// per impostare la visibilita' della ToolStrip del control
//---------------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::SetToolStripVisibility(const BOOL& bValue /*= TRUE*/)
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->SetToolStripVisibility(bValue == TRUE);
}

// Default image alignment within the viewer when the area of the viewer if smaller than the displayed document in its current zoom configuration.
//---------------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::SetDocumentAlignmentMiddleLeft()
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->DocumentAlignment = Microarea::TBPicComponents::TBPicViewerDocumentAlignment::DocumentAlignmentMiddleLeft;
}

// Default page position within the viewer when the displayed document in its current zoom configuration if smaller than then area of the viewer.
//---------------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::SetDocumentPositionMiddleLeft()
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->DocumentPosition = Microarea::TBPicComponents::TBPicViewerDocumentPosition::DocumentPositionMiddleLeft;
}

//---------------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::SetViewerMouseModePan()
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->MouseMode = Microarea::TBPicComponents::TBPicViewerMouseMode::MouseModePan;
}

//---------------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::SetViewerMouseModeAreaSelection()
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->MouseMode = Microarea::TBPicComponents::TBPicViewerMouseMode::MouseModeAreaSelection;
}

//---------------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::SetViewerMouseWheelModeVerticalScroll()
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->MouseWheelMode = Microarea::TBPicComponents::TBPicViewerMouseWheelMode::MouseWheelModeVerticalScroll;
}

//---------------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::SetViewerZoomModeFitToViewer()
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->ZoomMode = Microarea::TBPicComponents::TBPicViewerZoomMode::ZoomModeFitToViewer;
}

//---------------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::SetRectBorder()
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->RectBorderColor = System::Drawing::Color::Red;
	picViewer->RectBorderSize = 3;
}

//---------------------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::SetBackColorTBPicViewer(const COLORREF color)
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	DECLARE_PICVIEWERADV_HANDLER()

	if (picViewer == nullptr)
		return;

	picViewer->BackColor = picViewerHandler->ConvertColor(color);
}

// Displays a document from a file.
// Supported formats are RAW, PICT, PDF, BMP, DIB, RLE, ICO, EMF, WMF, GIF, ANIMATED GIF, JPEG, JPG, JPE, JFIF, PNG, TIFF, MULTIPAGE TIFF, PNM, 
// PPM, PBM, PGM, PFM, RPPM, RPGM, RPBM, PCX, XPM, XBM, WBMP, TGA, SGI, Sun RAS, PSD, MNG, Kodak PhotoCD files, KOALA files, JP2, J2K, JNG, 
// JBIG, JBIG2(requires Plugin), IFF, HDR, Raw, Fax G3, EXR, DDS and Dr.Halo files.
//---------------------------------------------------------------------------------------
CTBPicViewerAdvWrapper::E_TBPICTURESTATUS CTBPicViewerAdvWrapper::DisplayFromFile(const CString& sFilePath)
{
	ENSURE_PICVIEWERADV_CONTROL(E_TBPICTURESTATUS::OK)
	return (E_TBPICTURESTATUS)picViewer->DisplayFromFile(gcnew String(sFilePath), gcnew String(_TB("Preview not available")));
}

// Displays a GdPicture Image.
//---------------------------------------------------------------------------------------
CTBPicViewerAdvWrapper::E_TBPICTURESTATUS CTBPicViewerAdvWrapper::DisplayFromGdPictureImage(int nImageId)
{
	ENSURE_PICVIEWERADV_CONTROL(E_TBPICTURESTATUS::OK)
	return (E_TBPICTURESTATUS)picViewer->DisplayFromGdPictureImage(nImageId, gcnew String(_TB("Preview not available")));
}

//---------------------------------------------------------------------------------------
/*int CTBPicViewerAdvWrapper::DisplayFromByteArray(BYTE* pBuffer)
{
	ENSURE_PICVIEWERADV_CONTROL(NULL)
	
	int count = pBlob->GetLen();
	array<Byte>^ bytes = gcnew array<Byte>(count);
	for (int i = 0; i < count; i++)
		bytes[i] = pBuffer[i];

	Microarea::TBPicComponents::TBPictureStatus status = picViewer->DisplayFromByteArray(gcnew array<System::Byte, 1>(bytes->Length));
	return (int)status;
}*/

// Sets the coordinates in pixel of the rectangle of area selection over the page of the displayed document.
//--------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::SetRectCoordinatesOnDocument(int nLeft, int nTop, int nWidth, int nHeight)
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->SetRectCoordinatesOnDocument(nLeft, nTop, nWidth, nHeight);
}

// Closes the displayed document and clears the viewer.
//--------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::CloseDocument()
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->CloseDocument();
}

// Increases the current level of zoom.
//--------------------------------------------------------------------------------
CTBPicViewerAdvWrapper::E_TBPICTURESTATUS CTBPicViewerAdvWrapper::ZoomIN()
{
	ENSURE_PICVIEWERADV_CONTROL(E_TBPICTURESTATUS::OK)
	return (E_TBPICTURESTATUS)picViewer->ZoomIN();
}

// Decreases the current level of zoom.
//--------------------------------------------------------------------------------
CTBPicViewerAdvWrapper::E_TBPICTURESTATUS CTBPicViewerAdvWrapper::ZoomOUT()
{
	ENSURE_PICVIEWERADV_CONTROL(E_TBPICTURESTATUS::OK)
	return (E_TBPICTURESTATUS)picViewer->ZoomOUT();
}

// Displays the previous page of the current multipage document.
//--------------------------------------------------------------------------------
CTBPicViewerAdvWrapper::E_TBPICTURESTATUS CTBPicViewerAdvWrapper::DisplayPreviousPage()
{
	ENSURE_PICVIEWERADV_CONTROL(E_TBPICTURESTATUS::OK)
	return (E_TBPICTURESTATUS)picViewer->DisplayPreviousPage();
}

// Displays the next page of the current multipage document.
//--------------------------------------------------------------------------------
CTBPicViewerAdvWrapper::E_TBPICTURESTATUS CTBPicViewerAdvWrapper::DisplayNextPage()
{
	ENSURE_PICVIEWERADV_CONTROL(E_TBPICTURESTATUS::OK)
	return (E_TBPICTURESTATUS)picViewer->DisplayNextPage();
}

// Returns True if the rectangle of area selection is drawn on the viewer, else false.
//--------------------------------------------------------------------------------
BOOL CTBPicViewerAdvWrapper::IsRect()
{
	ENSURE_PICVIEWERADV_CONTROL(FALSE)
	return picViewer->IsRect();
}

// Returns True if the user is drawing the rectangle of area selection, else false.
//--------------------------------------------------------------------------------
BOOL CTBPicViewerAdvWrapper::IsRectDrawing()
{
	ENSURE_PICVIEWERADV_CONTROL(FALSE)
	return picViewer->IsRectDrawing();
}

// Removes the rectangle of area selection from the viewer.
//--------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::ClearRect()
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->ClearRect();
}

// Returns the coordinates in inches of the rectangle of area selection over the page of the displayed document.
//--------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::GetRectCoordinatesOnDocumentInches(float& fLeftArea, float& fTopArea, float& fWidthArea, float& fHeightArea)
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->GetRectCoordinatesOnDocumentInches(fLeftArea, fTopArea, fWidthArea, fHeightArea);
}

// Returns the coordinates in pixel of the rectangle of area selection over the page of the displayed document.
//--------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::GetRectCoordinatesOnDocument(int& nLeftArea, int& nTopArea, int& nWidthArea, int& nHeightArea)
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->GetRectCoordinatesOnDocument(nLeftArea, nTopArea, nWidthArea, nHeightArea);
}

// Returns the coordinates in pixel of the rectangle of area selection over the viewer.
//--------------------------------------------------------------------------------
void CTBPicViewerAdvWrapper::GetRectCoordinatesOnViewer(int& nLeftAreaVw, int& nTopAreaVw, int& nWidthAreaVw, int& nHeightAreaVw)
{
	VOID_ENSURE_PICVIEWERADV_CONTROL()
	picViewer->GetRectCoordinatesOnViewer(nLeftAreaVw, nTopAreaVw, nWidthAreaVw, nHeightAreaVw);
}

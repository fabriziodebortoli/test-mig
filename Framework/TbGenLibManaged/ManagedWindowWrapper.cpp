#include "stdafx.h"

#include "ManagedWindowWrapper.h"
#include "Main.h"
#include <TbGeneric\DatesFunctions.h>
#include <TbGeneric\TbStrings.h>

using namespace System;
using namespace Microarea::TaskBuilderNet::Core::Generic;

class CInternalManagedWindowWrapper
{
public:
	gcroot<PopupContainer^> m_pManagedWindow;
	CInternalManagedWindowWrapper()
	{
		m_pManagedWindow = nullptr;
	}
};
//---------------------------------------------------------------------------------------
ManagedWindowWrapper::ManagedWindowWrapper(HWND hwndFromControl)
{
	m_hwndFromControl = hwndFromControl;
	m_pWrapper = new CInternalManagedWindowWrapper();
}

//---------------------------------------------------------------------------------------
ManagedWindowWrapper::~ManagedWindowWrapper()
{
	delete m_pWrapper;
}

//Metodo che fa la chiamata managed per mostrare la finestra di selezione dei range
//Parametri:
// - Handle del parent
// - Data usata per calcolare i ranges in relazione ad essa
// - Path del file (eventuale) da cui caricare ranges personalizzati
//---------------------------------------------------------------------------------------
void ManagedWindowWrapper::ShowRangeSelector(HWND hwndParent, int applicationDateDay, int applicationDateMonth, int applicationDateYear, CString sFilePath)
{
	//InitThreadCulture();Chiamata resa inutile qui perche` fatta alla partenza del thread di documento
	DateTime applicationDate(applicationDateYear, applicationDateMonth, applicationDateDay);
	String^ filePath = gcnew String(sFilePath);
	
	DateRangeSelector^ pDateRangeSelector = gcnew DateRangeSelector();
	m_hwndManagedForm = (HWND)(int)pDateRangeSelector->ShowRangeSelector((System::IntPtr)hwndParent,(System::IntPtr)m_hwndFromControl, applicationDate, filePath);
	m_pWrapper->m_pManagedWindow = pDateRangeSelector;
}


//---------------------------------------------------------------------------------------
void ManagedWindowWrapper::ShowMonthCalendar(HWND hwndParent, int anchorDateDay, int anchorDateMonth, int anchorDateYear, int selectedDateDay, int selectedDateMonth, int selectedDateYear)
{
	//InitThreadCulture();Chiamata resa inutile qui perche` fatta alla partenza del thread di documento
	DateTime anchorDate(anchorDateYear, anchorDateMonth, anchorDateDay);
	DateTime selectedDate(selectedDateYear, selectedDateMonth, selectedDateDay);
	
	Calendar^ pCalendar = gcnew Calendar();
	
	m_hwndManagedForm = (HWND)(int)pCalendar->ShowMonthCalendar((System::IntPtr)hwndParent, (System::IntPtr)m_hwndFromControl, anchorDate, selectedDate);
	m_pWrapper->m_pManagedWindow = pCalendar;
}
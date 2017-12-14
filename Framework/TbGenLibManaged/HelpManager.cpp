#include "stdafx.h"

#include <TbNamesolver\ApplicationContext.h>
#include <TbNamesolver\PathFinder.h>
#include <TbGeneric\TBStrings.h>

#include "HelpManager.h"

#include <TbNameSolver\ThreadContext.h>


#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

using namespace System;
using namespace System::IO;
using namespace Microarea::TaskBuilderNet::Core::Generic;


//Metodo che dato namespace di un documento e culture lancia il browser sulla pagina di Help relativa al documento stesso
//-----------------------------------------------------------------------------
BOOL ShowHelp(const CString& strNamespace /* _T("")*/)
{
	if (strNamespace.IsEmpty())
	{
		ASSERT(FALSE);
		TRACE1 ("Help requested with empty help file name and with search namespace=%s!\n", strNamespace);
		return FALSE;
	}

	String^ smNamespace = gcnew String(strNamespace);
	String^ culture = gcnew String(AfxGetCulture());

	return HelpManager::CallOnlineHelp(smNamespace,  culture);
}

//Metodo che dato namespace di un documento e culture ritorna l'URL della pagina di Help relativa al documento stesso
//Utilizzato per creare il link nel bottone della toolbar in MagoWeb
//-----------------------------------------------------------------------------
CString GetOnlineHelpLink(const CString& strNamespace, bool fromEasyLook)
{
	if (strNamespace.IsEmpty())
	{
		ASSERT(FALSE);
		TRACE1 ("Help requested with empty help file name and with search namespace=%s!\n", strNamespace);
		return _T("");
	}

	String^ smNamespace = gcnew String(strNamespace);
	String^ culture = gcnew String(AfxGetCulture());
	CString strUrl = HelpManager::GetOnlineHelpUrl(smNamespace, culture, fromEasyLook);
	return strUrl;
}

//Metodo che lancia il browser sulla pagina principale del produttore (www.microarea.it)
//-----------------------------------------------------------------------------
void ConnectToProducerSite()
{
	HelpManager::ConnectToProducerSite();
}

//Metodo che lancia il browser sulla pagina di MyAccount del sito
//-----------------------------------------------------------------------------
void ConnectToProducerSiteLoginPage()
{
	HelpManager::ConnectToProducerSiteLoginPage(gcnew String(AfxGetAuthenticationToken()));
}

//Metodo che lancia il browser sulla pagina del sito di amministrazione dell'area riservata
//-----------------------------------------------------------------------------
void ConnectToProducerSitePrivateArea()
{
	HelpManager::ConnectToProducerSitePrivateArea(gcnew String(AfxGetAuthenticationToken()));
}
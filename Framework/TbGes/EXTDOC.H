
#pragma once

#include <TbGenlib\baseapp.h>			
#include <TbGenlib\basedoc.h>			
#include <TbGeneric\Array.h>
#include <TbGenlib\parsctrl.h>			
#include <TbGeneric\schedule.h>
#include <TbGeneric\critical.h>	

#include <TbGenlib\ToolBarButton.h>


//////////////////////////////////////////////////////////////////////////////
// inclusione dichiarazioni originali suddivise

#include "extdoc.hjson" //JSON AUTOMATIC UPDATE	

#include "extdocabstract.h"	

#include "extdocframe.h"	

#include "extdocview.h"	

#include "extdocfinderdoc.h"	

#include "extdocadm.h"	
		
#include "extdocclientdoc.h"			
//////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
//includere alla fine degli include del .H
#include "beginh.dex"
//=============================================================================
class TB_EXPORT CDynamicFormDoc : public CAbstractFormDoc, public ADMObj
{
	DECLARE_DYNCREATE(CDynamicFormDoc)

protected:
	BOOL OnAttachData(void);
	virtual BOOL OnNewDocument();
	virtual	ADMObj*	GetADM(){ return this; }
};

//=============================================================================
class TB_EXPORT CDynamicBatchFormDoc : public CDynamicFormDoc
{
	DECLARE_DYNCREATE(CDynamicBatchFormDoc)
public:
	CDynamicBatchFormDoc()	{ m_bBatch = TRUE; }
};

//=============================================================================
class TB_EXPORT DBrowserDocument : public CAbstractFormDoc
{
	DataStr m_strUrl;
	DECLARE_DYNCREATE(DBrowserDocument)
public:
	DBrowserDocument();
	virtual BOOL OnOpenDocument(LPCTSTR lpszPathName);
	virtual BOOL OnAttachData(void);
};

//=============================================================================
class CBrowserDocumentView : public CJsonFormView
{
	DECLARE_DYNCREATE(CBrowserDocumentView);
public:
	CBrowserDocumentView();
};
//=============================================================================
#include "endh.dex"

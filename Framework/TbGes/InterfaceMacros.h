
#pragma once

#include <TbOledb\InterfaceMacros.h>
#include <TbGes\extdoc.h>

//includere alla fine degli include del .H
#include "beginh.dex"

// useful class
class CTBNamespace;

//-------------------------------------------------------------------------------------------  ClientDoc
#define BEGIN_CLIENT_DOC()		virtual void AOI_AttachClientDocs(CAbstractFormDoc* pDoc, CClientDocArray* pAuxDocs, const CTBNamespace& aParent) {
#define		WHEN_SERVER_DOC(a)										if ( pDoc->HasClientDocToAttach(_T(#a), FALSE) && AfxIsActivated (aParent.GetApplicationName(), aParent.GetModuleName())){
#define			EXCLUDE_WHEN(e)										if (e) return;
#define			ATTACH_CLIENT_DOC_EXCLUDE_UNATTENDED_MODE(a, name)		if (!pDoc->IsInUnattendedMode()) {a* pClientDoc = new a; pClientDoc->Init(name, aParent); pAuxDocs->Add(pClientDoc);}
#define			ATTACH_CLIENT_DOC(a, name)								{ a* pClientDoc = new a; pClientDoc->Init(name, aParent); pAuxDocs->Add(pClientDoc); } 
#define		END_SERVER_DOC()										}
#define END_CLIENT_DOC()		}

//-------------------------------------------------------------------------------------------  ClientDoc
#define BEGIN_FAMILY_CLIENT_DOC()		virtual void AOI_AttachFamilyClientDocs(CAbstractFormDoc* pDoc, CClientDocArray* pAuxDocs, const CTBNamespace& aParent) {
#define		EXCLUDE_DOC_FROM_ATTACH(a)		if (pDoc->IsKindOf(RUNTIME_CLASS(a))) return;
#define		EXCLUDE_DOC_FROM_ATTACH_WHEN(e)	if (e) return;
#define		WHEN_FAMILY_SERVER_DOC_BYNAME(a)		if (pDoc->HasClientDocToAttach(_T(#a), TRUE) && AfxIsActivated (aParent.GetApplicationName(), aParent.GetModuleName()) ) {
#define		WHEN_FAMILY_SERVER_DOC(a)		if (pDoc->HasClientDocToAttach(RUNTIME_CLASS(a)) && AfxIsActivated (aParent.GetApplicationName(), aParent.GetModuleName()) ) {
#define			ATTACH_FAMILY_CLIENT_DOC(a, name)									{a* pClientDoc = new a; pClientDoc->Init(name, aParent); pAuxDocs->Add(pClientDoc);}
#define			ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_UNATTENDED_MODE(a, name)			if (!pDoc->IsInUnattendedMode()) {a* pClientDoc = new a; pClientDoc->Init(name, aParent); pAuxDocs->Add(pClientDoc); }
#define			ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_BATCH_MODE(a, name)				if (pDoc->GetType() != VMT_BATCH)  {a* pClientDoc = new a; pClientDoc->Init(name, aParent); pAuxDocs->Add(pClientDoc); } 
#define			ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_UNATTENDED_BATCH_MODE(a, name)		if (!pDoc->IsInUnattendedMode() &&  pDoc->GetType() != VMT_BATCH ) {a* pClientDoc = new a; pClientDoc->Init(name, aParent); pAuxDocs->Add(pClientDoc); } 
//è quasi una dublicazione della ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_BATCH_MODE ma in questo caso mi serve escludere solo le procedure batch
//per compatibilità con il passato anche gli ADM (che sono in UnattendedMode) hanno pDoc->GetType() == VMT_BATCH
#define			ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_FRAME(a, name, frame)		if (!pDoc->GetMasterFrame() ||(pDoc->GetMasterFrame() && !pDoc->GetMasterFrame()->IsKindOf(RUNTIME_CLASS(frame)))) {a* pClientDoc = new a; pClientDoc->Init(name, aParent); pAuxDocs->Add(pClientDoc); } 
#define			ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_BATCH_FRAME(a, name)		ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_FRAME(a, name, CBatchFrame)
#define			ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_WIZARD_BATCH_FRAME(a, name)	ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_FRAME(a, name, CWizardBatchFrame)
#define		END_FAMILY_SERVER_DOC()				}
#define END_FAMILY_CLIENT_DOC()				}


//=======================================================================
#include "endh.dex"

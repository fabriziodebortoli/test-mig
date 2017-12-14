#pragma once

#include <TbNameSolver\TbNamespaces.h>
#include <TbGeneric\dllmod.h>
#include <TbGeneric\DataObj.h>
#include <TbGenlib\funproto.h>
#include <TbGenlib\addonmng.h>
#include <TbGenlib\parsobj.h>
#include <TbGes\ItemSource.h>
#include <TbGes\Validator.h>
#include <TbGes\DataAdapter.h>

//includere alla fine degli include del .H
#include "beginh.dex"

// useful class
class CTBNamespace;

//per la gestione della configurazione
#define CFG_FREE		_T("Free")  

//per la protezione dei template
#define TPL_NO_PROTECTION		0x00000000 
#define TPL_ADMIN_PROTECTION	0x00000001 
#define TPL_FUNCTION_PROTECTION	0x00000002 
#define TPL_SECURITY_PROTECTION	0x00000004 

//==============================================================================================
// macro for registration of single and multiple interface libraries
//==============================================================================================
#ifdef _TbMultiDll
#define BEGIN_ADDON_WITH_NAME(n) class AddOnInterface##n : public AddOnInterfaceObj {
#define BEGIN_ADDON_WITH_PARAM(p) BEGIN_ADDON_WITH_NAME(p)
#define BEGIN_ADDON_INTERFACE() BEGIN_ADDON_WITH_PARAM(_AddOn_Interface_Of)

#define END_ADDON_WITH_NAME(n) }; extern "C" __declspec(dllexport) AddOnInterfaceObj* WINAPI TaskBuilderAddOn_##n() { return new AddOnInterface##n; } 
#define END_ADDON_WITH_PARAM(p) END_ADDON_WITH_NAME(p)
#define END_ADDON_INTERFACE() END_ADDON_WITH_PARAM(_AddOn_Interface_Of)
#else 
#define BEGIN_ADDON_INTERFACE() class AddOnInterface : public AddOnInterfaceObj { 
#define END_ADDON_INTERFACE() }; extern "C" __declspec(dllexport) AddOnInterfaceObj* WINAPI TaskBuilderAddOn_() { return new AddOnInterface(); }
#endif

//==============================================================================================

//-------------------------------------------------------------------------------------------  Template
#define BEGIN_TEMPLATE()	virtual void AOI_RegisterTemplates	(UINT nResource, CTBNamespace* pNamespace)\
	{\

#define END_TEMPLATE()		}

// definizione del documento
//-------------------------------------------------------------------------------------------  Template
#define BEGIN_DOCUMENT(docname, protections)\
{\
	CSingleExtDocTemplate* pTemplate = NULL;\
	CString sConfig = pNamespace->GetObjectName(CTBNamespace::MODULE);\
	DWORD dwProtections = protections;\
	CString sViewMode;\
	if (dwProtections == 0)	dwProtections = TPL_NO_PROTECTION;\
	CTBNamespace aNamespace(*pNamespace);\
	aNamespace.SetObjectName(CTBNamespace::DOCUMENT, docname, TRUE);

#define END_DOCUMENT()		}

//#define SET_CONFIGURATION(app, config) sConfig = config;

// Registrazione dei templates
//------------------------------------------------------------------------------------
#define Register_Template(t,d,f,v,view_id,frame_id)\
{\
	t = AfxGetBaseApp()->RegisterTemplate\
		(nResource, RUNTIME_CLASS(d),RUNTIME_CLASS(f),RUNTIME_CLASS(v),view_id,frame_id,\
		aNamespace, this, sConfig, dwProtections, sViewMode);\
		t->m_nViewID = view_id; \
	ASSERT(t);\
	GET_DLL_HINSTANCE(t->m_hDllInstance);\
}

// definizione del master template
#define REGISTER_MASTER_TEMPLATE(viewmode,doc,frame,view)\
{\
	sViewMode = viewmode;\
	if (sViewMode.IsEmpty()) sViewMode = szDefaultViewMode;\
	Register_Template(pTemplate, doc,frame,view,0,0);\
}
// definizione del master template
#define REGISTER_MASTER_JSON_TEMPLATE_FOR_VIEW(viewmode,doc,frame,view_ID)\
{\
	sViewMode = viewmode;\
	if (sViewMode.IsEmpty()) sViewMode = szDefaultViewMode;\
	Register_Template(pTemplate, doc,frame, CJsonFormView, view_ID,0);\
}

// definizione del master template
#define REGISTER_MASTER_JSON_TEMPLATE(viewmode,doc, frame_ID)\
{\
	sViewMode = viewmode;\
	nResource = frame_ID;\
	if (sViewMode.IsEmpty()) sViewMode = szDefaultViewMode;\
	Register_Template(pTemplate, doc, CJsonFrame, CJsonFormView, 0, frame_ID);\
}

// definizione dello slave
#define REGISTER_SLAVE_TEMPLATE(doc,frame,view)\
{\
	CSingleExtDocTemplate* pSlaveTemplate = NULL;\
	Register_Template(pSlaveTemplate,doc,frame,view,0,0);\
}
// definizione dello slave
#define REGISTER_SLAVE_JSON_TEMPLATE(frame_ID)\
{\
	CSingleExtDocTemplate* pTemplate = NULL;\
	CString sConfig = pNamespace->GetObjectName(CTBNamespace::MODULE);\
	DWORD dwProtections = TPL_NO_PROTECTION;\
	CString sViewMode;\
	CTBNamespace aNamespace(*pNamespace);\
	CSingleExtDocTemplate* pSlaveTemplate = NULL;\
	nResource = frame_ID;\
	{\
		pSlaveTemplate = AfxGetBaseApp()->RegisterTemplate\
			(nResource, NULL,RUNTIME_CLASS(CJsonSlaveFrame),RUNTIME_CLASS(CJsonFormView),0,frame_ID,\
			aNamespace, this, sConfig, dwProtections, sViewMode);\
			pSlaveTemplate->m_nViewID = 0; \
		ASSERT(pSlaveTemplate);\
		GET_DLL_HINSTANCE(pSlaveTemplate->m_hDllInstance);\
	}\
}

// definizione dello slave
#define REGISTER_BASE_TEMPLATE(doc,frame,view)\
{\
	CSingleExtDocTemplate* pBaseTemplate = NULL;\
	pBaseTemplate = AfxGetBaseApp()->RegisterTemplate\
		(nResource, RUNTIME_CLASS(doc),RUNTIME_CLASS(frame),RUNTIME_CLASS(view),0,0,\
		*pNamespace, this, _T(""), TPL_NO_PROTECTION, _T(""));\
	ASSERT(pBaseTemplate);\
	GET_DLL_HINSTANCE(pBaseTemplate->m_hDllInstance);\
}

// definizione del master in background
#define REGISTER_BKGROUND_TEMPLATE(viewmode,doc)\
{\
	sViewMode = viewmode;\
	if (sViewMode.IsEmpty()) sViewMode = szDefaultViewMode;\
	Register_Template(pTemplate,doc,ADMFrame,ADMView,0,0);\
}

// per inizializzare la dll dopo aver caricato tutte le dll di tutte le AddOnApplication
//-----------------------------------------------------------------------------
#define SET_ADDON_MODULE()	virtual void AOI_SetAddOnMod(){
#define END_SET_ADDON_MODULE()		}

// per inizializzare la dll dopo aver caricato tutte le dll di tutte le AddOnApplication
//-----------------------------------------------------------------------------
#define END_REGISTRATION()	virtual BOOL AOI_EndRegistration(const CTBNamespace& aNamespace){
#define END_END_REGISTRATION()		}

//-----------------------------------------------------------------------------
#define	BEGIN_HOTLINK() void AOI_RegisterHotLinks(const CTBNamespace& aModuleNs) {

#define	DECLARE_HOTLINK(HKLClass, HKLName) {\
		CTBNamespace aNamespace;\
		aNamespace.AutoCompleteNamespace(CTBNamespace::HOTLINK, HKLName, aModuleNs);\
		DeclareFunction (new FunctionDataInterface(aNamespace, RUNTIME_CLASS(HKLClass), NULL));\
	}

#define	DECLARE_HOTLINK_EX(HKLClass, HKLName, HKLControlClass) {\
		CTBNamespace aNamespace;\
		aNamespace.AutoCompleteNamespace(CTBNamespace::HOTLINK, HKLName, aModuleNs);\
		DeclareFunction (new FunctionDataInterface(aNamespace, RUNTIME_CLASS(HKLClass), RUNTIME_CLASS(HKLControlClass)));\
	}

#define END_HOTLINK()	}

//-----------------------------------------------------------------------------
#define	BEGIN_ITEMSOURCE() void AOI_RegisterItemSources(const CTBNamespace& aModuleNs) {
#define	DECLARE_ITEMSOURCE(DSClass, DSName) {\
		ASSERT(RUNTIME_CLASS(DSClass)->IsDerivedFrom(RUNTIME_CLASS(CItemSource)));\
		CTBNamespace aNamespace;\
		aNamespace.AutoCompleteNamespace(CTBNamespace::ITEMSOURCE, DSName, aModuleNs);\
		CString s = aNamespace.ToString();\
		s.MakeLower();\
		m_arItemSources[s] = RUNTIME_CLASS(DSClass);\
	}

#define END_ITEMSOURCE()	}

//-----------------------------------------------------------------------------
#define	BEGIN_CONTROL_BEHAVIOUR() void AOI_RegisterControlBehaviours(const CTBNamespace& aModuleNs) {
#define	DECLARE_CONTROL_BEHAVIOUR(DSClass, DSName) {\
		ASSERT(RUNTIME_CLASS(DSClass)->IsDerivedFrom(RUNTIME_CLASS(CControlBehaviour)));\
		CTBNamespace aNamespace;\
		aNamespace.AutoCompleteNamespace(CTBNamespace::CONTROL_BEHAVIOUR, DSName, aModuleNs);\
		CString s = aNamespace.ToString();\
		s.MakeLower();\
		m_arControlBehaviours[s] = RUNTIME_CLASS(DSClass);\
	}

#define END_CONTROL_BEHAVIOUR()	}

//-----------------------------------------------------------------------------
#define	BEGIN_VALIDATOR() void AOI_RegisterValidators(const CTBNamespace& aModuleNs) {
#define	DECLARE_VALIDATOR(DSClass, DSName) {\
		ASSERT(RUNTIME_CLASS(DSClass)->IsDerivedFrom(RUNTIME_CLASS(CValidator)));\
		CTBNamespace aNamespace;\
		aNamespace.AutoCompleteNamespace(CTBNamespace::VALIDATOR, DSName, aModuleNs);\
		CString s = aNamespace.ToString();\
		s.MakeLower();\
		m_arValidators[s] = RUNTIME_CLASS(DSClass);\
	}

#define END_VALIDATOR()	}

//-----------------------------------------------------------------------------
#define	BEGIN_DATA_ADAPTER() void AOI_RegisterDataAdapters(const CTBNamespace& aModuleNs) {
#define	DECLARE_DATA_ADAPTER(DAClass, DAName) {\
		ASSERT(RUNTIME_CLASS(DAClass)->IsDerivedFrom(RUNTIME_CLASS(CDataAdapter)));\
		CTBNamespace aNamespace;\
		aNamespace.AutoCompleteNamespace(CTBNamespace::DATA_ADAPTER, DAName, aModuleNs);\
		CString s = aNamespace.ToString();\
		s.MakeLower();\
		m_arDataAdapters[s] = RUNTIME_CLASS(DAClass);\
	}

#define END_DATA_ADAPTER()	}


//=======================================================================
#define BEGIN_FUNCTIONS()	virtual void AOI_RegisterFunctions	(CTBNamespace* pNamespace) {

// --- funzioni che gestiscono eventi
#define REGISTER_EVENT_HANDLER(fname,pf)\
 {	CTBNamespace aNamespace;\
	aNamespace.AutoCompleteNamespace(CTBNamespace::EVENTHANDLER, fname, *pNamespace);\
	FunctionDataInterface* pComponent = new FunctionDataInterface(aNamespace, pf);\
	DeclareFunction (pComponent);\
 }

#define REGISTER_JSON_CONTROL(id, rct) AfxGetApplicationContext()->RegisterControl(id, RUNTIME_CLASS(rct));

#define END_FUNCTIONS()		}

//=======================================================================
//---- Controls
static const TCHAR szMLabel[]				= _T("Microarea.Framework.TBApplicationWrapper.MLabel, TBApplicationWrapper");
static const TCHAR szMTab[]					= _T("Microarea.Framework.TBApplicationWrapper.MTab, TBApplicationWrapper");
static const TCHAR szMTabber[]				= _T("Microarea.Framework.TBApplicationWrapper.MTabber, TBApplicationWrapper");
static const TCHAR szMTileManager[]			= _T("Microarea.Framework.TBApplicationWrapper.MTileManager, TBApplicationWrapper");
static const TCHAR szMTileGroup[]			= _T("Microarea.Framework.TBApplicationWrapper.MTileGroup, TBApplicationWrapper");
static const TCHAR szMTilePanel[]			= _T("Microarea.Framework.TBApplicationWrapper.MTilePanel, TBApplicationWrapper");
static const TCHAR szMTilePanelTab[]		= _T("Microarea.Framework.TBApplicationWrapper.MTilePanelTab, TBApplicationWrapper");
static const TCHAR szMTileDialog[]			= _T("Microarea.Framework.TBApplicationWrapper.MTileDialog, TBApplicationWrapper");
static const TCHAR szMBodyEdit[]			= _T("Microarea.Framework.TBApplicationWrapper.MBodyEdit, TBApplicationWrapper");
static const TCHAR szMParsedEdit[]			= _T("Microarea.Framework.TBApplicationWrapper.MParsedEdit, TBApplicationWrapper");
static const TCHAR szMParsedStatic[]		= _T("Microarea.Framework.TBApplicationWrapper.MParsedStatic, TBApplicationWrapper");
static const TCHAR szMParsedCombo[]			= _T("Microarea.Framework.TBApplicationWrapper.MParsedCombo, TBApplicationWrapper");
static const TCHAR szMParsedButton[]		= _T("Microarea.Framework.TBApplicationWrapper.MParsedButton, TBApplicationWrapper");
static const TCHAR szMCheckBox[]			= _T("Microarea.Framework.TBApplicationWrapper.MCheckBox, TBApplicationWrapper");
static const TCHAR szMRadioButton[]			= _T("Microarea.Framework.TBApplicationWrapper.MRadioButton, TBApplicationWrapper");
static const TCHAR szMPushButton[]			= _T("Microarea.Framework.TBApplicationWrapper.MPushButton, TBApplicationWrapper");
static const TCHAR szMParsedListBox[]		= _T("Microarea.Framework.TBApplicationWrapper.MParsedListBox, TBApplicationWrapper");
static const TCHAR szMGroupBox[]			= _T("Microarea.Framework.TBApplicationWrapper.GenericGroupBox, TBApplicationWrapper");
static const TCHAR szMParsedGroupBox[]		= _T("Microarea.Framework.TBApplicationWrapper.MParsedGroupBox, TBApplicationWrapper");
static const TCHAR szMParsedControl[]		= _T("Microarea.Framework.TBApplicationWrapper.MParsedControl, TBApplicationWrapper");
static const TCHAR szMToolbar[]				= _T("Microarea.Framework.TBApplicationWrapper.MToolbar, TBApplicationWrapper");
static const TCHAR szMTreeView[]			= _T("Microarea.Framework.TBApplicationWrapper.MTreeView, TBApplicationWrapper");
static const TCHAR szMPropertyGrid[]		= _T("Microarea.Framework.TBApplicationWrapper.MPropertyGrid, TBApplicationWrapper");
static const TCHAR szMHeaderStrip[]			= _T("Microarea.Framework.TBApplicationWrapper.MHeaderStrip, TBApplicationWrapper");

#define BEGIN_REGISTER_CONTROLS()	virtual void AOI_RegisterParsedControls	(const CTBNamespace& aLibNamespace) { DataType aDataType;
#define DECLARE_CONTROLS_FAMILY(wrapperFamily, caption, defaultType) AfxGetWritableParsedControlsRegistry()->RegisterControlsFamily(wrapperFamily,caption, defaultType);
#define DECLARE_GENERIC_CONTROLS_FAMILY(wrapperFamily, caption, defaultType) AfxGetWritableParsedControlsRegistry()->RegisterControlsFamily(wrapperFamily,caption, defaultType, FALSE);
#define DECLARE_TYPE_DEFAULT_FAMILY(type, wrapperFamily) AfxGetWritableParsedControlsRegistry()->SetTypeDefaultFamily(type, wrapperFamily);

#define	 BEGIN_CONTROLS_TYPE(type) { aDataType = type;
#define	REGISTER_CONTROL(wrapperFamily,fname,localized,cls,style,exStyle,notStyle,notExStyle,isDefault)	\
										{ \
										CRegisteredParsedCtrl *pCtrl = new CRegisteredParsedCtrl(_T(#fname), localized, RUNTIME_CLASS(cls), aDataType, style, exStyle, notStyle, notExStyle); \
										AfxGetWritableParsedControlsRegistry()->RegisterParsedControl(wrapperFamily, pCtrl); \
										if (isDefault) \
											AfxGetWritableParsedControlsRegistry()->AddFamilyDefaultType(wrapperFamily, pCtrl); \
										}	
// default tb control
#define		REGISTER_EDIT_CONTROL(fname,localized,cls,isDefault) REGISTER_CONTROL(szMParsedEdit,fname,localized,cls,0,0,0,0,isDefault)
#define		REGISTER_STATIC_CONTROL(fname,localized,cls,isDefault) REGISTER_CONTROL(szMParsedStatic,fname,localized,cls,0,0,0,0,isDefault)
#define		REGISTER_COMBODROPDOWN_CONTROL(fname,localized,cls,isDefault) REGISTER_CONTROL(szMParsedCombo,fname,localized,cls,CBS_DROPDOWN,0,CBS_DROPDOWNLIST,0,isDefault)
#define		REGISTER_COMBODROPDOWNLIST_CONTROL(fname,localized,cls,isDefault) REGISTER_CONTROL(szMParsedCombo,fname,localized,cls,CBS_DROPDOWNLIST,0,0,0,isDefault)
#define		REGISTER_RADIOBN_CONTROL(fname,localized,cls,isDefault)  REGISTER_CONTROL(szMRadioButton,fname,localized,cls,BS_AUTORADIOBUTTON,0,0,0,isDefault)
#define		REGISTER_CHECKBN_CONTROL(fname,localized,cls,isDefault) REGISTER_CONTROL(szMCheckBox,fname,localized,cls,BS_AUTOCHECKBOX,0,0,0,isDefault)
#define		REGISTER_PUSHBN_CONTROL(fname,localized,cls,isDefault) REGISTER_CONTROL(szMPushButton,fname,localized,cls,BS_PUSHBUTTON,0,0,0,isDefault)
#define		REGISTER_LISTBOX_CONTROL(fname,localized,cls,isDefault) REGISTER_CONTROL(szMParsedListBox,fname,localized,cls,0,0,0,0,isDefault)
#define		REGISTER_GROUP_CONTROL(fname,localized,cls,isDefault)  REGISTER_CONTROL(szMParsedGroupBox,fname,localized,cls,0,0,0,0,isDefault)

#define	 END_CONTROLS_TYPE() }


#define END_REGISTER_CONTROLS()	}

//=======================================================================
//---- Formattatori
#define BEGIN_FORMATTERS()	virtual void AOI_RegisterFormatters(const CTBNamespace& aModuleNs) {
#define		DECLARE_FORMATTER(FormatterClass)\
								{\
									Formatter* pNewFmt = new FormatterClass();\
									pNewFmt->SetOwner(aModuleNs);\
									pNewFmt->SetEditable(FALSE);\
									pNewFmt->RecalcWidths();\
									FormatStyleTable* pTable = const_cast<FormatStyleTable*>(AfxGetStandardFormatStyleTable());\
									pTable->AddFormatter(pNewFmt);\
									AfxGetApplicationContext()->AddFormatterToLoginContext(pNewFmt);
#define		END_DECLARE_FORMATTER()\
								}
#define END_FORMATTERS()	}

//=======================================================================
#define RELEASE_ADDON_CODE(code) virtual BOOL AOI_ReleaseDLL (){ return code; }
//=======================================================================

#include "endh.dex"

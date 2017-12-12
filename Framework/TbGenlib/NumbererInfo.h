#pragma once

#include <TbNameSolver\TBNamespaces.h>
#include <TbGeneric\DataTypesFormatters.h>
#include <TbGeneric\LocalizableObjs.h>

#include "Behaviour.h"
#include "beginh.dex"

class DataObj;

//=============================================================================
class TB_EXPORT CNumbererRequestParams : public CObject
{
};

//=============================================================================
class TB_EXPORT CNumbererRequest : public IBehaviourRequest, public IJsonModelProvider
{
	friend class CParsedCtrl;

	DECLARE_DYNAMIC(CNumbererRequest);

private:
	BOOL					m_bIsPrimaryKey;
	BOOL					m_bNotifyUI;
	CTBNamespace			m_nsEntity;
	DataObj*				m_pData;
	DataObj*				m_pOldData;
	// dati che vengono inizializzati dalla lettura delle info
	DataBool*				m_pNumberingDisabled;
	BOOL					m_pOwnsNumberingDisabled;
	DataBool				m_bDatabaseNumberingDisabled;
	DataBool				m_bEnableCtrlInEditMode;
	CFormatMask				m_FormatMask;
	CNumbererRequestParams*	m_pParams;

public:
	CNumbererRequest (CObject* pOwner, DataObj* pData, const CString& sEntity, DataBool* pDisabled = NULL, CNumbererRequestParams* pParams = NULL);
	~CNumbererRequest();

	const BOOL&		HasNotifyUI     () const;
	const BOOL&		IsPrimaryKey	() const;

	const CTBNamespace&			GetEntity			  () const;
	DataObj*					GetData				  () const;
	DataObj*					GetOldData		 	  () const;
	CNumbererRequestParams*		GetParams			  () const;
	DataBool*					GetNumberingDisabled  ();
	const DataBool&				GetDatabaseNumberingDisabled  () const;
	const DataBool&				GetEnableCtrlInEditMode() const;
	CFormatMask*				GetFormatMask		  ();

	void SetData				(DataObj* value);
	void SetEntity				(const CString& value);
	void SetIsPrimaryKey		(BOOL value);
	void SetNotifyUI			(const BOOL& value);
	void SetOldData				(DataObj* value);
	void SetFormatMask			(const CFormatMask& value);
	void SetNumberingDisabled	(const BOOL& value);
	void SetDatabaseNumberingDisabled	(const BOOL& value);
	void SetEnableCtrlInEditMode(const BOOL& value);
	void SetParams				(CNumbererRequestParams* value);

	virtual void OnRequestExecuted	(const int& nEvent, IBehaviourContext* pContext);

	static CNumbererRequest* GetRequestFor (DataObj* value, IBehaviourContext* pContext, CString sEntity = _T(""));

	// serializzazione json
	virtual void		GetJson(CJsonSerializer& jsonSerializer, BOOL bOnlyWebBound);
	virtual void		SetJson(CJsonParser& jsonParser);
	virtual CString		GetComponentId();
	void				GetJson(CJsonSerializer& jsonSerializer, CWnd* pWnd);
};

//=============================================================================
class TB_EXPORT CDateNumbererRequestParams : public CNumbererRequestParams
{
	DataDate*		m_pDocDate;
	DataDate*		m_pOldDocDate;

public:
	CDateNumbererRequestParams (DataDate* pDataDate);

	DataDate*			GetDocDate			() const;
	DataDate*			GetOldDocDate		() const;

	void SetDocDate		(DataDate* value);
	void SetOldDocDate	(DataDate* value);
};

// public interface class to numberer service methods needed in libraries under TbGes
//=============================================================================
class TB_EXPORT INumbererService : public IBehaviourService 
{
public:
	virtual bool ReadInfo	(IBehaviourRequest* pRequest) = 0;
	virtual bool ReadNumber	(IBehaviourRequest* pRequest) = 0;
	virtual CArray<CString>* GetLinkedControls() = 0;
};

//=============================================================================
class TB_EXPORT CFormatMaskLegendaMng : public CObject
{
	friend class CFormatMaskLegendaDlg;
	CDialog*	m_pDlg;
	CWnd*		m_pParent;

public:
	CFormatMaskLegendaMng	(CWnd* pParent);
	~CFormatMaskLegendaMng	();

	void ToggleLegenda(BOOL bValue);
};

//=============================================================================
class CFormatMaskLegendaDlg : public CLocalizableDialog
{
	friend class CFormatMaskLegendaMng;

protected:
	CFormatMaskLegendaMng* m_pManager;

public:
	CFormatMaskLegendaDlg	(CFormatMaskLegendaMng* pManager);

protected:
	virtual BOOL OnInitDialog	();
	virtual void OnCancel		();
	
	afx_msg HBRUSH OnCtlColor (CDC* pDC, CWnd* pWnd, UINT nCtlColor);
	DECLARE_MESSAGE_MAP();
	DECLARE_DYNAMIC(CFormatMaskLegendaDlg)
};

#include "endh.dex"

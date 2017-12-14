
#include "StdAfx.h"

#include <TBNamesolver\TBWinThread.h>

#include "ExternalControllerInfo.h"
#include "ParsObj.h"
#include "AutoExprDlg.h"

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CExternalControllerInfo, CObject)

//-----------------------------------------------------------------------------
CExternalControllerInfo::CExternalControllerInfo(void)
:
	m_bEditAndRun		(FALSE),
	m_RunningStatus		(TASK_NO_RUN),
	m_ControllingMode	(NONE)
{
	m_arLockedValues.SetOwns(FALSE);
}

//-----------------------------------------------------------------------------
CExternalControllerInfo::~CExternalControllerInfo(void)
{
}

//-----------------------------------------------------------------------------
void CExternalControllerInfo::ClearData()
{
	m_Data.GetParameters().RemoveAll();
}

//-----------------------------------------------------------------------------
void CExternalControllerInfo::UnlockValues()
{
	m_Scheduler.CheckMessage(); //frees pending "OnChange"

	for (int i = 0; i < m_arLockedValues.GetCount(); i++)
		m_arLockedValues[i]->SetValueLocked(FALSE);
	
	m_arLockedValues.RemoveAll(); 
}

//-----------------------------------------------------------------------------
BOOL CExternalControllerInfo::IsAutoExpression(CAbstractCtrl* pCtrl)
{
	CString strNamespace = pCtrl->GetPublicName();

	DataObj *pValue = m_Data.GetParamValue(strNamespace);
	DataObj *pControlData = pCtrl->GetCtrlData();
	return pValue &&
			pControlData &&
			pControlData->IsKindOf(RUNTIME_CLASS(DataDate)) &&
			(pValue->IsKindOf(RUNTIME_CLASS(DataStr))) &&
			::IsAutoExpression (((DataStr*)pValue)->GetString());
			
}

//-----------------------------------------------------------------------------
// Recupera il valore dal control
void CExternalControllerInfo::RetrieveControlData(CAbstractCtrl* pCtrl)
{
	ASSERT(pCtrl);

	CString strNamespace = pCtrl->GetPublicName(); 
	DataObj *pData = NULL;

	if (!pCtrl->IsMultiValue())
	{
		DataStr strAutoExpr;
		if (pCtrl->IsAutomaticExpression())
		{
			strAutoExpr = pCtrl->GetAutomaticExpression();
			pData = &strAutoExpr;
		}
		else
		{
			pData = pCtrl->GetCtrlData();
		}

		RetrieveControlData(strNamespace, pData);
		return;
	}

	CStringArray arNames;
	if (pCtrl->EnumColumnName(arNames, FALSE) == 0)
	{
		//ASSERT(FALSE);
		return;
	}
	//pulisco tutti i parametri della lista per gestire i casi di cancellazione
	m_Data.RemoveParamsStartingWith(strNamespace);
	CString sName;
	for (int r=0; r < pCtrl->GetRowNumber(); r++)
	{
		for (int n=0; n < arNames.GetSize(); n++)
		{
			sName = arNames[n];
			CString sCellNamespace;
			sCellNamespace.Format(_T("%s.%d.%s"), (LPCTSTR)strNamespace, r, sName);
			pData = pCtrl->GetCtrlData(sName, r);

			RetrieveControlData(sCellNamespace, pData);
		}
	}
}

//-----------------------------------------------------------------------------
void CExternalControllerInfo::RetrieveControlData(const CString &strName, DataObj *pValue)
{
	ASSERT(pValue);
	if (pValue == NULL || strName.IsEmpty())
		return;
	DataObj *pData = m_Data.GetParamValue(strName);
	if (pData && pData->GetDataType() == pValue->GetDataType()) //data type should change if user inserts an auto expression
	{ 
		//if (pValue->IsUpperValue && pValue->GetDataType() == DataType::String)
		//{
		//	//TODO RICCARDO
		//	//((DataStr*)pValue)->TrimUpperLimit();
		//}
		pData->Assign(*pValue);
	}
	else
		m_Data.InternalAddParam(strName, pValue, TRUE);	
}

//-----------------------------------------------------------------------------
void CExternalControllerInfo::LockValue(DataObj *pValue)
{
	if (!m_bEditAndRun)//se sono in una condizione di edit e run (vedi report con TestManager) non effettuo il lock
	{
		pValue->SetValueLocked();
		m_arLockedValues.Add(pValue);
	}
}

//-----------------------------------------------------------------------------
// Valorizza il control
BOOL CExternalControllerInfo::ValorizeControlData(CAbstractCtrl* pCtrl)
{
	if (pCtrl == NULL) return FALSE;

	CString strNamespace = pCtrl->GetPublicName();
	
	DataObj *pValue = NULL;
	
	if (IsAutoExpression(pCtrl))
	{
		BOOL bValorized = FALSE;
		DataStr strAuto;
		pValue = &strAuto;

		DataObj* pData = m_Data.GetParamValue(strNamespace);
		if (pData && pValue)
		{
			ValorizeControlData(pData, pValue);
			bValorized = TRUE;
		}
		
		// se sono in stato di running, metto il valore effettivo, altrimenti l'esspressione
		if (IsRunning())
		{
			DataObj* pTargetValue = pCtrl->GetCtrlData();
			GetAutoExpressionValue(strAuto.GetString(), pTargetValue);
			LockValue(pTargetValue);
		}
		else
			pCtrl->SetAutomaticExpression(strAuto.GetString());
		
		return bValorized;
	}

	if (!pCtrl->IsMultiValue())
	{
		DataObj* pData = m_Data.GetParamValue(strNamespace);
		if (!pData)
			return FALSE;
		
		pValue = pCtrl->GetCtrlData();
		if (!pValue)
			return FALSE;
	
		ValorizeControlData(pData, pValue);
		return TRUE;
	}
	
	CStringArray arNames;
	if (pCtrl->EnumColumnName(arNames) == 0)
	{
		//ASSERT(FALSE);
		return FALSE;
	}
	CString sName;
	BOOL bAtLeastOne = FALSE;
	for (int r=0; r < pCtrl->GetRowNumber(); r++)
	{
		for (int n=0; n < arNames.GetSize(); n++)
		{
			sName = arNames[n];
			CString sCellNamespace;
			sCellNamespace.Format(_T("%s.%d.%s"), (LPCTSTR)strNamespace, r, sName);
			DataObj* pData = m_Data.GetParamValue(sCellNamespace);
			if (!pData)
				continue;

			pValue = pCtrl->GetCtrlData(sName, r);

			if (pValue)
			{
				ValorizeControlData(pData, pValue);
				bAtLeastOne = TRUE;
			}
		}
	}
	return bAtLeastOne;
}

//-----------------------------------------------------------------------------
void CExternalControllerInfo::ValorizeControlData(const DataObj* pControlData, DataObj *pValue)
{
	pValue->Assign(*pControlData);
	LockValue(pValue);
}

//-----------------------------------------------------------------------------
CString CExternalControllerInfo::GetXmlString()
{
	DataStr strXml;
	m_Data.UnparseArguments(strXml);
	return strXml.GetString();
}

//-----------------------------------------------------------------------------
void CExternalControllerInfo::SetFromXmlString(const CString& strXml)
{
	m_Data.ParseArguments(strXml);
}

//-----------------------------------------------------------------------------
void CExternalControllerInfo::WaitUntilFinished()
{
	CTBWinThread::LoopUntil(&m_Finished);
}

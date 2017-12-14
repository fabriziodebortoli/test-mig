#include "stdafx.h"

#include <io.h>

#include "FunctionObjectsInfo.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//----------------------------------------------------------------------------------------------
//							CFunctionObjectsDescription
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CFunctionObjectsDescription, CObject)

//----------------------------------------------------------------------------------------------
CFunctionObjectsDescription::CFunctionObjectsDescription ()
	:
	m_bLoaded	 (false)
{
}

//----------------------------------------------------------------------------------------------
CFunctionDescription* CFunctionObjectsDescription::GetFunctionInfo (const CTBNamespace& aNamespace) const
{
	return (CFunctionDescription*) m_arFunctions.GetInfo(aNamespace);
}

//----------------------------------------------------------------------------------------------
CFunctionDescription* CFunctionObjectsDescription::GetFunctionInfo (const CString& sName) const
{
	return (CFunctionDescription*) m_arFunctions.GetInfo(sName);
}

//----------------------------------------------------------------------------------------------
CFunctionDescription* CFunctionObjectsDescription::GetFunctionInfo (const int& nIndex) const
{
	return (CFunctionDescription*) m_arFunctions.GetAt(nIndex);
}

//----------------------------------------------------------------------------------------------
const CBaseDescriptionArray& CFunctionObjectsDescription::GetFunctions() const
{
	return m_arFunctions;
}

//----------------------------------------------------------------------------------------------
void CFunctionObjectsDescription::AddFunction (CFunctionDescription* pDescri)
{
	m_arFunctions.Add (pDescri);
}

//----------------------------------------------------------------------------------------------
//							CFunctionDescriptionArray
//----------------------------------------------------------------------------------------------
CFunctionDescription* CFunctionDescriptionArray::GetFunction (const CString& sName) const
{
	for (int i=0; i < GetSize(); i++)
	{
		CFunctionDescription* pF = GetAt(i);
		CString sFName = pF->GetName();
		int idx = sFName.Find('_');
		if (idx > 0)
			sFName = sFName.Mid(idx + 1);
		if (_tcsicmp(sName, sFName) == 0)
			return pF;
	}
	return NULL;
}

//----------------------------------------------------------------------------------------------
CBaseDescriptionArray*	CFunctionObjectsDescription::GetFunctionsByName	(const CString& sName) const
{
	CBaseDescriptionArray* pArray = NULL;

	CFunctionDescription* pDescri;
	for (int i=0; i <= m_arFunctions.GetUpperBound(); i++)
	{
		pDescri = (CFunctionDescription*) m_arFunctions.GetAt(i);
		if (!pDescri || _tcsicmp(pDescri->GetName(), sName) != 0)
			continue;

		if (!pArray)
		{
			pArray = new CBaseDescriptionArray();
			pArray->SetOwns(FALSE);
		}

		pArray->Add(pDescri);
	}

	return pArray;
}

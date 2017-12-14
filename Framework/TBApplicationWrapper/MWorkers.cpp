#include "StdAfx.h"

#include <TbGeneric/DataObj.h>
//#include <TbNameSolver/LoginContext.h>

#include "MWorkers.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace Microarea::Framework::TBApplicationWrapper;

/////////////////////////////////////////////////////////////////////////////
// 				class MWorker Implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
MWorker::MWorker(CWorker* pWorker) 
{  
	m_pWorker = pWorker; 
}

//-----------------------------------------------------------------------------
long MWorker::WorkerID::get()
{ 
	return m_pWorker->GetWorkerID();
}

//-----------------------------------------------------------------------------
System::String^ MWorker::Name::get()
{ 
	return gcnew System::String (m_pWorker->GetName());
}

//-----------------------------------------------------------------------------
System::String^ MWorker::LastName::get()
{ 
	return gcnew System::String (m_pWorker->GetLastName());
}

//-----------------------------------------------------------------------------
System::String^ MWorker::CompanyLogin::get()
{ 
	return gcnew System::String (m_pWorker->GetCompanyLogin());
}

//-----------------------------------------------------------------------------
bool MWorker::Disabled::get()
{ 
	return   (m_pWorker->GetDisabled());
}

/////////////////////////////////////////////////////////////////////////////
// 				class MWorkersTable Implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
MWorkersTable::MWorkersTable(CWorkersTableObj* pWorkersTable)
{
	m_pWorkersTable = pWorkersTable;
}

//-----------------------------------------------------------------------------
MWorkersTable::MWorkersTable(IntPtr pWorkersTable) 
{  
	m_pWorkersTable = (CWorkersTableObj*)pWorkersTable.ToInt64();
}

//-----------------------------------------------------------------------------
List<MWorker^>^ MWorkersTable::Workers::get()
{
	List<MWorker^>^ workersList = gcnew List<MWorker^>();
	CWorker* pWorker = NULL;
	if (m_pWorkersTable)
	{
		for (int i = 0; i <= m_pWorkersTable->GetWorkersCount(); i++)
		{
			pWorker = m_pWorkersTable->GetWorkerAt(i);
			if (pWorker)
				workersList->Add(gcnew MWorker(pWorker));
		}
	}
	return workersList;
}

//-----------------------------------------------------------------------------
MWorker^ MWorkersTable::GetWorker(long workerID)
{
	CWorker* pWorker = NULL;
	if (m_pWorkersTable)
		pWorker = m_pWorkersTable->GetWorker(workerID);
	return (pWorker) ? gcnew MWorker(pWorker) : nullptr;	
}
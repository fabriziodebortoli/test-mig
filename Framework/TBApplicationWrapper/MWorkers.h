#pragma once

#include <TbNameSolver/LoginContext.h>

namespace Microarea { namespace Framework { namespace TBApplicationWrapper
{
	/// <summary>
	/// Internal Use
	/// </summary>
	//=============================================================================
	public ref class MWorker 
	{	
	private:
		CWorker* m_pWorker;		

	public:
		MWorker(CWorker*);
		
		/// <summary>
		/// Internal Use
		/// </summary>
		property long		WorkerID	{ long get (); }
      
		/// <summary>
		/// Internal Use
		/// </summary>
		property System::String^	Name		{ System::String^ get (); }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		property System::String^	LastName	{ System::String^ get (); }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		property System::String^	CompanyLogin{ System::String^ get (); }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		property bool Disabled{ bool get (); }
		
	};

	/// <summary>
	/// Internal Use
	/// </summary>
	//=============================================================================
	public ref class MWorkersTable 
	{	
	private:
		CWorkersTableObj* m_pWorkersTable;		

	public:
		MWorkersTable(System::IntPtr pWorkersTable);
		MWorkersTable(CWorkersTableObj* pWorkersTable);

		/// <summary>
		/// Internal Use
		/// </summary>
		property System::Collections::Generic::List<MWorker^>^ Workers	{ System::Collections::Generic::List<MWorker^>^ get(); }
	
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		MWorker^	GetWorker(long workerID);
	};
}
}
}



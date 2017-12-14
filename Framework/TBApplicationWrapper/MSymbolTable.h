#pragma once

#include <TbParser\SymTable.h>

//includere alla fine degli include del .H
//#include "beginh.dex"

using namespace Microarea::TaskBuilderNet::Interfaces::Model;

namespace Microarea { namespace Framework	{ namespace TBApplicationWrapper 
{

	public ref class MSymTable 
	{
	protected:
		SymTablePtr* m_ppSymTable;
		bool m_ownTable;

		SymTable* GetSymTable();
		
	public:
		MSymTable ();
		MSymTable (System::IntPtr	pSymTable,	bool ownTable);
		
		~MSymTable();	
		!MSymTable();

		IDataObj^ GetFieldValueByName	(System::String^ name);
		IDataObj^ GetFieldValueByTag	(System::String^ tag);

		void Add (System::String^ name, System::String^ tag, IDataObj^ o);
		void Clear ();
	};

}}}

//#include "endh.dex"

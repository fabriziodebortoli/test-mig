#pragma once
#include <vcclr.h>
#include <tbges\hotlink.h>
#include "MHotLink.h"

using namespace Microarea::Framework::TBApplicationWrapper;

//===================================================================================
class EBHotKeyLink : public HotKeyLink
{
	DECLARE_DYNAMIC (EBHotKeyLink)

private:
	gcroot<MHotLink^> m_pMHotLink;

public:
	EBHotKeyLink(System::String^ tableName, MHotLink^ mHotLink);
	~EBHotKeyLink(void);

public:
	void InitializeXmlDescription ();

	bool SearchOnLinkUpperProxy	();
	bool SearchOnLinkLowerProxy	();

	virtual BOOL		IsValid					();

protected:
	virtual void		OnDefineQuery			(SelectionType nQuerySelection = DIRECT_ACCESS);
	virtual void		OnPrepareQuery			(::DataObj*, SelectionType nQuerySelection = DIRECT_ACCESS);
	virtual void		OnCallLink				();
	virtual ::DataObj*	GetDataObj				() const;
	virtual void		OnRadarRecordAvailable	();

	virtual BOOL		OnValidateRadarSelection(SqlRecord* pRec);
	virtual CString		FormatComboItem			(SqlRecord* pRec);
	

};
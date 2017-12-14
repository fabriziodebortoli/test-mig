#pragma once

#include <TbNameSolver\Diagnostic.h>
#include <TbOleDb\SqlRecoveryManager.h>

//includere alla fine degli include del .H
#include "beginh.dex"

// object to perform recovery operation
//=============================================================================
class TB_EXPORT DocRecoveryManager : public SqlRecoveryManager
{
private:
	Array	m_SnapShots;

public:
	DocRecoveryManager ();

private:
	BOOL SaveSnapshots		();
	BOOL CloseAllActivities	();
	BOOL PlaySnapshots		();

	void FreezeApplication	();
	void UnFreezeApplication();

protected:
	virtual BOOL OnPerformRecoveryActivity	();
};

#include "endh.dex"

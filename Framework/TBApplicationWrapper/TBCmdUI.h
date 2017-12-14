#pragma once

#include <tbgeneric\tbcmdui.h>
#include "TBToolStripMenuItem.h"
class CProxyCTBCmdUI : public CTBCmdUI
{
public:
	CProxyCTBCmdUI(int commandID) : CTBCmdUI(commandID) {}

	void AssignUpdates(Microarea::Framework::TBApplicationWrapper::TBToolStripMenuItem^ menuItem);
};

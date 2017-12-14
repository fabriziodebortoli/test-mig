
#pragma once

#include <TBGeneric\DataObj.h>

#include "beginh.dex"
TB_EXPORT DataBool	ExecOpenReport(DataStr& reportNamespace, DataStr& reportPath);
TB_EXPORT DataBool 	ExecOpenFormatter	();
TB_EXPORT DataBool 	ExecOpenFont		();
TB_EXPORT DataBool 	ExecOpenText		();
TB_EXPORT DataBool 	ExecManageFile		();
TB_EXPORT DataBool 	EnumsViewer			();
TB_EXPORT void		ShowAboutFramework	();

// imposta la data di applicazione
TB_EXPORT void		SetApplicationDate				(DataDate aData);
TB_EXPORT void	 	SetApplicationDateToSystemDate	();
TB_EXPORT DataDate	SetApplicationDate2				(DataDate aData);
TB_EXPORT void		SetApplicationDateIMago				(DataDate aData);

#include "endh.dex"
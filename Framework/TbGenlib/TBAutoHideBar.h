#pragma once

#include "beginh.dex"

class CTBNamespace;

//======================================================================
class TB_EXPORT CTBAutoHideBar : public CBCGPAutoHideToolBar
{
	DECLARE_DYNCREATE(CTBAutoHideBar)

private:	//private members
	int		m_nSize;

public:	//constructor/s
	CTBAutoHideBar();

	
public:	//public methods
			void	SetSize				(int nSize);
			BOOL	IsLayoutSuspended	();

protected:	//virtual methods
	virtual CSize	CalcFixedLayout		(BOOL bStretch, BOOL bHorz);
	virtual CSize	StretchControlBar	(int nLength, BOOL bVert);
};

#include "endh.dex"

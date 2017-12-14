#include	"stdafx.h"

#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\TBThemeManager.h>
#include <TbGenlib\TBToolBar.h>

//	local	declarations
#include	"CommonImages.h"	

#ifdef	_DEBUG
#undef	THIS_FILE
static	char	THIS_FILE[] = __FILE__;
#endif


static TCHAR szGlyphFolder[] = _T("Glyph");
static TCHAR szExtIconNamespace[] = _T("Image.Extensions.ExtensionsImages.Images.%s.%s.png");

//----------------------------------------------------------------------------
CString ExtensionsIcon(const CString& szIcon, IconSize size /*= TILEMNG*/)
{
	return ComposeIconNamespace(szExtIconNamespace, szIcon, size);
}

//----------------------------------------------------------------------------
CString ExtensionsGlyph(const CString& szGlyph)
{
	return cwsprintf(szExtIconNamespace, szGlyphFolder, szGlyph);
}
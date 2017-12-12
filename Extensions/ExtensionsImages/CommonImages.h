#pragma once

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>
#include "beginh.dex"

//DMS: toolbar button
static TCHAR szIconDevice[]		= _T("Device");
static TCHAR szIconFilter[]		= _T("Filter");
static TCHAR szIconRepository[] = _T("Repository");
static TCHAR szIconPapery[]		= _T("Papery");
static TCHAR szIconCheckIn[]	= _T("CheckIn");
static TCHAR szIconCheckOut[]	= _T("CheckOut");

//RowSecurity : toolbar button
static TCHAR szIconDenyGrant[]		= _T("DenyGrant");
static TCHAR szIconReadGrant[]		= _T("ReadGrant");
static TCHAR szIconFullGrant[]		= _T("FullGrant");
static TCHAR szIconLock[]			= _T("Lock");
static TCHAR szIconUnlock[]			= _T("Unlock");


// DMS: Glyph estensioni file 
static TCHAR szGlyphExtAVI[]		= _T("Ext_AVI20x20");
static TCHAR szGlyphExtBMP[]		= _T("Ext_BMP20x20");
static TCHAR szGlyphExtDefault[]	= _T("Ext_Default20x20");
static TCHAR szGlyphExtDOC[]		= _T("Ext_DOC20x20");
static TCHAR szGlyphExtGIF[]		= _T("Ext_GIF20x20");
static TCHAR szGlyphExtGZIP[]		= _T("Ext_GZIP20x20");
static TCHAR szGlyphExtHTML[]		= _T("Ext_HTML20x20");
static TCHAR szGlyphExtJPG[]		= _T("Ext_JPG20x20");
static TCHAR szGlyphExtMAIL[]		= _T("Ext_MAIL20x20");
static TCHAR szGlyphExtMP3[]		= _T("Ext_MP320x20");
static TCHAR szGlyphExtMPEG[]		= _T("Ext_MPEG20x20");
static TCHAR szGlyphExtPDF[]		= _T("Ext_PDF20x20");
static TCHAR szGlyphExtPPT[]		= _T("Ext_PPT20x20");
static TCHAR szGlyphExtPNG[]		= _T("Ext_PNG20x20");
static TCHAR szGlyphExtRAR[]		= _T("Ext_RAR20x20");
static TCHAR szGlyphExtRTF[]		= _T("Ext_RTF20x20");
static TCHAR szGlyphExtTIFF[]		= _T("Ext_TIFF20x20");
static TCHAR szGlyphExtTXT[]		= _T("Ext_TXT20x20");
static TCHAR szGlyphExtWAV[]		= _T("Ext_WAV20x20");
static TCHAR szGlyphExtWMV[]		= _T("Ext_WMV20x20");
static TCHAR szGlyphExtXLS[]		= _T("Ext_XLS20x20");
static TCHAR szGlyphExtXML[]		= _T("Ext_XML20x20");
static TCHAR szGlyphExtZIP[]		= _T("Ext_ZIP20x20");
static TCHAR szGlyphExtPapery[]		= _T("Ext_Papery20x20");

// info file
static TCHAR szGlyphCheckOut[]		= _T("CheckOut20x20");
static TCHAR szGlyphWoormReport[]	= _T("WoormReport20x20");
static TCHAR szGlyphAttachment[]	= _T("Attachment20x20");

static TCHAR szGlyphCategory[]			= _T("Category20x20");
static TCHAR szGlyphCategoryDisabled[]	= _T("CategoryDisabled20x20");

// SOSConnector
static TCHAR szGlyphSOSDocIdle[]		= _T("SOS_DocIdle20x20");
static TCHAR szGlyphSOSDocKO[]			= _T("SOS_DocKo20x20");
static TCHAR szGlyphSOSDocRdy[]			= _T("SOS_DocRdy20x20");
static TCHAR szGlyphSOSDocSent[]		= _T("SOS_DocSent20x20");
static TCHAR szGlyphSOSDocSign[]		= _T("SOS_DocSign20x20");
static TCHAR szGlyphSOSDocStd[]			= _T("SOS_DocStd20x20");
static TCHAR szGlyphSOSDocTemp[]		= _T("SOS_DocTemp20x20");
static TCHAR szGlyphSOSDocToResend[]	= _T("SOS_DocToResend20x20");
static TCHAR szGlyphSOSDocToSend[]		= _T("SOS_DocToSend20x20");

//RowSecurity gliph
static TCHAR szGlyphResourceDeny[] = _T("ResourceDeny");
static TCHAR szGlyphResourceRead[] = _T("ResourceRead");
static TCHAR szGlyphResourceFull[] = _T("ResourceFull");

static TCHAR szGlyphWorkerDeny[] = _T("WorkerDeny");
static TCHAR szGlyphWorkerRead[] = _T("WorkerRead");
static TCHAR szGlyphWorkerFull[] = _T("WorkerFull");


TB_EXPORT CString ExtensionsIcon	(const CString& szIcon, IconSize size);
TB_EXPORT CString ExtensionsGlyph	(const CString& szIcon);

#include "endh.dex"

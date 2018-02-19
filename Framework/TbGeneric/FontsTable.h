#pragma once

#include <TbNameSolver\TBNamespaces.h>

#include "Array.h"
#include "TbStrings.h"
#include "DataObj.h"

//includere alla fine degli include del .H
#include "beginh.dex"

typedef	int FontIdx;

// font standard di default utilizzati internamente dal framework
//===========================================================================
#define FNT_ERROR			-1
#define FNT_DEFAULT			_NS_FONT("Text")
#define FNT_STANDARD		_NS_FONT("Normal")
#define FNT_CELL_STRING		_NS_FONT("CellText")
#define FNT_CELL_NUM		_NS_FONT("CellNumber")
#define FNT_TOTAL_STRING	_NS_FONT("TotalText")
#define FNT_TOTAL_NUM		_NS_FONT("TotalNumber")
#define FNT_SUBTOTAL_STRING	_NS_FONT("SubTotalText")
#define FNT_SUBTOTAL_NUM	_NS_FONT("SubTotalNumber")
#define FNT_TABLE_TITLE		_NS_FONT("TableTitle")
#define FNT_TEXT			_NS_FONT("Text")
#define FNT_LABEL			_NS_FONT("Description")
#define FNT_COLUMN_TITLE	_NS_FONT("ColumnTitle")


//------------------------------------------------------------------------------
#define FNT_HYPERLINK							_NS_FONT("<Hyperlink>")
#define FNT_HYPERLINK_BROWSED					_NS_FONT("<Hyperlink_Browsed>")
#define FNT_CUSTOM_SEGMENT_HYPERLINK			_NS_FONT("_Hyperlink")
#define FNT_CUSTOM_SEGMENT_HYPERLINK_BROWSED	_NS_FONT("_Hyperlink_Browsed")

//------------------------------------------------------------------------------

inline BOOL	IsShowedAsString (const DataType& aType)
{
	return
		aType == DATA_NULL_TYPE	||
		aType == DATA_STR_TYPE	||
		aType == DATA_BOOL_TYPE	||
		aType == DATA_ENUM_TYPE ||
		aType == DATA_TXT_TYPE || 
		aType == DATA_DATE_TYPE ;
}

//=============================================================================
class TB_EXPORT FontStyle : public CObject 
{
	friend class CFontStylesDlg;

public:
	enum FontStyleSource { FROM_STANDARD, FROM_CUSTOM, FROM_WOORM, FROM_WOORM_TEMPLATE };

private:
	CString			m_strStyleName;
	LOGFONT			m_LogFont;
	COLORREF		m_rgbColor;
	BOOL			m_bChanged;
	BOOL			m_bDeleted;
	CTBNamespace	m_OwnerModule;
	FontStyleSource m_FromAndTo;
	CStringArray	m_LimitedContextArea;
	FontStyle*		m_pStandardFont;

public:
	// constructors
	FontStyle
	(
	  const	CString&			strStyleName,
	  const	CString&			strFaceName,
	  const	CTBNamespace&		aNamespace,
	  const	FontStyleSource&	aFromTo,
			int					nHeight,
			int					nWeight,
			BOOL				bItalic,
			BOOL				bUnderline		= FALSE,
			BOOL				bStrikeout		= FALSE,
            BOOL				bChanged		= FALSE,
			int					nEscapementOrientation = 0
	);
	FontStyle (const FontStyle&);
	~FontStyle();
  	const	FontStyle& operator	= (const FontStyle&);

	// read method
	const CString&			GetStyleName	() const { return m_strStyleName; }
	const CString			GetTitle		() const;
	const CTBNamespace&		GetOwner		() const { return m_OwnerModule; }
	const FontStyleSource	GetSource		() const { return m_FromAndTo; }
	const BOOL&				IsChanged		() const { return m_bChanged; }
	const BOOL&				IsDeleted		() const { return m_bDeleted; }
	const CString			GetLimitedArea	() const;
	const CStringArray&		GetLimitedAreas	() const;

	LOGFONT 	GetLogFont		() const;
	COLORREF	GetColor		() const { return m_rgbColor; }
	int			GetHeight		() const;
	CSize		GetStringWidth 	(CDC* pDC, int len = 1)		const;
	CSize		GetStringWidth 	(CDC* pDC, const CString&)	const;
	int			GetStringWidth2	(CDC* pDC, const CString&)	const;

	BOOL 		CreateFont		(CFont&) const;

	BOOL		IsItalic		() const { return m_LogFont.lfItalic; }
	BOOL		IsBold			() const { return m_LogFont.lfWeight == FW_BOLD; }
	BOOL		IsUnderline		() const { return m_LogFont.lfUnderline; }

	BOOL		GetOrientation	() const { return m_LogFont.lfOrientation; }

	// set methods
	void		SetColor		(COLORREF aColor)			{ m_rgbColor = aColor; }
	void		SetLogFont		(LOGFONT lf);
	void		SetChanged		(const BOOL& bValue)		{ m_bChanged = bValue; }
	void		SetDeleted		(const BOOL& bValue)		{ m_bDeleted = bValue; }
	void		SetOwner		(const CTBNamespace& aOwner){ m_OwnerModule = aOwner; }
	void		SetLimitedArea	(const CString& sArea);
	void		SetLimitedAreas	(const CStringArray& aAreas);
	void		SetStandardFont	(FontStyle* pFont);
	void		SetSource		(FontStyleSource src)		{ m_FromAndTo = src; }

	BOOL		IsNoneFont		() const;
public:
	// virtual function
	virtual	int		Compare		(const FontStyle& Fnt) const;
	virtual	void	Assign		(const FontStyle& Fnt);

public:
	// operators for reference
 	static const TCHAR s_szFontDefault[]; //ghost font style used by report template
};

//=============================================================================
class TB_EXPORT FontStylesGroup : public CObject 
{
	friend class CFontStylesDlg;
	friend class FontStyleTable;

private:
	CString		m_strStyleName;
	Array		m_FontsStyles;

public:
	FontStylesGroup (const CString& sName);

public:
	const CString&	GetStyleName	() const { return m_strStyleName; }
	const CString	GetTitle		() ;

	FontStyle*	GetFontStyle	(FontIdx idxFont);
	FontStyle*	GetFontStyle	(CTBNamespace* pContext);

	const Array& GetFontStyles	() const;

	// restituisce idx del array del gruppo del fonts
	FontIdx		GetFontIdx		(FontStyle* FontFrom, const FontStyle::FontStyleSource& aFormatSource);
	int			AddFont			(FontStyle* pFont);
	void		DeleteFont		(FontStyle* pFontStyleToDel);

	// operators
   	const FontStylesGroup& operator = (const FontStylesGroup&);

public:
	// virtual functions
	virtual	void Assign	 (const FontStylesGroup& Fnt);

protected:
	FontStyle*	GetFontStyle		(const FontStyle::FontStyleSource& aSource);
	FontStyle*	BestFontForContext	(CTBNamespace* pContext) const;
	BOOL		HasPriority			(const FontStyle* pOld, const FontStyle* pNew, const CTBNamespace* pContext) const;
};

// mantiene le informazioni base del file caricato
//=============================================================================
class TB_EXPORT FontStyleFile : public CObject
{ 
private:
	CTBNamespace				m_Owner;
	FontStyle::FontStyleSource	m_Source;
	SYSTEMTIME					m_dLastWrite;

public:
	FontStyleFile	(
						const CTBNamespace& aOwner,
						const FontStyle::FontStyleSource& aSource,
						const SYSTEMTIME& aFileDate
					);
	FontStyleFile	(const FontStyleFile&);

public:
	const CTBNamespace&					GetOwner	() { return m_Owner; }
	const FontStyle::FontStyleSource&	GetSource	() { return m_Source; }
	const SYSTEMTIME&					GetFileDate	() { return m_dLastWrite; }

	// metodi di scrittura
	void SetFileDate (const SYSTEMTIME& aDate);

public:
	// operators
   	const FontStyleFile& operator = (const FontStyleFile&);
};

//=============================================================================        
class TB_EXPORT FontAliasTable
{
public:
	class FontAlias: public CObject
	{
	public:
		CString m_sSrcName;
		CString m_sDstName;
		int		m_nSrcSize;
		int		m_nDstSize;

		FontAlias (const CString& sSrcName, const CString& sDstName, int nSrcSize = 0, int nDstSize = 0)
			:
			m_sSrcName (sSrcName), 
			m_sDstName (sDstName),
			m_nSrcSize (nSrcSize),
			m_nDstSize (nDstSize)
			{}

		FontAlias (const FontAlias& source)
			:
			m_sSrcName (source.m_sSrcName), 
			m_sDstName (source.m_sDstName),
			m_nSrcSize (source.m_nSrcSize),
			m_nDstSize (source.m_nDstSize)
			{}
	};

protected:
	Array	m_arFontAlias;
public:
	FontAliasTable () {}
	FontAliasTable (const FontAliasTable& source)
	{
		for (int i = 0; i < source.m_arFontAlias.GetSize(); i++)
		{
			m_arFontAlias.Add(new FontAlias(*source.GetAt(i)));
		}
	}

	FontAlias* GetAt (int pos) const  { return (FontAlias*)(m_arFontAlias.GetAt(pos)); }

	int Add (const CString& sSrcName, const CString& sDstName, int nSrcSize = 0, int nDstSize = 0);
	
	int Find (const CString& sName, int nSize = 0, int startPos = 0) const;
	CString LookupFaceName (const CString& sName) const;
};


//=============================================================================
class TB_EXPORT FontStyleTable : public Array, public CTBLockable
{   
private:
	BOOL	m_bModified;
	Array	m_arLoadedFiles;

	BOOL	m_bUseVCenterBottomAlignInWoormFields;
	int		m_nSizeOfDescriptionFont;

	FontAliasTable m_FontAliasTable;

public:
	FontStyleTable	();
	FontStyleTable	(const FontStyleTable&);

public:
	const CString&	 GetStyleName(FontIdx index) const;
	FontStylesGroup* GetAt		 (FontIdx Index) const { return (*this)[Index]; }

	 // restituiscono il best font
	FontStyle*	GetFontStyle	(FontIdx index, CTBNamespace* pContext) const;
	FontIdx 	GetFontIdx		(const CString& stylename, BOOL bInitDefault = TRUE) const;

	// restituiscono il puntatore a font specificato (non il BestFormatter)
	FontStyle*	GetFontStyle	(FontStyle* FontFrom,			FontStyle::FontStyleSource aFontSource);
	FontStyle*	GetFontStyle	(FontIdx index,					FontStyle::FontStyleSource aFontSource);
	FontIdx		GetFontIdx		(FontStyle* FontFrom,			FontStyle::FontStyleSource aFontSource = FontStyle::FROM_CUSTOM);
	SYSTEMTIME	GetFileDate		(const CTBNamespace& aOwner,	const FontStyle::FontStyleSource& aSource);

	BOOL		IsModified		() const			 { return m_bModified; }
	void		SetModified		(const BOOL& bValue) { m_bModified = bValue; } 

	int			AddFont			(FontStyle* pFont);
	int			AddFileLoaded	(const CTBNamespace& aOwner, const FontStyle::FontStyleSource& aSource, const SYSTEMTIME& aDate);
	void		CopyFileLoaded	(const FontStyleTable& fromFiles);
	void		RemoveFileLoaded(const CTBNamespace& aOwner, const FontStyle::FontStyleSource& aSource);
	BOOL		CheckFontTable	(CTBNamespaceArray& aNsToSave, const CTBNamespace& aNsReport = CTBNamespace());
	void		ClearFontsOf	(const CTBNamespace& aOwner, const FontStyle::FontStyleSource& aSource);

	// le funzioni di seguito lavorano sul nome localizzato
	FontIdx 	GetLocalizedFontIdx	(const CString& sLocalizedName) const;
	CString 	GetStyleTitle		(FontIdx index) const;
	
	// operators
   	const FontStyleTable& operator	= (const FontStyleTable&);

	//for lock tracer
	virtual LPCSTR GetObjectName() const { return "FontStyleTable"; }

	int AddAlias (const CString& sSrcName, const CString& sDstName, int nSrcSize = 0, int nDstSize = 0)
	{
		return m_FontAliasTable.Add(sSrcName, sDstName, nSrcSize, nDstSize);
	}

private:
	FontStylesGroup* operator[]	(FontIdx Index)	const;

public:
	void LoadApplicationCulture(CString sLanguage/* = _T("")*/);
	const FontAliasTable*	GetAliasTable() const ;

	int		GetSizeOfDescriptionFont();
	void	SetSizeOfDescriptionFont(int);

	BOOL	GetUseVCenterBottomAlignInWoormFields() const;
	void	SetUseVCenterBottomAlignInWoormFields(BOOL);
};

//=============================================================================        
DECLARE_SMART_LOCK_PTR(FontStyleTable)
DECLARE_CONST_SMART_LOCK_PTR(FontStyleTable)

//-----------------------------------------------------------------------------
TB_EXPORT const FontStyleTable* AFXAPI AfxGetStandardFontStyleTable();
TB_EXPORT FontStyleTableConstPtr AFXAPI AfxGetFontStyleTable();
TB_EXPORT FontStyleTablePtr AFXAPI AfxGetWritableFontStyleTable();

TB_EXPORT const FontAliasTable* AFXAPI AfxGetFontAliasTable();

//-----------------------------------------------------------------------------
#include "endh.dex"

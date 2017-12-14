#include "stdafx.h"

#include <TbNameSolver\JsonSerializer.h>

#include <TbNameSolver\ApplicationContext.h>
#include <TBNameSolver\LoginContext.h>
#include <TbNameSolver\TBNamespaces.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\Chars.h>
#include <TbNameSolver\PathFinder.h>

#include <TbGeneric\FontsTable.h>
#include <TbGeneric\GeneralFunctions.h>

#include <TbGeneric\DataObj.h>

#include "FormatsTable.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

static const TCHAR szAreaSep[] = _T(",");

//-----------------------------------------------------------------------------
const FormatStyleTable* AFXAPI AfxGetStandardFormatStyleTable()
{
	return AfxGetApplicationContext()->GetObject<const FormatStyleTable>(&CApplicationContext::GetStandardFormatsTable);
}

//-----------------------------------------------------------------------------
FormatStyleTableConstPtr AFXAPI AfxGetFormatStyleTable()
{ 
	CLoginContext* pContext = AfxGetLoginContext();
	FormatStyleTable* pTable = pContext
		? pContext->GetObject<FormatStyleTable>(&CLoginContext::GetFormatsTable)
		: NULL;

	return FormatStyleTableConstPtr(pTable, FALSE);
}    

//-----------------------------------------------------------------------------
FormatStyleTablePtr AFXAPI AfxGetWritableFormatStyleTable()
{ 
	return FormatStyleTablePtr(AfxGetLoginContext()->GetObject<FormatStyleTable>(&CLoginContext::GetFormatsTable), TRUE);
}  

//-----------------------------------------------------------------------------
CString FromDataTypeToFormatName(const DataType& aDataType)
{
	switch (aDataType.m_wType)
	{
		case DATA_STR_TYPE	:	return _NS_FMT("Text");
		case DATA_INT_TYPE	:	return _NS_FMT("Integer");
		case DATA_LNG_TYPE	:	return	aDataType.IsATime() ? 
										_NS_FMT("ElapsedTime") : 
										_NS_FMT("Long");
		case DATA_DBL_TYPE	:	return _NS_FMT("Double");
		case DATA_MON_TYPE	:	return AfxGetMoneyFormatterName();
		case DATA_QTA_TYPE	:	return _NS_FMT("Quantity");
		case DATA_PERC_TYPE	:	return _NS_FMT("Percent");
		case DATA_DATE_TYPE	:	return	!aDataType.IsFullDate() ? 
										_NS_FMT("Date") : 
										aDataType.IsATime() ? _NS_FMT("Time") : _NS_FMT("DateTime");
		case DATA_BOOL_TYPE	:	return _NS_FMT("Bool");
		case DATA_ENUM_TYPE	:	return _NS_FMT("Enum");
		case DATA_GUID_TYPE	:	return _NS_FMT("Uuid");
		case DATA_TXT_TYPE	:	return _NS_FMT("LongText");
		case DATA_BLOB_TYPE	:	return _NS_FMT("Blob");
		default				:	return _NS_FMT("");
	}
}

//-----------------------------------------------------------------------------
TB_EXPORT CString AfxGetMoneyFormatterName	(BOOL bIsAccountable /*TRUE*/)
{
	return bIsAccountable ? _T("Money") : _T("NotAccountableMoney");
}

//-----------------------------------------------------------------------------
Formatter* AfxGetMoneyFormatter (BOOL bIsAccountable /*TRUE*/, CTBNamespace* pContext /*NULL*/)
{
	return AfxGetFormatStyleTable()->GetFormatter(AfxGetMoneyFormatterName(bIsAccountable), pContext);
}

//============================================================================
//		class Formatter  implementation
//============================================================================
//
//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(Formatter, CObject)

//----------------------------------------------------------------------------
Formatter::Formatter(const CString& sName, FormatStyleSource aSource, const CTBNamespace& aOwner)
{
	m_strName			= sName;
	m_bChanged			= FALSE;
	m_bDeleted			= FALSE;	
	m_bEditable			= TRUE;
	m_nPaddedLen		= 0;
	m_nOutputCharLen	= 15;
	m_nInputCharLen		= 15;
	m_Align				= NONE;
	m_FromAndTo			= aSource;
	m_OwnerModule		= aOwner;
	m_pStandardFormatter= NULL; 
	m_bZeroPadded		= FALSE;
}

//----------------------------------------------------------------------------
Formatter::AlignType Formatter::GetDefaultAlign	()	const
{
	return NONE;
}

//------------------------------------------------------------------------------
const CString Formatter::GetLimitedArea () const
{
	CString s;
	for (int i=0; i <= m_LimitedContextArea.GetUpperBound(); i++)
		s = s + m_LimitedContextArea.GetAt(i) + 
		(i == m_LimitedContextArea.GetUpperBound() ? _T("") : szAreaSep);
    
	return s;
}

//------------------------------------------------------------------------------
const CStringArray& Formatter::GetLimitedAreas () const
{
	return m_LimitedContextArea;
}

//------------------------------------------------------------------------------
 void Formatter::SetLimitedArea (const CString& sArea) 
{
	if (sArea.IsEmpty())
		return;
	
	m_LimitedContextArea.RemoveAll();

	int nCurrPos	= -1;
	int nNexSepPos	= 0;
	CString s;

	do
	{
		nNexSepPos	= sArea.Find(szAreaSep, nCurrPos+1);
		s = sArea.Mid(nCurrPos+1, nNexSepPos > 0 ? nNexSepPos - nCurrPos-1 : sArea.GetLength());
		m_LimitedContextArea.Add(s);
		nCurrPos = nNexSepPos;
	}
	while (nCurrPos >= 0 && nCurrPos <= sArea.GetLength());
}

//----------------------------------------------------------------------------
void Formatter::SetLimitedAreas	(const CStringArray& aAreas)
{
	m_LimitedContextArea.RemoveAll();

	for (int i=0; i <= aAreas.GetUpperBound(); i++)
		m_LimitedContextArea.Add(aAreas.GetAt(i));
}

//------------------------------------------------------------------------------
void Formatter::SetStandardFormatter (Formatter* pFormatter)
{
	m_pStandardFormatter = pFormatter;
}

// implementazione di default
//----------------------------------------------------------------------------
void Formatter::RecalcWidths()
{
	if (m_nPaddedLen)
	{			
		CString str = m_strHead + CString(_T('w'), m_nPaddedLen) + m_strTail;
	
		Padder(str, m_Align!=RIGHT);
		m_nOutputCharLen = str.GetLength();
	}
}

//----------------------------------------------------------------------------
const CSize	 Formatter::GetInputWidth (CDC* pDC, int nCols /*-1*/, CFont* /*= NULL*/)
{
	ASSERT (FALSE);
	TRACE ("Formatter::GetDataWidth not implemented");
	
	return CSize(0,0);
}

//----------------------------------------------------------------------------
Formatter* Formatter::Clone() const
{
	Formatter* pF = (Formatter*) GetRuntimeClass()->CreateObject();
	
	pF->Assign(*this);
    return pF;
}

//----------------------------------------------------------------------------
void Formatter::Assign(const Formatter& Fmt)
{
	// non e` il caso di chiamare la RecalcWidths!!
	//
	m_strName				= Fmt.m_strName;
	m_OwnType				= Fmt.m_OwnType;
	m_nOutputCharLen		= Fmt.m_nOutputCharLen;
	m_nInputCharLen			= Fmt.m_nInputCharLen;
							
	m_nPaddedLen			= Fmt.m_nPaddedLen;
	m_Align					= Fmt.m_Align;
	m_strHead				= Fmt.m_strHead;
	m_strTail				= Fmt.m_strTail;
						
	m_bChanged				= Fmt.m_bChanged;
	m_bDeleted				= Fmt.m_bDeleted;
	m_bEditable				= Fmt.m_bEditable;
	m_OwnerModule			= Fmt.m_OwnerModule;
	m_FromAndTo				= Fmt.m_FromAndTo;
	m_pStandardFormatter	= Fmt.m_pStandardFormatter;
	m_bZeroPadded			= Fmt.m_bZeroPadded;

	m_LimitedContextArea.RemoveAll();
	for (int i=0; i <= Fmt.m_LimitedContextArea.GetUpperBound(); i++)
		m_LimitedContextArea.Add(Fmt.m_LimitedContextArea.GetAt(i));

}

//----------------------------------------------------------------------------
int Formatter::Compare(const Formatter& F) const
{
	if (m_nPaddedLen			!= F.m_nPaddedLen			) return 1;
	if (m_Align					!= F.m_Align				) return 1;
	if (m_strHead				!= F.m_strHead				) return 1;
	if (m_strTail				!= F.m_strTail				) return 1;
	if (m_OwnerModule			!= F.m_OwnerModule			) return 1;
	if (m_OwnType				!= F.m_OwnType				) return 1;
	if (m_strName				!= F.m_strName				) return 1;
	if (m_FromAndTo				!= F.m_FromAndTo			) return 1;
	if (m_bZeroPadded			!= F.m_bZeroPadded			) return 1;

	if (m_LimitedContextArea.GetSize() != F.m_LimitedContextArea.GetSize() ) return 1;

	for (int i=0; i < m_LimitedContextArea.GetSize(); i++)
		if (m_LimitedContextArea.GetAt(i) != F.m_LimitedContextArea.GetAt(i))
			return 1;

	return 0;
}
//------------------------------------------------------------------------------
void Formatter::SerializeJson(CJsonSerializer& strJson)  const
{
	//CString				m_strName;
	//FormatStyleSource	m_FromAndTo;

	//BOOL				m_bChanged;
	//BOOL				m_bDeleted;
	//BOOL				m_bEditable;
						
	//int					m_nOutputCharLen;
	//int					m_nInputCharLen;
	//CString				m_strHead;
	//CString				m_strTail;
	//CStringArray		m_LimitedContextArea;
	//BOOL				m_bZeroPadded;
	strJson.WriteString(	szJsonName	, m_strName);
	strJson.WriteInt(		szJsonPaddedLen	, m_nPaddedLen);
	strJson.WriteInt(		szJsonAlignType	, m_Align);


}
//------------------------------------------------------------------------------
void Formatter::Padder(CString &strToPadd, BOOL padRight) const
{
	if(m_nPaddedLen <= 0)
		return;

	//pad Right o Left, se specificata la paddedlen
	int posTo = m_nPaddedLen - strToPadd.GetLength();

	if (posTo < 0)	
	{
		strToPadd = TextOverflow(m_nPaddedLen); //se piu lungo di paddedlen mostro ****
		return;
	}

	if (posTo == 0) return;

	CString strPad(BLANK_CHAR, posTo);
	if(padRight)
		strToPadd.Append (strPad);
	else
		strToPadd.Insert (0,strPad);
}

//scrive tanti * quanto e' textLen
//------------------------------------------------------------------------------
CString Formatter::TextOverflow (int textLen)
{
	return CString(ASTERISK_CHAR, textLen);
}
		
//------------------------------------------------------------------------------
void Formatter::FormatDataObj(const DataObj& aDataObj, CString& Str, BOOL bPaddingEnabled) const
{ 
	if (aDataObj.GetDataType() == DataType::Array)
	{
		DataArray* pdar = (DataArray*)&aDataObj;
		ASSERT(pdar->IsKindOf(RUNTIME_CLASS(DataArray)));
		Str.Empty();
		for (int i = 0; i < pdar->GetSize(); i++)
		{
			CString sTmpOne;
			DataObj* pObj = pdar->GetAt(i);
			if (pObj)
				Format(pObj->GetRawData(), sTmpOne, bPaddingEnabled, aDataObj.IsCollateCultureSensitive());

			Str += sTmpOne + '\n';
		}
	}
	else
		Format(aDataObj.GetRawData(), Str, bPaddingEnabled, aDataObj.IsCollateCultureSensitive());
}

//------------------------------------------------------------------------------
const CString Formatter::GetTitle () const 
{ 
	CString sTitle = AfxLoadFormatterString(m_strName, m_OwnerModule);
	
	// se il formattatore è di report, la traduzione potrebbe non essere disponibile
	// quindi provo a cercare nel suo standard per vedere se fossero disponibili
	if (
			m_OwnerModule.GetType() != CTBNamespace::REPORT || 
			!m_pStandardFormatter ||
			m_pStandardFormatter == this ||
			_tcsicmp(sTitle, m_strName)
		)
		return sTitle;

	return m_pStandardFormatter->GetTitle(); 
}

//============================================================================
//		class FormatterGroup implementation
//============================================================================
//
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(FormatterGroup, CObject)

//----------------------------------------------------------------------------
FormatterGroup::FormatterGroup()
{
}

//----------------------------------------------------------------------------
FormatterGroup::FormatterGroup(const CString& aName)
{
	m_OwnType	= DATA_NULL_TYPE;
	m_strName	= aName;
}

//----------------------------------------------------------------------------
FormatterGroup::~FormatterGroup()
{
	m_Formatters.RemoveAll();
}

//----------------------------------------------------------------------------
FormatterGroup* FormatterGroup::Clone() const
{
	FormatterGroup* pF = (FormatterGroup*) GetRuntimeClass()->CreateObject();
	
	pF->Assign(*this);
    return pF;
}

//----------------------------------------------------------------------------
void FormatterGroup::Assign(const FormatterGroup& Fmt)
{
	// non e` il caso di chiamare la RecalcWidths!!
	//
	m_strName		= Fmt.m_strName;
	m_OwnType		= Fmt.m_OwnType;
	
	m_Formatters.RemoveAll();

	for (int i=0; i <= Fmt.m_Formatters.GetUpperBound(); i++)
		AddFormatter(((Formatter*)Fmt.m_Formatters.GetAt(i))->Clone());
}

//----------------------------------------------------------------------------
const CString FormatterGroup::GetTitle ()
{
	Formatter *pFormatter = GetFormatter(0);
	return pFormatter ? pFormatter->GetTitle() : _T("");
}

//----------------------------------------------------------------------------
Formatter* FormatterGroup::GetFormatter(const CTBNamespace* pContext) const
{
	if (!m_Formatters.GetSize ())
		return NULL;

	return BestFormatterForContext(pContext);
}

//----------------------------------------------------------------------------
Formatter* FormatterGroup::GetFormatter(const FormatIdx idxFormatter) 
{
	if (idxFormatter >= m_Formatters.GetSize())
		return NULL;

	return (Formatter*) m_Formatters.GetAt(idxFormatter);	
}

//------------------------------------------------------------------------------
Formatter* FormatterGroup::GetFormatter (const Formatter::FormatStyleSource aSource) 
{
	Formatter* pFmt;
	for (int i=0; i <= m_Formatters.GetUpperBound(); i++)
	{
		pFmt = (Formatter*) m_Formatters.GetAt(i);
		if (pFmt && pFmt->GetSource() == aSource)
			return pFmt;
	}

	return NULL;
}


// restituisce idx del array del gruppo del formatter
//----------------------------------------------------------------------------
FormatIdx FormatterGroup::GetFormatIdx(Formatter* FormatterFrom, const Formatter::FormatStyleSource aFormatSource)
{
	if (!m_Formatters.GetSize ())
		return -1;
	
	for (int i = 0; i <= m_Formatters.GetUpperBound(); i++)
	{
		Formatter* FormatterSource = (Formatter*) m_Formatters.GetAt(i);
		CTBNamespace NsFrom		= FormatterFrom->GetOwner();
		CTBNamespace NsSource	= FormatterSource->GetOwner();

		if (
				FormatterSource->GetSource()== aFormatSource && 
				FormatterSource->GetOwner() == FormatterFrom->GetOwner() 
		)	
			{
				return i;
			}
	}
	
	return -1;
}

//----------------------------------------------------------------------------
const Array& FormatterGroup::GetFormatters () const
{
	return m_Formatters;
}

// Si occupa di scegliere il formattatore migliore da applicare secondo contesto.
// La scaletta delle priorità è la seguente:
//	1) il corrispondente ad uno specifico namespace 
//	2) il corrispondente alla stessa applicazione e modulo
//	3) il corrispondente alla stessa applicazione	(il primo trovato)
//	4) il corrispondente di altre applicazioni		(il primo trovato)
//	5) l'ultimo caricato
//	- a parità di formattatore, il custom è più forte di quello standard
//------------------------------------------------------------------------------
Formatter* FormatterGroup::BestFormatterForContext (const CTBNamespace* pContext) const
{
	// l'unico possibile
	if (!pContext)
		return (Formatter*) m_Formatters.GetAt(m_Formatters.GetUpperBound());

	CTBNamespace nsModule(
							CTBNamespace::MODULE, 
							pContext->GetApplicationName()
							+ CTBNamespace::GetSeparator() + 
							pContext->GetObjectName(CTBNamespace::MODULE)
						);

	// Cerco il mio corrispondente preciso, e mi predispongo già quello 
	// con lo stesso nome di applicazione e/o con lo stesso nome di modulo
	Formatter* pFrmt		= NULL;
	Formatter* pExactFrmt	= NULL;
	Formatter* pAppFrmt		= NULL;
	Formatter* pModFrmt		= NULL;
	Formatter* pOtherAppFrmt= NULL;

	for (int i=0; i <= m_Formatters.GetUpperBound(); i++)
	{
		pFrmt = (Formatter*) m_Formatters.GetAt(i);

		if (!pFrmt || pFrmt->IsDeleted())
			continue;

		// ho il corrispondente identico
		if (pFrmt->GetOwner() == *pContext && HasPriority(pExactFrmt, pFrmt, pContext))
			pExactFrmt = pFrmt;

		// il primo trovato con la stessa applicazione
		if (
				pFrmt->GetOwner().GetApplicationName() == pContext->GetApplicationName() && 
				HasPriority(pAppFrmt, pFrmt, pContext)
			)
			pAppFrmt = pFrmt;

		// alcuni owner potrebbero essere Library, quindi
		// devo essere sicura di comparare bene i moduli
		CTBNamespace nsOwnerModule
			(
				CTBNamespace::MODULE,
				pFrmt->GetOwner().GetApplicationName()
				+ CTBNamespace::GetSeparator() + 
				pFrmt->GetOwner().GetObjectName(CTBNamespace::MODULE)
			);

		// il primo trovato con lo stesso modulo
		if (nsOwnerModule == nsModule && HasPriority(pModFrmt, pFrmt, pContext))
			pModFrmt = pFrmt;

		// il primo trovato di altre applicazioni
		if (
				pFrmt->GetOwner().GetApplicationName()!= pContext->GetApplicationName() && 
				HasPriority(pOtherAppFrmt, pFrmt, pContext)
			)
			pOtherAppFrmt = pFrmt;	
	}

	if (pExactFrmt)		return pExactFrmt;		// di stesso namespace (report di woorm)
	if (pModFrmt)		return pModFrmt;		// di modulo
	if (pAppFrmt)		return pAppFrmt;		// di applicazione
	if (pOtherAppFrmt)	return pOtherAppFrmt;	// di altre applicazioni

	// l'ultimo caricato
	return NULL;
}

// devo stare all'okkio a usare il custom dello stesso namespace preso prima
//----------------------------------------------------------------------------
BOOL FormatterGroup::HasPriority (const Formatter* pOld, const Formatter* pNew, const CTBNamespace* pContext) const
{
	if (!pNew)
		return FALSE;

	BOOL bOkForArea = !pNew->GetLimitedAreas().GetSize();

	// area di applicazione del formattatore
    if (!bOkForArea)
	{
		CString sArea;
		CTBNamespace nsModule(
								CTBNamespace::MODULE, 
								pContext->GetApplicationName()
								+ CTBNamespace::GetSeparator() + 
								pContext->GetObjectName(CTBNamespace::MODULE)
							);
		for (int i=0; i <= pNew->GetLimitedAreas().GetUpperBound(); i++)
		{
			sArea = pNew->GetLimitedAreas().GetAt(i);
			
			if	(
					*pContext == CTBNamespace(sArea) || 
					nsModule == sArea ||
					_tcsicmp(pContext->GetApplicationName(), sArea) == 0
				)
			{
				bOkForArea = TRUE;
				break;
			}
		}
	}
	if (!pOld)
		return bOkForArea;

	return	bOkForArea &&
			pOld->GetOwner () == pNew->GetOwner() &&
			pOld->GetSource() == FontStyle::FROM_STANDARD && 
			pNew->GetSource() == FontStyle::FROM_CUSTOM;
}

//----------------------------------------------------------------------------
int FormatterGroup::AddFormatter (Formatter* pFormatter)
{
	if (!pFormatter)
		ASSERT_TRACE(pFormatter,"Parameter pFormatter cannot be null");
	
	Formatter* pFmt;
	int nPos = -1;

	for (int i=0; i <= m_Formatters.GetUpperBound(); i++)
	{
		pFmt = (Formatter*) m_Formatters.GetAt(i);
		// se esiste già con le stesse caratteristiche lo sovrascrivo
		if	(
				pFmt->GetDataType() == pFormatter->GetDataType() &&
				pFmt->GetOwner() == pFormatter->GetOwner() && 
				pFmt->GetSource() == pFormatter->GetSource()
			)
		{
			nPos = i;
			break;
		}
	}

	if (nPos >= 0)
		m_Formatters.RemoveAt(nPos);

	Formatter* pStandardFormatter = GetFormatter(FontStyle::FROM_STANDARD);
	nPos = m_Formatters.Add(pFormatter);

	// devo aggiornare il puntatore allo standard se esiste
	for (int i=0; i <= m_Formatters.GetUpperBound(); i++)
	{
		pFmt = (Formatter*) m_Formatters.GetAt(i);
		if (pFmt)
			pFmt->SetStandardFormatter(pStandardFormatter);
	}

	return nPos;
}

//----------------------------------------------------------------------------
void FormatterGroup::DeleteFormatter(Formatter* FormatterToDel)
{
	if (!FormatterToDel)
	{
		ASSERT_TRACE(FormatterToDel,"Parameter FormatterToDel cannot be null");
		return;
	}

	FormatIdx idxGroup = GetFormatIdx(FormatterToDel, FormatterToDel->GetSource());
	if (idxGroup < 0)
		return;
	m_Formatters.RemoveAt(idxGroup);
}


//==============================================================================
//			Class FormatterFile
//==============================================================================
//------------------------------------------------------------------------------
FormatterFile::FormatterFile (
								const CTBNamespace& aOwner,
								const Formatter::FormatStyleSource& aSource,
								const SYSTEMTIME& aFileDate
							  )
{
	m_Owner		= aOwner;
	m_Source	= aSource;
	m_dLastWrite= aFileDate;
}

//------------------------------------------------------------------------------
FormatterFile::FormatterFile (const FormatterFile& aSource)
{
	*this = aSource;
}

//------------------------------------------------------------------------------
void FormatterFile::SetFileDate (const SYSTEMTIME& aDate)
{
	m_dLastWrite = aDate;
}

//------------------------------------------------------------------------------
const FormatterFile& FormatterFile::operator= (const FormatterFile& source)
{
	m_Owner		= source.m_Owner;
	m_Source	= source.m_Source;
	m_dLastWrite= source.m_dLastWrite;

	return *this;
}

//============================================================================
//		class FormatStyleTable implementation
//============================================================================

//----------------------------------------------------------------------------
FormatStyleTable::FormatStyleTable()
	:
	m_bModified	(FALSE) 
{
	m_arLoadedFiles.RemoveAll();
}

//------------------------------------------------------------------------------
FormatStyleTable::FormatStyleTable (const FormatStyleTable& source)
{
	*this = source;
}

//------------------------------------------------------------------------------
const FormatStyleTable& FormatStyleTable::operator= (const FormatStyleTable& source)
{
	TB_LOCK_FOR_WRITE ()
	RemoveAll();

	m_bModified	= source.m_bModified;
	
	for (int i = 0; i <= source.GetUpperBound(); i++)
		Add(source[i]->Clone());     
	
	CopyFileLoaded(source);
	return *this;
}

//----------------------------------------------------------------------------
void FormatStyleTable::CopyFileLoaded (const FormatStyleTable& fromFiles)
{
	// i files
	m_arLoadedFiles.RemoveAll();

	FormatterFile* pFile;
	for (int i=0; i <= fromFiles.m_arLoadedFiles.GetUpperBound(); i++)
	{
		pFile = (FormatterFile*) fromFiles.m_arLoadedFiles.GetAt(i);
		m_arLoadedFiles.Add(new FormatterFile(*pFile));
	}
}

//----------------------------------------------------------------------------
Formatter* FormatStyleTable::GetFormatter(const CString& stylename, const CTBNamespace* pContext) const
{
	int nIdx = GetFormatIdx(stylename);
	
	if (nIdx < 0) return NULL;

	return GetAt(nIdx)->GetFormatter(pContext);
}

//----------------------------------------------------------------------------
Formatter* FormatStyleTable::GetFormatter (const int& nIdx, const CTBNamespace* pContext) const
{
	if (nIdx < 0 || nIdx > GetUpperBound())
		return NULL;

	return GetAt(nIdx)->GetFormatter(pContext);
}

//----------------------------------------------------------------------------
Formatter* FormatStyleTable::GetFormatter(const DataType& type, const CTBNamespace* pContext) const
{
	int nIdx = GetFormatIdx(type);
	
	if (nIdx < 0) return NULL;

	return GetAt(nIdx)->GetFormatter(pContext);
}

//----------------------------------------------------------------------------
FormatIdx FormatStyleTable::GetFormatIdx(const CString& stylename) const
{
	for (int i = GetUpperBound(); i >= 0; i--)
		if (_tcsicmp(GetAt(i)->GetName(), stylename) == 0)
			return i;

    return -1;
}

//----------------------------------------------------------------------------
FormatIdx FormatStyleTable::GetFormatIdx(const DataType& dataType)	const
{
	return GetFormatIdx(FromDataTypeToFormatName(dataType));
}

//----------------------------------------------------------------------------
FormatIdx FormatStyleTable::GetFormatIdx(Formatter* FormatterFrom, const Formatter::FormatStyleSource aFormatSource)
{
	FormatIdx nIdxGroup = GetFormatIdx(FormatterFrom->GetName());
	if (nIdxGroup < 0)
		return nIdxGroup;

	int nStyle = GetAt(nIdxGroup)->GetFormatIdx(FormatterFrom, aFormatSource);
	
	if (nStyle >= 0)
		return nStyle;

	return -1;
}

//----------------------------------------------------------------------------
Formatter* FormatStyleTable::GetFormatter(Formatter* FormatterFrom, const Formatter::FormatStyleSource aFormatSource)
{
	Formatter* pFormatter = NULL;
	FormatIdx nIdxForm = GetFormatIdx(FormatterFrom, aFormatSource);
	if (nIdxForm < 0)
		return pFormatter;

	FormatIdx nIdxGroup = GetFormatIdx(FormatterFrom->GetName());
	return GetAt(nIdxGroup)->GetFormatter(nIdxForm);
}

//----------------------------------------------------------------------------
DataType FormatStyleTable::GetDataType(FormatIdx Index) const
{
	return Index >= 0 && Index <= GetUpperBound()
				? GetAt(Index)->GetDataType()
				: DataType::Null;
}

//----------------------------------------------------------------------------
CString FormatStyleTable::GetStyleName(FormatIdx Index) const
{
	return Index >= 0 && Index <= GetUpperBound()
			? GetAt(Index)->GetName()
			: _T("");
}

//------------------------------------------------------------------------------
CString FormatStyleTable::GetStyleTitle (FormatIdx nIndex) const
{
	if ((nIndex < 0) || (nIndex > GetUpperBound()))
		return _T("");

	return  GetAt(nIndex)->GetTitle();
}

//----------------------------------------------------------------------------
FormatterGroup* FormatStyleTable::GetAt (FormatIdx Index)	const
{
	if (Index < 0 || Index > GetUpperBound())
	{
		ASSERT_TRACE2(Index >= 0 && Index <= GetUpperBound(),"Wrong position: Index = %d, GetUpperBound() = %d", Index, GetUpperBound());
		return NULL;
	}
	return (FormatterGroup*) Array::GetAt(Index);
}

//----------------------------------------------------------------------------
FormatterGroup* FormatStyleTable::operator[](FormatIdx Index) const
{
	return GetAt(Index);
}

//----------------------------------------------------------------------------
int FormatStyleTable::GetPaddedLen (const FormatIdx Index, const CTBNamespace* pContext) const
{
	return Index >= 0 && Index <= GetUpperBound()
				? GetAt(Index)->GetFormatter(pContext)->GetPaddedLen()
				: -1;
}

//----------------------------------------------------------------------------
int FormatStyleTable::GetOutputCharLen(const DataType& type, const CTBNamespace* pContext) const
{
	int nIndex = GetFormatIdx(type);
	if (nIndex < 0)
		return -1;
	
	Formatter* pFormatter = GetAt(nIndex)->GetFormatter(pContext);

	return pFormatter ? pFormatter->GetOutputCharLen() : -1;
}

//----------------------------------------------------------------------------
int FormatStyleTable::GetInputCharLen(const DataType& type, const CTBNamespace* pContext) const
{
	int nIndex = GetFormatIdx(type);
	if (nIndex < 0)
		return -1;
	
	Formatter* pFormatter = GetAt(nIndex)->GetFormatter(pContext);

	return pFormatter ? pFormatter->GetInputCharLen() : -1;
}

//----------------------------------------------------------------------------
int FormatStyleTable::GetOutputCharLen(const FormatIdx nIndex, const CTBNamespace* pContext) const
{
	if (nIndex < 0 || nIndex > GetUpperBound())
		return -1;
	
	Formatter* pFormatter = GetAt(nIndex)->GetFormatter(pContext);

	return pFormatter ? pFormatter->GetOutputCharLen() : -1;
}

//----------------------------------------------------------------------------
int FormatStyleTable::GetInputCharLen(const FormatIdx nIndex, const CTBNamespace* pContext) const
{
	if (nIndex < 0 || nIndex > GetUpperBound())
		return -1;
	
	Formatter* pFormatter = GetAt(nIndex)->GetFormatter(pContext);

	return pFormatter ? pFormatter->GetInputCharLen() : -1;
}

//----------------------------------------------------------------------------
int	FormatStyleTable::AddFormatter (Formatter* pFormatter)
{
	if (!pFormatter)
		return -1;

	TB_LOCK_FOR_WRITE ();

	// cerca se esiste già un formattatore con lo stesso nome rappresentato dal suo gruppo
	FormatterGroup* pFormatterGroup = NULL;
	CString s;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		s = GetAt(i)->GetName();
		if (_tcsicmp(pFormatter->GetName(), s) == 0)
		{
			pFormatterGroup = GetAt(i);
			break;
		}
	}
	
	// se non c'è ancora il gruppo lo creo
	if (!pFormatterGroup)
	{
		pFormatterGroup = new FormatterGroup(pFormatter->GetName());
		pFormatterGroup->SetOwnType(pFormatter->GetDataType());
		Add(pFormatterGroup);
	}

	// e poi aggiungo il formattatore
	return pFormatterGroup->AddFormatter(pFormatter);
}

// sistema la tabella dei formattatori modificata eliminando i deleted e
// ritorna l'elenco dei namespace di moduli/report che hanno dei formattatori
// variati, di cui andrebbe eseguito il salvataggio
//----------------------------------------------------------------------------
BOOL FormatStyleTable::CheckFormatTable	(CTBNamespaceArray& aNsToSave, CTBNamespace aNsReport)
{
	if (!IsModified())
		return FALSE;

	// in caso di report, ritorna se bisogna fare unparse
	BOOL bToSave = FALSE;  

	// gruppi di formattatori
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		FormatterGroup* pFormatGrp = GetAt(i);

		if (!pFormatGrp)
			continue;

		// elenco dei formattatori
		for (int n = pFormatGrp->GetFormatters().GetUpperBound() ; n >= 0 ; n--)
		{
			Formatter* pFormat = (Formatter*) pFormatGrp->GetFormatters().GetAt(n);

			// sistemazione del Ns in caso di rinominazione nome report in save
			if (pFormat->GetOwner().GetType() == CTBNamespace::REPORT)
				pFormat->SetOwner(aNsReport);

			if (!pFormat->IsChanged())
				continue;

			BOOL bInToSaveList = FALSE;
			for (int s = 0; s <= aNsToSave.GetUpperBound(); s++)
				if (*aNsToSave.GetAt(s) == pFormat->GetOwner())
				{
					bInToSaveList = TRUE;
					break;
				}
			
			if (!bInToSaveList)
			{
				CTBNamespace* pNewNs = new CTBNamespace(pFormat->GetOwner());
				
				// se è di libreria lo trasformo di modulo
				if (pNewNs->GetType() == CTBNamespace::LIBRARY)
				{
					pNewNs->SetObjectName(CTBNamespace::LIBRARY, _T(""));
					pNewNs->SetType(CTBNamespace::MODULE);
				}
				aNsToSave.Add(pNewNs);
			}

			// cancellazione degli eliminati dalla tabella
			if (pFormat->IsDeleted())
				DeleteFormatter(pFormat);
			
			// woorm va sempre scritto
			else if (!bToSave && pFormat->GetSource() == Formatter::FROM_WOORM) 
				bToSave = TRUE;
		}
	}

	return bToSave;
}
//----------------------------------------------------------------------------
void FormatStyleTable::DeleteFormatter(Formatter* FormatterToDel)
{
	if (!FormatterToDel)
	{
		ASSERT_TRACE(FormatterToDel,"Parameter FormatterToDel cannot be null");
		return;
	}

	FormatIdx IdxGroup = GetFormatIdx(FormatterToDel->GetName());
	GetAt(IdxGroup)->DeleteFormatter(FormatterToDel);
}

// Si occupa di eliminare i formattatori relativi allo specifico Owner e sorgente
//----------------------------------------------------------------------------
void FormatStyleTable::ClearFormatsOf (const CTBNamespace& aOwner, const Formatter::FormatStyleSource& aSource)
{
	// gruppi di formattatori
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		FormatterGroup* pGroup = GetAt(i);
		
		if (!pGroup)
			continue;
		
		// elenco dei fonts
		for (int n = pGroup->GetFormatters().GetUpperBound(); n >= 0 ; n--)
		{
			Formatter* pStyle = (Formatter*) pGroup->GetFormatters().GetAt(n);
			
			if	(pStyle && pStyle->GetOwner() == aOwner && pStyle->GetSource() == aSource)
				pGroup->DeleteFormatter(pStyle);
		}
	}
}

//----------------------------------------------------------------------------
int FormatStyleTable::AddFileLoaded (const CTBNamespace& aOwner, const Formatter::FormatStyleSource& aSource, const SYSTEMTIME& aDate)
{
	FormatterFile* pFile;
	for (int i=0; i <= m_arLoadedFiles.GetUpperBound(); i++)
	{
		pFile = (FormatterFile*) m_arLoadedFiles.GetAt(i);
		if (pFile && pFile->GetOwner() == aOwner && pFile->GetSource() == aSource)
		{
			pFile->SetFileDate(aDate);
			return i;
		}
	}

	return m_arLoadedFiles.Add(new FormatterFile(aOwner, aSource, aDate));
}

//----------------------------------------------------------------------------
void FormatStyleTable::RemoveFileLoaded (const CTBNamespace& aOwner, const Formatter::FormatStyleSource& aSource)
{
	FormatterFile* pFile;
	for (int i=m_arLoadedFiles.GetUpperBound(); i >= 0 ; i--)
	{
		pFile = (FormatterFile*) m_arLoadedFiles.GetAt(i);
		if (pFile && pFile->GetOwner() == aOwner && pFile->GetSource() == aSource)
		{
			m_arLoadedFiles.RemoveAt(i);
			return;
		}
	}
}

//----------------------------------------------------------------------------
SYSTEMTIME FormatStyleTable::GetFileDate (const CTBNamespace& aOwner, const Formatter::FormatStyleSource& aSource)
{
	FormatterFile* pFile;
	for (int i=0; i <= m_arLoadedFiles.GetUpperBound(); i++)
	{
		pFile = (FormatterFile*) m_arLoadedFiles.GetAt(i);
		if (pFile && pFile->GetOwner() == aOwner && pFile->GetSource() == aSource)
			return pFile->GetFileDate();
	}

	// null date
	SYSTEMTIME aTime;
	aTime.wDay		= MIN_DAY;
	aTime.wMonth	= MIN_MONTH;
	aTime.wYear		= MIN_YEAR;
	aTime.wHour		= MIN_HOUR;
	aTime.wMinute	= MIN_MINUTE;
	aTime.wSecond	= MIN_SECOND;

	return aTime;
}

//----------------------------------------------------------------------------
void FormatStyleTable::GetCompatibleFormatterNames (const DataType& aDataType, CStringArray& arNames) const
{
	// gruppi di formattatori
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		FormatterGroup* pGroup = GetAt(i);
		
		if (pGroup && pGroup->GetDataType() == aDataType)
			arNames.Add (pGroup->GetName());
	}
}

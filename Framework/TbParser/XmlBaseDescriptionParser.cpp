#include "stdafx.h"

#include <io.h>

#include <TbXmlCore\XMLTags.h>
#include <TbXmlCore\XMLDocObj.h>
#include <TbXmlCore\XMLParser.h>

#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\TBNamespaces.h>
#include <TbNameSolver\Chars.h>
#include <TbNameSolver\Diagnostic.h>

#include <TbGeneric\DataObj.h>
#include <TBGeneric\EnumsTable.h>
#include <TBGeneric\GeneralFunctions.h>
#include <TBGeneric\TBStrings.h>

#include <TBGeneric\FunctionCall.h>
#include <TBClientCore\ClientObjects.h>

#include "XmlBaseDescriptionParser.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//=============================================================================
//							XmlAttributeValidator
//=============================================================================

// syntax
//-----------------------------------------------------------------------------
static const TCHAR szXmlISOSeparator[]			= _T(",");
static const TCHAR szOperatorAnd				= _T('&');
static const TCHAR szOperatorAND[]				= _T("and");
static const TCHAR szOperatorOr					= _T('|');
static const TCHAR szOperatorOR[]				= _T("or");
static const TCHAR szOperatorNot				= _T('!');
static const TCHAR szOperatorNOT[]				= _T("not");
static const TCHAR szOpenBrackets				= _T('(');
static const TCHAR szCloseBrackets				= _T(')');
static const TCHAR szNullOperand				= _T('\0');

static const TCHAR szActivationOperators[]		= { szOperatorAnd, szOperatorOr };
static const TCHAR szActivationKeywords[]		= { szOperatorAnd, szOperatorOr, szOperatorNot, szOpenBrackets, szCloseBrackets };

static const TCHAR szNumbers[]					= _T("numbers");
static const TCHAR szLetters[]					= _T("letters");
static const TCHAR szOtherChars[]				= _T("otherchars");
static const TCHAR szBlanks[]					= _T("blanks");
static const TCHAR szAll[]						= _T("all");
//-----------------------------------------------------------------------------
/*static*/ BOOL CXmlAttributeValidator::IsValidCountry (const CString& sCondition, const BOOL& bIsAllow)
{
	if (sCondition.IsEmpty())
		return TRUE;

	CString sCurrentISO = AfxGetLoginManager()->GetProductLanguage();
	
	if (sCurrentISO.IsEmpty())
		return TRUE;

	int nCurrPos = 0;
	CString sSingleState;

	while (nCurrPos < sCondition.GetLength())
	{
		sSingleState = sCondition.Tokenize(szXmlISOSeparator, nCurrPos);
		if (sSingleState.IsEmpty())
			break;

		sSingleState = sSingleState.Trim ();

		if (bIsAllow && sSingleState.Find (sCurrentISO) >= 0)
			return TRUE;

		if (!bIsAllow && sSingleState.Find (sCurrentISO) >= 0)
			return FALSE;
	}

	return !bIsAllow;
}

//-----------------------------------------------------------------------------
/*static*/ BOOL CXmlAttributeValidator::IsValidActivation (const CString& sActivationExpression)
{
	CString sExpression = sActivationExpression;

	sExpression.Trim ();

	if (sExpression.IsEmpty())
		return TRUE;

	// initializing variable
	TCHAR	szCurrentOperator = szNullOperand;
	BOOL	bExpressionValue = TRUE;

	// sActivatedExpression is tokenized into single expressions divided by operands
	while (!sExpression.IsEmpty())
	{		
		BOOL bTokenValue = TRUE;
		int nFirstKeyIndex = sExpression.FindOneOf(szActivationKeywords);

		// complex expression is evaluated with a recursive action
		if (nFirstKeyIndex >= 0)
		{
			if (nFirstKeyIndex == 0 && sExpression.GetAt(0) != szOperatorNot && sExpression.GetAt(0) != szOpenBrackets)
			{
				AfxGetDiagnostic()->Add(_TB("Error encountered into 'activation' attribute evaluation. Expression cannot start with a &|) character as they are syntax operands."), CDiagnostic::Warning);
				AfxGetDiagnostic()->Add(_TB("The 'activation' attribute will be ignored."),CDiagnostic::Warning);
				return TRUE;
			}

			bTokenValue = EvalIsValidActivationExpr (nFirstKeyIndex, sExpression, szCurrentOperator);
		}
		// while single token is evaluated with AfxIsActivated
		else
		{
			CTBNamespace aModuleNs (CTBNamespace::MODULE, sExpression);
			bTokenValue = AfxIsActivated(aModuleNs.GetApplicationName(), aModuleNs.GetModuleName());
			sExpression.Empty ();
		}

		// the evaluation of the single operation is in and, or or not with the previous one
		if (szCurrentOperator == szOperatorAnd)
			bExpressionValue = bExpressionValue && bTokenValue;
		else if (szCurrentOperator == szOperatorOr)
			bExpressionValue = bExpressionValue || bTokenValue;
		else if (szCurrentOperator == szNullOperand) // first operand !
			bExpressionValue = bTokenValue;

		// if there is no other expression to evaluated, it exits from loop
		if (sExpression.IsEmpty())
			break;

		// sets the current evaluated operand 
		szCurrentOperator = sExpression.GetAt(0);

		if (!bExpressionValue && szCurrentOperator == szOperatorAnd)
			return FALSE;

		if (bExpressionValue && szCurrentOperator == szOperatorOr)
			return TRUE;

		// sets the new token to analyze into variable
		sExpression = sExpression.Mid(1).TrimLeft();

		if (szCurrentOperator != szNullOperand && sExpression.IsEmpty())
		{
			AfxGetDiagnostic()->Add(_TB("Error encountered into 'activation' attribute evaluation. Expression cannot terminate with a &!|() character as they are syntax operands."), CDiagnostic::Warning);
			AfxGetDiagnostic()->Add(_TB("The 'activation' attribute will be ignored."), CDiagnostic::Warning);
			return FALSE;
		}
	}

	return bExpressionValue;
}

//-----------------------------------------------------------------------------
/*static*/ DWORD CXmlAttributeValidator::EvalInputAllowed (CString sExpr)
{
	sExpr = sExpr.MakeLower();

	// see parsobj.h
	int nPos = sExpr.Find(szAll);
	if (nPos >= 0)
		return 0x00000010;		// STR_STYLE_ALL

	DWORD wValue = 0;
	nPos = sExpr.Find(szNumbers);
	if (nPos >= 0)
		wValue |= 0x00000001;		// STR_STYLE_NUMBERS

	nPos = sExpr.Find(szLetters);
	if (nPos >= 0)
		wValue |= 0x00000002;		// STR_STYLE_LETTERS

	nPos = sExpr.Find(szLetters);
	if (nPos >= 0)
		wValue |= 0x00000008;		// STR_STYLE_OTHERCHARS

	nPos = sExpr.Find(szBlanks);
	if (nPos >= 0)
		wValue |= 0x00000040;		// STR_STYLE_BLANKS
	
	return wValue;
}

//-----------------------------------------------------------------------------
/*static*/ BOOL CXmlAttributeValidator::EvalIsValidActivationExpr 
		(
			const int& nFirstKeyIndex, 
			CString& sExpression, 
			const TCHAR szCurrentOperator
		)
{
	// if there is NOT operand, it is removed from the expression
	BOOL bNotToken = (sExpression.GetAt(0) == szOperatorNot);
	if (bNotToken)
		sExpression = sExpression.Mid (1).TrimLeft();

	//------------------------------------------------------------------
	// manage brackets into the expression
	//------------------------------------------------------------------
	int nOpenedBrackets = 0;
	int nCharIndex = 0;

	// while end when all opened brackets are closed
	do
	{
		if	(sExpression.GetAt(nCharIndex) == szOpenBrackets)		
			nOpenedBrackets++;
		else if	(sExpression.GetAt(nCharIndex) == szCloseBrackets)	
			nOpenedBrackets--;

		if	(nOpenedBrackets == 0)
			break;

		nCharIndex++;

	} while (nCharIndex < sExpression.GetLength()); 


	if (nOpenedBrackets != 0)
	{
		AfxGetDiagnostic()->Add (_TB("Error encountered into 'activation' attribute evaluation. Number of closed round brackets don't match opened round brackets."), CDiagnostic::Warning);
		AfxGetDiagnostic()->Add (_TB("The 'activation' attribute will be ignored."),CDiagnostic::Warning);
		return TRUE;
	}
	//------------------------------------------------------------------

	//------------------------------------------------------------------
	// token to evaluate calling a recursive IsValidActivation 
	//------------------------------------------------------------------
	CString sToken;

	if (nCharIndex <= 0)
	{
		// NOT operand
		if (bNotToken)
		{
			int nTokenLength = sExpression.FindOneOf(szActivationOperators);
			if (nTokenLength == -1)
			{
				sToken = sExpression;
				sExpression.Empty ();
			}
			else
			{
				sToken = sExpression.Mid(0, nTokenLength).Trim();
				sExpression = sExpression.Mid(nTokenLength).Trim();
			}
		}
		else
		{
			sToken = sExpression.Mid(0, nFirstKeyIndex).Trim();
			sExpression = sExpression.Mid(nFirstKeyIndex).Trim();
		}
	}
	else
	{
		// token starts with a bracket and nCharIndex refers to the last closed bracket
		sToken = sExpression.Mid(1, nCharIndex - 1).Trim();
		sExpression = sExpression.Mid(nCharIndex + 1).Trim();
	}

	// token is empty and the token is not the first evaluated
	if (szCurrentOperator != szNullOperand && sToken.IsEmpty()) 
	{
		AfxGetDiagnostic()->Add (_TB("Error encountered into 'activation' attribute evaluation. Expression cannot terminate with a &!|() character as they are syntax operands."),CDiagnostic::Warning);
		AfxGetDiagnostic()->Add (_TB("The 'activation' attribute will be ignored."),CDiagnostic::Warning);
		return TRUE;
	}

	// token evaluation with application of the NOT operand
	return bNotToken ? !IsValidActivation(sToken) : IsValidActivation(sToken);
}


//-----------------------------------------------------------------------------
/*static*/ BOOL	 CXmlAttributeValidator::IsValidGroup(int nGroupId, const Array* groupArray)
{
	if (nGroupId < 0)
		return FALSE;

	if (nGroupId == 0 && !groupArray)
		return TRUE;

	if (nGroupId > 0)
	{
		if (groupArray)
		{
			int nGroupsNo = groupArray->GetUpperBound();

			if (nGroupId == 0 && nGroupsNo == 0) // si tratta del gruppo di default
				return TRUE;


			if (nGroupId == 0 && nGroupsNo > 0) // è stato specificato in un elemento Report il gruppo 0 in presenza di altri gruppi.
				return FALSE;

			DataInt* pCurrGroupId(NULL);
			for (int i = 0; i <= nGroupsNo; i++)
			{
				pCurrGroupId = dynamic_cast<DataInt*>(groupArray->GetAt(i));
				if (!pCurrGroupId)
				{
					ASSERT(FALSE);
					return FALSE;
				}

				if (_wtoi(pCurrGroupId->Str()) == nGroupId)
					return TRUE;
			}

			return FALSE;
		}
		else
			return FALSE;
	}

	return FALSE;
}

//----------------------------------------------------------------------------------------------
//	class CXMLBaseDescriptionParser implementation
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLBaseDescriptionParser, CObject)

//----------------------------------------------------------------------------------------------
CXMLBaseDescriptionParser::CXMLBaseDescriptionParser ()
	:
	m_bExpandNS	(TRUE)
{
}

//----------------------------------------------------------------------------------------------
CXMLBaseDescriptionParser::CXMLBaseDescriptionParser (const CString& sTagName, BOOL	bExpandNs)
{
	m_sTagName		= sTagName;
	m_bExpandNS		= bExpandNs;
}

//----------------------------------------------------------------------------------------------
CXMLBaseDescriptionParser::~CXMLBaseDescriptionParser ()
{
}

//----------------------------------------------------------------------------------------------
void CXMLBaseDescriptionParser::SetTagName (const CString& sTagName)
{
	m_sTagName = sTagName;
}

//----------------------------------------------------------------------------------------------
void CXMLBaseDescriptionParser::SetExpandNamespace	(const BOOL& bValue)
{
	m_bExpandNS	= bValue;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLBaseDescriptionParser::Parse (CXMLNode* pNode, CBaseDescription* pDescri, const CTBNamespace& aParent)
{
	if (!pNode || !pDescri)
		return FALSE;

	CString sValue;

	pNode->GetName(sValue);
	if (_tcsicmp(sValue, m_sTagName) != 0)
	{
		AfxGetDiagnostic()->Add(cwsprintf(_TB("Wrong XML description for module {0-%s}: expected {1-%s}, found {2-%s}. Registration ignored."), 
							(LPCTSTR) aParent.ToString(), (LPCTSTR) m_sTagName, (LPCTSTR) sValue));
		return FALSE;
	}

	// prima guardo se c'è un namespace
	pNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, sValue);

	if (sValue.IsEmpty())
		pNode->GetAttribute(XML_NAME_ATTRIBUTE, sValue);

	pDescri->SetNamespace (sValue, aParent);

	// se non ha name lo salto
	if (sValue.IsEmpty())
	{
		AfxGetDiagnostic()->Add(cwsprintf(_TB("Wrong XML description for module {0-%s} Tag <{1-%s}>: attribute <name> or <namespace> not found.  Registration ignored."), 
							(LPCTSTR) aParent.ToString(), (LPCTSTR) m_sTagName));
		return FALSE;
	}

	CXMLNode* pDocNode;
	
	// se esiste come attributo localize
	if (!pNode->GetAttribute(XML_LOCALIZE_ATTRIBUTE, sValue))
		if (pDocNode = pNode->GetChildByName(XML_TITLE_TAG))
			pDocNode->GetText(sValue);

	if (sValue.IsEmpty())
		sValue = pDescri->GetName();

	pDescri->SetNotLocalizedTitle(sValue);
	return TRUE;
}

//----------------------------------------------------------------------------------------------
void CXMLBaseDescriptionParser::Unparse (CXMLNode* pNode, CBaseDescription* pDescri)
{
	if (!pNode || !pDescri)
		return;

	CXMLNode* pNewNode = pNode->CreateNewChild(m_sTagName);
	pNewNode->SetAttribute(XML_NAME_ATTRIBUTE, (LPCTSTR) (m_bExpandNS ? pDescri->GetNamespace().ToUnparsedString() : pDescri->GetName()));
	pNewNode->SetAttribute(XML_LOCALIZE_ATTRIBUTE, pDescri->GetNotLocalizedTitle());
}
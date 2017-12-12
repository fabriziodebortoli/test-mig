#pragma once

#include <TbXmlCore\XmlDocObj.h>
#include <TbXmlCore\XmlTags.h>

#include <TbGeneric\DataObj.h>
#include <TbGeneric\DataObjDescription.h>

#include "BeginH.dex"

class DataObj;
class DataType;

//esegue una chiamata ad una funzione di una classe di ApplicationServerInterface
//----------------------------------------------------------------------------
class TB_EXPORT CFunctionDescription : public CBaseDescription
{
	DECLARE_DYNCREATE(CFunctionDescription);

private:
	CString					m_strService;			// nome del WEB service (se esterno)
	CString					m_strServiceNamespace;	// namespace del WEB service (se esterno)
	CString					m_strServer;			// nome del server del WEB service (se esterno)
	UINT					m_nPort;				// porta del WEB service (se esterno)

	CDataObjDescription		m_ReturnValue;
	CBaseDescriptionArray	m_arFunctionParams;	// parametri di input

	BOOL					m_bPublished;
	BOOL					m_bPostCommand;
	BOOL					m_bAlwaysCalledIfEvent;
	CString					m_sManagedType;
	CString					m_sExecutePolicy;
	//BOOL					m_bThisCallMethods;
	CString					m_sClassType;
	CString					m_sTBScript;

public:
	CString					m_strError;			// descrizione errore funzione
	CString					m_strGroup;
public:
	//costruttore che prende un XML, lo parsa e riempie i membri
	CFunctionDescription (CString strFunctionName);
	CFunctionDescription (CTBNamespace::NSObjectType aNSType);
	CFunctionDescription (const CTBNamespace& ns);
	CFunctionDescription (const CFunctionDescription& fd) { Assign(fd); }
	CFunctionDescription ();

public:
	virtual const CString GetTitle	() const;

	//usate dal framework
	int						AddParam			(CDataObjDescription* pParam);
	int						InternalAddParam	(const CString& strParamName, DataObj* pValue, BOOL bDataObjOwner);
	//usate queste per aggiungere parametri al woorminfo
	int						AddStrParam			(const CString& strParamName, const CString& strValue);
	int						AddIntParam			(const CString& strParamName, int nValue);
	int						AddTimeParam		(const CString& strParamName, CTime dtValue);
	int						AddParam			(const CString& strParamName, DataObj* pValue);
	
	int						AddOutParam			(const CString& strParamName, DataObj* pValue);
	int						AddInOutParam		(const CString& strParamName, DataObj* pValue);

	int						AddOutParam			(CDataObjDescription* pParam);
	int						AddInOutParam		(CDataObjDescription* pParam);

	BOOL					RemoveParam			(const CString& strParamName);

	CDataObjDescription&	GetReturnValueDescription	() { return m_ReturnValue; }
	void					SetReturnValueDescription	(const CDataObjDescription& aRetValue);

	DataType				GetReturnValueDataType		() { return m_ReturnValue.GetDataType(); }
	DataObj*				GetReturnValue				() { return m_ReturnValue.GetValue(); }

	void					SetReturnValue				(const DataObj& aVal) { m_ReturnValue.SetValue(aVal); }
	void					SetVoidDefaultReturnValue	(const BOOL bValue)  { m_ReturnValue.SetVoidAsDefaultType(bValue); }

	BOOL					GetParamValue		(CString strParamName, CString &strRetVal);
	BOOL					GetParamValue		(CString strParamName, int& nRetVal);
	BOOL					GetBoolParamValue	(CString strParamName, BOOL& bRetVal);
	BOOL					GetParamValue		(CString strParamName, CTime& dtRetVal);
	BOOL					GetParamValue		(CString strParamName, CStringArray& arrayRetVal);

	DataObj*				GetParamValue		(const CString& strParamName);
	DataObj*				GetParamValue		(int i);	

	int						GetParamIndex		(const CString& strParamName);

	void					SetParamValue		(const CString& strParamName, const DataObj& aVal);
	void					SetParamValue		(int i, const DataObj& aVal);

	CDataObjDescription*	GetParamDescription (const int& i);
	CDataObjDescription*	GetParamDescription	 (const CString& strParamName);
	void					RemoveParamsStartingWith(const CString& strParamStart);
	CBaseDescriptionArray&	GetParameters		() { return m_arFunctionParams; }
	BOOL					SetParametersValue	(CBaseDescriptionArray&);
	
	int						GetContextHandle	();
	int						GetParamCount		() const { return m_arFunctionParams.GetCount(); }

	const BOOL				IsPublished			() const { return m_bPublished; } 
		  BOOL				IsThisCallMethods	() const { return /*m_bThisCallMethods*/!m_sClassType.IsEmpty(); } 
	const CString&			GetError			() const { return m_strError;	}
	const BOOL				IsPostCommand		() const { return m_bPostCommand; } 
	const BOOL				AlwaysCalledIfEvent	() const { return m_bAlwaysCalledIfEvent; } 
	const BOOL				IsManaged			() const { return !m_sManagedType.IsEmpty(); } 
	const CString&			GetManagedType		() const { return m_sManagedType;	}
	const BOOL				IsFullExecutePolicy	() const;
	const BOOL				IsAddOnExecutePolicy() const;
	const BOOL				IsBaseExecutePolicy	() const;
	const BOOL				IsContextFunction	();
	const BOOL				IsExternalFunction	() const  { return !GetService().IsEmpty();}

	const CString&			GetService			() const { return m_strService; } 
	const CString&			GetServiceNamespace	() const { return m_strServiceNamespace; } 
	const CString&			GetServer			() const { return m_strServer; } 
	const UINT&				GetPort				() const { return m_nPort; } 

	void					SetService			(const CString& value) { m_strService = value; } 
	void					SetServiceNamespace	(const CString& value) { m_strServiceNamespace = value; } 
	void					SetServer			(const CString& value) { m_strServer = value; } 
	void					SetPort				(UINT value)		   { m_nPort = value; } 

	void SetPublished			(const BOOL bValue)  { m_bPublished = bValue; } 
	//void SetThisCallMethods		(const BOOL bValue)  { m_bThisCallMethods = bValue; } 
	void SetAlwaysCalledIfEvent	(const BOOL bValue);
	
	BOOL ParsePrototype			(CXMLNode* pNode, const CTBNamespace& aParent);
	void UnparsePrototype		(CXMLNode*, const CString& sContainerTag = _T(""));

	BOOL	ParseRequest			(CXMLNode* pNode, const CTBNamespace& aParent = CTBNamespace());
	void	UnparseRequest			(CXMLNode* pNode, const CString& sContainerTag = _T(""));

	BOOL	ParseArguments			(CXMLNode* pNode, CTBNamespace* pParent = NULL);
	BOOL	ParseArguments			(const DataStr& strXml);

	BOOL	UnparseArguments		(CXMLNode* pNode);
	BOOL	UnparseArguments		(DataStr& strXml, LPCTSTR szRoot = XML_FUNCTION_TAG);	//XML_ARGUMENTS_TAG

	BOOL	ParseTbLinkArguments	(const CString& sArgs);
	BOOL	UnparseTbLinkArguments	(DataStr& sArgs);

	BOOL	ParseReturnValue		(CXMLNode* pNode, const CTBNamespace& aParent);
	void	ParseTbScript			(CXMLNode* pNode);

	const	CString&	GetTBScript		() const { return m_sTBScript; }
	void				SetTBScript		(const CString& sTBScript) { m_sTBScript = sTBScript; }

	const	CString&	GetClassType	() const { return m_sClassType; }
	void				SetClassType	(const CString& sClassType) { m_sClassType = sClassType; }

	// operatori e metodi equivalenti
public:
	CFunctionDescription&	operator=	(const CFunctionDescription& fd);
	BOOL					operator!=	(const CFunctionDescription& fd);
	BOOL					operator==	(const CFunctionDescription& fd);
	
	void					Assign	(const CFunctionDescription& fd);
	BOOL					IsEqual	(const CFunctionDescription& fd);
	CFunctionDescription*	Clone	();
	CString					ToString();
	CString					GetUrl();
	CString					GetHelpSignature();
};


//----------------------------------------------------------------
class TB_EXPORT CDecoratedFunctionDescription : public CFunctionDescription
{
	DECLARE_DYNCREATE(CDecoratedFunctionDescription)

public:
	CDecoratedFunctionDescription();
	CDecoratedFunctionDescription(const CDecoratedFunctionDescription& dd);
	CDecoratedFunctionDescription(CTBNamespace::NSObjectType aNSType);
protected:
	CString m_sRemarks;
	CString m_sExample;
	CString m_sResult;
	CString m_sPrototype;

public:
	const CString		GetRemarks()			const { return m_sRemarks; }
	const CString		GetExample()			const { return m_sExample; }
	const CString		GetResult()				const { return m_sResult; }
	const CString		GetPrototype()			const { return m_sPrototype; }

	void SetRemarks(const CString	aRemarks) { m_sRemarks = aRemarks; }
	void SetExample(const CString	aExample) { m_sExample = aExample; }
	void SetResult(const CString	aResult)	{ m_sResult = aResult; }
	void SetPrototype(const CString	aPrototype) { m_sPrototype = aPrototype; }

	CDecoratedFunctionDescription&	operator=		(const CDecoratedFunctionDescription& dd) { return Assign(dd); }
	BOOL						operator!=		(const CDecoratedFunctionDescription& dd);
	BOOL						operator==		(const CDecoratedFunctionDescription& dd);

	CDecoratedFunctionDescription&	Assign(const CDecoratedFunctionDescription& dd);
	BOOL						IsEqual(const CDecoratedFunctionDescription& dd);
};

#include "EndH.dex"

#pragma once

#include <TbNameSolver\TBNamespaces.h>
#include <TbNameSolver\TBResourceLocker.h>
#include <TbNameSolver\JsonSerializer.h>

#include "ARRAY.H" 
#include <vector>
#include "beginh.dex"
class CWndObjDescription;
class CLocalizableDialog;
class CAcceleratorDescription;
class CParsedForm;

class CFontDescription;

#define SPACING 2
#define COL1 _T("COL1")
#define COL2 _T("COL2")

//questa classe costruisce dinamicamente un template di risorsa, equilvalente alla build di un RC, da usare con la CreateDialogIndirect
class TB_EXPORT DialogTemplate {
public:
							LPCDLGTEMPLATE	Template(int & size);
							void			AlignToDword();
							void			Write(LPCVOID pvWrite, DWORD cbWrite);
	template<typename T>	void			Write(T t);
							void			WriteString(LPCWSTR psz);
private:
	std::vector<BYTE> v;
};


class TB_EXPORT CJsonContextObj
{
public:
	CFont* m_pFont = NULL;
	TArray<CFont> m_arFonts;//font creati da json
	CWnd* m_pWnd = NULL;
	CWndObjDescription *m_pDescription = NULL;
	bool m_bOwnDescription = true;
	CJsonResource m_JsonResource;
	CString m_strCurrentResourceContext;//path corrente da dove sto parsando il file (puo cambiare nel caso di href nidificati)
	DefineMap	m_Defines;
	int m_LatestY1 = 0;//l'ultimo rettangolo associato al controllo posizionato in modalità automatica (che segue il flusso verticale)
	int m_LatestY2 = 0;//l'ultimo rettangolo associato al controllo posizionato in modalità automatica (che segue il flusso verticale)
	CMap<CString, LPCTSTR, CRect, CRect> m_Rects;
	CJsonContextObj* m_pParentContext = NULL;
	BOOL m_bIsJsonDesigner = FALSE;
protected:
	CJsonContextObj(){ m_arFonts.SetOwns(TRUE); }

	
public:
	virtual ~CJsonContextObj();
	virtual void Associate(CWnd* pWnd) = 0;
	virtual bool CanCreateControl(CWndObjDescription* pWndDesc, UINT nID) = 0;
	virtual void Assign(CJsonContextObj* pOther);
	virtual void BuildDataControlLinks() = 0;
	LPCDLGTEMPLATE CreateTemplate(bool popup);
	LPCDLGTEMPLATE CreateTemplate(CWndObjDescription* pDescription, DWORD nStyle, DWORD nExStyle);
	static LPCDLGTEMPLATE CreateTemplate(const CRect& rect, const CString& sTitle, DWORD nStyle, DWORD nExStyle);
	
	CFont* CreateFontFromDesc(CFont* pTemplateFont, CFontDescription* pFontDesc);

	CJsonContextObj* GetRootContext();
	virtual bool Evaluate(CJsonExpressions& expressions, CWndObjDescription* pDescri) = 0;
	virtual bool CheckActivationExpression(const CString& activationExpression) = 0;

};


typedef CMap<HWND, HWND, CWndObjDescription*, CWndObjDescription*> WindowMap;
typedef CMap<CWndObjDescription*, CWndObjDescription*, HWND, HWND> ReverseWindowMap;


class TB_EXPORT CJsonFormEngineObj
{
	friend class CWndObjDescription;
	friend class CJsonTileDialog;
private:
	DECLARE_LOCKABLE(WindowMap, m_Windows);//cache delle descrizioni
	
protected:
	static CJsonFormEngineObj* g_pJsonFormEngine;
public:
	virtual ~CJsonFormEngineObj();
	static CJsonFormEngineObj* GetInstance();
	virtual BOOL CreateChilds(CJsonContextObj* pContext, CWnd* pParentWnd);
	virtual void ClearCache() = 0;
	virtual void BuildWebControlLinks(CParsedForm* pParsedForm, CJsonContextObj* pContext) = 0;
	virtual CJsonContextObj* CreateContext() = 0;
	virtual CJsonContextObj* CreateContext(const CJsonResource& sJsonResource, bool bCacheDescriptions = true) = 0;
	virtual void MergeContext(const CJsonResource& sJsonResource, CJsonContextObj* pContext) = 0;
	virtual void GetDeltaJsonFormInfos(const CString& sJsonId, CArray<CJsonResource>& sources) = 0;
	virtual BOOL IsValid(CLocalizableDialog* pDialog) = 0; 
	virtual BOOL GetLeftMargin(CWndObjDescription* pWndDesc, bool col1, bool bInStaticArea) = 0;
	void AddAssociation(HWND, CWndObjDescription* pDesc);
	void RemoveAssociation(HWND, CWndObjDescription* pDesc);
	CWndObjDescription* GetAssociation(HWND);
	static CString GetObjectName(CWndObjDescription* pWndDesc);
	CWndObjDescription* ParseDescriptions(CJsonContextObj* pContext, CArray<CJsonResource>& sources);
	static bool IsExpression(const CString& sText, CString& sBareText);
	static DWORD GetID(CWndObjDescription* pWndDesc);
protected:
	BOOL GetJsonFormInfo(const CString& sName, const CTBNamespace& ns, CString& sJsonFile, CTBNamespace& ownerModule);
	virtual BOOL ProcessWndDescription(CWndObjDescription* pWndDesc, CWnd* pParentWnd, CJsonContextObj*);
	
protected:
	void ParseDescription(CArray<CWndObjDescription*>&ar, CJsonContextObj* pContext, CJsonResource source, LPCTSTR sActivation, CWndObjDescription* pDescriptionToMerge, int expectedType);
	void ParseDescription(CArray<CWndObjDescription*>&ar, CJsonContextObj* pContext, const CString& sFile, LPCTSTR sActivation, CWndObjDescription* pDescriptionToMerge, int expectedType);
	CString CalculateLURect(CWndObjDescription* pWndDesc, CJsonContextObj* pContext);
	void UpdateAnchorInfo(const CString& sAnchor, CWndObjDescription* pWndDesc, CJsonContextObj* pContext);
public:
	static void ParseDescriptionFromText(CArray<CWndObjDescription*>&ar, CJsonContextObj* pContext, LPCTSTR lpszText, LPCTSTR sActivation, CWndObjDescription* pDescriptionToMerge, int expectedType);
};

class TB_EXPORT CJsonFormParser : public CJsonParser
{
public:
	bool m_bForAppend = false;
	CString m_sActivation;//per propagare l'attivazione ad ogni child di un elemento referenziato tramite href
	CJsonContextObj* m_pRootContext = NULL;

	CString ResolveString(LPCTSTR szName, UsedDefines& resolvedDefines);
	bool ResolveString(LPCTSTR szName, CJsonExpressions& map, CString& i);
	int ResolveInt(LPCTSTR szName, UsedDefines& resolvedDefines);
	bool ResolveInt(LPCTSTR szName, CJsonExpressions& map, int& i);
	bool ResolveBool(LPCTSTR szName, UsedDefines& resolvedDefines);
	bool ResolveBool(LPCTSTR szName, CJsonExpressions& map, bool& b);
	bool ResolveBool3(LPCTSTR szName, CJsonExpressions& map, Bool3& b);
	double ResolveDouble(LPCTSTR szName, UsedDefines& resolvedDefines);
	bool ResolveDouble(LPCTSTR szName, CJsonExpressions& map, double& d);
	bool ResolveValue(LPCTSTR szName, UsedDefines& resolvedDefines, Json::Value& val);

};

TB_EXPORT HACCEL		TBLoadAccelerators(UINT nAccelIDR);
TB_EXPORT CJsonContextObj* TBLoadAcceleratorContext(UINT nAccelIDR);
#include "endh.dex"

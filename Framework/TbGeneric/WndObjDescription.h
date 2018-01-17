#pragma once

class CDummyDescription;

#include <TBNamesolver\threadcontext.h>
#include <TbNameSolver\JsonSerializer.h>
#include "JsonFormEngine.h"

#include "dataobj.h"
#include "JsonTags.h"

//includere alla fine degli include del .H
#include "beginh.dex"



///////////////////////////////////////////////////////////////////////////////////////
//               PARSED CONTROL STYLES
///////////////////////////////////////////////////////////////////////////////////////
// Common control styles
//
#define CTRL_STYLE_SHOW_FIRST				0x00010000
#define CTRL_STYLE_SHOW_LAST				0x00020000
#define CTRL_STYLE_STORED_AUTO_EXPRESSION	0x00040000
#define COMBO_DROPDOWNLIST_STYLE_ENABLE_DEL	0x00080000
#define CTRL_STYLE_DO_POS_CHANGING			0x00008000
#define CTRL_STYLE_RESET_DEFAULTS			0x00100000

// ParsedEdit/ParsedCombo control sub-styles for strings :
//
// String style accepted
//
#define STR_STYLE_NUMBERS			0x00000001
#define STR_STYLE_LETTERS			0x00000002
#define STR_STYLE_UPPERCASE			0x00000004
#define STR_STYLE_OTHERCHARS		0x00000008
#define STR_STYLE_ALL           	0x00000010
#define STR_STYLE_NO_EMPTY			0x00000020
#define STR_STYLE_BLANKS			0x00000040
#define STR_STYLE_NAMESPACE			0x00000400
#define STR_STYLE_FILESYSTEM		0x00000800

//
// Numeric style accepted
//
#define NUM_STYLE_SHOW_ZERO			0x00000040
#define NUM_STYLE_NO_RIGHT_ALIGN	0x00000002
#define NUM_STYLE_HEXADECIMAL_VALUE	0x00000004
#define NUM_STYLE_INVERT_SIGN		0x00000008

//
// Path style accepted
//
#define PATH_STYLE_AS_PATH			0x00000100
#define PATH_STYLE_NO_CHECK_EXIST	0x00000200
#define PATH_STYLE_NO_EMPTY			STR_STYLE_NO_EMPTY

//
// Identifier style accepted
//
#define IDE_STYLE_NO_EMPTY			STR_STYLE_NO_EMPTY
#define IDE_STYLE_NO_CHECK			0x00001000
#define IDE_STYLE_MUST_NO_EXIST		(0x00002000 | IDE_STYLE_NO_EMPTY)
#define IDE_STYLE_MUST_EXIST		(0x00004000 | IDE_STYLE_NO_EMPTY)

// ParsedStatic style accepted
//
#define BMP_STYLE_STRETCH			0x00000001
#define STR_STYLE_NO_WORDBREAK		0x00000002

// PushButton style accepted
//
#define BTN_STYLE_NORMAL			0x00000000
#define BTN_STYLE_INVERTED			0x00000001

class CWndObjDescription;
class CControlBehaviourDescription;
class CMenuDescription;
class CParsedCtrl;
class CStreamSerializer;
class CWndObjDescriptionContainer;
#define EMPTY_COLOR 0xffffffff
#define NULL_COORD -10000
#define WND_IMAGE_BACK_COLOR (COLORREF)0xFF00FF

#define UPDATE_BOOL(var, style)\
bool b_##var = (dwStyle & style) == style;\
if (var != b_##var)\
{\
	var = b_##var;\
	SetUpdated(&var);\
}
#define UPDATE_BOOL_EX(var, style)\
bool b_##var = (dwExStyle & style) == style;\
if (var != b_##var)\
{\
	var = b_##var;\
	SetUpdated(&var);\
}
namespace ATL
{
	class CImage;
}
namespace Gdiplus
{
	class Image;
}



#define SERIALIZE_BOOL3(var, name) if (!SerializeExpression(name, &var, strJson) && (var != B_UNDEFINED)) { strJson.WriteBool(name, var == B_TRUE); }
#define SERIALIZE_BOOL(var, name, def) if (!SerializeDefine(name, strJson) && !SerializeExpression(name, &var, strJson) && (var != def)) { strJson.WriteBool(name, var);}
#define SERIALIZE_INT(var, name, def) if (!SerializeDefine(name, strJson) && !SerializeExpression(name, &var, strJson) && (var != def)) { strJson.WriteInt(name, var);}
#define SERIALIZE_STRING(var, name) if (!SerializeDefine(name, strJson) && !SerializeExpression(name, &var, strJson) && (!var.IsEmpty())) { strJson.WriteString(name, var);}
#define SERIALIZE_ENUM(var, name, def) if (!SerializeDefine(name, strJson) && (var != def)) { strJson.WriteString(name, GetEnumDescription(var));}
#define SERIALIZE_DOUBLE(var, name, def) if (!SerializeDefine(name, strJson) && !SerializeExpression(name, &var, strJson) && (var != def)) { strJson.WriteDouble(name, var);}

#define PARSE_BOOL3(var, name) if (parser.Has(name)) { parser.ResolveBool3(name, m_Expressions, var); } 
#define PARSE_BOOL(var, name) if (parser.Has(name)) { if (parser.IsObject(name)) var = parser.ResolveBool(name, m_Defines); else parser.ResolveBool(name, m_Expressions, var);}
#define PARSE_INT(var, name) if (parser.Has(name)) { if (parser.IsObject(name)) var = parser.ResolveInt(name, m_Defines); else parser.ResolveInt(name, m_Expressions, var);}
#define PARSE_STRING(var, name) if (parser.Has(name)) { if (parser.IsObject(name)) var = parser.ResolveString(name, m_Defines); else parser.ResolveString(name, m_Expressions, var);}
#define PARSE_DOUBLE(var, name) if (parser.Has(name)) { if (parser.IsObject(name)) var = parser.ResolveDouble(name, m_Defines); else parser.ResolveDouble(name, m_Expressions, var);}

#define PARSE_ENUM(var, name, enumType) \
		if (parser.Has(name)) \
		{	\
			int val; \
			bool ret = parser.TryReadInt(name, val);	\
			if (ret)	\
			{	\
				var = (enumType)val;	\
			}	\
			else   \
			{	\
				CString sEnum = parser.ReadString(name);	\
				GetEnumValue(sEnum, var);	\
			}   \
		}   \

typedef CArray<HACCEL, HACCEL> AcceleratorArray;

//TENERE ALLINEATI QUESTI ENUMERATIVI CON QUELLI IN TaskBuilderNet\Microarea.TaskBuilderNet.UI\TBWebFormControl\WindowDescriptions.cs
enum WndDescriptionState { REMOVED, UNCHANGED, UPDATED, ADDED, PARSED };
enum TextAlignment { TALEFT = 0, TACENTER = 1, TARIGHT = 2 };
enum VerticalAlignment { VATOP = 0, VACENTER = 1, VABOTTOM = 2 };
//TENERE ALLINEATI QUESTI ENUMERATIVI CON QUELLI IN TaskBuilderNet\Microarea.TaskBuilderNet.UI\TBWebFormControl\WindowDescriptions.cs


enum ResizableControl { R_NONE = 0, R_VERTICAL = 1, R_HORIZONTAL = 2, R_ALL = 3 };
enum ListCtrlAlign { LC_TOP = 0, LC_LEFT = 1 };
enum OwnerDrawType { NO, ODFIXED, ODVARIABLE };
enum SpinCtrlAlignment { UNATTACHED = -1, SC_LEFT = 0, SC_RIGHT = 1 };
enum ComboType { SIMPLE, DROPDOWN, DROPDOWNLIST };
enum EtchedFrameType { EFNO = -1, EFALL = 0, EFHORZ = 1, EFVERT = 2 };
enum ListCtrlViewMode { LC_ICON = -1, LC_SMALLICON = 0, LC_LIST = 1, LC_REPORT = 2 };
enum SelectionType { SINGLE, NOSEL, EXTENDEDSEL, MULTIPLESEL };
enum SplitterMode { S_VERTICAL, S_HORIZONTAL };
enum CommandCategory { CTG_UNDEFINED, CTG_EDIT, CTG_NAVIGATION, CTG_SEARCH, CTG_PRINT, CTG_TOOLS, CTG_ADVANCED, CTG_DELETE };
enum ViewCategory { VIEW_ACTIVITY, VIEW_BATCH, VIEW_DATA_ENTRY, VIEW_FINDER, VIEW_PARAMETER, VIEW_WIZARD };
enum ControlStyle {
	CS_NONE = 0,
	CS_RESET_DEFAULTS = CTRL_STYLE_RESET_DEFAULTS,
	CS_NUMBERS = STR_STYLE_NUMBERS,
	CS_LETTERS = STR_STYLE_LETTERS,
	CS_UPPERCASE = STR_STYLE_UPPERCASE,
	CS_OTHERCHARS = STR_STYLE_OTHERCHARS,
	CS_ALLCHARS = STR_STYLE_ALL,
	CS_NO_EMPTY = STR_STYLE_NO_EMPTY,
	CS_BLANKS = STR_STYLE_BLANKS,
	CS_COMBO_DROPDOWNLIST_STYLE_ENABLE_DEL = COMBO_DROPDOWNLIST_STYLE_ENABLE_DEL,
	CS_PATH_STYLE_AS_PATH = PATH_STYLE_AS_PATH,
	CS_PATH_STYLE_NO_CHECK_EXIST = PATH_STYLE_NO_CHECK_EXIST
};

class CListDescription;
class CMyMemFile;
TB_EXPORT BOOL SafeMapDialog(HWND hDlg, CRect& rect);
TB_EXPORT BOOL SafeMapDialog(HWND hDlg, CSize& sz);
TB_EXPORT BOOL SafeMapDialog(HWND hDlg, CPoint& pt);
TB_EXPORT BOOL ReverseMapDialog(HWND hDlg, CRect& rect);
TB_EXPORT BOOL ReverseMapDialog(HWND hDlg, CSize& sz);
TB_EXPORT BOOL ReverseMapDialog(HWND hDlg, CPoint& pt);


#define szJsonImagePngSessionCache				_T("image/%s.png?session=%s&cacheId=%s")
#define szJsonImagePngSessionCacheIconSource	_T("image/%s.png?session=%s&cacheId=%s&iconSource=%s")

class TB_EXPORT SerializableObj : public CObject
{
public:
	virtual void SerializeBinary(CStreamSerializer& serializer) = 0;
};
class TB_EXPORT CStreamSerializer
{
protected:
	CFile* m_pStream;
public:
	CStreamSerializer(CFile* pStream) : m_pStream(pStream) {}

	virtual void Serialize(CString& str) = 0;
	virtual void Serialize(CStringA& str) = 0;
	virtual void Serialize(BYTE* pBuff, UINT nSize) = 0;
	virtual void Serialize(int& i) = 0;
	virtual void Serialize(bool& b) = 0;
	virtual void Serialize(double& d) = 0;
	virtual void Serialize(float& f) = 0;
	virtual void Serialize(DWORD& dw) = 0;
	virtual void Serialize(SerializableObj*& pT) = 0;
	virtual void Serialize(CWndObjDescriptionContainer& cnt) = 0;
	virtual void Serialize(CWndObjDescription*& cnt) = 0;
};
class TB_EXPORT CStreamWriter : public CStreamSerializer
{

public:
	CStreamWriter(CFile* pStream) : CStreamSerializer(pStream) {}

	void Serialize(CString& str);
	void Serialize(CStringA& str);
	void Serialize(BYTE* pBuff, UINT nSize)
	{
		m_pStream->Write(pBuff, nSize);
	}
	template <class T> void Serialize(T t)
	{
		m_pStream->Write(&t, sizeof(t));
	}
	virtual void Serialize(int& i);
	virtual void Serialize(bool& b);
	virtual void Serialize(double& d);
	virtual void Serialize(float& f);
	virtual void Serialize(DWORD& dw);
	virtual void Serialize(SerializableObj*& pT);
	virtual void Serialize(CWndObjDescriptionContainer& cnt);
	virtual void Serialize(CWndObjDescription*& cnt);
};

class TB_EXPORT CStreamReader : public CStreamSerializer
{

public:
	CStreamReader(CFile* pStream) : CStreamSerializer(pStream) {}

	void Serialize(CString& str);
	void Serialize(CStringA& str);
	void Serialize(BYTE* pBuff, UINT nSize)
	{
		m_pStream->Read(pBuff, nSize);
	}
	template <class T> void Serialize(T& t)
	{
		m_pStream->Read(&t, sizeof(t));
	}
	virtual void Serialize(int& i);
	virtual void Serialize(bool& b);
	virtual void Serialize(double& d);
	virtual void Serialize(float& f);
	virtual void Serialize(DWORD& dw);
	virtual void Serialize(SerializableObj*& pT);
	virtual void Serialize(CWndObjDescriptionContainer& cnt);
	virtual void Serialize(CWndObjDescription*& cnt);
};

class TB_EXPORT ExpressionObject
{
public:
	UsedDefines	m_Defines;
	CJsonExpressions	m_Expressions;
	bool SerializeDefine(LPCTSTR szName, CJsonSerializer& strJson);
	bool inline SerializeExpression(LPCTSTR szName, void* pVal, CJsonSerializer& strJson);
	void Assign(ExpressionObject* pDesc);
	virtual void ActivateDefines(bool bActive);
};
//==============================================================================
class TB_EXPORT IJsonObject
{
public:
	virtual void SerializeJson(CJsonSerializer& serializer) = 0;
	virtual void ParseJson(CJsonFormParser& parser) = 0;
};
//==============================================================================
class TB_EXPORT CWndObjDescriptionContainer : public CArray<CWndObjDescription*, CWndObjDescription*>, public IJsonObject
{
	CWndObjDescription* m_pParent;
	/// <summary>
	/// Whether the list can be sorted.
	/// </summary>
	/// <remarks>
	///
	/// </remarks>	
	BOOL m_bSortable;

public:
	INT_PTR Add(CWndObjDescription* newElement);

public:
	/// <summary>
	/// Returns the ID the ctrl is asigned when its description will be created.
	/// </summary>
	/// <param name="pWnd"></param>
	/// <remarks>
	///
	/// </remarks>
	static CString GetCtrlID(CWnd* pWnd);
	static CString GetCtrlID(HWND hWnd);


	CWndObjDescriptionContainer(void) : m_pParent(NULL), m_bSortable(TRUE) {}
	virtual ~CWndObjDescriptionContainer(void);

	void Assign(CWndObjDescriptionContainer *pDesc);

	CWndObjDescription* AddWindow(CWnd* pWnd, CString strId = _T(""));
	void SetParent(CWndObjDescription* pParent) { m_pParent = pParent; }
	CWndObjDescription* GetParent() { return m_pParent; }
	CWndObjDescription* GetWindowDescription(CWnd* pWnd, CRuntimeClass* pClass, CString strId = _T(""), int nExpectedIndex = -1);
	void MoveGroupBoxChildren();
	void ApplyDeltaDesc(CWndObjDescription* pOriginal);
	void SortByTabOrder();
	BOOL GetSortable() { return m_bSortable; }
	void SetSortable(BOOL bSortable) { m_bSortable = bSortable; }
	bool RemoveItem(CWndObjDescription*, BOOL bDelete = TRUE);
	int IndexOf(CWndObjDescription*);
	int IndexOf(const CString& strId);
	virtual void SerializeJson(CJsonSerializer& serializer);
	virtual void ParseJson(CJsonFormParser& parser);
	void ParseJsonItem(CJsonFormParser& parser);
	void AddJsonItem(CWndObjDescription* pObj);
	CWndObjDescription* GetChild(CString sId, BOOL bRecursive);
};

class TB_EXPORT CAcceleratorItemDescription
{
public:
	ACCEL m_Accel;
	CString m_sId;
	CString m_sActivation;
	CAcceleratorItemDescription()
	{
		ZeroMemory(&m_Accel, sizeof(ACCEL));
	}
	CAcceleratorItemDescription* Clone()
	{
		CAcceleratorItemDescription* pDesc = new CAcceleratorItemDescription;
		pDesc->Assign(this);
		return pDesc;
	}
	void Assign(CAcceleratorItemDescription* pDesc)
	{
		m_Accel = pDesc->m_Accel;
		m_sId = pDesc->m_sId;
		m_sActivation = pDesc->m_sActivation;
	}
};
class TB_EXPORT CAcceleratorDescription : public ExpressionObject
{
public:

	CArray<CAcceleratorItemDescription*> m_arItems;

	CAcceleratorDescription()
	{

	}
	CAcceleratorDescription(const AcceleratorArray& hAccelTables);
	~CAcceleratorDescription()
	{
		Clear();
	}
	void Clear()
	{
		for (int i = 0; i < m_arItems.GetCount(); i++)
			delete m_arItems[i];
		m_arItems.RemoveAll();
	}
	void Assign(CAcceleratorDescription* pDesc);
	void Append(CAcceleratorDescription* pDesc);
	CAcceleratorDescription* Clone();
	ACCEL* ToACCEL(CJsonContextObj* pContext, int& nSize);
	CString GetDescription(WORD id);
	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
};

//-----------------------------------------------------------------------------
class TB_EXPORT CFontDescription : public SerializableObj
{
	DECLARE_DYNCREATE(CFontDescription);

	CString	m_strFaceName;
	float	m_nFontSize = 0;
	bool	m_bIsBold = false;
	bool	m_bIsItalic = false;
	bool	m_bIsUnderline = false;
	CFontDescription() {}
public:
	CFontDescription(CString strFaceName, float nFontSize) :
		m_strFaceName(strFaceName),
		m_nFontSize(nFontSize)
	{}
	CFontDescription* Clone();
	void Assign(CFontDescription* pDesc);

	void SerializeJson(CJsonSerializer& strJson, LPCTSTR szTagName);
	void ParseJson(CJsonFormParser& parser, LPCTSTR szTagName);
	void SerializeBinary(CStreamSerializer& serializer);
};


//-----------------------------------------------------------------------------
class TB_EXPORT CNumbererDescription : public SerializableObj
{
	DECLARE_DYNCREATE(CNumbererDescription);

	bool		m_bUseFormatMask = false;
	CString		m_sFormatMask;
	bool		m_bEnableCtrlInEdit = false;
	CString		m_sServiceNs;
	CString		m_sServiceName;

	CNumbererDescription() {}
public:
	CNumbererDescription* Clone();
	void Assign(CNumbererDescription* pDesc);

	void SerializeJson(CJsonSerializer& strJson, LPCTSTR szTagName);
	void ParseJson(CJsonFormParser& parser, LPCTSTR szTagName);
	void SerializeBinary(CStreamSerializer& serializer);
};


extern const CDummyDescription g_DummyDescription;

class TB_EXPORT CImageBuffer
{
	friend class CWndImageDescription;
	friend class CToolbarBtnDescription;

	UINT m_nSize;
	BYTE* m_pBuffer;
	CString m_strId;
public:
	BOOL Assign(ATL::CImage* pImage, const CString& strId);
	BOOL Assign(Gdiplus::Image* pImage, const CString& strId);
	BOOL Assign(HBITMAP hBitmap, const CString& strId, HDC hDc);
	BOOL Assign(HBITMAP hBitmap, const CString& strId, CWnd* pWnd);
	CImageBuffer() : m_pBuffer(NULL), m_nSize(0) {}
	virtual ~CImageBuffer();

	void Assign(CImageBuffer* pBuff);
	const CString& GetId() { return m_strId; }
	void ToImage(CImage& image);
	void GetData(int& nSize, BYTE*& pBuffer) { nSize = m_nSize; pBuffer = m_pBuffer; }
	void DetachData(int& nSize, BYTE*& pBuffer) { nSize = m_nSize; m_nSize = 0; pBuffer = m_pBuffer; m_pBuffer = NULL; }
	BOOL IsEmpty() { return m_nSize == 0 || m_pBuffer == NULL; }
};
#define CLASS_MAP_SIZE 100 //deve contenere tutti i WndObjType sotto definiti

#define REGISTER_WND_OBJ_CLASS(theClass, theType) bool __dummy##theClass##theType = CWndObjDescription::RegisterClassMap(RUNTIME_CLASS(theClass), CWndObjDescription::theType);

#ifdef TBWEB
class CTextObjDescription;
#endif
#define BTN_DEFAULT_BINDING 0xFFFF// BTN_DEFAULT

class TB_EXPORT HotLinkInfo : public ExpressionObject
{
public:
	CString		m_strName;
	CString		m_strNamespace;
	CString		m_strSelector;
	CString		m_strAddOnFlyNs;
	CStringArray m_arDescriptionFields;
	CStringArray m_arAdditionalSensitiveFields;
	TArray<HotLinkInfo> m_arHotlinks;//hotlink composito
	Bool3		m_bMustExistData = B_UNDEFINED;
	Bool3		m_bEnableAddOnFly = B_UNDEFINED;
	Bool3		m_bEnableHyperLink = B_UNDEFINED;
	Bool3		m_bEnableHotLink = B_UNDEFINED;
	bool		m_bAutoFind = true;
	BOOL IsEmpty();
	HotLinkInfo*  Clone();
	void SerializeJson(CJsonSerializer& strJson, int index = -1);
	void ParseJson(CJsonFormParser& parser, int index = -1);
	void AssignDefaults(HotLinkInfo* pDesc);
	virtual void ActivateDefines(bool bActive);

private:
	void Assign(HotLinkInfo* pDesc);
};

class TB_EXPORT BindingInfo : public ExpressionObject
{
public:
	CString		m_strDataSource;
	CString		m_strName;
	int			m_nButtonId = BTN_DEFAULT_BINDING;
	HotLinkInfo* m_pHotLink = NULL;

	~BindingInfo();
	BindingInfo*  Clone();
	void SerializeJson(CJsonSerializer& strJson);
	void ParseJson(CJsonFormParser& parser);
	void AssignDefaults(BindingInfo* pDesc);
	void ParseHotLink(CJsonFormParser& parser);
	virtual void ActivateDefines(bool bActive);
private:
	void Assign(BindingInfo* pDesc);
};

class TB_EXPORT StateData
{
public:
	~StateData() { SAFE_DELETE(m_pBindings); }
	BindingInfo* m_pBindings = NULL;
	Bool3 m_bInvertDefaultStates = B_UNDEFINED;
	bool m_bEnableStateInEdit = false;
	bool m_bEnableStateCtrlInEdit = false;

	StateData*  Clone();
	void SerializeJson(CJsonSerializer& strJson);
	void ParseJson(CJsonFormParser& parser);
	void AssignDefaults(StateData* pDesc);
private:
	void Assign(StateData* pDesc);
};



//=============================================================================
//			Class EnumDescriptionCouple
//=============================================================================
//Store the association enum value - enum description
template <class TEnum> class EnumDescriptionCouple
{
private:
	TEnum	m_eValue;
	CString m_sDescription;

public:
	EnumDescriptionCouple() {};
	EnumDescriptionCouple(TEnum eValue, CString sDescription)
	{
		m_eValue = eValue;
		m_sDescription = sDescription;
	}

	TEnum	GetValue() { return m_eValue; }
	CString GetDescription() { return m_sDescription; }
};


//=============================================================================
//			Class EnumDescriptionCollection
//=============================================================================
// Array of association enum value - enum description, provide methods to get value from description and vice-versa
template <class TEnum>  class EnumDescriptionCollection : public CArray<EnumDescriptionCouple<TEnum>*>
{
public:
	//--------------------------------------------------------------------------------
	void Add(TEnum value, CString description)
	{
		EnumDescriptionCouple<TEnum>*  pCouple = new EnumDescriptionCouple<TEnum>(value, description);
		__super::Add(pCouple);
	}

	//--------------------------------------------------------------------------------
	TEnum EnumDescriptionCollection::GetEnum(CString sDescription, bool flags = false)
	{
		int e = 0;
			
		if (flags)
		{
			int nStart = 0;
			CString sToken;
			while (true)
			{
				sToken = sDescription.Tokenize(_T("|"), nStart);
				if (nStart == -1)
					break;
				bool found = false;
				for (int i = 0; i < GetSize(); i++)
				{
					EnumDescriptionCouple<TEnum>* o = GetAt(i);
					if (o->GetDescription() == sToken)
					{
						e = e | o->GetValue();
						found = true;
						break;
					}
				}
				ASSERT_TRACE1(found, "Invalid tag for enum: %s", (LPCTSTR)sToken);
			}
		}
		else
		{
			for (int i = 0; i < GetSize(); i++)
			{
				EnumDescriptionCouple<TEnum>* o = GetAt(i);
				if (o->GetDescription() == sDescription)
					return o->GetValue();
			}

			ASSERT_TRACE1(FALSE, "Invalid tag for enum: %s", (LPCTSTR)sDescription);
		}

		return (TEnum)e;
	}

	//--------------------------------------------------------------------------------
	CString EnumDescriptionCollection::GetDescription(TEnum eValue, bool flags = false)
	{
		CString sDescri;
		for (int i = 0; i < GetSize(); i++)
		{
			EnumDescriptionCouple<TEnum>* o = GetAt(i);
			if (flags)
			{
				if (o->GetValue() == 0)
					continue;

				if ((o->GetValue() & eValue) == o->GetValue())
				{
					if (!sDescri.IsEmpty())
						sDescri += _T("|");
					sDescri += o->GetDescription();
				}

			}
			else
			{

				if ((o->GetValue() == eValue))
				{
					sDescri = o->GetDescription();
					break;
				}
			}
		}
		if (!sDescri.IsEmpty())
			return sDescri;
		ASSERT(FALSE);
		return _T("Unknown");
	}
	//--------------------------------------------------------------------------------
	~EnumDescriptionCollection()
	{
		for (int i = 0; i < GetSize(); i++)
		{
			EnumDescriptionCouple<TEnum>* o = GetAt(i);
			delete o;
		}
	}
};

class EnumDescriptionAssociations;


//************************************************
class TB_EXPORT CWndObjDescription : public SerializableObj, public IJsonObject, public ExpressionObject
{

	//************ATTENZIONE************
	//TENERE ALLINEATO QUESTO ENUMERATIVO CON QUELLO IN C:\DEVELOPMENT\Standard\TaskBuilder\TaskBuilderNet\Microarea.TaskBuilderNet.UI\TBWebFormControl\WindowDescriptions.cs
public:
	enum WndObjType
	{
		Undefined = 0, View = 1, Label = 2, Button = 3, Image = 8, Group = 9, Radio = 10, Check = 11, Combo = 12, Edit = 13, Toolbar = 14,
		ToolbarButton = 15, Tabber = 16, Tab = 17, BodyEdit = 18,
		ColTitle = 22, List = 24, CheckList = 25, Tree = 26, TreeNode = 27, Menu = 28, MenuItem = 29,
		ListCtrl = 30, ListCtrlItem = 31, ListCtrlDetails = 32, Spin = 33, /*Report = 34,*/ StatusBar = 35, SbPane = 36,
		/*Title = 37 non più usata, il titolo è nella proprietà text del'oggetto, il rettangolo del titolo non ci interessa più*/
		Frame = 40, Dialog = 43,
		PropertyDialog = 44, GenericWndObj = 45, TreeAdv = 51,
		ProgressBar = 63, CaptionBar = 64, RadioGroup = 65, Panel = 66, TabbedToolbar = 67,
		Constants = 69,
		//tile description type
		Tile = 71,
		//tile group type
		TileGroup = 72,
		//smaller element contained in a tile 
		TilePart = 73,
		//static section of tile part (usually contains labels)
		TilePartStatic = 74,
		//content section of tile part (usually contains input control)
		TilePartContent = 75,
		TileManager = 76,
		TilePanel = 77,
		LayoutContainer = 78,
		HeaderStrip = 79,
		PropertyGrid = 80,
		PropertyGridItem = 81,
		TreeBodyEdit = 82,
		StatusTile = 83,
		HotFilter = 84,
		StatusTilePanel = 85,
		Splitter = 86,
		DockingPane = 87
	};
	enum RuntimeState { UNDEFINED, RUNTIME, STATIC };
	enum IconTypes { M4, IMG, TB, CLASS };
	static CRuntimeClass* classMap[CLASS_MAP_SIZE];
	static EnumDescriptionAssociations singletonEnumDescription;
private:
	CWndObjDescription* m_pParent;
	WndDescriptionState m_descState = REMOVED;
	RuntimeState m_Runtime = UNDEFINED;
	DECLARE_DYNCREATE(CWndObjDescription);
public:
	CParsedCtrl* m_pAssociatedControl = NULL;
	HWND		m_hAssociatedWnd = NULL;
	CStringArray m_strIds;//contiene l'ID del controllo, che è overridabile e quindi serve un array per tenerne la storia
	CStringArray m_arHrefHierarchy;//se uso la tecnica di derivazione con href, gli id vengono sovrascritti, l'array ne tiene traccia per i client forms
	CString		m_strName;//contiene il nome del controllo, usato come token di namespace
	CString		m_strControlClass;//contiene o la runtime class, o la classe di finestra windows per creare il controllo
	ControlStyle m_ControlStyle = CS_NONE;//lo stile del parsed control
	CString		m_strCmd;
	CString		m_strContext;
	TArray<CJsonResource> m_Resources;
	WndObjType	m_Type = GenericWndObj;
	CString		m_strText;

	int			m_nChars = -1;
	int			m_nRows = 1;		
	int			m_nTextLimit = 0;
	int			m_nNumberDecimal = -1;
	CString		m_sMinValue;
	CString		m_sMaxValue;

	CString		m_strHint;
	CString		m_strBeforeId;//contiene l'IDC del controllo che deve precederlo nella lista di elementi del parent

	CString		m_strActivation;//stringa di attivazione moduli, nella forma: 	!ERP.AccountingAuditing&!ERP.SimplifiedAccounting&!ERP.Accounting_PL
	int			m_X = NULL_COORD;
	int			m_Y = NULL_COORD;
	int			m_Width = NULL_COORD;
	int			m_Height = NULL_COORD;
	int			m_MarginLeft = NULL_COORD;
	int			m_MarginTop = NULL_COORD;
	int			m_MarginBottom = NULL_COORD;
	bool		m_bEnabled = true;
	bool		m_bVisible = true;
	bool		m_bTabStop = TabStopDefault();
	bool		m_bHasTabIndex = true;
	bool		m_bUsed = false;
	bool		m_bRightAnchor = false;

	BindingInfo*m_pBindings = NULL;
	StateData*	m_pStateData = NULL;
	DWORD		m_nStyle = 0;
	DWORD		m_nExStyle = 0;
	bool		m_bAcceptFiles = false;
	//bool		m_bClientEdge = false;		gestito tramite m_bBorder, che diventa WS_EX_CLIENTEDGE o WS_BORDER a seconda della classe di finestra

	bool		m_bGroup = false;
	bool		m_bChild = true;
	bool		m_bHScroll = false;
	bool		m_bVScroll = false;
	bool		m_bBorder = GetDefaultBorder(); //false;
	bool		m_bTransparent = false;
	bool		m_bHFill = false;
	bool		m_bVFill = false;
	CString		m_sAnchor;//quando il posizionamento è dinamico e non assoluto, allinea il controllo alla destra del precedente
	CRect		m_CalculatedLURect;//ad uso e consumo dell'engine di rendering json
	CAcceleratorDescription* m_pAccelerator = NULL;
	CMenuDescription* m_pMenu = NULL;
	CFontDescription* m_pFontDescription = NULL;
	CNumbererDescription* m_pNumbererDescription = NULL;
	CWndObjDescriptionContainer m_Children;

	CPtrArray m_arUpdated;

	CFontDescription* m_pCaptionFontDescription = NULL;
	CControlBehaviourDescription* m_pControlBehaviourDescription = NULL;
	CString		m_strControlCaption;
	VerticalAlignment m_CaptionVerticalAlign = VerticalAlignment::VATOP;
	TextAlignment m_CaptionHorizontalAlign = TextAlignment::TARIGHT;
	int			m_CaptionWidth = NULL_COORD;


protected:
	CWndObjDescription();
public:
	virtual void		ActivateDefines(bool bActive);
	static	CString		GetEnumDescription(WndObjType value);
	static	void		GetEnumValue(CString description, WndObjType& retVal);

	static	CString		GetEnumDescription(VerticalAlignment value);
	static	void		GetEnumValue(CString description, VerticalAlignment& retVal);

	static	CString		GetEnumDescription(EtchedFrameType value);
	static	void		GetEnumValue(CString description, EtchedFrameType& retVal);

	static	CString		GetEnumDescription(WndDescriptionState value);
	static	void		GetEnumValue(CString description, WndDescriptionState& retVal);

	static	CString		GetEnumDescription(TextAlignment value);
	static	void		GetEnumValue(CString description, TextAlignment& retVal);

	static	CString		GetEnumDescription(ResizableControl value);
	static	void		GetEnumValue(CString description, ResizableControl& retVal);

	static	CString		GetEnumDescription(OwnerDrawType value);
	static	void		GetEnumValue(CString description, OwnerDrawType& retVal);

	static	CString		GetEnumDescription(ComboType value);
	static	void		GetEnumValue(CString description, ComboType& retVal);

	static	CString		GetEnumDescription(SelectionType value);
	static	void		GetEnumValue(CString description, SelectionType& retVal);

	static	CString		GetEnumDescription(SplitterMode value);
	static	void		GetEnumValue(CString description, SplitterMode& retVal);

	static CString		GetEnumDescription(CommandCategory value);
	static void			GetEnumValue(CString description, CommandCategory& retVal);

	static CString		GetEnumDescription(SpinCtrlAlignment value);
	static void			GetEnumValue(CString description, SpinCtrlAlignment& retVal);

	static CString		GetEnumDescription(ViewCategory value);
	static void			GetEnumValue(CString description, ViewCategory& retVal);

	static CString		GetEnumDescription(ControlStyle value);
	static void			GetEnumValue(CString description, ControlStyle& retVal);

	static bool RegisterClassMap(CRuntimeClass* pClass, WndObjType type);
	CWndObjDescription(CWndObjDescription* pParent);
	virtual ~CWndObjDescription(void);
	void WriteJsonStrings(CJsonSerializer* pResp);
	CString GetJsonID();
	virtual BOOL SkipWindow(CWnd* pWnd);
	virtual BOOL IsDummy() { return FALSE; }
	virtual void AddChildWindows(CWnd* pWnd);
	virtual CWndObjDescription* AddChildWindow(CWnd* pChild, CString strId = _T(""));
	virtual void UpdateAttributes(CWnd* pWnd);
	virtual void UpdateEnableStatus(CWnd *pWnd);
	virtual void UpdateWindowText(CWnd *pWnd);
	virtual BOOL HasTooltip() { return FALSE; }
	virtual bool TabStopDefault() { return true; }
	CWndObjDescription* Clone();
	CWndObjDescription* DeepClone();
	void SetParent(CWndObjDescription* pParent) { m_pParent = pParent; }
	CWndObjDescription* GetParent() { return m_pParent; }
	CWndObjDescription* GetRoot();
	CWndObjDescription* Find(const CString& id);
	void FindAll(const CString& id, CArray<CWndObjDescription*>&ar);
	CWndObjDescription* FindUpwards(const CString& id);
	static CString GetTempImagesPath(const CString& sFileName);
	static CString GetTempFilesPath(const CString& sFileName);
	static const CDummyDescription* GetDummyDescription() { return &g_DummyDescription; }
	static void UpdateMenuDescription(CWndObjDescription* pMenuDesc, CWndObjDescription* pFrameDesc);
	virtual void Assign(CWndObjDescription* pDesc);
	virtual void DeepAssign(CWndObjDescription* pDesc);
	void SetUpdated(void* pField);
	void SetAdded(bool deep = false);
	void SetRemoved();
	void SetDeepRemoved();
	void SetRuntimeState(RuntimeState state) { m_Runtime = state; }
	void SetParsed(bool deep = false);
	void SetUnchanged(bool deep = false);
	void SetState(WndDescriptionState descState) { m_descState = descState; }
	WndDescriptionState GetState() { return m_descState; }
	BOOL IsChanged() { return m_descState != UNCHANGED; }
	BOOL IsAdded() { return m_descState == ADDED; }
	BOOL IsRemoved() { return m_descState == REMOVED; }
	void PopulateDeltaDesc(CWndObjDescriptionContainer* pDeltaContainer);
	void SetFont(CString strFaceName, float nFontSize, bool bBold, bool bItalic, bool bUnderline);
	BOOL IsToolbarChild();
	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
	virtual void SerializeBinary(CStreamSerializer& serializer);
	static CWndObjDescription* ParseJsonObject(CJsonFormParser& parser, CWndObjDescription* pDescriptionToMerge = NULL, WndObjType expectedType = Undefined);
	static CWndObjDescription* ParseHref(CJsonFormParser& parser, const CString& sHref, CWndObjDescription* pDescriptionToMerge = NULL);
	void AttachTo(HWND hwnd);
	void GetResources(CArray<CJsonResource*>& arFiles);
	CWndObjDescription* GetRootOfFile();
	CJsonResource GetResource();
	static CWndObjDescription* GetFrom(HWND hwnd);

	//trasforma le proprietà nei corrispondenti stili per le finestre Windows
	virtual void ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle);
	//travasa gli stili Windows nelle corrispondenti proprietà
	virtual void UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle);

	virtual void ApplyBorderStyle(DWORD& dwStyle, DWORD& dwExStyle);
	virtual TbResourceType GetResourceType() { return TbControls; }

	virtual bool GetDefaultBorder() { return false; }
	virtual void EvaluateExpressions(CJsonContextObj * JsonContext, bool deep = true);
	CRect GetRect() const;
	void  SetRect(const CRect& r, BOOL bSetUpdated);
	CString GetID() const;
	void SetID(const CString& sId, bool bClearExisting = false);
	bool HasID(const CString& sId);
protected:
	CString GetID(CWnd* pWnd);
	virtual void GetWindowRect(CWnd *pWnd, CRect& rect);
private:
	bool inline IsUpdated(void* pField);
	CString GetName(CWnd* pWnd);
	CString GetControlClass(CWnd* pWnd);
	virtual void Dump(CDumpContext& dc) const;
};



//=============================================================================
//			Class CControlBehaviourDescription
//=============================================================================
class TB_EXPORT CControlBehaviourDescription : public ExpressionObject
{
public:
	CString m_strName;
	CString m_strNamespace;
	bool m_bItemSource = false;
	bool m_bDataAdapter = false;
	bool m_bValidator = false;
	bool m_bCommandHandler = false;
	CControlBehaviourDescription*  Clone();

	void SerializeJson(CJsonSerializer& strJson);
	void ParseJson(CJsonFormParser& parser);
private:
	void Assign(CControlBehaviourDescription* pDesc);
};



//-----------------------------------------------------------------------------
class TB_EXPORT CDummyDescription : public CWndObjDescription
{
	DECLARE_DYNCREATE(CDummyDescription);
	CDummyDescription() : CWndObjDescription(NULL) {}
public:
	virtual BOOL IsDummy() { return TRUE; }

};
//-----------------------------------------------------------------------------
class TB_EXPORT CFakeArrayContainerDescription : public CDummyDescription
{
	DECLARE_DYNCREATE(CFakeArrayContainerDescription);
	CFakeArrayContainerDescription() {}
public:
	void ParseJson(CJsonFormParser& parser);

};

//-----------------------------------------------------------------------------
class TB_EXPORT CWndColoredObjDescription : public CWndObjDescription
{
	DECLARE_DYNCREATE(CWndColoredObjDescription);

protected:
	CWndColoredObjDescription();

public:
	COLORREF		m_crBkgColor = EMPTY_COLOR;
	COLORREF		m_crTextColor = EMPTY_COLOR;
	TextAlignment	m_textAlign = TALEFT;

	CWndColoredObjDescription(CWndObjDescription* pParent);

	virtual void Assign(CWndObjDescription* pDesc);

	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
};

//-----------------------------------------------------------------------------
class TB_EXPORT CWndImageDescription : public CWndColoredObjDescription
{
	DECLARE_DYNCREATE(CWndImageDescription);
	bool m_bRealSize = false;
protected:
	CWndImageDescription() { m_Type = CWndObjDescription::Image; }
	~CWndImageDescription() { };
public:
	CImageBuffer m_ImageBuffer;
	CString		 m_sImgNamespace;

	CWndImageDescription(CWndObjDescription* pParent)
		: CWndColoredObjDescription(pParent)
	{
		m_Type = CWndObjDescription::Image;
	}
	virtual void Assign(CWndObjDescription* pDesc);

	virtual void SerializeJson(CJsonSerializer& strJson);
	CString GetImageJson();
	virtual void ParseJson(CJsonFormParser& parser);
	void GetImageBytes(BYTE*& buffer, int &nCount);

	virtual void ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle);
	//travasa gli stili Windows nelle corrispondenti proprietà
	virtual void UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle);

};

//-----------------------------------------------------------------------------
class TB_EXPORT CLinkDescription : public CObject
{
	DECLARE_DYNCREATE(CLinkDescription);

	int		m_nObjectAlias;
	int		m_nRow;
	CRect	m_Rect;

	CLinkDescription() :
		m_nObjectAlias(-1),
		m_nRow(0),
		m_Rect(0, 0, 0, 0)
	{}
public:
	CLinkDescription(CRect rect, int nAlias, int nRow) :
		m_nObjectAlias(nAlias),
		m_nRow(nRow),
		m_Rect(rect)
	{}
	CLinkDescription* Clone();
	void Assign(CLinkDescription* pDesc);

	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
};


//-----------------------------------------------------------------------------
class TB_EXPORT CWndButtonDescription : public CWndImageDescription
{
	DECLARE_DYNCREATE(CWndButtonDescription);

	VerticalAlignment	m_vertAlign = VATOP;


	bool			m_bMultiline = false;

	bool			m_bBitmap = false;
	bool			m_bText = false;
	bool			m_bFlat = false;
	bool			m_bIcon = false;
	bool			m_bClipChildren = false;

protected:
	CWndButtonDescription()
	{
		m_Type = CWndObjDescription::Button;
	}

public:
	CWndButtonDescription(CWndObjDescription* pParent)
		: CWndImageDescription(pParent)
	{
		m_Type = CWndObjDescription::Button;
	}

	virtual BOOL HasTooltip() { return TRUE; }
	void SerializeJson(CJsonSerializer& strJson);
	void ParseJson(CJsonFormParser& parser);
	void Assign(CWndObjDescription* pDesc);
	virtual void ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle);
	//travasa gli stili Windows nelle corrispondenti proprietà
	virtual void UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle);
	void UpdateAttributes(CWnd *pWnd);
	virtual bool IsOwnerDraw() { return false; }
};


//-----------------------------------------------------------------------------
class TB_EXPORT CPushButtonDescription : public CWndButtonDescription
{
	DECLARE_DYNCREATE(CPushButtonDescription);
	bool			m_bDefault = false;
	bool			m_bOwnerDraw = false;

protected:
	CPushButtonDescription()
	{
		m_Type = CWndObjDescription::Button;
	}
	void SerializeJson(CJsonSerializer& strJson);
	void ParseJson(CJsonFormParser& parser);
	void Assign(CWndObjDescription* pDesc);
	virtual void ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle);
	//travasa gli stili Windows nelle corrispondenti proprietà
	virtual void UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle);
	virtual bool IsOwnerDraw();
};

//-----------------------------------------------------------------------------
class TB_EXPORT CGroupBoxDescription : public CWndButtonDescription
{
	DECLARE_DYNCREATE(CGroupBoxDescription);
protected:
	CGroupBoxDescription()
	{
		m_Type = CWndObjDescription::Group;
		m_bTabStop = TabStopDefault();
	}

public:
	CGroupBoxDescription(CWndObjDescription* pParent)
		: CWndButtonDescription(pParent)
	{
		m_Type = CWndObjDescription::Group;
		m_bTabStop = TabStopDefault();
	}

	virtual void ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle);

	virtual bool TabStopDefault() { return false; }
};


//=============================================================================
//			Class CValidatorDescription
//=============================================================================
class TB_EXPORT CValidatorDescription : public ExpressionObject
{
public:
	CString m_strValidatorName;
	CString m_strValidatorNamespace;

	CValidatorDescription*  Clone();

	void SerializeJson(CJsonSerializer& strJson);
	void ParseJson(CJsonFormParser& parser);
private:
	void Assign(CValidatorDescription* pDesc);
};

//=============================================================================
//			Class CDataAdapterDescription
//=============================================================================
class TB_EXPORT CDataAdapterDescription : public ExpressionObject
{
public:
	CString m_strDataAdapterName;
	CString m_strDataAdapterNamespace;

	CDataAdapterDescription*  Clone();

	void SerializeJson(CJsonSerializer& strJson);
	void ParseJson(CJsonFormParser& parser);
private:
	void Assign(CDataAdapterDescription* pDesc);
};



//-----------------------------------------------------------------------------
class TB_EXPORT CTextObjDescription : public CWndColoredObjDescription
{
	DECLARE_DYNCREATE(CTextObjDescription);

protected:
	CTextObjDescription();

public:
	bool		m_bMultiline = false;
	CArray <CValidatorDescription*, CValidatorDescription*> m_arValidators;
	CDataAdapterDescription* m_pDataAdapter = NULL;
	Formatter*	m_pFormatter = NULL;
	CTextObjDescription(CWndObjDescription* pParent);
	~CTextObjDescription();

	virtual void UpdateAttributes(CWnd *pWnd);
	virtual void UpdateEnableStatus(CWnd *pWnd);
	virtual void Assign(CWndObjDescription* pDesc);

	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
};
//per il controllo EDIT
//-----------------------------------------------------------------------------
class TB_EXPORT CEditObjDescription : public CTextObjDescription
{
	DECLARE_DYNCREATE(CEditObjDescription);

	//Styles EDIT CONTROL
	bool			m_bAutoHScroll = false;
	bool			m_bAutoVScroll = false;
	bool			m_bNoHideSelection = false;
	bool			m_bNumber = false;
	bool			m_bPassword = false;
	bool			m_bReadOnly = false;
	bool			m_bUpperCase = false;
	bool			m_bLowerCase = false;
	bool			m_bWantReturn = false;
	ResizableControl	m_Resizable = ResizableControl::R_NONE;

public:
	virtual void UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle);
	virtual void ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle);
	CEditObjDescription();
	CEditObjDescription(CWndObjDescription* pParent);
	void SerializeJson(CJsonSerializer& strJson);
	void ParseJson(CJsonFormParser& parser);
	void Assign(CWndObjDescription* pDesc);

	virtual bool GetDefaultBorder() { return true; }
};


//=============================================================================
//			Class CItemSourceDescription
//=============================================================================
class TB_EXPORT CItemSourceDescription : public CWndObjDescription
{
public:
	CString m_strItemSourceName;
	CString m_strItemSourceNamespace;
	CString m_strItemSourceParameter;
	bool	m_bItemSourceUseProductLanguage = false;
	bool	m_bAllowChanges = false;
	CItemSourceDescription*  Clone();

	void SerializeJson(CJsonSerializer& strJson);
	void ParseJson(CJsonFormParser& parser);
private:
	void Assign(CItemSourceDescription* pDesc);
};



//per il controllo STATIC
//-----------------------------------------------------------------------------
class TB_EXPORT CLabelDescription : public CTextObjDescription
{
	DECLARE_DYNCREATE(CLabelDescription);

	bool m_bBitmap = false;
	bool m_bBlackFrame = false;
	bool m_bBlackRect = false;
	bool m_bCenterImage = false;
	bool m_bEditControl = false;
	bool m_bEndEllipsis = false;
	bool m_bGrayFrame = false;
	bool m_bGrayRect = false;
	bool m_bIcon = false;
	bool m_bLeftNoWrap = false;
	bool m_bNoPrefix = false;
	bool m_bOwnerDraw = false;
	bool m_bPathEllipsis = false;
	bool m_bSimple = false;
	bool m_bSunken = false;
	bool m_bWhiteFrame = false;
	bool m_bWhiteRect = false;
	bool m_bWordEllipsis = false;
	bool m_bNotify = false;
	EtchedFrameType		m_EtchedFrame = EFNO;
	VerticalAlignment	m_vertAlign = VATOP;

public:

	CLabelDescription();
	CLabelDescription(CWndObjDescription* pParent);
	virtual void UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle);
	virtual void ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle);
	virtual bool TabStopDefault() { return false; }
	void SerializeJson(CJsonSerializer& strJson);
	void ParseJson(CJsonFormParser& parser);
	void Assign(CWndObjDescription* pDesc);
};


//-----------------------------------------------------------------------------
class TB_EXPORT CComboDescription : public CTextObjDescription
{
	DECLARE_DYNCREATE(CComboDescription);

public:

	ComboType				m_nComboType = DROPDOWN;
	bool					m_bAuto = false;
	bool					m_bNoIntegralHeight = false;
	bool					m_bOemConvert = false;
	bool					m_bSort = false;
	bool					m_bUpperCase = false;
	OwnerDrawType			m_nComboOwnerDrawType = NO;
	CItemSourceDescription* m_pItemSourceDescri = NULL;

	CComboDescription()
		: CTextObjDescription()
	{
		m_bVScroll = true;
		m_Type = CWndObjDescription::Combo;
	}

	CComboDescription(CWndObjDescription* pParent)
		: CTextObjDescription(pParent)
	{
		m_bVScroll = true;
		m_Type = CWndObjDescription::Combo;
	}
	~CComboDescription() {
		delete m_pItemSourceDescri;
	}

	virtual void Assign(CWndObjDescription* pDesc);

	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
	virtual void UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle);
	virtual void ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle);
	virtual void GetWindowRect(CWnd *pWnd, CRect& rect);
	virtual void EvaluateExpressions(CJsonContextObj * JsonContext, bool deep = true);
};


//-----------------------------------------------------------------------------
class TB_EXPORT CTabbedToolbarDescription : public CWndObjDescription
{
	UINT m_iActiveTabIndex = 0;
	DECLARE_DYNCREATE(CTabbedToolbarDescription);

public:
	CTabbedToolbarDescription()
	{
		m_Type = CWndObjDescription::TabbedToolbar;
		m_Children.SetSortable(FALSE);
	}
	CTabbedToolbarDescription(CWndObjDescription* pParent)
		:
		CWndObjDescription(pParent)
	{
		m_Type = CWndObjDescription::TabbedToolbar;
		m_Children.SetSortable(FALSE);
	}
	void SetActiveTabIndex(UINT iActiveIndex);
	UINT GetActiveTabIndex() { return m_iActiveTabIndex; }
	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void Assign(CWndObjDescription* pDesc);
};

//-----------------------------------------------------------------------------
class TB_EXPORT CWndCheckRadioDescription : public CWndButtonDescription
{
	DECLARE_DYNCREATE(CWndCheckRadioDescription);
private:
	CWndCheckRadioDescription()
	{
	}

public:
	bool m_bChecked = false;
	bool m_bAutomatic = true;//per check
	bool m_bThreeState = false;//per check;
	bool m_bLabelOnLeft = false;
	CString m_strGroupName;

	CWndCheckRadioDescription(CWndObjDescription* pParent)
		: CWndButtonDescription(pParent)
	{ }
	virtual void UpdateAttributes(CWnd* pWnd);
	virtual void Assign(CWndObjDescription* pDesc);

	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
	virtual void UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle);
	virtual void ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle);
};

//-----------------------------------------------------------------------------
class TB_EXPORT CWndRadioGroupDescription : public CWndColoredObjDescription
{
	DECLARE_DYNCREATE(CWndRadioGroupDescription);
private:
	CWndRadioGroupDescription() { m_X = NULL_COORD; m_Y = NULL_COORD; m_Width = NULL_COORD; m_Height = NULL_COORD; }

public:
	CString m_strGroupName;

	CWndRadioGroupDescription(CWndObjDescription* pParent)
		: CWndColoredObjDescription(pParent) { }
	/*
	virtual void UpdateAttributes(CWnd* pWnd);
	virtual void Assign(CWndObjDescription* pDesc);

	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);*/

};

//-------------------------------------------------------------------------
class TB_EXPORT CItemDescription : public CObject, public ExpressionObject
{
	DECLARE_DYNAMIC(CItemDescription);
public:
	CItemDescription(void) {}
	CItemDescription* Clone();

	virtual void Assign(CItemDescription* pDesc);
	virtual BOOL UpdateItemAttributes(CWnd *pWnd, int i) = 0;

	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
};

//-------------------------------------------------------------------------
class TB_EXPORT CItemListBoxDescription : public CItemDescription
{
	DECLARE_DYNCREATE(CItemListBoxDescription);

	CString m_strText;
	bool	m_bSelected = false;

public:
	CItemListBoxDescription(void) {}

	virtual BOOL UpdateItemAttributes(CWnd *pWnd, int i);
	virtual void Assign(CItemDescription* pDesc);

	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
};

//-------------------------------------------------------------------------
class TB_EXPORT CItemCheckListDescription : public CItemListBoxDescription
{
	DECLARE_DYNCREATE(CItemCheckListDescription);

	bool	m_bDisabled = false;
	bool	m_bChecked = false;

public:
	CItemCheckListDescription(void) {}

	virtual BOOL UpdateItemAttributes(CWnd *pWnd, int i);
	virtual void Assign(CItemDescription* pDesc);

	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
};




//-------------------------------------------------------------------------
class TB_EXPORT CListDescription : public CWndObjDescription
{
	DECLARE_DYNCREATE(CListDescription);

	CRuntimeClass*	m_pListItemRTC = NULL;
	CArray <CItemDescription*, CItemDescription*> m_arItems;

	//gestione stili
	bool			m_bDisableNoScroll = false;
	bool			m_bHasStrings = false;
	bool			m_bNoIntegralHeight = false;
	bool			m_bSort = false;
	bool			m_bWantKeyInput = false;
	bool			m_bMultiColumn = false;
	bool			m_bNotify = false;
	SelectionType	m_nSelection = SINGLE;
	OwnerDrawType	m_nOwnerDraw = NO;

	CItemSourceDescription* m_pItemSourceDescri = NULL;
private:
	CListDescription()
	{
		//m_bVScroll = true;
		m_bBorder = GetDefaultBorder(); //true;
	};

public:
	CListDescription(CWndObjDescription* pParent)
		:CWndObjDescription(pParent)
	{
		//m_bVScroll = true;
		m_bBorder = GetDefaultBorder(); //true;
	};

	~CListDescription();
	void SetItemClass(CRuntimeClass* pClass);
	virtual void UpdateAttributes(CWnd *pWnd);
	virtual void Assign(CWndObjDescription* pDesc);

	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
	virtual void UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle);
	virtual void ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle);

	virtual bool GetDefaultBorder() { return true; }
	virtual void EvaluateExpressions(CJsonContextObj * JsonContext, bool deep = true);
};





class CMenuItemDescription;
//=============================================================================
//			Class CMenuDescription
//=============================================================================
class TB_EXPORT CMenuDescription : public CWndObjDescription
{
	DECLARE_DYNCREATE(CMenuDescription);
protected:
	CString m_sOwnerId;

private:
	CMenuDescription() {};
public:
	CMenuDescription(CWndObjDescription* pParent)
		:CWndObjDescription(pParent)
	{
	}
	virtual void UpdateAttributes(CMenu* pMenu, HWND hWnd);
	virtual void Assign(CWndObjDescription* pDesc);

	void SetOwnerID(CString sOwnerId) { m_sOwnerId = sOwnerId; }
	CString GetOwnerID() { return m_sOwnerId; }

};


//=============================================================================
//			Class CMenuItemDescription
//=============================================================================
class TB_EXPORT CMenuItemDescription : public CWndObjDescription
{
	DECLARE_DYNCREATE(CMenuItemDescription);
public:
	bool				m_bChecked = false;
	bool				m_bIsSeparator = false;
	CString				m_strIcon;
	IconTypes			m_IconType = IconTypes::IMG;

	CMenuDescription*	m_pSubMenu = NULL;
private:
	CMenuItemDescription() { m_Type = CWndObjDescription::MenuItem; };

public:
	CMenuItemDescription(CWndObjDescription* pParent) { m_Type = CWndObjDescription::MenuItem; }
	~CMenuItemDescription() { SAFE_DELETE(m_pSubMenu); }

	static	CString		GetEnumDescription(IconTypes value);
	static	void		GetEnumValue(CString description, IconTypes& retVal);

	virtual void Assign(CWndObjDescription* pDesc);

	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
};


//==============================================================================
class TB_EXPORT CListCtrlItemDescription : public CWndObjDescription
{
	DECLARE_DYNCREATE(CListCtrlItemDescription);

	int m_nIdxItem = -1;
	bool m_bSelected = false;
	CArray <CWndObjDescription*, CWndObjDescription*> m_arCells;

	CListCtrlItemDescription() {}

public:
	CListCtrlItemDescription(CWndObjDescription* pParent)
		: CWndObjDescription(pParent)
	{}
	~CListCtrlItemDescription();

	virtual BOOL UpdateItemDescription(CListCtrl* pListCtrl, int index);
	virtual void Assign(CWndObjDescription* pDesc);

	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
};



//==============================================================================
class TB_EXPORT CListCtrlDescription : public CWndImageDescription
{
	DECLARE_DYNCREATE(CListCtrlDescription);

	int				m_nIconHeight = 0;
	//CString			m_strViewMode;
	CStringArray	m_arColumnHeaderText;
	CArray<CListCtrlItemDescription*, CListCtrlItemDescription*> m_arItems;

	//gestione stili
	ListCtrlAlign		m_nAlignment = LC_LEFT;
	ListCtrlViewMode	m_nView = LC_ICON;
	bool				m_bAutoArrange = false;
	bool				m_bNoColumnHeader = false;
	bool				m_bNoSortHeader = false;
	bool				m_bAlwaysShowSelection = false;
	bool				m_bSingleSelection = false;

	CListCtrlDescription()
	{
		//m_bVScroll = true;
		m_bBorder = GetDefaultBorder(); //true;
	}

public:

	CListCtrlDescription(CWndObjDescription* pParent)
		: CWndImageDescription(pParent)
	{
		//m_bVScroll = true;
		m_bBorder = GetDefaultBorder(); //true;
	}

	~CListCtrlDescription();

	virtual void UpdateAttributes(CWnd* pWnd);
	virtual void Assign(CWndObjDescription* pDesc);

	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);

	virtual void UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle);
	virtual void ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle);

	static CString		GetEnumDescription(ListCtrlAlign value);
	static void			GetEnumValue(CString description, ListCtrlAlign& retVal);

	static CString		GetEnumDescription(ListCtrlViewMode value);
	static void			GetEnumValue(CString description, ListCtrlViewMode& retVal);

	virtual bool GetDefaultBorder() { return true; }
};


//=============================================================================
//			Class CWndStatusBarDescription
//=============================================================================
// Derivo una classe specifica per la descrizione di una CStatusBar, per reimplementare il metodo 
// virtuale AddChildWindows. Questo perche' i panelli indicatori della CStatusBar non sono finestre figlie della statusbar
// devo quindi aggiungerli ai figli di questa descrizione esplicitamente
class TB_EXPORT CWndStatusBarDescription : public CWndObjDescription
{
	DECLARE_DYNCREATE(CWndStatusBarDescription);

private:
	CWndStatusBarDescription() {};
public:
	CWndStatusBarDescription(CWndObjDescription* pParent)
		:CWndObjDescription(pParent)
	{
	}

	virtual void AddChildWindows(CWnd* pWnd);
};

//=============================================================================
// Derivo una classe specifica per la descrizione di una CProgressBar
class TB_EXPORT CWndProgressBarDescription : public CWndObjDescription
{
	DECLARE_DYNCREATE(CWndProgressBarDescription);
public:
	int m_nLower;
	int m_nUpper;
	int m_nPos;

private:
	CWndProgressBarDescription() {};
public:
	CWndProgressBarDescription(CWndObjDescription* pParent)
		: CWndObjDescription(pParent)
	{
	}

	virtual void UpdateAttributes(CWnd* pWnd);
	virtual void Assign(CWndObjDescription* pDesc);
};


//=============================================================================
//			Class CWndPanelDescription
// questa è una classe generica che rappresenta un contenitore di finestre, e può essere
// usata come view MFC, Dialog child o popup
//=============================================================================
class TB_EXPORT CWndPanelDescription : public CWndImageDescription
{
	DECLARE_DYNCREATE(CWndPanelDescription);
	bool m_bModalFrame = false;
	bool m_bCenter = false;
	bool m_bCenterMouse = false;
	bool m_bUserControl = false;
	bool m_bCaption = false;
	bool m_bSystemMenu = false;
	bool m_bClipChildren = false;
	bool m_bClipSiblings = false;
	bool m_bDialogFrame = false;
	bool m_bMaximizeBox = false;
	bool m_bMinimizeBox = false;
	bool m_bOverlapped = false;
	bool m_bThickFrame = false;
protected:
	CWndPanelDescription()
	{
		m_Type = CWndObjDescription::Panel;
		m_bTabStop = TabStopDefault();
	}

public:
	CWndPanelDescription(CWndObjDescription* pParent)
		: CWndImageDescription(pParent)
	{
		m_Type = CWndObjDescription::Panel;
		m_bTabStop = TabStopDefault();
	}
	virtual void Assign(CWndObjDescription* pDesc);
	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
	virtual bool TabStopDefault() { return false; }
	void UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle);
	void ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle);
	void ApplyBorderStyle(DWORD& dwStyle, DWORD& dwExStyle);

protected:
	virtual TbResourceType GetResourceType();
};

//-----------------------------------------------------------------------------
class TB_EXPORT CWndFrameDescription : public CWndPanelDescription
{
	DECLARE_DYNCREATE(CWndFrameDescription);
	bool m_bWizard = false;
	bool m_bStepper = false;
	bool m_bDockable = true;
	bool m_bStatusBar = true;

	ViewCategory m_ViewCategory = VIEW_DATA_ENTRY;

protected:
	CWndFrameDescription()
	{
		m_Type = CWndObjDescription::Frame;
		m_bSystemMenu = true;
	}

public:
	CWndFrameDescription(CWndObjDescription* pParent)
		: CWndPanelDescription(pParent)
	{
		m_Type = CWndObjDescription::Frame;
		m_bSystemMenu = true;
	}

	ViewCategory GetViewCategory() {
		return m_ViewCategory;
	}

	virtual void Assign(CWndObjDescription* pDesc);
	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
};


//-----------------------------------------------------------------------------
class TB_EXPORT CTabberDescription : public CWndObjDescription
{
	DECLARE_DYNCREATE(CTabberDescription);
public:
	Bool3 m_bIsVertical = B_UNDEFINED;
	bool m_bWizard = false;
	int	 m_nIconWidth = 0;
	int	 m_nIconHeight = 0;
	CTabberDescription()
	{
		m_Type = CWndObjDescription::Tabber;
		m_Children.SetSortable(FALSE);
	}

	CTabberDescription(CWndObjDescription* pParent)
		:CWndObjDescription(pParent)
	{
		m_Type = CWndObjDescription::Tabber;
		m_Children.SetSortable(FALSE);
	}

	virtual void Assign(CWndObjDescription* pDesc);
	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
};

//-----------------------------------------------------------------------------
class TB_EXPORT CTabDescription : public CWndPanelDescription
{
	DECLARE_DYNCREATE(CTabDescription);

private:
	CTabDescription()
	{
		m_Type = CWndObjDescription::Tab;
	}
public:
	bool	m_bActive = false;
	bool	m_bProtected = false;	//indica se la tab e' protetta via security
	CString m_strIconSource;

	CTabDescription(CWndObjDescription* pParent)
		:CWndPanelDescription(pParent)
	{
		m_Type = CWndObjDescription::Tab;
	}
	virtual void Assign(CWndObjDescription* pDesc);

	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
};
//=============================================================================
//			Class CWndDialogDescription
//=============================================================================
class TB_EXPORT CWndDialogDescription : public CWndPanelDescription
{
	DECLARE_DYNCREATE(CWndDialogDescription);
private:
	CWndDialogDescription()
	{
		m_Type = CWndObjDescription::Dialog;
		m_bChild = false;
		m_bSystemMenu = true;
	}

public:
	bool m_bIsModal = false;

	CWndDialogDescription(CWndObjDescription* pParent)
		: CWndPanelDescription(pParent)
	{
		m_Type = CWndObjDescription::Dialog;
		m_bChild = false;
		m_bSystemMenu = true;
	}

	virtual void Assign(CWndObjDescription* pDesc);
	virtual void UpdateAttributes(CWnd *pWnd);

	void ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle);
};





//=============================================================================
//			Class CToolbarDescription
//=============================================================================
class TB_EXPORT CToolbarDescription : public CWndButtonDescription
{
	DECLARE_DYNCREATE(CToolbarDescription);
private:
	CToolbarDescription();
public:
	int m_nImageHeight = 0;
	bool m_bBottom = false;
	bool m_bAutoHide = true;

	CToolbarDescription(CWndObjDescription* pParent);
	~CToolbarDescription();

	void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
	virtual BOOL HasTooltip() { return FALSE; }
	virtual void UpdateWindowText(CWnd *pWnd);

protected:
	virtual void Assign(CWndObjDescription* pDesc);
};

//=============================================================================
//			Class CToolbarBtnDescription
//=============================================================================
class TB_EXPORT CToolbarBtnDescription : public CWndObjDescription
{
	DECLARE_DYNCREATE(CToolbarBtnDescription);

	CMyMemFile* m_pImageBytes = NULL;
	CImageBuffer m_ImageBuffer;	//Used by Easylook, not used by WebLook

private:
	CToolbarBtnDescription();
	//bool m_bIsGhost = false;
	bool m_bHasMenu = false;
public:
	bool m_bDefault = false;
	//int m_iImage = -1;
	bool m_bIsDropdown = false;
	//bool m_bIsCheckButton = false;
	//bool m_bIsPressed = false;
	bool m_bIsSeparator = false;
	bool m_bRight = false;
	//bool m_bIsTabbedToolbarButton = false; //Used by Easylook, not used by WebLook

	CString m_sIcon;
	IconTypes			m_IconType = IconTypes::IMG;

	//CommandCategory m_Category = CommandCategory::CTG_UNDEFINED;

public:
	CToolbarBtnDescription(CWndObjDescription* pParent);
	~CToolbarBtnDescription();

	static	CString		GetEnumDescription(IconTypes value);
	static	void		GetEnumValue(CString description, IconTypes& retVal);

	void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
	virtual void Assign(CWndObjDescription* pDesc);
	virtual void UpdateAttributes(CWnd* pWnd);
	virtual BOOL HasTooltip() { return TRUE; }
	void GetImageBytes(BYTE*& buffer, int &nCount);
	virtual TbResourceType GetResourceType() { return TbCommands; }
	/*	void SetIsGhost(bool bIsGhost)
		{
			if (m_bIsGhost != bIsGhost)
			{
				m_bIsGhost = bIsGhost;
				SetUpdated(&m_bIsGhost);
			}
		}

		bool GetIsGhost(){ return m_bIsGhost; }

		bool GetHasMenu(){ return m_bHasMenu; }

		void SetHasMenu(bool bHasMenu)
		{
			if (m_bHasMenu != bHasMenu)
			{
				m_bHasMenu = bHasMenu;
				SetUpdated(&m_bHasMenu);
			}
		}*/
};

//=============================================================================
//			Class CCaptionBarDescription
//=============================================================================
class TB_EXPORT CCaptionBarDescription : public CWndButtonDescription
{
	DECLARE_DYNCREATE(CCaptionBarDescription);

	friend class CTaskBuilderCaptionBar;

private:
	CCaptionBarDescription();
	int	 m_nTextAlign;
	bool m_bHasButton = false;
	int m_nButtonAlign;
	int m_nImageAlign;

public:
	CCaptionBarDescription(CWndObjDescription* pParent);
	~CCaptionBarDescription();

	void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
	virtual BOOL HasTooltip() { return FALSE; }

protected:
	virtual void Assign(CWndObjDescription* pDesc);
};



//=============================================================================
//			Class CSpinCtrlDescription
//=============================================================================
class TB_EXPORT CSpinCtrlDescription : public CWndObjDescription
{
	DECLARE_DYNCREATE(CSpinCtrlDescription);

	//gestione stili
	SpinCtrlAlignment	m_nAlignment = UNATTACHED;
	bool				m_bArrowKeys = true;
	bool				m_bAutoBuddy = false;
	bool				m_bNoThousands = false;
	bool				m_bSetBuddyInteger = false;

	CSpinCtrlDescription()
	{
	}

public:

	CSpinCtrlDescription(CWndObjDescription* pParent)
		: CWndObjDescription(pParent)
	{
	}

	~CSpinCtrlDescription() {};

	virtual void Assign(CWndObjDescription* pDesc);

	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);

	virtual void ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle);
	void UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle);
};

//=============================================================================
//			Class CSplitterDescription
//=============================================================================
class TB_EXPORT CSplitterDescription : public CWndObjDescription
{
	DECLARE_DYNCREATE(CSplitterDescription);
private:
	CSplitterDescription()
	{
		m_Type = CWndObjDescription::Splitter;
	}

public:
	float				m_fSplitRatio = -1;
	bool				m_bPanesSwapped = false;
	int					m_nSplitResolution = 1;
	SplitterMode		m_SplitterMode = S_HORIZONTAL;
	int					m_nRows = -1;
	int					m_nCols = -1;

	CSplitterDescription(CWndObjDescription* pParent)
		: CWndObjDescription(pParent)
	{
		m_Type = CWndObjDescription::Dialog;
	}


	virtual void Assign(CWndObjDescription* pDesc);

	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
};


//=============================================================================
//			Class EnumDescriptionAssociations
//=============================================================================
// Singleton containing all the enum-description associations
class EnumDescriptionAssociations
{
public:
	EnumDescriptionCollection<CWndObjDescription::WndObjType>	m_arWndObjType;
	EnumDescriptionCollection<CommandCategory>					m_arCommandCategory;
	EnumDescriptionCollection<EtchedFrameType>					m_arEtchedFrameType;
	EnumDescriptionCollection<VerticalAlignment>				m_arVerticalAlignment;
	EnumDescriptionCollection<WndDescriptionState>				m_arWndDescriptionState;
	EnumDescriptionCollection<TextAlignment>					m_arTextAlignment;
	EnumDescriptionCollection<ResizableControl>					m_arResizableControl;
	EnumDescriptionCollection<OwnerDrawType>					m_arOwnerDrawType;
	EnumDescriptionCollection<ComboType>						m_arComboType;
	EnumDescriptionCollection<SelectionType>					m_arSelectionType;
	EnumDescriptionCollection<SplitterMode>						m_arSplitterMode;
	EnumDescriptionCollection<SpinCtrlAlignment>				m_arSpinCtrlAlignment;
	EnumDescriptionCollection<ViewCategory>						m_arViewCategory;
	EnumDescriptionCollection<ListCtrlViewMode>					m_arListCtrlViewMode;
	EnumDescriptionCollection<ListCtrlAlign>					m_arListCtrlAlign;
	EnumDescriptionCollection<CWndObjDescription::IconTypes>	m_arIconTypes;
	EnumDescriptionCollection<ControlStyle>						m_arControlStyles;


public:
	EnumDescriptionAssociations()
	{
		InitEnumDescriptionStructures();
	}

	void InitEnumDescriptionStructures();
};
#include "endh.dex"

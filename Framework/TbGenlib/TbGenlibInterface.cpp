
#include "stdafx.h" 

// library declaration
#include "messages.h"
#include "baseapp.h"
#include "interfacemacros.h"
#include "TbCommandInterface.h"
#include "parsbtn.h"
#include "parsedt.h"
#include "parscbx.h"
#include "parslbx.h"
#include "parsedtOther.h"
#include "TBToolBar.h"
#include "parsobjmanaged.h"
#include "AddressEdit.h"
#include "TBPropertyGrid.h"
#include "StretchableCtrl.h"
#include "TBLinearGauge.h"
#include <TbGes\HeaderStrip.h>
#include <TbGes\ItemsListTools.h>

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
//				INIZIO definizione della interfaccia di Add-On
/////////////////////////////////////////////////////////////////////////////
//
#define _AddOn_Interface_Of tbgenlibtbgenlib

//-----------------------------------------------------------------------------
BEGIN_ADDON_INTERFACE()	

	BEGIN_REGISTER_CONTROLS()
// questa sezione di codice non deve essere parsata dal TBLocalizer, si tratta di ogggetti programmativi, poi decideremo se tradurli
		// family declaration
		DECLARE_GENERIC_CONTROLS_FAMILY(szMLabel,	_T("Label"), DataType::Void)
		DECLARE_CONTROLS_FAMILY(szMParsedEdit,		_T("Edit"), DataType::String)
		DECLARE_CONTROLS_FAMILY(szMParsedStatic,	_T("Static"), DataType::String)
		DECLARE_CONTROLS_FAMILY(szMParsedCombo,		_T("Combo Box"), DataType::String)
		DECLARE_CONTROLS_FAMILY(szMCheckBox,		_T("Check Box"), DataType::Bool)
		DECLARE_CONTROLS_FAMILY(szMRadioButton,		_T("Radio Button"), DataType::Bool)
		DECLARE_CONTROLS_FAMILY(szMPushButton,		_T("Button"), DataType::Bool)
		DECLARE_CONTROLS_FAMILY(szMParsedListBox,	_T("List Box"), DataType::String)
		DECLARE_GENERIC_CONTROLS_FAMILY(szMGroupBox,_T("Generic Group Box"), DataType::Void)
		DECLARE_CONTROLS_FAMILY(szMTreeView,		_T("TreeView"), DataType::Null)
		DECLARE_CONTROLS_FAMILY(szMPropertyGrid,	_T("PropertyGrid"), DataType::Null)
		DECLARE_CONTROLS_FAMILY(szMHeaderStrip,		_T("HeaderStrip"), DataType::Null)
		DECLARE_CONTROLS_FAMILY(szMParsedGroupBox,  _T("Group Box"), DataType::Enum)

		BEGIN_CONTROLS_TYPE(DataType::String)
			REGISTER_EDIT_CONTROL(StringEdit, _T("String Edit"), CStrEdit, TRUE)
			REGISTER_EDIT_CONTROL(StretchableStrEdit, _T("Stretchable String Edit"), CStretchableStrEdit, TRUE)
			REGISTER_EDIT_CONTROL(ItemListEdit, _T("Item List Edit"), CItemsListEdit, FALSE)
			REGISTER_EDIT_CONTROL(AddressEdit, _T("Address Edit"), CAddressEdit, FALSE)
			REGISTER_EDIT_CONTROL(PathEdit, _T("Path Edit"), CPathEdit, FALSE)
			REGISTER_EDIT_CONTROL(BrowsePathEdit, _T("Browse Path Edit"), CBrowsePathEdit, FALSE)
			REGISTER_EDIT_CONTROL(NamespaceEdit, _T("Namespace Edit"), CNamespaceEdit, FALSE)
			REGISTER_EDIT_CONTROL(LinkEdit, _T("Link Edit"), CLinkEdit, FALSE)
			REGISTER_EDIT_CONTROL(PhoneEdit, _T("Phone Edit"), CPhoneEdit, FALSE)
			REGISTER_EDIT_CONTROL(EmailAddressEdit, _T("EMail Address Edit"), CEmailAddressEdit, FALSE)
			//REGISTER_EDIT_CONTROL(ReportNameSpaceEdit, _T("Report NameSpace Edit"), CReportNamespaceEdit, FALSE)
			//REGISTER_EDIT_CONTROL(ImageNameSpaceEdit, _T("Image NameSpace Edit"), CImageNamespaceEdit, FALSE)
			//REGISTER_EDIT_CONTROL(TextNameSpaceEdit, _T("Text NameSpace Edit"), CTextNamespaceEdit, FALSE)
			REGISTER_EDIT_CONTROL(IdentifierEdit, _T("Identifier Edit"), CIdentifierEdit, FALSE);
			REGISTER_STATIC_CONTROL(FileTextStatic, _T("File Text Static"), CShowFileTextStatic, FALSE);
			REGISTER_STATIC_CONTROL(StringStatic, _T("String Static"), CStrStatic, TRUE)
			REGISTER_STATIC_CONTROL(LabelStatic, _T("Label Static"), CLabelStatic, FALSE)
			REGISTER_STATIC_CONTROL(PictureStatic, _T("Picture Static"), CPictureStatic, FALSE)
			REGISTER_STATIC_CONTROL(NamespaceBitmap, _T("Namespace Bitmap"), CNSBitmap, FALSE)
			REGISTER_PUSHBN_CONTROL(WebControl, _T("Web Control"), CParsedWebCtrl, FALSE);
			REGISTER_COMBODROPDOWN_CONTROL(IdentifierCombo, _T("Identifier Combo"), CIdentifierCombo, FALSE);
			REGISTER_COMBODROPDOWN_CONTROL(XmlCombo, _T("Xml Combo"), CXmlCombo, FALSE);
			REGISTER_COMBODROPDOWN_CONTROL(StringComboDropDown, _T("Editable String Combo"), CStrCombo, TRUE)
			REGISTER_COMBODROPDOWNLIST_CONTROL(StringComboDropDownList, _T("Not Editable String Combo"), CStrCombo, FALSE)
			REGISTER_LISTBOX_CONTROL(StringListBox, _T("String Listbox"), CStrListBox, TRUE)
			REGISTER_CONTROL(szMParsedEdit, MultilineStringEdit, _T("String Edit MultiLine"), CStrEdit, ES_MULTILINE | ES_WANTRETURN | WS_VSCROLL | ES_AUTOVSCROLL, 0, 0, 0, FALSE)
			REGISTER_COMBODROPDOWNLIST_CONTROL(RadioCombo, _T("Radio Combo"), CRadioCombo, FALSE)
		END_CONTROLS_TYPE()
		
		BEGIN_CONTROLS_TYPE(DataType::Integer)
			REGISTER_EDIT_CONTROL(IntegerEdit, _T("Integer Edit"), CIntEdit, TRUE)
			REGISTER_STATIC_CONTROL(IntegerStatic, _T("Integer Static"), CIntStatic, FALSE)
			REGISTER_STATIC_CONTROL(IntegerBitmap, _T("Integer Bitmap"), CIntBitmap, FALSE)
			REGISTER_COMBODROPDOWN_CONTROL(IntegerComboDropDown, _T("Editable Integer Combo"), CIntCombo, FALSE)
			REGISTER_COMBODROPDOWNLIST_CONTROL(IntegerComboDropDownList, _T("Not Editable Integer Combo"), CIntCombo, FALSE)
			REGISTER_COMBODROPDOWNLIST_CONTROL(IntMonthNameCombo, _T("Not Editable Month Name Combo"), CIntMonthNameCombo, FALSE)
			REGISTER_COMBODROPDOWNLIST_CONTROL(IntWeekDaysCombo, _T("Not Editable WeekDay Combo"), CIntWeekDaysCombo, FALSE)
			REGISTER_LISTBOX_CONTROL(IntListBox, _T("Integer Listbox"), CIntListBox, FALSE)
			REGISTER_STATIC_CONTROL(ParsedStateImage,_T("Parsed State Image"), CParsedStateImage, FALSE)
		END_CONTROLS_TYPE()

		BEGIN_CONTROLS_TYPE(DataType::Long)
			REGISTER_EDIT_CONTROL(LongEdit, _T("Long Edit"), CLongEdit, TRUE)
			REGISTER_EDIT_CONTROL(ColorEdit, _T("Color Edit"), CColorEdit, FALSE)
			REGISTER_COMBODROPDOWN_CONTROL(LongComboDropDown, _T("Editable Long Combo"), CLongCombo, FALSE)
			REGISTER_COMBODROPDOWNLIST_CONTROL(LongComboDropDownList, _T("Not Editable Long Combo"), CLongCombo, FALSE)
			REGISTER_STATIC_CONTROL(LongStatic, _T("Long Static"), CLongStatic, FALSE)
			REGISTER_LISTBOX_CONTROL(LongListBox, _T("Long ListBox"), CLongListBox, FALSE)
		END_CONTROLS_TYPE()

		BEGIN_CONTROLS_TYPE(DataType::Date)
			REGISTER_EDIT_CONTROL(DateEdit, _T("Date Edit"), CDateEdit, TRUE)
			REGISTER_EDIT_CONTROL(DateSpinEdit, _T("Spin Date Edit"), CDateSpinEdit, FALSE)
			REGISTER_COMBODROPDOWN_CONTROL(DateComboDropDown, _T("Editable Date Combo"), CDateCombo, FALSE)
			REGISTER_COMBODROPDOWNLIST_CONTROL(DateComboDropDownList, _T("Not Editable Date Combo"), CDateCombo, FALSE)
			REGISTER_STATIC_CONTROL(DateStatic, _T("Date Static"), CDateStatic, FALSE)
		END_CONTROLS_TYPE()

		BEGIN_CONTROLS_TYPE(DataType::ElapsedTime)
			REGISTER_EDIT_CONTROL(ElapsedTimeEdit, _T("Elapsed Time Edit"), CElapsedTimeEdit, TRUE)
			REGISTER_STATIC_CONTROL(ElapsedTimeStatic, _T("Elapsed Time Static"), CElapsedTimeStatic, FALSE)
		END_CONTROLS_TYPE()

		BEGIN_CONTROLS_TYPE(DataType::Double)
			REGISTER_EDIT_CONTROL(DoubleEdit, _T("Double Edit"), CDoubleEdit, TRUE)
			REGISTER_COMBODROPDOWN_CONTROL(DoubleComboDropDown, _T("Editable Double Combo"), CDoubleCombo, FALSE)
			REGISTER_COMBODROPDOWNLIST_CONTROL(DoubleComboDropDownList, _T("Not Editable Double Combo"), CDoubleCombo, FALSE)
			REGISTER_STATIC_CONTROL(DoubleStatic, _T("Double Static"), CDoubleStatic, FALSE)
			REGISTER_LISTBOX_CONTROL(DoubleListBox, _T("Double Listbox"), CDoubleListBox, FALSE)
			REGISTER_STATIC_CONTROL(TBLinearGaugeCtrl, _T("Linear Gauge Control"), CTBLinearGaugeCtrl, FALSE)
			REGISTER_STATIC_CONTROL(TBCircularGaugeCtrl, _T("Circular Gauge Control"), CTBCircularGaugeCtrl, FALSE)
		END_CONTROLS_TYPE()

		BEGIN_CONTROLS_TYPE(DataType::Money)
			REGISTER_EDIT_CONTROL(MoneyEdit, _T("Money Edit"), CMoneyEdit, TRUE)
			REGISTER_COMBODROPDOWN_CONTROL(MoneyComboDropDown, _T("Editable Money Combo"), CMoneyCombo, FALSE)
			REGISTER_COMBODROPDOWNLIST_CONTROL(MoneyComboDropDownList, _T("Not Editable Money Combo"), CMoneyCombo, FALSE)
			REGISTER_STATIC_CONTROL(MoneyStatic, _T("Money Static"), CMoneyStatic, FALSE)
			END_CONTROLS_TYPE()
		BEGIN_CONTROLS_TYPE(DataType::Quantity)
			REGISTER_EDIT_CONTROL(QuantityEdit, _T("Quantity Edit"), CQuantityEdit, TRUE)
			REGISTER_COMBODROPDOWN_CONTROL(QuantityComboDropDown, _T("Editable Quantity Combo"), CQuantityCombo, FALSE)
			REGISTER_COMBODROPDOWNLIST_CONTROL(QuantityComboDropDownList, _T("Not Editable Quantity Combo"), CQuantityCombo, FALSE)
			REGISTER_STATIC_CONTROL(QuantityStatic, _T("Quantity Static"), CQuantityStatic, FALSE)
		END_CONTROLS_TYPE()

		BEGIN_CONTROLS_TYPE(DataType::DateTime)
			REGISTER_EDIT_CONTROL(DateTimeEdit, _T("DateTime Edit"), CDateTimeEdit, TRUE)
			REGISTER_STATIC_CONTROL(DateTimeStatic, _T("DateTime Static"), CDateTimeStatic, FALSE)
			REGISTER_LISTBOX_CONTROL(DateListBox, _T("Date Listbox"), CDateListBox, FALSE)
		END_CONTROLS_TYPE()

		BEGIN_CONTROLS_TYPE(DataType::Time)
			REGISTER_EDIT_CONTROL(TimeEdit, _T("Time Edit"), CTimeEdit, TRUE)
			REGISTER_STATIC_CONTROL(TimeStatic, _T("Time Static"), CTimeStatic, FALSE)
		END_CONTROLS_TYPE()

		BEGIN_CONTROLS_TYPE(DataType::Percent)
			REGISTER_EDIT_CONTROL(PercentEdit, _T("Percent Edit"), CPercEdit, TRUE)
			REGISTER_COMBODROPDOWN_CONTROL(PercentComboDropDown, _T("Editable Percent Combo"), CPercCombo, FALSE)
			REGISTER_COMBODROPDOWNLIST_CONTROL(PercentComboDropDownList, _T("Not Editable Percent Combo"), CPercCombo, FALSE)
			REGISTER_STATIC_CONTROL(PercentStatic, _T("Percent Static"), CPercStatic, FALSE)
		END_CONTROLS_TYPE()

		BEGIN_CONTROLS_TYPE(DataType::Bool)
			REGISTER_EDIT_CONTROL(BoolEdit, _T("Boolean Edit"), CBoolEdit, FALSE)
			REGISTER_PUSHBN_CONTROL(Button, _T("Push Button"), CPushButton, TRUE)
			REGISTER_RADIOBN_CONTROL(RadioButton, _T("Radio Button"), CBoolButton, TRUE)
			REGISTER_CHECKBN_CONTROL(CheckBox, _T("CheckBox"), CBoolButton, TRUE)
			REGISTER_CHECKBN_CONTROL(CheckBoxStatic, _T("CheckBox Static"), CBoolButtonStatic, FALSE)
			REGISTER_COMBODROPDOWNLIST_CONTROL(BoolCombo, _T("Boolean Combo"), CBoolCombo, FALSE)
			REGISTER_STATIC_CONTROL(BoolStatic, _T("Boolean Static"), CBoolStatic, TRUE)
			REGISTER_LISTBOX_CONTROL(BoolListBox, _T("Boolean Listbox"), CBoolListBox, FALSE)
			REGISTER_LISTBOX_CONTROL(BoolCheckListBox, _T("Boolean Check Listbox"), CBoolCheckListBox, FALSE)
		END_CONTROLS_TYPE()

		BEGIN_CONTROLS_TYPE(DataType::Guid)
			REGISTER_EDIT_CONTROL(UUidEdit, _T("Uuid Edit"), CGuidEdit, TRUE)
			REGISTER_STATIC_CONTROL(UUidStatic, _T("Uuid Static"), CGuidStatic, FALSE)
		END_CONTROLS_TYPE()

		BEGIN_CONTROLS_TYPE(DataType::Enum)
			REGISTER_COMBODROPDOWNLIST_CONTROL(EnumCombo, _T("Enum Combo"), CEnumCombo, TRUE)
			REGISTER_STATIC_CONTROL(EnumStatic, _T("Enum Static"), CEnumStatic, FALSE)
			REGISTER_LISTBOX_CONTROL(EnumListBox, _T("Enum Listbox"), CEnumListBox, FALSE)
			REGISTER_GROUP_CONTROL(EnumButton, _T("Enum Button"), CEnumButton, FALSE)
		END_CONTROLS_TYPE()
		
		BEGIN_CONTROLS_TYPE(DataType::Text)
		REGISTER_CONTROL(szMParsedEdit, TextEdit, _T("Text Edit"), CTextEdit, ES_MULTILINE | ES_WANTRETURN | WS_VSCROLL, 0, 0, 0, TRUE)
			REGISTER_STATIC_CONTROL(TextStatic, _T("Text Static"), CTextStatic, TRUE)
		END_CONTROLS_TYPE()

		BEGIN_CONTROLS_TYPE(DataType::Array)
			REGISTER_LISTBOX_CONTROL(ParsedCheckListBox, _T("Parsed Check Listbox"), CParsedCheckListBox, FALSE)
		END_CONTROLS_TYPE()
		
		BEGIN_CONTROLS_TYPE(DataType::Null)
			//non lo registro, perché lo stile va prima filtrato usando BS_TYPEMASK, e poi bisogna controllare se è uguale a BS_GROUPBOX
			//lo tratto quindi al pari degli altri generic controls, inline nella BaseWindowWrapper::Create
			//REGISTER_CONTROL(szMGroupBox, GroupBox, _T("Group Box"), CButton, BS_GROUPBOX, 0, BS_RADIOBUTTON, 0, FALSE)
			REGISTER_CONTROL		(szMLabel,			Label,			_T("Label"),		CStatic,			0, 0, 0, 0, FALSE)
			REGISTER_CONTROL		(szMTreeView,		TreeView,		_T("Tree View"),	CTreeViewAdvCtrl,	0, 0, 0, 0, TRUE)
			REGISTER_CONTROL		(szMPropertyGrid,	PropertyGrid,	_T("Property Grid"),CTBPropertyGrid,	0, 0, 0, 0, TRUE)
			REGISTER_CONTROL		(szMHeaderStrip,	HeaderStrip,	_T("Header Strip"), CHeaderStrip,		0, 0, 0, 0, TRUE)
			REGISTER_STATIC_CONTROL	(LinearGauge,	_T("Linear Gauge"),		CTBLinearGaugeCtrl,		FALSE)
			REGISTER_STATIC_CONTROL	(TBPicViewer,	_T("TBPicture Viewer"),	CTBPicViewerAdvCtrl,	FALSE)
		END_CONTROLS_TYPE()

		// associates default family for each data type
		DECLARE_TYPE_DEFAULT_FAMILY(DataType::String, szMParsedEdit)
		DECLARE_TYPE_DEFAULT_FAMILY(DataType::Text, szMParsedEdit)
		DECLARE_TYPE_DEFAULT_FAMILY(DataType::Integer, szMParsedEdit)
		DECLARE_TYPE_DEFAULT_FAMILY(DataType::Long, szMParsedEdit)
		DECLARE_TYPE_DEFAULT_FAMILY(DataType::Date, szMParsedEdit)
		DECLARE_TYPE_DEFAULT_FAMILY(DataType::DateTime, szMParsedEdit)
		DECLARE_TYPE_DEFAULT_FAMILY(DataType::ElapsedTime, szMParsedEdit)
		DECLARE_TYPE_DEFAULT_FAMILY(DataType::Time, szMParsedEdit)
		DECLARE_TYPE_DEFAULT_FAMILY(DataType::Percent, szMParsedEdit)
		DECLARE_TYPE_DEFAULT_FAMILY(DataType::Money, szMParsedEdit)
		DECLARE_TYPE_DEFAULT_FAMILY(DataType::Quantity, szMParsedEdit)
		DECLARE_TYPE_DEFAULT_FAMILY(DataType::Double, szMParsedEdit)
		DECLARE_TYPE_DEFAULT_FAMILY(DataType::Bool, szMCheckBox)
		DECLARE_TYPE_DEFAULT_FAMILY(DataType::Guid, szMParsedEdit)
		DECLARE_TYPE_DEFAULT_FAMILY(DataType::Enum, szMParsedCombo)
		DECLARE_TYPE_DEFAULT_FAMILY(DataType::Null, szMTreeView)
	END_REGISTER_CONTROLS()
END_ADDON_INTERFACE()
#undef _AddOn_Interface_Of

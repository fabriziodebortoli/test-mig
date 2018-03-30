//WINDOW USER MESSAGES
#define WM_USER_DIRTREE_LOCAL_CHANGE	(WM_USER + 1)
#define WM_USER_DIRTREE_REMOTE_CHANGE	(WM_USER + 2)
#define TTM_SETTITLEA           		(WM_USER + 32)  // wParam = TTI_*, lParam = char* szTitle
#define TTM_SETTITLEW           		(WM_USER + 33)  // wParam = TTI_*, lParam = wchar* szTitle

#define UM_DESTMOVE_SCROLLBAR			(WM_USER + 100)
#define MSM_HWNDASSOCIATESET			(WM_USER + 120)
#define MSM_HWNDASSOCIATEGET			(WM_USER + 121)
#define MSM_DWRANGESET					(WM_USER + 122)
#define MSM_DWRANGEGET					(WM_USER + 123)
#define MSM_WCURRENTPOSSET				(WM_USER + 124)
#define MSM_WCURRENTPOSGET				(WM_USER + 125)
#define MSM_FNOPEGSCROLLSET				(WM_USER + 126)
#define MSM_FNOPEGSCROLLGET				(WM_USER + 127)
#define MSM_FINVERTRANGESET				(WM_USER + 128)
#define MSM_FINVERTRANGEGET				(WM_USER + 129)
#define MSM_CRCOLORSET					(WM_USER + 130)
#define MSM_CRCOLORGET					(WM_USER + 131)
#define	UM_PARSE_BEGIN					(WM_USER + 132)
#define	UM_PARSE_TIC					(WM_USER + 133)
#define	UM_PARSE_END					(WM_USER + 134)
#define UM_BAD_VALUE					(WM_USER + 135)
#define UM_VALUE_CHANGED				(WM_USER + 136)
#define	UM_LOSING_FOCUS					(WM_USER + 137)
#define	UM_PUSH_BUTTON_CTRL				(WM_USER + 138)
#define	UM_CTRL_FOCUSED					(WM_USER + 139)
#define UM_FORMAT_STYLE_CHANGED			(WM_USER + 140)
#define UM_FONT_STYLE_CHANGED			(WM_USER + 141)
#define UM_RUN_BATCH					(WM_USER + 142)
#define	UM_CHECK_DROPDOWN				(WM_USER + 143)
#define UM_ITEM_CODE_REQUIRED			(WM_USER + 144)
#define UM_ITEM_TYPE_REQUIRED			(WM_USER + 145)
#define UM_RECALC_CTRL_SIZE				(WM_USER + 146)
#define UM_TROVATAFOGLIA				(WM_USER + 147)
#define UM_TROVATATAG		    		(WM_USER + 148)
#define UM_CHECKSTATECHANCETREE    		(WM_USER + 149)
#define UM_FORMATTERCHANGE	    		(WM_USER + 150)
#define UM_FONTCHANGE	    			(WM_USER + 151)
#define UM_EXTDOC_FETCH	    			(WM_USER + 152)
#define UM_MENU_REQUIRED				(WM_USER + 153)

#define UM_SIGNAL_TB_EVENT				(WM_USER + 801)
#define UM_SOAP_DISPATCH				(WM_USER + 802)
#define UM_MESSAGE_DISPATCH				(WM_USER + 803)

#define UM_GET_REPORT_NAMESPACE			(WM_USER + 900)
#define UM_GET_DIALOG_ID				(WM_USER + 901)
#define UM_DESTROY_CALENDAR				(WM_USER + 902)
#define UM_DESTROY_ADDRESSLIST			(WM_USER + 903)
#define UM_GET_SOAP_PORT				(WM_USER + 904)
#define UM_GET_CONTROL_DESCRIPTION		(WM_USER + 905)
#define UM_GET_LOCALIZER_INFO			(WM_USER + 906)
#define UM_UPDATE_FRAME_STATUS			(WM_USER + 907)
#define UM_DOCUMENT_CREATED				(WM_USER + 908)
#define UM_DOCUMENT_DESTROYED			(WM_USER + 909)
#define UM_UPDATE_EXTERNAL_MENU			(WM_USER + 910)
#define UM_GET_DOC_NAMESPACE_ICON		(WM_USER + 911)
#define UM_FRAME_TITLE_UPDATED			(WM_USER + 912)
#define UM_CLOSE_LOGIN_ASYNC			(WM_USER + 913)
#define UM_CLOSE_LOGIN					(WM_USER + 914)
#define UM_RADAR_SELECT					(WM_USER + 915)
#define UM_SET_STATUS_BAR_TEXT			(WM_USER + 916)
#define UM_CLEAR_STATUS_BAR				(WM_USER + 917)
#define UM_SET_MENU_WINDOW_TEXT			(WM_USER + 918)
#define UM_EXECUTE_FUNCTION				(WM_USER + 919)
#define UM_CLEAR_MESSAGE_QUEUE			(WM_USER + 920)
#define UM_GET_PARSED_CTRL				(WM_USER + 921)
#define UM_GET_CWND						(WM_USER + 922)
#define UM_ACTIVATE_TAB					(WM_USER + 923)
#define UM_ACTIVATE_TAB_PAGE			(WM_USER + 924)
#define UM_CLEAR_COMBO_ITEMS			(WM_USER + 925)
#define UM_HOTLINK_CLOSED				(WM_USER + 926)
#define UM_TEXT_CHANGED					(WM_USER + 927)
#define UM_FRAME_ACTIVATE				(WM_USER + 928)
#define UM_GET_WEB_COMMAND_TYPE			(WM_USER + 929)
#define UM_STOP_THREAD					(WM_USER + 930)
#define UM_KILL_THREAD					(WM_USER + 931)
#define UM_IS_UNATTENDED_WINDOW			(WM_USER + 932)
#define UM_GET_PARSED_CTRL_NS			(WM_USER + 933)
#define UM_RADAR_MOVEROW				(WM_USER + 934)
#define UM_GET_COMPONENT				(WM_USER + 935)
#define UM_GET_PARSEDCTRL_TYPE		    (WM_USER + 936)
#define UM_RANGE_SELECTOR_CLOSED		(WM_USER + 937)
#define UM_RANGE_SELECTOR_SELECTED		(WM_USER + 938)
#define UM_SET_USER_PANEL_TEXT			(WM_USER + 939)
#define UM_CLONE_DOCUMENT				(WM_USER + 940)
#define UM_EASYBUILDER_ACTION			(WM_USER + 941)
#define UM_GET_DOCUMENT_TITLE_INFO		(WM_USER + 942)
#define UM_REFRESH_USER_OBJECTS			(WM_USER + 943)
#define UM_SWITCH_ACTIVE_TAB			(WM_USER + 944)
#define UM_DESTROYING_DOCKABLE_FRAME	(WM_USER + 945)
#define UM_REFRESH_USER_LOGIN			(WM_USER + 946)
#define UM_CHOOSE_CUSTOMIZATION_CONTEXT_AND_EASYBUILDERIT_AGAIN		(WM_USER + 947)
#define UM_IS_ROOT_DOCUMENT				(WM_USER + 948)
#define UM_IMMEDIATE_BALLOON			(WM_USER + 949)
#define UM_HAS_INVALID_VIEW				(WM_USER + 950)
#define UM_GET_COMPONENT_STRINGS		(WM_USER + 951)
#define UM_REFRESH_ORGANIZER			(WM_USER + 952)
#define UM_BE_SETCELL					(WM_USER + 953)
#define UM_SET_UPLOADED_FILE_PATH		(WM_USER + 954)
#define	UM_GET_SPLITTED_STRING			(WM_USER + 953)
#define	UM_CAPTION_BAR_HYPERLINK_CLICKED		(WM_USER + 954)
#define	UM_MENU_CREATED					(WM_USER + 955)
#define	UM_EXTDOC_BATCH_COMPLETED		(WM_USER + 956)
#define	UM_CHANGE_LOGIN					(WM_USER + 957)
#define UM_DEFERRED_CREATE_SLAVE		(WM_USER + 958)
#define UM_ACTIVATE_MENU				(WM_USER + 959)
#define UM_CAPTURE_SCREENSHOT			(WM_USER + 960)
#define UM_LOGIN_COMPLETED				(WM_USER + 961)
#define UM_SHOW_IN_OPEN_DOCUMENTS		(WM_USER + 962)
#define UM_NEW_LOGIN					(WM_USER + 963)
#define UM_ACTIVATE_INTERNET			(WM_USER + 964)
#define UM_RELOGIN_COMPLETED			(WM_USER + 965)
#define UM_OPEN_CUSTOMIZATIONMANAGER	(WM_USER + 966)
#define UM_OPEN_MENUEDITOR				(WM_USER + 967)
#define UM_LOGGING						(WM_USER + 968)
#define UM_LOGIN_INCOMPLETED			(WM_USER + 969)
#define UM_CHOOSE_CUSTOMIZATION_CONTEXT (WM_USER + 970)
#define UM_PIN_UNPIN_ACTIONS			(WM_USER + 971)
#define UM_LAYOUT_SUSPENDED_CHANGED		(WM_USER + 972)
#define UM_TOOLBAR_UPDATE				(WM_USER + 973)
#define UM_OPEN_URL						(WM_USER + 974)
#define UM_INITIALIZE_TILE_LAYOUT		(WM_USER + 975)
#define UM_LAYOUT_CHANGED				(WM_USER + 976)
#define UM_GET_ACTIVATION_DATA			(WM_USER + 977)
#define UM_MENU_MNG_RESIZING			(WM_USER + 978)
#define UM_TILEPART_AFTER_RELAYOUT		(WM_USER + 979)
#define UM_SEND_CURRENT_TOKEN			(WM_USER + 980)
#define UM_EASYBUILDER_WEB_ACTION		(WM_USER + 981)

#define	EN_SPIN_RELEASED			0x7000
#define	EN_VALUE_CHANGED			0x7001
#define	EN_DATA_UPDATED				0x7002
#define	BEN_ROW_CHANGED				0x7003
#define	PCN_SET_FOCUS				0x7004
#define	EN_CTRL_STATE_CHANGED		0x7005
#define	EN_VALUE_CHANGED_FOR_FIND	0x7007	
#define	EN_AFTER_VALUE_CHANGED_BY_HKL		0x7008	
#define	EN_AFTER_VALUE_CHANGED_FOR_FIND_BY_HKL		0x7009	
#define	EN_REJECTED_UPDATE_BY_WARNING				0x7010

#define REFRESH_USER_DOCUMENT		1				
#define REFRESH_USER_REPORT			2	
#define RELOAD_ALL_MENUS			3

#define TBWEB//per abilitare la compilazione del nuovo TB WEB

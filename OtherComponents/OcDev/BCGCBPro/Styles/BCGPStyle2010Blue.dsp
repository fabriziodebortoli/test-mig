# Microsoft Developer Studio Project File - Name="Office 2010 (blue)" - Package Owner=<4>
# Microsoft Developer Studio Generated Build File, Format Version 6.00
# ** DO NOT EDIT **

# TARGTYPE "Win32 (x86) Dynamic-Link Library" 0x0102

CFG=Office 2010 (blue) - Win32 Debug
!MESSAGE This is not a valid makefile. To build this project using NMAKE,
!MESSAGE use the Export Makefile command and run
!MESSAGE 
!MESSAGE NMAKE /f "BCGPStyle2010Blue.mak".
!MESSAGE 
!MESSAGE You can specify a configuration when running NMAKE
!MESSAGE by defining the macro CFG on the command line. For example:
!MESSAGE 
!MESSAGE NMAKE /f "BCGPStyle2010Blue.mak" CFG="Office 2010 (blue) - Win32 Debug"
!MESSAGE 
!MESSAGE Possible choices for configuration are:
!MESSAGE 
!MESSAGE "Office 2010 (blue) - Win32 Debug" (based on "Win32 (x86) Dynamic-Link Library")
!MESSAGE "Office 2010 (blue) - Win32 Release" (based on "Win32 (x86) Dynamic-Link Library")
!MESSAGE 

# Begin Project
# PROP AllowPerConfigDependencies 0
CPP=cl.exe
MTL=midl.exe
RSC=rc.exe

!IF  "$(CFG)" == "Office 2010 (blue) - Win32 Debug"

# PROP BASE Use_MFC 0
# PROP BASE Use_Debug_Libraries 0
# PROP BASE Output_Dir "..\..\..\bin"
# PROP BASE Intermediate_Dir "Debug"
# PROP BASE Ignore_Export_Lib 1
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 0
# PROP Output_Dir "..\..\bin"
# PROP Intermediate_Dir "intermediate"
# PROP Ignore_Export_Lib 1
# PROP Target_Dir ""
# ADD BASE CPP /nologo /MT /W3 /GX /O1 /D "NDEBUG" /D "WIN32" /D "_WINDOWS" /D "_MBCS" /D "_USRDLL" /YX /FD /c
# ADD CPP /nologo /MT /W3 /D "NDEBUG" /D "WIN32" /D "_WINDOWS" /D "_MBCS" /D "_USRDLL" /FD /c
# SUBTRACT CPP /Gf /Gy /YX
# ADD BASE MTL /nologo /D "NDEBUG" /win32
# SUBTRACT BASE MTL /mktyplib203
# ADD MTL /nologo /D "NDEBUG" /win32
# SUBTRACT MTL /mktyplib203
# ADD BASE RSC /l 0x409 /d "NDEBUG"
# ADD RSC /l 0x409 /d "NDEBUG" /d "_BCGP_STYLE_DLL_"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 /nologo /dll /machine:I386 /nodefaultlib /NOENTRY
# SUBTRACT BASE LINK32 /pdb:none
# ADD LINK32 /nologo /dll /pdb:none /machine:I386 /nodefaultlib /out:"..\..\bin/BCGPStyle2010Blue2430.dll" /NOENTRY

!ELSEIF  "$(CFG)" == "Office 2010 (blue) - Win32 Release"

# PROP BASE Use_MFC 0
# PROP BASE Use_Debug_Libraries 0
# PROP BASE Output_Dir "..\..\..\bin"
# PROP BASE Intermediate_Dir "Release"
# PROP BASE Ignore_Export_Lib 1
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 0
# PROP Output_Dir "..\..\bin"
# PROP Intermediate_Dir "intermediate"
# PROP Ignore_Export_Lib 1
# PROP Target_Dir ""
# ADD BASE CPP /nologo /MT /W3 /GX /O1 /D "NDEBUG" /D "WIN32" /D "_WINDOWS" /D "_MBCS" /D "_USRDLL" /YX /FD /c
# ADD CPP /nologo /MT /W3 /D "NDEBUG" /D "WIN32" /D "_WINDOWS" /D "_MBCS" /D "_USRDLL" /FD /c
# SUBTRACT CPP /Gf /Gy /YX
# ADD BASE MTL /nologo /D "NDEBUG" /win32
# SUBTRACT BASE MTL /mktyplib203
# ADD MTL /nologo /D "NDEBUG" /win32
# SUBTRACT MTL /mktyplib203
# ADD BASE RSC /l 0x409 /d "NDEBUG"
# ADD RSC /l 0x409 /d "NDEBUG" /d "_BCGP_STYLE_DLL_"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 /nologo /dll /machine:I386 /nodefaultlib /NOENTRY
# SUBTRACT BASE LINK32 /pdb:none
# ADD LINK32 /nologo /dll /pdb:none /machine:I386 /nodefaultlib /out:"..\..\bin/BCGPStyle2010Blue2430.dll" /NOENTRY

!ENDIF 

# Begin Target

# Name "Office 2010 (blue) - Win32 Debug"
# Name "Office 2010 (blue) - Win32 Release"
# Begin Group "Source Files"

# PROP Default_Filter "cpp;c;cxx;rc;def;r;odl;idl;hpj;bat"
# Begin Source File

SOURCE=.\BCGPStyle2010Blue.rc
# End Source File
# Begin Source File

SOURCE=.\BCGPStyle2010Blue.rc2
# End Source File
# End Group
# Begin Group "Resource Files"

# PROP Default_Filter "ico;cur;bmp;dlg;rc2;rct;bin;rgs;gif;jpg;jpeg;jpe"
# Begin Group "AppCaption"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\AppCaption\SysBtn_Back.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\AppCaption\SysBtn_Back_C.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\AppCaption\SysBtn_Close.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\AppCaption\SysBtn_Close_S.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\AppCaption\SysBtn_Help.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\AppCaption\SysBtn_Help_S.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\AppCaption\SysBtn_Maximize.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\AppCaption\SysBtn_Maximize_S.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\AppCaption\SysBtn_Minimize.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\AppCaption\SysBtn_Minimize_S.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\AppCaption\SysBtn_Restore.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\AppCaption\SysBtn_Restore_S.png"
# End Source File
# End Group
# Begin Group "ComboBox"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\ComboBox\Btn.png"
# End Source File
# End Group
# Begin Group "MainWnd"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\MainWnd\AppCaption.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\MainWnd\Border.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\MainWnd\BorderCaption.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\MainWnd\BorderMDIChild.png"
# End Source File
# End Group
# Begin Group "MenuBar"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\MenuBar\Btn.png"
# End Source File
# End Group
# Begin Group "PopupBar"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\PopupBar\Border.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\PopupBar\Gripper.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\PopupBar\Item_Back.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\PopupBar\Item_Marker_C.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\PopupBar\Item_Marker_R.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\PopupBar\Item_Pin.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\PopupBar\MenuButton.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\PopupBar\MenuHighlight.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\PopupBar\MenuHighlightDisabled.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\PopupBar\MenuScrollBtn.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\PopupBar\ResizeBar.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\PopupBar\ResizeBar_HV.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\PopupBar\ResizeBar_HVT.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\PopupBar\ResizeBar_V.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\PopupBar\Tear.png"
# End Source File
# End Group
# Begin Group "StatusBar"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\StatusBar\Back.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\StatusBar\Back_Ext.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\StatusBar\PaneBorder.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\StatusBar\SizeGrip.png"
# End Source File
# End Group
# Begin Group "Tab"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\Tab\3D.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Tab\Dot_E.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Tab\Dot_R.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Tab\Flat.png"
# End Source File
# End Group
# Begin Group "TaskPane"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\TaskPane\ScrollBtn.png"
# End Source File
# End Group
# Begin Group "ToolBar"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\ToolBar\Btn.png"
# End Source File
# End Group
# Begin Group "Ribbon"

# PROP Default_Filter ""
# Begin Group "Buttons"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\BS_List_Icon.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\BS_Normal.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\BS_Push.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\BS_Separator_H.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\BS_Separator_V.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\BS_View.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\BS_View_Icon.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Check.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Default.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Default_Icon.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Default_Image.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Default_QAT.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Group.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Group_S.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\GroupMenu_F_C.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\GroupMenu_L_M.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\GroupSeparator.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Launch.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Launch_Icon.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Main.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Main_Panel.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Menu_H_C.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Menu_H_M.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Menu_V_C.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Menu_V_M.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Minimize_Icon.PNG"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Normal_B.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Normal_S.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Page_L.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Page_R.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Palette_B.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Palette_M.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Palette_T.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Push.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\Radio.PNG"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Buttons\StatusPane.png"
# End Source File
# End Group
# Begin Group "Category_Context"

# PROP Default_Filter ""
# Begin Group "Red"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Red\Back.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Red\Caption.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Red\Default.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Red\Panel.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Red\Separator.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Red\Tab.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Red\Tab_Glow.png"
# End Source File
# End Group
# Begin Group "Orange"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Orange\Back.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Orange\Caption.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Orange\Default.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Orange\Panel.PNG"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Orange\Separator.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Orange\Tab.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Orange\Tab_Glow.png"
# End Source File
# End Group
# Begin Group "Yellow"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Yellow\Back.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Yellow\Caption.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Yellow\Default.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Yellow\Panel.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Yellow\Separator.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Yellow\Tab.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Yellow\Tab_Glow.png"
# End Source File
# End Group
# Begin Group "Green"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Green\Back.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Green\Caption.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Green\Default.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Green\Panel.PNG"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Green\Separator.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Green\Tab.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Green\Tab_Glow.png"
# End Source File
# End Group
# Begin Group "Blue"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Blue\Back.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Blue\Caption.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Blue\Default.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Blue\Panel.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Blue\Separator.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Blue\Tab.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Blue\Tab_Glow.png"
# End Source File
# End Group
# Begin Group "Indigo"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Indigo\Back.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Indigo\Caption.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Indigo\Default.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Indigo\Panel.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Indigo\Separator.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Indigo\Tab.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Indigo\Tab_Glow.png"
# End Source File
# End Group
# Begin Group "Violet"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Violet\Back.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Violet\Caption.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Violet\Default.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Violet\Panel.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Violet\Separator.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Violet\Tab.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Context\Violet\Tab_Glow.png"
# End Source File
# End Group
# End Group
# Begin Group "Slider"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Slider\Minus.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Slider\Plus.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Slider\Thumb.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Slider\Thumb_H.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Slider\Thumb_L.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Slider\Thumb_R.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Slider\Thumb_T.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Slider\Thumb_V.png"
# End Source File
# End Group
# Begin Group "Progress"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Progress\Back.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Progress\Infinity.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Progress\Normal.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Progress\Normal_Ext.png"
# End Source File
# End Group
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Border_Floaty.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Border_Panel.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Border_QAT.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Caption_QA.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Caption_QA_Glass.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Back.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Tab.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Tab_Frame.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Category_Tab_Sep.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\KeyTip.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Panel_Back.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Panel_BS.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Panel_Main.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Panel_Main_Border.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Panel_QAT.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Ribbon\Separator.png"
# End Source File
# End Group
# Begin Group "Outlook"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\OutlookWnd\BarBack.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\OutlookWnd\BtnPage.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\OutlookWnd\Splitter.png"
# End Source File
# End Group
# Begin Group "Scroll"

# PROP Default_Filter ""
# Begin Group "Horz"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\Scroll\Horz\Back_1.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Scroll\Horz\Back_2.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Scroll\Horz\Item_1.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Scroll\Horz\Item_2.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Scroll\Horz\Thumb_1.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Scroll\Horz\Thumb_2.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Scroll\Horz\ThumbIcon.png"
# End Source File
# End Group
# Begin Group "Vert"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\Scroll\Vert\Back_1.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Scroll\Vert\Back_2.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Scroll\Vert\Item_1.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Scroll\Vert\Item_2.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Scroll\Vert\Thumb_1.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Scroll\Vert\Thumb_2.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Scroll\Vert\ThumbIcon.png"
# End Source File
# End Group
# End Group
# Begin Group "Slider No. 1"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\Slider\Thumb_B.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Slider\Thumb_H.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Slider\Thumb_L.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Slider\Thumb_R.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Slider\Thumb_T.png"
# End Source File
# Begin Source File

SOURCE=".\Office 2010 (blue)\Slider\Thumb_V.png"
# End Source File
# End Group
# Begin Group "Calculator"

# PROP Default_Filter ""
# Begin Source File

SOURCE=".\Office 2010 (blue)\Calculator\Btn.png"
# End Source File
# End Group
# Begin Source File

SOURCE=".\Office 2010 (blue)\Style.xml"
# End Source File
# End Group
# End Target
# End Project

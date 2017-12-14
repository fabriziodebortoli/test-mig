#pragma once

#include <TbGes/ExtdocAbstract.h>
#include "beginh.dex"


////////////////////////////////////////////////////////////////////////////////
//				class(interface) IAbstractFormDocObject definition
////////////////////////////////////////////////////////////////////////////////
//===========================================================================
class TB_EXPORT IAbstractFormDocObject
{
public:
	virtual BOOL	OnOpenDocument			(LPCTSTR)	= 0;
	virtual BOOL	OnNewDocument			()			= 0;
	virtual BOOL	OnSaveDocument			(LPCTSTR)	= 0;
	virtual void	OnBeforeSaveDocument	()			= 0;
	virtual void	OnAfterSaveDocument		()			= 0;

	virtual void	OnGoInBrowseMode		()			= 0;
	virtual void	Customize				()			= 0;
	virtual BOOL	OnBeforeBatchExecute	()			= 0;
	virtual void	OnAfterBatchExecute		()			= 0;
	virtual void	OnDocumentCreated		()			= 0;
	
	virtual void OnBeforeCloseDocument		()			= 0;
	virtual void OnCloseServerDocument		()			= 0;
	virtual void OnBeforeBrowseRecord		()			= 0;
	virtual BOOL OnOkTransaction			()			= 0;
	virtual BOOL OnBeforeOkTransaction		()			= 0;
	virtual void OnAfterSetFormMode			(CBaseDocument::FormMode oldFormMode) = 0;

	virtual BOOL SaveModified				()			= 0;
	virtual BOOL OnOkDelete					()			= 0;
	virtual BOOL OnOkEdit					()			= 0;
	virtual BOOL OnOkNewRecord				()			= 0;
	 
	virtual BOOL CanDoDeleteRecord			()			= 0;
	virtual BOOL CanDoEditRecord			()			= 0;
	virtual BOOL CanDoNewRecord				()			= 0;
		
	virtual BOOL OnBeforeDeleteRecord		()			= 0;
	virtual BOOL OnBeforeEditRecord			()			= 0;
	virtual BOOL OnBeforeNewRecord			()			= 0;
	virtual BOOL OnBeforeNewTransaction		()			= 0;
	virtual BOOL OnBeforeEditTransaction	()			= 0;
	virtual BOOL OnBeforeDeleteTransaction	()			= 0;
	virtual BOOL OnNewTransaction			()			= 0;
	virtual BOOL OnEditTransaction			()			= 0;
	virtual BOOL OnDeleteTransaction		()			= 0;

	virtual void OnExtraNewTransaction		()			= 0;
	virtual void OnExtraEditTransaction		()			= 0;
	virtual void OnExtraDeleteTransaction	()			= 0;

	virtual CAbstractFormDoc::LockStatus	OnLockDocumentForNew		() = 0;
	virtual CAbstractFormDoc::LockStatus	OnLockDocumentForEdit		() = 0;
	virtual CAbstractFormDoc::LockStatus	OnLockDocumentForDelete		() = 0;

	virtual BOOL OnAttachData				() = 0;
	virtual BOOL OnPrepareAuxData			() = 0;
	virtual BOOL OnInitAuxData				() = 0;
	virtual BOOL OnInitDocument				() = 0;
	virtual void OnCloseDocument			() = 0;

	virtual void DisableControlsForBatch	() = 0;
	virtual void EnableControlsForFind		() = 0;
	virtual void DisableControlsAlways		() = 0;

	virtual void	OnPrepareBrowser		(SqlTable*) = 0;

	virtual BOOL	OnRunReport			(CWoormInfo*)									= 0;
	virtual BOOL	OnHKLIsValid		(HotKeyLink* pHotKeyLink)						= 0;

	virtual BOOL OnValidateRadarSelection	(SqlRecord* pRec, CTBNamespace nsHotLinkNamespace, HotKeyLink* pHotKeyLink) = 0;
	virtual BOOL OnValidateRadarSelection	(SqlRecord* pRec, HotKeyLink* pHotKeyLink) = 0;
	virtual void OnAbortAllViews			() = 0;
};

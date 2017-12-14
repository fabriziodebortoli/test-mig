
#pragma once


#include <TbGes\Dbt.h>
#include <TbGes\Extdoc.h>

//Components
#include "beginh.dex"


class TResourcesDetails;
class DBTRSEntities;
class CTileRSMaintenance;

//=============================================================================
//							VRSEntities
//=============================================================================
//
//-----------------------------------------------------------------------------
class TB_EXPORT VRSEntities : public SqlVirtualRecord
{
	DECLARE_DYNCREATE(VRSEntities)

	// Local fields
	DataStr		l_EntityName;
	DataBool	l_IsProtected;

public:
	VRSEntities(BOOL bCallInit = TRUE);

public:
	virtual void	BindRecord();

public:
	static LPCTSTR GetStaticName();
};

//=============================================================================
class TB_EXPORT BDRSMaintenance : public CAbstractFormDoc
{
    DECLARE_DYNCREATE(BDRSMaintenance)

private:
	CRSResourcesArray*	m_pResources;
	CHierarchyArray*	m_pHierarchies;
	BOOL				m_bCloseAfterExecute;

public:
	DBTRSEntities*	 m_pDBTRSEntities;

	CString PathDone;
	CString PathNotDone;

	DataStr		StepStartImg;
	DataStr		Step1Img;
	DataStr		Step2Img;
	DataStr		Step3Img;
	DataStr		Step4Img;
	DataStr		StepEndImg;

	CPictureStatic*		m_pPictureStatic_Step1;
	CPictureStatic*		m_pPictureStatic_Step2;
	CPictureStatic*		m_pPictureStatic_Step3;
	CPictureStatic*		m_pPictureStatic_Step4;

	CTileRSMaintenance*		pTile;

public:
    BDRSMaintenance();
	~BDRSMaintenance();

public:
	BOOL ExecuteMaintenanceProcedure();

	
private:
	BOOL SaveUsedEntities		();
	BOOL ManageSubjects			();
	BOOL ManageHierarchies		();

	void AnalyzeResourcesDetails();
	void AnalyzeWorkersDetails	();
	void ExploreResource(const DataStr& aChildResourceType, const DataStr& aChildResourceCode, int aNrLevel, CRSResourcesArray* pParentArray);
	void ExploreWorker(int aWorkerId, int aNrLevel, CRSResourcesArray* pParentArray);

	void HideAll				();

	void	Start();
	BOOL	Step1();
	BOOL	Step2();
	BOOL	Step3();
	BOOL	Step4();
	BOOL	End();


protected:
	virtual void	OnBatchExecute		(); 
	virtual BOOL	CanRunDocument		();
    virtual BOOL	OnAttachData		();
    virtual BOOL	CanDoBatchExecute	();

   DECLARE_MESSAGE_MAP()
};


//////////////////////////////////////////////////////////////////////////////
//             class DBTRSEntities definition SLAVE FINTO
//////////////////////////////////////////////////////////////////////////////
//                                                                          
//=============================================================================
class TB_EXPORT DBTRSEntities : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTRSEntities)

public:
	DBTRSEntities(CRuntimeClass*, CAbstractFormDoc*);

public:
	BDRSMaintenance*	GetDocument()	const { return (BDRSMaintenance*)m_pDocument; }
	VRSEntities*		GetRowSecurityEntity()	const { return (VRSEntities*)GetRecord(); }
};

#include "endh.dex"

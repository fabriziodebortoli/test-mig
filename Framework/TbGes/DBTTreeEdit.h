///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------

#pragma once

#include <TbOleDb\sqlrec.h>
#include <TbGes\dbt.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// class CTreeBodyEditNodeInfo
//-----------------------------------------------------------------------------
class TB_EXPORT CTreeBodyEditNodeInfo : public CObject
{
protected:
	int			m_nLevel;
	int			m_nParent;
	BOOL		m_bHasChild;
	BOOL		m_bExpanded;
	int			m_idBitmap;

public:
	CTreeBodyEditNodeInfo() 
		:
		m_nLevel	(0),
		m_nParent	(0),
		m_bHasChild (FALSE),
		m_bExpanded (FALSE),
		m_idBitmap	(-1)
	{ }

	CTreeBodyEditNodeInfo(const CTreeBodyEditNodeInfo& ni) 
		:
		m_nLevel	(ni.m_nLevel),
		m_nParent	(ni.m_nParent),
		m_bHasChild (ni.m_bHasChild),
		m_bExpanded (ni.m_bExpanded),
		m_idBitmap	(ni.m_idBitmap)
	{ }

	CTreeBodyEditNodeInfo(const CString& sInfo) 
	{
		SetInfo(sInfo);
	}

	CTreeBodyEditNodeInfo(const DataStr& sInfo) 
	{
		SetInfo(sInfo.GetString());
	}

	void	SetInfo(const CString& sInfo);
		//{_stscanf(sInfo, _T("%d|%d|%d|%d|%d"), &m_nLevel, &m_nParent, &m_bHasChild, &m_bExpanded, &m_idBitmap);	}

	CString GetInfo()
	{
		return ToString
			(
				m_nLevel,
				m_nParent,
				m_bHasChild,
				m_bExpanded,
				m_idBitmap
			);
	}

	void ToggleExpand	()
	{
		if (m_bHasChild)
		{
			m_bExpanded = !m_bExpanded;
		}
	}

	void	SetLevel	(int nLevel)			{	m_nLevel	= nLevel;		}
	void	SetParent	(int nParent)			{	m_nParent	= nParent;		}
	void	SetHasChild	(BOOL bSet = TRUE)		{	m_bHasChild = bSet;			}
	void	SetExpanded	(BOOL bExpanded = TRUE)	{	m_bExpanded = bExpanded;	}
	void	SetIDB		(int nIdb)				{	m_idBitmap  = nIdb;			}

	//------------------- Data Access ---------------------------
	int			GetLevel()		const	{return m_nLevel;		}
	int			GetParent()		const	{return m_nParent;		}
	BOOL		HasChild()		const	{return m_bHasChild;	}
	BOOL		IsExpanded()	const	{return m_bExpanded;	}
	int			GetIDB()		const	{return m_idBitmap;		}

	static CString	ToString
							(
								int		nLevel,
								int		nIDParent,
								BOOL	bHasChild,
								BOOL	bExpanded,
								UINT	idb
							)
					{
						// Stringa del tipo
						// <Livello>|<IDPadre|><Figli>|<Expanded>|<IDB>
						CString	fullstring;
						fullstring.Format(_T("%.3d|%.5d|%d|%d|%.3d"),
							nLevel,
							nIDParent,
							bHasChild ? 1 : 0,
							bExpanded ? 1 : 0,
							idb
						);
						return fullstring;
					}
};

//-----------------------------------------------------------------------------
// class CTBExtTreeEditDBT
//-----------------------------------------------------------------------------
class  TB_EXPORT DBTTree : public DBTSlaveBuffered
{
	friend class CBodyEditTree;
	DECLARE_DYNAMIC(DBTTree)

protected:
	BOOL			m_bShowTreeMsg;

	int				m_nTreeDataIdx;
	int				m_nTreeEditIdx;

	int				m_nTreeIDIdx;

protected:
	DBTTree
		(
			CRuntimeClass*		pClass,
			CAbstractFormDoc*	pDocument,
			const CString&		sName,
			BOOL				bAllowEmpty = ALLOW_EMPTY_BODY,
			BOOL				bCheckDuplicateKey = !CHECK_DUPLICATE_KEY
		);

	virtual	SqlRecord*	InsertRecord	(int nRow);
	virtual	BOOL		DeleteRecord	(int nRow);

public:
	virtual ~DBTTree();

	virtual void 		Init			();

			BOOL		GetShowTreeMsg	()					{ return m_bShowTreeMsg; }
			void		SetShowTreeMsg	(BOOL bShow = TRUE) { m_bShowTreeMsg = bShow; }


	virtual BOOL		FindData		(BOOL bPrepareOld = TRUE);

	virtual void		OnPrepareRow	(int nRow, SqlRecord*);

	// init SQLRecord (TreeData)
	virtual DataStr*	GetTreeDataObj()	= 0;
	virtual DataObj*	GetTreeIDObj()		{ return NULL; }
	virtual	DataObj*	GetTreeEditObj()	= 0;

	// gestione SqlRecord (TreeData)
	DataStr&	GetTreeData		(SqlRecord* pRec);	// { return *((DataStr*)pRec->GetDataObjAt(m_nTreeDataIdx));	}
	DataStr&	GetTreeData		(int nIdx);			// { return GetTreeData(GetRow(nIdx));							}
	DataStr&	GetAllTreeData	(int nidx);			// { return GetTreeData(m_pAllRecords->GetAt(nidx));			}

	DataObj&	GetTreeID		(SqlRecord* pRec);	// { return *((DataObj*)pRec->GetDataObjAt(m_nTreeIDIdx));		}
	DataObj&	GetTreeID		(int nIdx);			// { return GetTreeID(GetRow(nIdx));							}
	DataObj&	GetAllTreeID	(int nidx);			// { return GetTreeID(m_pAllRecords->GetAt(nidx));				}

	DataObj&	GetTreeEdit		(SqlRecord* pRec);	// { return *((DataObj*)pRec->GetDataObjAt(m_nTreeEditIdx));	}
	DataObj&	GetTreeEdit		(int nIdx);			// { return GetTreeEdit(GetRow(nIdx));							}
	DataObj&	GetAllTreeEdit	(int nidx);			// { return GetTreeEdit(m_pAllRecords->GetAt(nidx));			}

	// operazioni sul tree
	void	BuildTree			();
	void	BuildTree			(int start, int end);
	int		GetMaxLevel			();
	int		GetParent			(int nRow);

	void	ExpandAll			()			{ if (CanDoExpandAll())			SetAllExpand(TRUE); }
	void	CollapseAll			()			{ if (CanDoCollapseAll())		SetAllExpand(FALSE); }
	void	ExpandNode			(int nRow)	{ if (CanDoExpandNode(nRow))	SetNodeExpand(nRow, TRUE); }
	void	CollapseNode		(int nRow)	{ if (CanDoCollapseNode(nRow))	SetNodeExpand(nRow, FALSE); }

	void	SetLevelAll			(int nLevel);

	virtual BOOL	MoveLeft			(BOOL bMoveNextBrothers = FALSE);
	virtual BOOL	MoveRight			(BOOL bMoveNextBrothers = FALSE);
	//BOOL	MoveLeftAll			();
	//BOOL	MoveRightAll		();

	void	SwapRecords			(int nRowA, SqlRecord* pRecA, int nRowB, SqlRecord* pRecB, int nLevel);
	BOOL	IsVisible			(int nIdx) { return (RemapIndexA2F(nIdx) != -1); }
	void	MakeVisible			(int nIdx);
	
	virtual	SqlRecord*	AddRecord		();
	virtual	SqlRecord*	Insert			(int nRow, int& nIdxNewRec);
	virtual	SqlRecord*	InsertChild		(int nRow, int& nIdxNewRec);
	virtual	SqlRecord*	InsertRoot		();

	virtual	void		Sort			(int nRow, int& nIdxLastSortedRec, BOOL bToRemap = TRUE);

	virtual BOOL	CanDoExpandAll		() { return TRUE; }
	virtual BOOL	CanDoCollapseAll	() { return TRUE; }
	virtual BOOL	CanDoExpandNode		(int nRow);
	virtual BOOL	CanDoCollapseNode	(int nRow);
	virtual BOOL	CanDoShowLevel		() { return TRUE; }
	virtual BOOL	CanDoSetLevelAll	() { return TRUE; }
	virtual BOOL	CanDoMoveLeft		();
	virtual BOOL	CanDoMoveRight		();
	//virtual BOOL	CanDoInsert			();
	//virtual BOOL	CanDoInsertChild	();
	//virtual BOOL	CanDoInsertSpecial	();
	virtual BOOL	CanDoSort			() { return TRUE; }
	virtual BOOL	CanDoSortAll		();
	virtual BOOL	CanDoSortNode		();
	virtual BOOL	CanDoFind			() { return TRUE; }
	virtual BOOL	CanDoSearch			();

	virtual BOOL	OnBeforeMoveLeft	() {/*does nothing*/return TRUE; }
	virtual void	OnAfterMoveLeft		() {/*does nothing*/}
	virtual BOOL	OnBeforeMoveRight	() {/*does nothing*/return TRUE; }
	virtual void	OnAfterMoveRight	() {/*does nothing*/}

			void		SetTreeIcon				(SqlRecord* pSqlRec, int nIdxBmp);
			BOOL		HasChild				(SqlRecord* pSqlRec);
			BOOL		IsExpanded				(SqlRecord* pSqlRec);
			int			GetLevel				(SqlRecord* pSqlRec);

			SqlRecord*	GetParent				(SqlRecord* pSqlRec);
			int			GetParentAllRecords		(int nRow);
			int			GetRowIndex				(SqlRecord* pSqlRec);
			BOOL		GetChild				(SqlRecord* pRecParent, SqlRecord*& pAfterThisChild/* = NULL*/);
			BOOL		GetChilds				(SqlRecord* pRecParent, RecordArray&);

protected:
			void	UpdateTreeDataParent(RecordArray* parRecords, int startIndex, int oldKey, int newKey);

			void	PrepareTreeData1	();
			void	DoAfterInsertRow1	(int nRow, SqlRecord* pRec);

public:
	virtual void	SetAllExpand	(BOOL bExpand, BOOL bBuildTree = TRUE);
	virtual void	SetNodeExpand	(int nRow, BOOL bExpand, BOOL bBuildTree  = TRUE);
	virtual void	SetNodeExpand	(int nRow, CTreeBodyEditNodeInfo& ni);

	BOOL GetRowsByLevel(RecordArray& ar, int level);
};

//==============================================================================

#include "endh.dex"

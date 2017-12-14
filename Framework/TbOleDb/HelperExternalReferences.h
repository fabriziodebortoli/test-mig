 
#pragma once

#include <TbGeneric\Array.h>
#include <TbGeneric\dataobj.h>
#include <TbGenlib\addonmng.h>
#include <TbNameSolver\TBNamespaces.h>
#include <TbOleDb\SqlCatalog.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
//						CHelperExternalReferences
//=============================================================================
class TB_EXPORT CHelperExternalReferences : public CObject
{
	DECLARE_DYNAMIC(CHelperExternalReferences)

public:
	CHelperExternalReferences();
	~CHelperExternalReferences();

public:
	//-------------------------------------------------------------------------
	class CTableDBT : public CObject
	{
		public:
			enum DBTType { NONE, MASTER, SLAVE, SLAVEBUFFERED };

			CString		m_sTableName;	// nome tabella in elaborazione
			CString		m_sDBTNS;		// namespace della dbt della tabella
			CString		m_sDocNS;		// namespace del documento del dbt
			CString		m_sDbtsxmlPath;	// path del dbts.xml (solo path non file completo)
			CString		m_sModuleNS;	// namespace modulo (per apertura file xml)
			DBTType		m_eDBTType;		// tipo DBT
			CTableDBT*	m_pDBTParent;	// oggetto CTableDBT parent (solo per slave)
			Array		m_arDBTSlaves;	// array di CTableDBT slaves (solo per master) 

			//-----------------------------------------------------------------
			CTableDBT(const CString& sTableName)
			{
				m_sTableName	= sTableName;
				m_sDBTNS		.Empty();
				m_sDocNS		.Empty();
				m_sDbtsxmlPath	.Empty();
				m_sModuleNS		.Empty();
				m_eDBTType		= CTableDBT::NONE;
				m_pDBTParent	= NULL;
				m_arDBTSlaves.SetOwns(FALSE);
			}

			//-----------------------------------------------------------------
			CTableDBT(const CString& sTableName, const CString& sDBTNS, const CString& sDocNS, const CString& sDbtsxmlPath, const CString& sModuleNS, DBTType eDBTType = CTableDBT::NONE, CTableDBT* pParent = NULL)
			{
				m_sTableName	= sTableName;
				m_sDBTNS		= sDBTNS;
				m_sDocNS		= sDocNS;
				m_sDbtsxmlPath	= sDbtsxmlPath;
				m_sModuleNS		= sModuleNS;
				m_eDBTType		= eDBTType;
				m_pDBTParent	= pParent;
				m_arDBTSlaves.SetOwns(FALSE);
			}
			~CTableDBT()
			{
				m_arDBTSlaves.RemoveAll();
			}

	};

	//-------------------------------------------------------------------------
	class CTableExtRefs : public CObject
	{
		public:
			CString m_sTableName;	//nome tabella in elaborazione
			CString m_sCurrDocNS;	// namespace del doc corrente
			Array m_arExtRefs;		//array di CTableSingleExtRef per riferimenti esterni

			//-----------------------------------------------------------------
			CTableExtRefs(const CString& sTableName)
			{
				m_sTableName = sTableName;
				m_arExtRefs.SetOwns(TRUE);
			}
			~CTableExtRefs()
			{
				m_arExtRefs.RemoveAll();
			}
	};

	//-------------------------------------------------------------------------
	class CTableSingleExtRef : public CObject
	{
		public:
			CString m_sForeignKey;		// campo chiave esterna nella tabella m_sTableName di CTableExtRefs
			CString m_sExtTableName;	// nome tabella esterna
			CString m_sExtPrimaryKey;	// campo chiave primaria nella tabella esterna
			CString m_sExtDocNS;		// namespace del documento tabella esterna (usato solo x documentazione)
			CString m_sExpression;		// eventuale espressione di filtro sull'external reference

			//-----------------------------------------------------------------
			CTableSingleExtRef(const CString& sForeignKey, const CString& sExtTableName, const CString& sExtPrimaryKey, const CString& sExtDocNS = _NS_DOC(""), const CString& sExpression = _NS_DOC(""))
			{
				m_sForeignKey		= sForeignKey;
				m_sExtTableName		= sExtTableName;
				m_sExtPrimaryKey	= sExtPrimaryKey;
				m_sExtDocNS			= sExtDocNS;
				m_sExpression		= sExpression;
			}

			//-----------------------------------------------------------------
			BOOL IsEqual(const CString& sForeignKey, const CString& sExtTableName, const CString& sExtPrimaryKey, const CString& sExtDocNS, const CString& sExpression)
			{
				return	m_sForeignKey.		CompareNoCase(sForeignKey)		== 0 &&
						m_sExtTableName.	CompareNoCase(sExtTableName)	== 0 &&
						m_sExtPrimaryKey.	CompareNoCase(sExtPrimaryKey)	== 0 &&
						m_sExtDocNS.		CompareNoCase(sExtDocNS)		== 0 &&
						GetCleanExpression(m_sExpression).CompareNoCase(GetCleanExpression(sExpression)) == 0;
			}

			//-----------------------------------------------------------------
			CString GetCleanExpression(CString sExpression)
			{
				// toglie spazi tabulazioni e a capo dall'espressione per confronto
				CString sCleanExpression = sExpression;
				sCleanExpression.Remove(_T(' '));
				sCleanExpression.Remove(_T('\r'));
				sCleanExpression.Remove(_T('\n'));
				sCleanExpression.Remove(_T('\t'));
				return sCleanExpression;
			}
	};

protected:
	Array	m_arDBTs;					//array di CTableDBT
	Array	m_arExternalReferences;		//array di CTableExtRefs
	CMapStringToOb m_mapNSDocToDbts;	//mappa di m_sDocNS di CTableDBT
	CMapStringToOb m_mapTableToExtRefs;	//mappa di m_sTableName di CTableExtRefs

	CHelperSqlCatalog*	m_pHelperSqlCatalog;
	BOOL				m_bOwnedHelperSqlCatalog;//indica se m_pHelperSqlCatalog è stato istanziato localmente
	BOOL				m_bDBTsLoaded;	 //indica se è già stato caricato l'array di dbts.xml

	void	LoadModuleDbts					(AddOnModule* pAddOnModule);
	void	LoadExternalReferences			(CTableDBT* pDBT, CTableExtRefs** ppExtRefs);
	void	LoadExternalReferencesToTable(CTableDBT* pDBT, CTableExtRefs** ppExtRefs, CString tableName);

public:
	BOOL	LoadDBTs				();
	void	SetHelperSqlCatalog		(CHelperSqlCatalog* pHelperSqlCatalog);
	void	LoadHelperSqlCatalog	();

	BOOL	IsTableAlreadyLoaded	(const CString& sTableName);
	BOOL	LoadByTableName			(const CString& sTableName);
	CHelperExternalReferences::CTableExtRefs*	GetTableExtRefs(const CString& sTableName);
	Array* GetExtRefsToTable(const CString& sTableName);	//array of CTableExtRefs
};



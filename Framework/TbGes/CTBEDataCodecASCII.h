#pragma once

#include <tbgeneric\dataobj.h>

#include "TBEDataCoDec.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//-----------------------------------------------------------------------------
#define	TBECODEC_RELEASE	3
#define	TBECODEC_RELEASE_CI	3

class	Unparser;
class	Parser;
class	CTBEDataCoDecDocument;

enum ECodecToken 
{
	T_CODEC_DOCUMENT      =           23000,
	T_CODEC_DBTMASTER,                
	T_CODEC_DBTSLAVEBUFFERED ,        
	T_CODEC_DBTSLAVE,                 
	T_CODEC_SQLRECORD,				 
	T_CODEC_SQLTABLE,				 
	T_CODEC_FIELDS,					 
	T_CODEC_VALUES,					 
	T_CODEC_EXTRA_INFO,				 
	T_CODEC_DBT_ID,					 
	T_CODEC_CLASSINFO,				 
	T_CODEC_CLASS/*,
	T_CODEC_SESSION*/
};

///////////////////////////////////////////////////////////////////////////////
//	CTBEDataCoDecASCII
//-----------------------------------------------------------------------------
class TB_EXPORT CTBEDataCoDecASCII : public CTBEDataCoDec
{
	DECLARE_DYNAMIC(CTBEDataCoDecASCII)
protected:
	CString					m_strFName;
	CLIPFORMAT				m_cfLoaded;

	CAbstractFormDoc*		m_pDocument;
	Unparser*				m_pUnparser;
	Parser*					m_pParser;

	int						m_nIndent;
	CTBEDataCoDecDocument*	m_pParsedDoc;
	int						m_nDBTID;
public:

						CTBEDataCoDecASCII	(LPCTSTR szFName = NULL);
	virtual				~CTBEDataCoDecASCII	();

	virtual	BOOL		Encode			(CAbstractFormDoc*	pDocument	)	;
	virtual	BOOL		Encode			(CBodyEdit*	pBody)			;
	virtual	BOOL		Save			(CArchive&	ar)						;
	virtual	BOOL		Load			(CArchive&	ar,	CLIPFORMAT	cfLoaded = 0);
	virtual	BOOL		Load			(LPCTSTR szFileName,	CLIPFORMAT	cfLoaded = 0)	;
	virtual	void		SetFileName		(LPCTSTR		szFName);

	virtual	void		SetLoadedCF			(CLIPFORMAT	cfLoaded = 0){	m_cfLoaded = cfLoaded; };
	virtual	CLIPFORMAT	GetLoadedCF			() { return m_cfLoaded; }

protected:
	virtual	BOOL		NewDocument		(CAbstractFormDoc*	pDocument	)	;
	virtual	void		Close			()									;

	virtual	void		AddDBTMaster	(DBTMaster*			pDbt)			;
	virtual	void		AddDBTSlave		(DBTSlave*			pDbt)			;
	virtual	void		AddDBTSlaveBuff	(DBTSlaveBuffered*	pDbt, RecordArray* pSelections = NULL) ;
	virtual	void		AddCustomData	();

	virtual	void		AddRecord			(SqlRecord*			pRecord);
	virtual	BOOL		SkipField			(SqlRecord*			pRecord, int nIdx) { return FALSE; }
	virtual	void		AddRecordExtraInfo	(SqlRecord*			pRecord);
	virtual	BOOL		ParseRecordExtraInfo(CTBEDataCoDecRecord& rec);
	virtual	BOOL		ParseClassesInfo	();
	virtual	BOOL		UnParseClassesInfo	();

	virtual	BOOL		AddSession			(CGuid);
public:
	virtual	CGuid		GetSession			();
protected:
			void		AddRecordFields		(SqlRecord*			pRecord);

			// da SqlDBLoader
			void		UnParseTDTDataElement	(SqlRecord* pRecord, int nIdx);

			Unparser*	GetUnParser		() const {return m_pUnparser;}
			Parser*		GetParser		() const {return m_pParser;}

			void		IncIndent		() {m_nIndent++;}
			void		DecIndent		() {m_nIndent--;}
			void		UnParseIndent	();
			void		BuildFileName	(BOOL bForce = FALSE);

			// parsing
			void		ParseDocument			();
			int			ParseDBTMaster			();
			int			ParseDBTSlave			();
			int			ParseDBTSlaveBuffered	(CTBEDataCoDecDBT**	ppDBT = NULL);
			int			ParseFields				(CStringArray& fld_order, CTBEDataCoDecRecord& rec);
			int			ParseRecord				(CStringArray& fld_order, CTBEDataCoDecRecord& rec);
			BOOL		ParseTDTDataElement		(LPCTSTR fld_name, 	CTBEDataCoDecRecord& rec);

};

//=============================================================================
#include "endh.dex"

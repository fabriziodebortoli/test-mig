#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

class CTBNamespace;
class CPathFinder;
class Unparser;
class Parser;
class CStatusBarMsg;

///////////////////////////////////////////////////////////////////////////////
//			class FormatsParser definition
///////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT FormatsParser
{   
protected:
	int		m_nRelease;
	BOOL	m_bParsing;	

private:
	Formatter::FormatStyleSource	m_Source;
	CTBNamespace					m_Namespace;
	FormatStyleTable*				m_pTable;

public:
	FormatsParser	();

public:
	BOOL IsParsing	() const	{ return m_bParsing; }
	
	// caricamento dai files 
	BOOL LoadFormats(
						FormatStyleTable*			pTable,
						const	CTBNamespace&		aModule, 
						const	CPathFinder*		pPathFinder,
								CStatusBarMsg*		pStatusBar,
								CPathFinder::PosType posType
					);
	BOOL SaveFormats(
						const	CTBNamespace&	aModule, 
						const	CPathFinder*	pPathFinder, 
						const	BOOL&			bSaveStandards
					);

	BOOL RefreshFormats	(
							FormatStyleTablePtr	pTable,
							const CTBNamespace&	aModule, 
							const CPathFinder*	pPathFinder
						);

	// per Woorm
	BOOL Parse		(
						const CTBNamespace&					aModule, 
						const Formatter::FormatStyleSource	aSource,
						FormatStyleTable*					pTable, 
						Parser&								lex
					);
	void Unparse	(
						const CTBNamespace&					aModule, 
						const Formatter::FormatStyleSource	aSource,
						FormatStyleTablePtr					pTable, 
						Unparser&							ofile
					);

private:
	// funzioni comuni a tutti i tipi
	void	UnparseFormatStyle		(Unparser&	ofile, Formatter* pFormatter);
	BOOL	UnparseFormatsStyles	(Unparser&);
	void	UnparseFormatter		(Unparser& ofile, Formatter* pFormatter);
	void	UnparseFmtCommon		(Unparser& ofile, Formatter* pFormatter);
	void	UnparseFmtAlign			(Unparser&, int, Formatter* pFormatter);

	BOOL	ParseFmtType			(Parser& lex, DataType& aType, CString& stylename);
	BOOL	ParseStyle				(Parser& ofile);
	BOOL	ParseStyles				(Parser& ofile);
	BOOL	ParseBlock				(Parser& ofile);
	BOOL	ParseFormatter			(Parser& lex, Formatter* pFormatter);
	BOOL	ParseFmtAlign			(Parser& lex, Formatter* pFormatter);
	BOOL	ParseFmtCommon			(Parser& lex, Formatter* pFormatter);

	// funzioni specializzate per tipologia di dato
	BOOL	ParseFmtVariable		(Parser& lex, Formatter* pFormatter);
	void	UnparseFmtVariable		(Unparser& ofile, Formatter* pFormatter);

	// per i double
	BOOL	ParseDoubleData			(Parser& lex, Formatter* pFormatter);
	BOOL	ParseFmtRound			(Parser& lex, Formatter* pFormatter);
	BOOL	ParseFmtDecimal			(Parser& lex, Formatter* pFormatter);
	BOOL	ParseDoubleFmtSep		(Parser& lex, Formatter* pFormatter);
	BOOL	ParseDoubleDataStyle	(Parser& lex, Formatter* pFormatter);

	void	UnparseDoubleData		(Unparser& ofile, Formatter* pFormatter);
	void	UnparseFmtRound			(Unparser& ofile, Formatter* pFormatter);
	void	UnparseFmtDecimal		(Unparser& ofile, Formatter* pFormatter);
	void	UnparseDoubleFmtSep		(Unparser& ofile, Formatter* pFormatter);
	void	UnparseDoubleDataStyle	(Unparser& ofile, Formatter* pFormatter);

	// per i bool
	void	UnparseBoolData			(Unparser& ofile,	Formatter* pFormatter);
	void	UnparseFmtBoolString	(Unparser& ofile,	Formatter* pFormatter);

	BOOL	ParseBoolData			(Parser& lex,	Formatter* pFormatter);
	BOOL	ParseFmtBoolString		(Parser& lex,	Formatter* pFormatter);

	// per le date e tempi
	void	UnparseDateData		(Unparser& ofile,	Formatter* pFormatter);
	void	UnparseFmtOrder		(Unparser& ofile,	Formatter* pFormatter);
	void	UnparseFmtWeekDay	(Unparser& ofile,	Formatter* pFormatter);
	void	UnparseFmtDayFmt	(Unparser& ofile,	Formatter* pFormatter);
	void	UnparseFmtMonthFmt	(Unparser& ofile,	Formatter* pFormatter);
	void	UnparseFmtYearFmt	(Unparser& ofile,	Formatter* pFormatter);
	void	UnparseFmtFirstSep	(Unparser& ofile,	Formatter* pFormatter);
	void	UnparseFmtSecondSep	(Unparser& ofile,	Formatter* pFormatter);
	void	UnparseDateTimeFmt	(Unparser& ofile,	Formatter* pFormatter);
	void	UnparseFmtTimeSep	(Unparser& ofile,	Formatter* pFormatter);
	void	UnparseFmtTimeAMPM	(Unparser& ofile,	Formatter* pFormatter);

	BOOL	ParseDateData		(Parser& lex,	Formatter* pFormatter);
	BOOL	ParseFmtOrder		(Parser& lex,	Formatter* pFormatter);
	BOOL	ParseFmtWeekDay		(Parser& lex,	Formatter* pFormatter);
	BOOL	ParseFmtDayFmt		(Parser& lex,	Formatter* pFormatter);
	BOOL	ParseFmtMonthFmt	(Parser& lex,	Formatter* pFormatter);
	BOOL	ParseFmtYearFmt		(Parser& lex,	Formatter* pFormatter);
	BOOL	ParseFmtFirstSep	(Parser& lex,	Formatter* pFormatter);
	BOOL	ParseFmtSecondSep	(Parser& lex,	Formatter* pFormatter);
	BOOL	ParseDateTimeFmt	(Parser& lex,	Formatter* pFormatter);
	BOOL	ParseFmtTimeSep		(Parser& lex,	Formatter* pFormatter);
	BOOL	ParseFmtTimeAMPM	(Parser& lex,	Formatter* pFormatter);

	// per gli elapsed times
	void	UnparseElapsedTimeData	(Unparser& ofile,	Formatter* pFormatter);
	void	UnparseFmtTimeDecimal	(Unparser& ofile,	Formatter* pFormatter);
	void	UnparseFmtTimePrompt	(Unparser& ofile,	Formatter* pFormatter);
	void	UnparseElapsedTimeFmt	(Unparser& ofile,	Formatter* pFormatter);

	BOOL	ParseElapsedTimeData	(Parser& lex,	Formatter* pFormatter);
	BOOL	ParseFmtTimeDecimal		(Parser& lex,	Formatter* pFormatter);
	BOOL	ParseFmtTimePrompt		(Parser& lex,	Formatter* pFormatter);
	BOOL	ParseElapsedTimeFmt		(Parser& lex,	Formatter* pFormatter);

	// per i numerici in generale
	void	UnparseFmtSign			(Unparser& ofile, Formatter* pFormatter);
	void	UnparseFmtTable			(Unparser& ofile, Formatter* pFormatter);
	void	UnparseFmtThousand		(Unparser& ofile, Formatter* pFormatter);

	BOOL	ParseFmtSign			(Parser& lex, Formatter* pFormatter);
	BOOL	ParseFmtTable			(Parser& lex, Formatter* pFormatter);
	BOOL	ParseFmtThousand		(Parser& lex, Formatter* pFormatter);

	// per i long
	void	UnparseLongData			(Unparser& ofile,	Formatter* pFormatter);
	void	UnparseLongDataStyle	(Unparser& ofile,	Formatter* pFormatter);

	BOOL	ParseLongData			(Parser& lex, Formatter* pFormatter);
	BOOL	ParseLongDataStyle		(Parser& lex, Formatter* pFormatter);

	// stringhe
	void	UnparseStringData		(Unparser& ofile, Formatter* pFormatter);
	void	UnparseStringDataStyle	(Unparser& ofile, Formatter* pFormatter);

	BOOL	ParseStringData			(Parser& lex, Formatter* pFormatter);
	BOOL	ParseStringDataStyle	(Parser& lex, Formatter* pFormatter);

	// per gli enums
	void	UnparseEnumData			(Unparser& ofile,	Formatter* pFormatter);
	void	UnparseEnumDataStyle	(Unparser& ofile,	Formatter* pFormatter);

	BOOL	ParseEnumData			(Parser& lex, Formatter* pFormatter);
	BOOL	ParseEnumDataStyle		(Parser& lex, Formatter* pFormatter);

	// metodi di controllo
	BOOL MustBeRefreshed	(
								FormatStyleTablePtr	pTable,
								const CTBNamespace&	aModule, 
								const CPathFinder*	pPathFinder
							);
};

#include "endh.dex"

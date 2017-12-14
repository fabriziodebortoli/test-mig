#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

class CTBNamespace;
class CPathFinder;
class Unparser;
class Parser;
class FontStyleSource;
class FontStyleTable;
class FontStyle;
class CStatusBarMsg;
//=============================================================================
class TB_EXPORT FontsParser
{ 
private:
	int							m_nRelease;
	FontStyle::FontStyleSource	m_Source;

public:
	FontsParser ();

	BOOL LoadFonts		(
							FontStyleTable*			pTable,
							const	CTBNamespace&	aModule, 
							const	CPathFinder*	pPathFinder,
									CStatusBarMsg*	pStatusBar,
									CPathFinder::PosType posType
						);
	BOOL LoadFontAlias(FontStyleTable* pTable);
	BOOL SaveFonts		(
							const	CTBNamespace&	aModule, 
							const	CPathFinder*	pPathFinder
						);

	BOOL RefreshFonts	(
							FontStyleTablePtr	pTable,
							const CTBNamespace&	aModule, 
							const CPathFinder*	pPathFinder
						);

	// usati da woorm per gestire la sua tabella dei fonts
	BOOL Parse		(
						const	CTBNamespace&				aModule, 
						const	FontStyle::FontStyleSource	aSource,
								FontStyleTable*				pFontTable, 
								Parser&						lex
					);
	void Unparse	(
						const	CTBNamespace&				aModule, 
						const	FontStyle::FontStyleSource	aSource,
								FontStyleTablePtr			pFontTable, 
								Unparser&					ofile
					);
private:
	// lettura
	BOOL ParseBlock			(
								const CTBNamespace& aModule,
								FontStyleTable*	pFontTable, 
								Parser& lex
							);
	BOOL ParseStyles		(
								const CTBNamespace& aModule,
								FontStyleTable*	pFontTable,
								Parser&	lex
							);
	BOOL ParseStyle			(
								const CTBNamespace& aModule,
								FontStyleTable*	pFontTable,
								Parser& lex
							);
	BOOL ParseStyleOption	(Parser& lex, FontStyle*);
	BOOL ParseLogFont		(Parser& lex, FontStyle* pStyle);

	// scrittura
	BOOL UnparseStyles		(
										CTBNamespace				Ns,
										FontStyleTablePtr			pFontTable, 
										Unparser&					oFile
							)	const;
	void UnparseStyle		(Unparser&, FontStyle* style)	const;
	void UnparseStyleOption	(Unparser&, FontStyle* style)	const;


	// metodi di controllo
	BOOL MustBeRefreshed	(
								FontStyleTablePtr		pTable,
								const CTBNamespace&	aModule, 
								const CPathFinder*	pPathFinder
							);
};

#include "endh.dex"

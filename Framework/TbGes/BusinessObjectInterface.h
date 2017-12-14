
#pragma once

#include <TbGeneric\Array.h>
#include <TbGeneric\DataObj.h>
#include <TbOleDb\SqlRec.h>

#include  "beginh.dex"

//	BusinessObjectInterface (BOI)
//
//	La classe BusinessObjectInterface permette di definire l'interfaccia tra due componenti
//	[ ... continua in fondo]

class CBusinessObjectInterface;

//=============================================================================
class TB_EXPORT CInterfaceItem : public CObject
{
	friend class CBusinessObjectInterface;
	DECLARE_DYNAMIC(CInterfaceItem)

public:
	CInterfaceItem
		(			
			DataObj**	pInterfaceDataPtr, 
			BOOL		bOptional
		);

public:
	void	Attach		(DataObj* pAttachedDatePtr);
	void	Attach		(SqlRecord* pRec, DataObj* pDataObj);
	void	Detach		();
	void	Bind		(SqlRecord* pRec);
	void	Unbind		();

	BOOL	IsEqual		(DataObj** pInterfaceDataPtr);
	BOOL	Check		();
	BOOL	IsAttached	();
			
private:
	DataObj**		m_pInterfaceDatePtr = NULL;
	int				m_nAttachedDateIdx = -1;
	CRuntimeClass*	m_pSqlRecordClass = NULL;
	DataObj*		m_pAttachedDatePtr = NULL;
	BOOL			m_bOptional;

public:
// Diagnostics
#ifdef _DEBUG
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const;
#endif // _DEBUG
};

typedef	CTypedPtrArray	<Array,		CInterfaceItem*>		InterfaceItemsArray;

class DBTObject;

//=============================================================================
class TB_EXPORT CBusinessObjectInterfaceObj : public CObject
{
	DECLARE_DYNAMIC(CBusinessObjectInterfaceObj)

public:
	CBusinessObjectInterfaceObj(DBTObject* pDBT = NULL);
	~CBusinessObjectInterfaceObj();

protected:
	// metodi da usarsi nel primo livello di derivazione per definire gli item di interfaccia
	void	AddItem
			(			
				DataObj**	pInterfaceDataPtr, 
				BOOL		bOptional
			);
	void	RemoveAllItems	(); // svuota completamente l'interfaccia rimuovendo tutti gli item

	// va implementata e richiamata nel costruttore del primo livello di derivazione. e' virtuale pura solo
	// per ricordare che e' necessario implementarla per definire un'interfaccia valida
	virtual void Build	()	= 0;
public:
	// metodi da usarsi nell'"implementazione" dell'interfaccia (secondo livello di derivazione)
	// per mettere in corrispondenza i DataObj che verranno usati a runtime con gli item di interfaccia
	void	AttachDataObj	
			(
				DataObj**	pInterfaceDataPtr, 
				DataObj*	pAttachedDatePtr
			);
	void	AttachDataObj	
			(
				DataObj**	pInterfaceDataPtr, 
				SqlRecord*	pRec, 
				DataObj*	pDataObj
			);
	// scollega gli item di interfaccia dai dataobj corrispondenti
	void	DetachAllDataObjs	();								
	
	// permettono di sapere se il client ha attachato un item opzionale
	BOOL	IsAttached		(DataObj**		pInterfaceDataPtr); 
	// queste overload servono ad evitare cast strani durante l'uso comune
	BOOL	IsAttached		(DataStr**		pDataStrPtr)		{ return IsAttached((DataObj**)pDataStrPtr);	}
	BOOL	IsAttached		(DataInt**		pDataIntPtr)		{ return IsAttached((DataObj**)pDataIntPtr);	}
	BOOL	IsAttached		(DataLng**		pDataLngPtr)		{ return IsAttached((DataObj**)pDataLngPtr);	} 
	BOOL	IsAttached		(DataDate**		pDataDatePtr)		{ return IsAttached((DataObj**)pDataDatePtr);	} 
	BOOL	IsAttached		(DataMon**		pDataMonPtr)		{ return IsAttached((DataObj**)pDataMonPtr);	} 
	BOOL	IsAttached		(DataQty**		pDataQtyPtr)		{ return IsAttached((DataObj**)pDataQtyPtr);	} 
	BOOL	IsAttached		(DataDbl**		pDataDblPtr)		{ return IsAttached((DataObj**)pDataDblPtr);	} 
	BOOL	IsAttached		(DataPerc**		pDataPercPtr)		{ return IsAttached((DataObj**)pDataPercPtr);	} 
	BOOL	IsAttached		(DataEnum**		pDataEnumPtr)		{ return IsAttached((DataObj**)pDataEnumPtr);	}	 
	BOOL	IsAttached		(DataBool**		pDataBoolPtr)		{ return IsAttached((DataObj**)pDataBoolPtr);	}

	// va implementata e richiamata nel costruttore del secondo livello di derivazione. e' virtuale pura solo
	// per ricordare che e' necessario implementarla per definire un'interfaccia valida
	virtual void	Attach	()	= 0;
	// verifica che tutti gli item di interfaccia definiti obbligatori siano stati effettivamente attachati
	// Lo stesso tipo di verifica scatta (con ASSERT) anche ad ogni bind
			BOOL	Check	();

	// Metodi da usarsi al momento dell'uso dell'interfaccia per bindare le effettive istanze
	// dei dataobj agli item di interfaccia
	//------------------------------------------------------------------------------------------
	// Binda un generico record, se il BOI ha un DBT deve essere dello 
	// stesso tipo legato al DBT passato nel costruttore
	void	BindDBT		(SqlRecord* pRec);
	// Binda il record legato al DBT passato nel costruttore. Solo DBT master o slave. Se no DBT o DBT SB, ASSERT
	void	BindDBT		(BOOL bOld = FALSE);					
	// Binda la riga i-esima del DBT SLaveBuffered passato nel costruttore. Se no DBT o DBT no SB, ASSERT
	void	BindLine	(int i, BOOL bOld = FALSE);
	// Rimuove il binding tra gli item di interfaccia e i dataobj attachati
	void	UnbindAll	(); 
	// Permette di verificare se il DBT passato e' lo stesso su cui e' costruita l'interfaccia, per effettuare o no il binding.
	// Se no DBT, ASSERT
	BOOL	SameDBT		(DBTObject* pDBT);

	// Metodi di manipolazione dei dati attachati all'interfaccia, specifici per DBTSlaveBuffered
	// Se non c'e' DBT o non e' SB, ASSERT
	//------------------------------------------------------------------------------------------
	int			GetUpperBound	(BOOL bOld = FALSE);		// Torna l'[old]upper bound del DBT
	int			GetCurrentRowIdx();							// Torna l'indice della riga corrente. ATTENZIONE! e' valido solo se il DBT e' correntemente utilizzato da un BE "vivo"
	void		SetCurrentRow	(int i);					// Imposta l'indice della riga corrente. ATTENZIONE! Funziona solo se il DBT e' correntemente utilizzato da un BE "vivo"
	BOOL		IsStorable		(int i);					// Dice se la riga i-esima del DBT e' storable
	void		SetStorable		(int i, bool bSet = TRUE);	// Assegna il valore di storable alla riga i-esima
	SqlRecord*	AddRecord		();							// Aggiunge un record in fondo al DBT
	void		DeleteRecord	(int i);					// Cancella la riga i-esima del DBT. Dopo la cancellazione l'interfaccia e' unbind
	void		RemoveAll		();							// Svuota completamente il DBT. Dopo l'interfaccia e' unbind
	//todoporting serve al lottimanager il SqlRecord come valore di ritorno
	SqlRecord*	InsertRecord	(int i);					// Inserisce un record alla riga i-esima del DBT
	//todoporting servono al matricolemanager
	SqlRecord*	GetRow			(int i);					// Torna il SqlRecord della riga i-esima del DBT
	SqlRecord*	GetOldRow		(int i);					// Torna l'Old SqlRecord della riga i-esima del DBT
	int			GetSize			();							// Torna la size del DBT 
	int			GetOldSize		();							// Torna la old size del DBT 
	DBTObject*	GetDBT			() { return m_pDBT; }
	void		SetDBT			(DBTObject* pDBT) { m_pDBT = pDBT; }

private:
	CInterfaceItem*	LookUp(DataObj** pInterfaceDataPtr);

private:
	InterfaceItemsArray		m_Items;
	DBTObject*				m_pDBT = NULL;

public:
// Diagnostics
#ifdef _DEBUG
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const;
#endif // _DEBUG
};

// per chiarezza durante la creazione degli item per specificare se sono opzionali o meno
#define BOI_MANDATORY	FALSE
#define BOI_OPTIONAL	TRUE

#define BOI_ADD_ITEM(pIDataPtr,bOptional)				AddItem((DataObj**)&pIDataPtr, bOptional)
#define BOI_ATTACH(pIDataPtr,ToDate)					AttachDataObj((DataObj**)&pIDataPtr,&ToDate)
#define BOI_ATTACH_FIELD(pIDataPtr,pRec,ToDate)			AttachDataObj((DataObj**)&pIDataPtr,pRec,&(pRec->ToDate))
#define BOI_ATTACH_ADDON_FIELD(pIDataPtr,pRec,ToDate)	AttachDataObj((DataObj**)&pIDataPtr,pRec,&ToDate)

#include "endh.dex"

/*	
	BusinessObjectInterface (BOI)
	-----------------------------

	La classe BusinessObjectInterface permette di definire l'interfaccia tra due componenti. 
	L'interfaccia serve a mettere in collegamento un componente fornitore di servizi (provider) con
	un componente che richiede tali servizi (client). Tramite l'interfaccia il provider manipola
	il client (ad es.: ne valorizza dei datamember) senza conoscerne l'effettiva struttura. In questo
	senso il client "espone" l'interfaccia richiesta dal provider.

	La BOI e' costituita prevalentemente da dati, volendo anche da metodi. I dati sono DataObj*, sia indipendenti
	che colonne di un SqlRecord. Nella pratica, il client espone tramite la BOI i propri DataObj e/o SqlRecord (DBT),
	in modo che il provider possa leggerli e/o valorizzarli

	La definizione e l'uso della BOI prevede tre step:

	1)	Il provider definisce la BOI attraverso la quale puo' fornire servizi, derivando da BusinesObjectInterface
		e dichiarando dei datamember DataObj*. In un metodo Build, da richiamarsi nel costruttore, inserisce tali 
		datamember nell'interfaccia, tramite il metodo AddItem o la macro BOI_ADD_ITEM
		Opzionalmente puo' anche dichiarare metodi pure virtual che chi implementa l'interfaccia dovra' fornire

	2)	Il client implementa la BOI, derivando da BOI<provider> e creando la corrispondenza tra i propri DataObj e
		i puntatori della BOI parent. In  un metodo Attach, da richiamarsi nel costruttore, collega con i metodi 
		AttachDataObj o le macro BOI_ATTACH i propri DataObj e/o SqlRecord (DBT). Se la BOI parent prevede metodi 
		virtuali puri, li implementa.

	3)	A runtime, il provider accede ai DataObj del client per tramite dei propri DataObj*, preventivamente
		effettuando una Bind. La Bind "ricollega" i puntatori su una particolare istanza di un SqlRecord
		(ad es.: la riga corrente di un DBT SB)

	Sono previste queste possibilita':

	-	Un DataObj* di interfaccia puo' essere dichiarato "optional" o "mandatory" dal provider (durante la AddItem). 
		Se un membro di interfaccia e' "optional", il provider dichiara di accettare che non venga implementato dal 
		client, ed e' quindi in grado di gestire la sua mancanza (il puntatore resta a NULL). Se e' "mandatory", il 
		client e' obbligato a reimplementarlo (ASSERT durante la Bind). Il client puo' anche reimplementarlo con un
		dato valorizzato a default, se non ha nulla da fornire come interfaccia. Il comportamento in questo caso 
		dipende esclusivamente dall'uso che ne fa il provider.

	-	L'Attach puo' essere fatta direttemente con un DataObj indipendente o con una colonna di SqlRecord. In 
		questo caso si mantiene traccia dell'indice della colonna, in modo da potere a runtime effettuare la
		bind con un'altra istanza dello stesso SqlRecord. 

	-	Il BOI puo' essere costruito collegato ad un DBT. In questo caso il binding puo' essere fatto con l'istanza
		di SqlRecord contenuta nel DBT (se master o slave) o con la i-esima riga (se SB). 

	NOTA: il binding con DataObj indipendenti (ossia non colonne di SqlRecord) non viene mai "mosso" dalle Bind
	dopo la Attach
*/
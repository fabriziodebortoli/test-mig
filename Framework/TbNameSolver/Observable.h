#pragma once

/*
Queste classi implementano il design pattern OBSERVER, nel quale un oggetto osservato (SUBJECT)
mantiene una lista di oggetti osservatori (OBSERVERS) a cui comunica il cambiamento di stato
*/
//#include "array.h"
//includere alla fine degli include del .H
#include "beginh.dex"



class CObservable;
class CDataEventsObj;

typedef TB_EXPORT CMap<CObservable*, CObservable*, CDataEventsObj*, CDataEventsObj*> ObservableMap;


enum TB_EXPORT EventType { ON_CHANGING, ON_CHANGED };

class TB_EXPORT CObserverResults : public CObject
{
public:
	EventType	m_eEventType;
	ObservableMap   m_ObservablesMap;

public:
	CObserverResults(EventType eObsType)
		:
	m_eEventType(eObsType)
	{}
};

//contesto di osservazione: ogni classe di evento fornisce il proprio contesto
//di osservazione; col metodo Signal registra nel contesto che l'evento è
//pronto per essere scatenato (ma non lo scatena);
//il metodo fire del contesto scatena tutti gli eventi che sono stati segnalati
class TB_EXPORT CObserverContext
{
	friend class CDataEventsObj;

public:
	~CObserverContext();

private:
	/*Array*/CObArray	m_arObservableMaps;

	void Add(CObservable* pObservable, CDataEventsObj* pDataEvents, EventType eType);
	int  GetObservableMapIndex(EventType eType) const;

public:
	bool IsObserving(EventType eType) const;
	void StartObserving(EventType eType);
	void Fire(EventType eType);
	void EndObserving(EventType eType);

	ObservableMap* GetObservableMap(EventType eType) const;
};

///classe astratta per la gestione degli eventi del dataobj
class TB_EXPORT CDataEventsObj 
{
	
friend class CObservable;
	
private:
	CDataEventsObj* m_pParent;

protected:
	bool			m_bOwned;

	void SetParent(CDataEventsObj* pParent) 
	{
#ifdef DEBUG	//controllo di non ricorsivita'
		CDataEventsObj* p = m_pParent;
		while (p)
		{
			ASSERT(p != pParent);
			p = p->GetParent();
		}
#endif
		m_pParent = pParent; 
	}
public:
	CDataEventsObj* GetParent() const { return m_pParent; }

	CDataEventsObj() 
	: 
		m_pParent	(NULL),
		m_bOwned	(false)
	{ }
	virtual ~CDataEventsObj() {  }
	
	//metodo astratto per scatenare l'evento 
	virtual void Fire(CObservable* pSender, EventType eType) = 0;
	virtual CObserverContext* GetContext() const = 0;
	virtual void OnDeletingObservable(CObservable* pSender) {}
	
	//metodo per segnalare che l'evento può essere scatenato
	virtual void Signal(CObservable* pSender, EventType eType);
};

//=============================================================================
//			Class CDataEventsProxy
//=============================================================================
class TB_EXPORT CDataEventsProxy : public CDataEventsObj
{
	DECLARE_DYNAMIC(CDataEventsProxy)
	CObservable* m_pTarget;
public:
	CDataEventsProxy(CObservable* pTarget) 
	: 
		m_pTarget (pTarget)
	{
		m_bOwned = true;
	}
	virtual void Fire(CObservable* pSender, EventType eType);
	virtual void Signal(CObservable* pSender, EventType eType);
	virtual CObserverContext* GetContext() const;
};

//classe da cui derivare gli oggetti soggetto dell'osservazione (es. il DataObj)
class TB_EXPORT CObservable
{
	friend class CDataEventsProxy;

	CDataEventsObj *m_pDataEvents;
	
public:
	CObservable(void);
	~CObservable(void);

	void AttachEvents				(CDataEventsObj *pEvents);
	void DetachEvents				(CDataEventsObj *pEvents);
	
	void Fire						(EventType eType);	
	void Signal						(EventType eType);	
	const CDataEventsObj* GetDataEvents() { return m_pDataEvents; }
};


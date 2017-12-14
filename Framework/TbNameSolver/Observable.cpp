#include "StdAfx.h"
#include "Observable.h"

//si aggiunge al proprio contesto come evento 'pronto per essere scatenato'
void CDataEventsObj::Signal(CObservable* pSender, EventType eType)
{
	CObserverContext* p = GetContext();
	if (p)
		p->Add(pSender, this, eType);
}

IMPLEMENT_DYNAMIC(CDataEventsProxy, CObject)
//-----------------------------------------------------------------------------
void CDataEventsProxy::Signal(CObservable* pSender, EventType eType)
{
	CDataEventsObj *pEvents = m_pTarget->m_pDataEvents;
	//risalgo tutta la catena dei parent e scateno l'evento
	while (pEvents) 
	{
		pEvents->Signal(pSender, eType); 
		pEvents = pEvents->GetParent();
	}
}
//-----------------------------------------------------------------------------
void CDataEventsProxy::Fire(CObservable* pSender, EventType eType)
{
	CDataEventsObj *pEvents = m_pTarget->m_pDataEvents;
	//risalgo tutta la catena dei parent e scateno l'evento
	while (pEvents) 
	{
		pEvents->Fire(pSender, eType); 
		pEvents = pEvents->GetParent();
	}
}

//-----------------------------------------------------------------------------
CObserverContext* CDataEventsProxy::GetContext() const
{
	return m_pTarget->GetDataEvents() ? m_pTarget->GetDataEvents()->GetContext() : NULL; 
}
//-----------------------------------------------------------------------------
CObservable::CObservable(void)
	:
	m_pDataEvents(NULL)
{
}

//-----------------------------------------------------------------------------
CObservable::~CObservable(void)
{
	CDataEventsObj *pEvents = m_pDataEvents;
	CDataEventsObj *pParent = NULL;
	//risalgo tutta la catena dei parent e segnalo che sto morendo
	while (pEvents) 
	{
		pEvents->OnDeletingObservable(this); 
		pParent = pEvents->GetParent();
		if (pEvents->m_bOwned)
		{
			DetachEvents(pEvents);
			delete pEvents;
		}
		
		pEvents = pParent;
	}
}
//aggancia gli eventi all'Observable; se ne esistono, si inserisce in una catena
//-----------------------------------------------------------------------------
void CObservable::AttachEvents (CDataEventsObj *pEvents)
{
	if (!pEvents)
	{
		return;
	}
	//se ho già un listener, questo diventa il papà di quello che sto aggancianto, quest'ultimo
	//diventa il listener corrente (per evitare di avere una array che appesantisce il dataobj
	if (m_pDataEvents)
		pEvents->SetParent(m_pDataEvents);
	
	m_pDataEvents = pEvents;
}

//sgancia gli eventi dall'Observable
//-----------------------------------------------------------------------------
void CObservable::DetachEvents (CDataEventsObj *pEvents)
{
	if (!m_pDataEvents)
		return;

	//ho una sequenza gerarchica: m_pDataEvents è la foglia,
	//se il nodo da rimuovere è m_pDataEvents, imposto la nuova foglia uguale al suo parent 
	if (pEvents == m_pDataEvents)
	{
		m_pDataEvents = m_pDataEvents->GetParent();
		pEvents->SetParent(NULL);
		return;
	}

	//in caso contrario, devo risalire la catena dei parent, rimuovere il papà e impostare
	//il nonno come parent del figlio
	CDataEventsObj *pFather = m_pDataEvents->GetParent();
	//l'oggetto della precedente iterazione inizialmente è nullo
	CDataEventsObj *pSon = m_pDataEvents;
	while (pFather)
	{
		//se il puntatore è quello buono, lo rimuovo ed esco
		if (pFather == pEvents)
		{
			//se ho un oggetto dalla precedente iterazione, significa che
			//sto rimuovendo il papà nella catena nonno-papà-figlio (pOldDataEvent è il figlio)
			if (pSon)
			{
				CDataEventsObj *pGrandFather = pFather->GetParent();
				pFather->SetParent(NULL);
				pSon->SetParent(pGrandFather);
				
			}
			break;
		}

		//altrimenti salgo su di un colpo
		pSon = pFather;
		pFather = pFather->GetParent();
	}
	
}

//-----------------------------------------------------------------------------
//scatena gli eventi per l'observable
void CObservable::Fire(EventType eType)
{
	CDataEventsObj *pEvents = m_pDataEvents;
	//risalgo tutta la catena dei parent e scateno l'evento
	while (pEvents) 
	{
		pEvents->Fire(this, eType); 
		pEvents = pEvents->GetParent();
	}
}	


//-----------------------------------------------------------------------------
//segnala che l'evento può essere scatenato
void CObservable::Signal(EventType eType)
{
	CDataEventsObj *pEvents = m_pDataEvents;
	//risalgo tutta la catena dei parent e scateno l'evento
	while (pEvents) 
	{
		pEvents->Signal(this, eType); 
		pEvents = pEvents->GetParent();
	}
}

//=============================================================================
// CObsserverContext definition
//=============================================================================

//-----------------------------------------------------------------------------
CObserverContext::~CObserverContext()
{
	// cleanup of the Observable array
	CObject* pO;
	for (int i = 0; i < m_arObservableMaps.GetSize(); i++) 
	{
		if (pO = m_arObservableMaps.GetAt(i)) 
			delete pO;
	}
	m_arObservableMaps.RemoveAll();
}

//-----------------------------------------------------------------------------
void CObserverContext::Add(CObservable* pObservable, CDataEventsObj* pDataEvents, EventType eType)
{
	ObservableMap* pObservableMap = GetObservableMap(eType);
	if (!pObservableMap)
		return;

	CDataEventsObj* pEvents;
	if (pObservableMap->Lookup(pObservable, pEvents))
		return;
	
	(*pObservableMap)[pObservable] = pDataEvents;
}

//mi faccio restituire la mappa del tipo passato
//-----------------------------------------------------------------------------
ObservableMap* CObserverContext::GetObservableMap(EventType eType) const
{
	CObserverResults* pObserverResult = NULL;
	for (int i = 0; i < m_arObservableMaps.GetSize(); i++)
	{
		pObserverResult = (CObserverResults*)m_arObservableMaps.GetAt(i);
		if (pObserverResult->m_eEventType ==  eType)
			return &pObserverResult->m_ObservablesMap;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
int CObserverContext::GetObservableMapIndex(EventType eType) const
{
	CObserverResults* pObserverResult = NULL;
	for (int i = 0; i < m_arObservableMaps.GetSize(); i++)
	{
		pObserverResult = (CObserverResults*)m_arObservableMaps.GetAt(i);
		if (pObserverResult->m_eEventType ==  eType)
			return i;
	}

	return -1;
}

//-----------------------------------------------------------------------------
bool CObserverContext::IsObserving(EventType eType) const 
{
	int nIndex = GetObservableMapIndex(eType);
	return (nIndex > -1);  
}

//-----------------------------------------------------------------------------
//alloca l'array degli oggetti 'segnalati'
void CObserverContext::StartObserving(EventType eType)
{
	int nIndex = GetObservableMapIndex(eType);
	if (nIndex > -1)
	{
		ASSERT(FALSE); //non è stato chiamato EndObserving?
		CObject* pO = m_arObservableMaps.GetAt(nIndex);
		m_arObservableMaps.RemoveAt(nIndex);
		if (pO) delete pO;
	}
	m_arObservableMaps.Add(new CObserverResults(eType));
}

//-----------------------------------------------------------------------------
//scatena gli eventi per tutti gli Observable che hanno segnalato un cambiamento
void CObserverContext::Fire(EventType eType)
{
	ObservableMap* pObservableMap = GetObservableMap(eType);
	if (!pObservableMap)
		return;

	POSITION pos = pObservableMap->GetStartPosition();
	CObservable* pObservable;
	while (pos)
	{
		CDataEventsObj* pEvents;
		pObservableMap->GetNextAssoc(pos, pObservable, pEvents);
		pObservable->Fire(eType);
	}
}
//-----------------------------------------------------------------------------
void CObserverContext::EndObserving(EventType eType)
{	
	int nIndex = GetObservableMapIndex(eType);
	if (nIndex > -1)
	{
		CObject* pO = m_arObservableMaps.GetAt(nIndex);
		m_arObservableMaps.RemoveAt(nIndex);
		if (pO) delete pO;
	}
	else
		ASSERT(FALSE); //non è stato chiamato StartObserving?
}
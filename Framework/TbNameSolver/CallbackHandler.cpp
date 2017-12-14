#include "StdAfx.h"
#include "CallbackHandler.h"

CCallbackHandler::CCallbackHandler(void)
{
} 


CCallbackHandler::~CCallbackHandler(void)
{

#ifdef DEBUG
	TB_LOCK_FOR_READ();
	ASSERT(m_arOnDisposingListeners.GetCount() == 0);
	ASSERT(m_arOnDisposing.GetCount() == 0);
#endif
}

void CCallbackHandler::AddDisposingHandler (CObject* pListener, ON_DISPOSING_METHOD pHandler)
{
	TB_LOCK_FOR_WRITE();
	m_arOnDisposingListeners.Add(pListener); 
	m_arOnDisposing.Add(pHandler); 
}

void CCallbackHandler::RemoveDisposingHandlers (CObject* pListener)
{
	TB_LOCK_FOR_WRITE();
	for (int i = m_arOnDisposingListeners.GetUpperBound(); i >= 0; i--)
	{
		CObject* pCurrListener = m_arOnDisposingListeners[i];
		//ON_DISPOSING_METHOD pCurrHandler = m_arOnDisposing[i];
		if (pCurrListener == pListener /*è diverso fra add e remove && pCurrHandler == pHandler*/)
		{
			m_arOnDisposingListeners.RemoveAt(i);
			m_arOnDisposing.RemoveAt(i);
		}
	}
}

void CCallbackHandler::FireDisposing(CObject* pSender)
{
	ASSERT_VALID(pSender);

	TB_LOCK_FOR_WRITE();

	//faccio a rovescia perché la OnDisposing mi mette a NULL il puntatore del listener e me li rimuove dalla lista
	for (int i = m_arOnDisposing.GetUpperBound(); i >= 0; i--)
	{
		CObject* pCurrListener = m_arOnDisposingListeners[i];
		ASSERT_VALID(pCurrListener);

		ON_DISPOSING_METHOD pCurrHandler = m_arOnDisposing[i];
		ASSERT(pCurrHandler);

		(pCurrListener->*pCurrHandler)(pSender);
	}

}

class CMyMemFile : public CMemFile, public IStream
{
private:
	LONG m_Refcount;

public:

	CMyMemFile()
	{
		m_bAutoDelete = FALSE;
		m_Refcount = 1;
	}

	BYTE* GetBuffer()
	{
		return m_lpBuffer;
	}

public:

	virtual HRESULT STDMETHODCALLTYPE QueryInterface(REFIID iid, void ** ppvObject)
	{
		if (iid == __uuidof(IUnknown)
			|| iid == __uuidof(IStream)
			|| iid == __uuidof(ISequentialStream))
		{
			*ppvObject = static_cast<IStream*>(this);
			AddRef();
			return S_OK;
		}
		else
			return E_NOINTERFACE;
	}

	virtual ULONG STDMETHODCALLTYPE AddRef(void)
	{
		return (ULONG)InterlockedIncrement(&m_Refcount);
	}

	virtual ULONG STDMETHODCALLTYPE Release(void)
	{
		ULONG res = (ULONG)InterlockedDecrement(&m_Refcount);
		if (res == 0)
			delete this;
		return res;
	}

	// ISequentialStream Interface
public:
	virtual HRESULT STDMETHODCALLTYPE Read(void* pv, ULONG cb, ULONG* pcbRead)
	{
		*pcbRead = __super::Read(pv, cb);
		return S_OK;
	}

	virtual HRESULT STDMETHODCALLTYPE Write(void const* pv, ULONG cb, ULONG* pcbWritten)
	{
		__super::Write(pv, cb);
		*pcbWritten = cb;
		return S_OK;
	}

	// IStream Interface
public:
	virtual HRESULT STDMETHODCALLTYPE SetSize(ULARGE_INTEGER)
	{
		return E_NOTIMPL;
	}

	virtual HRESULT STDMETHODCALLTYPE CopyTo(IStream*, ULARGE_INTEGER, ULARGE_INTEGER*,
		ULARGE_INTEGER*)
	{
		return E_NOTIMPL;
	}

	virtual HRESULT STDMETHODCALLTYPE Commit(DWORD)
	{
		return E_NOTIMPL;
	}

	virtual HRESULT STDMETHODCALLTYPE Revert(void)
	{
		return E_NOTIMPL;
	}

	virtual HRESULT STDMETHODCALLTYPE LockRegion(ULARGE_INTEGER, ULARGE_INTEGER, DWORD)
	{
		return E_NOTIMPL;
	}

	virtual HRESULT STDMETHODCALLTYPE UnlockRegion(ULARGE_INTEGER, ULARGE_INTEGER, DWORD)
	{
		return E_NOTIMPL;
	}

	virtual HRESULT STDMETHODCALLTYPE Clone(IStream **)
	{
		return E_NOTIMPL;
	}

	virtual HRESULT STDMETHODCALLTYPE Seek(LARGE_INTEGER liDistanceToMove, DWORD dwOrigin,
		ULARGE_INTEGER* lpNewFilePointer)
	{
		DWORD dwMoveMethod;

		switch (dwOrigin)
		{
		case STREAM_SEEK_SET:
			dwMoveMethod = FILE_BEGIN;
			break;
		case STREAM_SEEK_CUR:
			dwMoveMethod = FILE_CURRENT;
			break;
		case STREAM_SEEK_END:
			dwMoveMethod = FILE_END;
			break;
		default:
			return STG_E_INVALIDFUNCTION;
			break;
		}

		__super::Seek(liDistanceToMove.QuadPart, dwMoveMethod);
		return S_OK;
	}

	virtual HRESULT STDMETHODCALLTYPE Stat(STATSTG* pStatstg, DWORD grfStatFlag)
	{
		pStatstg->cbSize.QuadPart = __super::GetLength();
		return S_OK;
	}

};


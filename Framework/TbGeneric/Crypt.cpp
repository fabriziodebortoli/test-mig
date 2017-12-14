
#include "stdafx.h"

#include "crypt.h"

//-------------------------------------------------------------------------
bool CCrypto::SetError(DWORD err, LPCTSTR szFun, int myErr/*=0*/)
{
	CString s;

	if (myErr)
	{
		switch (myErr)
		{
			case 1:
					m_dwErrors.Add(err);
					s.Format(_T("%s failed: empty handle (CryptProv: %x, Key: %x)"), szFun, m_hCryptProv, m_hKey);
					m_arErrors.Add(s);
					return false;
			case 2:
					m_dwErrors.Add(err);
					s.Format(_T("%s failed: Object unserializable"), szFun);
					m_arErrors.Add(s);
					return false;
		}
	}

	m_dwErrors.Add(err);

	LPVOID lpMsgBuf;
 
	::FormatMessage(
        FORMAT_MESSAGE_ALLOCATE_BUFFER | 
        FORMAT_MESSAGE_FROM_SYSTEM |
        FORMAT_MESSAGE_IGNORE_INSERTS,
        NULL,
        err,
        MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
        (LPTSTR) &lpMsgBuf,
        0, NULL );

	s.Format(_T("%s failed with error %x: %s"), szFun, err, lpMsgBuf);

    LocalFree(lpMsgBuf);
 
	m_arErrors.Add(s);

	return false;
}

//	Constructor, intialises Crypto API.
//-------------------------------------------------------------------------
CCrypto::CCrypto() 
	: 
m_hCryptProv(NULL), m_hKey(NULL), m_hHash(NULL)
{
	//	Create the Crypt context.
	if (!::CryptAcquireContext(&m_hCryptProv, NULL, NULL, PROV_RSA_FULL, 0)) 
	{
		DWORD dwErr1 = ::GetLastError();
		if (dwErr1 == NTE_BAD_KEYSET)
		{
			BOOL bOk = ::CryptAcquireContext(&m_hCryptProv, NULL, NULL, PROV_RSA_FULL, CRYPT_NEWKEYSET);
			if (!bOk)
			{
				DWORD dwErr2 = ::GetLastError();
				SetError(dwErr1, _T("CryptAcquireContext"));
				SetError(dwErr2, _T("CryptAcquireContext(CRYPT_NEWKEYSET)"));
				return;
			}
		}
		else
		{
			SetError(dwErr1, _T("CryptAcquireContext"));
			return;
		}
	}

	//	Create an empty hash object.
	if(!::CryptCreateHash(m_hCryptProv, CALG_MD5, 0, 0, &m_hHash)) 
	{
		SetError(::GetLastError(), _T("CryptCreateHash"));
		return;
	}

	//	Memory files are opened automatically on construction, we don't
	//	explcitly call open.
}

//	Destructor, frees Crypto stuff.
//-------------------------------------------------------------------------
CCrypto::~CCrypto()
{
	//	Close the file.
	m_file.Close();

	// Clean up.
	if(m_hHash)
		::CryptDestroyHash(m_hHash);

	if(m_hKey)
		::CryptDestroyKey(m_hKey);

	if(m_hCryptProv)
		::CryptReleaseContext(m_hCryptProv, 0);
}

//	Derive a key from a password.
//-------------------------------------------------------------------------
bool CCrypto::DeriveKey(CString strPassword)
{
	//	Return failure if we don't have a context or hash.
	if(m_hCryptProv == NULL || m_hHash == NULL)
		return SetError(0, _T("DeriveKey"), 1);

	//	If we already have a hash, trash it.
	if(m_hHash)
	{
		CryptDestroyHash(m_hHash);
		m_hHash = NULL;
		if(!CryptCreateHash(m_hCryptProv, CALG_MD5, 0, 0, &m_hHash)) 
		{
			return SetError(::GetLastError(), _T("CryptCreateHash"));
		}
	}

	//	If we already have a key, destroy it.
	if (m_hKey)
	{
		::CryptDestroyKey(m_hKey);
		m_hKey = NULL;
	}

	//	Hash the password. This will have a different result in UNICODE mode, as it
	//	will hash the UNICODE string (this is by design, allowing for UNICODE passwords, but
	//	it's important to be aware of this behaviour.
	if(!CryptHashData(m_hHash, (const BYTE*)strPassword.GetString(), strPassword.GetLength() * sizeof(TCHAR), 0)) 
	{
		return SetError(::GetLastError(), _T("CryptHashData"));
	}
	
	//	Create a session key based on the hash of the password.
	if (!CryptDeriveKey(m_hCryptProv, CALG_RC2, m_hHash, CRYPT_EXPORTABLE, &m_hKey))
	{
		return SetError(::GetLastError(), _T("CryptDeriveKey"));
	}

	//	And we're done.
	return true;
}

//-------------------------------------------------------------------------
bool CCrypto::Encrypt(const CObject& serializable, CByteArray& arData)
{
	//	Return failure if we don't have a context or key.
	if (m_hCryptProv == NULL || m_hKey == NULL)
	{
		return SetError(0, _T("Encrypt"), 1);
	}

	//	Return failure if the object is not serializable.
	if (!serializable.IsSerializable())
	{
		return SetError(0, _T("Encrypt"), 2);
	}

	//	Before we write to the file, trash it.
	m_file.SetLength(0);

	//	Create a storing archive from the memory file.
	CArchive ar(&m_file, CArchive::store);

	//	We know that serialzing an object will not change it's data, as we can
	//	safely use a const cast here.

	//	Write the data to the memory file.
	const_cast<CObject&>(serializable).Serialize(ar);
	
	//	Close the archive, flushing the write.
	ar.Close();

	//	Encrypt the contents of the memory file and store the result in the array.
	return InternalEncrypt(arData);
}

//-------------------------------------------------------------------------
bool CCrypto::Decrypt(const CByteArray& arData, CObject& serializable)
{
	//	Return failure if we don't have a context or key.
	if (m_hCryptProv == NULL || m_hKey == NULL)
	{
		return SetError(0, _T("Decrypt"), 1);
	}

	//	Return failure if the object is not serializable.
	if (!serializable.IsSerializable())
	{
		return SetError(0, _T("Decrypt"), 2);
	}

	//	Decrypt the contents of the array to the memory file.
	if (!InternalDecrypt(arData))
		return false;

	//	Create a reading archive from the memory file.
	CArchive ar(&m_file, CArchive::load);

	//	Read the data from the memory file.
	serializable.Serialize(ar);
	
	//	Close the archive.
	ar.Close();

	//	And we're done.
	return true;
}

//-------------------------------------------------------------------------
bool CCrypto::Encrypt(const CString& str, CByteArray& arData)
{
	//	Return failure if we don't have a context or key.
	if (m_hCryptProv == NULL || m_hKey == NULL)
	{
		return SetError(0, _T("Encrypt"), 1);
	}

	//	Before we write to the file, trash it.
	m_file.SetLength(0);

	//	Create a storing archive from the memory file.
	CArchive ar(&m_file, CArchive::store);

	//	Write the string to the memory file.
	ar << str;

	//	Close the archive, flushing the write.
	ar.Close();

	//	Encrypt the contents of the memory file and store the result in the array.
	return InternalEncrypt(arData);
}

//-------------------------------------------------------------------------
bool CCrypto::Decrypt(const CByteArray& arData, CString& str)
{
	//	Return failure if we don't have a context or key.
	if (m_hCryptProv == NULL || m_hKey == NULL)
	{
		return SetError(0, _T("Decrypt"), 1);
	}

	//	Decrypt the contents of the array to the memory file.
	if (!InternalDecrypt(arData))
	{
		return false;
	}

	//	Create a reading archive from the memory file.
	CArchive ar(&m_file, CArchive::load);

	//	Read the data from the memory file.
	ar >> str;
	
	//	Close the archive.
	ar.Close();

	//	And we're done.
	return true;
}

//-------------------------------------------------------------------------
bool CCrypto::InternalEncrypt(CByteArray& arDestination)
{
	//	Get the length of the data in memory. Increase the capacity to handle the size of the encrypted data.
	ULONGLONG uLength = m_file.GetLength();
	ULONGLONG uCapacity = uLength * 2;
	m_file.SetLength(uCapacity);

	//	Acquire direct access to the memory.
	BYTE* pData = m_file.Detach();

	//	We need a DWORD to tell encrypt how much data we're encrypting.
	DWORD dwDataLength = static_cast<DWORD>(uLength);

	//	Now encrypt the memory file.
	if(!::CryptEncrypt(m_hKey, NULL, TRUE, 0, pData, &dwDataLength, static_cast<DWORD>(uCapacity)))
	{
		DWORD e = ::GetLastError();
		//	Free the memory we release from the memory file.
		delete [] pData;

		return SetError(e, _T("CryptEncrypt"));
	}	

	//	Assign all of the data we have encrypted to the byte array- make sure anything 
	//	already in the array is trashed first.
	arDestination.RemoveAll();
	arDestination.SetSize(static_cast<INT_PTR>(dwDataLength));
	memcpy(arDestination.GetData(), pData, dwDataLength);

	//	Free the memory we release from the memory file.
	delete [] pData;

	return true;
}

//-------------------------------------------------------------------------
bool CCrypto::InternalDecrypt(const CByteArray& arSource)
{
	//	Trash the file.
	m_file.SetLength(0);

	//	Write the contents of the byte array to the memory file.
	m_file.Write(arSource.GetData(), static_cast<UINT>(arSource.GetCount()));
	m_file.Flush();

	//	Acquire direct access to the memory file buffer.
	BYTE* pData = m_file.Detach();

	//	We need a DWORD to tell decrpyt how much data we're encrypting.
	DWORD dwDataLength = static_cast<DWORD>(arSource.GetCount());
	DWORD dwOldDataLength = dwDataLength;

	//	Now decrypt the data.
	if(!::CryptDecrypt(m_hKey, NULL, TRUE, 0, pData, &dwDataLength))
	{
		DWORD e = ::GetLastError();
		//	Free the memory we release from the memory file.
		delete [] pData;

		return SetError(e, _T("CryptDecrypt"));
	}

	//	Set the length of the data file, write the decrypted data to it.
	m_file.SetLength(dwDataLength);
	m_file.Write(pData, dwDataLength);
	m_file.Flush();
	m_file.SeekToBegin();

	//	Free the memory we release from the memory file.
	delete [] pData;

	return true;
}
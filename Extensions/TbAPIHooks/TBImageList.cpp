#include "StdAfx.h"
#include "TBImageList.h"


TBImageList::TBImageList(int cx, int cy, UINT nFlags, int nCount, int nGrow) : m_cx(cx), m_cy(cy), m_nFlags(nFlags), m_Handle((HIMAGELIST)Get_New_CImageListMap())
{
	CSingleLock l (Get_CImageListMapSection(), TRUE);
	Get_CImageListMap()[m_Handle] = this;
	m_arImages.SetSize(nCount, nGrow);
}


TBImageList::~TBImageList(void)
{
	CSingleLock l (Get_CImageListMapSection(), TRUE);
	Get_CImageListMap().RemoveKey(m_Handle);
	for (int i = 0; i < m_arImages.GetCount(); i++)
		delete (m_arImages[i]);
}

BOOL TBImageList::GetImageInfo (int i, IMAGEINFO *pImageInfo)
{
	if (i < 0 || i > m_arImages.GetCount())
	{
		return FALSE;
	}
	IMAGEINFO* pInfo = m_arImages[i];
	if (pInfo == NULL)
		return FALSE;
	memcpy_s(pImageInfo, sizeof(IMAGEINFO), pInfo, sizeof(IMAGEINFO));
	return TRUE;
}

int TBImageList::AddMasked(HBITMAP hbmImage, COLORREF crMask)
{
	BITMAP bitMap;
	::GetObject(hbmImage, sizeof(BITMAP), &bitMap);
	int nCount = bitMap.bmWidth / m_cx;
	int nStart = m_arImages.GetSize();
	for (int i = 0; i < nCount; i++)
	{
		IMAGEINFO *pInfo = new IMAGEINFO;
		ZeroMemory(pInfo, sizeof(IMAGEINFO));
		m_arImages.Add(pInfo);
		pInfo->rcImage = CRect(m_cx*(nStart + i), 0, m_cx*(nStart + i + 1), m_cy);
	}
	return nStart;
}

HICON TBImageList::GetIcon (int i, UINT flags)
{
	return (HICON)1;
}

CImageListMap::~CImageListMap()
{
	POSITION pos =  GetStartPosition();
	HIMAGELIST key;
	TBImageList* pVal;
	while (pos)
	{
		GetNextAssoc(pos, key, pVal);
		delete pVal;
	}
}

CImageMap::~CImageMap()
{
	POSITION pos =  GetStartPosition();
	CString key;
	CTBImage* pVal;
	while (pos)
	{
		GetNextAssoc(pos, key, pVal);
		delete pVal;
	}
}
TBImageList* GetTBImageList(HIMAGELIST handle)
{
	CSingleLock l (Get_CImageListMapSection(), TRUE);
	TBImageList* pImageList = NULL;
	Get_CImageListMap().Lookup(handle, pImageList);
	return pImageList;
}
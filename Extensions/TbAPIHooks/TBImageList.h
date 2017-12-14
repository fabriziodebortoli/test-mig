#pragma once
class TBImageList
{
	HIMAGELIST m_Handle;
	int m_cx;
	int m_cy;
	UINT m_nFlags;
	CArray<IMAGEINFO*> m_arImages;

public:
	TBImageList(int cx, int cy, UINT nFlags, int nCount, int nGrow);
	~TBImageList(void);

	int GetCount() { return m_arImages.GetSize(); }
	void SetCount(int nCount) { m_arImages.SetSize(nCount);}
	BOOL GetImageInfo (int i, IMAGEINFO *pImageInfo);
	int AddMasked(HBITMAP hbmImage, COLORREF crMask);
	HICON GetIcon (int i, UINT flags);
	HIMAGELIST GetHandle(){return m_Handle;}
};

class CTBImage
{
private:
	int m_nRefs;
	HGDIOBJ m_handle;
	CString m_sKey;
public:
	CTBImage(HGDIOBJ h, LPCTSTR szKey) : m_nRefs(0), m_handle(h), m_sKey(szKey){}
	void AddRef(){ m_nRefs++; }
	int Release(){ return --m_nRefs; } 
	HGDIOBJ GetHandle() { return m_handle; }
	const CString& GetKey() { return m_sKey; }
};
class CImageListMap : public CMap<HIMAGELIST, HIMAGELIST, TBImageList*, TBImageList*>
{
public:
	~CImageListMap();
};
class CImageMap : public CMap<CString, LPCTSTR, CTBImage*, CTBImage*>
{
public:
	~CImageMap();
};
class CHGDIOBJMap : public CMap<HGDIOBJ, HGDIOBJ, CTBImage*, CTBImage*>
{
};
TBImageList* GetTBImageList(HIMAGELIST handle);
#include"StdAfx.h"
#include".\sourcesafeitem.h"
#include".\sourcecontrolwrapper.h"
#include".\GlobalFunctions.h"

#using <mscorlib.dll>

using namespace Microarea::Library::SourceControl;

//--------------------------------------------------------------------------------
CSourceSafeItem::CSourceSafeItem()
{
	m_nHandle = 0;
}

//--------------------------------------------------------------------------------
CSourceSafeItem::CSourceSafeItem(long handle)
{
	m_nHandle = handle;
	SourceControlWrapper::ObjectRepository::AddReference(m_nHandle);
}

//--------------------------------------------------------------------------------
CSourceSafeItem::CSourceSafeItem(const CSourceSafeItem& source)
{
	m_nHandle = source.m_nHandle;
	SourceControlWrapper::ObjectRepository::AddReference(m_nHandle);
}

//--------------------------------------------------------------------------------
CSourceSafeItem CSourceSafeItem::operator = (const CSourceSafeItem& source)
{
	m_nHandle = source.m_nHandle;
	SourceControlWrapper::ObjectRepository::AddReference(m_nHandle);
	return *this;
}

//--------------------------------------------------------------------------------
CSourceSafeItem::~CSourceSafeItem(void)
{	
	SourceControlWrapper::ObjectRepository::Delete(m_nHandle);	
}

//--------------------------------------------------------------------------------
CString CSourceSafeItem::GetName()
{
	return GetItemObject(m_nHandle)->Name;
}

//--------------------------------------------------------------------------------
CString CSourceSafeItem::GetPath()
{
	return GetItemObject(m_nHandle)->Path;
}

//--------------------------------------------------------------------------------
int CSourceSafeItem::GetType()
{
	return GetItemObject(m_nHandle)->Type;
}

//--------------------------------------------------------------------------------
BOOL CSourceSafeItem::IsCheckedOutToMe()
{
	return GetItemObject(m_nHandle)->IsCheckedOutToMe;
}

//--------------------------------------------------------------------------------
BOOL CSourceSafeItem::IsCheckedOut()
{
	return GetItemObject(m_nHandle)->IsCheckedOut;
}

//--------------------------------------------------------------------------------
BOOL CSourceSafeItem::IsProject()
{
	return GetItemObject(m_nHandle)->IsProject;
}

//--------------------------------------------------------------------------------
CString CSourceSafeItem::GetLocalPath()
{
	return GetItemObject(m_nHandle)->LocalPath;
}
//--------------------------------------------------------------------------------
void CSourceSafeItem::SetLocalPath(const CString strPath)
{
	GetItemObject(m_nHandle)->LocalPath=strPath;
}

//--------------------------------------------------------------------------------
BOOL CSourceSafeItem::GetBinary()
{
	return GetItemObject(m_nHandle)->Binary;
}

//--------------------------------------------------------------------------------
void CSourceSafeItem::SetBinary(BOOL bSet)
{

	GetItemObject(m_nHandle)->Binary=bSet;
}

//--------------------------------------------------------------------------------
CSourceSafeItemCollection CSourceSafeItem::GetItems()
{
	return ConvertItem(GetItemObject(m_nHandle)->GetItems());
}

//--------------------------------------------------------------------------------
void CSourceSafeItem::CheckIn(const CString local,const CString comment)
{
	GetItemObject(m_nHandle)->CheckIn(local, comment);
}
//--------------------------------------------------------------------------------
void CSourceSafeItem::CheckOut(const CString local,const CString comment, BOOL updateLocal)
{
	GetItemObject(m_nHandle)->CheckOut(local, comment, updateLocal);
}

//--------------------------------------------------------------------------------
void CSourceSafeItem::UndoCheckOut(const CString local)
{
	GetItemObject(m_nHandle)->UndoCheckOut(local);
}

//--------------------------------------------------------------------------------
void CSourceSafeItem::Rename(const CString newName)
{
	GetItemObject(m_nHandle)->Rename(newName);
}

//--------------------------------------------------------------------------------
void CSourceSafeItem::GetLatestVersion(const CString local)
{
	GetItemObject(m_nHandle)->GetLatestVersion(local);
}

//--------------------------------------------------------------------------------
void CSourceSafeItem::Delete()
{
	GetItemObject(m_nHandle)->Delete();
}

//--------------------------------------------------------------------------------
CSourceSafeItem CSourceSafeItem::Add(const CString local, const CString comment,BOOL isProject)
{
	return ConvertItem(GetItemObject(m_nHandle)->Add(local, comment, isProject));
}

//--------------------------------------------------------------------------------
BOOL CSourceSafeItem::IsDifferent(const CString localPath)
{
	GetItemObject(m_nHandle)->IsDifferent(localPath);
}

//--------------------------------------------------------------------------------
void CSourceSafeItem::Label(const CString label,const CString comment)
{
	GetItemObject(m_nHandle)->Label(label, comment);
}
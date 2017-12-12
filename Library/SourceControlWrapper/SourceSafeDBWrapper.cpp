#include "StdAfx.h"
#include ".\sourcesafedbwrapper.h"
#include ".\sourcecontrolwrapper.h"
#include ".\GlobalFunctions.h"

#using <mscorlib.dll>

using namespace Microarea::Library::SourceControl;

//--------------------------------------------------------------------------------
CSourceSafeDBWrapper::CSourceSafeDBWrapper()
{
	SourceSafeDBWrapper *db = new SourceSafeDBWrapper();
	m_nHandle = SourceControlWrapper::ObjectRepository::Add(db);
	SourceControlWrapper::ObjectRepository::AddReference(m_nHandle);
}

//--------------------------------------------------------------------------------
CSourceSafeDBWrapper::CSourceSafeDBWrapper(const CString strRemoteServer)
{
	SourceSafeDBWrapper *db = new SourceSafeDBWrapperProxy(strRemoteServer);

	m_nHandle = SourceControlWrapper::ObjectRepository::Add(db);
	SourceControlWrapper::ObjectRepository::AddReference(m_nHandle);
}

//--------------------------------------------------------------------------------
CSourceSafeDBWrapper::~CSourceSafeDBWrapper(void)
{
	SourceControlWrapper::ObjectRepository::Delete(m_nHandle);
}

//--------------------------------------------------------------------------------
BOOL CSourceSafeDBWrapper::IsOpen()
{
	return GetDatabaseObject(m_nHandle)->IsOpen;	
}

//--------------------------------------------------------------------------------
BOOL CSourceSafeDBWrapper::Open(const CString iniPath, const CString userName, const CString password)
{
	return GetDatabaseObject(m_nHandle)->Open(iniPath, userName, password);
}

//--------------------------------------------------------------------------------
BOOL CSourceSafeDBWrapper::CheckOutFile(const CString  file, const CString  localPath)
{
	return GetDatabaseObject(m_nHandle)->CheckOutFile(file, localPath);
}

//--------------------------------------------------------------------------------
BOOL CSourceSafeDBWrapper::CheckInFile(const CString file, const CString localPath)
{
	return GetDatabaseObject(m_nHandle)->CheckInFile(file, localPath);
}

//--------------------------------------------------------------------------------
CSourceSafeItem CSourceSafeDBWrapper::CreateProject(const CString path, const CString comment, BOOL recursive)
{
	return ConvertItem(GetDatabaseObject(m_nHandle)->CreateProject(path, comment, recursive));
}

//--------------------------------------------------------------------------------
CSourceSafeItem CSourceSafeDBWrapper::GetItem(const CString aVssPath)
{
	return ConvertItem(GetDatabaseObject(m_nHandle)->GetItem(aVssPath));
}

//--------------------------------------------------------------------------------
CString CSourceSafeDBWrapper::GetCurrentProject()
{
	return GetDatabaseObject(m_nHandle)->GetCurrentProject();
}

//--------------------------------------------------------------------------------
CSourceSafeItemCollection CSourceSafeDBWrapper::GetItems(const CString aVssPath)
{
	return ConvertItem(GetDatabaseObject(m_nHandle)->GetItems(aVssPath));
}
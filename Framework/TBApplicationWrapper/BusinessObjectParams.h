#pragma once

#include <TbGenlib\BaseDoc.h>

//================================================================================
class TB_EXPORT CBusinessObjectComponent : public CManagedDocComponentObj
{
public:
	gcroot<Microarea::TaskBuilderNet::Core::EasyBuilder::EasyBuilderComponent^>	Component;

public:
	CBusinessObjectComponent() : Component(nullptr) {}
};

/// <summary>
/// allows to incapsulate managed/unmanaged parameters into business object RunDocument
/// </summary>
//================================================================================
class TB_EXPORT CBusinessObjectInvocationInfo : public CManagedDocComponentObj
{
private:
	BOOL isExposing;
	CBusinessObjectComponent* m_pCaller;
public:
	CBusinessObjectInvocationInfo (bool isExposing);
	~CBusinessObjectInvocationInfo();

	virtual void CreateNewDocumentOf(CBaseDocument* pDoc);

	BOOL IsExposing() { return isExposing; }
	
	void SetCaller(Microarea::TaskBuilderNet::Core::EasyBuilder::EasyBuilderComponent^ caller);
	CBusinessObjectComponent* GetCallerComponent() {  return m_pCaller; }
};

//================================================================================
class TB_EXPORT CBusinessObjectComponentRequest : public CManagedDocComponentObj
{
public:
	gcroot<System::Collections::IList^>	Types;
	
public:
	CBusinessObjectComponentRequest() : Types (nullptr) { }
};

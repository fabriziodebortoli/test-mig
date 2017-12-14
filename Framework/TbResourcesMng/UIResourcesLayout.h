#pragma once

#include "ModuleObjects\ResourcesLayout\JsonForms\IDD_RESOURCES_LAYOUT.hjson"

#include "beginh.dex"

class BDResourcesLayout;

//=======================================================================================
class CResourcesLayoutTreeVView : public CJsonFormView
{
	DECLARE_DYNCREATE(CResourcesLayoutTreeVView)

public:
	CResourcesLayoutTreeVView();
};

//=======================================================================================
class CResourcesLayoutDetailsView : public CJsonFormView
{
	DECLARE_DYNCREATE(CResourcesLayoutDetailsView)

public:
	CResourcesLayoutDetailsView();
};

#include "endh.dex"

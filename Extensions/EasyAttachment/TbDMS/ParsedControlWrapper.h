#pragma once

using namespace Microarea::EasyAttachment::Components;

class CParsedCtrl;
//================================================================================
ref class CParsedControlWrapper : public ParsedControlWrapper
{
	CParsedCtrl* m_pParsedCtrl;

public:
	CParsedControlWrapper(CParsedCtrl* pParsedCtrl);
	virtual ~CParsedControlWrapper(void);

	virtual void UpdateView() override;
};


#pragma once
ref class CSourceControl
{
public:
	CSourceControl();
	static bool CheckOutIfNeeded(System::String^ file);
	static bool CheckOut(System::String^ file);
};


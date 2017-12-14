#include "stdafx.h"
#include <TbGeneric\WndObjDescription.h>
#include "MAccelerator.h"


using namespace Microarea::Framework::TBApplicationWrapper;

//-----------------------------------------------------------------------------
MAccelerator::MAccelerator()
	: m_pAcceleratorItem(new CAcceleratorItemDescription())
{
	m_pAcceleratorItem->m_sId = _T("ID_ACCELERATOR");
	
}

//-----------------------------------------------------------------------------
MAccelerator::MAccelerator(CAcceleratorItemDescription* pAcceleratorItem)
	: m_pAcceleratorItem(pAcceleratorItem->Clone())
{


}
//-----------------------------------------------------------------------------
String^ MAccelerator::ToString()
{
	return Id;
}
//-----------------------------------------------------------------------------
String^ MAccelerator::Id::get()
{
	return gcnew String(m_pAcceleratorItem->m_sId);
}
//-----------------------------------------------------------------------------
void MAccelerator::Id::set(String^ value)
{
	if (String::IsNullOrEmpty(value))
		throw gcnew Exception(gcnew String(_TB("Accelerator ID cannot be empty")));
	m_pAcceleratorItem->m_sId = value;
}

//-----------------------------------------------------------------------------
Keys MAccelerator::VirtualKey::get()
{
	if ((m_pAcceleratorItem->m_Accel.fVirt & FVIRTKEY) != FVIRTKEY)
	{
		return Keys::None;
	}
	Keys k = (Keys)m_pAcceleratorItem->m_Accel.key;
	if ((m_pAcceleratorItem->m_Accel.fVirt & FCONTROL) == FCONTROL)
		k = k | Keys::Control;

	if ((m_pAcceleratorItem->m_Accel.fVirt & FALT) == FALT)
		k = k | Keys::Alt;

	if ((m_pAcceleratorItem->m_Accel.fVirt & FSHIFT) == FSHIFT)
		k = k | Keys::Shift;

	return k;
}
//-----------------------------------------------------------------------------
void MAccelerator::VirtualKey::set(Keys k)
{
	ZeroMemory(&m_pAcceleratorItem->m_Accel, sizeof(ACCEL));
	if ((k & Keys::Control) == Keys::Control)
	{
		m_pAcceleratorItem->m_Accel.fVirt |= FCONTROL;
		k = k & ~Keys::Control;
	}
	if ((k & Keys::Alt) == Keys::Alt)
	{
		m_pAcceleratorItem->m_Accel.fVirt |= FALT;
		k = k & ~Keys::Alt;
	}

	if ((k & Keys::Shift) == Keys::Shift)
	{
		m_pAcceleratorItem->m_Accel.fVirt |= FSHIFT;
		k = k & ~Keys::Shift;
	}
	m_pAcceleratorItem->m_Accel.fVirt |= FVIRTKEY;//imposto il flag virtual key
	m_pAcceleratorItem->m_Accel.key = (WORD)k;

}

//-----------------------------------------------------------------------------
Char MAccelerator::ASCIIKey::get()
{
	if ((m_pAcceleratorItem->m_Accel.fVirt & FVIRTKEY) == FVIRTKEY)
	{
		return 0;
	}

	return m_pAcceleratorItem->m_Accel.key;
}
//-----------------------------------------------------------------------------
void MAccelerator::ASCIIKey::set(Char c)
{
	m_pAcceleratorItem->m_Accel.key = c;
	m_pAcceleratorItem->m_Accel.fVirt &= ~FVIRTKEY;//imposto ascii: tologo il flag virtual key
}

//-----------------------------------------------------------------------------
bool MAccelerator::Control::get()
{
	return (m_pAcceleratorItem->m_Accel.fVirt & FCONTROL) == FCONTROL;
}
//-----------------------------------------------------------------------------
void MAccelerator::Control::set(bool b)
{
	if (b)
		m_pAcceleratorItem->m_Accel.fVirt |= FCONTROL;
	else
		m_pAcceleratorItem->m_Accel.fVirt &= ~FCONTROL;
}


//-----------------------------------------------------------------------------
bool MAccelerator::Alt::get()
{
	return (m_pAcceleratorItem->m_Accel.fVirt & FALT) == FALT;
}
//-----------------------------------------------------------------------------
void MAccelerator::Alt::set(bool b)
{
	if (b)
		m_pAcceleratorItem->m_Accel.fVirt |= FALT;
	else
		m_pAcceleratorItem->m_Accel.fVirt &= ~FALT;
}


//-----------------------------------------------------------------------------
bool MAccelerator::Shift::get()
{
	return (m_pAcceleratorItem->m_Accel.fVirt & FSHIFT) == FSHIFT;
}
//-----------------------------------------------------------------------------

void MAccelerator::Shift::set(bool b)
{
	if (b)
		m_pAcceleratorItem->m_Accel.fVirt |= FSHIFT;
	else
		m_pAcceleratorItem->m_Accel.fVirt &= ~FSHIFT;
}
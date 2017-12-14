#pragma once

namespace Microarea { namespace Framework	{ namespace TBApplicationWrapper  {
ref class MStringList : public System::Collections::IEnumerable, System::Collections::Generic::IList<System::String^>
{
	CStringArray* m_parList;
public:
	MStringList(CStringArray* parList)
		: m_parList(parList)
	{
	}
	property int Count { virtual int get(); }
	property bool IsReadOnly{ virtual bool get(); }
	virtual System::Collections::Generic::IEnumerator<System::String^>^ GetEnumerator1(void) = System::Collections::Generic::IEnumerable<System::String^>::GetEnumerator;
	virtual System::Collections::IEnumerator^ GetEnumerator();
	virtual void Add(System::String^ s);
	virtual bool Remove(System::String^ s);
	virtual void Clear();
	virtual bool Contains(System::String^ s);
	virtual void RemoveAt(int index);
	virtual void Insert(int index, System::String^ s);
	virtual int IndexOf(System::String^ s);
	virtual void set_Item(int index, System::String^ s);
	virtual System::String^ get_Item(int index);
	virtual void CopyTo(cli::array<System::String^>^ ar, int index);
};

}}}
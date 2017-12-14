#include "StdAfx.h"

#include <TBNameSolver\ApplicationContext.h>
#include <TBNameSolver\LockTracer.h>

#include "CLockStructureViewer.h"

using namespace System;
using namespace System::Threading;
using namespace System::Drawing;
using namespace System::Drawing::Drawing2D;
using namespace System::Windows::Forms;
using namespace System::Collections::Generic;

using namespace Microarea::Framework::TBApplicationWrapper;

//--------------------------------------------------------------------------------------------------------------------------------
int CLockStructureViewer::AddNode(const CLockStructure* pStructure, CString strObject, Dictionary<System::String^, int>^ ht)
{
	int index = 0;
	CLockRelation* pRelation = NULL;
	if (pStructure->m_ObjectMap.Lookup(strObject, pRelation))
	{
		for (int i = 0; pRelation && i < pRelation->m_strParentNames.GetCount(); i++)
			index = Math::Max(index, AddNode(pStructure, pRelation->m_strParentNames.GetAt(i), ht)); 
	}
	System::String^ sObject = gcnew System::String(strObject);
		
	int idx;
	if (!ht->TryGetValue(sObject, idx))
	{
		ht[sObject] = ++index;
	}
	else
	{
		index = idx;
	}

	return index;
}

//--------------------------------------------------------------------------------------------------------------------------------
Void CLockStructureViewer::CLockStructureViewer_Load(Object^ sender, EventArgs^ e) 
{
	CLockTracer* pTracer = AfxGetApplicationContext()->GetLockTracer();
	if (!pTracer) 
		return;
	
	try
	{
		const CLockStructure* pStructure = pTracer->GetLockStructure();
		CLockRelation* pRelation;
		CString strObject;
		Dictionary<System::String^, int>^ ht = gcnew Dictionary<System::String^, int>();
		
		int index = 0;
		POSITION pos = pStructure->m_ObjectMap.GetStartPosition();
		while (pos)
		{
			pStructure->m_ObjectMap.GetNextAssoc(pos, strObject, pRelation);
			index = Math::Max(index, AddNode(pStructure, strObject, ht));
		}

		int* locations = new int[index];
		int* widths = new int[index];
		for (int i = 0; i < index; i++)
			locations[i] = 0;
		for (int i = 0; i < index; i++)
			widths[i] = 0;

		Graphics^ graphics = CreateGraphics();

		int hDelta = 50, vDelta = 100;
		for each(System::String^ s in ht->Keys)
		{
			int idx = ht[s] - 1;
			widths[idx] = Math::Max(widths[idx], Convert::ToInt32(graphics->MeasureString(s, Font).Width));
		}
		delete graphics;

		Dictionary<System::String^, TextBox^>^ htControls = gcnew Dictionary<System::String^,  TextBox^>();
		for each(System::String^ s in ht->Keys)
		{
			int idx = ht[s] - 1;
			int currWidth = widths[idx];
			int prevWidth = hDelta;
			
			for (int i = 0; i < idx; i++)
				prevWidth += widths[i];

			locations[idx]++;

			TextBox^ tb = gcnew TextBox();
			tb->Width = currWidth;
			tb->Text = s;
			tb->Left = idx * hDelta + prevWidth;
			tb->Top = locations[idx] * (tb->Height + vDelta);
			lockStructurePanel->Controls->Add(tb);
				
			htControls[s] = tb;
		}
	
		delete locations;
		delete widths;

		pos = pStructure->m_ObjectMap.GetStartPosition();
		while (pos)
		{
			pStructure->m_ObjectMap.GetNextAssoc(pos, strObject, pRelation);
			if (pRelation)
			{
				TextBox^ tbDest = htControls[gcnew System::String(strObject)];
				Point pt(tbDest->Left, tbDest->Top + tbDest->Height / 2);
				for (int i = 0; i < pRelation->m_strParentNames.GetCount(); i++)
				{
					TextBox^ tbOrigin = htControls[gcnew System::String(pRelation->m_strParentNames.GetAt(i))];
					fromPointList->Add(Point(tbOrigin->Right, tbOrigin->Top + tbOrigin->Height / 2));
					toPointList->Add(pt);
					CStringArray* pFunctionList = pRelation->m_arFunctions.GetAt(i);
					
					CString s;
					for (int j = 0; j < pFunctionList->GetCount(); j++)
					{
						s += pFunctionList->GetAt(j);
						s += "\r\n";
					}
					tooltipList->Add(gcnew System::String(s));
				}
			}
		}
	}
	catch(Exception^ ex)
	{
		MessageBox::Show(this, ex->Message);
	}
	finally
	{
		
	}
}	
//--------------------------------------------------------------------------------------------------------------------------------
void CLockStructureViewer::lockStructurePanel_Paint(System::Object^  sender, System::Windows::Forms::PaintEventArgs^  e) 
{
	Pen ^p = gcnew Pen(Color::Red, 2);
	
	e->Graphics->DrawLine(p, 1, 1, 2, 2);
	lineRGB = GetPixel(Point(1, 1));

	for (int i = 0; i < fromPointList->Count; i++)
	{
		Point p1 = fromPointList[i];
		p1.Offset(-lockStructurePanel->HorizontalScroll->Value, -lockStructurePanel->VerticalScroll->Value);
		Point p2 = toPointList[i];
		p2.Offset(-lockStructurePanel->HorizontalScroll->Value, -lockStructurePanel->VerticalScroll->Value);
		e->Graphics->DrawLine(p, p1, p2);
	}

	delete p;
}

//--------------------------------------------------------------------------------------------------------------------------------
COLORREF CLockStructureViewer::GetPixel(Point p)
{
	CPoint p1 = CPoint(p.X, p.Y);
	Graphics^ graphics = lockStructurePanel->CreateGraphics();
	System::IntPtr hdc = graphics->GetHdc();
	HDC hdc1 = (HDC)hdc.ToInt32();
		
	DPtoLP(hdc1, &p1, 1);
	
	COLORREF rgb = ::GetPixel(hdc1, p1.x, p1.y);
	graphics->ReleaseHdc(hdc);
	delete graphics;

	return rgb;		
}

//--------------------------------------------------------------------------------------------------------------------------------
void CLockStructureViewer::ShowTooltip(Point p)
{
	System::String^ s = nullptr;
	try
	{
		
		if (GetPixel(p) != lineRGB)
		{
			toolTip->SetToolTip(lockStructurePanel, s);
			return;
		}

		float minDelta = 100;
		for (int i = 0; i < fromPointList->Count; i++)
		{
			Point p1 = fromPointList[i];
			p1.Offset(-lockStructurePanel->HorizontalScroll->Value, -lockStructurePanel->VerticalScroll->Value);
			Point p2 = toPointList[i];
			p2.Offset(-lockStructurePanel->HorizontalScroll->Value, -lockStructurePanel->VerticalScroll->Value);

			float ratio1;
			float ratio2;

			if (p.X < p1.X || p.X > p2.X )
				continue;

			ratio1 = (p.Y == p1.Y) ? 0 : (float)(p.X - p1.X) / (float)(p.Y - p1.Y);
			ratio2 = (p2.Y == p1.Y) ? 0 : (float)(p2.X - p1.X) / (float)(p2.Y - p1.Y);
		
			float abs = Math::Abs(ratio1 - ratio2);
			if (abs < minDelta)
			{
				minDelta = abs;
				s = tooltipList[i];
			}
		}
		
		toolTip->SetToolTip(lockStructurePanel, s);
	}
	catch (Exception^ e)
	{
		System::String^ s = e->Message;
	}
}

//--------------------------------------------------------------------------------------------------------------------------------
void CLockStructureViewer::lockStructurePanel_MouseMove(System::Object^  sender, System::Windows::Forms::MouseEventArgs^  e)
{
	ShowTooltip(e->Location);
}

//--------------------------------------------------------------------------------------------------------------------------------
bool CLockStructureViewer::CanShowLockStructure::get()
{
	return AfxGetApplicationContext()->GetLockTracer() != NULL;	
}
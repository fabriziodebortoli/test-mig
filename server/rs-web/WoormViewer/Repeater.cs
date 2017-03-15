﻿using System.Collections.Generic;

using Microarea.Common.Generic;
using Microarea.Common.Lexan;

using Microarea.RSWeb.WoormEngine;
using Microarea.RSWeb.Objects;

namespace Microarea.RSWeb.WoormViewer
{
	//==============================================================================
	public class RepeaterObjects : List<BaseObjList>
	{
		protected Repeater repeater;

		//---------------------------------------------------------------------------
		public RepeaterObjects(Repeater pRep)
		{
			repeater = pRep;
			BaseObjList pMasterObjs = new BaseObjList(repeater.Document);
			Add(pMasterObjs);
		}

		//---------------------------------------------------------------------------
		public RepeaterObjects(Repeater pRep, RepeaterObjects source)
		{
			repeater = pRep;
			foreach (BaseObjList item in source)
				Add(item.Clone());
		}

		//---------------------------------------------------------------------------
		public BaseObjList GetMasterObjects()
		{
			if (Count == 0)
			{
				BaseObjList pMasterObjs = new BaseObjList(repeater.Document);
				Add(pMasterObjs);
			}
			return this[0];
		}

		//---------------------------------------------------------------------------
		protected void Detach(BaseObj pObj)
		{
			/*
			pObj->m_AnchorRepeaterID = 0;

			if (!pObj->IsKindOf(RUNTIME_CLASS(FieldRect)))
				return;
			//----

			WoormTable* pSymTable = (
				m_pRepeater->m_pDocument->m_pEditorManager 
				? 
				m_pRepeater->m_pDocument->m_pEditorManager->GetSymTable() 
				: NULL);
	       	
			if (pSymTable && pObj->GetInternalID())
			{
				WoormField* pF = pSymTable->GetFieldByID(pObj->GetInternalID());
				if (pF && pF->GetFieldType() == WoormField::FIELD_COLUMN)
				{
					pF->RemoveDispTable();
				}
			}
			*/
		}

		//---------------------------------------------------------------------------
		public virtual void RemoveAll()
		{
			if (Count <= 0)
				return;

			BaseObjList pMasterObjs = GetMasterObjects();
			if (pMasterObjs == null)
			{
				foreach (BaseObj item in pMasterObjs)
					Detach(item);
			}
			Clear();
		}

		//---------------------------------------------------------------------------
		public void AddChild(BaseObj pObj)
		{
			pObj.AnchorRepeaterID = repeater.InternalID;

			GetMasterObjects().Add(pObj);

			if (!(pObj is FieldRect))
				return;
			//----
			/* TODO per EDITOR
			 * 
			WoormTable* pSymTable = (
				m_pRepeater->m_pDocument->m_pEditorManager 
				? 
				m_pRepeater->m_pDocument->m_pEditorManager->GetSymTable() 
				: NULL);

			if (pSymTable && pObj->GetInternalID())
			{
				WoormField* pF = pSymTable->GetFieldByID(pObj->GetInternalID());
				if (pF && pF->GetFieldType() == WoormField::FIELD_NORMAL)
					pF->SetDispTable(m_pRepeater->GetName());
			}
			*/
		}

		//---------------------------------------------------------------------------
		public void RemoveChild(BaseObj pObj)
		{
			/*
			 * 	int idx = GetMasterObjects()->FindPtr(pObj);
			if (idx < 0) 
				return;

			GetMasterObjects()->RemoveAt(idx);	//delete automatica DISABILITATA nel primo array

			idx += 1; //in posizione 0 c'e' un SqrRect di cornice
			for (int r = 1; r < GetSize(); r++)
			{
				CBaseObjArray* pRow = (CBaseObjArray*) GetAt(r);
				pRow->RemoveAt(idx);	//delete automatica
			}
			//----
			Detach(pObj);

			 * */
		}

		//---------------------------------------------------------------------------
		public void Replicate()
		{
			BaseObjList pMasterObjs = GetMasterObjects();

			int w = repeater.Rect.Width + repeater.nXOffset;
			int h = repeater.Rect.Height + repeater.nYOffset;

			//creo i blocchi di oggetti ripetuti (all'indice 0 ci sono gli originali in aliasing)
			int nr = repeater.RowsNumber;
			for (int r = 1; r < nr; r++)
			{
				BaseObjList pRow = pMasterObjs.Clone();

				SqrRect pR = new SqrRect(repeater);
				pR.AnchorRepeaterID = repeater.InternalID;
				pRow.Insert(0, pR);

				int x = repeater.ByColumn
					? w * (r / (repeater.nRows))
					: w * (r % (repeater.nColumns));

				int y = repeater.ByColumn
					? h * (r % (repeater.nRows))
					: h * (r / (repeater.nColumns));

				pRow.MoveBaseRect(x, y, true);

				for (int i = 0; i < pRow.Count; i++)
					pRow[i].RepeaterRow = r;

				Add(pRow);
			}

			for (int i = 0; i < pMasterObjs.Count; i++)
				pMasterObjs[i].RepeaterRow = 0;
		}

		//---------------------------------------------------------------------
        public void MoveBaseRect(int xOffset, int yOffset, bool bIgnoreBorder = false)
		{
			foreach (BaseObjList row in this)
                row.MoveBaseRect(xOffset, yOffset, bIgnoreBorder);
		}

		//---------------------------------------------------------------------

		//void	DisableData				();
		//void	Draw					(CDC&, BOOL bPreview, int nStart = 1);
		//void	ClearDynamicAttributes	();
		//BaseObj GetFieldByPosition		(const CPoint&, int& nRow);

		//---------------------------------------------------------------------------
		internal void ClearData()
		{
			foreach (BaseObjList row in this)
				row.ClearData();
		}

        //------------------------------------------------------------------------------
        internal void SetStyle(BaseRect templateRect)
        {
            foreach (BaseObjList row in this)
                row.SetStyle(templateRect);
        }

        //------------------------------------------------------------------------------
        internal void ClearStyle()
        {
            foreach (BaseObjList row in this)
                row.ClearStyle();
        }

        //------------------------------------------------------------------------------
        internal void RemoveStyle()
        {
            foreach (BaseObjList row in this)
                row.RemoveStyle();
        }
	}
	
	//=========================================================================
	public class Repeater : SqrRect
	{
		public int nRows = 1;
		public int nColumns = 1;

		public int nXOffset = 30;
		public int nYOffset = 60;

		public bool ByColumn = false;

		public RepeaterObjects Rows = null;

		public int CurrentRow = 0; // riga dove viene valorizzata la cella quando leggo da RDE
		public int ViewCurrentRow = -1; // riga corrente in fase di renderizzazione (per attributi dinamici)

        public int RowsNumber { get { return nRows * nColumns; } }

		//------------------------------------------------------------------------------
		public Repeater(WoormDocument document)
			: base(document)
		{
			Rows = new RepeaterObjects(this);
		}
	
		//------------------------------------------------------------------------------
        public override void MoveBaseRect(int xOffset, int yOffset, bool bIgnoreBorder = false)
		{
            Rows.MoveBaseRect(xOffset, yOffset, bIgnoreBorder);

            base.MoveBaseRect(xOffset, yOffset, bIgnoreBorder);
		}

		//------------------------------------------------------------------------------
		void Attach(BaseObj obj)
		{
			if (obj is Table || obj is Repeater || obj.InternalID >= SpecialReportField.REPORT_LOWER_SPECIAL_ID)
				return;

			Rows.AddChild(obj);
		}

		//------------------------------------------------------------------------------
		void Detach(BaseObj obj)
		{
			Rows.RemoveChild(obj);
		}

		//------------------------------------------------------------------------------
		public void Rebuild(BaseObjList bl)
		{
			Rows.RemoveAll();

			foreach (BaseObj item in bl)
			{
				if (this.Rect.Contains(item.Rect))
					Attach(item);
			}

			if (Rows.GetMasterObjects().Count > 0)
				Rows.Replicate();

			/* per l'editor
			 * 	if (m_pDocument->m_pEditorManager->ExistsTable(m_wInternalID))
				{
					if (!m_pDocument->m_pEditorManager->SetTableRows(m_wInternalID, m_nRows * m_nColumns))
					{
						// table wID not found must signal error but must delete table
						m_pDocument->Message(_TB("Id table not found"));
					}
				}
			 */
		}

		//---------------------------------------------------------------------
		public override bool Parse(WoormParser lex)
		{
			bool bOk =
				lex.ParseRepeater(out nRows, out nColumns);

			ByColumn = lex.Parsed(Token.COLUMN);

			bOk = bOk &&
				lex.ParseAlias(out InternalID) &&
				lex.ParseTR(Token.INTERLINE, out nYOffset, out nXOffset) &&
				lex.ParseRect(out Rect) &&
				lex.ParseRatio(out HRatio, out VRatio) &&
				ParseBlock(lex);

			return bOk;
		}

		//------------------------------------------------------------------------------
		public override bool Unparse(Unparser ofile)
		{
			//---- Template Override
			BaseRect tempDefault = Default;
			if (IsTemplate && Document.Template.IsSavingTemplate)
				Default = null;
			//----
			ofile.WriteRepeater(nRows, nColumns, false);

			if (ByColumn)
				ofile.WriteTag(Token.COLUMN, false);

			ofile.WriteAlias(InternalID, false);
			ofile.WriteTR(Token.INTERLINE, nYOffset, nXOffset, false);
			ofile.WriteRect(Rect, false);

			if (IsNotDefaultRatio())
				ofile.WriteRatio(HRatio, VRatio, false);

			UnparseProp(ofile);

			//----
			Default = tempDefault;
			return true;
		}

		//------------------------------------------------------------------------------
		//GetFieldRow
		public FieldRect FindField(ushort id, int r = -1)
		{
			if (r == -1)
				r = CurrentRow;

			if (r < 0 || r >= Rows.Count)
				return null;

			BaseObjList bl = Rows[r];

			return bl.FindBaseObj(id) as FieldRect;
		}

		//------------------------------------------------------------------------------
		public override void ClearData()
		{
			CurrentRow = 0;

			Rows.ClearData();   //DisableData such as C++
		}

		//------------------------------------------------------------------------------
		internal void Detach(BaseRect baseRect, bool repaint = false)
		{
			Rows.RemoveChild(baseRect);

			//TODOLUCA
			//if (repaint) UpdateDocument();  
		}

        //------------------------------------------------------------------------------
        public override void SetStyle(BaseRect templateRect)
        {
            base.SetStyle(templateRect);
            Rows.SetStyle(templateRect);
        }

        //------------------------------------------------------------------------------
        public override void ClearStyle()
        {
            base.ClearStyle();
            Rows.ClearStyle();
        }

        //------------------------------------------------------------------------------
        public override void RemoveStyle()
        {
            base.RemoveStyle();
            Rows.RemoveStyle();
        }

        //------------------------------------------------------------------------------
        override public string ToJsonTemplate(bool bracket)
        {
            string name = "repeater";

            string s = string.Empty;
            if (!name.IsNullOrEmpty())
                s = '\"' + name + "\":";

            s += '{' +
                base.ToJsonTemplate(false) + ',' +

                this.nRows      .ToJson("rows") + ',' +
                this.nColumns   .ToJson("columns") + ',' +

                this.nXOffset   .ToJson("xoffset") + ',' +
                this.nYOffset   .ToJson("yoffset") +

              '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        override public string ToJsonData(bool bracket)
        {
            string name = "repeater";

            string s = string.Empty;
            if (!name.IsNullOrEmpty())
                s = '\"' + name + "\":";

            s += '{' +
                base.ToJsonData(false) +
              '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

    }

}

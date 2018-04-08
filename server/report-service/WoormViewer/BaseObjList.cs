using System;
using System.Collections.Generic;

using Microarea.Common.Lexan;
using Microarea.Common.Generic;

using Microarea.RSWeb.WoormEngine;
using Microarea.RSWeb.Objects;
using Microarea.Common.CoreTypes;
using System.Drawing;

namespace Microarea.RSWeb.WoormViewer
{
    //[Serializable]
    public class BaseObjList : List<BaseObj>
    {
        protected WoormDocument document = null;
        public int CountAutoObjects = 0;

        public BaseObjList()
        {

        }
        // ---------------------------------------------------------------------------------
        public BaseObjList(WoormDocument doc)
        {
            document = doc;
        }

        // ---------------------------------------------------------------------------------
        public BaseObjList Clone()
        {
            BaseObjList cl = new BaseObjList(document);

            foreach (BaseObj item in this)
            {
                cl.Add(item.Clone());
            }
            return cl;
        }

        // ---------------------------------------------------------------------------------
        public void MoveBaseRect(int xOffset, int yOffset, bool bIgnoreBorder = false)
        {
            foreach (BaseObj item in this)
            {
                item.MoveBaseRect(xOffset, yOffset, bIgnoreBorder);
            }
        }

        // ---------------------------------------------------------------------------------
        public void ApplyRepeater()
        {
            foreach (BaseObj item in this)
            {
                if (item is Repeater)
                {
                    ((Repeater)item).Rebuild(this);
                }
            }
        }

        // ---------------------------------------------------------------------------------
        public BaseObj FindBaseObj(ushort id)
        {
            return this.Find((item) => item.InternalID == id);
        }

        // ---------------------------------------------------------------------------------
        public Column FindColumn(ushort id)
        {
            foreach (BaseObj obj in this)
            {
                if (!(obj is Table)) continue;
                Table t = obj as Table;
                int pos = t.ColumnIndexFromID(id);
                if (pos == -1) continue;
                return t.Columns[pos];
            }
            return null;
        }
        public short FindColumnIndex(ushort id)
        {
            foreach (BaseObj obj in this)
            {
                if (!(obj is Table)) continue;
                Table t = obj as Table;
                short pos = (short)t.ColumnIndexFromID(id);
                if (pos == -1) continue;
                return pos;
            }
            return -1;
        }

        //------------------------------------------------------------------------------
        internal bool UpdateObject(object woormObject)
        {
            BaseObj objToRemove = FindBaseObj(((BaseObj)woormObject).InternalID);
            BaseObj objToAdd = woormObject as BaseObj;

            if (objToAdd == null || objToRemove == null)
                return false;

            objToAdd.Document = objToRemove.Document;
            int indexObjToRemove = IndexOf(objToRemove);
            RemoveAt(indexObjToRemove);
            List<BaseObj> l = new List<BaseObj>();
            l.Add(objToAdd);
            InsertRange(indexObjToRemove, l);
            return true;
        }

        // ---------------------------------------------------------------------------------
        public void ClearData()
        {
            foreach (BaseObj item in this)
            {
                item.ClearData();
            }
        }

        // ---------------------------------------------------------------------------------
        public void SetStyle(BaseRect templateRect)
        {
            foreach (BaseObj item in this)
            {
                item.SetStyle(templateRect);
            }
        }

        // ---------------------------------------------------------------------------------
        public void ClearStyle()
        {
            foreach (BaseObj item in this)
            {
                item.ClearStyle();
            }
        }
        // ---------------------------------------------------------------------------------
        public void RemoveStyle()
        {
            foreach (BaseObj item in this)
            {
                item.RemoveStyle();
            }
        }

        //---------------------------------------------------------------------
        virtual public string ToJson(bool template, string name /*= string.Empty*/, bool bracket = false, bool array = true)
        {
            string s = string.Empty;
            if (!name.IsNullOrEmpty())
                s = '\"' + name + "\":";

            if (array) s += '[';
            bool first = true;
            foreach (BaseObj item in this)
            {
                if (item.IsHidden && item.HideExpr == null)
                    continue;
 
               if (item.InternalID >= SpecialReportField.REPORT_LOWER_SPECIAL_ID)
                    continue;

                if (array && item.AnchorRepeaterID != 0)
                    continue;   //li emette il repeater 

                if (!template && (item.HideExpr != null||item.AnchorRepeaterID != 0) && item.DynamicIsHidden)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else s += ',';

                    s += item.ToJsonHiddenData(true);
                    continue;
                }

                if (!(template || item.MayHaveDataToSerialize()))
                {
                    continue;
                }
                    
                if (!template && item.InternalID == 0)
                {
                    //TODO BUG!
                    continue;
                }

                if (first)
                {
                    first = false;
                }
                else s += ',';
 
                if (template)
                    s += item.ToJsonTemplate(true);
                else
                    s += item.ToJsonData(true);
            }
            if (array) s += ']';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

    }

    /// <summary>
    /// Array degli objects per un layout del report.
    /// </summary>
    /// ================================================================================
    //[Serializable]
    //[KnownType(typeof(BaseRect))]
    //[KnownType(typeof(BaseObj))]
    //[KnownType(typeof(SqrRect))]
    //[KnownType(typeof(TextRect))]
    //[KnownType(typeof(FileRect))]
    //[KnownType(typeof(FieldRect))]
    //[KnownType(typeof(Table))]
    public class Layout : BaseObjList
    {
        public const string DefaultName = "default";

        public string Name = DefaultName;
        public bool Invert = false;

        public Layout() : base(null) { }
        /// ---------------------------------------------------------------------------------
        public Layout(WoormDocument woorm, string name = DefaultName)
            : base(woorm)
        {
            Name = name;
            if (woorm != null)
            {
                AddSpecialLayoutFieldRect(woorm);
                AddSpecialCurrentPageFieldRect(woorm);
                AddSpecialLastPageFieldRect(woorm);
                AddSpecialCurrentCopyFieldRect(woorm);
                AddSpecialOwnerIDFieldRect(woorm);
                AddSpecialStatusFieldRect(woorm);
                AddSpecialPrintOnLetterHeadFieldRect(woorm);
            }
        }

        /// ---------------------------------------------------------------------------------
        /// <summary>
        /// Metodo che aggiunge un campo di tipo FieldRect Dummy nascosto per far si che il valore della variabile CurrentPageNumber 
        /// nella symboltable venga aggiornato su ogni pagina. Se manca l'oggetto grafico il valore non viene aggiornato 
        /// (vedere metodo RDEReader.SetElement che se non trova il corrispondente oggetto grafico del valore di symbolTable
        /// non lo aggiorna)
        /// </summary>
        /// 
        private void AddSpecialCurrentPageFieldRect(WoormDocument woorm)
        {
            FieldRect f = new FieldRect(woorm);
            f.IsHidden = true;
            f.InternalID = SpecialReportField.ID.PAGE;
            Add(f);
        }

        private void AddSpecialLastPageFieldRect(WoormDocument woorm)
        {
            FieldRect f = new FieldRect(woorm);
            f.IsHidden = true;
            f.InternalID = SpecialReportField.ID.LAST_PAGE;
            Add(f);
        }

        private void AddSpecialLayoutFieldRect(WoormDocument woorm)
        {
            FieldRect f = new FieldRect(woorm);
            f.IsHidden = true;
            f.InternalID = SpecialReportField.ID.LAYOUT;
            Add(f);
        }

        private void AddSpecialCurrentCopyFieldRect(WoormDocument woorm)
        {
            FieldRect f = new FieldRect(woorm);
            f.IsHidden = true;
            f.InternalID = SpecialReportField.ID.CURRENT_COPY;
            Add(f);
        }

        private void AddSpecialPrintOnLetterHeadFieldRect(WoormDocument woorm)
        {
            FieldRect f = new FieldRect(woorm);
            f.IsHidden = true;
            f.InternalID = SpecialReportField.ID.PRINT_ON_LETTERHEAD;
            Add(f);
        }

        private void AddSpecialStatusFieldRect(WoormDocument woorm)
        {
            FieldRect f = new FieldRect(woorm);
            f.IsHidden = true;
            f.InternalID = SpecialReportField.ID.STATUS;
            Add(f);
        }

        private void AddSpecialOwnerIDFieldRect(WoormDocument woorm)
        {
            FieldRect f = new FieldRect(woorm);
            f.IsHidden = true;
            f.InternalID = SpecialReportField.ID.OWNER;
            Add(f);
        }

        //---------------------------------------------------------------------
        internal void AddIDToDynamicStaticObjects()
        {
            foreach (BaseObj obj in this)
            {
                if (obj.InternalID <= 0 && obj.IsDynamic())
                {
                    obj.InternalID = document.SymbolTable.GetNewID();
                }

                if (obj is Repeater)
                    ((Repeater)obj).AddIDToDynamicStaticObjects();
            }
        }

        //---------------------------------------------------------------------
        internal void AnchorFieldToColumn(BaseRect baseRect)
        {
            if (baseRect.AnchorLeftColumnID == 0)
                return;

            if (baseRect.AnchorLeftColumnID == baseRect.AnchorRightColumnID)
                baseRect.AnchorRightColumnID = 0;

            Column leftCol = FindColumn(baseRect.AnchorLeftColumnID);
            Column rightCol = baseRect.AnchorRightColumnID > 0 ? FindColumn(baseRect.AnchorRightColumnID) : leftCol;
            if (leftCol == null)
            {
                if (rightCol == null)
                {
                    baseRect.AnchorLeftColumnID = baseRect.AnchorRightColumnID = 0;
                    return;
                }
                leftCol = rightCol;
            }

            Rectangle colLeftRect = leftCol.ColumnRect;
            if (leftCol != rightCol && rightCol.ColumnRect.Left < leftCol.ColumnRect.Left)
                rightCol = leftCol;

            if (leftCol.AnchoredRectList == null)
                leftCol.AnchoredRectList = new List<BaseRect>();
            leftCol.AnchoredRectList.Add(baseRect);
            baseRect.AnchorLeftColumnIndex = FindColumnIndex(baseRect.AnchorLeftColumnID);

            if (leftCol != rightCol)
            {
                if (rightCol.AnchoredRectList == null)
                    rightCol.AnchoredRectList = new List<BaseRect>();
                rightCol.AnchoredRectList.Add(baseRect);
                baseRect.AnchorRightColumnIndex = FindColumnIndex(baseRect.AnchorRightColumnID);
            }
            else baseRect.AnchorRightColumnIndex = baseRect.AnchorLeftColumnIndex;

            bool isFirstVisibleLeftCol = false; //TODO

            int left = leftCol.ColumnRect.Left;
            if (baseRect.Borders.Left && !isFirstVisibleLeftCol)   //bIgnoreBorder
                left -= baseRect.BorderPen.Width;

            baseRect.Rect = new Rectangle(  left, 
                                            baseRect.Rect.Top,
                                            rightCol.ColumnRect.Right - leftCol.ColumnRect.Left,
                                            baseRect.Rect.Height
                                         );
        }

        internal void AnchorFieldToColumn()
        {
            foreach (BaseObj obj in this)
            {
                if (obj is BaseRect)
                {
                    BaseRect pBaseRect = obj as BaseRect;
                    if (pBaseRect.AnchorLeftColumnID > 0)
                    {
                        AnchorFieldToColumn(pBaseRect);
                    }
                }

            }
        }

        //---------------------------------------------------------------------
        internal void CheckDynamicColumns()
        {
            foreach (BaseObj obj in this)
            {
                if (obj is Table)
                {
                    Table t = obj as Table;
                    t.CheckDynamicColumns();
                }
            }
        }

        //---------------------------------------------------------------------
        internal void SortLayoutObjectsOnPosition() //RSWEB TODO
        {
            foreach (BaseObj obj in this)
            {

            }
        }

        //---------------------------------------------------------------------
        internal void Unparse(Unparser unparser, bool isSavingTemplate, ref bool thereIsTemplate)
        {
            // write all elements CBaseObjArray* pObjects
            foreach (BaseObj obj in this)
            {
                if (obj.InheritByTemplate) //|| !obj.IsPersistent) //TODOLUCA
                    continue;

                if (isSavingTemplate)
                {
                    if (obj is Table)
                    {
                        Table table = new Table(obj as Table);

                        if (!table.IsTemplate)
                            continue;

                        thereIsTemplate = true;

                        table.MarkTemplateOverridden();
                        table.MergeTemplateColumns();
                        table.PurgeTemplateColumns();
                        //obj = table;  inutile?
                    }
                    else if (obj is BaseRect)
                    {
                        BaseRect rect = obj as BaseRect;
                        if (!rect.IsTemplate)
                            continue;

                        thereIsTemplate = true;

                        rect.MarkTemplateOverridden();
                    }
                }

                obj.Unparse(unparser);
            }
        }

        //---------------------------------------------------------------------
        override public string ToJson(bool template, string name, bool bracket = false, bool array = true)
        {
            this.CheckDynamicColumns();  //RSWEB TODO dynamic width column/anchored fields

            string s = string.Empty;
            if (!name.IsNullOrEmpty())
                s = '\"' + name + "\":";

            s += '{' +
                    Name.ToJson("name") + ',' +
                    base.ToJson(template, "objects") +
                    '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

     }
}

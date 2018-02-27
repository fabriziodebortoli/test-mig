using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

using TaskBuilderNetCore.Interfaces;

using Microarea.Common.CoreTypes;
using Microarea.Common.Generic;
using Microarea.Common.Lexan;
using Microarea.Common.Hotlink;

namespace Microarea.RSWeb.WoormEngine
{
    /// <summary>
    /// ProcedureSymbolTable.
    /// </summary>
    //============================================================================
    public class ProcedureSymbolTable : List<Procedure>
	{
		//-----------------------------------------------------------------------------
		public Procedure Find(string name) 
		{ 
			foreach (Procedure procedure in this)
				if (string.Compare(name, procedure.PublicName,StringComparison.OrdinalIgnoreCase) == 0)
					return procedure;

			return null;
		}

        //---------------------------------------------------------------------------
        public bool Unparse(Unparser unparser)
        {
            if (this.Count == 0)
                return true;

            unparser.WriteLine();
            unparser.WriteTag(Token.PROCEDURES);

			unparser.IncTab();
            unparser.WriteBegin();

			unparser.IncTab();
			foreach (Procedure proc in this)
                proc.Unparse(unparser);
			unparser.DecTab();

			unparser.WriteEnd();
			unparser.DecTab();

            return true;
        }
	}

	/// <summary>
	/// QueryObjectSymbolTable.
	/// </summary>
	//============================================================================
	public class QueryObjectSymbolTable : List<QueryObject>
	{
		//-----------------------------------------------------------------------------
		public QueryObject Find(string name) 
		{ 
			foreach (QueryObject queryObject in this)
				if (string.Compare(name, queryObject.Name, StringComparison.OrdinalIgnoreCase) == 0)
					return queryObject;

			return null;
		}

		//-----------------------------------------------------------------------------
		public bool Unparse(Unparser unparser)
		{
            if (this.Count == 0)
                return true;

            unparser.WriteLine();
			unparser.WriteTag(Token.QUERIES);
			
			unparser.IncTab();
			unparser.WriteBegin();

			unparser.IncTab();
			foreach (QueryObject queryObject in this)
				queryObject.Unparse(unparser);
			unparser.DecTab();

			unparser.WriteEnd();
			unparser.DecTab();
			
			return true;
		}
	}

	/// <summary>
	/// FieldSymbolTable.
	/// </summary>
	//============================================================================
	//[Serializable]
	//[KnownType(typeof(Dictionary<string, Variable>))]
	//[KnownType(typeof(Field))]
    public class FieldSymbolTable : SymbolTable//, ISerializable
    {
        public FieldSymbolTable()
        {
            AddAlias("Woorm", "Framework.TbWoormViewer.TbWoormViewer");
        }

        public FieldSymbolTable(FieldSymbolTable parent)
            : base(parent)
        {
            if (parent == null)
                AddAlias("Woorm", "Framework.TbWoormViewer.TbWoormViewer");
        }

			//--------------------------------------------------------------------------
		//public FieldSymbolTable(SerializationInfo info, StreamingContext context)
		//{
		//	//error = info.GetBoolean(ERROR);
		//}

		////--------------------------------------------------------------------------
		//public override void GetObjectData(SerializationInfo info, StreamingContext context)
		//{
		//	base.GetObjectData(info, context);
		//}

        //-----------------------------------------------------------------------------
        public new Field Find(string name)
        {
            return base.Find(name) as Field;
        }

        //-----------------------------------------------------------------------------
        public override IFunctionPrototype ResolveCallMethod(string name, out string handleName)
        {
            handleName = string.Empty;

            int idx = name.IndexOf('.');
            if (idx < 0)
                return null;
            handleName = name.Left(idx);
            name = name.Mid(idx + 1);

            Field f = Find(handleName);
            if (f == null)
                return null;
            if (f.DataType != "Long" && f.DataType != "Var" && f.DataType != "Int64")
                return null;

            return f.FindMethod(name);
        }
    }

	/// <summary>
	/// DisplayTableSymbolTable.
	/// </summary>
	//============================================================================
	public class DisplayTableSymbolTable : List<DisplayTable>
	{
		//-----------------------------------------------------------------------------
		public DisplayTable Find(string name) 
		{ 
			foreach (DisplayTable displayTable in this)
                if (name.CompareNoCase(displayTable.PublicName))
					return displayTable;

			return null;
		}
        //-----------------------------------------------------------------------------
        public DisplayTable Find(ushort id)
        {
            foreach (DisplayTable displayTable in this)
                if (id == displayTable.InternalId)
                    return displayTable;

            return null;
        }

        //-----------------------------------------------------------------------------
        public DisplayTable Find(ushort id, string layout)
        {
            foreach (DisplayTable displayTable in this)
                if (
                    (id == displayTable.InternalId)
                    &&
                    layout.CompareNoCase(displayTable.LayoutTable)
                    )
                    return displayTable;

            return null;
        }

		//-----------------------------------------------------------------------------
		public DisplayTable Find(string name, string layout)
		{
			foreach (DisplayTable displayTable in this)
				if (
					name.CompareNoCase(displayTable.PublicName)
					&&
                    layout.CompareNoCase(displayTable.LayoutTable)
					)
					return displayTable;

			return null;
		}

		//-----------------------------------------------------------------------------
		public bool Unparse(Unparser unparser)
		{
			return true;
		}

        //-----------------------------------------------------------------------------
        public void ResetAllRowsCounter()
        {
            foreach (DisplayTable displayTable in this)
            {
                displayTable.ResetRowsCounter();
            }
        }

        //-----------------------------------------------------------------------------
        internal bool IsEmpty
		{
			get
			{
				foreach (DisplayTable displayTable in this)
					if (!displayTable.TableActions.IsEmpty)
						return false;

				return true;
			}
		}
	}

	/// <summary>
	/// RepSymTable.
	/// </summary>
	//============================================================================
	[Serializable]
	[KnownType(typeof(FieldSymbolTable))]
	public class RepSymTable : ISerializable
	{
		const string FIELDS = "fields";
		
		public FieldSymbolTable			AskDialogState; // memorizza Fields dopo il run delle AskDialog
		public FieldSymbolTable			Fields;
		public ProcedureSymbolTable		Procedures;
		public QueryObjectSymbolTable	QueryObjects;
		public DisplayTableSymbolTable	DisplayTables;

		public int DisplayTablesNum { get { return DisplayTables != null ? DisplayTables.Count : 0; } }

		//-----------------------------------------------------------------------------
		public RepSymTable()
		{
			Fields          = new FieldSymbolTable();

			AskDialogState  = new FieldSymbolTable();
			Procedures      = new ProcedureSymbolTable();
			DisplayTables   = new DisplayTableSymbolTable();
			QueryObjects    = new QueryObjectSymbolTable();
		}

        //-----------------------------------------------------------------------------
        public RepSymTable(RepSymTable parent)
        {
            Fields          = new FieldSymbolTable(parent.Fields);

            AskDialogState  = parent.AskDialogState;
            Procedures      = parent.Procedures;
            DisplayTables   = parent.DisplayTables;
            QueryObjects    = parent.QueryObjects;
        }

		//--------------------------------------------------------------------------
		public RepSymTable(SerializationInfo info, StreamingContext context)
		{
			//error = info.GetBoolean(ERROR);
		}

		//--------------------------------------------------------------------------
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(FIELDS, Fields);
		}

		// salva lo stato della symbol table delle variabili dopo aver eseguito le AskDialog
		//-----------------------------------------------------------------------------
		public void SaveAskDialogFieldsState()
		{
			AskDialogState.Clear();
			foreach (Field field in Fields)
				AskDialogState.Add(field);
		}

		///<summary>
		///Metodo che associa per ogni  field in symboltable la DisplayTable che appartiene al layout passato come argomento
		/// </summary>
		//-----------------------------------------------------------------------------
		public void ReattachCurrentDisplayTable(string layout)
		{
			foreach (Field f in AskDialogState)
			{
				f.ReattachCurrentDisplayTable(layout);
			}
		}
		
		//-----------------------------------------------------------------------------
		public bool IsEmpty 
		{ 
			get 
			{
				return Fields.Count <= 0;
				//return Fields.Count <=  m_nCountSpecialFields;  m_nCountSpecialFields in cpp è uguale a GetSize
				//le parti in cui viene fatto ++ o -- sono commentate
			}
		}

        //-----------------------------------------------------------------------------
        public Procedure ResolveCallProcedure(string name)
        {
            Procedure p = Procedures.Find(name);
            return p;
        }

        //-----------------------------------------------------------------------------
        /*
        public FunctionPrototype ResolveCallQuery(string name, out string handleName, FunctionsList funList)
        {
            handleName = string.Empty;

            int idx = name.IndexOf('.');
            if (idx < 0)
                return null;
            string  qName = name.Left(idx);
            string sFuncName = name.Mid(idx + 1);

            QueryObject q = QueryObjects.Find(qName);
            if (q == null)
                return null;

            FunctionPrototype fp = null;

            if (sFuncName.CompareNoCase("ReadOne"))
                fp = funList.GetPrototype("Framework.TbWoormViewer.TbWoormViewer.QueryReadOne");
            else if (sFuncName.CompareNoCase("Read"))
                fp = funList.GetPrototype("Framework.TbWoormViewer.TbWoormViewer.QueryRead");
            else if (sFuncName.CompareNoCase("Open"))
                fp = funList.GetPrototype("Framework.TbWoormViewer.TbWoormViewer.QueryOpen");
            else if (sFuncName.CompareNoCase("Close"))
                fp = funList.GetPrototype("Framework.TbWoormViewer.TbWoormViewer.QueryClose");
            else if (sFuncName.CompareNoCase("Execute"))
                fp = funList.GetPrototype("Framework.TbWoormViewer.TbWoormViewer.QueryExecute");
            else if (sFuncName.CompareNoCase("IsOpen"))
                fp = funList.GetPrototype("Framework.TbWoormViewer.TbWoormViewer.QueryIsOpen");
           
            if (fp != null)
                handleName = '\"' + qName + '\"';

            return fp;
        }
        */
        
        //-----------------------------------------------------------------------------
        public bool ResolveCallQuery(string name, FunctionsList functionList, out string handleName, out FunctionPrototype fp)
        {
            handleName = string.Empty;
            fp = null;

            int idx = name.IndexOf('.');
            if (idx < 0)
                return false;
            string qName = name.Left(idx);
            string sFuncName = name.Mid(idx + 1);

            QueryObject q = QueryObjects.Find(qName);
            if (q == null)
                return false;

            if (sFuncName.CompareNoCase("ReadOne"))
                fp = functionList.GetPrototype("Framework.TbWoormViewer.TbWoormViewer.QueryReadOne");
            else if (sFuncName.CompareNoCase("Read"))
                fp = functionList.GetPrototype("Framework.TbWoormViewer.TbWoormViewer.QueryRead");
            else if (sFuncName.CompareNoCase("Open"))
                fp = functionList.GetPrototype("Framework.TbWoormViewer.TbWoormViewer.QueryOpen");
            else if (sFuncName.CompareNoCase("Close"))
                fp = functionList.GetPrototype("Framework.TbWoormViewer.TbWoormViewer.QueryClose");
            else if (sFuncName.CompareNoCase("Execute"))
                fp = functionList.GetPrototype("Framework.TbWoormViewer.TbWoormViewer.QueryExecute");
            else if (sFuncName.CompareNoCase("IsOpen"))
                fp = functionList.GetPrototype("Framework.TbWoormViewer.TbWoormViewer.QueryIsOpen");
            else if (sFuncName.CompareNoCase("Call"))
                fp = functionList.GetPrototype("Framework.TbWoormViewer.TbWoormViewer.QueryCall");

            if (fp != null)
            {
                //handleName = '\"' + qName + '\"';
                handleName = qName;
                return true;
            }
            return false;
        }		
	}
}

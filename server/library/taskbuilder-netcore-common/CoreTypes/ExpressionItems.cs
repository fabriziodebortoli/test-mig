using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;



using Microarea.Common.Generic;
using TaskBuilderNetCore.Interfaces.Model;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.Common.CoreTypes
{
	/// <summary>
	/// Summary description for Item.
	/// </summary>
	public class Item //: ICloneable
	{
		//-----------------------------------------------------------------------------
		public Item () {  }
		public virtual object Clone() { return base.MemberwiseClone(); }
		public virtual Item	  Expand(){ return (Item)Clone(); }
	}

	//============================================================================
	public class DataItem : Item
	{
		protected object	data;
		protected bool		valid = true;

		public CultureInfo CollateCulture = CultureInfo.InvariantCulture;
		
		//test of culture infos used for comparing strings: first value is predominant, specific culture too
		//-----------------------------------------------------------------------------
		protected static CultureInfo GetCollateCulture(DataItem v1, DataItem v2)
		{
			if (v1.CollateCulture != CultureInfo.InvariantCulture)
				return v1.CollateCulture;
			else if (v2.CollateCulture != CultureInfo.InvariantCulture)
				return v2.CollateCulture;
			else
				return CultureInfo.InvariantCulture;
		}

		//-----------------------------------------------------------------------------
		public DataItem () : base() { this.data = null;}
		public DataItem (object data) : base() { this.data = data;}
		public override object Clone() { return base.MemberwiseClone(); }

		//-----------------------------------------------------------------------------
        public virtual string DataType 
        { 
            get { 
                    if (Data == null)
                        return string.Empty;

                    if (Data is IDataObj && !(Data is DataEnum) && !(Data is DataArray))
                    {
                        IDataObj d = Data as IDataObj;
                        return d.Value != null ? d.Value.GetType().Name : string.Empty;
                    }

                    return Data.GetType().Name; 
            } 
        }	

		//-----------------------------------------------------------------------------
		virtual public object	Data { get { return data; } set { data = value; }}
		virtual public bool		Valid { get { return valid; } set { valid = value; }}
		
		//-----------------------------------------------------------------------------
		public bool AssignArrayData(DataItem aData, DataItem aIdx) 
		{ 
			Valid = false;
			if (!aData.Valid || !aIdx.Valid)
				return false;
			int idx = Convert.ToInt32(aIdx);
			if (idx < 0)
			{
				return false;
			}

			DataArray ar = (DataArray) Data ;
			ar.SetAtGrow(idx, aData.Data);

			Valid = true;
			return true;
		}
		//-----------------------------------------------------------------------------

	}

	// anche internamente deve essere usata la proerty perchè è virtuale
	//============================================================================
	[Serializable]
	[KnownType(typeof(DataEnum))]
	public class Variable : DataItem, IComparable, ISerializable
	{
		const string ID = "id";
		const string NAME = "name";

		protected string	name;
		protected ushort	id = 0;
		protected bool		isColumn2 = false;
        public ushort       EnumTag = 0;    // used by Enums
        protected string    woormType = string.Empty;
        public string       BaseType = string.Empty;
                                       //-----------------------------------------------------------------------------
        public string       WoormType   { 
            get {
                if (woormType == string.Empty)
                    return DataType;

                return woormType; 
            } 
            set { 
                woormType = value;

                //if (woormType == "String" && DataType != "String" && DataType != string.Empty)
                //{
                //    System.Diagnostics.Debug.Fail("field with inconsisten data type:" + name);
                //}
            } 
        }
		public string	        Name        { get { return name; } set { name = value; }}
		public ushort           Id          { get { return id;} set { id = value; } }
		public virtual bool     IsColumn2   { get { return isColumn2; } set { isColumn2 = value; } }
		
        //-----------------------------------------------------------------------------
		public Variable(string name, object data) : base(data)
		{
			this.name = name;
			this.id = 0;
		}

		//-----------------------------------------------------------------------------
		public Variable(string name) : base()
		{
			this.name = name;
			this.id = 0;
		}

		//-----------------------------------------------------------------------------
		public Variable(string name, ushort id) : base()
		{
			this.name = name;
			this.id = id;
		}

		//-----------------------------------------------------------------------------
		public Variable(string name, object data, ushort id) : base(data)
		{
			this.name = name;
			this.id = id;
		}

		//-----------------------------------------------------------------------------
		public Variable() : base()
		{
			//TODO SILVANO
		}

		//-----------------------------------------------------------------------------
		public Variable(SerializationInfo info,StreamingContext context)
		{
			id = info.GetUInt16(ID);
			name = info.GetString(NAME);
		}

		//-----------------------------------------------------------------------------
		public virtual void GetObjectData(SerializationInfo info,StreamingContext context)
		{
			info.AddValue(ID, id); 
			info.AddValue(NAME, name);
		}
		//-----------------------------------------------------------------------------
		public override object Clone()
		{
			Variable v = new Variable(Name, Data);
			CopyProperties(v);
			return v;
		}

		//-----------------------------------------------------------------------------
		protected void CopyProperties(Variable v)
		{
			v.CollateCulture = this.CollateCulture;
			v.IsColumn2 = this.IsColumn2;
			v.Id = this.Id;
            v.woormType = this.WoormType;
            v.EnumTag = this.EnumTag;
        }

		//-----------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return Name.ToLower().GetHashCode();
		}

		//-----------------------------------------------------------------------------
		public override bool Equals(object item)
		{
			if (!(item is Variable)) return false;
			Variable itemVar =  item as Variable;

			return string.Compare(Name, itemVar.Name, StringComparison.OrdinalIgnoreCase) == 0;
		}

		//-----------------------------------------------------------------------------
		public override string ToString()
		{
			return Data.ToString();
		}

        //-----------------------------------------------------------------------------
        public int CompareTo(object obj)
        {
            Variable v = obj as Variable;
            if (v != null)
                return string.Compare(Name, v.Name, StringComparison.OrdinalIgnoreCase);
            else
                throw new ArgumentException("Object is not a Temperature");
        }

		//-----------------------------------------------------------------------------
        public virtual bool IsRuleFields() 
        {
            return false;
        }
	}

	//============================================================================
	/// <summary>
	/// Aggiunge alla classe variable informazioni  circa l'utilizzo del campo nel report
	/// </summary>
	public class ExtendedVariable : Variable
	{
		public ExtendedVariable(string name)
			: base(name)
		{

		}
		public bool IsTableRuleField { get; set; }
		public bool IsExpressionRuleField { get; set; }
		public bool IsFunctionField { get; set; }
		public bool IsAskField { get; set; }
	}

	// anche internamente deve essere usata la property perchè è virtuale
	//============================================================================
	public class Value : DataItem
	{
        public bool IsVariant = false;

		//-----------------------------------------------------------------------------
		public Value() : base ()
		{
		}

		//-----------------------------------------------------------------------------
		public Value(object data) : base (data)
		{
		}

		//-----------------------------------------------------------------------------
		public override object Clone()
		{
			Value v = new Value(Data);
			v.CollateCulture = this.CollateCulture;
			return v;
		}

		//-----------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		//-----------------------------------------------------------------------------
		public override bool Equals(object item)
		{
			if (!(item is Value)) return false;
			Value itemConstant = item as Value;

			return	
				Data != null &&
				Value.Equals(this.Data, itemConstant.Data);
		}

		//-----------------------------------------------------------------------------
		public override string ToString()
		{
			return Data.ToString();
		}

		//-----------------------------------------------------------------------------
		public static bool operator >=(Value v1, Value v2)	{ return v1 == v2 || v1 > v2; }
		public static bool operator < (Value v1, Value v2)	{ return !(v1 >= v2); }
		public static bool operator <=(Value v1, Value v2)	{ return v1 == v2 || !(v1  > v2); }
		public static bool operator !=(Value v1, Value v2)	{ return !(v1 == v2); }

		// Gestisce anche il caso di Boolean rappresentati da String ("1","0")
		//-----------------------------------------------------------------------------
		public static bool operator ==(Value v1, Value v2)
		{
			if (object.Equals(v2, null) && object.Equals(v1, null)) return true;
			if (object.Equals(v2, null) && !object.Equals(v1, null)) return false;
			if (object.Equals(v1, null) && !object.Equals(v2, null)) return false;

			string t1 = v1.DataType;
			string t2 = v2.DataType;

			switch (t1)
			{
				case "String": 
                    {
					switch (t2)
					{
						case "String": return string.Compare((string)v1.Data, (string)v2.Data, false) == 0;
						case "Boolean":	return ObjectHelper.CastBool(v1.Data) == ((bool)v2.Data);
						default:		break;
					}
					break;
                    }
				case "Double":
                    {
                        switch (t2)
                        {
                            case "Double": return ObjectHelper.IsDblEquals((double)v1.Data, (double)v2.Data);
                            case "Int16": return (double)v1.Data == (short)v2.Data;
                            case "Int32": return (double)v1.Data == (int)v2.Data;
                            case "Int64": return (double)v1.Data == (long)v2.Data;
                            case "Single": return (double)v1.Data == (float)v2.Data;
                            default: break;
                        }
                        break;
                    }
				case "DateTime":
                    {
					switch (t2)
					{
						case "DateTime": return (DateTime)v1.Data == (DateTime)v2.Data;
						default: break;
					}
					break;
				}
				case "DataEnum":
					{
					switch (t2)
					{
						case "DataEnum": return (DataEnum)v1.Data == (DataEnum)v2.Data;
						case "Int32": return (uint)((DataEnum)v1.Data) == (int)v2.Data;
                        case "Int64": return (uint)((DataEnum)v1.Data) == (long)v2.Data;
						default: break;
					}
					break;
				}
				case "Boolean":
					{
				    switch (t2)
				    {
					    case "Boolean":	return (bool)v1.Data == (bool)v2.Data;
					    case "String":	return ObjectHelper.CastBool(v2.Data) == ((bool)v1.Data);
					    default:		break;
				    }
					break;
				}

				case "Int16":
				{
				    switch (t2)
				    {
					    case "Int16":		return (short)v1.Data == (short)v2.Data;
					    case "Int32":		return (short)v1.Data == (int)v2.Data;
					    case "Int64":		return (short)v1.Data == (long)v2.Data;
					    case "Double":		return (short)v1.Data == (double)v2.Data;
					    case "Single":		return (short)v1.Data == (float)v2.Data;
					    default:			break;
				    }
					break;
			    }
				case "Int32":
				{
				    switch (t2)
				    {
					    case "Int32":		return (int)v1.Data == (int)v2.Data;
					    case "Int16":		return (int)v1.Data == (short)v2.Data;
					    case "Int64":		return (int)v1.Data == (long)v2.Data;
					    case "Double":		return (int)v1.Data == (double)v2.Data;
					    case "DataEnum":	return (int)v1.Data == (uint)((DataEnum)v2.Data);
					    case "Single":		return (int)v1.Data == (float)v2.Data;
					    default:			break;
				    }
					break;
				}
				case "Int64":
				{
				    switch (t2)
				    {
					    case "Int64":	return (long)v1.Data == (long)v2.Data;
					    case "Int16":	return (long)v1.Data == (short)v2.Data;
					    case "Int32":	return (long)v1.Data == (int)v2.Data;
					    case "Double":	return (long)v1.Data == (double)v2.Data;
					    case "DataEnum":return (long)v1.Data == (uint)((DataEnum)v2.Data);
					    case "Single":	return (long)v1.Data == (float)v2.Data;
					    default:		break;
				    }
					break;
				}
				case "Single":
				{
				    switch (t2)
				    {
					    case "Single":	return (float)v1.Data == (float)v2.Data;
					    case "Double":	return (float)v1.Data == (double)v2.Data;
					    case "Int16":	return (float)v1.Data == (short)v2.Data;
					    case "Int32":	return (float)v1.Data == (int)v2.Data;
					    case "Int64":	return (float)v1.Data == (long)v2.Data;
					    default:		break;
				    }
					break;
			    }
			}
			return false;
		}			

		//-----------------------------------------------------------------------------
		public static bool operator >(Value v1, Value v2)
		{
			if (object.Equals(v2, null) && object.Equals(v1, null)) return false;
			if (object.Equals(v2, null) && !object.Equals(v1, null)) return false;
			if (object.Equals(v1, null) && !object.Equals(v2, null)) return false;
			

			//Senza si tromba il libro brogliaccio
			if (v1.Data == null || v2.Data == null)
				return false;
			
			string t1 = v1.DataType;
			string t2 = v2.DataType;

			switch (t1)
			{
				case "Double":
					switch (t2)
					{
						case "Double": return ObjectHelper.IsDblGreater((double)v1.Data, (double)v2.Data);
						case "Int16": return (double)v1.Data > (short)v2.Data;
						case "Int32": return (double)v1.Data > (int)v2.Data;
						case "Int64": return (double)v1.Data > (long)v2.Data;
						case "Single": return (double)v1.Data > (float)v2.Data;
						default: break;
					}
					break;

				case "DateTime":
					switch (t2)
					{
						case "DateTime": return (DateTime)v1.Data > (DateTime)v2.Data;
						default: break;
					}
					break;
				case "String": 
				switch (t2)
				{
					case "String":	
					{
						string s1 = ((string)v1.Data);
						string s2 = ((string)v2.Data);
						return string.Compare(s1, s2, StringComparison.OrdinalIgnoreCase) > 0;
					}

					default: 
						break;
				}
					break;

				case "Int16":
				switch (t2)
				{
					case "Int16":	return (short)v1.Data > (short)v2.Data;
					case "Int32":	return (short)v1.Data > (int)v2.Data;
					case "Int64":	return (short)v1.Data > (long)v2.Data;
					case "Double":	return (short)v1.Data > (double)v2.Data;
					case "Single":	return (short)v1.Data > (float)v2.Data;
					default:		break;
				}
					break;

				case "Int32":
				switch (t2)
				{
					case "Int32":	return (int)v1.Data > (int)v2.Data;
					case "Int16":	return (int)v1.Data > (short)v2.Data;
					case "Int64":	return (int)v1.Data > (long)v2.Data;
					case "Double":	return (int)v1.Data > (double)v2.Data;
					case "Single":	return (int)v1.Data > (float)v2.Data;
					default:		break;
				}
					break;

				case "Int64":
				switch (t2)
				{
					case "Int64":	return (long)v1.Data > (long)v2.Data;
					case "Int16":	return (long)v1.Data > (short)v2.Data;
					case "Int32":	return (long)v1.Data > (int)v2.Data;
					case "Double":	return (long)v1.Data > (double)v2.Data;
					case "Single":	return (long)v1.Data > (float)v2.Data;
					default:		break;
				}
					break;

				case "Single":
				switch (t2)
				{
					case "Int16":	return (float)v1.Data > (short)v2.Data;
					case "Int32":	return (float)v1.Data > (int)v2.Data;
					case "Int64":	return (float)v1.Data > (long)v2.Data;
					case "Single":	return (float)v1.Data > (float)v2.Data;
					case "Double":	return (float)v1.Data > (double)v2.Data;
					default:		break;
				}
					break;
			}
			return false;
		}			
	}

    //============================================================================

    ///<summary>
    ///Classe contenitore dei nomi e degli ID delle variabili speciali di report
    ///ATTENZIONE: TENERE ALLINEATO CON "TaskBuilder\Framework\TbWoormEngine\RepTable.h"
    /// </summary>
    //==============================================================================
    public class SpecialReportField
    {
        public const ushort NO_INTERNAL_ID = 0xFFFF;
        public struct ID {        //ID riservati da Woorm
            public const ushort STATUS = 0x7FFe;
            public const ushort OWNER = 0x7FFd;
            public const ushort LAYOUT = 0x7FFc;
            public const ushort PAGE = 0x7FFb;
            public const ushort LINKED_DOC = 0x7FFa;
            public const ushort IS_PRINTING = 0x7FF9;
            public const ushort USE_DEFAULT_ATTRIBUTE = 0x7FF8;
            public const ushort LAST_PAGE = 0x7FF7;
            public const ushort EA_BARCODE = 0x7FF6;
            public const ushort IS_ARCHIVING = 0x7FF5;
            public const ushort IS_EXPORTING = 0x7FF4;
            public const ushort FUNCTION_RETURN_VALUE = 0x7FF3;
            public const ushort HIDE_ALL_ASK_DIALOGS = 0x7FF2;
            public const ushort PRINT_ON_LETTERHEAD   = 0x7FF1;
            public const ushort IS_FIRST_TUPLE = 0x7FF0;
            public const ushort IS_LAST_TUPLE = 0x7FEF;
            public const ushort CURRENT_COPY = 0x7FEE;
            public const ushort EMPTY_COLUMN = 0x7FED;
        };
        //the latest used id
        public const ushort REPORT_LOWER_SPECIAL_ID = 0x7FEC;
        //nomi delle variabili riservate
        public const string REPORT_DEFAULT_LAYOUT_NAME                      = "default";
        public struct NAME
        {
            public const string STATUS                  = "ReportStatus";
            public const string OWNER                   = "OwnerID";
            public const string LAYOUT                  = "ReportLayout";
            public const string PAGE                    = "ReportCurrentPageNumber";
            public const string LAST_PAGE               = "ReportLastPageNumber";
            public const string IS_PRINTING             = "ReportIsPrinting";
            public const string IS_ARCHIVING            = "ReportIsArchiving";
            public const string PRINT_ON_LETTERHEAD     = "PrintOnLetterHead";
            public const string USE_DEFAULT_ATTRIBUTE   = "UseDefaultAttribute";
            public const string LINKED_DOC_ID           = "LinkedDocumentID";
            public const string FUNCTION_RETURN_VALUE   = "_ReturnValue";
            public const string HIDE_ALL_ASK_DIALOGS    = "_HideAllAskDialogs";
            public const string EMPTY_COLUMN            = "_EmptyColumn";
            public const string IS_FIRST_TUPLE          = "IsFirstTuple";
            public const string IS_LAST_TUPLE           = "IsLastTuple";
            public const string CURRENT_COPY            = "CurrentCopyNumber";
        };
    }
    //============================================================================
    public class ValueContentOf : Value
	{
		//-----------------------------------------------------------------------------
		public ValueContentOf()
			: base()
		{
		}

		//-----------------------------------------------------------------------------
		public ValueContentOf(object data)
			: base(data)
		{
		}
		
		//-----------------------------------------------------------------------------
		public override object Clone()
		{
			ValueContentOf v = new ValueContentOf(Data);
			v.CollateCulture = this.CollateCulture;
			return v;
		}
	}


	//[Serializable]
	//[KnownType(typeof(Dictionary<string, Variable>))]
	//[KnownType(typeof(Variable))]
	public class SymbolTable : IEnumerable//, ISerializable
	{
		const string SYMBOLS = "symbols";
        
		protected Dictionary<string, string>   alias   = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        protected Dictionary<string, Variable> symbols = new Dictionary<string, Variable>(StringComparer.OrdinalIgnoreCase);

        private ArrayList localizableStrings = new ArrayList();
        protected SymbolTable parent = null;

        public ushort maxID = 1000;
        public ushort GetNewID() { return ++maxID; }

        public SymbolTable Parent { get { return parent; } set { parent = value; } }
		public int Count { get { return symbols.Keys.Count; } }

        //-----------------------------------------------------------------------------
        public ArrayList LocalizableStrings { get { return this.parent != null ? this.parent.LocalizableStrings : localizableStrings; } }

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return symbols.Values.GetEnumerator();
        }

        #endregion

		//-----------------------------------------------------------------------------
		public SymbolTable([Optional] SymbolTable parent)
		{
			this.parent = parent;
		}

		//--------------------------------------------------------------------------
		//public SymbolTable(SerializationInfo info, StreamingContext context)
		//{
		//	//TODO SILVANO
		//}

		////--------------------------------------------------------------------------
		//public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		//{
		//	info.AddValue(SYMBOLS, symbols);
		//}
		//-----------------------------------------------------------------------------
		public virtual void Add(Variable v)
		{
			symbols.Add(v.Name, v);

            if (maxID < v.Id && v.Id < SpecialReportField.REPORT_LOWER_SPECIAL_ID) 
                maxID = v.Id;
        }
        
        //-----------------------------------------------------------------------------
        public virtual void AddAlias(string k, string v)
        {
            alias.Add(k, v);
        }

 		//-----------------------------------------------------------------------------
		public void Remove(Variable v)
		{
			symbols.Remove(v.Name);
		}

		//-----------------------------------------------------------------------------
		public virtual Variable Find(string name)
		{
			Variable v = null;
			if (symbols.TryGetValue(name, out v) && v != null)
				return v;
			return parent != null ? parent.Find(name) : null;
		}

		//-----------------------------------------------------------------------------
		public void Clear()
		{
			symbols.Clear();
		}

		//-----------------------------------------------------------------------------
		public virtual string GetMemberDataType(string name)
		{
			Variable v = Find(name);
			return v != null ? v.DataType : string.Empty;
		}

		//-----------------------------------------------------------------------------
		public Variable FindById(ushort id)
		{
			foreach (Variable nm in this)
				if (id == nm.Id)
					return nm;

			return parent != null ? parent.FindById(id) : null;
		}

		//-----------------------------------------------------------------------------
		public bool Contains(string name)
		{
			return Find(name) != null;
		}

		//-----------------------------------------------------------------------------
		public bool Contains(ushort id)
		{
			return FindById(id) != null;
		}

        //-----------------------------------------------------------------------------
        public virtual IFunctionPrototype ResolveCallMethod(string name, out string handleName)
        {
            //will be overridden
            handleName = string.Empty;
            return null;
        }

        //-----------------------------------------------------------------------------
        public virtual bool ResolveAlias(string name, out string expandName)
        {
            expandName = string.Empty;

            int idx = name.IndexOf('.');
            if (idx < 0)
                return false;

            string prefix = name.Left(idx);
            string funcName = name.Mid(idx);    //conserva il .

            if (alias.TryGetValue(prefix, out expandName))
            {
                expandName = expandName + funcName;
                return true;
            }

            return parent != null ? parent.ResolveAlias(name, out expandName) : false;
        }
	}
}

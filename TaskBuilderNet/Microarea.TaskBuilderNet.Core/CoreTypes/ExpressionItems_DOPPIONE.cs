using System;
using System.Globalization;

namespace Microarea.TaskBuilderNet.Core.CoreTypes_DOPPIONE
{
	/// <summary>
	/// Summary description for Item.
	/// </summary>
	public class Item : ICloneable
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
		public string DataType { get { return Data.GetType().Name; }}	

		//-----------------------------------------------------------------------------
		virtual public object	Data { get { return data; } set { data = value; }}
		virtual public bool		Valid { get { return valid; } set { valid = value; }}
	}

	// anche internamente deve essere usata la proerty perchè è virtuale
	//============================================================================
	public class Variable : DataItem, IComparable
	{
		protected string	name;
		protected short		id = 0;
		protected bool		isColumn2 = false;

		//-----------------------------------------------------------------------------
		public string	Name { get { return name; } set { name = value; }}
		public short Id { get { return id;} set { id = value; } }
		public virtual bool IsColumn2 { get { return isColumn2; } set { isColumn2 = value; } }

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
		public Variable(string name, short id) : base()
		{
			this.name = name;
			this.id = id;
		}

		//-----------------------------------------------------------------------------
		public Variable(string name, object data, short id) : base(data)
		{
			this.name = name;
			this.id = id;
		}

		//-----------------------------------------------------------------------------
		public override object Clone()
		{
			Variable v = new Variable(Name, Data, Id);
			v.CollateCulture = this.CollateCulture;
			v.isColumn2 = this.isColumn2;
			return v;
		}

		//-----------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return Name.ToLower(CollateCulture).GetHashCode();
		}

		//-----------------------------------------------------------------------------
		public override bool Equals(object item)
		{
			if (!(item is Variable)) return false;
			Variable itemVar =  item as Variable;

			return string.Compare(Name, itemVar.Name, true, GetCollateCulture(this, itemVar)) == 0;
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
                return string.Compare(Name, v.Name, true, GetCollateCulture(this, v));
            else
                throw new ArgumentException("Object is not a Temperature");
        }

	}

	// anche internamente deve essere usata la property perchè è virtuale
	//============================================================================
	public class Value : DataItem
	{
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
            if ((object)v1 == null && (object)v2 == null) return true;
            if ((object)v1 == null || (object)v2 == null) return false;

			//string t1 = v1.DataType;
			string t2 = v2.DataType;

            switch (v1.DataType)
            {
                case "String":
                    {
                        switch (t2)
                        {
                            case "String": return string.Compare((string)v1.Data, (string)v2.Data, false) == 0;
                            case "Boolean": return ObjectHelper.CastBool(v1.Data) == ((bool)v2.Data);
                            default: break;
                        }
                        break;
                    }
                case "Double":
                    { 
                        switch (t2)
                        {
                            case "Double": return (double)v1.Data == (double)v2.Data;
                            case "Int16": return (double)v1.Data == (short)v2.Data;
                            case "Int32": return (double)v1.Data == (int)v2.Data;
                            case "Int64": return (double)v1.Data == (long)v2.Data;
                            case "Single": return (double)v1.Data == (float)v2.Data;
                               default: break;
                        }
                        break;

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
                            case "Int32": return (int)((DataEnum)v1.Data) == (int)v2.Data;
                            case "Int64": return (long)((DataEnum)v1.Data) == (long)v2.Data;
                            default: break;
                        }
                        break;
                    }
                case "Boolean":
                    {
                        switch (t2)
                        {
                            case "Boolean": return (bool)v1.Data == (bool)v2.Data;
                            case "String": return ObjectHelper.CastBool(v2.Data) == ((bool)v1.Data);
                            default: break;
                        }
                        break;
                    }

                case "Int16":
                    {
                        switch (t2)
                        {
                            case "Int16": return (short)v1.Data == (short)v2.Data;
                            case "Int32": return (short)v1.Data == (int)v2.Data;
                            case "Int64": return (short)v1.Data == (long)v2.Data;
                            case "Double": return (short)v1.Data == (double)v2.Data;
                            case "Single": return (short)v1.Data == (float)v2.Data;
                            default: break;
                        }
                        break;
                    }
                case "Int32":
                    {
                        switch (t2)
                        {
                            case "Int32": return (int)v1.Data == (int)v2.Data;
                            case "Int16": return (int)v1.Data == (short)v2.Data;
                            case "Int64": return (int)v1.Data == (long)v2.Data;
                             case "Double": return (int)v1.Data == (double)v2.Data;
                            case "DataEnum": return (int)v1.Data == (int)((DataEnum)v2.Data);
                           case "Single": return (int)v1.Data == (float)v2.Data;
                            default: break;
                        }
                        break;
                    }
                case "Int64":
                    {
                        switch (t2)
                        {
                           case "Int64": return (long)v1.Data == (long)v2.Data;
                            case "Int16": return (long)v1.Data == (short)v2.Data;
                            case "Int32": return (long)v1.Data == (int)v2.Data;
                             case "Single": return (long)v1.Data == (float)v2.Data;
                            case "Double": return (long)v1.Data == (double)v2.Data;
                            case "DataEnum": return (long)v1.Data == (int)((DataEnum)v2.Data);
                            default: break;
                        }
                        break;
                    }
                case "Single":
                    {
                        switch (t2)
                        {
                            case "Single": return (float)v1.Data == (float)v2.Data;
                            case "Double": return (float)v1.Data == (double)v2.Data;
                            case "Int16": return (float)v1.Data == (short)v2.Data;
                            case "Int32": return (float)v1.Data == (int)v2.Data;
                            case "Int64": return (float)v1.Data == (long)v2.Data;
                            default: break;
                        }
                        break;
                    }
            }
			return false;
		}			

		//-----------------------------------------------------------------------------
		public static bool operator >(Value v1, Value v2)
		{
            if ((object)v1 == null || (object)v2 == null) return false;
			
			//LARA 21-01-05
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
					case "Int16":	return (double)v1.Data > (short)v2.Data;
					case "Int32":	return (double)v1.Data > (int)v2.Data;
					case "Int64":	return (double)v1.Data > (long)v2.Data;
					case "Single":	return (double)v1.Data > (float)v2.Data;
					default:		break;
				}
					break;

				case "DateTime":
				switch (t2)
				{
					case "DateTime":	return (DateTime)v1.Data > (DateTime)v2.Data;
					default:			break;
				}
					break;
				case "String": 
				switch (t2)
				{
					case "String":	
					{
						string s1 = ((string)v1.Data);
						string s2 = ((string)v2.Data);
						return string.Compare(s1, s2, true, GetCollateCulture(v1, v2)) > 0;
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
                        case "Int16": return (float)v1.Data > (short)v2.Data;
                        case "Int32": return (float)v1.Data > (int)v2.Data;
                        case "Int64": return (float)v1.Data > (long)v2.Data;
                        case "Single": return (float)v1.Data > (float)v2.Data;
                        case "Double": return (float)v1.Data > (double)v2.Data;
                        default: break;
                    }
                    break;
			}
			return false;
		}			
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

}


using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.Common.Lexan;
using Microarea.Common.ExpressionManager;
using Microarea.Common;

namespace Microarea.RSWeb.WoormEngine
{
	/// <summary>
	/// DataFunction, deriva da Expression per poterne valutare il valore
	/// </summary>
	//============================================================================
	abstract public class DataFunction : Expression
	{
		public string   PublicName = string.Empty;
		private Token   token;
		private	Field	functionField;     //il field in symbol table
        public long     Occurrence = 0;    //per gestire AVG/Count/First
                                        
        //---------------------------------------------------------------------------
        public abstract void	GetFromData	(ref object aData, ref bool aValid);
		public abstract void	GetToData	(ref object aData, ref bool aValid);

		public abstract void	SetToData	(object aData, bool aValid);
		public abstract bool	IsUpdated();
		
		public Token Token { get { return token; } }

		//---------------------------------------------------------------------------
		public Field	FunctionField	{ get { return functionField; }}
		public bool		IsAnExpression	{ get { return token == Token.NOTOKEN; }}

		//---------------------------------------------------------------------------
		public DataFunction(TbReportSession session, FieldSymbolTable symTable): base(session, symTable)
		{
			functionField	= null;
		}

		//---------------------------------------------------------------------------
		public bool EvalFunction()
		{
			if (IsAnExpression)
			{
				Value v = Eval();
				if (!Error)
				{
					SetToData(v.Data, v.Valid);
					return true;
				}

				// se è un errore dovuto a NULL operand allora non lo considero errore e proseguo
				return !v.Valid;
			}

			// evita di utilizzare dei dati che sono presenti nella tupla ma non sono stati
			// aggiornati da una regola di estrazione (ricordati che posso avere rule in sequenza)
			// la prima estrae una riga, la seconda 4 righe e la funzione da valutare sulla tupla
			// deve sommare 4 volte sulla seconda estrazione e solo una volta dalla prima.
			if (!IsUpdated())
				return true;

			object from = null;
			object to = null;
			bool fromValid = true;
			bool toValid = true;
			
			GetFromData(ref from, ref fromValid);
			GetToData(ref to, ref toValid);

			// se qualche operando è NULL non eseguo la chiamata ma proseguo senza fare il calcolo
			if (!fromValid || !toValid)
			{
				//Dare eventualmente una Warning: ExpressionManagerStrings.InvalidDataFunctionParameter
				return true;
			}

			switch (token)
			{
				case Token.CCAT :
					if	((ObjectHelper.DataType(to) == "String") && (ObjectHelper.DataType(from) == "String"))
					{
						string s = ObjectHelper.CastString(to) + " " + ObjectHelper.CastString(from);
						SetToData((object) s, true);
						return true;
					}

					return false;

				case Token.CSUM :
				{
					if	(ObjectHelper.DataType(to) == "String")
					{
						string s = ObjectHelper.CastString(to) + ObjectHelper.CastString(from);
						SetToData((object) s, true);
						return true;
					}
					
					if	(ObjectHelper.DataType(to) == "Int32")
					{
						int i = ObjectHelper.CastInt(to) + ObjectHelper.CastInt(from);
						SetToData((object) i, true);
						return true;
					}
										
					if	(ObjectHelper.DataType(to) == "Int64")
					{
						long result = ObjectHelper.CastLong(to) + ObjectHelper.CastLong(from);
						SetToData((object) result, true);
						return true;
					}
										
					if	(ObjectHelper.DataType(to) == "Single")
					{
						double result = ObjectHelper.CastFloat(to) + ObjectHelper.CastFloat(from);
						SetToData((object) result, true);
						return true;
					}
										
					if	(ObjectHelper.DataType(to) == "Double")
					{
						double result = ObjectHelper.CastDouble(to) + ObjectHelper.CastDouble(from);
						SetToData((object) result, true);
						return true;
					}
					return false;
				}

				case Token.CMIN :
					if (ObjectHelper.IsGreater(to, from)) SetToData(from, true);
					return true;

				case Token.CMAX :
					if (ObjectHelper.IsLess(to, from)) SetToData(from, true);
					return true;
			}
			return false;
		}


		//---------------------------------------------------------------------------
		protected bool LookForAFunc(Parser lex)
		{
			switch (lex.LookAhead())
			{
				case Token.CSUM		: 
				case Token.CCAT 	:
				case Token.CMIN		:
				case Token.CMAX		: return true;
			}

			return false;
		}			

		//---------------------------------------------------------------------------
		public bool ParseFunction(Parser lex, object groupByData)
		{
			if (!LookForAFunc(lex))
			{
				lex.SetError(WoormEngineStrings.UnknownFuction);
				return false;
			}

			string itemName;
			token = lex.LookAhead();
			lex.SkipToken();

			if (!lex.ParseItem(out itemName))
				return false;

			functionField = SymbolTable.Find(itemName) as Field;
			if (functionField == null)
			{
				lex.SetError(string.Format(WoormEngineStrings.UnknownFuction, itemName));
				return false;
			}

			if (functionField.ReadOnly)
			{
				lex.SetError(string.Format(WoormEngineStrings.ReadOnlyField, itemName));
				return false;
			}
		    
			bool ok = true;
			switch (token)
			{
				case Token.CCAT :
				{
					ok =	functionField.Data.GetType().Name == "String" &&
							(groupByData == null || groupByData.GetType().Name == "String");
					break;
				}       
				case Token.CSUM :
				{
					if (groupByData != null)
					{
						ok = false;
						string first = functionField.Data.GetType().Name;
						string second = groupByData.GetType().Name;

						if (first == "DateEnum" || first == "DateTime" || first == "Boolean")	break;
						if (first == "String" && second != "String")							break;

						ok = true;
					}
					break;
				}
				case Token.CMIN :
				case Token.CMAX :
				{
					ok = functionField.Data.GetType().Name != "Boolean";
					break;
				}
				default  : 
				{
					ok = false; 
					break;
				}
			}
		    
			if (!ok)
				lex.SetError(string.Format(WoormEngineStrings.IncompatibleFunctionOperand, itemName));
		
			return ok;
		}

		//---------------------------------------------------------------------------
		public bool ParseExpression(Parser lex, Field field)
		{
			if (lex.Error) return false;
			if (field == null)
			{
				lex.SetError(ExpressionManagerStrings.UnknownField);
				return false;
			}

			if (field.ReadOnly)
			{
				lex.SetError(string.Format(WoormEngineStrings.ReadOnlyField, field.PublicName));
				return false;
			}

			token = Token.NOTOKEN;
			functionField = field;

			return Compile(lex, CheckResultType.Compatible, field.DataType);
		}

		//---------------------------------------------------------------------------
		public bool Unparse(Unparser unparser)
		{
			if (IsAnExpression)
				unparser.WriteExpr(this.ToString());
			else
				UnparseFunction(unparser);

			return true;
		}

		//----------------------------------------------------------------------------
		void UnparseFunction(Unparser unparser)
		{
			unparser.WriteTag(Token, false);
			unparser.WriteOpen(false);
			unparser.WriteID(functionField.PublicName, false);
			unparser.WriteClose(false);
		}
	}
	/// <summary>
	/// EventFunction
	/// </summary>
	/// esempio di uso in zona di dichiarazione di variabili:
	///		Integer      TotNrGGBanca = Csum (NrGGBanca ) Alias 8  ColTotal ;
	///			TotNrGGBanca è accumulator
	///			NrGGBanca è functionField
	//============================================================================
	public class EventFunction : DataFunction
	{
		private Field accumulator;  // deve essere un reference perchè si usa il field in symbol table
		
		//---------------------------------------------------------------------------
		public EventFunction (TbReportSession session, FieldSymbolTable symTable, Field accumulator) : base(session, symTable)
		{
			this.accumulator = accumulator;
		}

		//---------------------------------------------------------------------------
		public override bool IsUpdated() { return FunctionField.EventDataUpdated; }

		//---------------------------------------------------------------------------
		public override void GetFromData(ref object aData, ref bool aValid)
		{
			aData = FunctionField.EventData;
			aValid = FunctionField.ValidEventData;
		}
			
		//---------------------------------------------------------------------------
		public override void GetToData(ref object aData, ref bool aValid)
		{
			aData = accumulator.EventData;
			aValid = accumulator.ValidEventData;
		}

		//---------------------------------------------------------------------------
		public override void SetToData(object aData, bool aValid)
		{ 
			accumulator.AssignEventData(aData, aValid);
			accumulator.EventDataUpdated = true;
		}

		//---------------------------------------------------------------------------
		public bool Parse(Parser lex)
		{
			return 
				LookForAFunc(lex)
					? ParseFunction(lex, accumulator.Data)
					: ParseExpression(lex, accumulator);
		}

		//---------------------------------------------------------------------------
		internal string GetPublicName()
		{
			return accumulator.PublicName;
		}

		//---------------------------------------------------------------------------
		public override bool IsEmpty
		{
			get
			{
				return (Token  == Token.NOTOKEN && base.IsEmpty);
			}
		}
	}

	/// <summary>
	/// GroupFunction
	/// </summary>
	/// esempio di uso in zona di dichiarazione di variabili:
	///		GroupBy Articolo begin Csum (NrGGBanca); end;
	///			NrGGBanca è functionField
	///	la funzione di groupby (Csum in questo caso) viene eseguita a livello di 
	///	GroupByData pescando il valore da RuleData
	//============================================================================
	public class GroupFunction : DataFunction
	{
		//---------------------------------------------------------------------------
		public GroupFunction(TbReportSession session, FieldSymbolTable  symTable) : base(session, symTable)
		{}

		//---------------------------------------------------------------------------
		public override bool IsUpdated() { return FunctionField.RuleDataFetched; }

		//---------------------------------------------------------------------------
		public override void GetFromData(ref object aData, ref bool aValid)
		{
			aData = FunctionField.RuleData;
			aValid = FunctionField.ValidRuleData;
		}
			
		//---------------------------------------------------------------------------
		public override void GetToData(ref object aData, ref bool aValid)
		{
			aData = FunctionField.GroupByData;
			aValid = FunctionField.ValidGroupByData;
		}

		//---------------------------------------------------------------------------
		public override void SetToData(object aData, bool aValid)
		{ 
			FunctionField.AssignGroupByData(aData, aValid);
			FunctionField.GroupByDataUpdated = true;
		}

		//---------------------------------------------------------------------------
		public bool Parse(Parser lex)
		{
			if (lex.LookAhead(Token.ID))
			{
				if (!lex.ParseID(out PublicName) || !lex.ParseTag(Token.ASSIGN)) 
					return false;

				Field pAccItem = SymbolTable.Find(PublicName) as Field;
				if (pAccItem == null)
				{
					lex.SetError(string.Format(ExpressionManagerStrings.FieldByRule, PublicName));
					return false;
				}

				// pAccItem will stored in thePFuncItem DataFunction member
				if (!ParseExpression(lex, pAccItem)) 
					return false;
			}
			else
				if (!ParseFunction(lex, null)) 
					return false;
				        	
			if (!FunctionField.OwnRule)
			{
				lex.SetError(string.Format(WoormEngineStrings.IllegalGroupExpression, FunctionField.PublicName));
				return false;
			}

			if (FunctionField.GroupFunction != null)
			{
				lex.SetError(string.Format(WoormEngineStrings.FunctionExist, FunctionField.PublicName));
				return false;
			}

			FunctionField.GroupFunction = this;

			return true;
		}
	}
}

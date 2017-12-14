using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.Model;

using Microarea.TaskBuilderNet.Data.DatabaseLayer;

using Microarea.TaskBuilderNet.Core.StringLoader;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Lexan;

namespace Microarea.TaskBuilderNet.Woorm.ExpressionManager
{
	public enum CheckResultType { Compatible, Match, Ignore }
	
	/// <summary>
	/// Summary description for Expression.
	/// </summary>
	///=============================================================================
	public class Expression
	{
		private const string		barCodeEncoderChars = "@@";

		protected Parser			parser;
        protected string            auditExpr = string.Empty;
		protected Stack				expressionStack = new Stack();

		protected Diagnostic		diagnostic = new Diagnostic("Expression");
		protected StopTokens		stopTokens = null;
		protected string			resultType = null;

		protected SymbolTable		symbolTable;
		protected TbReportSession   reportSession;

		public    bool				ForceSkipTypeChecking = false;

        //public bool               HasExternalFunctionCall = false;
        public    bool              HasRuleFields = false;

        //-----------------------------------------------------------------------------
        public Expression Clone()
        {
            /*
           Expression e = new Expression(reportSession, symbolTable);

           e.parser = this.parser;
           diagnostic = diagnostic;
           stopTokens = stopTokens;
           ForceSkipTypeChecking = ForceSkipTypeChecking
             * 
             * DUBBIO su expressionStack    //DEEP clone ?
            */
            return this.MemberwiseClone() as Expression;
        }


		//-----------------------------------------------------------------------------
		public TbReportSession	ReportSession	{ get { return reportSession; }}
		public StopTokens	StopTokens			{ get { return stopTokens; } set { stopTokens = value; }}
		public string		ResultType			{ get { return resultType; }}
		public Diagnostic	Diagnostic			{ get { return diagnostic; }}
		public bool			Error				{ get { return diagnostic.Error; }}
		virtual public bool	IsEmpty				{ get { return expressionStack.Count == 0; }}
		public SymbolTable	SymbolTable			{ get { return symbolTable; }}
        public ILocalizer   Localizer           { get { return ReportSession.Localizer; } /*set { localizer = value; }*/ }
		public bool			SkipTypeChecking	{ get { return ForceSkipTypeChecking || ReportSession.SkipTypeChecking; } }

        override public string ToString()       { return auditExpr; }

		//=============================================================================
		// When user type in a new expression, or modify an existing one, first
		// only the string form is assigned. The internal form is assigned by parsing
		// the string only when the expression is tested for validity or evaluated.
		// Once parsed, the internal form is kept and used for repeated evaluations,
		// until the user modify the string form and re-Assign it again.
		//-----------------------------------------------------------------------------
		public Expression(TbReportSession session, SymbolTable symbolTable)
		{
			this.symbolTable	= symbolTable;
			this.reportSession	= session; 
		}

		//-----------------------------------------------------------------------------
		public Expression(Expression expression) 
		{
			this.symbolTable = null;
			this.stopTokens = null;
			this.HasRuleFields= false;
		
			//TODOLUCA
			//m_nErrorPos		(-1),
			//m_nErrorID		(EMPTY_MESSAGE),
			//m_bVrbCompiled	(FALSE),
			//m_bHasExternalFunctionCall (FALSE),
			//m_nParseStartLine	(0) 

			Assign(expression);
		}

		//-----------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		//-----------------------------------------------------------------------------
		public override bool Equals(object aExpr)
		{
			Expression expr = aExpr as Expression;
			return Stack.Equals(expressionStack, expr.expressionStack);
		}

		//-----------------------------------------------------------------------------
		public static bool	operator ==	(Expression e1, Expression e2)	{ return Expression.Equals(e1, e2); }
		public static bool	operator !=	(Expression e1, Expression e2)	{ return !(e1 == e2); }
		
		//-----------------------------------------------------------------------------
		public static string EncodeBarCode(string barCodeValue, int barCodeType, int chkSumType, string humanReadable)
		{
            string s = barCodeValue + barCodeEncoderChars + barCodeType.ToString();
            if (humanReadable != string.Empty)
                s += barCodeEncoderChars + chkSumType.ToString() + barCodeEncoderChars + humanReadable;
            else if (chkSumType != 0)
                s += barCodeEncoderChars + chkSumType.ToString(); 
            return s;
		}
		
		//-----------------------------------------------------------------------------
        public static bool DecodeBarCode(string encodedBarCode, out string barCodeValue, out int barCodeType, out int chkSumType, out string humanReadable)
		{
            chkSumType = 0;
            humanReadable = string.Empty;

            string[] t = encodedBarCode.Split(barCodeEncoderChars.ToCharArray());
			if (t.GetLength(0) < 2)
			{
				barCodeValue = encodedBarCode;
				barCodeType = -1;
				return false;
			}

			barCodeValue = t[0];

            try
            { 
			    barCodeType = int.Parse(t[1]);
                if (t.GetLength(0) > 2)
                {
                    chkSumType = int.Parse(t[2]);
                }
                if (t.GetLength(0) > 3)
                {
                    humanReadable = t[3];
                }
            }
            catch(Exception)
            {
                barCodeType = -1;
                return false;
            }
			
            return true;
		}
        //-----------------------------------------------------------------------------
        
        static string [] barCodeName = {
            "Default", 
            "UPCA",
            "UPCE",
            "EAN13",
            "EAN8",
            "CODE39",
            "EXT39",
            "INT25",
            "CODE128",
            "CODABAR",
            "ZIP",
            "MSIPLESSEY",
            "CODE93",
            "EXT93",
            "UCC128",
            "HIBC",
            "PDF417",
            "UPCE0",
            "UPCE1",
            "CODE128A",
            "CODE128B",
            "CODE128C",
            "EAN128",
            "DataMatrix",
            "MicroQR",
            "QR"
        };

        static long[] barCodeEnum = {
            5636117, 
            5636099,
            5636100,
            5636097,
            5636096,
            5636098,
            5636101,
            5636102,
            5636103,
            5636104,
            5636105,
            5636106,
            5636107,
            5636108,
            5636109,
            5636110,
            5636111,
            5636112,
            5636113,
            5636114,
            5636115,
            5636116,
            5636118,
            5636119,
            5636120,
            5636121
        };
       
        public static int GetBarCodeId(string nameBarCode)
        {
            for (int i = 0; i < barCodeName.GetLength(0); i++)
                if (string.Compare(nameBarCode, barCodeName[i], true) == 0)
                    return 1;
            return 0;
        }

        public static int GetBarCodeId(long eBc)
        {
            for (int i = 0; i < barCodeEnum.GetLength(0); i++)
                if (eBc == barCodeEnum[i])
                    return 1;
            return 0;
        }

		//-----------------------------------------------------------------------------
		static bool LikeFunction(string target, string pattern) { return LikeFunction(target, pattern, "\\"); }
		static bool LikeFunction(string target, string pattern, string escape)
		{
			pattern = Regex.Escape(pattern);
			// passa da formato SQL in regular expression
			pattern = String.Concat("^", pattern);
			pattern = pattern.Replace('_', '.');
			pattern = pattern.Replace("%", ".*");

			// se esistono dei caratteri escaped allora li riporta all'originale modificato dalle
			// righe di sopra che sostituiscono in forma totale non tenendo conto del carattere di escape
			pattern = pattern.Replace(escape + '.', escape + '_');
			pattern = pattern.Replace(escape + ".*", escape + '%');

			Regex regex = new Regex(pattern);
			return regex.IsMatch(target);
		}

		// binary PLUS(+) operator
		//-----------------------------------------------------------------------------
		string PlusReturnType(Value v1, Value v2)
		{
			string t1 = v1.DataType;
			string t2 = v2.DataType;

			switch (t1)
			{
				case "String":
				switch (t2)
				{
					case "String":	return "String";
					default:		return null;
				}

				case "Int16":
				switch (t2)
				{
					case "Int16":		return "Int16";
					case "Int32":		return "Int32";
					case "Int64":		return "Int64";
					case "Decimal":		return "Decimal";
					case "Single":		return "Single";
					case "Double":		return "Double";
					case "DateTime":	return "DateTime";
					default:			return null;
				}

				case "Int32":
				switch (t2)
				{
					case "Int16":		return "Int32";
					case "Int32":		return "Int32";
					case "Int64":		return "Int64";
					case "Decimal":		return "Decimal";
					case "Single":		return "Single";
					case "Double":		return "Double";
					case "DateTime":	return "DateTime";
					default:			return null;
				}

				case "Int64":
				switch (t2)
				{
					case "Int16":		return "Int64";
					case "Int32":		return "Int64";
					case "Int64":		return "Int64";
					case "Decimal":		return "Decimal";
					case "Single":		return "Single";
					case "Double":		return "Double";
					case "DateTime":	return "DateTime";
					default:			return null;
				}

				case "Decimal":
				switch (t2)
				{
					case "Int16":		return "Decimal";
					case "Int32":		return "Decimal";
					case "Int64":		return "Decimal";
					case "Decimal":		return "Decimal";
					case "Single":		return "Single";   
					case "Double":		return "Double";   
					default:			return null;
				}

				case "Single":
				switch (t2)
				{
					case "Int16":	return "Single";
					case "Int32":	return "Single";
					case "Int64":	return "Single";
					case "Decimal":	return "Single";
					case "Single":	return "Single";
					case "Double":	return "Double";
					default:		return null;
				}

				case "Double":
				switch (t2)
				{
					case "Int16":	return "Double";
					case "Int32":	return "Double";
					case "Int64":	return "Double";
					case "Decimal":	return "Double";
					case "Single":	return "Double";
					case "Double":	return "Double";
					default:		return null;
				}

				case "DateTime":
				switch (t2)
				{
					case "Int16":	return "DateTime";
					case "Int32":	return "DateTime";
					case "Int64":	return "DateTime";
					case "Decimal":	return "DateTime";
					default:		return null;
				}
				default : return null;
			}
		}			

		// binary MINUS(-) operator
		//-----------------------------------------------------------------------------
		string MinusReturnType(Value v1, Value v2)
		{
			string t1 = v1.DataType;
			string t2 = v2.DataType;

			switch (t1)
			{
				case "String": 
                    return null;

				case "Int16":
				    switch (t2)
				    {
					    case "Int16":	return "Int16";
					    case "Int32":	return "Int32";
					    case "Int64":	return "Int64";
					    case "Decimal":	return "Decimal";
					    case "Single":	return "Single";
					    case "Double":	return "Double";
					    default:		return null;
				    }

				case "Int32":
				    switch (t2)
				    {
					    case "Int16":	return "Int32";
					    case "Int32":	return "Int32";
					    case "Int64":	return "Int64";
					    case "Decimal":	return "Decimal";
					    case "Single":	return "Single";
					    case "Double":	return "Double";
					    default:		return null;
				    }

				case "Int64":
				    switch (t2)
				    {
					    case "Int16":	return "Int64";
					    case "Int32":	return "Int64";
					    case "Int64":	return "Int64";
					    case "Decimal":	return "Decimal";
					    case "Single":	return "Single";
					    case "Double":	return "Double";
					    default:		return null;
				    }
				case "Decimal":
				    switch (t2)
				    {
					    case "Int16":	return "Decimal";
					    case "Int32":	return "Decimal";
					    case "Int64":	return "Decimal";
					    case "Decimal":	return "Decimal";
					    case "Single":	return "Single";
					    case "Double":	return "Double";
					    default:		return null;
				    }

				case "Single":
				    switch (t2)
				    {
					    case "Int16":	return "Single";
					    case "Int32":	return "Single";
					    case "Int64":	return "Single";
					    case "Decimal":	return "Single";
					    case "Single":	return "Single";
					    case "Double":	return "Double";
					    default:		return null;
				    }

				case "Double":
				    switch (t2)
				    {
					    case "Int16":	return "Double";
					    case "Int32":	return "Double";
					    case "Int64":	return "Double";
					    case "Decimal":	return "Double";
					    case "Single":	return "Double";
					    case "Double":	return "Double";
					    default:		return null;
				    }

				case "DateTime":
				    switch (t2)
				    {
					    case "Int16":		return "DateTime";
					    case "Int32":		return "DateTime";
					    case "Int64":		return "DateTime";
					    case "DateTime":	return "Int64";
					    default:			return null;
				    }
				default :
                    Debug.Assert(false, "Expression.MinusReturnType");
                    return null;
			}
		}			

		// binary STAR(*) operator
		//-----------------------------------------------------------------------------
		string StarReturnType(Value v1, Value v2)
		{
			string t1 = v1.DataType;
			string t2 = v2.DataType;

			switch (t1)
			{
				case "Int16":
				switch (t2)
				{
					case "Int16":	return "Int16";
					case "Int32":	return "Int32";
					case "Int64":	return "Int64";
					case "Decimal":	return "Decimal";
					case "Single":	return "Single";
					case "Double":	return "Double";
					default:		return null;
				}

				case "Int32":
				switch (t2)
				{
					case "Int16":	return "Int32";
					case "Int32":	return "Int32";
					case "Int64":	return "Int64";
					case "Decimal":	return "Decimal";
					case "Single":	return "Single";
					case "Double":	return "Double";
					default:		return null;
				}

				case "Int64":
				switch (t2)
				{
					case "Int16":	return "Int64";
					case "Int32":	return "Int64";
					case "Int64":	return "Int64";
					case "Decimal":	return "Decimal";
					case "Single":	return "Single";
					case "Double":	return "Double";
					default:		return null;
				}

				case "Decimal":
				switch (t2)
				{
					case "Int16":	return "Decimal";
					case "Int32":	return "Decimal";
					case "Int64":	return "Decimal";
					case "Decimal":	return "Decimal";
					case "Single":	return "Single";
					case "Double":	return "Double";
					default:		return null;
				}
				
				case "Single":
				switch (t2)
				{
					case "Int16":	return "Single";
					case "Int32":	return "Single";
					case "Int64":	return "Single";
					case "Decimal":	return "Single";
					case "Single":	return "Single";
					case "Double":	return "Double";
					default:		return null;
				}

				case "Double":
				switch (t2)
				{
					case "Int16":	return "Double";
					case "Int32":	return "Double";
					case "Int64":	return "Double";
					case "Decimal":	return "Double";
					case "Single":	return "Double";
					case "Double":	return "Double";
					default:		return null;
				}

				default : return null;
			}
		}			

		// binary SLASH(/) operator
		//-----------------------------------------------------------------------------
		string SlashReturnType(Value v1, Value v2)
		{
			string t1 = v1.DataType;
			string t2 = v2.DataType;

			switch (t1)
			{
				case "Int16":
				switch (t2)
				{
					case "Int16":
					case "Int32":
					case "Int64":
					case "Decimal":
					case "Single":
					case "Double":	return "Double";
					default:		return null;
				}

				case "Int32":
				switch (t2)
				{
					case "Int16":
					case "Int32":
					case "Int64":
					case "Decimal":
					case "Single":
					case "Double":	return "Double";
					default:		return null;
				}

				case "Int64":
				switch (t2)
				{
					case "Int32":
					case "Int64":
					case "Decimal":
					case "Single":
					case "Double":	return "Double";
					default:		return null;
				}

				case "Decimal":
				switch (t2)
				{
					case "Int32":
					case "Int64":
					case "Decimal":
					case "Single":
					case "Double":	return "Double";
					default:		return null;
				}
						
				case "Single":
				switch (t2)
				{
					case "Int32":
					case "Int64":
					case "Decimal":
					case "Single":
					case "Double":	return "Double";
					default:		return null;
				}

				case "Double":
				switch (t2)
				{
					case "Int32":
					case "Int64":
					case "Decimal":
					case "Single":
					case "Double":	return "Double";
					default:		return null;
				}
				default : return null;
			}
		}			

		// binary PERC(%) operator (remainder of SLASH operator)
		//-----------------------------------------------------------------------------
		string PercReturnType(Value v1, Value v2)
		{
			string t1 = v1.DataType;
			string t2 = v2.DataType;

			switch (t1)
			{
				case "Int16":
				switch (t2)
				{
					case "Int16":	return "Int32";
					case "Int32":
					case "Int64":	return "Int32"; 
					default:		return null;
				}

				case "Int32":
				switch (t2)
				{
					case "Int16":
					case "Int32":
					case "Int64":	return "Int32";
					default:		return null;
				}

				case "Int64":
				switch (t2)
				{
					case "Int64":	return "Int32"; 
					default:		return null;
				}
				default : return null;
			}
		}			

		// binary RELATION( == <= >= != BETWEEN) operators
		// String è confronatbile con Boolean a causa della rappresentazione in TBC++
		// del DataBool come Varchar(1) con valori "1" e "0"
		// Inoltre anche DataEnum è confrontabile con Int32 che ne è la sua rappresentazione
		// interna.
		//-----------------------------------------------------------------------------
		string RelReturnType(Value v1, Value v2)
		{
			string t1 = v1.DataType;
			string t2 = v2.DataType;

			switch (t1)
			{
				case "Boolean":
				switch (t2)
				{
					case "Boolean":	return "Boolean";
					case "String":	return "Boolean";
					default:		return null;
				}

				case "Int16":
				switch (t2)
				{
					case "Int16":
					case "Int32":
					case "Int64":
					case "Decimal":
					case "Single":
					case "Double":	return "Boolean";
					default:		return null;
				}

				case "Int32":
				switch (t2)
				{
					case "Int16":
					case "Int32":
					case "Int64":
					case "Decimal":
					case "Single":
					case "DataEnum":
					case "Double":	return "Boolean";
					default:		return null;
				}

				case "Int64":
				switch (t2)
				{
					case "Int16":
					case "Int32":
					case "Int64":
					case "Decimal":
					case "Single":
					case "Double":	return "Boolean";
					default:		return null;
				}

				case "Decimal":
				switch (t2)
				{
					case "Int16":
					case "Int32":
					case "Int64":
					case "Decimal":
					case "DataEnum":
					case "Single":
					case "Double":	return "Boolean";
					default:		return null;
				}

				case "Single":
				switch (t2)
				{
					case "Int16":
					case "Int32":
					case "Int64":
					case "Decimal":
					case "Single":
					case "Double":	return "Boolean";
					default:		return null;
				}

				case "Double":
				switch (t2)
				{
					case "Int16":
					case "Int32":
					case "Int64":
					case "Decimal":
					case "Single":
					case "Double":	return "Boolean";
					default:		return null;
				}

				case "String":
				switch (t2)
				{
					case "Boolean":	return "Boolean";
					case "String":	return "Boolean";
					case "Guid":	return "Boolean";
					default:		return null;
				}

				case "Guid":
				switch (t2)
				{
					case "Guid":
					case "String":	return "Boolean";
					default:		return null;
				}

				case "DateTime":
				switch (t2)
				{
					case "DateTime":	return "Boolean";
					default:			return null;
				}

				case "DataEnum":
                case "Enum":
                switch (t2)
				{
					case "Int32":		return "Boolean";
					case "Decimal":		return "Boolean";
					case "DataEnum":	return "Boolean";
					default:			return null;
				}

                case "DataArray":
                case "Array":
                switch (t2)
                {
                    case "DateArray": return "Boolean";
                    default: return null;
                }

				default : return null;
			}
		}			

		//-----------------------------------------------------------------------------
		string SetReturnType(Token operatorID, Value v1, Value v2)
		{
            switch (operatorID)
            {
                case Token.IN:
                    if (v2.DataType == "DataArray" || v2.DataType == "Array")
                        return "Boolean";
                    break;

                case Token.CONTAINS:
                    if (v1.DataType == "DataArray" || v2.DataType == "Array") 
                        return "Boolean";
                    break;
            }

            //if	(v1.DataType == "Int64" && v2.DataType == "Int64")
            //{
            //    switch (operatorID)
            //    {
            //        case Token.CONTAINS	:	
            //            return "Boolean";
            //    }
            //}
            return null;
		}

		//-----------------------------------------------------------------------------
		string BitwiseReturnType(Value v1) { return BitwiseReturnType(v1, null); }
		string BitwiseReturnType(Value v1, Value v2)
		{
			if	(
				(v2 == null || v1.DataType == v2.DataType) &&
				(
				v1.DataType == "Int16" ||
				v1.DataType == "Int32" ||
				v1.DataType == "Int64" ||
				v1.DataType == "Decimal"
				)
				)
				return v1.DataType;

			return null;
		}

		//-----------------------------------------------------------------------------
		string LogicalReturnType(Value v1) { return LogicalReturnType(v1, null); }
		string LogicalReturnType(Value v1, Value v2)
		{
			if	(
				v1.DataType == "Boolean" &&
				(v2 == null || v2.DataType == "Boolean")
				)
				return "Boolean";

			return null;
		}

		//-----------------------------------------------------------------------------
		string LikeReturnType(Value v1, Value v2)
		{
			if	(
				v1.DataType == "String" &&
				v2.DataType == "String"
				)
				return "Boolean";

			return null;
		}

		// Return the dataType result, if it is equal to null
		// then operation is not possibile.
		//-----------------------------------------------------------------------------
		string GiveMeResultType
			        (
			            OperatorItem ope,
			            Value v1,
			            Value v2,
			            Value v3
			        )
		{
			switch (ope.GetOperatorType())
			{
				case OperatorItem.OperatorType.Unary :
				{
					if (v1 == null) return null;

					switch (ope.OperatorID)
					{
						case Token.EXPR_UNNARY_MINUS:
						switch (v1.DataType)
						{
							case "Int16":
							case "Int32":
							case "Int64":
							case "Decimal":
							case "Single":
							case "Double":
								return v1.DataType;
						}
							return null;
				
						case Token.NOT		:
						case Token.OP_NOT	:
							return LogicalReturnType(v1);
							
						case Token.BW_NOT	:
							return BitwiseReturnType(v1);

						case Token.EXPR_IS_NULL	:
						case Token.EXPR_IS_NOT_NULL	:
							return "Boolean";
					}			

					return null;
				}
				case OperatorItem.OperatorType.Binary  :
				{
					if (v1 == null || v2 == null) return null;

					switch (ope.OperatorID)
					{
						case Token.PLUS		:	return PlusReturnType(v1, v2);
						case Token.MINUS	:	return MinusReturnType(v1, v2);
						case Token.STAR		:	return StarReturnType(v1, v2);
						case Token.SLASH	:	return SlashReturnType(v1, v2);
						case Token.PERC		:	return PercReturnType(v1, v2);
						case Token.LIKE		:	return LikeReturnType(v1, v2);

						case Token.GT		:
						case Token.GE		:
						case Token.LT		:
						case Token.LE		:
						case Token.NE		:
						case Token.DIFF		:
						case Token.EQ		:
						case Token.ASSIGN	:
						{
							if	(RelReturnType(v1, v2) == null)
								break;

							return "Boolean";
						}
						case Token.BW_AND	:
						case Token.BW_OR	:
						case Token.BW_XOR	:
							return BitwiseReturnType(v1);
							
						case Token.CONTAINS	:
                        case Token.IN:
                            return SetReturnType(ope.OperatorID, v1, v2);
					}

					return null;
				}

				case OperatorItem.OperatorType.Logical :
				{
					if (v1 == null || v2 == null) return null;
					switch (ope.OperatorID)
					{
						case Token.AND		:
						case Token.OP_AND	:
						case Token.OR		:
						case Token.OP_OR	:
							return LogicalReturnType(v1, v2);

						case Token.BETWEEN	:
						{
							if (v3 == null) return null;
							if	(
								RelReturnType(v1, v2) != null &&
								RelReturnType(v1, v3) != null
								)
								return "Boolean";
							break;
						}
						case Token.QUESTION_MARK:
						{
                           if (v3 == null) 
                                return null;
                           if (
                                v1.DataType == "Boolean" &&
                                ObjectHelper.Compatible(v2.DataType, v3.DataType)
                                )
                            {
                               string t = v2.DataType;
                               return t.Length > 0 ? t : v3.DataType;
                            }
							break;
						}
					}

					return null;
				}	
			
				case OperatorItem.OperatorType.Ternary :
				{
					if (v1 == null || v2 == null || v3 == null) return null;

					switch (ope.OperatorID)
					{
						case Token.EXPR_ESCAPED_LIKE:
							if	(
								LikeReturnType(v1, v2) != null &&
								v3.DataType == "String"
								)
								return "Boolean";
							break;
					}

					return null;
				}
			}

			return null;
		}

		//-----------------------------------------------------------------------------
		public void Assign(Expression aExp)
		{
			expressionStack	= aExp.expressionStack.Clone() as Stack;
			symbolTable		= aExp.symbolTable; //.Clone();
			
			if (aExp.stopTokens == null) stopTokens = null;
			else stopTokens = aExp.stopTokens.Clone() as StopTokens;
		}

		// viene usata per determinare se l'espressione contiene un determinata variabile
		//-----------------------------------------------------------------------------
		virtual public bool HasMember(string name) { return HasMember(name, expressionStack); }
		virtual public bool HasMember(string name, Stack stack)
		{
			foreach (Item item in stack)
			{
				if (item is Variable)
				{
					Variable variable = item as Variable;
					if (string.Compare(variable.Name, name, true, CultureInfo.InvariantCulture) == 0)
						return true;
				}
				else if (item is OperatorItem)
				{
					OperatorItem o = item as OperatorItem;
					if (HasMember(name, o.FirstStack) || HasMember(name, o.SecondStack))
						return true;
				}
                else if (item is FunctionContentOfItem)
                {
                    FunctionContentOfItem fco = item as FunctionContentOfItem;
                    if (HasMember(name, fco.CofExpression))
                        return true;
                }
			}
			return false;
		}

		//-----------------------------------------------------------------------------
		public void SetError(Diagnostic diagnostic)
		{ 
			this.diagnostic.Set(DiagnosticType.Error, diagnostic); 
		}

		//-----------------------------------------------------------------------------
		public void SetError(string explain)
		{
            Debug.Assert(false, "Expression.SetError" + explain);

			// aggiungo l'errore al parser e faccio ereditare tutto al diagnostico
			// di Expression perchè potrei avere già errori nel parser
			if (parser != null)
			{
				parser.SetError(explain);
				diagnostic.Set(DiagnosticType.Error, parser.Diagnostic);
				return;
			}

			diagnostic.Set(DiagnosticType.Error, explain);
		}

		//-----------------------------------------------------------------------------
		virtual public void Reset()
		{
			expressionStack.Clear();   
			symbolTable = null;
		}

		//-----------------------------------------------------------------------------
		public string Unparse()
		{
			string str = string.Empty;
			ExpressionUnparser unparser = new ExpressionUnparser();
			unparser.Unparse(out str, expressionStack);
			return str;
		}

		//-----------------------------------------------------------------------------
		virtual public bool Compile(Parser aParser, CheckResultType check, string type)
		{
			bool ok = true;
			parser = aParser;
			expressionStack.Clear();

            string preExprAudit = string.Empty;
            bool currDoAudit = aParser.DoAudit;
            if (currDoAudit)
                preExprAudit = aParser.GetAuditString();
            else
                aParser.DoAudit = true;

			// creo la struttura postfissa all'interno dello stack di espressione
			ExpressionParser expParser = CreateParser();
			ok = expParser.Parse(parser, expressionStack);
            this.HasRuleFields = expParser.HasRuleFields;

            this.auditExpr = aParser.GetAuditString();
            if (currDoAudit)
                aParser.SetAuditString (preExprAudit.IsNullOrEmpty() ? this.auditExpr : preExprAudit + ' ' + this.auditExpr);
            else
                aParser.DoAudit = false;

			// lo stack non può essere vuoto altrimenti avrei una espressione vuota da analizzare
			if (ok && expressionStack.Count == 0) 
			{
                Debug.Assert(false, "Error in Expression.Compile");

				SetError(ExpressionManagerStrings.EmptyExpression);
				ok = false;
			}
			// nel caso del localize la connessione non esiste e pertanto devo disabilitare tutti i controlli di tipo
			// perche viene forzata la compialzione ma non ho database di appoggio e quindi non conosco i tipi delle colonne
			if (!SkipTypeChecking)
			{
                if (ok)
                {
                    resultType = Compile(expressionStack);

                    Debug.Assert(resultType != null, "Error in Expression.Compile");
                }
				ok = ok && resultType != null;

				if (ok)
					switch (check)
					{
						case CheckResultType.Compatible	:
							if (!ObjectHelper.Compatible(resultType, type)) 
							{
                                Debug.Assert(resultType != null, "Error in Expression.Compile");
								SetError(string.Format(ExpressionManagerStrings.BadReturnType, resultType, type)); 
								ok = false; 
							}
							break;
						case CheckResultType.Match :
							if (resultType != type)
							{
                                if (resultType == "Int32" && type == "Int16")
                                    break;
                                if (resultType == "Int64" && (type == "Int32" || type == "Int16"))
                                    break;

                                Debug.Assert(resultType != null, "Error in Expression.Compile");
								SetError(string.Format(ExpressionManagerStrings.BadReturnType, resultType, type));
								ok = false; 
							}
							break;
					}
			}

			// rilascio il parser perchè serve solo durante la compilazione
			parser = null; 
			return ok;
		}

		//-----------------------------------------------------------------------------
		protected virtual ExpressionParser CreateParser()
		{
			return new ExpressionParser(reportSession, symbolTable, stopTokens);
		}

		// Determina se l'espressione parsata e` anche valutabile correttamente
		//-----------------------------------------------------------------------------
		public string Compile(Stack aExprStack)
		{
			Stack workStack = new Stack();
			Stack tmpStack = new Stack();

			Utility.MoveStack(aExprStack, workStack);

			Value v =  AnalyzeAll(workStack, tmpStack);
			if (v == null)
            {
                Debug.Assert(false, "Error in Expression.Compile");
				return null;
            }

			while (tmpStack.Count > 0)
				aExprStack.Push(tmpStack.Pop());

			return v.DataType;
		}

		//-----------------------------------------------------------------------------
		private Value AnalyzeAll(Stack workStack, Stack tmpStack)
		{
			if (!Analyze(workStack, tmpStack))
				return null;

			Value v = workStack.Pop() as Value;
			if (v == null)
			{
                Debug.Assert(false, "Error in Expression.AnalyzeAll");

				SetError(string.Format(ExpressionManagerStrings.BadReturnType, "", ""));
				return null;
			}

			//potrei avere funzioni concatenate(a.b().c()), in tal caso lo stack non sarebbe vuoto ed occorre analizzarlo
			//il valore di ritorno è quello della funzione più a destra
			if (
                workStack.Count != 0 &&								//ho ancora elementi
				workStack.Peek() is ThiscallFunctionItem			//il primo è una funzione
				)
				return AnalyzeAll(workStack, tmpStack);

			return v;
		}

		// Valuta l'espressione (entry point primario)
		//-----------------------------------------------------------------------------
		public bool Eval(ref Variable vrb)
		{
			if (resultType == null)
			{
                Debug.Assert(false, "Error in Expression.Eval");

				SetError(ExpressionManagerStrings.InvalidState);			
				return false;
			}

			if (!ObjectHelper.Compatible(resultType, vrb.DataType))
			{
                Debug.Assert(false, "Error in Expression.Eval");

                SetError(string.Format(ExpressionManagerStrings.BadReturnType, resultType, vrb.DataType));
				return false;
			}

			Value res = Eval();
            if (res == null)
            {
                Debug.Assert(false, "Error in Expression.Eval");

                return false;
            }
			if (!res.Valid)
			{
                Debug.Assert(false, "Error in Expression.Eval");

				SetError(ExpressionManagerStrings.InvalidOperation);
				return false;
			}

			AssignResult(vrb, res);
			return true;
		}

		// Valuta l'espressione e ritorna un Value del tipo giusto
		//-----------------------------------------------------------------------------
		public Value Eval()
		{
			return Eval(expressionStack);
		}

		//-----------------------------------------------------------------------------
		public Value Eval(Stack aExprStack)
		{
			// non modifica lo stack originario ma usa uno di lavoro
			Stack workStack = aExprStack.Clone() as Stack;

            if (!Execute(workStack))
            {
                //TODO: Le espressioni dinamiche di Woorm.Net quando valutano UseDefaultAttribute passano di qui
                //Debug.Assert(false, "Error in Expression.Eval");

                return null;
            }

			return ExtractValue(workStack);
		}

		//-----------------------------------------------------------------------------
		private Value ExtractValue(Stack workStack)
		{
			//prendo il valore in cima allo stack, è il valore di ritorno dell'espressione, 
			//ma potrebbe essere da utilizzare come this di una successiva chiamata a funzione
			//se lo stack è ancora pieno
			Value res = workStack.Pop() as Value;
			if (res == null)
			{
                Debug.Assert(false, "Error in Expression.Eval");

				SetError(ExpressionManagerStrings.Unknown);
				return null;
			}

			//se lo stack contiene ancora elementi ed il primo è una funzione, entro in ricorsione
			ThiscallFunctionItem f = null;
			if (
                    workStack.Count != 0 &&								//ho ancora elementi
				    (f = workStack.Peek() as ThiscallFunctionItem) != null 	//il primo è una funzione
				)
			{
				string tmpName = Guid.NewGuid().ToString();		//nome della variabile temporanea da usare come oggetto per la chiamata
				Variable v = new Variable(tmpName, res.Data);	//aggiungo il valore di ritorno della funzione precedente nella symboltable
				symbolTable.Add(v);

				Debug.Assert(f.ObjectName == "");

				f.ObjectName = tmpName; //assegno la variabile per effettuare la chiamata
				ExecuteFunction(workStack);
				f.ObjectName = "";
				symbolTable.Remove(v);

				return ExtractValue(workStack);
			}
			else
			{
				return res;
			}
		}

		//-----------------------------------------------------------------------------
		void AssignResult(Variable vrb, Value res)
		{
			if	(vrb.DataType == res.DataType)
			{
				vrb.Data = res.Data;
				vrb.Valid = res.Valid;
				return;
			}

			switch (vrb.DataType)
			{
				case "String"	: vrb.Data = CastString(res);	break;
				case "Double"	: vrb.Data = CastDouble(res);	break;
				case "Boolean"	: vrb.Data = CastBool(res);		break;
				case "Int16"	: vrb.Data = CastShort(res);	break;
				case "Int32"	: vrb.Data = CastInt(res);		break;
				case "Int64"	: vrb.Data = CastLong(res);		break;
				case "Guid"		: vrb.Data = CastGuid(res);		break;
                case "DataArray": 
                case "Array"    : vrb.Data = CastDataArray(res); break;
                case "DataEnum" :
 				case "Enum"	    : vrb.Data = CastDataEnum(res); break;
                case "Decimal"  : vrb.Data = CastDecimal(res);  break;							
				case "Float"	: vrb.Data = CastFloat(res);	break;

					// se succede vuol dire che le mappe di compatibilita` sono errate
				default: throw(new Exception("Illegal data type " + vrb.DataType));
			}
			
			vrb.Valid = res.Valid;
		}

		//-----------------------------------------------------------------------------
		Value CheckType	(OperatorItem ope, Value v1)					{ return CheckType(ope, v1, null, null); }
		Value CheckType	(OperatorItem ope, Value v1, Value v2)			{ return CheckType(ope, v1, v2, null); }
		Value CheckType	(OperatorItem ope, Value v1, Value v2, Value v3)
		{
			string aResultType;

			if (ope.GetOperatorType() == OperatorItem.OperatorType.Logical)
			{
				Debug.Assert(v2 == null && v3 == null);

				aResultType = Compile(ope.SecondStack);
				if (aResultType != null)
				{
					if (ope.OperatorID == Token.BETWEEN || ope.OperatorID == Token.QUESTION_MARK)
					{
						v3 = new Value();
                        v3.Data = ObjectHelper.CreateObject(aResultType);

						aResultType = Compile(ope.FirstStack);
						if (aResultType != null)
						{
							v2 = new Value();
							v2.Data = ObjectHelper.CreateObject(aResultType);
						}
					}
					else
					{
						v2 = new Value();
                        v2.Data = ObjectHelper.CreateObject(aResultType);
					}
				}
			}

			aResultType = GiveMeResultType(ope, v1, v2, v3);
			if (aResultType == null)
			{
				SetError(ExpressionManagerStrings.InvalidType);
				return null;
			}

			ope.ResultType = aResultType;

            object o = ObjectHelper.CreateObject(aResultType);
            if (o == null)
            {
                if (!aResultType.CompareNoCase("Void") && !aResultType.CompareNoCase("Variant"))
                {
                    SetError(ExpressionManagerStrings.InvalidType);
                    return null;
                }
            }

			Value res = new Value(o);
			return res;
		}

		//-----------------------------------------------------------------------------
		static public bool		CastBool		(Value d) { return ObjectHelper.CastBool(d.Data); }
		static public string	CastString		(Value d) { return ObjectHelper.CastString(d.Data); }
		static public Guid		CastGuid		(Value d) { return ObjectHelper.CastGuid(d.Data); }
		static public short		CastShort		(Value d) { return ObjectHelper.CastShort(d.Data); }
        static public ushort    CastUShort      (Value d) { return ObjectHelper.CastUShort(d.Data); }
        static public int       CastInt         (Value d) { return ObjectHelper.CastInt(d.Data); }
		static public long		CastLong		(Value d) { return ObjectHelper.CastLong(d.Data); }
		static public decimal	CastDecimal		(Value d) { return ObjectHelper.CastDecimal(d.Data); }
		static public float		CastFloat		(Value d) { return ObjectHelper.CastFloat(d.Data); }
		static public double	CastDouble		(Value d) { return ObjectHelper.CastDouble(d.Data); }
		static public DateTime	CastDateTime	(Value d) { return ObjectHelper.CastDateTime(d.Data); }
		static public DataEnum	CastDataEnum	(Value d) { return ObjectHelper.CastDataEnum(d.Data); }
		static public DataArray CastDataArray	(Value d) { return ObjectHelper.CastDataArray(d.Data); }

		//-----------------------------------------------------------------------------
		Value GiveMeResult(OperatorItem ope, Value o1)
		{
			Object data = null;
			Value retValue = null;
			bool valid2 = o1.Valid;

            Debug.Assert(!(o1.Data is Item));

			switch (ope.OperatorID)
			{
				case Token.EXPR_UNNARY_MINUS	:
				{
					switch (ope.ResultType)
					{
						case "Double":	data = -CastDouble(o1);	break;
						case "Int16":	data = -CastShort(o1);	break;
						case "Int32":	data = -CastInt(o1);	break;
						case "Int64":	data = -CastLong(o1);	break;
						case "Single":	data = -CastFloat(o1);	break;
					}
					break;
				}
				case Token.NOT		:
				case Token.OP_NOT	:
				{
					data = !CastBool(o1);
					break;
				}

				case Token.AND		:
				case Token.OP_AND	:
				case Token.OR		:
				case Token.OP_OR	:
				{
					bool bRes = CastBool(o1);
					if	(
						valid2 &&
						(
						(ope.OperatorID == Token.AND || ope.OperatorID == Token.OP_AND)
						? bRes
						: !bRes
						)
						)
					{
						retValue = Eval(ope.SecondStack);
						if (retValue != null)
						{
							valid2 = retValue.Valid;
							bRes = CastBool(retValue);
						}
					}

					data = bRes;
					break;
				}
				case Token.QUESTION_MARK :
				{
					retValue = CastBool(o1) ? Eval(ope.FirstStack) : Eval(ope.SecondStack);
                    if (retValue != null)
                    {
                        valid2 = valid2 && retValue.Valid;
                        data = retValue.Data;
                    }
                    else
                    {
                        valid2 = false;
                        data = false;
                    }
					break;
				}
				case Token.BETWEEN	:
				{
					bool bRes = false;
					if (valid2)
					{
						retValue = Eval(ope.FirstStack);
						valid2 = retValue.Valid;
						if (valid2 && (o1 >= retValue))
						{
							retValue = Eval(ope.SecondStack);
							valid2 = retValue.Valid;

							if (valid2)
								bRes = o1 <= retValue;
						}
					}

					data = bRes;
					break;
				}
                // lo stato valid2 in questo caso e` stato usato per valorizzare
                // il ritorno della operazione
                case Token.EXPR_IS_NULL:
                {
                    data = !valid2;
                    valid2 = true;
                    break;
                }

                // lo stato valid2 in questo caso e` stato usato per valorizzare
                // il ritorno della operazione
                case Token.EXPR_IS_NOT_NULL:
                {
                    data = valid2;
                    valid2 = true;
                    break;
                }
			}

			if (data != null)
			{
				retValue = new Value(data);
				retValue.Valid = valid2;
			}
			else
            {
                Debug.Assert(false, "Expression.GiveMeResult");
				SetError(ExpressionManagerStrings.InvalidType);
            }

			return retValue;
		}
        
		//-----------------------------------------------------------------------------
		Value GiveMeResult(OperatorItem ope, Value o1, Value o2)
		{
			Object data = null;
			bool valid2 = o1.Valid && o2.Valid;

            Debug.Assert(!(o1.Data is Item));
            Debug.Assert(!(o2.Data is Item));

			switch (ope.ResultType)
			{
                case "Boolean":
                    {
                        switch (ope.OperatorID)
                        {
                            case Token.GT: data = o1 > o2; break;
                            case Token.GE: data = o1 >= o2; break;
                            case Token.LT: data = o1 < o2; break;
                            case Token.LE: data = o1 <= o2; break;
                            case Token.EQ:
                            case Token.ASSIGN: data = o1 == o2; break;
                            case Token.NE:
                            case Token.DIFF: data = o1 != o2; break;
                            case Token.LIKE:
                                {
                                    data = LikeFunction(CastString(o1), CastString(o2));
                                    break;
                                }
                            case Token.IN:
                                {
                                    DataArray ar = CastDataArray(o2);

                                    data = ar.Find(o1.Data) > -1;
                                    break;
                                }
                            case Token.CONTAINS:
                                {
                                    DataArray ar = CastDataArray(o1);

                                    data = ar.Find(o2.Data) > -1;
                                    break;
                                }
                        }
                        break;
                    }

                case "Double":
                    {
                        double d1 = CastDouble(o1);
                        double d2 = CastDouble(o2);

                        switch (ope.OperatorID)
                        {
                            case Token.PLUS: data = d1 + d2; break;
                            case Token.MINUS: data = d1 - d2; break;
                            case Token.STAR: data = d1 * d2; break;
                            case Token.SLASH:
                                {
                                    double nVal = d2;
                                    if (nVal == 0)
                                    {
										SetError(ExpressionManagerStrings.DivisionByZero);
                                        data = 0.0;
                                        valid2 = false;
                                    }
                                    else
                                        data = d1 / nVal;

                                    break;
                                }
                        }
                        break;
                    }

                case "String":
                    {
                        string s1 = CastString(o1);
                        string s2 = CastString(o2);

                        if (ope.OperatorID == Token.PLUS) data = s1 + s2;
                        break;
                    }

                case "Int16":
				{
					short l1 = CastShort(o1);
					short l2 = CastShort(o2);

					switch (ope.OperatorID)
					{
						case Token.PLUS		: data = l1 + l2;	break;
						case Token.MINUS	: data = l1 - l2;	break;
						case Token.STAR		: data = l1 * l2;	break;
						case Token.SLASH	:
						case Token.PERC		:
						{
							long nVal = l2;
							if (nVal == 0)
							{
								SetError(ExpressionManagerStrings.DivisionByZero);
								data = 0xFFFFFFFF;
								valid2 = false;
							}
							else
								if (ope.OperatorID == Token.SLASH)
								data = l1 / nVal;
							else
								data = l1 % nVal;

							break;
						}
						case Token.BW_AND	: data = l1 & l2;	break;
						case Token.BW_OR	: data = (ushort)l1 | (ushort)l2;	break;
						case Token.BW_XOR	: data = l1 ^ l2;	break;
					}
					break;
				}	

				case "Int32":
				{
					// caso speciale di (data - data) viene gestito a parte
					if ((o1.Data is DateTime) && (o1.Data is DateTime) && (ope.OperatorID == Token.MINUS))
					{
						DateTime d1 = CastDateTime(o1);
						DateTime d2 = CastDateTime(o2);
						TimeSpan ts = d1 - d2;

						data = (int)ts.TotalDays;
						break;
					}

					int i1 = CastInt(o1);
					int i2 = CastInt(o2);
					switch (ope.OperatorID)
					{
						case Token.PLUS		: data = i1 + i2;	break;
						case Token.MINUS	: data = i1 - i2;	break;
						case Token.STAR		: data = i1 * i2;	break;
						case Token.SLASH	:
						case Token.PERC		:
						{
							if (i2 == 0)
							{
								SetError(ExpressionManagerStrings.DivisionByZero);
								data = 0;
								valid2 = false;
							}
							else
								if (ope.OperatorID == Token.SLASH)
								data = i1 / i2;
							else
								data = i1 % i2;

							break;
						}
						case Token.BW_AND	: data = i1 & i2;	break;
						case Token.BW_OR	: data = i1 | i2;	break;
						case Token.BW_XOR	: data = i1 ^ i2;	break;
					}
					break;
				}
	
				case "Int64":
				{
					long l1 = CastLong(o1);
					long l2 = CastLong(o2);

					switch (ope.OperatorID)
					{
						case Token.PLUS		: data = l1 + l2;	break;
						case Token.MINUS	: data = l1 - l2;	break;
						case Token.STAR		: data = l1 * l2;	break;
						case Token.SLASH	:
						case Token.PERC		:
						{
							long nVal = l2;
							if (nVal == 0)
							{
								SetError(ExpressionManagerStrings.DivisionByZero);
								data = 0xFFFFFFFF;
								valid2 = false;
							}
							else
								if (ope.OperatorID == Token.SLASH)
								data = l1 / nVal;
							else
								data = l1 % nVal;

							break;
						}
						case Token.BW_AND	: data = l1 & l2;	break;
						case Token.BW_OR	: data = l1 | l2;	break;
						case Token.BW_XOR	: data = l1 ^ l2;	break;
					}
					break;
				}	

                case "DateTime":
                {
                    bool fd = (o2.Data is int) || (o2.Data is long);
                    int days = fd ? CastInt(o2) : CastInt(o1);
                    DateTime d = fd ? CastDateTime(o1) : CastDateTime(o2);
                    TimeSpan t = new TimeSpan(days, 0, 0, 0, 0); // considera solo giorni da sommare

                    switch (ope.OperatorID)
                    {
                        case Token.PLUS: data = d + t; break;
                        case Token.MINUS: data = d - t; break;
                    }
                    break;
                }
	
                case "Single":
                {
                    float d1 = CastFloat(o1);
                    float d2 = CastFloat(o2);

                    switch (ope.OperatorID)
                    {
                        case Token.PLUS: data = d1 + d2; break;
                        case Token.MINUS: data = d1 - d2; break;
                        case Token.STAR: data = d1 * d2; break;
                        case Token.SLASH:
                            {
                                float nVal = d2;
                                if (nVal == 0)
                                {
									SetError(ExpressionManagerStrings.DivisionByZero);
                                    data = 0.0;
                                    valid2 = false;
                                }
                                else
                                    data = d1 / nVal;

                                break;
                            }
                    }
                    break;
                }	
			}

			Value retValue = null;

            if (data != null)
            {
                retValue = new Value(data);
                retValue.Valid = valid2;
            }
            else
            {
                Debug.Assert(false, "Expression.GiveMeResult");
                SetError(ExpressionManagerStrings.InvalidType);
            }
			return retValue;
		}

		//-----------------------------------------------------------------------------
		Value GiveMeResult(OperatorItem ope, Value o1, Value o2, Value o3)
		{
			Object data = null;
			switch (ope.OperatorID)
			{
				case Token.EXPR_ESCAPED_LIKE:
					if (ope.ResultType == "Boolean")
						data = LikeFunction(CastString(o1), CastString(o2), CastString(o3));
					break;
			}

			Value retValue = null;

            if (data != null)
            {
                retValue = new Value(data);
                retValue.Valid = o1.Valid && o2.Valid && o3.Valid;
            }
            else
            {
                Debug.Assert(false, "Expression.GiveMeResult");
                SetError(ExpressionManagerStrings.InvalidType);
            }
			return retValue;
		}

		//-----------------------------------------------------------------------------
		Value ApplyFunction(FunctionItem function, Stack paramStack)
		{
            Token tkFun = Language.GetKeywordsToken(function.Name);
            switch (tkFun)
			{
                case Token.NOTOKEN:
                    break;

				case Token.FORMAT:
				{
					Value p1 = (Value) paramStack.Pop();

					if (function.Parameters.Count == 2)
					{
					    string data = CastString(p1);   //lo converte
						Value p2 = (Value) paramStack.Pop();
						string r1 = ReportSession.ApplicationFormatStyles.Format(CastString(p2), data, null);
						return new Value(r1);
					}

					// usa i formattatori di default
					string r2 = ReportSession.ApplicationFormatStyles.Format(p1.Data, null);
					return new Value(r2);
				}

				case Token.DATE:
				{
					if (function.Parameters.Count == 3)
					{
						Value p1 = (Value) paramStack.Pop();
						Value p2 = (Value) paramStack.Pop();
						Value p3 = (Value) paramStack.Pop();
						int day		= CastInt(p1);
						int month	= CastInt(p2);
						int year	= CastInt(p3);
						DateTime dt = new DateTime(year, month, day);
						return new Value(dt);
					}
			
					return new Value(DateTime.Today);
				}

				case Token.DATETIME:
				{
					if (function.Parameters.Count == 6)
					{
						Value p1 = (Value) paramStack.Pop();
						Value p2 = (Value) paramStack.Pop();
						Value p3 = (Value) paramStack.Pop();
						Value p4 = (Value) paramStack.Pop();
						Value p5 = (Value) paramStack.Pop();
						Value p6 = (Value) paramStack.Pop();

						int day	= CastInt(p1);
						int month	= CastInt(p2);
						int year	= CastInt(p3);
						int hour	= CastInt(p4);
						int minute	= CastInt(p5);
						int second	= CastInt(p6);

						DateTime dt = new DateTime(year, month, day, hour, minute, second);
						return new Value(dt);
					}
			
					return new Value(DateTime.Now);
				}

				case Token.TIME:
				{
					if (function.Parameters.Count == 3)
					{
						Value p1 = (Value) paramStack.Pop();
						Value p2 = (Value) paramStack.Pop();
						Value p3 = (Value) paramStack.Pop();
						int hour	= CastInt(p1);
						int minute	= CastInt(p2);
						int second	= CastInt(p3);

						DateTime dt = new DateTime
							(
                            DateTimeFunctions.MinTimeYear, DateTimeFunctions.MinTimeMonth,
                            DateTimeFunctions.MinTimeDay, hour, minute, second
							);
						return new Value(dt);
					}

                    DateTime now = new DateTime
                        (
                            DateTimeFunctions.MinTimeYear, DateTimeFunctions.MinTimeMonth,
                            DateTimeFunctions.MinTimeDay, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second
                        );
					return new Value(now);
				}

				case Token.ELAPSED_TIME:
				{
					long elapsed = 0L;

					if (function.Parameters.Count == 2)
					{
						Value p1 = (Value) paramStack.Pop();
						Value p2 = (Value) paramStack.Pop();

						DateTime dt1 = CastDateTime(p1);
						DateTime dt2 = CastDateTime(p2);

						TimeSpan ts = dt2 - dt1;
						elapsed = (long) ts.TotalSeconds;
					}

					return new Value(elapsed);
				}


				case Token.DTOC:
				{
					Value p1 = (Value) paramStack.Pop();
					DateTime dt = CastDateTime(p1);
					string s = dt.ToString("yyyyMMdd");
					return new Value(s);
				}

				case Token.CDOW:
				{
					Value p1 = (Value) paramStack.Pop();
					DateTime dt = CastDateTime(p1);
					string s = DateTimeFunctions.WeekdayName(dt);
					return new Value(s);
				}

				case Token.CMONTH:
				{
					Value p1 = (Value) paramStack.Pop();
					DateTime dt = CastDateTime(p1);
					string s = DateTimeFunctions.MonthName(dt);
					return new Value(s);
				}

				case Token.CTOD:
				{
					Value p1 = (Value) paramStack.Pop();
					string str = CastString(p1);
                    string[] format = { "yyyyMMdd" };
                    DateTime date;

                    if (DateTime.TryParseExact(str,
                                               format,
                                               System.Globalization.CultureInfo.InvariantCulture,
                                               System.Globalization.DateTimeStyles.None,
                                               out date))
                    {
                        return new Value(date);
                    }

                    DateTime dt = DateTime.Parse(str);
                    return new Value(dt);
				}
				case Token.CTYPE:
				{
					Value p1 = (Value)paramStack.Pop();//valore da convertire
					Value p2 = (Value)paramStack.Pop();//tipo di destinazione in formato stringa
					object o = ObjectHelper.CreateObject((string)p2.Data);//creo un oggetto "template" per avere un tipo di destinazione
					object converted = ObjectHelper.ConvertType(p1.Data, o.GetType());//effettuo il cast
					return new Value(converted);
				}
				case Token.LAST_MONTH_DAY:
				{
					Value p1 = (Value) paramStack.Pop();
					DateTime dt = CastDateTime(p1);
					int day = DateTime.DaysInMonth(dt.Year, dt.Month);
					DateTime dt2 = new DateTime(dt.Year, dt.Month, day);
					return new Value(dt2);
				}

				case Token.MONTH_NAME:
				{
					Value p1 = (Value) paramStack.Pop();
					int month = CastInt(p1);
					string s = DateTimeFunctions.MonthName(month);
					return new Value(s);
				}

				case Token.APPYEAR:
				{
					int y = (ReportSession.UserInfo == null) 
						? DateTime.Today.Year 
						: ReportSession.UserInfo.ApplicationDate.Year;
					return new Value(y);
				}

				case Token.APPDATE:
				{
					DateTime ad = (ReportSession.UserInfo == null) 
						? DateTime.Today 
						: ReportSession.UserInfo.ApplicationDate;
					return new Value(ad);
				}

				case Token.GIULIANDATE:
				{
					Value p1 = (Value) paramStack.Pop();
					DateTime dt = CastDateTime(p1);
					long l = DateTimeFunctions.GiulianDate(dt);
					return new Value(l);
				}

				case Token.MONTH_DAYS:
				{
					Value p1 = (Value) paramStack.Pop();
					Value p2 = (Value) paramStack.Pop();
					int month = CastInt(p1);
					int year = CastInt(p2);
					int i = DateTime.DaysInMonth(year, month);
					return new Value(i);
				}
					
				case Token.DAY:
				{
					Value p1 = (Value) paramStack.Pop();
					DateTime dt = CastDateTime(p1);
					int i = dt.Day;
					return new Value(i);
				}
						
				case Token.MONTH:
				{
					Value p1 = (Value) paramStack.Pop();
					DateTime dt = CastDateTime(p1);
					int i = dt.Month;
					return new Value(i);
				}
						
				case Token.DAYOFWEEK:
				{
					Value p1 = (Value) paramStack.Pop();
					DateTime dt = CastDateTime(p1);
					int i = DateTimeFunctions.DayOfWeek(dt) + 1;
					return new Value(i);
				}
				
				case Token.WEEKOFMONTH:
				{
					Value p1 = (Value) paramStack.Pop();
					DateTime dt = CastDateTime(p1);

					if (function.Prototype.NrParameters == 2)
					{
						Value p2 = (Value)paramStack.Pop();
						int alg = CastInt(p2);
						if (alg == 1)
							return new Value(DateTimeFunctions.WeekOfMonthISO(dt));
					}
					return new Value(DateTimeFunctions.WeekOfMonth(dt));
				}
				
				case Token.WEEKOFYEAR:
				{
					Value p1 = (Value) paramStack.Pop();
					DateTime dt = CastDateTime(p1);
					int i = DateTimeFunctions.WeekOfYear(dt);
					return new Value(i);
				}
				
				case Token.DAYOFYEAR:
				{
					Value p1 = (Value) paramStack.Pop();
					DateTime dt = CastDateTime(p1);
					int i = dt.DayOfYear;
					return new Value(i);
				}

				case Token.YEAR:
				{
					Value p1 = (Value) paramStack.Pop();
					DateTime dt = CastDateTime(p1);
					return new Value(dt.Year);
				}

					// substring word wrap
				case Token.SUBSTRWW:
				{
					Value p1 = (Value) paramStack.Pop();
					Value p2 = (Value) paramStack.Pop();
					Value p3 = (Value) paramStack.Pop();
					string s = CastString(p1);
					if (s.Length > 0)
					{
						int len = s.Length;
						int start = CastInt(p2) - 1;
						if (start < 0) start = 0;

						// massimo numero di caratteri da ritornare
						int maxChars = CastInt(p3);
						if (maxChars < 0) maxChars = 0;

						int chars = maxChars;
						if	(start < len && (chars <= 0 || (start + chars) > len))
							chars = len - start;

						if (chars <= 0 || start >= len)
							s = "";
						else
						{
							if (chars != len - start)
							{

								while (chars > 0 && char.IsWhiteSpace(s[start + chars]))
									chars--; 

								if (chars == 0)
									chars = maxChars;
							}
			
							// non vengono strippati i blank ne` a destra ne` a sinistra
							// della stringa ritornata
							s = s.Mid(start, chars);
						}
					}
					return new Value(s);
				}
		
				case Token.LOADTEXT:
				{
					Value p1 = (Value) paramStack.Pop();
					string filename = CastString(p1);
                    //TODO potrebbe essere un namespace
					string result = "";

					StreamReader inputFile = null;
					try
					{
                        if (!File.Exists(filename))
                        {
                            NameSpace ns = new NameSpace(filename);
                            if (ns.IsValid())
                                filename = ReportSession.PathFinder.GetFilename(ns, string.Empty);
                        }

                        if (File.Exists(filename)) 
						{
							FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
							inputFile = new StreamReader(fs, System.Text.Encoding.GetEncoding(0));
							result = inputFile.ReadToEnd();
						}
						else
                            result = ExpressionManagerStrings.FileNotFound + CastString(p1);
					}
					catch (IOException e)
					{
						result = e.ToString();
					}
					finally
					{
						if (inputFile != null)
							inputFile.Close();
					}
					return new Value(result);
				}
                case Token.SAVETEXT:
                {
                    Value p1 = (Value)paramStack.Pop();
                    Value p2 = (Value)paramStack.Pop();

                    StreamWriter oFile = null;
                    string filename = CastString(p1);
                    
                    string text = CastString(p2);
                    
                    int nfmt = 0;
                    if (function.Parameters.Count == 3)
                    {
                        Value v2 = (Value)paramStack.Pop();
                        nfmt = CastInt(v2);
                    }
                    System.Text.Encoding enc = System.Text.Encoding.ASCII;
                    switch (nfmt)
                    { 
                        case 1:
                            enc = System.Text.Encoding.UTF8;
                            break;
                        case 2:
                            enc = System.Text.Encoding.BigEndianUnicode;
                            break;
                        case 3:
                            enc = System.Text.Encoding.Unicode;
                            break;

                        case 0:
                        default:
                            break;
                    }

                    try
                    {
                        if (!File.Exists(filename))
                        {
                            NameSpace ns = new NameSpace(filename);
                            if (ns.IsValid())
                                filename = ReportSession.PathFinder.GetFilename(ns, string.Empty);
                        }

                        if (File.Exists(filename)) 
						{
                            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read);
                            oFile = new StreamWriter(fs, enc);
                            oFile.Write(text);
                        }
                        else
                            return new Value(false);

                    }
                    catch (IOException)
                    {
                        return new Value(false);
                    }
                    finally
                    {
                        if (oFile != null)
                        {
                            oFile.Close();
                            oFile.Dispose();
                        }
                    }
                    return new Value(true);
                }
		
				case Token.LOCALIZE:
				{
					Value v1 = (Value) paramStack.Pop();
					string localized = CastString(v1);

					if (function.Parameters.Count == 2)
                    {
					    Value v2 = (Value) paramStack.Pop();
					    string filename = CastString(v2);
						WoormLocalizer loc = new WoormLocalizer(filename, ReportSession.PathFinder);
					    if (loc != null) 
                            localized = loc.Translate(localized);
                    }
                    else
					    localized = Localizer.Translate(localized); 
					
					return new Value(localized);
				}	

				case Token.ISACTIVATED:
				{
					Value v1 = (Value) paramStack.Pop();
					string application = CastString(v1);

					Value v2 = (Value) paramStack.Pop();
					string functionality = CastString(v2);

					bool ok = 
						ReportSession.UserInfo.LoginManager != null &&
						ReportSession.UserInfo.LoginManager.IsActivated(application, functionality);

					return new Value(ok);
				}
                case Token.ISADMIN:
                {
                    bool ok = ReportSession.UserInfo.Admin;

                    return new Value(ok);
                }

                case Token.ISWEB:
                case Token.ISREMOTEINTERFACE:
                {
                     return new Value(true);
                }
                case Token.ISRUNNINGFROMEXTERNALCONTROLLER:
                {
                    return new Value(false);
                }

                case Token.ABS:
				{
					Value v = (Value) paramStack.Pop();
					double d = Math.Abs(CastDouble(v));
					return new Value(d);
				}	
				case Token.ASC:
				{
					Value v = (Value) paramStack.Pop();
					string s = CastString(v);
					int i = (int)s[0];
					return new Value(i);
				}
				case Token.CEILING:
				{
					Value v = (Value) paramStack.Pop();
					double d = Math.Ceiling(CastDouble(v));
					return new Value(d);
				}
				case Token.FLOOR:
				{
					Value v = (Value) paramStack.Pop();
					double d = Math.Floor(CastDouble(v));
					return new Value(d);
				}

				case Token.MAX:
				case Token.MIN:
				{
                    if (function.CurrentParametersCount == 1)
                    {
                        Value va = (Value)paramStack.Pop();
                        DataArray ar = CastDataArray(va);

                        return tkFun == Token.MAX ? new Value(ar.GetMaxElem()) : new Value(ar.GetMinElem());
                    }

                    Value v1 = (Value)paramStack.Pop();
                    Value ret = v1;
                    for (int np = 1; np < function.CurrentParametersCount; np++)
                    {
                        Value v2 = (Value)paramStack.Pop();

                        if (!ObjectHelper.CheckType(v2.Data, v1.Data))
                        {
                            //TODO ErrHandler(FORMAT_TYPEERR, itemFun);
                            return null;
                        }

                        if (
                            tkFun == Token.MAX
                                ?
                                ret < v2
                                :
                                ret > v2
                            )
                        {
                            ret = v2;
                        }
                    }
                    return (Value)ret.Clone();
				}

                case Token.ISEMPTY:
                {
                    Value v = (Value)paramStack.Pop();

                    bool b = !v.Valid || v.Data == null || ObjectHelper.IsCleared(v.Data);

                    return new Value(b);
                }
                case Token.ISNULL:
                {
                    Value v = (Value)paramStack.Pop();
                    Value v2 = (Value)paramStack.Pop();

                    bool b = !v.Valid || v.Data == null || ObjectHelper.IsCleared(v.Data);

                    return b ? (Value)v2.Clone() : (Value)v.Clone();
                }

				case Token.LEN:
				{
					Value p1 = (Value) paramStack.Pop();
					string s = CastString(p1);
					int i = s.Length;
					return new Value(i);
				}
				case Token.LEFT:
				{
					Value p1 = (Value) paramStack.Pop();
					Value p2 = (Value) paramStack.Pop();
					string s = CastString(p1);
					int i = CastInt(p2);
					if (i < 0) i = 0;
					string s2 = s.Left(i);
					return new Value(s2);
				}
                case Token.FIND:
				{
					Value p1 = (Value) paramStack.Pop();
					Value p2 = (Value) paramStack.Pop();

					string s = CastString(p2);
					int startIndex = 0;
                    int occurence = 1;
					if (function.Parameters.Count > 2)
					{
						Value p3 = (Value) paramStack.Pop();
						startIndex = CastInt(p3);

                        if (function.Parameters.Count > 3)
                        {
                            Value p4 = (Value)paramStack.Pop();
                            occurence = CastInt(p4);
                        }
					}
                    int i = s.IndexOfOccurrence(CastString(p1), occurence, startIndex);
					return new Value(i);
				}
				case Token.REVERSEFIND:
				{
					Value p1 = (Value) paramStack.Pop();
					Value p2 = (Value) paramStack.Pop();

					string s = CastString(p2);
					int startIndex = s.Length - 1;;
                    int occurence = 1;
                    
                    if (function.Parameters.Count > 2)
					{
						Value p3 = (Value) paramStack.Pop();
						startIndex = CastInt(p3);

                        if (function.Parameters.Count > 3)
                        {
                            Value p4 = (Value)paramStack.Pop();
                            occurence = CastInt(p4);
                        }
					}
                    int i = s.LastIndexOfOccurrence(CastString(p1), occurence, startIndex);
					return new Value(i);
				}
                case Token.WILDCARD_MATCH:
                {
                    Value p1 = (Value)paramStack.Pop();
                    Value p2 = (Value)paramStack.Pop();
                    
                    string s = CastString(p1);

                    bool b = s.WildcardMatch(CastString(p2));

                    return new Value(b);
                }
                case Token.REPLACE:
				{
					Value p1 = (Value) paramStack.Pop();
					Value p2 = (Value) paramStack.Pop();
					Value p3 = (Value) paramStack.Pop();
					string s = CastString(p1);
					s = s.Replace(CastString(p2), CastString(p3));
					return new Value(s);
				}
/* TODO RSWEB
                case Token.REPLICATE:
                {
                    //TODO
                    return new Value(string.Empty);
                }
                case Token.PADLEFT:
                {
                    //TODO
                    return new Value(string.Empty);
                }
                case Token.PADRIGHT:
                {
                    //TODO
                    return new Value(string.Empty);
                }
comparenocase
*/
                case Token.REMOVENEWLINE:
				{
					Value p1 = (Value) paramStack.Pop();
					string s = CastString(p1);
					s = s.Replace("\r\n", " ");
					s = s.Replace("\r", " ");
					s = s.Replace("\n", " ");
					return new Value(s);
				}
				case Token.MOD:
				{
					Value p1 = (Value) paramStack.Pop();
					Value p2 = (Value) paramStack.Pop();
					double d1 = CastDouble(p1);
					double d2 = CastDouble(p2);
					int i = (int) Math.IEEERemainder(d1, d2);
					return new Value(i);
				}
				case Token.RAND:
				{
					Random rand = new Random();
					double d = rand.NextDouble();
					return new Value(d);
				}
				case Token.RIGHT:
				{
					Value p1 = (Value) paramStack.Pop();
					Value p2 = (Value) paramStack.Pop();
					string s1 = CastString(p1);
					int i = CastInt(p2);
					if (i < 0) i = 0;
					string s2 = s1.Right(i);
					return new Value(s2);
				}
				case Token.ROUND:
				{
					Value p1 = (Value) paramStack.Pop();
					double d = Math.Round(CastDouble(p1));
					if (function.Parameters.Count == 2)
					{
						Value p2 = (Value) paramStack.Pop();
						int digits = CastInt(p2);
					    d = Math.Round(CastDouble(p1), digits);
					}
					return new Value(d);
				}
				case Token.SIGN:
				{
					Value p1 = (Value) paramStack.Pop();
					int i = CastDouble(p1) >= 0.0 ? +1 : -1;
					return new Value(i);
				}
				case Token.SPACE:
				{
					Value p1 = (Value) paramStack.Pop();
					int count = CastInt(p1);
					string s = new string(' ', count);
					return new Value(s);
				}
				case Token.LTRIM:
				{
					Value p1 = (Value) paramStack.Pop();
					string s = CastString(p1);
					return new Value(s.TrimStart());
				}
				case Token.RTRIM:
				{
					Value p1 = (Value) paramStack.Pop();
					string s1 = CastString(p1);
					string s2 = s1.TrimEnd();
					return new Value(s2);
				}
				case Token.TRIM:
				{
					Value p1 = (Value) paramStack.Pop();
					string s1 = CastString(p1);
					string s2 = s1.Trim();
					return new Value(s2);
				}
				case Token.UPPER:
				{
					Value p1 = (Value) paramStack.Pop();
					string s1 = CastString(p1);
					bool bLocalizedCompare = true;

					if (function.Parameters.Count == 2)
					{
						Value v2 = (Value) paramStack.Pop();	//Optional invariantCulture bool
						bLocalizedCompare = !CastBool(v2);
					}
					string s2 = bLocalizedCompare 
						        ? s1.ToUpper(p1.CollateCulture) 
						        : s1.ToUpper(CultureInfo.InvariantCulture);
					return new Value(s2);
				}
				case Token.LOWER:
				{
					Value p1 = (Value) paramStack.Pop();
					string s1 = CastString(p1);
					bool bLocalizedCompare = true;

					if (function.Parameters.Count == 2)
					{
						Value v2 = (Value) paramStack.Pop();	//Optional invariantCulture bool
						bLocalizedCompare = !CastBool(v2);
					}
					string s2 = bLocalizedCompare 
						? s1.ToLower(p1.CollateCulture) 
						: s1.ToLower(CultureInfo.InvariantCulture);
		
					return new Value(s2);
				}
				case Token.VAL:
				{
					Value p1 = (Value) paramStack.Pop();
					string s = CastString(p1);
					double d = 0.0;
					if (!string.IsNullOrEmpty(s))
					{
						s = RemoveNonDigitChar(s);
						d = double.Parse(s, NumberFormatInfo.InvariantInfo);
					}
					return new Value(d);
				}
				case Token.FINT:
				{
					Value p1 = (Value) paramStack.Pop();
                    double d = CastDouble(p1);
                    int i = (int) d;
					return new Value(i);
				}
				case Token.FLONG:
				{
					Value p1 = (Value) paramStack.Pop();
                    double d = CastDouble(p1);
                    long l = (long)d;
					return new Value(l);
				}
				case Token.TYPED_BARCODE:
				{
					Value p1 = (Value) paramStack.Pop();
					Value p2 = (Value) paramStack.Pop();

					string itemBC = CastString(p1);
					int typeBC =  CastInt(p2);

                    int chkSum = 0;
                    string humanReadable = string.Empty;

                    if (function.Parameters.Count > 2)
                    {
                        Value p3 = (Value)paramStack.Pop();
                        chkSum = CastInt(p3);

                        if (function.Parameters.Count > 3)
                        {
                            Value p4 = (Value)paramStack.Pop();
                            humanReadable = CastString(p4);
                        }
                    }
                    return new Value(EncodeBarCode(itemBC, typeBC, chkSum, humanReadable));
				}
                case Token.GETBARCODE_ID:
                {
                    int nBc = 0;
                    Value p1 = (Value)paramStack.Pop();
                    if (function.Parameters.Count == 2)
                    {
                        Value p2 = (Value)paramStack.Pop();
                        string nameBC = CastString(p2);

                        nBc = GetBarCodeId(nameBC);
                    }
                    else
                    {
                        long enumBC = CastLong(p1);

                        nBc = GetBarCodeId(enumBC);
                    }
                    return new Value(nBc);
               }
				case Token.CHR:
				{
					Value p1 = (Value) paramStack.Pop();
					string s = new string((char)CastInt(p1), 1);
					return new Value(s);
				}
				case Token.STR:
				{
					Value p1 = (Value) paramStack.Pop();
					Value p2 = (Value) paramStack.Pop();
	
					int len = CastInt(p2);
					if (function.Parameters.Count == 3)
					{
						Value p3 = (Value) paramStack.Pop();
						int precision = CastInt(p3);
						string f = "{0,-" + len.ToString() + ":F" + precision.ToString() + "}";
						string s3 = string.Format(f, p1.Data);
						return new Value(s3);
					}

					string s = "{0,-" + len.ToString();
					s += (p1.DataType == "Single" || p1.DataType == "Double") ? ":F}" : ":D}";
					
					string s2 = string.Format(s, p1.Data);
					return new Value(s2);
				}
				case Token.SUBSTR:
				{
					Value p1 = (Value) paramStack.Pop();
					Value p2 = (Value) paramStack.Pop();
					string s1  = CastString(p1);
					int i = CastInt(p2) - 1;
					int j = 1;
					if (function.Parameters.Count == 3)
                    {
                        Value p3 = (Value)paramStack.Pop();
                        j = CastInt(p3);
                    }
					string s2 = s1.Mid(i, j);

					return new Value(s2);
				}
				case Token.RGB:
				{
					Value p1 = (Value) paramStack.Pop();
					Value p2 = (Value) paramStack.Pop();
					Value p3 = (Value) paramStack.Pop();
					int n1 = CastInt(p1);
					int n2 = CastInt(p2);
					int n3 = CastInt(p3);
					
					//return new Value(Color.FromArgb(n1, n2, n3).ToArgb()); //not used because there is opacity, in c++ no 
					return    new Value( 
										Convert.ToInt32
											(
												(byte)(n3) | 
												Convert.ToInt16((byte)(n2))<<8 |
												Convert.ToInt32((byte)(n1))<<16
											)
										);

				}
				case Token.GETAPPTITLE:
				{
					Value v1 = (Value) paramStack.Pop();
					string ns	= CastString(v1);
					NameSpace aNameSpace = new NameSpace (ns, NameSpaceObjectType.Document);
					//NON ha la proprietà title quindi si puo' solo ritornare il nome NON localizzato
					return new Value(aNameSpace.Application);
				}	
				case Token.GETMODTITLE:
				{
					Value v1 = (Value) paramStack.Pop();
					string ns	= CastString(v1);
					NameSpace aNameSpace = new NameSpace (ns, NameSpaceObjectType.Document);
					IBaseModuleInfo baseModuleInfo = ReportSession.PathFinder.GetModuleInfoByName(aNameSpace.Application, aNameSpace.Module);
					return new Value(baseModuleInfo.Title);
				}	
				case Token.GETDOCTITLE:
				{
					Value v1 = (Value) paramStack.Pop();
					string ns	= CastString(v1);
					NameSpace documentNamespace = new NameSpace (ns, NameSpaceObjectType.Document);
					IDocumentInfo documentInfo = ReportSession.PathFinder.GetDocumentInfo(documentNamespace);
					return new Value(documentInfo.Title);
				}

                case Token.GETDATABASETYPE:
                {
                    return new Value(ReportSession.UserInfo.LoginManager.GetDatabaseType());
                }
                case Token.ISDATABASEUNICODE:
                {
                    return new Value(ReportSession.UserInfo.LoginManager.UseUnicode);
                }
                case Token.GETEDITION:
				{
					return new Value(ReportSession.UserInfo.LoginManager.GetEdition());
				}
				case Token.GETPRODUCTLANGUAGE:
				{
					return new Value(ReportSession.UserInfo.LoginManager.GetCountry());
				}
				case Token.GETCOMPUTERNAME:							 
				{
                    string name = System.Net.Dns.GetHostName();
                    
                    bool stripSpecial = true;

					if (function.Parameters.Count == 1)
                    {
                        Value v1 = (Value)paramStack.Pop();
                        if (v1.Data is bool)
                        {
                            stripSpecial = CastBool(v1);
                        }
                    }
                    if (stripSpecial)
                    {
                        for (int i = 0; i < name.Length; i++)
                            if (!char.IsLetterOrDigit(name[i]))
                                name = name.ReplaceAt(i, ' ');
                    }

					return new Value(name);
				}
				case Token.GETLOGINNAME:							 
				{
					return new Value(ReportSession.PathFinder.User);
				}
				case Token.GETUSERDESCRIPTION:
				{
					if (function.Parameters.Count == 1)
					{   
						Value v1 = (Value) paramStack.Pop();
						if (v1.Data is String)
						{
							string sid = CastString(v1);
							return new Value(ReportSession.UserInfo.LoginManager.GetUserDescriptionByName(sid));
						}
						else
						{
							int id = CastInt(v1);
							return new Value(ReportSession.UserInfo.LoginManager.GetUserDescriptionById(id));
						}
					}
				   return new Value(ReportSession.UserInfo.LoginManager.GetUserDescriptionById(ReportSession.UserInfo.LoginId));
				}
				case Token.GETWINDOWUSER:							 
				{
                        //Se servisse anche il Domain
                        //					string computerName		= System.Net.Dns.GetHostName();
                        //					string loginName		= System.Windows.Forms.SystemInformation.UserName;
                        //					string userDomainName	= System.Windows.Forms.SystemInformation.UserDomainName;
                        //					string fullLoginName	= userDomainName + Path.DirectorySeparatorChar + loginName;
                        //					if (userDomainName.Length == 0)
                        //						fullLoginName = computerName + Path.DirectorySeparatorChar + loginName;
                        //					return new Value(fullLoginName);

                        return new Value(System.Windows.Forms.SystemInformation.UserName);
                        
				}
				case Token.GETINSTALLATIONNAME:							 
				{
					return new Value(ReportSession.PathFinder.Installation);
				}
				case Token.GETINSTALLATIONPATH:
				{
					return new Value(ReportSession.PathFinder.GetInstallationPath());
				}
				case Token.GETINSTALLATIONVERSION:
				{
					if (ReportSession != null && ReportSession.PathFinder != null && ReportSession.UserInfo != null && ReportSession.UserInfo.LoginManager != null &&
						ReportSession.UserInfo.LoginManager.LoginManagerState == LoginManagerState.Logged)
					{
						return new Value(ReportSession.UserInfo.LoginManager.GetInstallationVersion());
					}
					return new Value("");
				}
				case Token.GETCOMPANYNAME:							 
				{
					return new Value(ReportSession.PathFinder.Company);
				}
				case Token.GETNEWGUID:							 
				{
					return new Value(Guid.NewGuid().ToString());
				}
				case Token.ISAUTOPRINT:
				{
					return new Value(false);
				}
                case Token.GETTITLE:
                {
                    paramStack.Pop(); 
                    return new Value("");
                }
                case Token.GETTHREADCONTEXT:
                {
                    paramStack.Pop(); paramStack.Pop();
                    return new Value(false);
                }
                case Token.OWNTHREADCONTEXT:
                {
                    paramStack.Pop(); 
                    return new Value(false);
                }

                case Token.GETREPORTNAME:
                {
                    string sName = ReportSession.ReportName;

                    Value v1 = (Value)paramStack.Pop();
                    if (v1.Data is bool)
                    {
                        bool stripExtension = CastBool(v1);
                        if (stripExtension)
                        {
                            int idx = sName.LastIndexOf('.');
                            if (idx > 0)
                            {
                                sName = sName.Left(idx);
                            }
                        }
                    }

                    return new Value(sName);
                }
                case Token.GETREPORTNAMESPACE:							 
				{
					return new Value(ReportSession.ReportNamespace);
				}
				case Token.GETREPORTMODULENAMESPACE:							 
				{
					string [] sa = ReportSession.ReportNamespace.Split(new Char[] {'.'});

					return new Value(sa[0] + "." + sa[1]);
				}
				case Token.GETREPORTPATH:							 
				{
					return new Value(ReportSession.ReportPath);
				}
				case Token.GETOWNERNAMESPACE:							 
				{
					//per forza nullo
					Value v1 = (Value) paramStack.Pop();
					return new Value("");
				}
				case Token.GETUPPERLIMIT:
				{
					Value v1 = (Value) paramStack.Pop();
					int len = CastInt(v1);

					string upper= "z";
					if (ReportSession != null && ReportSession.PathFinder != null && ReportSession.UserInfo != null && ReportSession.UserInfo.LoginManager != null)
					{
						upper = ReadSetting.GetMaxString(ReportSession.PathFinder, ReportSession.UserInfo.LoginManager.PreferredLanguage);
					}

                    string upperlimit;
					if (upper.Length != 0)
					    upperlimit = new string(upper[0], len); 
					else 
                        upperlimit = new string('z', len);
					
					return new Value(upperlimit);
				}	
				case Token.MAKEUPPERLIMIT:
				{
					Value v1 = (Value) paramStack.Pop();
					string s = CastString(v1);
					s = s.Trim();

					string upper = "z";
					if (ReportSession != null && ReportSession.PathFinder != null && ReportSession.UserInfo != null && ReportSession.UserInfo.LoginManager != null)
					{
						upper = ReadSetting.GetMaxString(ReportSession.PathFinder, ReportSession.UserInfo.LoginManager.PreferredLanguage);
					}

					while (s.EndsWith(upper))
						s = s.Substring(0, s.Length - 1);

					if (s.Length == 0)
						s = CoreTypeStrings.Ultimo;
					
					return new Value(s);
				}	
				case Token.MAKELOWERLIMIT:
				{ 
					Value v1 = (Value) paramStack.Pop();
					string s = CastString(v1);
					s = s.Trim();

					string upper = "z";
					if (ReportSession != null && ReportSession.PathFinder != null && ReportSession.UserInfo != null && ReportSession.UserInfo.LoginManager != null)
					{
						upper = ReadSetting.GetMaxString(ReportSession.PathFinder, ReportSession.UserInfo.LoginManager.PreferredLanguage);
					}

					if (s.Length == 0)
						s = CoreTypeStrings.Primo;
					
					return new Value(s);
				}

				case Token.ARRAY_ATTACH:
				{
					Value v1 = (Value) paramStack.Pop();
					DataArray ar = CastDataArray(v1);

					Value v2 = (Value) paramStack.Pop();
					DataArray ar2 = CastDataArray(v2);

					return new Value(ar.Attach(ar2));
				}
				case Token.ARRAY_DETACH:
				{
					Value v1 = (Value) paramStack.Pop();
					DataArray ar = CastDataArray(v1);

					ar.Detach();

					return new Value(true);	//it is a function, it must returns something
				}
				case Token.ARRAY_CLEAR:
				{
					Value v1 = (Value) paramStack.Pop();
					DataArray ar = CastDataArray(v1);
					ar.Clear();
					return new Value(0);	//it is a function, it must returns something
				}
				case Token.ARRAY_SIZE:
				{
					Value v1 = (Value) paramStack.Pop();
					DataArray ar = CastDataArray(v1);
					return new Value(ar.Count);
				}
                case Token.ARRAY_CONTAINS:
                {
                    Value v1 = (Value)paramStack.Pop();
                    DataArray ar = CastDataArray(v1);

                    Value v2 = (Value)paramStack.Pop();
 
                    int idx = ar.Find(v2.Data, 0);
                    return new Value(idx != -1);
                }
				case Token.ARRAY_FIND:
				{
					Value v1 = (Value) paramStack.Pop();
					DataArray ar = CastDataArray(v1);
					
					Value v2 = (Value) paramStack.Pop();
					int nStartSearch = 0;

					if (function.Parameters.Count == 3) //check here 
					 {
						Value v3 = (Value) paramStack.Pop();
						nStartSearch = Convert.ToInt32(CastLong(v3));  //v3.data e' un int64 per compatibilita con tipo long di woorm, ma il DataArray lavora su Int32
				     }
					int idx = ar.Find(v2.Data, nStartSearch);
					return new Value(idx);
				}
				case Token.ARRAY_SORT:
				{
					Value v1 = (Value) paramStack.Pop();
					DataArray ar = CastDataArray(v1);
					bool descending = false;
                    int start = 0;
                    int end = -1;
					if (function.Parameters.Count > 1)
					{
						Value v2 = (Value) paramStack.Pop();
						descending = CastBool(v2);

                        if (function.Parameters.Count > 2)
                        {
                            Value v3 = (Value)paramStack.Pop();
                            start = Convert.ToInt32(CastLong(v3));

                            if (function.Parameters.Count > 3)
                            {
                                Value v4 = (Value)paramStack.Pop();
                                end = Convert.ToInt32(CastLong(v4));
                            }
                        }
                     }
                     return new Value (ar.Sort(descending, start, end));
				}
				case Token.ARRAY_GETAT:
				{
					Value v1 = (Value) paramStack.Pop();
					DataArray ar = CastDataArray(v1);
					
                    Value v2 = (Value) paramStack.Pop();
					int idx = Convert.ToInt32(CastLong(v2)); //v2.data e' un int64 per compatibilita con tipo long di woorm, ma il DataArray lavora su Int32
					
					if (idx < 0 || idx >= ar.Count)
					{
						//TODO ErrHandler(UNKNOWN_FIELD, p2);
						break;
					}
					Object pO = ar.Elements[idx];
					if (pO == null)
					{
						//TODO ErrHandler(UNKNOWN_FIELD, p2);
						break;
					}
					return new Value(pO);
				}
				case Token.ARRAY_SETAT:
				{
					Value v1 = (Value) paramStack.Pop();
					DataArray ar = CastDataArray(v1);
					
					Value v2 = (Value) paramStack.Pop();
					int idx = Convert.ToInt32(CastLong(v2));//v2.data e' un int64 per compatibilita con tipo long di woorm, ma il DataArray lavora su Int32
					
					if (idx < 0)
					{
						//TODO ErrHandler(UNKNOWN_FIELD, p2);
						break;
					}
					
					Value v3 = (Value) paramStack.Pop();
					ar.SetAtGrow(idx, v3.Data);

					return new Value(v3.Data); //it is a function, it must return something, it returns inserted value
				}
                case Token.ARRAY_INSERT:
                {
                    Value v1 = (Value)paramStack.Pop();
                    DataArray ar = CastDataArray(v1);

                    Value v2 = (Value)paramStack.Pop();
                    int idx = Convert.ToInt32(CastLong(v2));//v2.data e' un int64 per compatibilita con tipo long di woorm, ma il DataArray lavora su Int32

                    if (idx < 0)
                    {
                        //TODO ErrHandler(UNKNOWN_FIELD, p2);
                        break;
                    }

                    Value v3 = (Value)paramStack.Pop();
                    ar.Insert(idx, v3.Data);

                    return new Value(true); //it is a function, it must return something, it returns inserted value
                }
                case Token.ARRAY_REMOVE:
                {
                    Value v1 = (Value)paramStack.Pop();
                    DataArray ar = CastDataArray(v1);

                    Value v2 = (Value)paramStack.Pop();
                    int idx = Convert.ToInt32(CastLong(v2));//v2.data e' un int64 per compatibilita con tipo long di woorm, ma il DataArray lavora su Int32

                    if (idx < 0)
                    {
                        //TODO ErrHandler(UNKNOWN_FIELD, p2);
                        break;
                    }

                    return new Value(ar.Remove(idx)); //it is a function, it must return something, it returns inserted value
                }
                case Token.ARRAY_COPY:
                {
                    Value v1 = (Value)paramStack.Pop();
                    DataArray ar = CastDataArray(v1);

                    Value v2 = (Value)paramStack.Pop();
                    DataArray ar2 = CastDataArray(v2);

                    return new Value(ar.Copy(ar2));
                }
                case Token.ARRAY_APPEND:
                {
                    Value v1 = (Value)paramStack.Pop();
                    DataArray ar = CastDataArray(v1);

                    Value v2 = (Value)paramStack.Pop();
                    DataArray ar2 = CastDataArray(v2);

                    return new Value(ar.Append(ar2));
                }
                case Token.ARRAY_ADD:
                {
                    Value v1 = (Value)paramStack.Pop();
                    DataArray ar = CastDataArray(v1);

                    Value v2 = (Value)paramStack.Pop();

                    return new Value(ar.Add(v2.Data) >= 0);
                }
                case Token.ARRAY_CREATE:
                {
                    Value v1 = (Value)paramStack.Pop();
                    string baseDt = v1.DataType;
                    DataArray  ar = new DataArray(baseDt);

                    ar.Add(v1.Data);

                    for (int np = 1; np < function.CurrentParametersCount; np++)
                    {
                        Value v2 = (Value)paramStack.Pop();

                        if (!ObjectHelper.CheckType(v2.Data, v1.Data))
                        {
                            //TODO ErrHandler(FORMAT_TYPEERR, itemFun);
                            return null;
                        }

                        ar.Add(v2.Data);
                    }
                    return new Value(ar);
                }

                case Token.ARRAY_SUM:
                {
                    Value v1 = (Value)paramStack.Pop();
                    DataArray ar = CastDataArray(v1);

                    return new Value(ar.CalcSum());
                }
                      
                case Token.DECODE:
                {
                    Value v1 = (Value)paramStack.Pop();
                    //string baseDt = v1.DataType;
                    Value ret = null;
                    int np = 1;
                    while (np < (function.CurrentParametersCount - 1))
                    {
                        Value v2 = (Value)paramStack.Pop();
                        Value v3 = (Value)paramStack.Pop();
                        np += 2;

                        if (!ObjectHelper.CheckType(v2.Data, v1.Data))
                        {
                            //TODO ErrHandler(FORMAT_TYPEERR, itemFun);
                            return null;
                        }

                        if (v1 == v2)
                        {
                            ret = new Value(v3.Data);
                            break;
                        }
                    }

                    //ha trovato la corrispondenza
                    if (ret != null && np < function.CurrentParametersCount)
                    {
                        for (; np < function.CurrentParametersCount; np++)
                            paramStack.Pop();
                    }

                    //non ha trovato la corrispondenza, tenta il default (ultimo parametro)
                    if (ret == null  && (np + 1) == function.CurrentParametersCount)
                    {
                        Value v3 = (Value)paramStack.Pop();
                        ret = new Value(v3.Data);
                    }

                   return ret;
                }

                case Token.IIF:
                    {
                        Value ret = null;
                        int np = 0;
                        while (np < (function.CurrentParametersCount - 2))
                        {
                            Value v1 = (Value)paramStack.Pop();
                            Value v2 = (Value)paramStack.Pop();
                            np += 2;

                            bool b = CastBool(v1);

                            if (b)
                            {
                                ret = new Value(v2.Data);
                                break;
                            }
                        }

                        //ha trovato la corrispondenza
                        if (ret != null && np < function.CurrentParametersCount)
                        {
                            for (; np < function.CurrentParametersCount; np++)
                                paramStack.Pop();
                        }

                        //non ha trovato la corrispondenza, tenta il default (ultimo parametro)
                        if (ret == null && (np + 1) == function.CurrentParametersCount)
                        {
                            Value v3 = (Value)paramStack.Pop();
                            ret = new Value(v3.Data);
                        }

                        return ret;
                    }
                case Token.CHOOSE:
                {
                    Value v1 = (Value)paramStack.Pop();
                    int index = CastInt(v1);

                    Value ret = null;
                        
                    if (index > 0 && index < function.CurrentParametersCount)
                        for (int np = 1; np < function.CurrentParametersCount; np++)
                        {
                            Value v2 = (Value)paramStack.Pop();
 
                            if (np == index)
                            {
                                ret = new Value(v2.Data); 
                            }
                        }

                    return ret;
                }

                case Token.TABLEEXISTS:
				{
					TBConnection connection = null;
					try
					{	
					    connection = new TBConnection(ReportSession.CompanyDbConnection, TBDatabaseType.GetDBMSType(ReportSession.Provider));
						connection.Open();
						TBDatabaseSchema mySchema = new TBDatabaseSchema(connection);
						Value v1 = (Value) paramStack.Pop();
						string tableName = CastString(v1);
						if (function.Parameters.Count == 2)
						{
							Value v2 = (Value) paramStack.Pop();	//Optional column name
							string columnName = CastString(v2);
							if (mySchema.ExistTable(tableName))
							{				
								DataTable cols = mySchema.GetTableSchema(tableName, false);
								foreach (DataRow col in cols.Rows)
								{
									if (string.Compare(col["ColumnName"].ToString(), columnName, true, CultureInfo.InvariantCulture) == 0)
										return new Value(true);	
								} 
								return new Value(false);					
							}
						}
						return new Value(mySchema.ExistTable(tableName)); 
					}
					catch (TBException e)
					{
                        Debug.Assert(false, e.Message);
						return new Value(false);
					}
					finally
					{
						connection.Close();
					}
				}
				case Token.GETSETTING:
				{
					Value v1 = (Value) paramStack.Pop();
					string nsSetting = CastString(v1);
					Value v2 = (Value) paramStack.Pop();
					string sSection = CastString(v2);
					Value v3 = (Value) paramStack.Pop();
					string sSetting = CastString(v3);
					Value v4 = (Value) paramStack.Pop();
                
				    object setting = null;
					if (ReportSession != null && ReportSession.PathFinder != null && ReportSession.UserInfo != null && ReportSession.UserInfo.LoginManager != null)
					{
						setting = ReadSetting.GetSettings(ReportSession.PathFinder, nsSetting, sSection, sSetting, v4.Data);
					} 
			
					return new Value(setting);
				}
                case Token.SETSETTING:
                {
                    Value v1 = (Value)paramStack.Pop();
                    string nsSetting = CastString(v1);
                    Value v2 = (Value)paramStack.Pop();
                    string sSection = CastString(v2);
                    Value v3 = (Value)paramStack.Pop();
                    string sSetting = CastString(v3);
                    Value v4 = (Value)paramStack.Pop();

                    object setting = null;
                    if (ReportSession != null && ReportSession.PathFinder != null && ReportSession.UserInfo != null && ReportSession.UserInfo.LoginManager != null)
                    {
                        setting = ReadSetting.GetSettings(ReportSession.PathFinder, nsSetting, sSection, sSetting, v4.Data);

                        //TODO

                    }

                    return new Value(setting);
                }
                case Token.VALUEOF:
				{
					Value v1 = (Value)paramStack.Pop();
					return new Value ( 
                        TBDatabaseType.DBNativeConvert (	
							v1.Data, 
							ReportSession.UserInfo.UseUnicode, 
							TBDatabaseType.GetDBMSType(ReportSession.Provider)
						)
					);
				}
				
                case Token.FILEEXISTS:  //M. 4196
                {
                    Value v1 = (Value)paramStack.Pop();
                    string sPath = CastString(v1);
                    return new Value(File.Exists(sPath));
                }
                case Token.SETCULTURE:  /*M. 4211*/
                {
                    Value v1 = (Value)paramStack.Pop();
                    string sUICulture = CastString(v1);

                    string sPrevUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
                    System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(sUICulture);
                    ReportSession.UICulture = sUICulture;
                    this.Localizer.Build(ReportSession.ReportPath, ReportSession.PathFinder);

                    return new Value(sPrevUICulture);
                }
                case Token.GETCULTURE:  //M. 4196
				{
					bool bUI = true;
					string sType = "user";

					if (function.Parameters.Count > 0)
                    {
                        Value v = (Value)paramStack.Pop();
                        sType = CastString(v).ToLower();
                    }
                    else
                    {
                        string s = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
                        return new Value(s);
                    }

					if (function.Parameters.Count == 2)
					{
						Value v = (Value)paramStack.Pop();	
						bUI = CastBool(v);
					}
					
                    if (bUI)
					{
						if (sType == "company")	
						{
							//pData->Assign(AfxGetLoginInfos()->m_strCompanyLanguage);
							if (ReportSession != null && ReportSession.PathFinder != null && ReportSession.UserInfo != null && ReportSession.UserInfo.LoginManager != null &&
						ReportSession.UserInfo.LoginManager.LoginManagerState == LoginManagerState.Logged)
							{
								string s = ReportSession.UserInfo.LoginManager.PreferredCompanyLanguage;
								return new Value(s);
							}
						}
						else if (sType == "server")
						{
							//pData->Assign(AfxGetCommonClientObjects()->GetServerConnectionInfo()->m_sPreferredLanguage);
							string s = InstallationData.ServerConnectionInfo.PreferredLanguage;
							return new Value(s);
						}
						else if (sType == "user" || sType == "login")
						{
							//pData->Assign(AfxGetLoginInfos()->m_strPreferredLanguage);
							if (ReportSession != null && ReportSession.PathFinder != null && ReportSession.UserInfo != null && ReportSession.UserInfo.LoginManager != null &&
						ReportSession.UserInfo.LoginManager.LoginManagerState == LoginManagerState.Logged)
							{
								string s = ReportSession.UserInfo.LoginManager.PreferredLanguage;
								return new Value(s);
							}
						}
					}
					else
					{
						if (sType == "company")	//TODO M.4196
						{
							//pData->Assign(AfxGetLoginInfos()->m_strCompanyApplicationLanguage);
							if (ReportSession != null && ReportSession.PathFinder != null && ReportSession.UserInfo != null && ReportSession.UserInfo.LoginManager != null &&
						ReportSession.UserInfo.LoginManager.LoginManagerState == LoginManagerState.Logged)
							{
								string s = ReportSession.UserInfo.LoginManager.ApplicationCompanyLanguage;
								return new Value(s);
							}
						}
						else if (sType == "server")
						{
							//pData->Assign(AfxGetCommonClientObjects()->GetServerConnectionInfo()->m_sApplicationLanguage);
							string s = InstallationData.ServerConnectionInfo.ApplicationLanguage;
							return new Value(s);
						}
						else if (sType == "user" || sType == "login")
						{
							//pData->Assign(AfxGetLoginInfos()->m_strApplicationLanguage);
							if (ReportSession != null && ReportSession.PathFinder != null && ReportSession.UserInfo != null && ReportSession.UserInfo.LoginManager != null &&
						ReportSession.UserInfo.LoginManager.LoginManagerState == LoginManagerState.Logged)
							{
								string s = ReportSession.UserInfo.LoginManager.ApplicationLanguage;
								return new Value(s);
							}
						}
					}
					return new Value("");
				}

                case Token.GETNSFROMPATH:   //M. 4196
                {
                    Value v1 = (Value)paramStack.Pop();
                    string sPath = CastString(v1);

					Microarea.TaskBuilderNet.Interfaces.INameSpace ns = ReportSession.PathFinder.GetNamespaceFromPath(sPath);
					if (ns != null && ns.IsValid())
						return new Value(ns.FullNameSpace);

					return new Value("");
                }
                case Token.GETPATHFROMNS:   //M. 4196
                {
                    Value v1 = (Value)paramStack.Pop();
                    string sNs = CastString(v1);
					NameSpace ns = new NameSpace(sNs);

					string sType = "auto";
					if (function.Parameters.Count == 2)
					{
						Value v = (Value)paramStack.Pop();
						sType = CastString(v).ToLower();
					}

					if (sType == "auto")
					{
						String sPath = ReportSession.PathFinder.GetFilename(ns, string.Empty);
						if (sPath != null)
							return new Value(sPath);
					}
					//TODO M.4196
					else if (sType == "user" || sType == "login")
					{
						String sPath = ReportSession.PathFinder.GetFilename(ns, string.Empty);
						if (sPath != null)
							return new Value(sPath);
					}
					else if (sType == "company")
					{
					}
					else if (sType == "standard")
					{
					}
					return new Value("");
                }

                case Token.DateAdd:
                {
                    Value vda = (Value)paramStack.Pop();
                    DateTime dt = CastDateTime(vda);

                    Value vd = (Value)paramStack.Pop();
                    Value vM = (Value)paramStack.Pop();
                    Value vy = (Value)paramStack.Pop();
                    dt.AddYears(CastInt(vy));
                    dt.AddMonths(CastInt(vM));
                    dt.AddDays(CastInt(vd));

                    if (function.Parameters.Count > 4)
                    {
                        Value vh = (Value)paramStack.Pop();
                        dt.AddHours(CastInt(vh));

                        if (function.Parameters.Count > 5)
                        {
                            Value vm = (Value)paramStack.Pop();
                            dt.AddMinutes(CastInt(vm));

                            if (function.Parameters.Count > 6)
                            {
                                Value vs = (Value)paramStack.Pop();
                                dt.AddSeconds(CastInt(vs));
                            }
                        }
                    }
                    return new Value(dt);
                }

                case Token.WeekStartDate:
                {
                    Value vy = (Value)paramStack.Pop();
                    Value vw = (Value)paramStack.Pop();

                    int year = CastInt(vy);
                    int week = CastInt(vw);
                    
                     return new Value((new DateTime(year, 1, 1)).WeekStartDate(week));
                }

                case Token.IsLeapYear:
                {
                    Value vy = (Value)paramStack.Pop();
 
                    int year = CastInt(vy);
 
                    return new Value(DateTime.IsLeapYear(year));
                }

                case Token.EasterSunday:
                {
                    Value vy = (Value)paramStack.Pop();

                    int year = CastInt(vy);

                    return new Value((new DateTime(year,1,1)).EasterSunday());
                }

                case Token.FormatTbLink:
                {
                    paramStack.Pop();
                    paramStack.Pop();
                    paramStack.Pop();

                    //TODO
                    return new Value(string.Empty);
                }
                case Token.SendBalloon:
                {
                    paramStack.Pop();
                    paramStack.Pop();
                    paramStack.Pop();
                    paramStack.Pop();
                    paramStack.Pop();
                    paramStack.Pop();
                    paramStack.Pop();

                    //TODO
                    return new Value(false);
                }

                //TODO funzioni di scripting avanzate, già implementate in C++, post 3.1
				case Token.TYPEOF:
				{
					Value v1 = (Value)paramStack.Pop();

					//TODO

					return new Value(string.Empty);
				}
				case Token.ADDRESSOF:
				{
					Value v1 = (Value)paramStack.Pop();
					Value v2 = (Value)paramStack.Pop();

					//TODO

					return new Value(0);
				}
				case Token.CONVERT:
				{
					Value v1 = (Value)paramStack.Pop();
					Value v2 = (Value)paramStack.Pop();

					//TODO

					return new Value(0);
				}
				case Token.EXECUTESCRIPT:
				{
					Value v1 = (Value)paramStack.Pop();
                    String script = CastString(v1);
                    TbScript tbScript = new TbScript (ReportSession, this.SymbolTable, new SilentMessageProvider());
					Parser lex = new Parser (Parser.SourceType.FromString);
                    if (!lex.Open(script))
                    {

                        return new Value(false);
                    }
                    if (!tbScript.Parse(lex))
                    {
                        return new Value(false);
                    }
                    if (!tbScript.Exec())
                    {
                        return new Value(false);
                    }
 					return new Value(true);
				}
            } // switch

            if (function.HasLateBinding && function.Prototype == null)
            {
                string handleName = string.Empty;
                FunctionPrototype fp = this.symbolTable.ResolveCallMethod(function.Name, out handleName) as FunctionPrototype;
                if (fp != null)
                {
                    paramStack.Push(new Variable(handleName));
                    function.Prototype = fp;
                }
            }
			// prova a vedere se una derivazione specializzata della expression implementa localmente la funzione
			Value specializedValue = ApplySpecializedFunction(function, paramStack);
			if (specializedValue != null)
				return specializedValue;

			// allora potrebbe essere una funzione esterna
			return ApplyExternalFunction(function, paramStack);
		}

		//-----------------------------------------------------------------------------
		virtual public Value ApplySpecializedFunction(FunctionItem function, Stack paramStack)
		{
            //ridefinita in WoormExpression (Microarea.TaskBuilderNet.Woorm.WoormEngine, Actions.cs)
			return null;
		}
		
		//-----------------------------------------------------------------------------
		Value ApplyExternalFunction(FunctionItem function, Stack paramStack)
		{
			try
			{
				if (function is ThiscallFunctionItem)
					return ApplyThisCallFunction((ThiscallFunctionItem)function, paramStack);

				// non devo eseguirla perchè non è supportata in WebMode
                if (ExpressionParser.IsNoWebFunction(function.Name))
                {
                    if (function.ReturnType == "Boolean")
                        return new Value(true);
                    else if (function.ReturnType == "String")
                        return new Value(string.Empty);
                    else if (function.ReturnType == "Double")
                        return new Value(0.0);
                    else if (function.ReturnType == "DateTime")
                        return new Value(DateTime.Today);
                    else if ((function.ReturnType == "Enum" || function.ReturnType == "DataEnum") && !function.ReturnBaseType.IsNullOrEmpty())
                        return new Value(new DataEnum(ushort.Parse(function.ReturnBaseType), 0));
                    else //if (function.ReturnType == "Int64")
                        return new Value(0);
                }

                System.Diagnostics.Debug.Assert(function.Parameters != null);

				List<object> parms = new List<object>();
				foreach (DataItem item in paramStack)
					parms.Add(WcfTypes.To(item.Data));

				object[] objs = parms.ToArray();
				ITbLoaderClient tbLoader = GetTBClientInterface();
				object ret = tbLoader.Call(function.Prototype, objs);

				for (int i = 0; i < function.Parameters.Count; i++)
				{
					DataItem item = (DataItem)paramStack.Pop();
					Parameter p = function.Parameters[i];
					if (p.Mode != ParameterModeType.In)
						item.Data = WcfTypes.From(objs[i], p.Type, p.BaseType);
				}
				return new Value(WcfTypes.From(ret, function.ReturnType, function.ReturnBaseType));
			}
			catch (SoapClientException e)
			{
				SetError(string.Format(ExpressionManagerStrings.SoapCallError, function.Name));
				SetError(e.Message);
				return new Value(null);
			}
			catch (TbLoaderClientInterfaceException e)
			{ 
				SetError(e.Message);
				return new Value(null);
			}
		}

		//-----------------------------------------------------------------------------
		private ITbLoaderClient GetTBClientInterface()
		{
			
			return reportSession.GetTBClientInterface();
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// funzione su oggetto (managed)
		/// </summary>
		/// <param name="function"></param>
		/// <param name="paramStack"></param>
		/// <returns></returns>
		private Value ApplyThisCallFunction(ThiscallFunctionItem function, Stack paramStack)
		{
			Variable v = symbolTable.Find(function.ObjectName);
			if (v == null)
			{
				SetError(string.Format(ExpressionManagerStrings.UnknownField, function.ObjectName));
				return null;
			}

			object[] parms = new object[paramStack.Count];
			Type[] types = new Type[paramStack.Count];
			int i = 0;
			foreach (DataItem item in paramStack)
			{
				parms[i] = item.Data;
				types[i] = item.Data.GetType();
				i++;
			}
			MethodInfo mi = ObjectHelper.GetMethod(v.Data, function.Name, types);
			if (mi == null)
			{
				SetError(string.Format(ExpressionManagerStrings.UnknownFunction, function.ObjectName + "." + function.Name));
				return null;
			}
			return new Value(mi.Invoke(v.Data, parms));
		}

		//-----------------------------------------------------------------------------
		virtual protected Variable ResolveSymbolByReference(Variable var, bool allowVariableNotInSymbolTable)
		{
			if (symbolTable == null)
			{
				SetError(ExpressionManagerStrings.NoSymbolTable);
				return null;
			}

			Variable foundVar = symbolTable.Find(var.Name);
			if (foundVar == null)
			{
				if (!allowVariableNotInSymbolTable)
					SetError(string.Format(ExpressionManagerStrings.UnknownField, var.Name));
				return null;
			}

			/* PROFILE : il controllo costa troppo
			// se si referenzia la variabile predefinita OwnerID il suo valore deve essere valido
			// ossia deve contenere un puntatore (Int32) diverso da zero.
			if (
				string.Compare(var.Name, Language.GetTokenString(Token.OWNER_ID), true, CultureInfo.InvariantCulture) == 0 &&
				ObjectHelper.CastInt(var.Data) == 0
				)
			{
				SetError(ExpressionManagerStrings.InvalidOwnerID);
				return null;
			}
			*/
			return foundVar;
		}

		//-----------------------------------------------------------------------------
		virtual protected Variable ResolveSymbolByReference(Variable var)
		{
			return ResolveSymbolByReference(var, false);
		}

		//-----------------------------------------------------------------------------
		virtual protected Value ResolveSymbol(Variable var)
		{
			Variable foundVar = ResolveSymbolByReference(var);
			if (foundVar == null)
				return null;

            object data = foundVar.Data;

            if (!(data is DataEnum) && !(data is DataArray))
            {
                IDataObj d = data as IDataObj;
                if (d != null)
                {

                    data = d.Value;
                }
            }

            Value v = new Value(data);
			v.Valid = foundVar.Valid;
			return v;
		}

		// nel caso si stia eseguendo una espressione contenuta in un parametro di funzione
		// bisogna fare in modo che durante la valutazione se abbiamo a che fare con una Variable
		// dobbiamo rimettere nello stack una Variable se il parametro è [Out] o [In Out] per
		// poter valorizzare i dati del chiamante (passaggio non per valore)
		//-----------------------------------------------------------------------------
		bool Execute(Stack workStack, ParameterModeType direction = ParameterModeType.In)
		{
            if (Error)
            {
                return false;
            }

			// gli item vengono "pop-ati" ma rimangono inseriti nello stack
			// originale che li possiede e li cancellera` nel distruttore
			object o = workStack.Peek();
			if (o is OperatorItem)
			{
				Value res = null;
				OperatorItem itemOpe = (OperatorItem) workStack.Pop();
				if (!Execute(workStack)) 
                    return false;

				Value v1 = (Value) workStack.Pop();

				switch (itemOpe.GetOperatorType())
				{
					case OperatorItem.OperatorType.Logical:
					case OperatorItem.OperatorType.Unary:
					{
						res = GiveMeResult(itemOpe, v1);
						break;
					}
						
					case OperatorItem.OperatorType.Binary	:
					{
						if (!Execute(workStack))
							return false;
						
						Value v2 = (Value) workStack.Pop();
						res = GiveMeResult(itemOpe, v2, v1);
						break;
					}
					
					case OperatorItem.OperatorType.Ternary:
					{
						if (!Execute(workStack))
							return false;

						Value v2 = (Value) workStack.Pop();

						if (!Execute(workStack))
							return false;

						Value v3 = (Value) workStack.Pop();
						res = GiveMeResult(itemOpe, v3, v2, v1);
						break;
					}
				}

				if (res == null) 
                    return false;

				workStack.Push(res);
				return true;
			}

			if (o is FunctionItem)
			{
				return ExecuteFunction(workStack);
			}

			if (o is Variable)
			{
				Variable var = (Variable) workStack.Pop();

				// é un passaggio by reference e quindi metto l'intera Variabile
				if (direction != ParameterModeType.In)
				{
					Variable res = ResolveSymbolByReference(var);
					if (res == null) 
                        return false;
					workStack.Push(res);
				}
				else
				{
					Value res = ResolveSymbol(var);
					if (res == null || res.Data == null) 
                        return false;
					workStack.Push(res);
				}
				return true;
			}
			return true;
		}

		//-----------------------------------------------------------------------------
		private bool ExecuteFunction(Stack workStack)
		{
			FunctionItem function = (FunctionItem)workStack.Pop();
			Stack paramStack = new Stack();

            //System.Diagnostics.Debug.Assert(function.Parameters != null);
            if (function.Parameters != null)
            {
                for (int p = function.CurrentParametersCount - 1; p >= 0; p--)
                {
                    // controllo il tipo di passaggio di parametro (Value o Reference)
                    ParameterModeType mode = p < function.Parameters.Count
                                                ? function.Parameters[p].Mode
                                                : ParameterModeType.In;
                       
                    if (!Execute(workStack, mode))
                        return false;

                    object opar = workStack.Pop();

                    //forse non serve, da debuggare
                    //if (p == 0 && (function.Name.CompareNoCase("IsNull") || function.Name.CompareNoCase("IsEmpty")))
                    //{
                    //}

                    paramStack.Push(opar);
                }
            }
            else if (function.HasLateBinding && function.CurrentParametersCount > 0)
            {
                for (int p = function.CurrentParametersCount; p > 0; p--)
                {
                    if (!Execute(workStack))
                        return false;

                    paramStack.Push(workStack.Pop());
                }
            }

			Value res = ApplyFunction(function, paramStack);
            if (res == null)
            {
                Debug.Assert(false, "Expression.ExecuteFunction");

                return false;
            }
			workStack.Push(res);
			
			return true;
		}

		//-----------------------------------------------------------------------------
        bool Analyze(Stack workStack, Stack outStack, ParameterModeType direction = ParameterModeType.In)
		{
			object o = workStack.Peek();
			if (Error || o == null)
				return false;

			if (o is Value)
			{
				Value	res = o as Value;
				outStack.Push(res.Clone());
				return true;
			}

			if (o is OperatorItem)
			{
				Value res = null;

				OperatorItem itemOpe = (OperatorItem) workStack.Pop();
				outStack.Push(itemOpe);

				if (!Analyze(workStack, outStack))
					return false;

				Value v1 = (Value) workStack.Pop();
				switch (itemOpe.GetOperatorType())
				{
					case OperatorItem.OperatorType.Logical	:
					case OperatorItem.OperatorType.Unary	:
					{
						res = CheckType(itemOpe, v1);
						break;
					}
						
					case OperatorItem.OperatorType.Binary	:
					{
						if (!Analyze(workStack, outStack))
							return false;
						
						Value v2 = (Value) workStack.Pop();
		
						res = CheckType(itemOpe, v2, v1);
						break;
					}
					
					case OperatorItem.OperatorType.Ternary:
					{
						if (!Analyze(workStack, outStack))
							return false;

						Value v2 = (Value) workStack.Pop();

						if (!Analyze(workStack, outStack))
							return false;

						Value v3 = (Value) workStack.Pop();

						res = CheckType(itemOpe, v3, v2, v1);
						break;
					}
				}

				if (res == null) 
                    return false;
				workStack.Push(res);
				return true;
			}

			if (o is FunctionItem)
			{
				Value	res = null;

				FunctionItem function = (FunctionItem) workStack.Pop();
				outStack.Push(function);

                Token tkFun = Language.GetKeywordsToken(function.Name);

				string returnType = function.ReturnType;
				string variantType = string.Empty;

                if (function.Parameters != null)
                {
                    for (int p = function.CurrentParametersCount - 1; p >= 0; p--)
                    {
                        DataItem di = null;
                        if (p < function.Parameters.Count)
                        {
                            // controllo il tipo di passaggio di parametro (Value o Reference)
                            ParameterModeType mode = function.Parameters[p].Mode;

                            if (!Analyze(workStack, outStack, mode))
                                return false;
                            di = (DataItem)workStack.Pop();

                            string parameterType = function.Parameters[p].Type;

                            // se il tipo di parametro è null allora devo evitare il controllo di compatibilità (es: format)
                            if (parameterType != null && !ObjectHelper.Compatible(di.DataType, parameterType))
                            {
                                SetError(string.Format(ExpressionManagerStrings.InvalidParameter, function.FullName));
                                return false;
                            }
                        }
                        else 
                        {
                            if (!Analyze(workStack, outStack))
                                return false;
                            di = (DataItem)workStack.Pop();
                        }

                        //---- check return Variant type 
                        if
                            (
                                p == 0 &&
                                di.Data != null &&
                                di.Data is DataArray &&
                                (
                                    tkFun == Token.ARRAY_GETAT
                                    ||
                                    tkFun == Token.ARRAY_SETAT
                                    ||
                                    tkFun == Token.ARRAY_REMOVE
                                )
                            )
                        {
                            returnType = ((DataArray)(di.Data)).BaseType;
                        }
                        else if
                            (
                                p == 3 &&
                                di.Data != null &&
                                (
                                    tkFun == Token.GETSETTING ||
                                    tkFun == Token.SETSETTING
                                )
                            )
                        {
                            returnType = di.Data.GetType().Name;
                        }
                        else if
                            (
                                p == 1 &&
                                di.Data != null &&
                                (
                                    tkFun == Token.ARRAY_FIND
                                    ||
                                    tkFun == Token.ARRAY_ADD
                                )
                            )
                        {
                            variantType = di.Data.GetType().Name;
                        }
                        else if
                            (
                                p == 2 &&
                                di.Data != null
                            )
                        {
                            if
                            (
                                tkFun == Token.ARRAY_SETAT
                                ||
                                tkFun == Token.ARRAY_INSERT
                            )
                                variantType = di.Data.GetType().Name;
                            else if (tkFun == Token.DECODE)
                                returnType = di.Data.GetType().Name;
                        }

                        //---- check parameter Variant type 
                        if
                            (
                                p == 0 &&
                                di.Data != null
                            )
                        {
                            if (di.Data is DataArray)
                            {
                                if (tkFun == Token.ARRAY_SETAT)
                                {
                                    if (!ObjectHelper.Compatible(variantType, returnType))
                                    {
                                        SetError(string.Format(ExpressionManagerStrings.InvalidParameter, function.FullName));
                                        return false;
                                    }
                                }
                                else if (
                                    tkFun == Token.ARRAY_FIND ||
                                    tkFun == Token.ARRAY_INSERT ||
                                    tkFun == Token.ARRAY_ADD
                                   )
                                {
                                    if (!ObjectHelper.Compatible(variantType, ((DataArray)(di.Data)).BaseType))
                                    {
                                        SetError(string.Format(ExpressionManagerStrings.InvalidParameter, function.FullName));
                                        return false;
                                    }
                                }
                                else if (tkFun == Token.ARRAY_REMOVE)
                                {
                                    returnType = ((DataArray)(di.Data)).BaseType;
                                }
                            }
                            else if (tkFun == Token.MIN || tkFun == Token.MAX)
                            {
                                returnType = di.Data.GetType().Name;
                            }
                        }
                    }
                }
                else if (function.HasLateBinding && function.CurrentParametersCount > 0)
                {
                    for (int p = function.CurrentParametersCount; p > 0; p--)
                    {
                        if (!Analyze(workStack, outStack))
                            return false;
                     }
                }

				res = new Value();
                if (returnType == "Variant")
                {
                    res.Data = null;
                    res.IsVariant = true;
                }
                else
				    res.Data = ObjectHelper.CreateObject(returnType);

				workStack.Push(res);
				return true;
			}

			if (o is Variable)
			{
				Variable var = (Variable) workStack.Pop();
				outStack.Push(var);

				// é un passaggio by reference e quindi metto l'intera Variabile
				if (direction != ParameterModeType.In)
				{
					Variable res = ResolveSymbolByReference(var);
					if (res == null) 
                        return false;
					workStack.Push(res);
				}
				else
				{
					Value res = ResolveSymbol(var);
					if (res == null) 
                        return false;
					workStack.Push(res);
				}
				return true;
			}

			return false;
		}

		/// <summary>
		///	Metodo utilizzato per imitare il comportamento della _tstof usata da woorm c++ per estrarre un 
		///	numero da una stringa.
		///	Esempi:
		///	"10" -> "10"
		/// "100%"-> "100"
		/// "gh,2g5" -> "2"
		/// "4.5" -> "4.5"
		/// "4,5" -> "4"
		/// "16,521" -> "16"
		/// </summary>
		//-----------------------------------------------------------------------------
		string RemoveNonDigitChar(string strValue)
		{
			bool digitFound = false;

			for (int i = 0; i < strValue.Length; i++)
			{
				char currentChar = strValue[i];
				if (!char.IsDigit(currentChar) && currentChar != '.')
				{
					if (digitFound)
						return strValue.Remove(i);
					strValue = strValue.Remove(i, 1);
					i--;
				}
				else
				{
					digitFound = true;
				}
			}
			return strValue;
		}

	}
}

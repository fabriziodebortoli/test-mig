using System.Collections;
using System.Collections.Generic;

using Microarea.Common.Applications;
using Microarea.Common.Lexan;
using Microarea.Common.ExpressionManager;
using Microarea.Common.CoreTypes;
using Microarea.Common.Hotlink;
using Microarea.Common;
using Microarea.Common.Generic;

namespace Microarea.RSWeb.WoormEngine
{

    /// <summary>
    /// GroupBy 
    ///		ha la sintassi di espressione di somma di segmenti il cui risultato finale
    ///		deve essere nua stringa per poter fare confronti di rottura. Esempio
    ///			Group By Citta+Provincia.
    ///		posso usare anche delle espressioni tipo:
    ///			Group By Citta + Format(titolo) + chr(anno)
    ///			
    ///		il group by è valido se le variabili presenti nella espressione dipendono
    ///		da rules nello stesso ordine in cui le rules vengono valutate. (OrderedRules)
    /// Esempio :
    ///				group by a+b+c+d
    ///				rule     i i j i
    /// non è valida perchè le rules di dipendenza delle singole variabili non sono
    /// nella sequenza di valutazione delle OrderedRules
    /// </summary>
    //============================================================================
    public class GroupBy : Expression
	{
		//---------------------------------------------------------------------------
		private ReportEngine		engine = null;
		private List<GroupFunction> groupFunctions = new List<GroupFunction>();
		private string				oldValue;
		private RuleObj				currentRule = null;

		//---------------------------------------------------------------------------
		public GroupBy (TbReportSession session, FieldSymbolTable symTable) : base (session, symTable) {}

		//---------------------------------------------------------------------------
		private bool Eval(out string result)
		{
			FieldSymbolTable st = (FieldSymbolTable)SymbolTable;

			engine.SetLevel(DataLevel.Rules);
			Value v = Eval();
			result = "";

			if (Error)
				engine.SetError(WoormEngineStrings.EvalGroupExpression);
			else
				result = (string) v.Data;

			engine.SetLevel(DataLevel.GroupBy);
			return !Error;
		}

		//---------------------------------------------------------------------------
		public bool Init (ReportEngine engine)
		{	
			// attach the owner
			this.engine = engine;
			return Eval(out oldValue);
		}

		//---------------------------------------------------------------------------
		public bool IsChanged ()
		{
			string newValue;
			if (Eval(out newValue) && (oldValue != newValue))
			{
				oldValue = newValue;
				return true;
			}

			return false;
		}

		//---------------------------------------------------------------------------
		public bool EvalFunction() { return EvalFunction(false); }
		public bool EvalFunction(bool onlyExpr)
		{
			foreach (GroupFunction function in groupFunctions)
			{
				if ((!onlyExpr || function.IsAnExpression) && !function.EvalFunction())
				{
					engine.SetError(WoormEngineStrings.EvalGroupFunction);
					return false;
				}
			}
			return true;
		}

		//---------------------------------------------------------------------------
		public bool Parse(Parser lex)
		{
			StopTokens = new StopTokens(new Token[] { Token.DO });

			if (!(
					Compile(lex, CheckResultType.Match, "String") &&
					CheckVariables(lex, expressionStack) &&
					lex.ParseTag(Token.DO)
				))
				return false;

			bool groupStatem = lex.Matched(Token.BEGIN);
		                      
			if (!lex.Error)
				do
				{
					GroupFunction groupFuction = new GroupFunction(TbSession as TbReportSession, SymbolTable as FieldSymbolTable);
					groupFunctions.Add(groupFuction);
					if (!groupFuction.Parse(lex)) 
						return false;
				}
				while (lex.ParseSep() && groupStatem && !lex.Matched(Token.END));

			return !lex.Error;
		}

		//-----------------------------------------------------------------------------
		public bool Unparse(Unparser unparser)
		{
			if (!this.IsEmpty && groupFunctions.Count > 0)
			{
				unparser.IncTab();
				unparser.WriteTag(Token.GROUP, false);
				unparser.WriteTag(Token.BY, false);
				unparser.WriteExpr(this.ToString(), false);
				unparser.WriteTag(Token.DO, false);

				if (groupFunctions.Count > 1)
				{
					unparser.WriteLine();
					unparser.WriteBegin();
					unparser.IncTab();
				}

				foreach (GroupFunction function in groupFunctions)
				{
					function.Unparse(unparser);
					unparser.WriteSep(false);

					if (function != groupFunctions[groupFunctions.Count - 1])
						unparser.WriteLine();
				}

				if (groupFunctions.Count > 1)
				{
					unparser.WriteLine();
					unparser.DecTab();
					unparser.WriteEnd();
				}
				unparser.WriteLine();
				unparser.DecTab();
			}
			return true;
		}

		//-----------------------------------------------------------------------------
		public bool CheckVariables(Parser lex, Stack<Item> stack)
		{
			Field	field;
			string	name;

			foreach (Item o in stack)
			{
				if (o is OperatorItem)
				{
					OperatorItem op = (OperatorItem) o;
					if	(
							!CheckVariables(lex, op.FirstStack) ||
							!CheckVariables(lex, op.SecondStack)
						)
						return false;

					continue;
				}

				if (o is Variable)
				{
					name = ((Variable) o).Name;
					field = SymbolTable.Find(name) as Field; 

					if (field == null)
					{
						lex.SetError(string.Format(ExpressionManagerStrings.UnknownField, name));
						return false;
					}
					if (!field.OwnRule)
					{
						lex.SetError(string.Format(WoormEngineStrings .IllegalField, name));
						return false;
					}
				}
			}

			return true;
		}

		//-----------------------------------------------------------------------------
		public bool CheckRuleGraph(ref ArrayList vectRules, Stack<Item> stack)
		{
			bool needTemp = false;
			Stack<Item> reversed = new Stack<Item>();
			
			// deve leggere dallo stack a partire dal l'elemento più in alto
			Utility.ReverseStack(stack, reversed);
			foreach (object o in reversed)
			{
				if (o is OperatorItem)
				{
					OperatorItem ope = (OperatorItem)o;
					// devo passare un clone dello stack della espressione perchè viene consumato
					if (CheckRuleGraph(ref vectRules, ope.FirstStack.Clone()))
						needTemp = true;

					// devo passare un clone dello stack della espressione perchè viene consumato
					if (CheckRuleGraph(ref vectRules, ope.SecondStack.Clone()))
						needTemp = true;

					continue;
				}

				if (o is Variable)
				{
					Variable v = (Variable)o;
					Field field = SymbolTable.Find(v.Name) as Field;

					if (currentRule != field.OwnerRule)
					{
						currentRule = field.OwnerRule;
				    
						// la regola e` gia` stata trovata?
						foreach (RuleObj rule in vectRules)
							if (currentRule == rule)
							{
								needTemp = true;
								break;
							}
					
						vectRules.Add(currentRule);
					}
				}
			}

			return needTemp;
		}

		// deve essere chiamata solo dopo aver fatto il calcolo delle Rule per determinare
		// se per fare il GroupBy deve utilizzare un file temporaneo (che adesso non è previsto)
		// in pratica si accettano GroupBy con variabili che provengono solo da una rule
		//-----------------------------------------------------------------------------
		public bool IsTemporaryRequired(ref ArrayList vectRules)
		{
			if (expressionStack.Count == 0) return false;
			currentRule = null;

			// costruisce un array di Rules referenziate dagli elementi nella GroupBy
			// devo passare un clone dello stack della espressione perchè viene consumato
			bool needTemp = CheckRuleGraph(ref vectRules, expressionStack.Clone());

			// una GroupBy di solo costanti non è ammessa
			return needTemp || vectRules.Count  == 0;
		}
	}
}

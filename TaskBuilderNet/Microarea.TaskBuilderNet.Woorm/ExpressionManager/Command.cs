//#define DEBUG_SCRIPT

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.Lexan;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;

namespace Microarea.TaskBuilderNet.Woorm.ExpressionManager
{
    //================================================================================
    public interface IMessageProvider
	{
		void Message(string message);
	}

	//================================================================================
	class SilentMessageProvider : IMessageProvider
	{
		Diagnostic d = new Diagnostic("SilentMessageProvider", true);//scrive nell'event viewer
		public void Message(string message)
		{
			d.SetInformation(message);
		}

	}
	//================================================================================
	class TBScriptExpression : Expression
	{
		//--------------------------------------------------------------------------------
		public TBScriptExpression(TbReportSession tbReportSession, SymbolTable symTable)
			: base(tbReportSession, symTable)
		{
		}
		//--------------------------------------------------------------------------------
		protected override ExpressionParser CreateParser()
		{
			ExpressionParser parser = new ExpressionParser(reportSession, symbolTable, stopTokens);
			parser.AllowThisCallMethods = true;
			return parser;
		}

	}
	//================================================================================
	public class Command : Object
	{
		public enum CommandState { NORMAL, RETURN, BREAK, CONTINUE, ABORT };

		protected TbScript scriptContext;

		public CommandState State = Command.CommandState.NORMAL;

#if  DEBUG_SCRIPT
		public int Level = 0;
#endif
		public int Index = 0;
		public string CommandString = string.Empty;
		public bool HasBreakpoint = false;
		public Expression ConditionalBreakpoint = null;
		//----
		protected SymbolTable symTable;
		//--------------------------------------------------------------------------------
		public Command(TbScript context, SymbolTable symTable)
		{
			this.scriptContext = context;
			this.symTable = symTable;
		}

		//--------------------------------------------------------------------------------
		public SymbolTable SymTable { get { return symTable; } }
		//--------------------------------------------------------------------------------
		public TbScript Context { get { return scriptContext; } }
		//--------------------------------------------------------------------------------
		public bool CanRun { get { return State == Command.CommandState.NORMAL; } }

		//--------------------------------------------------------------------------------
		public virtual bool Exec() { return false; }
		//--------------------------------------------------------------------------------
		public virtual bool Parse(Parser lex) { return false; }

		//--------------------------------------------------------------------------------
		public void EndParse(Parser lex)
		{
			CommandString = lex.GetAuditString();
#if DEBUG_SCRIPT
			this.Level = scriptContext.Level;
#endif
		}

		//--------------------------------------------------------------------------------
		public bool Fail()
		{
			return Fail(ExpressionManagerStrings.GenericScriptError);
		}

		//--------------------------------------------------------------------------------
		public bool Fail(string error)
		{
			State = Command.CommandState.ABORT;

			scriptContext.SetError(error);
			return false;
		}
	}

	//============================================================================
	public class CommandBlock : Command
	{
		protected ArrayList commands = new ArrayList();
		protected bool hasBeginEnd = false;

		//--------------------------------------------------------------------------------
		public CommandBlock(TbScript context, SymbolTable symTable)
			:
			base(context, null)
		{
			this.symTable = new DynamicSymbolTable(symTable);	//incremento di livello
		}

		//--------------------------------------------------------------------------------
		public bool IsEmpty { get { return commands.Count == 0; } }

		//--------------------------------------------------------------------------------
		public void Add(Command pCmd)
		{
			int nIdx = commands.Add(pCmd);
			pCmd.Index = nIdx;
#if DEBUG_SCRIPT
			scriptContext.AddCommand(pCmd);
#endif
		}

		//--------------------------------------------------------------------------------
		override public bool Exec()
		{
            this.State = Command.CommandState.NORMAL;
			for (int i = 0; i < commands.Count; i++)
			{
				Command cmd = (Command)commands[i];

				if (cmd.HasBreakpoint)
				{
					bool showDebug = true;
					if (cmd.ConditionalBreakpoint != null)
					{
						Value v = cmd.ConditionalBreakpoint.Eval();
						showDebug = v.Valid ? Expression.CastBool(v) : true;
					}
#if DEBUG_SCRIPT
					if (showDebug)
					{
						scriptContext.Paused = true;
					}
#endif
				}

				//TODO
				//CommandBlock block = cmd as CommandBlock;

				if (!cmd.Exec() || cmd.State == Command.CommandState.ABORT)
				{
					return Fail();
				}

				if (!cmd.CanRun)
				{
					this.State = cmd.State;
					break;
				}
			}
			return true;
		}

		//--------------------------------------------------------------------------------
		public Command ParseCommand(Parser parser)
		{
			parser.GetAuditString();

			Command cmd = Context.ParseExtendedCommands(parser);
			if (cmd != null)
				return cmd;

			Token actionToken = parser.LookAhead();
			switch (actionToken)
			{
				case Token.ID:
					{
						string name = parser.CurrentLexeme;/*GetCurrentStringToken()*/
						Variable pField = SymTable.Find(name);
						if (pField == null)
							cmd = new ExprCommand(scriptContext, this.SymTable);
						else
							cmd = new AssignCommand(scriptContext, this.SymTable);

						if (cmd.Parse(parser))
							return cmd;
						break;
					}

				case Token.IF:
					{
						parser.SkipToken();
						cmd = new ConditionalCommand(scriptContext, this.SymTable);
						if (cmd.Parse(parser))
							return cmd;
						break;
					}

				case Token.WHILE:
					{
						parser.SkipToken();
						cmd = new WhileCommand(scriptContext, this.SymTable);
						if (cmd.Parse(parser))
							return cmd;
						break;
					}

				case Token.BREAK:
					{
						parser.SkipToken();
						cmd = new BreakCommand(scriptContext, this.SymTable);
						if (cmd.Parse(parser))
							return cmd;
						break;
					}
				case Token.CONTINUE:
					{
						parser.SkipToken();
						cmd = new ContinueCommand(scriptContext, this.SymTable);
						if (cmd.Parse(parser))
							return cmd;
						break;
					}

				case Token.RETURN:
					{
						parser.SkipToken();
						cmd = new ReturnCommand(scriptContext, this.SymTable);
						if (cmd.Parse(parser))
							return cmd;
						break;
					}
				case Token.MESSAGE:
					//case Token.ABORT:
					{
						parser.SkipToken();
						cmd = new MessageCommand(scriptContext, this.SymTable);
						if (cmd.Parse(parser))
							return cmd;
						break;
					}

				default:
					{
						cmd = new DeclareCommand(scriptContext, this.SymTable);
						if (cmd.Parse(parser))
							return cmd;
						break;
					}
			}
			// exit with error
			return null;
		}

		//--------------------------------------------------------------------------------
		override public bool Parse(Parser parser)
		{
			Command cmd;

			if (parser.Parsed(Token.BEGIN))
			{
				// also accepts empty "begin..end" block sections
				//
				while (!parser.Parsed(Token.END))
				{
					cmd = ParseCommand(parser);
					if (cmd == null)
						return false;

					cmd.EndParse(parser);
					Add(cmd);
				}

				hasBeginEnd = true;
			}
			else
			{
				// single action
				//
				cmd = ParseCommand(parser);
				if (cmd == null)
					return false;

				cmd.EndParse(parser);
				Add(cmd);

				hasBeginEnd = false;
			}
			if (!parser.ParseTag(Token.EOF))
				return false;
			
			if (parser.Error)
			{ 
				parser.SetError("generic error");
				return false;
			}

			return true;
		}
	}

	//============================================================================
	class AssignCommand : Command
	{
		protected Variable field = null;
		protected Expression localExpr = null;
		protected Expression indexerExpr = null;

		//--------------------------------------------------------------------------------
		public AssignCommand(TbScript context, SymbolTable symTable)
			: base(context, symTable)
		{
		}

		//--------------------------------------------------------------------------------
		override public bool Exec()
		{
			if (localExpr == null)
				return Fail(); //TODO EMIT ERROR

			Value v = localExpr.Eval();
			if (localExpr.Error)
			{
				return Fail();
			}

			if (field.DataType == "Array")
			{
				if (indexerExpr != null)
				{
					Value vIdx = indexerExpr.Eval();
					if (indexerExpr.Error)
					{
						return Fail();
					}

					//// se sono di una rule devo solo gestire eventData
					//if (m_pField.OwnRule)
					//{
					//    m_pField.AssignArrayEventData(v.Data, v.Valid && vIdx.Valid, vIdx.Data);
					//    m_pField.EventDataUpdated = true;
					//    return true;
					//}

					// devo aggiornare tutti gli "xxData" perchè posso utilizzarli anche
					// in altri contesti con il DataLevel diverso da event. Es: rule
					field.AssignArrayData(v, vIdx);
				}
				else
				{
					if (v.Data is DataArray)
						field.Data = v.Data;
				}
			}
			else
			{
				if (indexerExpr != null)	//m_pField.DataType == "String"
				{
					Value vIdx = indexerExpr.Eval();
					if (indexerExpr.Error)
					{
						return Fail();
					}

					string prevData = field.Data as string;
					string setChar = v.Data as string;
					if (prevData == null || setChar == null || !(vIdx.Data is int))
					{
						return Fail();
					}
					int idx = (int)(vIdx.Data);
					idx -= 1; //in woorm le stringhe sono 1-based
					if (setChar != null && setChar.Length > 0)
					{
						if (idx < prevData.Length)
							prevData = prevData.Substring(0, idx) + setChar.Substring(0, 1) + prevData.Substring(idx + 1);
						else
						{
							prevData += setChar.Substring(0, 1);
						}
						v.Data = prevData;
					}
				}

				//// se sono di una rule devo solo gestire eventData
				//if (m_pField.OwnRule)
				//{
				//    m_pField.AssignEventData(v.Data, v.Valid);
				//    m_pField.EventDataUpdated = true;

				//    return true;
				//}

				// devo aggiornare tutti gli "xxData" perchè posso utilizzarli anche
				// in altri contesti con il DataLevel diverso da event. Es: rule
				field.Data = v.Data;
			}
			return true;
		}

		//--------------------------------------------------------------------------------
		override public bool Parse(Parser parser)
		{
			string fieldName;
			if (!parser.ParseID(out fieldName))
				return false;

			field = SymTable.Find(fieldName);
			if (field == null)
			{
				parser.SetError(string.Format(ExpressionManagerStrings.UnknownField, fieldName));
				return false;
			}

			if (field.DataType == "Array" || (parser.LookAhead(Token.SQUAREOPEN) && field.DataType == "String"))
			{
				if (parser.Parsed(Token.SQUAREOPEN))
				{
					indexerExpr = new TBScriptExpression(Context.ScriptingSession, SymTable);
					indexerExpr.StopTokens = new StopTokens(new Token[] { Token.SQUARECLOSE });
					indexerExpr.StopTokens.skipInnerSquareBrackets = true;
					if (!indexerExpr.Compile(parser, CheckResultType.Compatible, "Int64"))
						return false;

					if (!parser.ParseTag(Token.SQUARECLOSE)) return false;
				}
				else indexerExpr = null;
			}

			if (!parser.ParseTag(Token.ASSIGN)) return false;

			localExpr = new TBScriptExpression(Context.ScriptingSession, SymTable);
			//TODO mi manca ArrayBaseType
			//if (!m_pLocalExpr.Compile(parser,
			//    CheckResultType.Compatible,
			//    (m_pField.DataType == "Array" && m_pIndexerExpr != null )? m_pField.ArrayBaseType : m_pField.DataType
			//    ))
			if (!localExpr.Compile(parser, CheckResultType.Compatible, field.DataType))
				return false;

			return parser.ParseSep();
		}
	}

	//============================================================================
	class ConditionalCommand : Command
	{
		protected Expression conditionExpr;
		protected CommandBlock thenBlock;
		protected CommandBlock elseBlock;

		//--------------------------------------------------------------------------------
		public ConditionalCommand(TbScript context, SymbolTable symTable)
			: base(context, symTable)
		{
			conditionExpr = new TBScriptExpression(Context.ScriptingSession, SymTable);
			thenBlock = new CommandBlock(context, symTable);
			elseBlock = new CommandBlock(context, symTable);
		}

		//--------------------------------------------------------------------------------
		override public bool Exec()
		{
			Value v = conditionExpr.Eval();
			if (!v.Valid)
				return Fail();	//TODO EMIT ERROR
			bool goodVal = Expression.CastBool(v);

			CommandBlock cmd = goodVal
					? thenBlock
					: elseBlock;

			bool ok = cmd.IsEmpty ? true : cmd.Exec();
			if (!ok || cmd.State == Command.CommandState.ABORT)
				return Fail();

			State = cmd.State;
			return true;
		}

		//--------------------------------------------------------------------------------
		override public bool Parse(Parser parser)
		{
			conditionExpr.StopTokens = new StopTokens(new Token[] { Token.THEN });
			if (!conditionExpr.Compile(parser, CheckResultType.Compatible, "Boolean"))
				return Fail();

			if (!parser.ParseTag(Token.THEN))
				return false;
			if (!thenBlock.Parse(parser))
				return false;

			if (parser.Parsed(Token.ELSE))
			{
				if (!elseBlock.Parse(parser))
					return false;

				parser.Parsed(Token.SEP);

				return /*m_ElseBlock.m_bHasBeginEnd ? parser.ParseSep() :*/ true;
			}

			parser.Parsed(Token.SEP);

			// ELSE not found
			return /*m_ThenBlock.m_bHasBeginEnd ? parser.ParseSep() :*/ true;
		}
	}

	//============================================================================
	class WhileCommand : Command
	{
		protected Expression conditionExpr;
		protected CommandBlock block;

		//--------------------------------------------------------------------------------
		public WhileCommand(TbScript context, SymbolTable symTable)
			:
			base(context, symTable)
		{
			conditionExpr = new TBScriptExpression(Context.ScriptingSession, SymTable);
			block = new CommandBlock(context, symTable);
		}

		//--------------------------------------------------------------------------------
		override public bool Exec()
		{
			for (; ; )
			{
				Value v = conditionExpr.Eval();
				if (!v.Valid)
					return Fail();
				bool ok = Expression.CastBool(v);
				if (!ok)
					return true;

				ok = block.Exec();
				if (!ok || block.State == Command.CommandState.ABORT)
					return Fail();

				if (block.State == Command.CommandState.RETURN)
				{
					State = block.State;
					return true;
				}
				if (block.State == Command.CommandState.BREAK)
				{
					return true;
				}
			}
		}

		//--------------------------------------------------------------------------------
		override public bool Parse(Parser parser)
		{
			conditionExpr.StopTokens = new StopTokens(new Token[] { Token.DO });
			if (!conditionExpr.Compile(parser, CheckResultType.Compatible, "Boolean"))
				return Fail();

			if (!parser.ParseTag(Token.DO))
				return false;
			if (!block.Parse(parser))
				return false;

			parser.Parsed(Token.SEP);

			return /*m_Block.m_bHasBeginEnd ? parser.ParseSep() :*/ true;
		}
	}

	//============================================================================
	class BreakCommand : Command
	{
		//--------------------------------------------------------------------------------
		public BreakCommand(TbScript context, SymbolTable symTable)
			: base(context, symTable)
		{ }

		//--------------------------------------------------------------------------------
		override public bool Exec()
		{
			State = Command.CommandState.BREAK;
			return true;
		}

		//--------------------------------------------------------------------------------
		override public bool Parse(Parser parser)
		{
			parser.Parsed(Token.SEP);

			return /*parser.ParseSep()*/true;
		}
	}

	//============================================================================
	class ContinueCommand : Command
	{
		//--------------------------------------------------------------------------------
		public ContinueCommand(TbScript context, SymbolTable symTable)
			: base(context, symTable)
		{ }

		//--------------------------------------------------------------------------------
		override public bool Exec()
		{
			State = Command.CommandState.CONTINUE;
			return true;
		}

		//--------------------------------------------------------------------------------
		override public bool Parse(Parser parser)
		{
			parser.Parsed(Token.SEP);

			return /*parser.ParseSep()*/true;
		}
	}

	//============================================================================
	class MessageCommand : Command
	{
		protected Expression messageExpr;
		//--------------------------------------------------------------------------------
		public MessageCommand(TbScript context, SymbolTable symTable)
			: base(context, symTable)
		{
			messageExpr = new TBScriptExpression(Context.ScriptingSession, SymTable);
		}

		//--------------------------------------------------------------------------------
		override public bool Exec()
		{
			Value v = messageExpr.Eval();
			if (v == null || !v.Valid)
				return Fail();
			
			string message = Expression.CastString(v);
			scriptContext.MessageProvider.Message(message);
			
			return true;
		}

		//--------------------------------------------------------------------------------
		override public bool Parse(Parser parser)
		{
			if (!messageExpr.Compile(parser, CheckResultType.Compatible, "String"))
				return false;
            //TODO RSWEB
            //Message "ciao" , "icon"
            if (parser.Parsed(Token.COMMA))
                parser.SkipToken();

			return parser.ParseSep();
		}
	}

	//============================================================================
	class DeclareCommand : Command
	{
		protected Variable field = null;
		protected Expression initExpr = null;

		//--------------------------------------------------------------------------------
		public DeclareCommand(TbScript context, SymbolTable symTable)
			: base(context, symTable)
		{
		}

		//--------------------------------------------------------------------------------
		override public bool Exec()
		{
			if (initExpr != null)
			{
				Value v = initExpr.Eval();
				if (v == null || !v.Valid)
					return Fail();
				try
				{
					field.Data = ObjectHelper.ConvertType(v.Data, field.Data.GetType());
				}
				catch (Exception e)
				{
					return Fail(e.Message);
				}
			}
			return true;
		}

		//--------------------------------------------------------------------------------
		override public bool Parse(Parser parser)
		{
			ushort tag = 0;
			//ushort item = 0;
			string aType = "String";
			string woormType = "";
			string aBaseType = "";

			if (!DataTypeParser.Parse(parser, this.Context.EnumsTable, out aType, out woormType, out tag, out aBaseType))
			{
				return false;
			}

			string fieldName;
			if (!parser.ParseID(out fieldName))
				return false;

			Variable pExistField = SymTable.Find(fieldName);
			if (pExistField != null)	//TODO ammettere se livello di scope è superiore
			{
				parser.SetError(string.Format(ExpressionManagerStrings.DuplicateIdentifier, fieldName));
				return false;
			}

			field = new Variable(fieldName);
			field.Data = ObjectHelper.CreateObject(aType, aBaseType, tag, 0);

			if (parser.Parsed(Token.ASSIGN))
			{
				initExpr = new TBScriptExpression(Context.ScriptingSession, SymTable);

				if (!initExpr.Compile(parser, CheckResultType.Compatible, aType))
					return false;
			}
			SymTable.Add(field);

			return parser.ParseSep();
		}

	}

	//============================================================================
	class ReturnCommand : Command
	{
		protected Expression returnExpr = null;

		//--------------------------------------------------------------------------------
		public ReturnCommand(TbScript context, SymbolTable symTable)
			: base(context, symTable)
		{
		}

		//--------------------------------------------------------------------------------
		override public bool Exec()
		{
			if (returnExpr != null)
			{
				Value v = returnExpr.Eval();
				if (!v.Valid)
					return Fail();

				scriptContext.ReturnValue = v.Data;
			}
			State = Command.CommandState.RETURN;
			return true;
		}

		//--------------------------------------------------------------------------------
		override public bool Parse(Parser parser)
		{
			if (!scriptContext.ReturnType.CompareNoCase("Void") && !scriptContext.ReturnType.CompareNoCase("System.Void"))
			{
				returnExpr = new TBScriptExpression(Context.ScriptingSession, SymTable);

				if (!returnExpr.Compile(parser, CheckResultType.Compatible, scriptContext.ReturnType))
					return false;
				return parser.ParseSep();
			}
			parser.Parsed(Token.SEP);
			return true;
		}
	}

	//============================================================================
	class ExprCommand : Command
	{
		protected Expression commandExpr;

		//--------------------------------------------------------------------------------
		public ExprCommand(TbScript context, SymbolTable symTable)
			:
			base(context, symTable)
		{
			commandExpr = new TBScriptExpression(Context.ScriptingSession, SymTable);
		}

		//--------------------------------------------------------------------------------
		override public bool Exec()
		{
			Value dummy = commandExpr.Eval();
			if (!dummy.Valid)
			{
				return Fail();
			}
			return true;
		}

		//--------------------------------------------------------------------------------
		override public bool Parse(Parser parser)
		{
			if (!commandExpr.Compile(parser, CheckResultType.Compatible, "Variant"))
				return false;

			return parser.ParseSep();
		}
	}

	//============================================================================
	public class TbScript
	{
		private SymbolTable symbolTable;

		protected CommandBlock block;

		private List<string> errors = new List<string>();
		private TbReportSession scriptingSession = null;

		private IMessageProvider messageProvider;

#if DEBUG_SCRIPT
		protected ArrayList commands = new ArrayList();		//DEBUG member
		protected int level;			//DEBUG member
		protected bool paused;			//DEBUG member
#endif

		//--------------------------------------------------------------------------------
		public string ReturnType { get; set; }
		//--------------------------------------------------------------------------------
		public object ReturnValue { get; set; }

		//--------------------------------------------------------------------------------
		public TbReportSession ScriptingSession { get { return scriptingSession; } }
		//--------------------------------------------------------------------------------
		public Enums EnumsTable { get { return scriptingSession.Enums; } }
		//--------------------------------------------------------------------------------
		public FunctionsList FunctionsTable { get { return scriptingSession.Functions; } }
		//--------------------------------------------------------------------------------
		public SymbolTable SymbolTable { get { return symbolTable; } }
		//--------------------------------------------------------------------------------
		public List<string> Errors { get { return errors; } }
		//--------------------------------------------------------------------------------
		internal IMessageProvider MessageProvider { get { return messageProvider; } }

		//--------------------------------------------------------------------------------
		public string ErrorText
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				foreach (string error in errors)
					sb.AppendLine(error);

				return sb.ToString();
			}
		}


#if DEBUG_SCRIPT
		//--------------------------------------------------------------------------------
		public int Level { get { return level; } }
		//--------------------------------------------------------------------------------
		public bool Paused { get { return paused; } set { paused = value; } }
#endif

		//--------------------------------------------------------------------------------------
		public TbScript(TbReportSession reportSession, SymbolTable globalSymTable, IMessageProvider messageProvider)
		{
			scriptingSession = reportSession;
			this.messageProvider = messageProvider;
#if DEBUG_SCRIPT
			level = 0;
			paused = false;
#endif
			this.symbolTable = new DynamicSymbolTable(globalSymTable);
			
		}

		//--------------------------------------------------------------------------------------
		public bool SetError(string error)
		{
			errors.Add(error);
			return false;
		}
#if DEBUG_SCRIPT
		//--------------------------------------------------------------------------------
		public void AddCommand(Command pCmd)
		{ 
			commands.Add(pCmd); 
		}
#endif
		//--------------------------------------------------------------------------------
		public virtual bool Exec()
		{
			return block.Exec();
		}

		//--------------------------------------------------------------------------------
		public virtual bool Parse(Parser parser)
		{
			block = new CommandBlock(this, symbolTable);
			return block.Parse(parser);
		}

		//--------------------------------------------------------------------------------
		public virtual Command ParseExtendedCommands(Parser lex)
		{
			return null;
		}
	
	}

	/// <summary>
	/// Symbol table dinamica in grado di accedere ai field o properties di oggetti
	/// via reflection
	/// </summary>
	//================================================================================
	internal class DynamicSymbolTable : SymbolTable
	{
		//--------------------------------------------------------------------------------
		public DynamicSymbolTable(SymbolTable parent)
			: base(parent)
		{
		}

		//--------------------------------------------------------------------------------
		public override Variable Find(string name)
		{
			Variable v = base.Find(name);
			if (v != null)
				return v;
			int dotIndex = name.LastIndexOf('.');
			if (dotIndex == -1)
				return null; //non c'è il punto: non è una chiamata a proprietà o campo appartenente ad oggetto

			string varName = name.Substring(0, dotIndex);	//nome variabile

			v = Find(varName);	//cerca la variabile (se necessario ricorsivamente)
			if (v == null)
				return null;

			DynamicVariable var = new DynamicVariable(name, name.Substring(dotIndex + 1), v);
			Add(var);
			return var;
		}
	}

	//================================================================================
	/// <summary>
	/// variabile che non incapsula un dato, ma le informazioni per valutare dinamicamente via reflection
	/// una property o un field di un oggetto
	/// </summary>
	internal class DynamicVariable : Variable
	{
		/// <summary>
		/// la variabile che rappresenta il 'this' su cui effettuare la chiamata
		/// </summary>
		private Variable objectVariable;
		/// <summary>
		/// nome della proprietà o del campo
		/// </summary>
		private string propName;
		//--------------------------------------------------------------------------------
		public DynamicVariable(string name, string propName, Variable objectVariable)
			: base(name)
		{
			this.objectVariable = objectVariable;
			this.propName = propName;
		}

		//--------------------------------------------------------------------------------
		public override string DataType { get { return "Object"; } }
		//--------------------------------------------------------------------------------
		public override object Data
		{
			get
			{
				//prima verifico se l'oggetto ha una proprietà con quel nome
				PropertyInfo pi = ObjectHelper.GetProperty(objectVariable.Data, propName);
				if (pi != null)
					return pi.GetGetMethod().Invoke(objectVariable.Data, null);

				//poi ripiego sul campo
				FieldInfo fi = ObjectHelper.GetField(objectVariable.Data, propName);
				if (fi != null)
					return fi.GetValue(objectVariable.Data);

				throw new ApplicationException(string.Format(ExpressionManagerStrings.InvalidProperty, objectVariable.Data.GetType().FullName, propName));
			}
			set
			{
				//prima verifico se l'oggetto ha una proprietà con quel nome
				PropertyInfo pi = ObjectHelper.GetProperty(objectVariable.Data, propName);
				if (pi != null)
				{
					pi.GetSetMethod().Invoke(objectVariable.Data, new object[] { value });
					return;
				}

				//poi ripiego sul campo
				FieldInfo fi = ObjectHelper.GetField(objectVariable.Data, propName);
				if (fi != null)
				{
					fi.SetValue(objectVariable.Data, value);
					return;
				}

				throw new ApplicationException(string.Format(ExpressionManagerStrings.InvalidProperty, objectVariable.Data.GetType().FullName, propName));
			}
		}

		//--------------------------------------------------------------------------------
		public override object Clone()
		{
			DynamicVariable v = new DynamicVariable(Name, propName, objectVariable);
			CopyProperties(v);
			return v;
		}

		//--------------------------------------------------------------------------------
		public override bool Equals(object item)
		{
			if (!base.Equals(item))
				return false;

			DynamicVariable var = item as DynamicVariable;
			if (var == null)
				return false;

			return var.objectVariable.Equals(objectVariable) &&
				string.Compare(propName, var.propName, true, GetCollateCulture(this, var)) == 0;
		}

		//--------------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}


}

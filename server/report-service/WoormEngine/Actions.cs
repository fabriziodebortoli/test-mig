using System.Collections;
using System.Collections.Generic;

using Microarea.Common.Generic;
using Microarea.Common.Lexan;
using Microarea.Common.ExpressionManager;
using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using TaskBuilderNetCore.Data;
using Microarea.Common.Hotlink;
using Microarea.Common;

namespace Microarea.RSWeb.WoormEngine
{
    //============================================================================
    public class WoormEngineExpression : Expression
	{
		protected ReportEngine engine = null ;

		//-----------------------------------------------------------------------------
		public WoormEngineExpression(ReportEngine engine, TbReportSession session, SymbolTable symbolTable)
			: base (session, symbolTable)
		{
			this.engine = engine;
		}

		//-----------------------------------------------------------------------------
        override public Value ApplySpecializedFunction(FunctionItem function, Stack<Item> paramStack)
		{
            if (function.HasLateBinding && function.Prototype == null)
            {
                string handleName; FunctionPrototype fp;
                if (engine.RepSymTable.ResolveCallQuery(function.Name, engine.Session.Functions, out handleName, out fp))
                {
                   function.Prototype = fp;
                   paramStack.Push(new Value(handleName));
                }
                else 
                {
                    Procedure p = engine.RepSymTable.ResolveCallProcedure(function.Name);
                    if (p != null)
                    {
                        return p.Exec(paramStack);
                    }
                }
            }

            //--------------
            if (function.FullName.CompareNoCase("Framework.TbWoormViewer.TbWoormViewer.QueryOpen"))
			{
				Value v1 = (Value) paramStack.Pop();
				string name = CastString(v1);

				QueryObject q = engine.RepSymTable.QueryObjects.Find(name);
				if (q == null)
					return new Value(false);

				return new Value(q.Open());
			}
            else if (function.FullName.CompareNoCase("Framework.TbWoormViewer.TbWoormViewer.QueryIsOpen"))
			{
				Value v1 = (Value) paramStack.Pop();
				string name = CastString(v1);

				QueryObject q = engine.RepSymTable.QueryObjects.Find(name);
				if (q == null)
					return new Value(false);

				return new Value(q.IsOpen());
			}
            else if (function.FullName.CompareNoCase("Framework.TbWoormViewer.TbWoormViewer.QueryClose"))
			{
				Value v1 = (Value) paramStack.Pop();
				string name = CastString(v1);

				QueryObject q = engine.RepSymTable.QueryObjects.Find(name);
				if (q == null)
					return new Value(false);

				return new Value(q.Close());
			}
            else if (function.FullName.CompareNoCase("Framework.TbWoormViewer.TbWoormViewer.QueryRead"))
			{
				Value v1 = (Value) paramStack.Pop();
				string name = CastString(v1);

				QueryObject q = engine.RepSymTable.QueryObjects.Find(name);
				if (q == null)
					return new Value(false);

				return new Value(q.Read());
			}
            else if (function.FullName.CompareNoCase("Framework.TbWoormViewer.TbWoormViewer.QueryReadOne"))
			{
				Value v1 = (Value)paramStack.Pop();
				string name = CastString(v1);

				QueryObject q = engine.RepSymTable.QueryObjects.Find(name);
				if (q == null)
					return new Value(false);

				Value v = new Value(q.Open() && q.Read());
				q.Close();

				return v;
			}
            else if (function.FullName.CompareNoCase("Framework.TbWoormViewer.TbWoormViewer.QueryExecute"))
			{
				Value v1 = (Value) paramStack.Pop();
				string name = CastString(v1);

				QueryObject q = engine.RepSymTable.QueryObjects.Find(name);
				if (q == null)
					return new Value(false);

				return new Value(q.Execute());
			}
            else if (function.FullName.CompareNoCase("Framework.TbWoormViewer.TbWoormViewer.QueryCall"))
            {
                Value v1 = (Value)paramStack.Pop();
                string name = CastString(v1);

                QueryObject q = engine.RepSymTable.QueryObjects.Find(name);
                if (q == null)
                    return new Value(false);

                return new Value(q.Call());   
            }
            else if (string.Compare(function.Name, "Framework.TbWoormViewer.TbWoormViewer.GetTableRows", true) == 0)
			{
				Value v1 = (Value) paramStack.Pop();
				string name = CastString(v1);
				//int id = CastInt(v1);

                string layout = null;
                Field fLayout = engine.RepSymTable.Fields.Find(SpecialReportField.NAME.LAYOUT);
                if (fLayout != null && fLayout.Data != null && fLayout.Data is string)
                    layout = fLayout.Data as string;

                DisplayTable dt = engine.RepSymTable.DisplayTables.Find(name, layout.IsNullOrEmpty() ? DisplayTable.LayoutDefaultName: layout);  //id);
				if (dt != null)
					return new Value(dt.RowsNumber);

				return new Value(0);
			}
			else if (string.Compare(function.Name, "Framework.TbWoormViewer.TbWoormViewer.GetTableCurrentRow", true) == 0)
			{
				Value v1 = (Value)paramStack.Pop();
				string name = CastString(v1);
				//int id = CastInt(v1);
                string layout = null;
                Field fLayout = engine.RepSymTable.Fields.Find(SpecialReportField.NAME.LAYOUT);
                if (fLayout != null && fLayout.Data != null && fLayout.Data is string)
                    layout = fLayout.Data as string;

                DisplayTable dt = engine.RepSymTable.DisplayTables.Find(name, layout.IsNullOrEmpty() ? DisplayTable.LayoutDefaultName: layout);  //id);
				if (dt != null)
					return new Value(dt.CurrentRow);

				return new Value(0);
            }

            else if (
                string.Compare(function.Name, "Framework.TbWoormViewer.TbWoormViewer.CurrentRowContainsCellTail", true) == 0
                ||
                string.Compare(function.Name, "Framework.TbWoormViewer.TbWoormViewer.CurrentRowContainsCellSubTotal", true) == 0
                )
            {
                Value v1 = (Value)paramStack.Pop();
                string name = CastString(v1);
                //int id = CastInt(v1);
                string layout = null;
                Field fLayout = engine.RepSymTable.Fields.Find(SpecialReportField.NAME.LAYOUT);
                if (fLayout != null && fLayout.Data != null && fLayout.Data is string)
                    layout = fLayout.Data as string;

                DisplayTable dt = engine.RepSymTable.DisplayTables.Find(name, layout.IsNullOrEmpty() ? DisplayTable.LayoutDefaultName : layout);  //id);
                if (dt != null)
                {
                    ;   //TODO
                }
                return new Value(false);
            }

            else if (string.Compare(function.Name, "Framework.TbWoormViewer.TbWoormViewer.GetRows", true) == 0)
            {
                Value v1 = (Value)paramStack.Pop();
                ushort id = CastUShort(v1);

                string layout = null;
                Field fLayout = engine.RepSymTable.Fields.Find(SpecialReportField.NAME.LAYOUT);
                if (fLayout != null && fLayout.Data != null && fLayout.Data is string)
                    layout = fLayout.Data as string;

                DisplayTable dt = engine.RepSymTable.DisplayTables.Find(id, layout.IsNullOrEmpty() ? DisplayTable.LayoutDefaultName : layout);  //id);
                if (dt != null)
                    return new Value(dt.RowsNumber);

                return new Value(0);
            }
            else if (string.Compare(function.Name, "Framework.TbWoormViewer.TbWoormViewer.GetCurrentRow", true) == 0)
            {
                Value v1 = (Value)paramStack.Pop();
                ushort id = CastUShort(v1);

                string layout = null;
                Field fLayout = engine.RepSymTable.Fields.Find(SpecialReportField.NAME.LAYOUT);
                if (fLayout != null && fLayout.Data != null && fLayout.Data is string)
                    layout = fLayout.Data as string;

                DisplayTable dt = engine.RepSymTable.DisplayTables.Find(id, layout.IsNullOrEmpty() ? DisplayTable.LayoutDefaultName : layout);  //id);
                if (dt != null)
                    return new Value(dt.CurrentRow);

                return new Value(0);
            }

            else if (string.Compare(function.Name, "Framework.TbWoormViewer.TbWoormViewer.UpdateOutputParametersEvenIfReportDoesNotFetchRecords", true) == 0)
            {
                //TODO
                return new Value(true);
            }
            else if (string.Compare(function.Name, "Framework.TbWoormViewer.TbWoormViewer.SkipContext", true) == 0)
            {
                //TODO
                return new Value(true);
            }
            else if (string.Compare(function.Name, "Framework.TbWoormViewer.TbWoormViewer.QuerySetConnection", true) == 0)
			{
				//TODO
				Value v1 = (Value)paramStack.Pop();
				string name = CastString(v1);

				QueryObject q = engine.RepSymTable.QueryObjects.Find(name);
				if (q == null)
					return new Value(false);

				Value hc = (Value)paramStack.Pop();
				long idx = CastLong(hc);

				object o = null;
				//dato l'indice di connection lo cerca nel bag
				//o = (bag.Find(idx);
				if (o != null)
					q.TbConnection = (DBConnection)o;

				Value v = new Value(o != null);
				return v;
			}
			else if (string.Compare(function.Name, "Framework.TbWoormViewer.TbWoormViewer.GetConnection", true) == 0)
			{
				//TODO
				Value hc = (Value)paramStack.Pop();
				long s = CastLong(hc);
				long idx = 0;
				if (s == 0)
				{
					// get report connection
					//put in bag
				}
				else if (s == -1)
				{
					//todo new connection
					//put in bag
				}

				Value v = new Value(idx);
				return v;
			}
			else if (string.Compare(function.Name, "Framework.TbWoormViewer.TbWoormViewer.CloseConnection", true) == 0)
			{
				//TODO
				Value hc = (Value)paramStack.Pop();
				long idx = CastLong(hc);

				object o = null;
				//dato l'indice di connection lo cerca nel bag
				//o = (bag.Find(idx);
				if (o != null)
				{
                    DBConnection conn = (DBConnection)o;
					conn.Close();
					conn.Dispose();
				}

				Value v = new Value(o != null);
				return v;
			}

			return null;
		}
	}

	/// <summary>
	/// ActionObj
	/// </summary>
	//============================================================================
	abstract public class ActionObj
	{
		protected ReportEngine engine = null;

        //TODO nome c++ per porting in corso, quando funziona fare refactoring
        public enum ActionStates { STATE_NORMAL, STATE_RETURN, STATE_BREAK, STATE_CONTINUE, STATE_ABORT, STATE_QUIT };
        public ActionStates ActionState = ActionStates.STATE_NORMAL;

		RepSymTable SymTable = null;
        public FunctionPrototype Fun = null;
        protected ActionObj parent = null;

        public bool CanRun() { return ActionState == ActionStates.STATE_NORMAL; }
		public bool IsUnparsable = true;
        virtual public bool HasMember(string name) { return false; }
        public virtual RepSymTable GetSymTable() { return SymTable; }

        public Block GetRoot() { return (parent != null ? parent.GetRoot() : this as Block); }
		
        //---------------------------------------------------------------------------
        public bool Fail (string error = "")
        {
           ActionState = ActionStates.STATE_ABORT;

	        return false;
        }

		//---------------------------------------------------------------------------
		public TbReportSession Session { get { return engine.Session; }}
			
		//---------------------------------------------------------------------------
        public ActionObj(ActionObj par, ReportEngine engin, RepSymTable symTable = null, FunctionPrototype fp = null) 
		{
            this.parent = par;
			this.engine = engin;
            this.SymTable = symTable != null ? symTable : engin.RepSymTable;
            this.Fun = fp;
		}

		//---------------------------------------------------------------------------
		virtual public bool Exec() { return true; }

		//---------------------------------------------------------------------------
		public virtual bool Unparse(Unparser unparser)
		{
			return true;
		}
	}

	/// <summary>
	/// Block
	/// </summary>
	//============================================================================
    public class Block : ActionObj
	{
		protected List<ActionObj> actions = new List<ActionObj>();
        protected bool limited = false;    //m_bRaiseEvents in C++ ?
		public bool	HasBeginEnd = false;

        RepSymTable localSymTable = null;
        public bool IsRuleScope = false;

        public override RepSymTable GetSymTable	()  { return localSymTable != null ? localSymTable : base.GetSymTable(); }

		//---------------------------------------------------------------------------
		public bool IsEmpty			{ get { return actions.Count == 0 && !HasBeginEnd; } }
		public void AddAction		(ActionObj action) { actions.Add(action); }
		public void InsertActionAt	(int pos, ActionObj action) { actions.Insert(pos, action); }

        public bool EmptyCommands { get { return actions.Count == 0; } }
		   
		//---------------------------------------------------------------------------
        public Block(ActionObj par, ReportEngine engine, RepSymTable symTable = null, FunctionPrototype fp = null, bool limited = false)
            : base(par, engine, symTable, fp)
		{
			this.limited = limited;
		}

        //---------------------------------------------------------------------------
        public void AddLocalField (Field pLocal)
        {
	        if (localSymTable == null)
				localSymTable = new RepSymTable(base.GetSymTable());
	        
			localSymTable.Fields.Add(pLocal);
        }

		//---------------------------------------------------------------------------
		public bool ContainsOnlyNonUnparsableActions()
		{
			int counter = 0;
			foreach (ActionObj item in actions)
			{
				if (!item.IsUnparsable)
					counter++;
			}
			return counter == actions.Count;
		}

		//---------------------------------------------------------------------------
		public override bool Exec()
		{
            ActionState = ActionStates.STATE_NORMAL;

            foreach (ActionObj cmd in actions)
            {
                if (!cmd.Exec()) 
                    return Fail();

                if (cmd.ActionState == ActionStates.STATE_ABORT)
                {
                    return Fail();
                }

                if (cmd.ActionState == ActionStates.STATE_QUIT)
                {
                    return false;
                }

                if (!cmd.CanRun())  //break, continue, return
                {
                    ActionState = cmd.ActionState;
                    break;
                }
            }
			return true;
		}

		//---------------------------------------------------------------------------
		private ActionObj ParseDisplayTableAction(Parser lex, RdeWriter.Command aCmd)
		{
            Expression customTitle = null;
            if (aCmd == RdeWriter.Command.SubTitleLine)
            {
                customTitle = new Expression(engine.Session, GetSymTable().Fields);
                bool ok = customTitle.Compile(lex, CheckResultType.Match, "String");
                if (!ok)
                {
                   return null;
                }               
                aCmd = RdeWriter.Command.SubTitleLine;
            }
            else if (aCmd == RdeWriter.Command.TitleLine)
            {
                aCmd = RdeWriter.Command.TitleLine;
            }

            DisplayTable displayTable = engine.ParseDisplayTable(lex);
			if (displayTable == null) 
                return null;    // error set by ParseDisplayTable

			engine.DisplayAction = true;

			if (!lex.ParseSep()) 
                return null;

			return new DisplayTableAction(this, engine, GetSymTable(), aCmd, displayTable, customTitle);
		}

		//---------------------------------------------------------------------------
		private bool ActionNotAllowed(Token action)
		{
			if 	(!limited) return false;

			return
				action == Token.DISPLAY				||
				action == Token.DISPLAY_TABLE_ROW	||
				action == Token.DISPLAY_FREE_FIELDS	||
				action == Token.FORMFEED			||
				action == Token.INTERLINE			|| 
				action == Token.SPACELINE			|| 
				action == Token.NEXTLINE			||
                action == Token.TITLELINE           ||
                action == Token.SUBTITLELINE    ||
                action == Token.ASK					||
				action == Token.ABORT				||
				action == Token.MESSAGE;
		}

		//---------------------------------------------------------------------------
		private ActionObj ParseAction(Parser lex)
		{
			Token actionToken = lex.LookAhead();

			if (lex.Error) return null;
			
			if (ActionNotAllowed(actionToken))
			{
				lex.SetError(string.Format(WoormEngineStrings.ActionNotAllowed, actionToken.ToString()));
				return null;
			}

			switch (actionToken)
			{
				case Token.ID    :
                case Token.EVAL  :
				case Token.RESET :
				{
 					AssignEvalResetAction actionObj = new AssignEvalResetAction(this, engine, GetSymTable());
					if (actionObj.Parse(lex))
						return actionObj;
					break;
				}

				case Token.IF :
				{
                    lex.SkipToken();
                    ConditionalAction actionObj = new ConditionalAction(this, engine, GetSymTable(), Fun);
					if (actionObj.Parse(lex))
						return actionObj;
					break;
				}

				case Token.WHILE :
				{
                    lex.SkipToken();
                    WhileLoopAction actionObj = new WhileLoopAction(this, engine, GetSymTable(), Fun);
					if (actionObj.Parse(lex))
						return actionObj;
					break;
				}

				case Token.DISPLAY :
				case Token.DISPLAY_TABLE_ROW :
				case Token.DISPLAY_FREE_FIELDS :
				{
                    lex.SkipToken();
                    DisplayFieldsAction actionObj = new DisplayFieldsAction(this, engine, GetSymTable());
					if (actionObj.Parse(actionToken, lex, GetSymTable()))
					{
						engine.DisplayAction = true;
						return actionObj;
					}
					break;
				}

				case Token.FORMFEED :
				{
                    lex.SkipToken();
					if (engine.OnFormFeedAction)
					{
						lex.SetError(WoormEngineStrings.IllegalAction);
						return null;
					}

                    FormFeedAction actionObj = new FormFeedAction(this, engine, GetSymTable());
					if (!actionObj.Parse(lex))
						return null;

					engine.DisplayAction = true;

					return actionObj;
				}

				case Token.INTERLINE    : { lex.SkipToken(); return ParseDisplayTableAction(lex, RdeWriter.Command.Interline); }
                //TODO RSWEB
                case Token.TITLELINE    : { lex.SkipToken(); return ParseDisplayTableAction(lex, RdeWriter.Command.TitleLine); }
                case Token.SUBTITLELINE: { lex.SkipToken(); return ParseDisplayTableAction(lex, RdeWriter.Command.SubTitleLine); }

                case Token.SPACELINE    :   //sinonimo - TODO dovrebbe essere doppia NEXTLINE
				case Token.NEXTLINE     : { lex.SkipToken(); return ParseDisplayTableAction(lex, RdeWriter.Command.NextLine); }

				case Token.CALL :
				{
                    lex.SkipToken();
                    CallAction actionObj = new CallAction(this, engine, GetSymTable());
					if (actionObj.Parse(lex))
						return actionObj;
					
					break;
				}

				case Token.ASK:
				{
                    lex.SkipToken();
                    AskDialogAction actionObj = new AskDialogAction(this, engine, GetSymTable());
					if (actionObj.Parse(lex))
						return actionObj;
						
					break;
				}

				case Token.MESSAGE:
                case Token.DEBUG:
				case Token.ABORT:
                {
                    MessageBoxAction actionObj = new MessageBoxAction(this, engine, GetSymTable());
					if (actionObj.Parse(lex))
						return actionObj;
						
					break;
				}

                case Token.QUIT:
                case Token.BREAK:
                case Token.CONTINUE:
                {
                    QuitBreakContinueAction actionObj = new QuitBreakContinueAction(this, engine, GetSymTable());
                    if (actionObj.Parse(lex))
                        return actionObj;

                    break;
                }
                case Token.DO:
                {
                    DoExprAction actionObj = new DoExprAction(this, engine, GetSymTable());
                    if (actionObj.Parse(lex))
                        return actionObj;

                    break;
                }
                case Token.RETURN:
                {
                    ReturnAction actionObj = new ReturnAction(this, engine, GetSymTable(), Fun);
                    if (actionObj.Parse(lex))
                        return actionObj;

                    break;
                }
                case Token.DISPLAY_CHART:
                {
                    DisplayChartAction actionObj = new DisplayChartAction(this, engine, GetSymTable());
                    if (actionObj.Parse(lex))
                        return actionObj;

                    break;
                }

                default:
				{
                    DeclareAction actionObj = new DeclareAction(this, engine, GetSymTable(), this);
                    if (actionObj.Parse(lex))
                        return actionObj;

					lex.SetError(string.Format(WoormEngineStrings.UnknownAction, Language.GetTokenString(actionToken)));
					return null;
				}
			}

			return null;
		}

		//---------------------------------------------------------------------------
		virtual public bool Parse(Parser lex)
		{
			ActionObj actionObj;
			HasBeginEnd = lex.Matched(Token.BEGIN);

			// also accepts empty "begin..end" block sections
			do
			{
				if (HasBeginEnd && lex.Matched(Token.END))
					break;

				if (lex.Error || ((actionObj = ParseAction(lex)) == null))
					return false;
		
				AddAction(actionObj);
			}
			while (HasBeginEnd);

			return !lex.Error;
		}

		//---------------------------------------------------------------------------
		public override bool Unparse(Unparser unparser)
		{
			// to accept empty begin end blocks
			if (HasBeginEnd || actions.Count > 1)
			{
				unparser.WriteLine();
				unparser.WriteBegin(true);
				unparser.IncTab();
			}

			foreach (ActionObj item in actions)
			{
				if (item.IsUnparsable)
					item.Unparse(unparser);
			}

			if (HasBeginEnd || actions.Count > 1)
			{
				unparser.DecTab();
				unparser.WriteEnd(false);
			}

			return true;
		}

        //----------------------------------------------------------------------------
        override public bool HasMember(string name)
        {
            foreach (ActionObj cmd in actions)
            {
                if (cmd.HasMember(name))
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Procedure
    /// </summary>
    //============================================================================
    public class Procedure : Block
	{
		private bool	calledFromOnFormFeed;
		private bool	calledFromOnTable;

		private string	publicName = string.Empty;

		//---------------------------------------------------------------------------
        public string   PublicName { get { return publicName; } set { publicName = value; } }
		public int		InternalId { get { return 0; }}

		//---------------------------------------------------------------------------
		public Procedure (ReportEngine engine)
            : base (null, engine, null)
		{
			calledFromOnFormFeed	= engine.OnFormFeedAction;
			calledFromOnTable	    = engine.OnTableAction;
		}

		//---------------------------------------------------------------------------
        public override bool Parse(Parser lex)
        {
            if (!lex.ParseTag(Token.PROCEDURE))
                return false;

            if (!lex.ParseID(out publicName))
                return false;

            Procedure p = GetSymTable().Procedures.Find(publicName);
            if (p == null)
            {
                GetSymTable().Procedures.Add(this);

                return ParseBody (lex);
            }

            //la procedura è stata utilizzata (CALL myproc) ma non ancora definita
            if (!p.IsEmpty)
            {
                lex.SetError(string.Format(WoormEngineStrings.ProcedureExist, publicName));
                return false;
            }
            //per rispettare l'ordine di definizione la metto in fondo
            GetSymTable().Procedures.Remove(p);
            GetSymTable().Procedures.Add(p);

            return p.ParseBody(lex);
        }
        
        //---------------------------------------------------------------------------
		bool ParseBody (Parser lex)
		{
	        if (lex.Matched(Token.ROUNDOPEN))
	        {
		        Fun = new FunctionPrototype (publicName, "Void", null);
                Fun.Parameters = new ParametersList();
		        do
		        {
                    //TODO RSWEB manca parsing direzione dei parametri delle PROCEDURE

                    string wType, wBaseType, type;
                    ushort enumTag;
                    if (!DataTypeParser.Parse(lex, engine.Session.Enums, out type, out wType, out enumTag, out wBaseType))
                    {
				        return false;
			        }

                    string paramName;
			        if (!lex.ParseID(out paramName))
				        return false;

			        Parameter param = new Parameter(paramName, type/*, param mode*/);
                    param.SetType(type, wBaseType, enumTag);
                    param.TbType = wType;


			        Fun.Parameters.Add(param);

                    Field fd = new Field(type, paramName, engine);
                        fd.WoormType = wType;
                        fd.ArrayBaseType = wBaseType;
                        fd.EnumTag = enumTag;
                        fd.Input = true;
                    base.AddLocalField(fd);
				
			        if (!lex.LookAhead(Token.ROUNDCLOSE))
				        lex.ParseTag(Token.COMMA);
		        }
		        while (!lex.Matched(Token.ROUNDCLOSE));
	        }

	        if (lex.Matched(Token.AS))
	        {
                string wType, wBaseType, type;
                ushort enumTag;
                if (!DataTypeParser.Parse(lex, engine.Session.Enums, out type, out wType, out enumTag, out wBaseType))
		        {
			        return false;
		        }

                if (Fun == null)
                    Fun = new FunctionPrototype(publicName, type, null);

                Fun.SetReturnType(type, wBaseType, enumTag);
                Fun.ReturnTbType = wType;
	        }

            //----------------
			// for checking the procedure call from a FormFeed event associated action                                      
			engine.OnFormFeedAction	= calledFromOnFormFeed;

			// for checking the procedure call from a Table event associated action
			engine.OnTableAction = calledFromOnTable;

			bool ok = base.Parse(lex);      
			
			engine.OnFormFeedAction	= false;
			engine.OnTableAction	= false;

			return ok;
		}

        //---------------------------------------------------------------------
        override public bool Unparse(Unparser unparser)
        {
            unparser.WriteTag(Token.PROCEDURE, false);
            unparser.WriteID(this.PublicName, false);

            if (Fun != null)
            {
                if (Fun.Parameters != null && Fun.Parameters.Count > 0)
                {
                    unparser.WriteTag(Token.ROUNDOPEN, false);
                    if (Fun.Parameters != null && Fun.Parameters.Count > 0)
                    {
                        bool first = true;
                        foreach (Parameter p in Fun.Parameters)
                        {
                            if (first)
                                first = false;
                            else
                                unparser.WriteTag(Token.COMMA, false);

                            unparser.UnparseDataType(p.TbType, p.TbBaseType, p.EnumTag, false, false);
                            if (p.EnumTag != 0 && this.engine != null)
                                unparser.Write(" /* " + this.engine.Session.Enums.TagName(Fun.ReturnEnumTag) + " */ ");

                            unparser.WriteID(p.Name, false);
                        }
                    }
                    unparser.WriteTag(Token.ROUNDCLOSE, false);
                }

                if (!Fun.ReturnType.CompareNoCase("Void"))
                {
                    unparser.WriteTag(Token.AS, false);

                    unparser.UnparseDataType(Fun.ReturnTbType, Fun.ReturnTbBaseType, Fun.ReturnEnumTag, false, false);
                    if (Fun.ReturnEnumTag != 0 && this.engine != null)
                        unparser.Write(" /* " + this.engine.Session.Enums.TagName(Fun.ReturnEnumTag) + " */ ");
                }
            }
           
			unparser.IncTab();
            base.Unparse(unparser);
			unparser.DecTab();

			unparser.WriteLine();
			unparser.WriteLine();
			
			return true;
        }

        //---------------------------------------------------------------------------
        public override bool Exec()
        {
            if (IsEmpty)
            {
                engine.SetError(string.Format(WoormEngineStrings.EmptyProcedure, PublicName));
                return false;
            }
            return base.Exec();
        }

        //---------------------------------------------------------------------
        internal Value Exec(Stack<Item> paramStack)
        {
           if (IsEmpty)
           {
                engine.SetError(string.Format(WoormEngineStrings.EmptyProcedure, PublicName));
                return null;
           }
           //if (this.Fun == null)
           //     return null;

           if (this.Fun != null && this.Fun.Parameters != null && this.Fun.Parameters.Count > 0)
            {
                if (this.Fun.Parameters.Count > paramStack.Count)
                    return null;

                foreach (Parameter par in this.Fun.Parameters)
                {
                    Field f = this.GetSymTable().Fields.Find(par.Name);
                    if (f == null)
                        return null;
                    if (paramStack.Count > 0)
                    {
                        object o = paramStack.Pop();
                        Value v = (Value)o;
                        f.Data = v.Data;
                    }
                }
            }

            if (!base.Exec())
            {
                return null;
            }

            return new Value(this.Fun != null ? this.Fun.ReturnValue.Data : null);
        }
    }

	/// <summary>
	/// EventActions
	/// </summary>engine
	//============================================================================
	public class EventActions
	{
		private		Block			beforeActions;
		private		Block			afterActions;
		protected	ReportEngine	engine;

		//---------------------------------------------------------------------------
		public TbReportSession Session { get { return engine.Session; }}

		//---------------------------------------------------------------------------
		public Block BeforeActions	{ get { return beforeActions; }}
		public Block AfterActions	{ get { return afterActions; }}
		public ReportEngine Engine	{ get { return engine; }}

		//---------------------------------------------------------------------------
		public EventActions (ReportEngine engine)
		{
			this.engine = engine;
			beforeActions  = new Block(null, engine);
			afterActions   = new Block(null, engine);
		}

		//---------------------------------------------------------------------------
		public bool ParseEventActions(Parser lex)
		{
			if (lex.Matched(Token.BEFORE) && !BeforeActions.Parse(lex))
				return false;

			if (!lex.Error && lex.Matched(Token.AFTER) && !AfterActions.Parse(lex))
				return false;
	
			return !lex.Error;
		}

		//---------------------------------------------------------------------------
		public bool ContainsOnlyNonUnparsableActions()
		{
			return beforeActions.ContainsOnlyNonUnparsableActions() && afterActions.ContainsOnlyNonUnparsableActions();
		}

		//---------------------------------------------------------------------------
		public virtual bool Unparse(Unparser unparser)
		{
			if (!beforeActions.IsEmpty)
			{
				unparser.IncTab();
				unparser.WriteTag(Token.BEFORE, false);

				unparser.IncTab();
				beforeActions.Unparse(unparser);
				unparser.WriteLine();
				unparser.DecTab();
			
				unparser.DecTab();
			}

			if (!afterActions.IsEmpty)
			{
				unparser.IncTab();
				unparser.WriteTag(Token.AFTER, false);

				unparser.IncTab();
				afterActions.Unparse(unparser);
				unparser.WriteLine(); 
				unparser.DecTab();
		
				unparser.DecTab();
			}
			return true;
		}
		
		//---------------------------------------------------------------------------
		public virtual bool IsEmpty
		{
			get
			{
				return (beforeActions.IsEmpty && afterActions.IsEmpty);
			}
		}

		//---------------------------------------------------------------------------
		internal void UnparseFormFeedEvent(Unparser unparser)
		{
			if (IsEmpty)
				return;

			unparser.WriteTag(Token.FORMFEED, false);
			unparser.WriteTag(Token.COLON, false);
			unparser.WriteTag(Token.DO, true);

			Unparse(unparser);

			unparser.WriteLine();
		}
	}

	/// <summary>
	/// ComputationalAction
	/// </summary>
	//============================================================================
	public class AssignEvalResetAction : ActionObj
	{
		private Field		field;
		private WoormEngineExpression	localExpr;
		private Token		action;		// Token.EVAL or Token.RESET
		private WoormEngineExpression	indexerExpr;

		//---------------------------------------------------------------------------
        public AssignEvalResetAction(ActionObj par, ReportEngine engine, RepSymTable symTable)
            : base(par, engine, symTable)
		{
			field    	= null;
			localExpr	= null;
			action	= Token.NOTOKEN;
			indexerExpr = null;
		}

		//---------------------------------------------------------------------------
		public override bool Exec()
		{
            ActionState = ActionStates.STATE_NORMAL;
            
            if (localExpr != null)
			{
				Value v = localExpr.Eval();
				if (localExpr.Error)
				{
					engine.SetError(localExpr.Diagnostic, string.Format(WoormEngineStrings.EvalEventExpression, field.PublicName));
					return false;
				}

				if (field.IsArray)
				{
					if (indexerExpr != null)
					{
						Value vIdx = indexerExpr.Eval();
						if (indexerExpr.Error)
						{
							engine.SetError(localExpr.Diagnostic, string.Format(WoormEngineStrings.EvalEventExpression, field.PublicName));
							return false;
						}

						// se sono di una rule devo solo gestire eventData
						if (field.OwnRule)
						{
							field.AssignArrayEventData(v.Data, v.Valid && vIdx.Valid, vIdx.Data);
							field.EventDataUpdated = true;
							return true;
						}

						// devo aggiornare tutti gli "xxData" perchè posso utilizzarli anche
						// in altri contesti con il DataLevel diverso da event. Es: rule
						field.SetArrayAllData(v.Data, v.Valid, vIdx.Data);
					}
					else
					{
						if (v.Data is DataArray)
							field.AssignAllArray((DataArray)v.Data, v.Valid);
					}
				}
				else
				{
					if (indexerExpr != null)
					{
						Value vIdx = indexerExpr.Eval();
						if (indexerExpr.Error)
						{
							engine.SetError(localExpr.Diagnostic, string.Format(WoormEngineStrings.EvalEventExpression, field.PublicName));
							return false;
						}

						string prevData = field.EventData as string;
						string setChar = v.Data as string;
						if (prevData == null || setChar == null || !(vIdx.Data is int))
						{
							engine.SetError(localExpr.Diagnostic, string.Format(WoormEngineStrings.EvalEventExpression, field.PublicName));
							return false;
						}
						int idx = (int) (vIdx.Data) ;
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

                    if (GetRoot() != null && engine != null && GetRoot().IsRuleScope)
                    {
                        DataLevel lev = engine.DataLevel;

                        field.AssignData(lev, v.Data, v.Valid);
                    }
                    else if (field.OwnRule && engine.Status != ReportEngine.ReportStatus.Init)
					{ 
                        // se sono di una rule devo solo gestire eventData
						field.AssignEventData(v.Data, v.Valid);
						field.EventDataUpdated = true;

						return true;
					}

					// devo aggiornare tutti gli "xxData" perchè posso utilizzarli anche
					// in altri contesti con il DataLevel diverso da event. Es: rule
					field.SetAllData(v.Data, v.Valid);
				}
				return true;
			}

			switch (action)
			{
				case Token.EVAL :
				{
					action = Token.EVAL;
					EventFunction aFun = field.EventFunction;
					aFun.EvalFunction();
					if (aFun.Error)
					{
						engine.SetError(aFun.Diagnostic, string.Format(WoormEngineStrings.EvalEventFunction, field.PublicName));
						return false;
					}
					break;
				}
				case Token.RESET:
				{
					action = Token.RESET;
					
					if (!field.Init())
					{
						engine.SetError(string.Format(WoormEngineStrings.EvalInitExpression, field.PublicName));
						return false;
					}
					break;
				}
				default :
				engine.SetError(string.Format(WoormEngineStrings.UnknownAction, Language.GetTokenString(action)));
					return false;
			}
			return true;
		}

		//---------------------------------------------------------------------------
		public bool Parse(Parser lex)
		{
			Token actionToken = lex.LookAhead();
			if (actionToken == Token.EVAL || actionToken == Token.RESET)
				lex.SkipToken();

			string fieldName;
			if (!lex.ParseID(out fieldName)) 
                return false;

            field = GetSymTable().Fields.Find(fieldName);
			if (field == null)
			{
				lex.SetError(string.Format(ExpressionManagerStrings.UnknownField, fieldName));
				return false;
			}

           GetSymTable().Fields.AddTraceFieldModify(fieldName);

            if (actionToken == Token.ID)
			{
				if (field.IsArray || (lex.LookAhead(Token.SQUAREOPEN) && field.DataType == "String"))
				{
					if (lex.Matched(Token.SQUAREOPEN))
					{
						indexerExpr = new WoormEngineExpression(engine, Session, GetSymTable().Fields);
						indexerExpr.StopTokens = new StopTokens(new Token[] { Token.SQUARECLOSE });
						indexerExpr.StopTokens.skipInnerSquareBrackets = true;
						if (!indexerExpr.Compile(lex, CheckResultType.Compatible, "Int64"))
							return false;

						if (!lex.ParseTag(Token.SQUARECLOSE)) return false;
					}
					else indexerExpr = null;
				}

				if (!lex.ParseTag(Token.ASSIGN))
					return false;

				action = Token.ASSIGN;
                localExpr = new WoormEngineExpression(engine, Session, GetSymTable().Fields);
				if (!localExpr.Compile(lex, CheckResultType.Compatible, (field.IsArray && indexerExpr != null ) ? field.ArrayBaseType : field.DataType))
					return false;
			}
			else
			{
				if ((field.EventFunction == null) && (actionToken == Token.EVAL))
				{
					lex.SetError(string.Format(WoormEngineStrings.MissingFuction, fieldName));
					return false;
				}
				else
					action = actionToken;
			}
			return lex.ParseSep();
		}

		//---------------------------------------------------------------------------
		public override bool Unparse(Unparser unparser)
		{
			switch (action)
			{				
				case Token.EVAL:
					unparser.WriteTag(Token.EVAL, false);
					unparser.WriteID(field.Name, false);
					break;

				case Token.RESET:
					unparser.WriteTag(Token.RESET, false);
					unparser.WriteID(field.Name, false);
					break;

				case Token.ASSIGN:
					if (field == null || localExpr == null)
					return true;

					unparser.WriteID(field.Name, false);
					if (indexerExpr != null)
					{
						unparser.WriteTag(Token.SQUAREOPEN, false);
						unparser.WriteExpr(indexerExpr.ToString(), false);
						unparser.WriteTag(Token.SQUARECLOSE, false);
					}
					unparser.WriteTag(Token.ASSIGN, false);
					unparser.WriteExpr(localExpr.ToString(), false);
					break;
				default:
					return false;
			}
			unparser.WriteSep(true);
			
			return true;
		}

        //----------------------------------------------------------------------------
        override public bool HasMember(string name)
        {
             if (localExpr != null && localExpr.HasMember(name))
                return true;
            if (indexerExpr != null && indexerExpr.HasMember(name))
                return true;
            return false;
        }
    }

    /// <summary>
    /// ConditionalAction
    /// </summary>
    //============================================================================
    public class ConditionalAction : ActionObj
	{
		private WoormEngineExpression	conditionExpr;
		private Block		thenBlock;
		private Block		elseBlock;

		//---------------------------------------------------------------------------
        public ConditionalAction(ActionObj par, ReportEngine engine, RepSymTable symTable, FunctionPrototype fp = null)
            : base(par, engine, symTable, fp)
		{
            conditionExpr   = new WoormEngineExpression(engine, Session, GetSymTable().Fields);
            thenBlock = new Block(this, engine, GetSymTable(), fp);
            elseBlock = new Block(this, engine, GetSymTable(), fp);
		}

		//---------------------------------------------------------------------------
		public override bool Exec()
		{
            ActionState = ActionStates.STATE_NORMAL;

            Value v = conditionExpr.Eval();

			if (conditionExpr.Error)
			{
				engine.SetError(conditionExpr.Diagnostic, WoormEngineStrings.EvalCondition);
				return false;
			}

			bool condition = (bool)v.Data;

            Block block =
                condition
                    ? thenBlock
                    : (elseBlock.IsEmpty ? null : elseBlock);

            if (block == null)
                return true;

            bool b = block.Exec();

            if (!b || block.ActionState == ActionStates.STATE_ABORT)
            {
                ActionState = ActionStates.STATE_ABORT;
                return false;
            }

            if (
                block.ActionState == ActionStates.STATE_RETURN ||
                block.ActionState == ActionStates.STATE_BREAK ||
                block.ActionState == ActionStates.STATE_CONTINUE
                )
            {
                ActionState = block.ActionState;
            }

            return true;
		}

		//---------------------------------------------------------------------------
		public bool Parse(Parser lex)
		{
			conditionExpr.StopTokens = new StopTokens(new Token[] { Token.THEN });
			if (!(conditionExpr.Compile(lex, CheckResultType.Match, "Boolean"))) 
				return false;

			if (!lex.ParseTag(Token.THEN))	return false;
			if (!thenBlock.Parse(lex))	return false;
			
			if (lex.Matched(Token.ELSE))
			{
				if (!elseBlock.Parse(lex))	return false;
				
				return elseBlock.HasBeginEnd ? lex.ParseSep() : true;
			}
			
			// lexan error ?
			if (lex.Error)	return false;
			
			// ELSE not found
			return thenBlock.HasBeginEnd ? lex.ParseSep() : true;
		}

		//---------------------------------------------------------------------------
		public override bool Unparse(Unparser unparser)
		{
			if (thenBlock.IsEmpty && elseBlock.IsEmpty)
				return true;

			unparser.WriteTag(Token.IF, false);
			unparser.WriteExpr(conditionExpr.ToString(), true);

			unparser.IncTab();
			unparser.WriteTag(Token.THEN, false);

			// if block has Begin-End syntax no CrLf is wrote to append a separator
			// if there isn't any ELSE statement, otherwise after a simple action
			// is always appended a separator plus a CrLf
			unparser.IncTab();
			thenBlock.Unparse(unparser);
			unparser.DecTab();

			if (!elseBlock.IsEmpty)
			{
				//if (elseBlock.HasBeginEnd)
					unparser.WriteLine();	// it need a cr-lf pair but not a separator

				unparser.WriteTag(Token.ELSE, false);

				// if block has Begin-End syntax no CrLf is wrote to append a separator,
				// otherwise after a simple action is always appended a separator plus
				// a CrLf
				elseBlock.Unparse(unparser);

				if (elseBlock.HasBeginEnd)
					unparser.WriteSep(true);	// it need a separator and a cr-lf pair
			}
			else // ELSE not present
				if (thenBlock.HasBeginEnd)
					unparser.WriteSep(true);	// it need a separator and a cr-lf pair

			unparser.DecTab();
			return true;
		}

        //----------------------------------------------------------------------------
        override public bool HasMember(string name)
        {
            if (conditionExpr != null && conditionExpr.HasMember(name))
                return true;
            if (thenBlock != null && thenBlock.HasMember(name))
                return true;
            if (elseBlock != null && elseBlock.HasMember(name))
                return true;
            return false;
        }
    }

    /// <summary>
    /// WhileLoopAction
    /// </summary>
    //============================================================================
    public class WhileLoopAction : ActionObj
	{
		private WoormEngineExpression	conditionExpr;
		private Block		    block;

		//---------------------------------------------------------------------------
        public WhileLoopAction(ActionObj par, ReportEngine engine, RepSymTable symTable, FunctionPrototype fp = null) 
            : base (par, engine, symTable, fp)
		{
            conditionExpr = new WoormEngineExpression(engine, Session, GetSymTable().Fields);
            block = new Block(this, engine, GetSymTable(), fp);
		}

		//---------------------------------------------------------------------------
		public override bool Exec()
		{
            ActionState = ActionStates.STATE_NORMAL;

			while (!engine.UserBreak)
			{
				Value v = conditionExpr.Eval();
				if (conditionExpr.Error)
				{
					engine.SetError(conditionExpr.Diagnostic, WoormEngineStrings.EvalCondition);
					return false;
				}

				if (!((bool)v.Data))
					return true;

                bool b = block.Exec();
 
                if (!b || block.ActionState == ActionStates.STATE_ABORT)
                {
                    ActionState = ActionStates.STATE_ABORT;
                    return false;
                }

                if (block.ActionState == ActionStates.STATE_RETURN)
                {
                    ActionState = ActionStates.STATE_RETURN;
                    return true;
                }
                if (block.ActionState == ActionStates.STATE_BREAK)
                     return true;
			}

			return false;
		}

		//---------------------------------------------------------------------------
		public bool Parse(Parser lex)
		{
			conditionExpr.StopTokens = new StopTokens(new Token[] {Token.DO});
			if (!(conditionExpr.Compile(lex, CheckResultType.Match, "Boolean"))) 
				return false;

			if (!lex.ParseTag(Token.DO))	return false;
			if (!block.Parse(lex))	return false;
			
			return block.HasBeginEnd ? lex.ParseSep() : true;
		}

		//---------------------------------------------------------------------------
		public override bool Unparse(Unparser unparser)
		{
			if (block.IsEmpty) 
				return true;

			unparser.WriteTag(Token.WHILE, false);
			unparser.WriteExpr(conditionExpr.ToString(), true);

			unparser.IncTab();
			unparser.WriteTag(Token.DO, false);

			// if block has Begin-End syntax no CrLf is wrote to append a separator
			block.Unparse(unparser);

			if (block.HasBeginEnd)
				unparser.WriteSep(true);	// it need a separator and a cr-lf pair

			unparser.DecTab();
			return true;
		}

        //----------------------------------------------------------------------------
        override public bool HasMember(string name)
        {
            if (conditionExpr != null && conditionExpr.HasMember(name))
                return true;
            if (block != null && block.HasMember(name))
                return true;
            return false;
        }

    }

    /// <summary>
    /// DisplayFieldsAction
    /// </summary>
    //============================================================================
    public class DisplayFieldsAction : ActionObj
	{
		private List<Field> fields = new List<Field>();	// array of Field property of RepSymTable
		private Token actionToken = Token.NOTOKEN;
		string tableName = string.Empty;
		private List<Field> hiddenFields = new List<Field>();
		
		//---------------------------------------------------------------------------
        public DisplayFieldsAction(ActionObj par, ReportEngine engine, RepSymTable symTable)
            : base(par, engine, symTable) 
		{
		}

		//---------------------------------------------------------------------------
		public void AddField(Field repField)
		{
			fields.Add(repField);
		}

		//---------------------------------------------------------------------------
		public override bool Exec()
		{
            ActionState = ActionStates.STATE_NORMAL;

            //dovuto gestione stessa tabella con numero di righe diverse su layout diversi
			//devo aggiornare la DisplayTable associata ai Fields
            Field f1 = GetSymTable().Fields.Find(SpecialReportField.NAME.LAYOUT);
			if (!string.IsNullOrEmpty(f1.Data.ToString()))
			{

                foreach (Field rf in fields)
                {
                    if (rf.WoormType.CompareNoCase("Array") || rf.WoormType.CompareNoCase("DataArray"))
                        continue;

                    rf.ReattachCurrentDisplayTable(f1.Data.ToString());
                }
			}

            foreach (Field rf in fields)
            {
                if (rf.WoormType.CompareNoCase("Array") || rf.WoormType.CompareNoCase("DataArray"))
                { 
                    rf.WriteArray(engine); 
                    continue; 
                }

                if (!rf.Display(engine))
                    return false;
            }

			Field f = engine.RepSymTable.Fields.Find(SpecialReportField.NAME.PAGE);
			if (f != null)
			{
				int np = (int)f.Data;
				f.SetAllData(engine.OutChannel.PageNumber, true);
			}

			return true;
		}

		//---------------------------------------------------------------------------
		public bool ParseDisplay(Parser lex, RepSymTable symbolTable)
		{
			string name;
			do
			{
				if (!lex.ParseID(out name)) break;

				Field aField = symbolTable.Fields.Find(name);
				if (aField == null)
				{
					lex.SetError(string.Format(ExpressionManagerStrings.UnknownField, name));
					return false;
				}

				if	(
						aField.IsColumn && !aField.IsColTotal &&
						engine.OnFormFeedAction
					)
				{
					lex.SetError(string.Format(WoormEngineStrings.IllegalFormFeedOnColumnDisplay, name));
					return false;
				}

                if (!aField.Hidden)
                {
                    aField.Displayed = true;
                    AddField(aField);
                }
                else
                {
                    if (aField.WoormType.CompareNoCase("Array") || aField.WoormType.CompareNoCase("DataArray"))
                        AddField(aField);
                    else
                    {
                        //Gli hidden fields li tengo in un array separato, serve per poter unparsare eventuali Display inserite
                        //a mano dall'utente
                        hiddenFields.Add(aField);
                    }
                }
			}
			while (lex.Matched(Token.COMMA));
			return true;
		}

		//---------------------------------------------------------------------------
		public bool ParseDiplayTableRow(Parser lex, RepSymTable symbolTable)
		{
			DisplayTable displayTable = engine.ParseDisplayTable(lex);
			if (displayTable == null)
				return false;

			tableName = displayTable.PublicName;

			if (engine.OnFormFeedAction)
			{
				lex.SetError(string.Format(WoormEngineStrings.IllegalFormFeedOnRowDisplay, displayTable.PublicName));
				return false;
			}

			foreach (Field aField in displayTable.Columns)
			{
				if (!aField.Hidden && !aField.IsColTotal && !aField.IsSubTotal)
				{
					aField.Displayed = true;
					AddField(aField);
				}
			}
			return true;
		}

		//---------------------------------------------------------------------------
		public bool ParseDisplayFreeFields(Parser lex, RepSymTable symbolTable)
		{
			// vengono visualizzati solo i campi liberi (i totali di colonna no)
			foreach (Field aField in symbolTable.Fields)
				if (!aField.Hidden && !aField.IsColumn)
				{
					aField.Displayed = true;
					AddField(aField);
				}

			return true;
		}

		//---------------------------------------------------------------------------
		public bool Parse(Token actToken, Parser lex, RepSymTable symbolTable)
		{
			switch (actToken)
			{
				case Token.DISPLAY :
				{
					actionToken = Token.DISPLAY;
					
					if (!ParseDisplay(lex, symbolTable)) 
						return false;

					break;
				}
				case Token.DISPLAY_TABLE_ROW :
				{
					actionToken = Token.DISPLAY_TABLE_ROW;
					
					if (!ParseDiplayTableRow(lex, symbolTable)) 
						return false;

					break;
				}
				case Token.DISPLAY_FREE_FIELDS :
				{
					actionToken = Token.DISPLAY_FREE_FIELDS;
					if (!ParseDisplayFreeFields(lex, symbolTable))
						return false;

					break;
				}
			}

			if (lex.Error)  return false;
			return lex.ParseSep();
		}

		//---------------------------------------------------------------------------
		public override bool Unparse(Unparser unparser)
		{
			//TODOLUCA, questo non sono sicuro che serva, in cpp qui ci passa solo quando serve, in c# ci passa
			//più spesso
			if (actionToken == Token.NOTOKEN)
				return true;

			if (actionToken == Token.DISPLAY_TABLE_ROW)
			{
				unparser.WriteTag(Token.DISPLAY_TABLE_ROW, false);

				if (engine.RepSymTable.DisplayTables.Count >1 && !tableName.IsNullOrEmpty())
					unparser.WriteID(tableName, false);

				unparser.WriteSep(true);
				return true;
			}

			if (actionToken == Token.DISPLAY_FREE_FIELDS)
			{
				unparser.WriteTag(Token.DISPLAY_FREE_FIELDS, false);
				unparser.WriteSep(true);
				return true;
			}

			if (fields.Count > 0)
			{
				unparser.WriteTag(Token.DISPLAY, false);

				foreach (Field item in fields)
				{
					unparser.WriteID(item.Name, false);

					if (item != fields[fields.Count - 1])
						unparser.WriteComma(false);
				}
				unparser.WriteSep(true);
			}

			if (hiddenFields.Count > 0)
			{
				unparser.WriteTag(Token.DISPLAY, false);

				foreach (Field hitem in hiddenFields)
				{
					unparser.WriteID(hitem.Name, false);
					if (hitem.Hidden)	//se non ha cambiato stato per editing dopo la lettura
						unparser.Write(" /* HIDDEN */");
					if (hitem != hiddenFields[hiddenFields.Count - 1])
						unparser.WriteComma(false);
				}
				unparser.WriteSep(true);
			}
			return true;
		}

		//---------------------------------------------------------------------------
		public bool ExistColumnOf	(DisplayTable pDT)
		{
			foreach(Field rf in fields)
			{
				DisplayTable pOwnDT = rf.DisplayTable;
				if (pOwnDT != null && pOwnDT.InternalId == pDT.InternalId)
					return true;
			}

			return false;
		}
	}

	/// <summary>
	/// FormFeedAction
	/// </summary>
	//============================================================================
	public class FormFeedAction : ActionObj
	{
		private bool calledFromOnTable;
		private string layout = string.Empty;
        private bool forced;

		//---------------------------------------------------------------------------
        public FormFeedAction(ActionObj par, ReportEngine engine, RepSymTable symTable)
            : base(par, engine, symTable)
		{
			calledFromOnTable = engine.OnTableAction;
		}

		//---------------------------------------------------------------------------
		public override bool Exec()
		{
            ActionState = ActionStates.STATE_NORMAL;

            if (
                    engine.Status != ReportEngine.ReportStatus.Body &&
                    engine.Status != ReportEngine.ReportStatus.Before &&
                    !calledFromOnTable &&
                    !forced
                )
				return true;

			if (engine.OutChannel == null)
				return false;

			EventActions onFFActions = engine.OnFFActions;

			if (onFFActions != null && !onFFActions.BeforeActions.Exec())
				return false;

			RepSymTable symTable = engine.RepSymTable;

			Field f = symTable.Fields.Find(SpecialReportField.NAME.PAGE);
			if (f != null)
			{
				int np = (int)f.Data;
				f.SetAllData(engine.OutChannel.PageNumber + 1, true);
			}

			Field fLayout = symTable.Fields.Find(SpecialReportField.NAME.LAYOUT);
			if (fLayout != null && !string.IsNullOrEmpty(layout))
			{
				fLayout.SetAllData(layout, true);
			}

			// informa che è finita la pagina e ne inizia un'altra
			engine.OutChannel.NextPage();

			// azzera i contatori di riga corrente per tutte le DisplayTable
			foreach (DisplayTable dt in symTable.DisplayTables)
				dt.ResetRowsCounter();

			if (fLayout != null)
			{
				//reattach della corrente display table ai field
				symTable.ReattachCurrentDisplayTable((string)fLayout.Data);
			}

			return onFFActions == null || onFFActions.AfterActions.Exec();
		}
		
		//---------------------------------------------------------------------------
		public bool Parse(Parser lex)
		{
            forced = lex.Matched(Token.FORCE);

			if (lex.LookAhead(Token.TEXTSTRING) && !lex.ParseString(out layout))
				return false;

			return lex.ParseSep();
		}

		//---------------------------------------------------------------------------
		public override bool Unparse(Unparser unparser)
		{
			unparser.WriteTag(Token.FORMFEED, false);

            if (forced)
                unparser.WriteTag(Token.FORCE, false);

			if (!layout.IsNullOrEmpty())
				unparser.WriteString(layout, false);

			unparser.WriteSep(true);
			return true;
		}
	}

	/// <summary>
	/// DisplayTableAction
	/// </summary>
	//============================================================================
	public class DisplayTableAction : ActionObj
	{
		private RdeWriter.Command		rdeCommand = RdeWriter.Command.NextLine;
		private DisplayTable			displayTable = null;   // property of RepSymTable
        private Expression              subTitleExpr = null;
        //---------------------------------------------------------------------------
        public DisplayTableAction
			(
                ActionObj par, 
                ReportEngine engine, 
                RepSymTable         symTable,
				RdeWriter.Command	rdeCommand,
                DisplayTable        displayTable,
                Expression          customTitle = null
            )
            : base(par, engine, symTable)
		{
			this.rdeCommand		= rdeCommand;
			this.displayTable	= displayTable;
            this.subTitleExpr    = customTitle;
        }

		//---------------------------------------------------------------------------
		public override bool Exec()
		{
			//Skip se current layout != da quello della display table
			//dovuto gestione stessa tabella con numero di righe diverse su layout diversi
			Field currentLayout = engine.RepSymTable.Fields.Find(SpecialReportField.NAME.LAYOUT);
			if (currentLayout != null)
			{
				if (string.Compare((string)currentLayout.Data, displayTable.LayoutTable) != 0)
					return true;
			}
 
            if (rdeCommand == RdeWriter.Command.SubTitleLine)
            {
                if (this.subTitleExpr == null) 
                    return true;

                Value subTtitle = subTitleExpr.Eval();

                if (subTitleExpr.Error)
                {
                    engine.SetError(subTitleExpr.Diagnostic, WoormEngineStrings.EvalEventExpression);
                    return false;
                }

                displayTable.DataDisplayed = true;

                return displayTable.WriteLine(engine, rdeCommand, subTtitle.Data);
            }

            if (rdeCommand == RdeWriter.Command.TitleLine)
                displayTable.DataDisplayed = true;

            return displayTable.WriteLine(engine, rdeCommand);
		}

		//---------------------------------------------------------------------------
		public override bool Unparse(Unparser unparser)
		{
			switch (rdeCommand)
			{
				case RdeWriter.Command.NextLine:
					unparser.WriteTag(Token.NEXTLINE, false);
					break;
				case RdeWriter.Command.TitleLine:
					unparser.WriteTag(Token.TITLELINE, false);
					break;
                case RdeWriter.Command.SubTitleLine:
                    unparser.WriteTag(Token.SUBTITLELINE, false);
                    //TODO manca espressione
                    break;
                case RdeWriter.Command.Interline:
					unparser.WriteTag(Token.INTERLINE, false);
					break;
				default:
					return true;	//TODO segnalare errore 
			}

			if (engine.RepSymTable.DisplayTables.Count > 1)
			{
				unparser.WriteID(this.displayTable.PublicName, false);
			}

			unparser.WriteTag(Token.SEP, true);

			return true;
		}
	}

	/// <summary>
	/// CallAction
	/// </summary>
	//============================================================================
	public class CallAction : ActionObj
	{
		private Procedure procedure;

		//---------------------------------------------------------------------------
        public CallAction(ActionObj par, ReportEngine engine, RepSymTable symTable)
            : base(par, engine, symTable)
		{}

		//---------------------------------------------------------------------------
		public override bool Exec()
		{
            ActionState = ActionStates.STATE_NORMAL;

            return procedure.Exec();
		}

		//---------------------------------------------------------------------------
		public bool Parse(Parser lex)
		{
			string name;
			if (!lex.ParseID(out name)) 
                return false;

			if ((procedure = GetSymTable().Procedures.Find(name)) == null)
			{
				procedure = new Procedure(engine);
                procedure.PublicName = name;
				GetSymTable().Procedures.Add(procedure);
			}

			return lex.ParseSep();
		}

		//---------------------------------------------------------------------------
		public override bool Unparse(Unparser unparser)
		{
			unparser.WriteTag(Token.CALL, false);
			unparser.WriteID(procedure.PublicName, false); 

			unparser.WriteSep(true);
			return true;
		}
	}

	/// <summary>
	/// AskDialogAction
	/// </summary>
	//============================================================================
	public class AskDialogAction : ActionObj
	{
		private string	name;

		//---------------------------------------------------------------------------
        public AskDialogAction(ActionObj par, ReportEngine engine, RepSymTable symTable)
            : base (par, engine, symTable)
		{}

		// Non viene più supportato il costrutto ASK nelle action per le dialog On Ask
		// In compilazione accetto la sintassi per poter utilizzare il motore anche
		// nel localizer di Perasso
		//---------------------------------------------------------------------------
		public override bool Exec()
		{
            ActionState = ActionStates.STATE_NORMAL;

            engine.SetError(WoormEngineStrings.UnsupportedOnAskDialog);
			return false;
		}

		//---------------------------------------------------------------------------
		public bool Parse(Parser lex)
		{
			return lex.ParseID(out name) && lex.ParseSep();
		}

		//---------------------------------------------------------------------------
		public override bool Unparse(Unparser unparser)
		{
			unparser.WriteTag(Token.ASK, false);
			unparser.WriteID(name, false); 

			unparser.WriteSep(true);
			return true;
		}
	}

	/// <summary>
	/// MessageBoxAction
	/// </summary>
	//============================================================================
	public class MessageBoxAction : ActionObj
	{
		private WoormEngineExpression	message = null;
		private Token			actionToken;
  
		//---------------------------------------------------------------------------
        public MessageBoxAction(ActionObj par, ReportEngine engine, RepSymTable symTable)
            : base(par, engine, symTable)
		{
            this.message = new WoormEngineExpression(engine, Session, GetSymTable().Fields);
        }

		// Gestione approssimata della MessageBox perchè siamo in ambiente disconnesso 
		// e non deve essere il motore a fare la MessageBox ma il client. Non è possibile 
		// però sospendere il motore in attesa di una risposta del client e quindi o 
		// interrompo e avviso il client tramite rde oppure faccio dare solo un messaggio
		// che non interrompe (sempre dal client).
		//---------------------------------------------------------------------------
		public override bool Exec()
		{
            ActionState = ActionStates.STATE_NORMAL;

            Value v = message.Eval();
			
			if (message.Error)
			{
				engine.SetError(message.Diagnostic, WoormEngineStrings.EvalEventExpression);
				return false;
			}

			string msg = (string) v.Data;
            if (actionToken == Token.ABORT)
			{
				engine.StopEngine(msg);
				return false;
			}

			return engine.OutChannel.WriteMessageBox(RdeWriter.Command.Message, msg);
		}

		//---------------------------------------------------------------------------
		public bool Parse(Parser lex)
		{
			actionToken = lex.SkipToken();
			switch (actionToken)
			{
				case Token.MESSAGE:
					actionToken = Token.MESSAGE;
					break;

				case Token.ABORT:
					actionToken = Token.ABORT;
					break;

				case Token.DEBUG:
					actionToken = Token.DEBUG;
					break;
			}

			bool ok = message.Compile(lex, CheckResultType.Match, "String");
			return ok && lex.ParseSep();
		}

		//---------------------------------------------------------------------------
		public override bool Unparse(Unparser unparser)
		{
			switch (actionToken)
			{
				case Token.MESSAGE:
				case Token.ABORT:
				case Token.DEBUG:
					if (!message.IsEmpty)
					{
						unparser.WriteTag(actionToken, false);
						unparser.WriteExpr(message.ToString(), false);
					}
					break;
				case Token.QUIT:
					unparser.WriteTag(Token.QUIT, false);
					break;
			}
			
			unparser.WriteSep(true);
			return true;
		}
	}

    /// <summary>
    /// Quit,Brak,Continue
    /// </summary>
    //============================================================================
    public class QuitBreakContinueAction : ActionObj
    {
        private bool isQuit = false;
        private bool isBreak = false;
        private bool isContinue = false;
		private Token actionToken;

        //---------------------------------------------------------------------------
        public QuitBreakContinueAction(ActionObj par, ReportEngine engine, RepSymTable symTable)
            : base (par, engine, symTable)
        {
        }

        // Gestione approssimata della MessageBox perchè siamo in ambiente disconnesso 
        // e non deve essere il motore a fare la MessageBox ma il client. Non è possibile 
        // però sospendere il motore in attesa di una risposta del client e quindi o 
        // interrompo e avviso il client tramite rde oppure faccio dare solo un messaggio
        // che non interrompe (sempre dal client).
        //---------------------------------------------------------------------------
        public override bool Exec()
        {
            ActionState = ActionStates.STATE_NORMAL;

            if (isQuit)
            {
                engine.StopEngine(string.Empty);

                ActionState = ActionStates.STATE_QUIT;
                return true;
            }
            if (isBreak)
            {
                ActionState = ActionStates.STATE_BREAK;
                return true;
            }
            if (isContinue)
            {
                ActionState = ActionStates.STATE_CONTINUE;
                return true;
            }
            return false;
        }

        //---------------------------------------------------------------------------
        public bool Parse(Parser lex)
        {
			
            if (lex.Matched(Token.QUIT))
            {
				actionToken = Token.QUIT;
                isQuit = true;
            }
            else if (lex.Matched(Token.BREAK))
            {
				actionToken = Token.BREAK;
				isBreak = true;
            }
            else if (lex.Matched(Token.CONTINUE))
            {
				actionToken = Token.CONTINUE;
				isContinue = true;
            }
            else 
                return false;

            return lex.ParseSep();
        }

		//---------------------------------------------------------------------------
		public override bool Unparse(Unparser unparser)
		{
			switch (actionToken)
			{				
				case Token.BREAK:
					unparser.WriteTag(Token.BREAK, false);
					break;
				case Token.CONTINUE:
					unparser.WriteTag(Token.CONTINUE, false);
					break;
				case Token.QUIT:
					unparser.WriteTag(Token.QUIT, false);
					break;
			}

			unparser.WriteSep(true);

			return true;
		}
    }

    /// <summary>
    /// ReturnAction
    /// </summary>
    //============================================================================
    public class ReturnAction : ActionObj
    {
        private WoormEngineExpression returnExpr = null;
  
        //---------------------------------------------------------------------------
        public ReturnAction(ActionObj par, ReportEngine engine, RepSymTable symTable, FunctionPrototype fp)
            : base (par, engine, symTable, fp)
        {
        }

        //---------------------------------------------------------------------------
        public override bool Exec()
        {
            ActionState = ActionStates.STATE_NORMAL;

            if (returnExpr != null)
            {
                Value ret = returnExpr.Eval();

                if (returnExpr.Error)
                {
                    engine.SetError(returnExpr.Diagnostic, WoormEngineStrings.EvalEventExpression);
                    return false;
                }

                Fun.ReturnValue = ret;
            }

            ActionState = ActionStates.STATE_RETURN;
            return true;
        }

        //---------------------------------------------------------------------------
        public bool Parse(Parser lex)
        {
            bool ok = lex.ParseTag(Token.RETURN);

            if (ok && lex.LookAhead() != Token.SEP)
            {
                FunctionPrototype fp = this.Fun;
                if (fp == null || fp.ReturnTbType.CompareNoCase("void"))
                {
                    return false;
                }

                returnExpr = new WoormEngineExpression(engine, Session, GetSymTable().Fields);
                ok = returnExpr.Compile(lex, CheckResultType.Compatible, fp.ReturnTbType);
            }

            return ok && lex.ParseSep();
        }

		//---------------------------------------------------------------------------
		public override bool Unparse(Unparser unparser)
		{
			unparser.WriteTag(Token.RETURN, false);
 			
			if (!returnExpr.IsEmpty)
				unparser.WriteExpr(returnExpr.ToString(), false);

			unparser.WriteSep(true);
			return true;
		}
    }

    /// <summary>
    /// DoAction
    /// </summary>
    //============================================================================
    public class DoExprAction : ActionObj
    {
        private WoormEngineExpression expr = null;

        //---------------------------------------------------------------------------
        public DoExprAction(ActionObj par, ReportEngine engine, RepSymTable symTable)
            : base (par, engine, symTable)
        {

        }

        //---------------------------------------------------------------------------
        public override bool Exec()
        {
            ActionState = ActionStates.STATE_NORMAL;

            if (expr != null)
            {
                Value v = expr.Eval();

                if (expr.Error)
                {
                    engine.SetError(expr.Diagnostic, WoormEngineStrings.EvalEventExpression);
                    return false;
                }
            }

            return true;
        }

        //---------------------------------------------------------------------------
        public bool Parse(Parser lex)
        {
            bool ok = lex.ParseTag(Token.DO);

            if (ok && lex.LookAhead() != Token.SEP)
            {
                this.expr = new WoormEngineExpression(engine, Session, GetSymTable().Fields);
                //expr.ForceSkipTypeChecking = true;
                ok = expr.Compile(lex, CheckResultType.Compatible, "Variant");
            }
            return ok && lex.ParseSep();
        }

		//---------------------------------------------------------------------------
		public override bool Unparse(Unparser unparser)
		{
			unparser.WriteTag(Token.DO, false);
			unparser.WriteExpr(expr.ToString(), false);
			unparser.WriteSep(true);
			
			return true;
		}
    }

    /// <summary>
    /// DeclareAction
    /// </summary>
    //============================================================================
    public class DeclareAction : ActionObj
    {
        private WoormEngineExpression initExpr = null;
        private Field localField = null;
        private Block scopeBlock = null;

        //---------------------------------------------------------------------------
        public DeclareAction(ActionObj par, ReportEngine engine, RepSymTable symTable, Block scope)
            : base(par, engine, symTable)
        {
            scopeBlock = scope;
        }

        //---------------------------------------------------------------------------
        public override bool Exec()
        {
            ActionState = ActionStates.STATE_NORMAL;

            if (initExpr != null)
            {
                Value v = initExpr.Eval();

                if (initExpr.Error)
                {
                    engine.SetError(initExpr.Diagnostic, WoormEngineStrings.EvalEventExpression);
                    return false;
                }

                localField.Data = v.Data;
            }

            return true;
        }

        //---------------------------------------------------------------------------
        public bool Parse(Parser lex)
        {
            localField = Field.ParseFieldInfo(lex, GetSymTable(), engine);
            if (localField == null)
                return false;

            scopeBlock.AddLocalField(localField);

            if (lex.Matched(Token.ASSIGN))
            {
                localField.AddMethods(lex, engine);

                this.initExpr = new WoormEngineExpression(engine, Session, GetSymTable().Fields);
                if (!initExpr.Compile(lex, CheckResultType.Compatible, localField.DataType))
                    return false;
            }
            return lex.ParseSep();
        }

		//---------------------------------------------------------------------------
		public override bool Unparse(Unparser unparser)
		{
			localField.UnparseDataType(unparser, false);

			unparser.WriteID(localField.Name, false);

			if (!initExpr.IsEmpty)
			{
				unparser.WriteTag(Token.ASSIGN, false);
				unparser.WriteExpr(initExpr.ToString(), false);
			}

			unparser.WriteSep(true);

			return true;
		}
    }

    /// <summary>
    /// MessageBoxAction
    /// </summary>
    //============================================================================
    public class DisplayChartAction : ActionObj
    {
        private string ChartName;

        //---------------------------------------------------------------------------
        public DisplayChartAction(ActionObj par, ReportEngine engine, RepSymTable symTable)
            : base(par, engine, symTable)
        {
        }

        // Gestione approssimata della MessageBox perchè siamo in ambiente disconnesso 
        // e non deve essere il motore a fare la MessageBox ma il client. Non è possibile 
        // però sospendere il motore in attesa di una risposta del client e quindi o 
        // interrompo e avviso il client tramite rde oppure faccio dare solo un messaggio
        // che non interrompe (sempre dal client).
        //---------------------------------------------------------------------------
        public override bool Exec()
        {
            ActionState = ActionStates.STATE_NORMAL;

            //TODO CHART filtrare array bindati al chart name

            foreach (Field f in this.GetSymTable().Fields)
            {
                if (f.WoormType != "Array" && f.WoormType != "DataArray")
                    continue;

                //DataArray ar = f.Data as DataArray;
                f.WriteArray(this.engine);
            }

            return true;
        }

        //---------------------------------------------------------------------------
        public bool Parse(Parser lex)
        {
            if (!lex.ParseTag(Token.DISPLAY_CHART))
            {
                return false;
            }
            if (!lex.ParseID(out ChartName))
            {
                return false;
            }

            return lex.ParseSep();
        }

        //---------------------------------------------------------------------------
        public override bool Unparse(Unparser unparser)
        {
            unparser.WriteTag(Token.DISPLAY_CHART, false);
            unparser.WriteID(ChartName);
            unparser.WriteSep(true);

            return true;
        }
    }

    /// <summary>
    /// ReportActions
    /// </summary>
    //============================================================================
    public class ReportEventActions : EventActions
	{
		private	Block	alwaysBlock;
		private	Block	finalizeBlock;

		//---------------------------------------------------------------------------
		public ReportEventActions (ReportEngine engine): base(engine)
		{
			alwaysBlock = new Block(null, engine);
			finalizeBlock = new Block(null, engine);
		}

		//---------------------------------------------------------------------------
		public Block AlwaysActions { get { return alwaysBlock;	}}
		public Block FinalizeActions { get { return finalizeBlock;	}}

		//---------------------------------------------------------------------------
		public bool AlwaysParse(Parser lex)
		{
			if (lex.Matched(Token.ALWAYS))
			{
				if (!alwaysBlock.Parse(lex))
					return false;

				engine.ExplicitDisplayNotAllowed = engine.DisplayAction;
			}

			return true;
		}

		// Siccome la Finalize viene compilata dopo la Always devo ignorare il fatto
		// che quest'ultima abbia torvato una DisplayAction perchè in quel caso è lecita
		// metre se la trova nella Finalize non è ammessa
		//---------------------------------------------------------------------------
		public bool FinalizeParse(Parser lex)
		{
			if (lex.Matched(Token.FINALIZE))
			{
				bool prev = engine.DisplayAction;
				engine.DisplayAction = false;

				if (!finalizeBlock.Parse(lex))
					return false;

				engine.ExplicitDisplayNotAllowed = engine.ExplicitDisplayNotAllowed || engine.DisplayAction;
				engine.DisplayAction = prev;
			}

			return true;
		}

		//---------------------------------------------------------------------------
		public bool Parse(Parser lex)
		{
			return 
				AlwaysParse(lex) &&
				ParseEventActions(lex) &&
				FinalizeParse(lex);
		}

		//---------------------------------------------------------------------------
		public override bool Unparse(Unparser unparser)
		{
			if (IsEmpty)
				return true;

			unparser.WriteTag(Token.REPORT,	false);
			unparser.WriteTag(Token.COLON, false);
			unparser.WriteTag(Token.DO, true);

			if (!alwaysBlock.IsEmpty)
			{
				unparser.IncTab();
				unparser.WriteTag(Token.ALWAYS, false);

				unparser.IncTab();
				alwaysBlock.Unparse(unparser);
				unparser.WriteLine();
				unparser.DecTab();

				unparser.DecTab();
			}

			base.Unparse(unparser);

			if (!finalizeBlock.IsEmpty)
			{
				unparser.IncTab();
				unparser.WriteTag(Token.FINALIZE, false);

				unparser.IncTab();
				finalizeBlock.Unparse(unparser);
				unparser.WriteLine();
				unparser.DecTab();

				unparser.DecTab();
			}
			return true;
		}

		//---------------------------------------------------------------------------
		public override bool IsEmpty
		{
			get
			{
				return (alwaysBlock.IsEmpty && finalizeBlock.IsEmpty && base.IsEmpty );
			}
		}
	}

	/// <summary>
	/// TriggeredEvent
	/// </summary>
	//============================================================================
	public class TriggeredEventActions : EventActions
	{
		private bool		occurred;
		private List<Field> breakList = new List<Field>();	// array of Field to check break
		private Token		boolOperator;					// Token.AND | Token.OR
		private WoormEngineExpression	whenExpression;
		private string name = string.Empty;

		//---------------------------------------------------------------------------
		public string Name { get { return name; } set { name = value; } }

		//---------------------------------------------------------------------------
		public TriggeredEventActions (ReportEngine engine) : base(engine)
		{
			occurred		= true;
			boolOperator	= Token.NOTOKEN;
			whenExpression	= new WoormEngineExpression(engine, Session, engine.RepSymTable.Fields);
		}

		//---------------------------------------------------------------------------
        private bool EvalWhen(bool qry)
		{
            if (qry) 
                engine.SetLevel(DataLevel.GroupBy);

			Value v = whenExpression.Eval();
			bool  ok = true;;

			if (whenExpression.Error)
			{
				engine.SetError(whenExpression.Diagnostic, WoormEngineStrings.EvalWhenExpression);
				ok = false;
			}
			else ok = (bool) v.Data;

			engine.SetLevel(DataLevel.Events);
			return ok;
		}

		//---------------------------------------------------------------------------
		public bool Check (bool qry = true)
		{        
			occurred = true;
				
			if ((breakList.Count == 0) || (boolOperator != Token.NOTOKEN))
			{
				// if there is a WHEN condition then eval the when expression
				// for all rows excluding the last one
				//
                if (engine.Status != ReportEngine.ReportStatus.LastRow || !whenExpression.HasRuleFields)
				{
					occurred = EvalWhen(qry);
				
					if (!occurred && engine.Diagnostic.Error)
						return false;
				}
				else
					occurred = false;
			}

			if (breakList.Count != 0)
			{
				bool brk = false;

				if (engine.Status != ReportEngine.ReportStatus.Body)
					brk = true;
				else
					foreach (Field rf in breakList)
					{
						brk = rf.IsGroupByDataChanged;
						if (brk) break;
					}

				switch (boolOperator)
				{
					case Token.AND  	:	occurred = occurred && brk;   break;
					case Token.OR   	:	occurred = occurred || brk;   break;
					case Token.NOTOKEN	:	occurred = brk; break;
					default				:	occurred = false; break;
				}
			}

			return true;
		}

		//---------------------------------------------------------------------------
		public bool DoBeforeActions()
		{
			return occurred ? BeforeActions.Exec() : true;
		}

		//---------------------------------------------------------------------------
		public bool DoAfterActions()
		{
			bool ok = true;
			if (occurred) ok = AfterActions.Exec();

			occurred = false;
			return ok;
		}

		//---------------------------------------------------------------------------
		public bool Parse(Parser lex)
		{
			bool whenFound = false;

			if (lex.Matched(Token.BREAKING))
			{
				string strBuffer;
				do
				{
					if (!lex.ParseID(out strBuffer)) return false;

					Field aField = engine.RepSymTable.Fields.Find(strBuffer);
					if (aField == null)
					{
						lex.SetError(string.Format(ExpressionManagerStrings.UnknownField, strBuffer));
						return false;
					}

					if (!aField.OwnRule)
					{
						lex.SetError(string.Format(WoormEngineStrings.IllegalField, strBuffer));
						return false;
					}

					breakList.Add(aField);
				}
				while (lex.Matched(Token.COMMA));

				if (lex.Error) return false;
				switch (lex.LookAhead())
				{
					case Token.AND  :   lex.SkipToken(); boolOperator = Token.AND; break;
					case Token.OR   :   lex.SkipToken(); boolOperator = Token.OR; break;
					default     :   if (lex.Error)
										return false;
									else
										boolOperator = Token.NOTOKEN;
									break;
				}

				if (boolOperator != Token.NOTOKEN)
				{
					lex.Matched(Token.WHEN); // optional WHEN keyword after AND/OR operator
					whenFound = true;       // anyway an expression comes after
				}
			}
			else
				whenFound = lex.ParseTag(Token.WHEN);

			if (lex.Error) return false;

			if (whenFound)
			{
				whenExpression.StopTokens = new StopTokens(new Token[] { Token.DO });
				if (!(whenExpression.Compile(lex, CheckResultType.Match, "Boolean")))
					return false;
			}

			return lex.ParseTag(Token.DO) && ParseEventActions(lex);
		}

		//---------------------------------------------------------------------------
		public override bool Unparse(Unparser unparser)
		{
			if (IsEmpty)
				return true;

			unparser.WriteID(this.name, false);
			unparser.WriteTag(Token.COLON, false);

			if (breakList.Count > 0)
			{
				unparser.WriteTag(Token.BREAKING, false);
				unparser.WriteID(breakList[0].Name, false);

				for (int i = 1; i < breakList.Count; i++)
				{
					unparser.WriteComma(false);
					unparser.WriteBlank();
					unparser.WriteID(breakList[i].Name, false);
				}

				if (whenExpression != null && !whenExpression.IsEmpty)
				{
					unparser.WriteTag(boolOperator, false);
					unparser.WriteTag(Token.WHEN, false);
					unparser.WriteExpr(whenExpression.ToString(), false);
				}
			}
			else
			{
				if (whenExpression != null && !whenExpression.IsEmpty)
				{
					unparser.WriteTag(Token.WHEN, false);
					unparser.WriteExpr(whenExpression.ToString(), false);
				}
			}

			unparser.WriteBlank();
			unparser.WriteTag(Token.DO, true);

			base.Unparse(unparser);

			unparser.WriteLine();
			return true;
		}
	}
}

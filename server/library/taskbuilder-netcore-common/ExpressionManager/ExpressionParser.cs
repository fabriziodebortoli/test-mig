using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Diagnostics;

using Microarea.Common.Generic;
using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.Common.Lexan;

namespace Microarea.Common.ExpressionManager
{
    //-----------------------------------------------------------------------------
    // An expression is stored in a ready-to-evaluate form as a stack of
    // items in prefix notation. The hierarchy for Item class is the following:
    //
    //	(split in CoreType/ExpressionItems)
    //	Item				an expression item can be ...
    //	  |
    //	  *---- Operator		an operator,
    //	  |
    //	  *---- Variable		a reference to a variable symbol,
    //	  |
    //	  *---- Function		a built-in and external implemented function call,
    //	  |
    //	  *---- Value		a costant value
    //
    //-----------------------------------------------------------------------------

    //============================================================================
    public class OperatorItem : Item
    {
        public enum OperatorType { Unary, Binary, Ternary, Logical };

        public Token OperatorID;

        protected string resultType;

        public string ResultType
        {
            get { return resultType; }
            set { resultType = value; }
        }

        public Stack<Item> FirstStack = new Stack<Item>();
        public Stack<Item> SecondStack = new Stack<Item>();
 
        //-----------------------------------------------------------------------------
        public OperatorItem(Token op) : base()
        {
            OperatorID = op;
            resultType = null;
        }

        //-----------------------------------------------------------------------------
        public OperatorItem(Token op, string type) : base()
        {
            Debug.Assert(type != null && type != string.Empty, "OperatorItem has null result type");

            OperatorID = op;
            resultType = type;
        }

        //-----------------------------------------------------------------------------
        public override object Clone()
        {
            OperatorItem op = new OperatorItem(OperatorID, ResultType);

            op.FirstStack = FirstStack.Clone();
            op.SecondStack = SecondStack.Clone();

            return op;
        }

        //-----------------------------------------------------------------------------
        public override Item Expand()
        {
            OperatorItem op = new OperatorItem(OperatorID, ResultType);

            ExpressionParser.ExpandStack(FirstStack, ref op.FirstStack);
            ExpressionParser.ExpandStack(SecondStack, ref op.SecondStack);

            return op;
        }

        //-----------------------------------------------------------------------------
        public override int GetHashCode()
        {
            return (int)OperatorID;
        }

        //-----------------------------------------------------------------------------
        public override bool Equals(object item)
        {
            if (!(item is OperatorItem))
                return false;

            OperatorItem itemOperator = item as OperatorItem;

            return
                OperatorID == itemOperator.OperatorID &&
                OperatorItem.Equals(this.FirstStack, itemOperator.FirstStack) &&
                OperatorItem.Equals(this.SecondStack, itemOperator.SecondStack);
        }

        //-----------------------------------------------------------------------------
        public OperatorType GetOperatorType()
        {
            if (
                OperatorID == Token.EXPR_IS_NULL ||
                OperatorID == Token.EXPR_IS_NOT_NULL ||
                OperatorID == Token.EXPR_UNNARY_MINUS ||
                OperatorID == Token.NOT ||
                OperatorID == Token.OP_NOT ||
                OperatorID == Token.BW_NOT
                )
                return OperatorType.Unary;

            if (
                OperatorID == Token.OR ||
                OperatorID == Token.OP_OR ||
                OperatorID == Token.AND ||
                OperatorID == Token.OP_AND ||
                OperatorID == Token.BETWEEN ||
                OperatorID == Token.QUESTION_MARK
                )
                return OperatorType.Logical;

            if (OperatorID == Token.EXPR_ESCAPED_LIKE)
                return OperatorType.Ternary;

            return OperatorType.Binary;
        }
    }

    //============================================================================
    public class FunctionItem : Item
    {
        protected string name;
        protected FunctionPrototype prototype = null;
        public bool HasLateBinding = false;

        protected int parametersCount = 0;

        public int CurrentParametersCount
        {
            get
            {
                if (prototype == null)
                    return parametersCount;
                if (parametersCount == 0)
                    return prototype.Parameters.Count;
                return parametersCount;
            }
            set { parametersCount = value; }
        }

        virtual public string Name { get { return name; } }
        virtual public string FullName { get { return prototype != null ? prototype.FullName : name; } }

        virtual public FunctionPrototype Prototype { get { return prototype; } set { prototype = value; } }
        virtual public List<Parameter> Parameters { get { return prototype != null ? prototype.Parameters : null; } }

        virtual public string ReturnType { get { return prototype != null ? prototype.ReturnType : "Variant"; } }
        virtual public string ReturnBaseType { get { return prototype != null ? prototype.ReturnBaseType : "Variant"; } }

        //-----------------------------------------------------------------------------
        public FunctionItem(string name, FunctionPrototype prototype, bool hasLateBinding = false, int nrCurrentParameters = 0)
            : base()
        {
            this.name = name;
            this.prototype = prototype;
            this.HasLateBinding = hasLateBinding;
            this.CurrentParametersCount = nrCurrentParameters;
        }

        //-----------------------------------------------------------------------------
        public FunctionItem(Token functionID, FunctionPrototype prototype, int nrCurrentParameters = 0)
            : base()
        {
            this.name = Language.GetTokenString(functionID);
            this.prototype = prototype;
            this.CurrentParametersCount = nrCurrentParameters;
        }

        //-----------------------------------------------------------------------------
        public override object Clone()
        {
            FunctionItem f = new FunctionItem(name, prototype, HasLateBinding, CurrentParametersCount);
            return f;
        }

        //-----------------------------------------------------------------------------
        public override int GetHashCode()
        {
            return this.name.GetHashCode();
        }

        //-----------------------------------------------------------------------------
        public override bool Equals(object item)
        {
            if (!(item is FunctionItem))
                return false;
            FunctionItem itemFunction = item as FunctionItem;

            return
                name == itemFunction.Name &&
                prototype == itemFunction.prototype &&
                HasLateBinding == itemFunction.HasLateBinding &&
                CurrentParametersCount == itemFunction.CurrentParametersCount;
        }

        //-----------------------------------------------------------------------------
        public override string ToString()
        {
            return this.name;
        }
    }

    //============================================================================
    class CTypeFunctionItem : FunctionItem
    {
        private string returnType;
        private List<Parameter> parameters = new List<Parameter>();

        public override List<Parameter> Parameters { get { return parameters; } }

        public override string FullName { get { return name; } }

        public override string ReturnType { get { return returnType; } }
        public override string ReturnBaseType { get { return ""; } }

        //-----------------------------------------------------------------------------
        public CTypeFunctionItem(string returnType)
            : base("CType", null)
        {
            this.returnType = returnType;
            parameters.Add(new Parameter("object", "Object", TaskBuilderNetCore.Interfaces.ParameterModeType.In));
            parameters.Add(new Parameter("type", returnType, TaskBuilderNetCore.Interfaces.ParameterModeType.In));
        }
    }
    //============================================================================
    class ThiscallFunctionItem : FunctionItem
    {
        string objectName;

        public ThiscallFunctionItem(string objectName, string functionName, FunctionPrototype fp)
            : base(functionName, fp)
        {
            this.objectName = objectName;
        }

        public string ObjectName
        {
            get { return objectName; }
            set { objectName = value; }
        }


    }
    //============================================================================
    public class FunctionContentOfItem : FunctionItem
    {
        public Stack<Item> CofExpression;
        public SymbolTable SymbolTable;
        public TbSession Session;

        //-----------------------------------------------------------------------------
        public FunctionContentOfItem(FunctionPrototype prototype, Stack<Item> expression, TbSession session, SymbolTable symbolTable)
            : base(Token.CONTENTOF, prototype)
        {
            CofExpression = expression;
            SymbolTable = symbolTable;
            Session = session;
        }

        //-----------------------------------------------------------------------------
        public override Item Expand()
        {
            Expression e = new Expression(Session, SymbolTable);
            Value val = e.Eval(CofExpression);

            ValueContentOf contentOfVal = new ValueContentOf(val.Data);
            return contentOfVal;
        }

    }

    /// <summary>
    /// Summary description for SymbolTable.
    /// contiene la symbol table delle variabili usate nella valutazione di una espressione
    /// � case insensitive come il Basic o il Pascal
    /// </summary>
    //============================================================================

    //============================================================================
    public class Utility
    {

        //-----------------------------------------------------------------------------
        public static void MoveStack(Stack<Item> fromStack, Stack<Item> toStack) { MoveStack(fromStack, toStack, -1); }
        public static void MoveStack(Stack<Item> fromStack, Stack<Item> toStack, int nItems)
        {
            int n = nItems < 0 ? fromStack.Count : nItems;
            Stack<Item> tmp = new Stack<Item>();

            for (int i = 0; i < n; i++) tmp.Push(fromStack.Pop());
            for (int i = 0; i < n; i++) toStack.Push(tmp.Pop());
        }

        //-----------------------------------------------------------------------------
        public static void ReverseStack(Stack<Item> fromStack, Stack<Item> toStack)
        {
            int n = fromStack.Count;
            for (int i = 0; i < n; i++) toStack.Push(fromStack.Pop());
        }
    }

    //============================================================================
    public class StopTokens : List<Token>
    {
        public bool skipInnerBlock = false;
        public bool skipInnerRoundBrackets = false;
        public bool skipInnerSquareBrackets = false;
        public bool skipInnerBraceBrackets = false;

        protected int countInnerBlock = 0;
        protected int countInnerRoundBrackets = 0;
        protected int countInnerSquareBrackets = 0;
        protected int countInnerBraceBrackets = 0;

        public StopTokens() { }

        public StopTokens(StopTokens a) { Assign(a); }

        public StopTokens(Token[] a)
        {
            Clear();
            for (int i = 0; i < a.GetLength(0); i++)
                Add(a[i]);
        }

        public void Assign(StopTokens a)
        {
            skipInnerBlock = a.skipInnerBlock;
            skipInnerRoundBrackets = a.skipInnerRoundBrackets;
            skipInnerSquareBrackets = a.skipInnerSquareBrackets;
            skipInnerBraceBrackets = a.skipInnerBraceBrackets;

            countInnerBlock = a.countInnerBlock;
            countInnerRoundBrackets = a.countInnerRoundBrackets;
            countInnerSquareBrackets = a.countInnerSquareBrackets;
            countInnerBraceBrackets = a.countInnerBraceBrackets;

            Clear();
            for (int i = 0; i < a.Count; i++)
                Add(a[i]);
        }

        public bool IsStopParse(Token tk)
        {
            if (skipInnerBlock)
            {
                if (tk == Token.BEGIN)
                    countInnerBlock++;
                else if (tk == Token.END)
                    countInnerBlock--;

                if (countInnerBlock > 0)
                    return false;
            }
            if (skipInnerRoundBrackets)
            {
                if (tk == Token.ROUNDOPEN)
                    countInnerRoundBrackets++;
                else if (tk == Token.ROUNDCLOSE)
                    countInnerRoundBrackets--;

                if (countInnerRoundBrackets > 0)
                    return false;
            }
            if (skipInnerSquareBrackets)
            {
                if (tk == Token.SQUAREOPEN)
                    countInnerSquareBrackets++;
                else if (tk == Token.SQUARECLOSE)
                    countInnerSquareBrackets--;

                if (countInnerSquareBrackets > 0)
                    return false;
            }
            if (skipInnerBraceBrackets)
            {
                if (tk == Token.BRACEOPEN)
                    countInnerBraceBrackets++;
                else if (tk == Token.BRACECLOSE)
                    countInnerBraceBrackets--;

                if (countInnerBraceBrackets > 0)
                    return false;
            }

            int num = this.Count;
            for (int i = 0; i < num; i++)
                if (tk == (Token)(this[i]))
                    return true;
            return false;
        }
    }

    //============================================================================
    public class ExpressionParser
    {
        private StopTokens stopTokens = null;
        private SymbolTable symbolTable = null;
        private TbSession session;

        private bool allowThisCallMethods = false;

        //public bool HasExternalFunctionCall = false;
        public bool HasRuleFields = false;

        //aggiunte espressioni per allinearsi con la versione gdi
        public bool hasField = false;
        public bool hasInputFields = false;
        public bool hasAskFields = false;

        public bool hasExternalFunctionCall = false;
        public bool hasDynamicFragment = false;

        public bool vrbCompiled = false;

        /// <summary>
        /// EasyStudio - abilita il parsing di funzioni del tipo "object.method()"
        /// </summary>
        public bool AllowThisCallMethods
        {
            get { return allowThisCallMethods; }
            set { allowThisCallMethods = value; }
        }

        //-----------------------------------------------------------------------------
        public TbSession Session { get { return session; } }

        //-----------------------------------------------------------------------------
        public ExpressionParser(TbSession session) { this.session = session; }

        //-----------------------------------------------------------------------------
        public ExpressionParser(TbSession session, SymbolTable symbolTable, StopTokens stopTokens)
        {
            this.stopTokens = stopTokens;
            this.symbolTable = symbolTable;
            this.session = session;
        }

        //-----------------------------------------------------------------------------
        public bool StopParse(Parser lex)
        {
            Token tk = lex.LookAhead();

            if (lex.Error) return true;
            if (tk == Token.EOF) return true;
            if (tk == Token.SEP) return true;

            // controlla eventuali token di stop gestiti dal programmatore
            if (stopTokens != null)
                return stopTokens.IsStopParse(tk);

            return false;
        }

        //-----------------------------------------------------------------------------
        public bool Parse(Parser lex, Stack<Item> stack)
        {
            if (!StopParse(lex))
                Expression(lex, stack);

            // se non c'� errore allora devo trovare uno token di stop altrimenti
            // � un  errore di sintassi
            if (!lex.Error && !StopParse(lex))
                lex.SetError(ExpressionManagerStrings.SyntaxError);

            return !lex.Error;
        }

        //-----------------------------------------------------------------------------
        void Expression(Parser lex, Stack<Item> stack)
        {
            Disjunctive(lex, stack);

            while (!StopParse(lex))
            {
                switch (lex.LookAhead())
                {
                    case Token.OR:
                    case Token.OP_OR:
                        {
                            OperatorItem op = new OperatorItem(Token.OR);
                            lex.SkipToken();

                            int currentSize = stack.Count;
                            Disjunctive(lex, stack);

                            Utility.MoveStack(stack, op.SecondStack, stack.Count - currentSize);
                            stack.Push(op);
                            break;
                        }

                    case Token.QUESTION_MARK:   // sintassi <x> ? <y> : <z>
                        {
                            OperatorItem op = new OperatorItem(Token.QUESTION_MARK);
                            lex.SkipToken();

                            int currentSize = stack.Count;
                            Expression(lex, stack);

                            Utility.MoveStack(stack, op.FirstStack, stack.Count - currentSize);
                            if (!lex.ParseTag(Token.COLON))
                                return;

                            Expression(lex, stack);

                            Utility.MoveStack(stack, op.SecondStack, stack.Count - currentSize);
                            stack.Push(op);
                            return;
                        }

                    default: return;
                } // switch
            }
        }

        //-----------------------------------------------------------------------------
        void Disjunctive(Parser lex, Stack<Item> stack)
        {
            Conjunctive(lex, stack);

            while (!StopParse(lex))
            {
                switch (lex.LookAhead())
                {
                    case Token.AND:
                    case Token.OP_AND:
                        {
                            OperatorItem op = new OperatorItem(Token.AND);
                            lex.SkipToken();

                            int currentSize = stack.Count;

                            Conjunctive(lex, stack);

                            Utility.MoveStack(stack, op.SecondStack, stack.Count - currentSize);
                            stack.Push(op);
                            break;
                        }

                    default: return;
                } // switch
            }
        }

        //-----------------------------------------------------------------------------
        void Conjunctive(Parser lex, Stack<Item> stack)
        {
            Formula(lex, stack);

            while (!StopParse(lex))
            {
                int nPosNot = lex.Matched(Token.NOT) ? lex.CurrentPos : -1;

                switch (lex.LookAhead())
                {
                    case Token.GT:
                    case Token.LT:
                    case Token.LE:
                    case Token.GE:
                    case Token.NE:
                    case Token.DIFF:
                    case Token.EQ:
                    case Token.ASSIGN:
                    case Token.CONTAINS:
                    case Token.IN:
                        {
                            OperatorItem op = new OperatorItem(lex.LookAhead());
                            lex.SkipToken();
                            Formula(lex, stack);
                            stack.Push(op);

                            if (nPosNot >= 0)
                            {
                                // sintassi <x> [NOT] CONTAINS <y>
                                // sintassi <x> [NOT] IN <y>
                                if (op.OperatorID == Token.CONTAINS || op.OperatorID == Token.IN)
                                    stack.Push(new OperatorItem(Token.NOT));
                                else
                                    lex.SetError(ExpressionManagerStrings.SyntaxError);
                            }
                            break;
                        }
                    case Token.BETWEEN: // sintassi <x> [NOT] BETWEEN <y> AND <z>
                        {
                            OperatorItem op = new OperatorItem(Token.BETWEEN);
                            lex.SkipToken();

                            int currentSize = stack.Count;

                            Formula(lex, stack);
                            Utility.MoveStack(stack, op.FirstStack, stack.Count - currentSize);

                            if (!lex.ParseTag(Token.AND))
                                return;

                            Formula(lex, stack);
                            Utility.MoveStack(stack, op.SecondStack, stack.Count - currentSize);
                            stack.Push(op);

                            if (nPosNot >= 0)
                                stack.Push(new OperatorItem(Token.NOT));

                            break;
                        }
                    case Token.LIKE:        // sintassi <x> [NOT] LIKE <y> [ ESCAPE <z> ]
                        {
                            OperatorItem op = new OperatorItem(Token.LIKE);
                            lex.SkipToken();
                            Formula(lex, stack);

                            if (lex.Matched(Token.ESCAPE))
                            {
                                // accettata solo costante stringa contenente un solo carattere
                                string aString;
                                if (!lex.ParseString(out aString) || aString.Length != 1)
                                {
                                    lex.SetError(ExpressionManagerStrings.SyntaxError);
                                    return;
                                }

                                // boxing del dato
                                stack.Push(new Value((object)aString));

                                // modifica il tipo di operatore
                                op.OperatorID = Token.EXPR_ESCAPED_LIKE;
                            }

                            stack.Push(op);

                            if (nPosNot >= 0)
                                stack.Push(new OperatorItem(Token.NOT));

                            break;
                        }
                    case Token.IS:
                        {
                            lex.SkipToken();
                            Token nPT = Token.EXPR_IS_NULL;

                            if (lex.Matched(Token.NOT))
                                nPT = Token.EXPR_IS_NOT_NULL;

                            if (!lex.ParseTag(Token.NULL)) return;

                            stack.Push(new OperatorItem(nPT));
                            break;
                        }

                    default: return;
                } // switch
            }
        }

        //-----------------------------------------------------------------------------
        void Formula(Parser lex, Stack<Item> stack)
        {
            Term(lex, stack);

            while (!StopParse(lex))
            {
                switch (lex.LookAhead())
                {
                    case Token.PLUS:
                    case Token.MINUS:
                        {
                            OperatorItem op = new OperatorItem(lex.SkipToken());
                            Term(lex, stack);
                            stack.Push(op);
                            break;
                        }
                    default: return;
                } // switch
            }
        }

        //-----------------------------------------------------------------------------
        void Term(Parser lex, Stack<Item> stack)
        {
            Factor(lex, stack);

            while (!StopParse(lex))
            {
                switch (lex.LookAhead())
                {
                    case Token.STAR:
                    case Token.SLASH:
                    case Token.PERC:
                    case Token.EXP:
                    case Token.BW_AND:
                    case Token.BW_XOR:
                    case Token.BW_OR:
                        {
                            OperatorItem op = new OperatorItem(lex.LookAhead());
                            lex.SkipToken();
                            Factor(lex, stack);
                            stack.Push(op);
                            break;
                        }
                    case Token.ID:
                        {
                            //ho trovato un ID che inizia per ".": potrebbe essere una funzione il 
                            //cui oggetto di chiamata (this) � il valore di ritorno dell'espressione precedente
                            if (allowThisCallMethods && lex.CurrentLexeme.StartsWith("."))
                            {
                                Stack<Item> tmpStack = stack.Clone();
                                stack.Clear();
                                Factor(lex, stack);//metto la funzione (o le funzioni) in uno stack pulito

                                //aggiungo le funzioni preesistenti in 
                                //modo da avere il corretto ordine di esecuzione
                                //(prima quelle a sinistra, parsate per prime)
                                Utility.MoveStack(tmpStack, stack);

                                continue;//potrei avere altre funzioni
                            }
                            return;
                        }

                    default: return;
                } // switch
            }
        }

        //-----------------------------------------------------------------------------
        void Factor(Parser lex, Stack<Item> stack)
        {
            if (lex.Error) return;

            switch (lex.LookAhead())
            {
                case Token.MINUS: // unary minus
                    {
                        OperatorItem unm = new OperatorItem(Token.EXPR_UNNARY_MINUS);

                        lex.ParseTag(Token.MINUS);
                        Factor(lex, stack);
                        stack.Push(unm);
                        break;
                    }

                case Token.BW_NOT:
                case Token.NOT:
                    {
                        OperatorItem not = new OperatorItem(lex.LookAhead());

                        lex.SkipToken();
                        Factor(lex, stack);
                        stack.Push(not);
                        break;
                    }

                // questa � la gestione nuova della vecchia ParseComplexData in C++
                case Token.BRACEOPEN:
                    {
                        object anydata = null;
                        if (!ComplexDataParser.Parse(lex, Session.Enums, out anydata))
                            return;
                        stack.Push(new Value(anydata));
                        break;
                    }

                case Token.SQUAREOPEN:
                    {
                        if (!ParseArrayCreate(lex, stack))
                            return;
                        break;
                    }

                case Token.ROUNDOPEN:
                    {
                        lex.ParseTag(Token.ROUNDOPEN);
                        Expression(lex, stack);
                        if (!lex.ParseTag(Token.ROUNDCLOSE))
                            return;
                        break;
                    }

                case Token.BYTE:
                case Token.SHORT:
                case Token.INT:
                    {
                        int aInt;

                        if (!lex.ParseInt(out aInt)) return;
                        stack.Push(new Value((object)aInt));
                        break;
                    }

                case Token.LONG:
                    {
                        long aLong;

                        if (!lex.ParseLong(out aLong)) return;
                        stack.Push(new Value((object)aLong));
                        break;
                    }

                case Token.FLOAT:
                case Token.DOUBLE:
                    {
                        double aDouble;

                        if (!lex.ParseDouble(out aDouble)) return;
                        stack.Push(new Value((object)aDouble));
                        break;
                    }

                case Token.ID:
                    {
                        string name;
                        if (!lex.ParseID(out name))
                            return;

                        if (lex.LookAhead(Token.ROUNDOPEN))
                        {
                            if (!ParseFunction(name, lex, stack)) return;
                            break;
                        }
                        else if (lex.LookAhead(Token.SQUAREOPEN))
                        {
                            if (!ParseArrayIndexer(name, lex, stack)) return;
                            break;
                        }

                        // variable identifier
                        if (this.symbolTable != null)
                        {
                            Variable v = this.symbolTable.Find(name);
                            if (v != null)
                            {
                                this.hasField = true;
                                if (v.IsRuleFields())
                                {
                                    this.HasRuleFields = true;
                                }
                                else if (v.IsAsk())
                                {
                                    this.hasInputFields = true;
                                    this.hasAskFields = true;
                                }
                                else if (v.IsInput())
                                {
                                    this.hasInputFields = true;
                                }
                                stack.Push(v);
                                break;

                            }
                        }
                        stack.Push(new Variable(name));
                        break;
                    }

                case Token.TEXTSTRING:
                    {
                        string aString;

                        if (!lex.ParseString(out aString)) return;
                        stack.Push(new Value((object)aString));
                        break;
                    }

                case Token.TRUE:
                    {
                        stack.Push(new Value((object)true));
                        lex.SkipToken();
                        break;
                    }

                case Token.FALSE:
                    {
                        stack.Push(new Value((object)false));
                        lex.SkipToken();
                        break;
                    }

                case Token.ABS:
                case Token.ASC:
                case Token.CDOW:
                case Token.CEILING:
                case Token.CHR:
                case Token.CMONTH:
                case Token.CTOD:
                case Token.DATE:
                case Token.DATETIME:
                case Token.APPDATE:
                case Token.APPYEAR:
                case Token.DAY:
                case Token.DTOC:
                case Token.FLOOR:
                case Token.LEFT:
                case Token.LEN:
                case Token.LOWER:
                case Token.LTRIM:
                case Token.MAX:
                case Token.MIN:
                case Token.MOD:
                case Token.MONTH:
                case Token.RAND:
                case Token.RIGHT:
                case Token.ROUND:
                case Token.RTRIM:
                case Token.SIGN:
                case Token.SPACE:
                case Token.STR:
                case Token.SUBSTR:
                case Token.SUBSTRWW:
                case Token.TIME:
                case Token.TRIM:
                case Token.UPPER:
                case Token.VAL:
                case Token.YEAR:
                case Token.FINT:
                case Token.FLONG:
                case Token.FORMAT:
                case Token.TYPED_BARCODE:
                case Token.GETBARCODE_ID:
                case Token.MONTH_DAYS:
                case Token.MONTH_NAME:
                case Token.LAST_MONTH_DAY:
                case Token.FIND:
                case Token.LOADTEXT:
                case Token.SAVETEXT:
                case Token.LOCALIZE:
                case Token.ELAPSED_TIME:
                case Token.DAYOFWEEK:
                case Token.WEEKOFMONTH:
                case Token.WEEKOFYEAR:
                case Token.DAYOFYEAR:
                case Token.GIULIANDATE:
                case Token.ISACTIVATED:
                case Token.ISADMIN:
                case Token.REVERSEFIND:
                case Token.REMOVENEWLINE:
                case Token.GETAPPTITLE:
                case Token.GETMODTITLE:
                case Token.GETDOCTITLE:
                case Token.REPLACE:

                case Token.GETCOMPANYNAME:
                case Token.GETCOMPUTERNAME:
                case Token.GETCULTURE:
                case Token.GETDATABASETYPE:
                case Token.GETEDITION:
                case Token.GETINSTALLATIONNAME:
                case Token.GETINSTALLATIONPATH:
                case Token.GETINSTALLATIONVERSION:
                case Token.GETLOGINNAME:
                case Token.GETPRODUCTLANGUAGE:
                case Token.GETUSERDESCRIPTION:
                case Token.GETWINDOWUSER:

                case Token.GETNSFROMPATH:
                case Token.GETPATHFROMNS:

                case Token.GETUPPERLIMIT:
                case Token.MAKEUPPERLIMIT:
                case Token.MAKELOWERLIMIT:
                case Token.SETCULTURE:

                case Token.GETNEWGUID:
                case Token.RGB:

                case Token.TABLEEXISTS:
                case Token.FILEEXISTS:
                case Token.GETSETTING:
                case Token.SETSETTING:

                case Token.ARRAY_ATTACH:
                case Token.ARRAY_CLEAR:
                case Token.ARRAY_COPY:
                case Token.ARRAY_DETACH:
                case Token.ARRAY_FIND:
                case Token.ARRAY_GETAT:
                case Token.ARRAY_SIZE:
                case Token.ARRAY_SETAT:
                case Token.ARRAY_SORT:
                case Token.ARRAY_ADD:
                case Token.ARRAY_APPEND:
                case Token.ARRAY_INSERT:
                case Token.ARRAY_REMOVE:
                case Token.ARRAY_CONTAINS:
                case Token.ARRAY_CREATE:
                case Token.ARRAY_SUM:

                case Token.DECODE:
                case Token.CHOOSE:
                case Token.IIF:

                case Token.VALUEOF:

                case Token.ISDATABASEUNICODE:
                case Token.ISEMPTY:
                case Token.ISNULL:
                case Token.ISREMOTEINTERFACE:
                case Token.ISRUNNINGFROMEXTERNALCONTROLLER:
                case Token.ISWEB:

                case Token.CONVERT:
                case Token.TYPEOF:
                case Token.ADDRESSOF:
                case Token.EXECUTESCRIPT:

                case Token.GETTITLE:
                case Token.GETTHREADCONTEXT:
                case Token.OWNTHREADCONTEXT:

                //namespace TbWoormViewer
                case Token.GETREPORTNAMESPACE:
                case Token.GETREPORTMODULENAMESPACE:
                case Token.GETREPORTPATH:
                case Token.GETREPORTNAME:
                case Token.GETOWNERNAMESPACE:
                case Token.ISAUTOPRINT:

                case Token.DateAdd:
                case Token.WeekStartDate:
                case Token.EasterSunday:
                case Token.IsLeapYear:

                case Token.WILDCARD_MATCH:
                case Token.REPLICATE:
                case Token.PADLEFT:
                case Token.PADRIGHT:
                case Token.COMPARE_NO_CASE:

                case Token.SendBalloon:
                case Token.FormatTbLink:
                    {
                        if (!ParseFunction(lex, stack)) return;
                        break;
                    }

                case Token.CONTENTOF:
                    {
                        if (!ParseFunctionContentOf(lex, stack, false)) return;
                        break;
                    }
                case Token.CTYPE:
                    {
                        if (!ParseCTypeFunction(lex, stack)) return;
                        break;
                    }
                case Token.EOF:
                    lex.SetError(ExpressionManagerStrings.UnexpectedEof);
                    return;

                default:
                    lex.SetError(ExpressionManagerStrings.SyntaxError);
                    return;
            } // switch
        }

        //-----------------------------------------------------------------------------
        bool ParseArrayCreate(Parser lex, Stack<Item> exprStack)
        {
            if (!lex.ParseTag(Token.SQUAREOPEN))
                return false;

            int nrParam = 1;
            for (; ; nrParam++)
            {
                Expression(lex, exprStack);

                if (lex.Error)
                {
                    return false;
                }

                if (lex.Matched(Token.SQUARECLOSE))
                {
                    break;
                }

                if (!lex.ParseTag(Token.COMMA))
                {
                    return false;
                }
            }

            FunctionPrototype fp = Session.Functions.GetPrototype(Language.GetTokenString(Token.ARRAY_CREATE), 1);
            if (fp == null)
            {
                lex.SetError(string.Format(ExpressionManagerStrings.UndefinedFunction, "static const array definition"));
                return false;
            }

            FunctionItem pFun = new FunctionItem(Token.ARRAY_CREATE, fp, nrParam);
            exprStack.Push(pFun);
            return true;
        }

        //-----------------------------------------------------------------------------
        bool ParseArrayIndexer(string name, Parser lex, Stack<Item> exprStack)    //DataArray
        {
            if (!lex.ParseTag(Token.SQUAREOPEN))
                return false;

            Token tkFun = Token.ARRAY_GETAT;
            if (symbolTable != null && symbolTable.GetMemberDataType(name) == "String")
            {
                tkFun = Token.SUBSTR;
            }

            FunctionPrototype fp = Session.Functions.GetPrototype(Language.GetTokenString(tkFun), 2);
            if (fp == null)
            {
                lex.SetError(string.Format(ExpressionManagerStrings.UndefinedFunction, name));
                return false;
            }

            FunctionItem f = new FunctionItem(tkFun, fp);

            exprStack.Push(new Variable(name)); //array name

            Expression(lex, exprStack); //array index

            if (!lex.ParseTag(Token.SQUARECLOSE))
            {
                return false;
            }

            exprStack.Push(f);
            return true;
        }

        //-----------------------------------------------------------------------------
        public bool ParseFunctionContentOf(Parser lex, Stack<Item> exprParentStack, bool native)
        {
            Token tokenFun = lex.LookAhead();
            lex.SkipToken();


            if (!lex.ParseTag(Token.ROUNDOPEN))
                return false;

            Stack<Item> exprStack = new Stack<Item>();
            Expression(lex, exprStack);

            if (lex.Error || !lex.ParseTag(Token.ROUNDCLOSE))
            {
                lex.SetError(ExpressionManagerStrings.SyntaxError);
                return false;
            }

            // cerca il prototipo con quel numero di parametri. Se non lo trova ferma il parsing
            FunctionPrototype fp = Session.Functions.GetPrototype(Language.GetTokenString(tokenFun), 1);
            if (fp == null)
            {
                lex.SetError(string.Format(ExpressionManagerStrings.UndefinedFunction, Language.GetTokenString(tokenFun)));
                return false;
            }

            Expression e = new Expression(session, symbolTable);
            e.Compile(exprStack);

            if (!native)
            {
                ValueContentOf param = new ValueContentOf("ContentOf parameter");
                exprParentStack.Push(param);
            }

            FunctionContentOfItem contentOfFun = new FunctionContentOfItem(fp, exprStack, Session, symbolTable);
            exprParentStack.Push(contentOfFun);

            return true;
        }

        //-----------------------------------------------------------------------------
        bool ParseFunction(Parser lex, Stack<Item> stack)
        {
            Token tokenFun = lex.SkipToken();
            return ParseFunction(Language.GetTokenString(tokenFun), lex, stack, tokenFun);
        }

        //-----------------------------------------------------------------------------
        bool ParseFunction(string name, Parser lex, Stack<Item> stack, Token tokenFun = Token.ID)
        {
            if (!lex.ParseTag(Token.ROUNDOPEN))
                return false;

            int nrParams = 0;
            FunctionPrototype fp = null;

            if (this.symbolTable != null && tokenFun == Token.ID)
            {
                int idx = name.IndexOf('.');
                if (idx > 0)
                {
                    string sArrayName = name.Left(idx);
                    Variable field = symbolTable.Find(sArrayName);
                    //funzione interna su array espressa in notazione object oriented
                    if (field != null && field.DataType == "DataArray")
                    {
                        tokenFun = Language.GetKeywordsToken("Array_" + name.Mid(idx + 1));
                        if (tokenFun != Token.NOTOKEN)
                        {
                            stack.Push(new Variable(sArrayName, lex.CurrentPos)); //first parameter: array name
                            nrParams++;
                            name = Language.GetTokenString(tokenFun);
                        }
                    }
                }
            }

            //only WoormScript
            if (!allowThisCallMethods && name.IndexOf('.') > 0)
            {
                fp = Session.Functions.GetPrototype(name);

                if (fp == null && this.symbolTable != null)
                {
                    string handleName = string.Empty;
                    fp = this.symbolTable.ResolveCallMethod(name, out handleName) as FunctionPrototype;
                    if (fp != null && !handleName.IsNullOrEmpty())
                    {
                        // push object handle as first parameters
                        stack.Push(new Variable(handleName));
                        nrParams++;
                    }
                    else
                    {
                        string fullName;
                        if (this.symbolTable.ResolveAlias(name, out fullName))
                            fp = Session.Functions.GetPrototype(fullName);
                    }
                }
            }

            while (lex.LookAhead() != Token.ROUNDCLOSE && !lex.Error && !lex.Eof)
            {
                nrParams++;
                Expression(lex, stack);

                if (lex.Error)
                    return false; // errore nel parsing del parametro

                // se sono finiti i parametri esce altrimenti deve esserci una virgola
                if (lex.LookAhead() == Token.ROUNDCLOSE)
                    break;

                if (!lex.ParseTag(Token.COMMA))
                {
                    lex.SetError(ExpressionManagerStrings.SyntaxError);
                    return false;
                }

                if (IsNoWebFunction(name) && IsVariantNumberArgsFunction(name) && !SkipNoWebFunctionParameters(lex))
                {
                    lex.SetError(ExpressionManagerStrings.SyntaxError);
                    return false;
                }
            }

            if (!lex.ParseTag(Token.ROUNDCLOSE))
            {
                lex.SetError(ExpressionManagerStrings.SyntaxError);
                return false;
            }

            // si gestisce un caso particolare. Se si incontra la funzione LOCALIZE con un solo
            // parametro la si trasforma in quella a due parametri mettendo come secondo parametro
            // il nome del corrente file. Si aggiunge inoltre la stringa alla lista di quelle da localizzare
            if ((string.Compare(name, Token.LOCALIZE.ToString(), StringComparison.OrdinalIgnoreCase) == 0))
                ManageLocalizeFunction(name, lex, stack, ref nrParams);

            // cerca il prototipo con quel numero di parametri, salvo eccezioni per funzioni a parametri variabili illimitati
            if (fp == null)
            {
                int np = nrParams;
                switch (tokenFun)
                {
                    case Token.MIN:
                    case Token.MAX:
                        np = 2;
                        break;
                    case Token.DECODE:
                        np = 3;
                        break;
                    case Token.ARRAY_CREATE:
                        np = 1;
                        break;
                }
                fp = Session.Functions.GetPrototype(name, np);
            }

            // se non l'ho trovata e le chiamate su oggetto sono abilitate, istanzio una funzione con late binding (non mi arrabbio se non � dichiarata)
            string varName;
            if (fp == null)
            {
                if (allowThisCallMethods)
                {
                    fp = GetThisCallFunction(name, stack, nrParams, out varName);
                    if (fp != null)
                    {
                        stack.Push(new ThiscallFunctionItem(varName, fp.Name, fp));
                        return true;
                    }
                }
                else if (AllowLateBinding(name))
                {
                    //fp = new FunctionPrototype(name, "Variant", null);
                    stack.Push(new FunctionItem(name, null, true, nrParams));    // HasLateBinding
                    return true;
                }
            }

            if (fp == null)
            {
                lex.SetError(string.Format(ExpressionManagerStrings.UndefinedFunction, name));
                return false;
            }

            stack.Push(new FunctionItem(fp.FullName, fp, false, nrParams));
            return true;
        }

        //-----------------------------------------------------------------------------
        bool AllowLateBinding(string name)
        {
            //int idx = name.IndexOf('.');
            //if (idx > 0)
            //{
            //string sPrefix = name.Left(idx);
            //if (
            //    sPrefix.CompareNoCase("OwnerID") ||
            //    sPrefix.CompareNoCase("LinkedDocumentID") 
            //    )
            //{
            //    return true;
            //}
            //    return true; // queryCustomer.ReadOne();
            //}
            return true;    //per le Procedure
        }

        //-----------------------------------------------------------------------------
        /// <summary>
        /// Funzione speciale che gestisce il cast in un altro tipo woorm
        /// </summary>
        /// <param name="lex"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        bool ParseCTypeFunction(Parser lex, Stack<Item> stack)
        {
            lex.SkipToken();//nome funzione
            if (!lex.ParseTag(Token.ROUNDOPEN)) return false;
            //primo argomento (oggetto da castare)
            Expression(lex, stack);
            if (lex.Error) return false; // errore nel parsing del parametro

            if (!lex.ParseTag(Token.COMMA))
            {
                lex.SetError(ExpressionManagerStrings.SyntaxError);
                return false;
            }

            //secondo argomento (tipo di dato di destinazione)
            ushort tag = 0;
            string aType = "String";
            string woormType = "";
            string aBaseType = "";
            if (!DataTypeParser.Parse(lex, Session.Enums, out aType, out woormType, out tag, out aBaseType))
                return false;

            if (!lex.ParseTag(Token.ROUNDCLOSE))
            {
                lex.SetError(ExpressionManagerStrings.SyntaxError);
                return false;
            }
            //nello stack metto l'equivalente stringa del tipo di dato di destinazione
            stack.Push(new Value(woormType));
            //la funzione � di tipo speciale, non ha prototipo classico
            stack.Push(new CTypeFunctionItem(woormType));
            return true;
        }

        //-----------------------------------------------------------------------------
        /// <summary>
        /// Cerca dinamicamente la funzione via reflection nel contesto corrente
        /// </summary>
        private FunctionPrototype GetThisCallFunction(string name, Stack<Item> stack, int nrParams, out string varName)
        {
            int dotIndex = name.LastIndexOf('.');
            varName = "";
            if (dotIndex == -1)
                return null; //non c'� il punto: non � una chiamata a funzione appartenente ad oggetto

            varName = name.Substring(0, dotIndex);  //nome variabile
            string funcName = name.Substring(dotIndex + 1); //nome funzione
            string[] types = new string[nrParams];
            if (nrParams > 0)
            {
                Stack<Item> tmpStack = stack.Clone();
                for (int i = 0; i < nrParams; i++)
                    types[i] = GetParameterType((DataItem)tmpStack.Pop());
            }

            return new FunctionPrototype(funcName, "Object", types);//finch� non la eseguo non so che tipo di ritorno ha
        }

        //-----------------------------------------------------------------------------
        //recupera il tipo del data item, eventualmente andando a controllare nella symbol table
        private string GetParameterType(DataItem di)
        {
            //caso pi� semplice: il dataitem contiene un dato
            if (di.Data != null)
                return di.DataType;
            else if (di is Variable) //il dataitem non ha dato, ma � una variabile che posso andare a cercare in symbol table
            {
                Variable v = (symbolTable == null) ? null : symbolTable.Find(((Variable)di).Name);
                if (v != null && v.Data != null)
                    return v.DataType;
            }
            //non ho informazioni sul tipo: � un generico object
            return "Object";
        }

        //-----------------------------------------------------------------------------
        public static bool IsNoWebFunction(string name)
        {
            int idx = name.LastIndexOf('.');
            if (idx > 0)
            {
                string mod = name.Left(idx);
                name = name.Mid(idx + 1);

                bool b1 = string.Compare(mod, "Woorm", StringComparison.OrdinalIgnoreCase) != 0;
                bool b2 = string.Compare(mod, "Framework.TbWoormViewer.TbWoormViewer", StringComparison.OrdinalIgnoreCase) != 0;
                if (b1 && b2)
                    return false;
            }
            string[] noWebFun = new string[]
                {
                        //"RunReport", 
                        "RunProgram",
                        "RunDocument",
                        "BrowseDocument",

                        "MailSetFrom",
                        "MailSetSubject",
                        "MailSetBody",
                        "MailSetMapiProfile",
                        "MailSetBodyTemplateFileName",
                        "MailSetTo",
                        "MailSetCc",
                        "MailSetBcc",
                        "MailSetAttachment",
                        "MailSetBodyParameter",
                        "MailSend",
                        "MailSetAttachmentReportName",
                        "MailSetToCertified",
                        "MailSetCertifiedFilter",

                        "PostaLiteSetAddressee",
                        "PostaLiteSetSendType",
                        "PostaLiteSend",

                        "ReportSaveAs",
                        "ArchiveReport",
                        "SendMail",

                        "UpdateOutputParametersEvenIfReportDoesNotFetchRecords",

                        "QueryCall",
                        "QueryIsEof",
                        "QueryIsFailed",
                        "QueryGetErrorInfo",
                        "QueryGetSqlString",

                        "QuerySetConnection",
                        "QuerySetCursorType",

                        "GetConnection",
                        "OpenConnection",
                        "CloseConnection",

                        "ConnectionBeginTrans",
                        "ConnectionCommit",
                        "ConnectionRollback",

                        "ConnectionLockRecord",
                        "ConnectionUnlockRecord",
                        "ConnectionUnlockAll"
                };

            foreach (string f in noWebFun)
                if (string.Compare(name, f, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            return false;
        }

        //-----------------------------------------------------------------------------
        public static bool IsVariantNumberArgsFunction(string name)
        {
            int idx = name.LastIndexOf('.');
            if (idx > 0)
            {
                string mod = name.Left(idx);
                name = name.Mid(idx + 1);

                if (
                    string.Compare(mod, "Woorm", StringComparison.OrdinalIgnoreCase) != 0
                    &&
                    string.Compare(mod, "Framework.TbWoormViewer.TbWoormViewer", StringComparison.OrdinalIgnoreCase) != 0
                    )
                    return false;
            }
            string[] noWebFun = new string[]
                {
                        "RunReport",
                        "RunDocument",
                        "BrowseDocument",

                        "ConnectionLockRow",
                        "ConnectionUnlockRow"
                };

            foreach (string f in noWebFun)
                if (string.Compare(name, f, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            return false;
        }

        //-----------------------------------------------------------------------------
        public static void ExpandStack(Stack<Item> fromStack, ref Stack<Item> toStack)
        {
            toStack.Clear();
            object[] array = fromStack.ToArray();

            for (int i = array.Length - 1; i >= 0; i--)
            {
                Item item = array[i] as Item;
                if (item != null)
                    toStack.Push(item.Expand());
            }
        }


        // Siccome esiste la funzione di TB RunReport con parametri variabili che in c#
        // non � supportata, per compatibilit� devo far passare il parsing correttamente
        // pertanto devo skippare tutti i parametri sino alla parentesi tonda.
        // Tanto in esecuzione la funzione non viene eseguita realmente perch�
        // non � possibile lanciare un altro report.
        //-----------------------------------------------------------------------------
        private bool SkipNoWebFunctionParameters(Parser lex)
        {
            return lex.SkipToToken(new Token[] { Token.ROUNDCLOSE }, false, true, true) && !lex.Error && !lex.Eof;
        }

        //-----------------------------------------------------------------------------
        void ManageLocalizeFunction(string name, Parser lex, Stack<Item> stack, ref int nrParams)
        {
            object o = stack.Peek();
            if (o is Value)
            {
                Value v = (Value)o;
                if (v.DataType == "String")
                {
                    string s = (string)v.Data;
                    if (s != null && s != string.Empty)
                        symbolTable.LocalizableStrings.Add(s);
                }
            }
        }
    }

    //============================================================================
    public class ExpressionUnparser
    {
        //-----------------------------------------------------------------------------
        public bool Unparse(out string exprStr, Stack<Item> stack)
        {
            StringCollection collection = new StringCollection();
            Stack<Item> tmpStack = stack.Clone();

            exprStr = "";
            if (!Expression(collection, tmpStack))
                return false;

            StringBuilder sb = new StringBuilder();
            for (int i = collection.Count; i > 0; i--)
                sb.Append(collection[i - 1]);

            exprStr = sb.ToString();
            return true;
        }

        //-----------------------------------------------------------------------------
        bool Expression(StringCollection collection, Stack<Item> workStack)
        {
            string strTmp;

            if (workStack.Count <= 0)
                return false;

            object o = workStack.Peek();
            if (o == null)
                return false;

            if (o is Variable)
            {
                Variable var = (Variable)workStack.Pop();
                collection.Add(UnparseVariable(var));
                return true;
            }

            if (o is Value)
            {
                Value itemVal = (Value)workStack.Pop();
                collection.Add(UnparseValue(itemVal));
                return true;
            }

            if (o is OperatorItem)
            {

                OperatorItem ope = (OperatorItem)workStack.Pop();

                collection.Add(")");
                if (ope.GetOperatorType() == OperatorItem.OperatorType.Logical)
                {
                    if (!Unparse(out strTmp, ope.SecondStack))
                        return false;

                    collection.Add(strTmp);

                    if (ope.OperatorID == Token.QUESTION_MARK || ope.OperatorID == Token.BETWEEN)
                    {
                        if (ope.OperatorID == Token.QUESTION_MARK)
                            collection.Add(UnparseOperator(Token.COLON));

                        if (ope.OperatorID == Token.BETWEEN)
                            collection.Add(UnparseOperator(Token.AND));

                        if (!Unparse(out strTmp, ope.FirstStack))
                            return false;

                        collection.Add(strTmp);
                    }
                }

                if (ope.OperatorID != Token.EXPR_IS_NULL && ope.OperatorID != Token.EXPR_IS_NOT_NULL)
                {
                    if (ope.GetOperatorType() != OperatorItem.OperatorType.Logical && !Expression(collection, workStack))
                        return false;

                    if (ope.OperatorID == Token.EXPR_ESCAPED_LIKE)
                    {
                        collection.Add(UnparseOperator(Token.ESCAPE));
                        if (!Expression(collection, workStack))
                            return false;
                    }
                }

                collection.Add(UnparseOperator(ope.OperatorID));
                if (
                        (
                            ope.GetOperatorType() != OperatorItem.OperatorType.Unary ||
                            ope.OperatorID == Token.EXPR_IS_NULL || ope.OperatorID == Token.EXPR_IS_NOT_NULL
                        ) &&
                        !Expression(collection, workStack)
                    )
                    return false;

                collection.Add("(");
                return true;
            }

            if (o is FunctionItem)
            {
                collection.Add(")");

                FunctionItem function = (FunctionItem)workStack.Pop();
                for (int p = function.Parameters.Count; p > 0; p--)
                {
                    if (!Expression(collection, workStack))
                        return false;

                    if (p > 1) collection.Add(",");
                }
                collection.Add("(");
                collection.Add(function.FullName);
                return true;
            }

            return true;
        }

        //-----------------------------------------------------------------------------
        string UnparseVariable(Variable var)
        {
            return " " + var.Name + " ";
        }


        //-----------------------------------------------------------------------------
        string UnparseOperator(Token operatorID)
        {
            string str = "";
            switch (operatorID)
            {
                case Token.NOT:
                case Token.OP_NOT:
                    str = Language.GetTokenString(Token.NOT);
                    break;

                case Token.EXPR_UNNARY_MINUS:
                    str = Language.GetTokenString(Token.MINUS);
                    break;

                case Token.EXPR_IS_NULL:
                case Token.EXPR_IS_NOT_NULL:
                    str = Token.IS.ToString() + " ";
                    if (operatorID == Token.EXPR_IS_NOT_NULL)
                        str += Language.GetTokenString(Token.NOT) + " ";

                    str += Language.GetTokenString(Token.NULL);
                    break;

                case Token.EXPR_ESCAPED_LIKE:
                    str = Language.GetTokenString(Token.LIKE);
                    break;

                case Token.AND:
                case Token.OP_AND:
                    str = Language.GetTokenString(Token.AND);
                    break;

                case Token.OR:
                case Token.OP_OR:
                    str = Language.GetTokenString(Token.OR);
                    break;

                case Token.NE:
                case Token.DIFF:
                    str = Language.GetTokenString(Token.DIFF);
                    break;

                case Token.EQ:
                case Token.ASSIGN:
                    str = Language.GetTokenString(Token.ASSIGN);
                    break;

                default:
                    str = Language.GetTokenString(operatorID);
                    break;

            }

            return str;
        }

        //-----------------------------------------------------------------------------
        string UnparseValue(Value v)
        {
            string str = "";
            switch (v.DataType)
            {
                // { dt "21/12/1956 12:23:12"}
                case "DateTime":
                    str += "{dt \"" + ((DateTime)v.Data).ToString("G") + "\"}";
                    break;

                // gestisce le stringhe che contengono virgolette
                case "String":
                    str += "\"" + Unparser.EscapeString(v.Data.ToString()) + "\"";
                    break;

                default:
                    str += v.Data.ToString();
                    break;
            }

            return str;
        }
    }
}

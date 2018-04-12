using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.Serialization;

using TaskBuilderNetCore.Interfaces;

using Microarea.Common.Generic;
using Microarea.Common.ExpressionManager;
using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.Common.Lexan;
using Microarea.Common.Hotlink;
using Microarea.Common;

namespace Microarea.RSWeb.WoormEngine
{
	/// <summary>
	/// Field.
	/// </summary>
	//============================================================================
	//[Serializable]
	public class Field : Variable, IDisposable
	{
		const string DATA = "data";
		const string DATATYPE = "dataType";

		// servono per gestire le properti che restituiscono data e valid in funzione di un contesto
		protected object	eventData = null;
		protected object	groupByData = null;

		protected bool		validEventData = true;
		protected bool		validGroupByData = true;

		private bool		groupByDataUpdated = false;
		private bool		eventDataUpdated = false;

		private Token	inputLimit = Token.NOTOKEN;
		private bool	displayed = false;
		private bool	isColTotal = false;
		private bool	isSubTotal = false;
		private bool	isColumn = false;

		//usato per i campi speciali currentPage e layou che vengono inseriti nella symboltable gia inizializzati e non devono 
		//essere ripuliti dopo l'esecuzione delle Askdialgo dalla ClearAllData()
		private bool isSpecialFieldInitialized = false;

		private RuleEngine				engine = null;
		private GroupFunction			groupFunction = null;		// property of GroupBy
		private DisplayTable			ownDisplayTable = null;		// Display Table Owner
		private ushort					originalAlias = 0;			// Field Owner (only for SubTotal/ColTotal fields)
		private EventFunction			eventFunction = null;
		private WoormEngineExpression	initExpression = null;
		private StringCollection		substrings = null;

		private string		physicalName;			// nome fisico della colonna di database (se esiste)
		private int			numDec = 0;				// temporary file// ITRI forse posso eliminarle
		private bool		readOnly = false;
		private RuleObj		ownerRule = null;		// indica in quale rule il Field è referenziato.
		private WoormEngineExpression	defaultExpression = null;

		private bool	hidden = false;
		private bool	reinit = false;
		private bool	statico = false;
		private bool	input = false;
		private bool	ask = false;
		private bool	column = false;
		
		public	bool	NativeColumnExpr = false;	//to support selection of sql-expression columns such as Count(*), Max( Distinct colum), Sum (a, b)

        protected string tagXml = string.Empty;    //Alias  field name on Xml report
        protected bool noXml = false;	        //skip field on Xml report
        protected string contextName = string.Empty;    

        private List<FunctionPrototype> Methods = new List<FunctionPrototype>();

		//l'originalAlias è sempre uguale al campo ID, tranne con field SubTotal e ColTotal, in questi campi
		//l'id viene sostituito con quello del campo a cui fanno riferimento. serve solo per l'unparsing
		public ushort OriginalAlias { get { return originalAlias == 0 ? id : originalAlias; } }

		//------------------------------------------------------------------------------------
		public bool IsTypeColumn() { return column; }

		//---------------------------------------------------------------------------
		public string	PhysicalName	
		{
			get 
			{	
				int index = physicalName.IndexOf('.');
				string name = index == -1 || NativeColumnExpr
					? physicalName
					: physicalName.Substring(index + 1);
				return name; 
			}
			set { physicalName = value; }
		}
		public string	QualifiedPhysicalName	{ get { return physicalName; } set { physicalName = value; } }
		public string	PublicName				{ get { return this.Name; }}
		public int		InternalId				{ get { return Id; }}
		public TbReportSession Session		{ get { return engine.Session; }}

		//----------------------------------------------------------------------------
		public bool		Hidden		{ get { return hidden; } set { hidden = value; }} 
		public bool		ReInit		{ get { return reinit; } set { reinit = value; }}
		public bool		Input		{ get { return input; } set { input = value; }}
		public bool		Ask			{ get { return ask; } set { ask = value; }}
		public bool		Displayed	{ get { return displayed; } set { displayed = value; }}
		public bool		IsColumn	{ get { return isColumn; }}
		public bool		IsColTotal	{ get { return isColTotal; }}
		public bool		IsSubTotal	{ get { return isSubTotal; }}
		public bool		ReadOnly	{ get { return readOnly; }}
        public bool     Statico     { get => statico; }

        public bool		IsSpecialFieldInitialized { get { return isSpecialFieldInitialized; }	set { isSpecialFieldInitialized = value; }}

		public DisplayTable		DisplayTable	{ get { return ownDisplayTable; }}
		public EventFunction	EventFunction	{ get { return eventFunction; }}
		public WoormEngineExpression	InitExpression	{ get { return initExpression; }}
		public Token			InputLimit		{ get { return inputLimit; } set { inputLimit = value; }}

		//----------------------------------------------------------------------------
		public object	RuleData	{ get { return this.data;	} set { this.data = value; }}
		public object	GroupByData	{ get { return groupByData;	} set { groupByData = value; }}
		public object	EventData	{ get { return eventData;	} set { eventData = value; }}
		public object	AskData		{ get { return eventData;	} set { eventData = value; this.data = value; }}

		//public bool	ValidRuleData		{ get { return this.valid;			} set { this.valid = value; }}
		public bool	ValidGroupByData	{ get { return validGroupByData;	} set { validGroupByData = value; }}
		public bool	ValidEventData		{ get { return validEventData;		} set { validEventData = value; }}
		public bool	ValidAskData		{ get { return validEventData;		} set { validEventData = value; this.valid = value; }}

		public int	NumDec	{ get { return numDec; } set { numDec = value; }}

		//----------------------------------------------------------------------------
		public GroupFunction	GroupFunction	{ get { return groupFunction;} set { groupFunction = value; }}
		public RuleObj			OwnerRule		{ get { return ownerRule; } set {ownerRule  = value; }}
		public bool				OwnRule			{ get { return ownerRule != null; }}

		//----------------------------------------------------------------------------
		//public bool	RuleDataFetched		{ get { return ruleDataFetched;		} set {ruleDataFetched  = value; }}
		public bool	GroupByDataUpdated	{ get { return groupByDataUpdated;	} set {groupByDataUpdated  = value; }}
		public bool	EventDataUpdated	{ get { return eventDataUpdated;	} set {eventDataUpdated  = value; }}

		//----------------------------------------------------------------------------
		public bool IsGroupByDataChanged{ get { return groupByDataUpdated && !ObjectHelper.IsEquals(GroupByData, EventData); }}

        //-----------------------------------------------------------------------------
        public override bool IsRuleFields()
        {
            return this.OwnerRule != null;
        }

		//----------------------------------------------------------------------------
		public override object Data
		{
			get
			{
				// non ho ancora inizializzato engine perchè sto arrivando qui dalla chiamata
				// del costruttore base che tenta di inizializzare il campo this.data
				if (this.engine == null)
					return this.data;

				switch (this.engine.DataLevel)
				{
					case DataLevel.Rules	: return this.data;
					case DataLevel.Events	: return eventData;
					case DataLevel.GroupBy	: return groupByData;
					default: throw (new FieldException(WoormEngineStrings.UnknownDataLevel));
				}
			}
			set
			{
				if (DataType == "Boolean")
					value = ObjectHelper.CastBool(value);
				
				//An.17985 
				if (WoormType == "Date" && value is DateTime)
					value = ((DateTime)value).Date;
				//

				if (this.engine == null)
				{
					this.data = value;
					return;
				}

				switch (this.engine.DataLevel)
				{
					case DataLevel.Rules	: this.data = value; break;
					case DataLevel.Events	: eventData = value; break;
					case DataLevel.GroupBy	: groupByData = value; break;
					default: throw (new FieldException(WoormEngineStrings.UnknownDataLevel));
				}
			}
		}

        //----------------------------------------------------------------------------
        override public object GetData(DataLevel lev)
        {
            if (this.engine == null)
                return this.data;

            switch (lev)
            {
                case DataLevel.Rules: return this.data;
                case DataLevel.Events: return eventData;
                case DataLevel.GroupBy: return groupByData;
                default: throw (new FieldException(WoormEngineStrings.UnknownDataLevel));
            }
        }

        override public void SetData(DataLevel lev, object value)
        {
            if (DataType == "Boolean")
                value = ObjectHelper.CastBool(value);

            if (this.engine == null)
            {
                this.data = value;
                return;
            }

            switch (lev)
            {
                case DataLevel.Rules: this.data = value; break;
                case DataLevel.Events: eventData = value; break;
                case DataLevel.GroupBy: groupByData = value; break;
                default: throw (new FieldException(WoormEngineStrings.UnknownDataLevel));
            }
        }

		//----------------------------------------------------------------------------
		public override bool Valid
		{
			get
			{
				// non ho ancora inizializzato engine perchè sto arrivando qui dalla chiamata
				// del costruttore base che tenta di inizializzare il campo this.data
				if (this.engine == null)
					return this.valid;

				switch (this.engine.DataLevel)
				{
					case DataLevel.Rules	: return this.valid;
					case DataLevel.Events	: return validEventData;
					case DataLevel.GroupBy	: return validGroupByData;
					default: throw (new FieldException(WoormEngineStrings.UnknownDataLevel));
				}
			}
			set
			{
				if (this.engine == null)
				{
					this.valid = value;
					return;
				}

				switch (this.engine.DataLevel)
				{
					case DataLevel.Rules	: this.valid = value; break;
					case DataLevel.Events	: validEventData = value; break;
					case DataLevel.GroupBy	: validGroupByData = value; break;
					default: throw (new FieldException(WoormEngineStrings.UnknownDataLevel));
				}
			}
		}

		//----------------------------------------------------------------------------
		public Field(string dataType, string name, RuleEngine engine) : base(name)
		{
			Initialize(dataType, name, 0, 0, false, false, false, engine);
		}

		// da usarsi per creare Field contenenti DataEnum
		//----------------------------------------------------------------------------
		public Field(string dataType, string name, ushort tag, ushort item, RuleEngine engine) : base(name)
		{
			Initialize(dataType, name, tag, item, false, false, false, engine);
		}

        public new Field Clone()
        {
            Field clone = new Field(DataType, Name, engine);
            clone.Data = Data;
            clone.eventData = eventData;
            clone.groupByData = groupByData;

            clone.validEventData = validEventData;
            clone.validGroupByData = validGroupByData;

            clone.groupByDataUpdated = groupByDataUpdated;
            clone.eventDataUpdated = eventDataUpdated;

            clone.inputLimit = inputLimit;
            clone.displayed = displayed;
            clone.isColTotal = isColTotal;
            clone.isSubTotal = isSubTotal;
            clone.isColumn = isColumn;

            clone.isSpecialFieldInitialized = isSpecialFieldInitialized;

            clone.originalAlias = originalAlias;
            clone.physicalName = physicalName;
            clone.numDec = numDec;            
            clone.readOnly = readOnly;

            clone.hidden = hidden;
            clone.reinit = reinit;
            clone.statico = statico;
            clone.input = input;
            clone.ask = ask;
            clone.column = column;

            clone.NativeColumnExpr = NativeColumnExpr;

            clone.tagXml = tagXml;
            clone.noXml = noXml;	     
            clone.contextName = contextName;

            return clone;
            
            /*  RuleEngine engine = null;
                GroupFunction groupFunction = null;     // property of GroupBy
                DisplayTable ownDisplayTable = null;        // Display Table Owner
                private EventFunction eventFunction = null;
                private WoormEngineExpression initExpression = null;
                private StringCollection substrings = null;
                private List<FunctionPrototype> Methods = new List<FunctionPrototype>(); 

                private RuleObj ownerRule = null;       // indica in quale rule il Field è referenziato.
                private WoormEngineExpression defaultExpression = null;    */
        }

        //----------------------------------------------------------------------------
        //public Field() : base()
        //{
        //	//TODO SILVANo: per deserializzatore json del nuovo woorm
        //}

        //public Field(SerializationInfo info,StreamingContext context)
        //{
        //	//TODO SILVANO: per deserializzatore json del nuovo woorm
        //}

        //public override void GetObjectData(SerializationInfo info,StreamingContext context)
        //{
        //	base.GetObjectData(info, context);

        //	info.AddValue(DATA, Data);
        //	info.AddValue(DATATYPE, Data.GetType().Name); 
        //}
        //----------------------------------------------------------------------------

        public string ToJson()
        {
            string s = "\"" + "field" + "\":{";

            s += this.Name.ToJson("name") + ',';
            s += this.Id.ToJson("id", "id") + ',';

            //Per le date viene usato il WoormType per pilotarne il formattatore (nelle askdialog ad es. le variabili di tipo Date non mostrano ore:minuti, quelle dateTime si)
            //Il type del Data non serve a discriminare perche in entrambi i casi e' DateTime
            if (this.Data is DateTime)
            {
                s += this.WoormType.ToJson("type") + ',';
            }
            else
            {
                s += this.Data.GetType().Name.ToJson("type") + ',';
            }
            s += this.Data.ToJson("value");

            return s + '}';
        }

        //----------------------------------------------------------------------------
        virtual public void Dispose()
        {
            if (this.fwf != null)
            {
                fwf.Dispose();
                fwf = null;
            }
        }

		//----------------------------------------------------------------------------
		private void Initialize
			(
				string		dataType, 
				string		name, 
				ushort		tag,
				ushort		item,
				bool		hidden, 
				bool		reinit, 
				bool		statico, 
				RuleEngine	engine
			)
		{
			this.engine			= engine;
			this.physicalName	= name;
			this.hidden			= hidden;
			this.reinit			= reinit;
			this.statico		= statico;
            this.EnumTag        = tag;

			this.data			= ObjectHelper.CreateObject(dataType, "", tag, item);
			this.eventData		= ObjectHelper.CreateObject(dataType, "",  tag, item);
			this.groupByData	= ObjectHelper.CreateObject(dataType, "", tag, item);
        }

		//----------------------------------------------------------------------------
		public bool IsArray 
		{ get { return string.Compare(Language.GetTokenString(Token.ARRAY), WoormType, StringComparison.OrdinalIgnoreCase) == 0; } } 

		//----------------------------------------------------------------------------
		public string ArrayBaseType
		{ 
			get 
			{ 
				if (!IsArray)
					return string.Empty;

				return ((DataArray)(this.data)).BaseType; 
			}
			set
			{
				if (!IsArray)
					return;

				this.data = new DataArray(value);
				this.eventData = new DataArray(value);
				this.groupByData = new DataArray(value);
			}
		}

        //----------------------------------------------------------------------------
        protected void AssignArrayData (object aData, bool aValid, object objIdx, ref bool validData, object fieldData) 
		{ 
			validData = aValid; 
			if (!aValid)
				return;
			int idx = Convert.ToInt32(objIdx);
			if (idx < 0)
			{
				validData = false;
				return;
			}

			DataArray ar = (DataArray) fieldData ;
			ar.SetAtGrow(idx, aData);
		}

		//----------------------------------------------------------------------------
		public void AssignArrayEventData (object aData, bool aValid, object objIdx) 
		{ 
			AssignArrayData(aData, aValid, objIdx, ref this.validEventData, this.eventData);
		}

		public void AssignArrayGroupByData (object aData, bool aValid, object objIdx) 
		{ 
			AssignArrayData(aData, aValid, objIdx, ref this.validGroupByData, this.groupByData);
		}

		public void AssignArrayRuleData (object aData, bool aValid, object objIdx) 
		{ 
			AssignArrayData(aData, aValid, objIdx, ref this.valid, this.data);
		}


		//----------------------------------------------------------------------------
		public void AssignAllArray(DataArray aData, bool aValid)
		{
			data = aData;
			eventData = aData;
			groupByData = aData;
			
			RuleDataFetched = true;
			GroupByDataUpdated = true;
			EventDataUpdated = true;
		}

		//----------------------------------------------------------------------------
		public void SetArrayAllData(object aData, bool aValid, object objIdx)
		{
			AssignArrayRuleData(aData, aValid, objIdx);
			AssignArrayGroupByData(aData, aValid, objIdx);
			AssignArrayEventData(aData, aValid, objIdx);

			RuleDataFetched = true;
			GroupByDataUpdated = true;
			EventDataUpdated = true;
		}

		//----------------------------------------------------------------------------
		public void SetLowerLimit ()		
		{ 
			InputLimit = Token.LOWER_LIMIT;

			this.data	= ObjectHelper.SetLowerLimit(this.data); 
			eventData	= ObjectHelper.SetLowerLimit(eventData); 
			groupByData	= ObjectHelper.SetLowerLimit(groupByData); 
		}

		//----------------------------------------------------------------------------
		public void SetUpperLimit ()		
		{ 
			InputLimit = Token.UPPER_LIMIT;

			this.data	= ObjectHelper.SetUpperLimit(this.data, engine.Report.MaxString, this.Len); 
			eventData	= ObjectHelper.SetUpperLimit(eventData, engine.Report.MaxString, this.Len); 
			groupByData = ObjectHelper.SetUpperLimit(groupByData, engine.Report.MaxString, this.Len); 
		}

		//----------------------------------------------------------------------------
		public void SetReadOnly ()
		{
			readOnly = true;

			eventData = RuleData;
			groupByData = RuleData;
		}

		//----------------------------------------------------------------------------
		public void SetPrecision (int l, int d)
		{
			len = l;
			numDec = d;
		}

		//----------------------------------------------------------------------------
		public void AssignRuleData		(object aData, bool aValid) { ObjectHelper.Assign(ref this.data, aData);	this.valid = aValid; }
		public void AssignGroupByData	(object aData, bool aValid) { ObjectHelper.Assign(ref groupByData, aData);	validGroupByData = aValid; }
		public void AssignEventData		(object aData, bool aValid) { ObjectHelper.Assign(ref eventData, aData);	validEventData = aValid; }

        public void AssignData(DataLevel lev, object aData, bool aValid)
        {
            switch (lev)
            {
                case DataLevel.Events:
                    AssignEventData(aData, aValid);
                    EventDataUpdated = true;
                    break;
                case DataLevel.GroupBy:
                    AssignGroupByData(aData, aValid);
                    GroupByDataUpdated = true;
                    break;
                case DataLevel.Rules:
                    AssignRuleData(aData, aValid);
                    break;
            }
        }
        //----------------------------------------------------------------------------
        //public void ClearRuleData		() { ObjectHelper.Clear(ref this.data);		this.valid = true; }
        public void ClearGroupByData	() { ObjectHelper.Clear(ref groupByData);	validGroupByData = true; }
		public void ClearEventData		() { ObjectHelper.Clear(ref eventData);		validEventData = true;}

		//----------------------------------------------------------------------------
		public void UpdateGroupByData ()
		{
			if (OwnRule && RuleDataFetched)
				AssignGroupByData(RuleData, ValidRuleData);
		}

		//----------------------------------------------------------------------------
		public void UpdateEventData ()
		{
			if (OwnRule && groupByDataUpdated)
				AssignEventData(GroupByData, ValidGroupByData);
		}

		//----------------------------------------------------------------------------
		override public void SetAllData(object aData, bool aValid)
		{
            if (aData == null)
            {
                Debug.WriteLine("Field init expression failed" + '(' + this.Name + ')');
                return;
            }
            
            AssignRuleData(aData, aValid);
			AssignGroupByData(aData, aValid);
			AssignEventData(aData, aValid);

			RuleDataFetched = true;
			GroupByDataUpdated = true;
			EventDataUpdated = true;
		}

		//----------------------------------------------------------------------------
		override public void ClearAllData()
		{
            base.ClearAllData();			

			ClearGroupByData();
			ClearEventData();

			GroupByDataUpdated = true;
			EventDataUpdated = true;
		}

		// serve a far considerare il field come se fosse un null data in modo da
		// poterlo considerare tale nelle varie espression e quin gestire "IS NULL" o "IS NOT NULL"
		// nelle rule dei vari tipi (condizionali, espressioni, datatable, query)
		//----------------------------------------------------------------------------
		public void SetNullRuleData()
		{
			ClearRuleData();

			Valid = false;	// deve usare l'attributo
			RuleDataFetched = true;
		}

		//----------------------------------------------------------------------------
		public bool Init()
		{
			// carica il valore di default valutando l'espressione di inizalizzazione, se esiste
			if (initExpression != null)
			{
				Value v = initExpression.Eval();
				if (initExpression.Error)
				{
					engine.SetError(initExpression.Diagnostic);
					return false;
				}
                
                SetAllData(v.Data, v.Valid);

				return v.Data != null;
			}

			//Se non ha un'espressione di inizializzazione e se non e' un campo speciale gia inizializzato da codice,
			//resetto il suo valore
			if (!isSpecialFieldInitialized)
				ClearAllData();
			
			return true;
		}

		//---------------------------------------------------------------------------
		public bool ParseDefault(Parser lex, FieldSymbolTable symbolTable)
		{
			defaultExpression = new WoormEngineExpression(engine.Report.Engine, Session, symbolTable);
			defaultExpression.StopTokens = new StopTokens(new Token[] { Token.COMMA });

			if (!defaultExpression.Compile(lex, CheckResultType.Compatible, DataType))
			{
				lex.SetError(string.Format(WoormEngineStrings.IllegalDefaultQuery, PublicName));
				return false;
			}

			return true;
		}

		// ALGORITMO 1 - esame puntuale della stringa con carattere asiatico pesato
		//----------------------------------------------------------------------------
		int CalculateDataOccupancy (string data, int maxOccupancy)
		{
			if (!DictionaryFunctions.IsAsianCulture() || data.Length == 0)
				return maxOccupancy;

			// per ogni carattere asiatico incontrato calcolo un peso due volte e mezza
			double nTotalStringWeight = 0;
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i] > 256)
					nTotalStringWeight += 2.5;
				else
					nTotalStringWeight += 1.0;

				// non appena la stringa pesata supera l'occupazione dichiarata
				// da Woorm, esco e ritorno quanti caratteri prendere (quindi i-1+1)
				if (((int) nTotalStringWeight) > maxOccupancy)
					return i;				
			}

			return maxOccupancy;
		}

		//----------------------------------------------------------------------------
        FieldWidthFactors fwf = null;

        StringCollection SplitStringOptimized(string data)
        {
            int nStrLen = data.Length;
            if (nStrLen <= 0)
                return null;

            IWoormDocumentObj woormObj = engine.Report.woormDoumentObj;
            if (woormObj == null)
                return SplitStringOld(data);

            if (fwf == null)
            {
                fwf = new FieldWidthFactors();
                if (!woormObj.GetFieldWidthFactors(this.Id, ref fwf))
                    return SplitStringOld(data);

                fwf.m_nWidth = Math.Max(1, (int)(fwf.m_nWidth * engine.ColumnWidthPercentage));
            }
            //----
	        int nAvgLen = woormObj.CalculateFieldWidth(Id, data);
	        //ASSERT(nAvgLen >= 0);
	        if (nAvgLen <= 0)
		        nAvgLen = this.Len;
	        //ASSERT(nAvgLen > 0);
	        if (nAvgLen <= 0)
		        nAvgLen = 1;

	        StringCollection strings = new StringCollection();
            data = data.Replace("\r", "");
            string [] lines = data.Split(new char[] { '\n' });
            foreach (string l in lines)
            {
                string line = l;
                while (!line.IsNullOrEmpty())
                {
                    int nLenHeadLine = Math.Min(nAvgLen, line.Length);
                    string sHeadLine = line.Left(nLenHeadLine);

                    //try to extend length: head line is all lowercase and it is shorter than actual column width
                    while (nLenHeadLine < line.Length)
                    {
                        int w = fwf.GetStringWidth(sHeadLine);
                        if (w >= fwf.m_nWidth)
                            break;

                        nLenHeadLine++;
                        sHeadLine = line.Left(nLenHeadLine);
                    }
                    //reduce length to fit actual column width: head line is all uppercase and it is longer than actual column width
                    while (nLenHeadLine > 1)
                    {
                        int w = fwf.GetStringWidth(sHeadLine);
                        if (w < fwf.m_nWidth)
                            break;

                        nLenHeadLine--;
                        sHeadLine = line.Left(nLenHeadLine);
                    }
                    if (nLenHeadLine == line.Length)
                    {
                        strings.Add(sHeadLine);
                        break;
                    }
                    //length fit actual column width: now it searches a point to break line
                    int nLenBreak = 0;
                    for (nLenBreak = nLenHeadLine; nLenBreak > 0 && (Char.IsLetterOrDigit(sHeadLine[nLenBreak - 1]) || sHeadLine[nLenBreak - 1] == '/'); nLenBreak--) ;
                    //if there is not a break point it get all head line
                    if (nLenBreak == 0) nLenBreak = nLenHeadLine;

                    sHeadLine = line.Left(nLenBreak);
                    sHeadLine = sHeadLine.Trim();

                    if (!sHeadLine.IsNullOrEmpty())
                        strings.Add(sHeadLine);

                    //---- loop on tail line
                    line = line.Mid(nLenBreak);
                }
            }
            
            return strings.Count > 0 ? strings : null;
        }

		//----------------------------------------------------------------------------
		StringCollection SplitStringOld(string data)
		{
            data = data.TrimStart('\r', '\n');

            int dataLength = data.Length;
			if (dataLength <= 0)
				return null;
         
            //Applico una tolleranza del 10% come fa woorm c++ (quindi equivale a dire che ho uno spazio a disposizione del 90%)
            //senza questa tolleranza capita che una stringa non sia splittata, ma poi a video non si riesce a vedere
            int maxLen = (int)(this.Len * engine.ColumnWidthPercentage);

			StringCollection stringLines = new StringCollection();

			int startChar = 0;
			int realChars;

			while (startChar < dataLength)
			{
				for	(
					realChars = 0;
					realChars < maxLen && startChar + realChars < dataLength;
					realChars++
					)
					if (data[startChar + realChars] == '\n')
					{
						// se si esce invece per naturale terminazione del loop
						// realChars e` gia` incrementato
						realChars++;
						break;
					}

				// mi posiziono sul carattere successivo a quello teoricamente ultimo
				// (male che vada e` il '\0' di terminazione)
				int idx = startChar + realChars;
				if (idx < dataLength && data[idx - 1] != '\n')
				{
					if (!Char.IsWhiteSpace(data[idx]))
					{
						// se mi trovo ancora su un carattere vado a sinistra cercando il primo
						// separatore
						while (idx > startChar && !Char.IsWhiteSpace(data[idx - 1]))
							idx--; 

						// se la parola e` troppo lunga si utilizzano
						// tutti gli realChars calcolati precedentemente
						if (idx == startChar)
							idx = startChar + realChars;
					}
					else
					{
						// se mi trovo su un blank vado a destra cercando il primo
						// carattere buono
						while (idx < dataLength && Char.IsWhiteSpace(data[idx]))
							idx++;
					}
				}

				// numero di caratteri totali della sotto-stringa
				realChars = idx - startChar;
				idx--;

				// strip blank, CR e LF
				while (idx >= startChar && Char.IsWhiteSpace(data[idx]))
					idx--;

				if (idx >= startChar)
				{
					string splittedString = data.Substring(startChar, idx - startChar + 1);
					int realOccupancy = CalculateDataOccupancy(splittedString, maxLen);
					if (realOccupancy < maxLen)
					{
						splittedString = data.Substring(startChar, realOccupancy);
						realChars = splittedString.Length;
					}
					stringLines.Add(splittedString);
				}
				else
					stringLines.Add("");

				startChar += realChars;
			}

			// non è necessario splittare la stringa su più righe
			return (stringLines.Count > 1) ? stringLines : null;
		}

		//----------------------------------------------------------------------------
		public bool Display (ReportEngine repEngine)
		{
			if (Hidden) return true;

			// per i campi liberi ed i totali di colonna di tipo non si esegue la rasterizzazione
			if (!IsColumn || IsColTotal)
				return Write(repEngine);

			// tentativo di visualizzare una colonna che si sta rasterizzando gia` o mentre si e`
			// in fase di rasterizzazione avanzata (cioe` dalla seconda riga in poi)
			if (substrings != null || (ownDisplayTable != null && ownDisplayTable.MultiLineFieldsCurrLine > 0))
			{
				Debug.WriteLine(WoormEngineStrings.BadInterline + " " + PublicName);
				return true;
			}

 		    // Si "rasterizza" la stringa corrente se la colonna e` multiline (NumDec > 0)
            bool multiLine = NumDec > 0 && !repEngine.Session.XmlReport;
            if (EventData != null && EventData.GetType().Name == "String" && multiLine)
			{
				string data = (string)EventData;
                //TODO RSWEB splitstring 
                //if (engine.OptimizedLineBreak)
                //    substrings = SplitStringOptimized(data);
                //else
                    substrings = SplitStringOld(data);

				// si incrementa il numero di colonne rasterizzate
				if (substrings != null)
					ownDisplayTable.MultiLineFieldsNum++;
			}

			return Write(repEngine);
		}

		//----------------------------------------------------------------------------
		private void reset()
		{
			ownDisplayTable.MultiLineFieldsNum--;
			substrings = null;
		}
        //----------------------------------------------------------------------------
        public bool WriteArray(ReportEngine repEngine)
        {
            string Name = (repEngine.Session.XmlReport && this.tagXml.Length > 0) ? this.tagXml : this.Name;

            bool ok = repEngine.OutChannel.WriteArray(Name, Id, data, WoormType, Valid);
            return ok;
        }

        //----------------------------------------------------------------------------
        public bool Write(ReportEngine repEngine)
		{
            string Name = (repEngine.Session.XmlReport && this.tagXml.Length > 0) ? this.tagXml : this.Name;

			// se e` un campo colonnare e sto "rasterizzando" la riga della tabella con 
			// numero di riga raster successive alla prima scrivo solo se ho delle sotto-stringhe
			// ancora da smaltire
			if	(
					!IsColTotal &&
					IsColumn && 
					ownDisplayTable != null &&
					ownDisplayTable.MultiLineFieldsCurrLine > 0 &&
					substrings == null
				)
				return true;

            if (noXml)
                return true;

			bool lastLine = 
				(IsColumn ? ownDisplayTable != null && ownDisplayTable.MultiLineFieldsCurrLine > 0 : false ) &&
				substrings != null;

			ushort	tableId = (ownDisplayTable == null ? (ushort)0 : ownDisplayTable.InternalId);
			object data = null;
			string dataStr;

			// sending a ColTotal it behaves like a sending last nextLine
			// and then the owner table must exist
			if (IsColumn && ownDisplayTable != null)
				if (IsColTotal)
					ownDisplayTable.TableFull = true;
				else
					if (!ownDisplayTable.ExecOverflowActions())
						goto writeError;

			if (IsColumn && substrings != null)
			{
				// ci si e` protetti gia` prima sull'esaurimento delle sotto-stringhe
				dataStr = substrings[ownDisplayTable.MultiLineFieldsCurrLine];
				data = (object)dataStr;

				// se e` stata scritta l'ultima sotto-stringa si decrementa il numero
				// di campi multilinea della tabella che si devono ancora scrivere
				int x1 = ownDisplayTable.MultiLineFieldsCurrLine;
				int x2 = substrings.Count - 1;
				//ITRI qui impazzisce il compilatore perchè sbaglia la condizione
				if (x1 == x2)
					reset();
			}
			else
				// devo aver estratto qualcosa con la rule per farlo vedere. 
				data = EventData;

			// da indicazione al viewer di visualizzare la stringa: Primo(a)
			if (Valid && inputLimit == Token.LOWER_LIMIT && ObjectHelper.IsLowerValue(data))
			{
				if (!repEngine.OutChannel.WriteIDCommand(tableId, Name, Id, data, RdeWriter.Command.LowerInput, WoormType))
					goto writeError;

				return true;
			}

			// da indicazione al viewer di visualizzare la stringa: Ultimo(a)
			
			if (Valid && inputLimit == Token.UPPER_LIMIT)
			{
				if (ObjectHelper.IsUpperValue(data, engine.Report.MaxString))
				{
                    if (!repEngine.OutChannel.WriteIDCommand(tableId, Name, Id, data, RdeWriter.Command.UpperInput, WoormType))
						goto writeError;

					return true;
				}
				if (this.WoormType == "String" && data != null)
				{
					String s = (string) data;
					data = ObjectHelper.TrimMaxString(s, repEngine.Report.MaxString);
				}
			}

			bool ok = true;
            if (Valid || repEngine.Report.ReportSession.WriteNotValidField)
			{
				if (IsColTotal)
                    ok = repEngine.OutChannel.WriteColTotal(tableId, Name, Id, data, lastLine);
				else if (IsSubTotal)
                    ok = repEngine.OutChannel.WriteSubTotal(tableId, Name, Id, data, lastLine);
				else if (IsColumn)
                    ok = repEngine.OutChannel.WriteCell(tableId, Name, Id, data, lastLine, WoormType, Valid);
				else
                    ok = repEngine.OutChannel.WriteField(Name, Id, data, WoormType, Valid);
			}
			if (!ok) 
				goto writeError;

			if (ownDisplayTable!= null && !IsColTotal)
				ownDisplayTable.DataDisplayed = true;

			return true;

			writeError:
			repEngine.SetError(string.Format(WoormEngineStrings.CannotDisplayField, PublicName));
				return false;
		}

		//----------------------------------------------------------------------------
		protected bool ParseDisplayAttr(Parser lex, ReportEngine engine)
		{
			if (lex.Matched(Token.HIDDEN))
				hidden = true;

			if (lex.Matched(Token.REINIT))
			{
				reinit = true;

				if (lex.LookAhead() != Token.INPUT)
				{
					lex.SetError(WoormEngineStrings.MissingTableName);
					return false;
				}
			}
			else if (lex.Matched(Token.STATIC))
			{
				statico = true;

				if (lex.LookAhead() != Token.INPUT)
				{
					lex.SetError(WoormEngineStrings.MissingTableName);
					return false;
				}
			}

			switch (lex.LookAhead())
			{
				case Token.COLUMN:
					IsColumn2 = isColumn = true;
					lex.SkipToken();

					if (lex.Matched(Token.OF))
						ownDisplayTable = engine.ParseDisplayTable(lex);
					else
						if (!lex.Error)
						{
							if (engine.RepSymTable.DisplayTables.Count != 1)
							{
								lex.SetError(WoormEngineStrings.MissingTableName);
								return false;
							}

							// search the unique DisplayTable
							ownDisplayTable = (DisplayTable)engine.RepSymTable.DisplayTables[0];
						}

					if (!IsColumn)
					{
						lex.SetError(WoormEngineStrings.TableUndefined);
						return false;
					}
					ownDisplayTable.AddColumn(this);
					return true;

				case Token.INPUT:

					lex.SkipToken();
					input = true;

					if (eventFunction != null)
					{
						lex.SetError(string.Format(WoormEngineStrings.IllegalFunction, PublicName));
						return false;
					}
					SetReadOnly();
					return true;

				case Token.SUBTOTAL:
				case Token.COLTOTAL:
					// the public id is used to identify the correct subtotal/coltotal
					if (lex.Matched(Token.SUBTOTAL))
						isSubTotal = true;
					else
						if (lex.ParseTag(Token.COLTOTAL))
							isColTotal = true;
						else
							return false;

					if (eventFunction == null)
					{
						lex.SetError(string.Format(WoormEngineStrings.MissingFuction, PublicName));
						return false;
					}
					string name = "";
					Field field = null;

					// if it isn't a expression function we can omit the owner column name
					if (lex.Matched(Token.OF))
						lex.ParseID(out name);

					if (lex.Error) return false;

					if (eventFunction.IsAnExpression)
					{
						if (name.Length == 0)
						{
							lex.SetError(WoormEngineStrings.ColumnNameExpected);
							return false;
						}

						field = engine.RepSymTable.Fields.Find(name);

						if (field == null)
						{
							lex.SetError(string.Format(ExpressionManagerStrings.UnknownField, name));
							return false;
						}
					}
					else
					{
						field = eventFunction.FunctionField;

						if (name.Length != 0 && string.Compare(field.PublicName, name, StringComparison.OrdinalIgnoreCase) == 0)
						{
							lex.SetError(string.Format(WoormEngineStrings.IllegalTotalName, name));
							return false;
						}

						name = field.PublicName;
					}

					if (!field.IsColumn)
					{
						lex.SetError(string.Format(WoormEngineStrings.FieldNotColumn, name));
						return false;
					}

					if (field.Hidden)
						hidden = true;

					originalAlias = Id;
					Id = field.Id;
					ownDisplayTable = field.DisplayTable;
				
					if (IsColumn)
						ownDisplayTable.AddColumn(this);

					return true;

				default: return !lex.Error;
			}
		}

        //---------------------------------------------------------------------------
        static public Field ParseFieldInfo(Parser lex, RepSymTable symTable, ReportEngine engine)
        {
            ushort tag = 0;
            ushort item = 0;
            string aType = "String";
            string woormType = "";
            string baseType = "";

            if (!DataTypeParser.Parse(lex, engine.Session.Enums, out aType, out woormType, out tag, out baseType))
                return null;

            string fieldName;
            if (!lex.ParseID(out fieldName))
                return null;

            // non è ammesso qualificare i nomi delle variabili per non incorrere in
            // collisioni con namespace e colonne qualificate di database
            if (fieldName.IndexOf('.') > -1)
            {
                lex.SetError(string.Format(WoormEngineStrings.IllegalFieldName, fieldName));
                return null;
            }

            if (symTable.Fields.Contains(fieldName))
            {
                lex.SetError(string.Format(WoormEngineStrings.FieldExist, fieldName));
                return null;
            }

            Field rf = new Field(aType, fieldName, tag, item, engine);
            rf.WoormType = woormType;
            if (rf.IsArray) 
                rf.ArrayBaseType = baseType;
		    
            return rf;
        }

		//----------------------------------------------------------------------------
		public bool Parse (Parser lex, ReportEngine engine)
		{
			// Match of optional field precision
			int	length	= 0;
			int	dec	= 0;
		                    
			if (lex.LookAhead(Token.SQUAREOPEN) && !lex.ParseSubscr(out length, out dec))
				return false;

			switch (DataType)
			{
				case "Int32":  
				case "Int64":  
				case "Single":	
				case "Double":	
				{
					if (length > 0 && dec >= length)
					{
						lex.SetError(WoormEngineStrings.IllegalPrecision);
						return false;
					}
					break;
				}
			}

			SetPrecision(length, dec);

			// Match of optional EventFunction
			if (lex.Matched(Token.ASSIGN))
			{
				eventFunction = new EventFunction(Session, engine.RepSymTable.Fields, this);
				eventFunction.StopTokens = new StopTokens(new Token[] {Token.ALIAS});
				if (!eventFunction.Parse(lex)) return false;
			}

			// Match of ALIAS identifier
			ushort wInternalId;
			if (!lex.ParseAlias(out wInternalId))
                return false;

			if (wInternalId <= 0)
			{
				lex.SetError(WoormEngineStrings.IllegalAlias);
				return false;
			}

			if (engine.RepSymTable.Fields.Contains(wInternalId))
			{
				lex.SetError(string.Format(WoormEngineStrings.AliasExist, wInternalId.ToString()));
				return false;
			}

			Id = wInternalId;

            //pars As "alias name"
            if (lex.Matched(Token.AS) && !lex.ParseID(out tagXml))
            {
                lex.SetError(string.Format(WoormEngineStrings.BadAlternativeName, wInternalId.ToString()));
                return false;
            }
            if (lex.Matched(Token.NO_XML))
                noXml = true;

            if (lex.Matched(Token.CONTEXT) && !lex.ParseString(out contextName))
            {
                lex.SetError(string.Format(WoormEngineStrings.BadAlternativeName, wInternalId.ToString()));
                return false;
            }

			// Match of display attribute
			if (!ParseDisplayAttr(lex, engine)) return false;

			// Match INIT expression
			if (lex.Matched(Token.INIT) && lex.ParseTag(Token.ASSIGN))
			{
                AddMethods(lex, engine);

				initExpression = new WoormEngineExpression(engine, Session, engine.RepSymTable.Fields);
				if (!initExpression.Compile(lex, CheckResultType.Compatible, DataType))
					return false;
			}

			return !lex.Error;
		}

        //----------------------------------------------------------------------------
        List<IFunctionPrototype> tbMethods = null;

        bool AddMethods (string sClassName, ReportEngine engine)
        {
	        if (sClassName.IsNullOrEmpty())
		        return false;

            if (DataType != "Int64" && DataType != "Var" && DataType != "Long")
		        return false;

            if (tbMethods == null)
                tbMethods = new List<IFunctionPrototype>();

	        string [] arClasses = sClassName.Split(new char [] {','});
	        foreach (string c in arClasses)
	        {
		        string cl = c.Trim();

                List<IFunctionPrototype> methods = null;
		        if (engine.Session.Functions.ClassMethods.TryGetValue(cl, out methods))
		        {
                    foreach(IFunctionPrototype fp in methods)
                    {
                        tbMethods.Add(fp);
                    }
		        }
	        }
	        return true;
        }

        public bool AddMethods(Parser lex, ReportEngine engine)
        {
	        if (
                    (DataType == "Int64"|| DataType == "Var" || DataType == "Long" )
			        &&
                    lex.LookAhead(Token.ID)
		        )
	        {
                
                string fName = lex.CurrentLexeme;

                FunctionPrototype aFuncPrototype = engine.Session.Functions.GetPrototype(fName);

		        if (aFuncPrototype != null)
		        {
			        return AddMethods(aFuncPrototype.ReturnBaseType, engine);
		        }
	        }
	        return false;
        }

        //------------------------------------------------------------------------------
        public IFunctionPrototype FindMethod(string name) 
        {
            if (tbMethods == null)
		        return null;

            IFunctionPrototype pF = tbMethods.Find(
                    new Predicate<IFunctionPrototype>
                        ( (IFunctionPrototype fp) 
                            => { 
                                    string mName = fp.Name;
                                    int idx = mName.IndexOf('_');   //
                                    if (idx > 0)
                                        mName = mName.Mid(idx + 1);
                                    return name.CompareNoCase(mName);
                                }
                     ));

	        return pF;
        }

		///<summary>
		///Metodo che associa al field la DisplayTable che appartiene al layout passato come argomento
		/// </summary>
		//----------------------------------------------------------------------------
		internal void ReattachCurrentDisplayTable(string layout)
		{
			if (ownDisplayTable == null)
				return;
			if (string.IsNullOrEmpty(layout))
				layout = DisplayTable.LayoutDefaultName;
			
			//se la display table associata al Field e' gia quella del layout corrente: esco.
			if (string.Compare(ownDisplayTable.LayoutTable,layout) == 0)
				return;
			
			//recupero la displaytable di questo filed nel corrente layout
			DisplayTable dt = this.engine.RepSymTable.DisplayTables.Find(ownDisplayTable.PublicName, layout);
			//la assegno al field
			if (dt != null)
				ownDisplayTable = dt;
		}

		//----------------------------------------------------------------------------
		internal bool Unparse(Unparser unparser, bool noDispTableName)
		{
			if (IsPredefinedField())
			    return true;

			UnparseDataType(unparser);

			unparser.WriteID(name, false);

			if (Len > 0 || NumDec > 0)
				unparser.WriteSubscr(Len, NumDec, false);

			if (eventFunction != null && !eventFunction.IsEmpty)
			{
				unparser.WriteTag(Token.ASSIGN, false);
				eventFunction.Unparse(unparser);
			}

			unparser.WriteAlias(OriginalAlias, false);

			if (!tagXml.IsNullOrEmpty()) 
			{
				unparser.WriteTag(Token.AS, false);
				unparser.WriteID(tagXml, false);
			};
			if (noXml)
				unparser.WriteTag(Token.NO_XML, false);

            if (!contextName.IsNullOrEmpty())
            {
                unparser.WriteTag(Token.CONTEXT, false);
                unparser.WriteString(contextName, false);
            };

			UnparseDisplayAttribute(unparser, noDispTableName);

			if (InitExpression != null && !InitExpression.IsEmpty)
			{
				unparser.WriteTag(Token.INIT, false);
				unparser.WriteTag(Token.ASSIGN, false);
				string str = InitExpression.ToString();
				unparser.Write(str);
			}

			unparser.WriteSep(true);

			return true;
		}

		//----------------------------------------------------------------------------
		private bool IsPredefinedField()
		{
			return Id >= SpecialReportField.REPORT_LOWER_SPECIAL_ID; 
		}

		//----------------------------------------------------------------------------
		private void UnparseDisplayAttribute(Unparser unparser, bool noDispTableName)
		{
			string defaultDispTbl = string.Empty;

			if (Hidden)
			{
				unparser.WriteBlank();
				unparser.WriteTag(Token.HIDDEN, false);
			}

			if (ReInit)
			{
				unparser.WriteBlank();
				unparser.WriteTag(Token.REINIT, false);
			}
			else if (Statico)
			{
				unparser.WriteBlank();
				unparser.WriteTag(Token.STATIC, false);
			}

			if (Input)
			{
				unparser.WriteBlank();
				unparser.WriteTag(Token.INPUT, false);

				return;
			}

			if (IsColumn)
			{
				unparser.WriteBlank();
				unparser.WriteTag(Token.COLUMN, false);

				
				if (!noDispTableName)
				{
					if (!DisplayTable.PublicName.IsNullOrEmpty())
					{
						unparser.WriteBlank();
						unparser.WriteTag(Token.OF, false);
						unparser.WriteID(DisplayTable.PublicName, false);
					}
				}
				return;
			}

			if (IsSubTotal ||  IsColTotal)
			{
				unparser.WriteBlank();
				if (IsSubTotal)
					unparser.WriteTag(Token.SUBTOTAL, false);
				else
					unparser.WriteTag(Token.COLTOTAL, false);

				// if it isn't an expression function we omit the owner column pszName
				if (EventFunction != null && EventFunction.IsAnExpression)
				{
					unparser.WriteTag(Token.OF, false);
					unparser.WriteID(EventFunction.GetPublicName(), false);
				}
			}
		}

		//----------------------------------------------------------------------------
		public void UnparseDataType(Unparser unparser, bool indent = true)
		{
            unparser.UnparseDataType(WoormType, ArrayBaseType, EnumTag, false, indent);
            if (EnumTag != 0)
                unparser.Write(" /* " + Session.Enums.TagName(EnumTag) + " */ ");
		}
	}

    //=============================================================================
    public class FieldException : Exception
    {
        public FieldException(string message) : base(message) { }
        public FieldException(string message, Exception inner) : base(message, inner) { }
    }
}

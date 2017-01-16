using System;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;

using Microarea.RSWeb.Applications;
using Microarea.RSWeb.CoreTypes;
using Microarea.RSWeb.Generic;
using Microarea.RSWeb.Lexan;
using Microarea.RSWeb.ExpressionManager;

namespace Microarea.RSWeb.WoormEngine
{

    /// <summary>
    /// AskEntry
    /// </summary>
    //============================================================================
    [Serializable]
	[KnownType(typeof(Field))]
	[KnownType(typeof(List<string>))]
	public class AskEntry : ISerializable
	{
		const string CAPTION = "Caption";
		const string FIELD = "Field";
		const string CONTROLSTYLE = "ControlStyle";
		const string ENABLED = "enabled";
		const string ISREFERENCED = "isReferenced";
		const string ENUMSLIST = "enumsList";
		const string SELECTEDENUMINDEX = "selectedEnumIndex";

		private Enums Enums { get { return Group.Dialog.Report.ReportSession.Enums; }}

		public string			Caption;
        public Expression       CaptionExpr;
        public Expression       ReadOnlyExpr;
		public bool				StaticHidden = false;
		public Expression		WhenExpr;	//WhenExpr != null => DynamicHidden
		public int				Len = 15;
		public int				Rows = 0;
		public int				ControlStyle = -1;
        public bool             LeftAligned = false;
        public bool             LeftTextBool = false;
        public Token			CaptionPos = Token.DEFAULT;
		public ReferenceObject	Hotlink = null;
		public Field			Field = null;
		public AskGroup			Group;
        public bool             MultiSelections = false;
		//public bool			DynamicHidden = false;

		//----------------------------------------------------------------------------
		public 	StringCollection UserChanged { get { return Group.Dialog.UserChanged; }}

		//---------------------------------------------------------------------------
		public string ControlStyleAttributeValue
		{
			get
			{
				switch (ControlStyle)
				{
					case AskStyle.EDIT_BOOL_STYLE: return XmlWriterTokens.AttributeValue.Text;
					case AskStyle.CHECK_BOX_BOOL_STYLE: return XmlWriterTokens.AttributeValue.Check;
					case AskStyle.RADIO_BUTTON_BOOL_STYLE: return XmlWriterTokens.AttributeValue.Radio;
					case AskStyle.COMBO_STYLE: return XmlWriterTokens.AttributeValue.Combo; 
				}
		
				return XmlWriterTokens.AttributeValue.Text;
			}
		}

		//----------------------------------------------------------------------------
		public string LocalizedCaption 
		{ 
			get 
			{
                if (CaptionExpr != null)
                {
                    Value v = CaptionExpr.Eval();
                    if (!CaptionExpr.Error)
                        return (string)v.Data;
                }

				if (Group == null || Group.Dialog == null || Group.Dialog.Report == null) 
					return Caption;

				return Group.Dialog.Report.Localizer.Translate(Caption);
			}
		}
		
		// controlla l'espressione di hidden per nascondere il control
		//--------------------------------------------------------------------------
		public bool Hidden
		{
			get
			{
				if (WhenExpr != null)
				{
					Value v = WhenExpr.Eval();
					if (!WhenExpr.Error)
						return !(bool)v.Data;
				}	
				return false;
			}
		}
		
		// controlla l'espressione di readonly per disabilitare il control
		//--------------------------------------------------------------------------
		public bool Enabled
		{
			get
			{
				if (ReadOnlyExpr != null)
				{
					Value v = ReadOnlyExpr.Eval();
					if (!ReadOnlyExpr.Error)
						return !(bool)v.Data;
				}	
				return true;
			}
		}

		//----------------------------------------------------------------------------
		public AskEntry(AskGroup group)
		{
			this.Group = group;
		}

			//----------------------------------------------------------------------------
		public AskEntry()
		{
			//TODO Silvano costruttore per Deserializzatore
		}

		//----------------------------------------------------------------------------
		public void GetObjectData(SerializationInfo info,StreamingContext context)
		{
			info.AddValue(CAPTION, LocalizedCaption);
			info.AddValue(FIELD, Field);
			info.AddValue(CONTROLSTYLE, ControlStyle);
			info.AddValue(ENABLED, Enabled);
			//Se e' referenziato devo fare un postBack per ricalcolare i campi della askDialog sul server
			bool isReferenced = Group.Dialog.IsReferenced(this);
			info.AddValue(ISREFERENCED, isReferenced);

			//se e' un enum devo aggiungere la lista dei valori leggibile
			if (Field.Data is DataEnum)
			{
				DataEnum data = Field.Data as DataEnum;
				EnumItems items = Enums.EnumItems(Enums.TagName(data));
				List<string> list = new List<string>();

				//TODO SILVANO
				if (items == null)
						list.Add("BAD ENUM");
				else
					for (int i = 0; i < items.Count; i++) 
					{
						EnumItem ei = items[i];
						if (data.Item == ei.Value) 
						{
							info.AddValue(SELECTEDENUMINDEX, i);
						}
						list.Add(ei.LocalizedName);
					}
				info.AddValue(ENUMSLIST, list);
			}
		}

		// Valorizza i limiti superiore/inferiore solo se l'utente non ha cambiato i valori
		// o se il campo non viene inizializzato allo start (zona variables)
		//--------------------------------------------------------------------------
		public string SetUpperLowerLimit()
		{
			string text = ObjectHelper.CastString(Field.AskData);
			if (UserChanged.Contains(Field.Name))
				return text;
			
			switch (Field.InputLimit)
			{
				case Token.LOWER_LIMIT : 
				case Token.UPPER_LIMIT : 
					if (ObjectHelper.IsCleared(Field.AskData))
						text = ""; 
					break;
			}

			// nel caso delle date se sono una data null (31/12/1799) metto lo stesso stringa vuota
			// perch� si comporta cos� woorm c++
			if (Field.DataType == "DateTime" && ObjectHelper.IsCleared(Field.AskData))
				text = "";

			return text;
		}

		//--------------------------------------------------------------------------
		public bool Unparse(Unparser unparser, AskGroup askGroup)
		{
			unparser.WriteID(Field.Name, false);

			if (StaticHidden)
			{
				unparser.WriteTag(Token.HIDDEN, false);
				unparser.WriteSep(true);
				return true;
			}

            int nStyle = ControlStyle;

            if (LeftAligned)
                nStyle |= AskStyle.BOOL_BTN_LEFT_ALIGNED;

            if (LeftTextBool)
                nStyle |= AskStyle.BOOL_BTN_LEFT_TEXT;


            if (Field != null && Field.DataType != "Boolean"  && nStyle == AskStyle.EDIT_BOOL_STYLE)
				nStyle = 0;

			if (nStyle > 0)
			{
				unparser.WriteTag(Token.STYLE, false);
				unparser.WriteTag(Token.ASSIGN, false);
				unparser.Write(nStyle, false);
				unparser.WriteBlank();
			}

			if (CaptionPos != Token.DEFAULT)
				unparser.WriteTag(CaptionPos, false);

			unparser.WriteTag(Token.PROMPT, false);
			unparser.WriteTag(Token.ASSIGN, false);

			if (CaptionExpr != null)
			{
				unparser.WriteTag(Token.ROUNDOPEN, false);
				unparser.WriteExpr(CaptionExpr.ToString(), false);
				unparser.WriteTag(Token.ROUNDCLOSE, false);
			}
			else
			{
				if (!Caption.IsNullOrEmpty())
					unparser.WriteString(
						unparser.IsLocalizableTextInCurrentLanguage()
						? unparser.LoadReportString(Caption)
						: Caption, false);
			}

			if (Field.InputLimit != Token.NOTOKEN)
				unparser.WriteTag(Field.InputLimit, false); 
	
			if (ReadOnlyExpr != null)
			{
				unparser.WriteTag(Token.READ_ONLY, false);
				unparser.WriteExpr(ReadOnlyExpr.ToString(), false);
			}
			if (WhenExpr != null)
			{
				//if (DynamicHidden)  //TODOLUCA, non valorizzato, e forse non serve neanche
				//    unparser.WriteTag(Token.DYNAMIC, false);

				unparser.WriteTag(Token.WHEN, false);
				unparser.WriteExpr(WhenExpr.ToString(), false);
			}

			WriteHotlink(unparser);

			unparser.WriteSep(true);
			return true;
		}

		//--------------------------------------------------------------------------
		private void WriteHotlink(Unparser unparser)
		{
			//TODOLUCA Hotlink.Prototype.NameSpace == m_nsHotLink?? da verificare	
			if (Hotlink == null || !Hotlink.Prototype.NameSpace.IsValid())
				return;

			unparser.WriteTag(Token.HOTLINK, false);
			unparser.WriteID(Hotlink.Prototype.NameSpace.GetNameSpaceWithoutType(), false);

			// le parentesi diventano obbligatorie
			unparser.WriteTag(Token.ROUNDOPEN, false);
			
			// unparse dei parametri di personalizzazione dell'hotlink
			foreach (Expression exp in Hotlink.ActualParams)
			{
				unparser.WriteExpr(exp.ToString().Replace("\n\t", ""), false);
				
				if (exp != Hotlink.ActualParams[Hotlink.ActualParams.Count - 1])
					unparser.WriteTag(Token.COMMA, false);
			}	

			unparser.WriteTag(Token.ROUNDCLOSE, false);

            if (this.MultiSelections)
                unparser.WriteTag(Token.MULTI_SELECTIONS, false);
		}
	}

	/// <summary>
	/// AskGroup
	/// </summary>
	//============================================================================
	[Serializable]
	[KnownType(typeof(List<AskEntry>))]
	public class AskGroup : ISerializable
	{
		//stringhe usate per serializzare
		const string ISVISIBLE = "IsVisible"; 
		const string CAPTION = "Caption";
		const string ENTRIES = "Entries";
		const string HIDDEN = "Hidden";

		private List<AskEntry> entries = new List<AskEntry>();

		public bool IsVisible = true;
		public string Caption;
		public Token CaptionPos = Token.DEFAULT;
		public int MaxCaptionLen = 0;
		public int MaxEntryLen = 0;
		public AskDialog Dialog;
		public Expression WhenExpr;

		//----------------------------------------------------------------------------
		public string LocalizedCaption
		{
			get
			{
				if (Dialog == null || Dialog.Report == null)
					return Caption;

				return Dialog.Report.Localizer.Translate(Caption);
			}
		}

		// controlla l'espressione di hidden per nascondere il control
		//--------------------------------------------------------------------------
		public bool Hidden
		{
			get
			{
				// se tutte le entries sono nascoste,
				// nascondo anche il gruppo
				bool allEntriesHidden = true;
				foreach (AskEntry anEntry in Entries)
					if (!anEntry.Hidden)
					{
						allEntriesHidden = false;
						break;
					}

				if (allEntriesHidden)
					return true;

				// altrimenti controllo la WHEN expression
				if (WhenExpr != null)
				{
					Value v = WhenExpr.Eval();
					if (!WhenExpr.Error)
						return !(bool)v.Data;
				}

				//di default, � visibile
				return false;
			}
		}

		//----------------------------------------------------------------------------
		public bool IsEmpty { get { return entries.Count == 0; } }

		//----------------------------------------------------------------------------
		public List<AskEntry> Entries { get { return this.entries; } }

		//----------------------------------------------------------------------------
		public AskGroup(AskDialog dialog)
		{
			this.Dialog = dialog;
		}


		//----------------------------------------------------------------------------
		public AskGroup()
		{
			//TODO SILVANO costruttore per Deserializzatore
		}

		//----------------------------------------------------------------------------
		public void GetObjectData(SerializationInfo info,StreamingContext context)
		{
			info.AddValue(ISVISIBLE, IsVisible);
			info.AddValue(CAPTION, Caption);
			info.AddValue(ENTRIES, entries);
			info.AddValue(HIDDEN, Hidden);
		}

		//----------------------------------------------------------------------------
		public void Add(AskEntry askEntry)
		{
			Entries.Add(askEntry);
		}

		//----------------------------------------------------------------------------
		public bool Unparse(Unparser unparser)
		{
			if (entries.Count == 0)
				return true;

			string strDefaultTitle = "Group Without Title";

			unparser.IncTab();
			unparser.WriteBegin(false);
			unparser.WriteBlank();

			if (!Caption.IsNullOrEmpty() && Caption.IndexOf(strDefaultTitle) == -1)
				unparser.WriteString(
					unparser.IsLocalizableTextInCurrentLanguage()
					? unparser.LoadReportString(Caption)
					: Caption, false);

			if (Hidden)
				unparser.WriteTag(Token.HIDDEN, false);

			if (WhenExpr != null)
			{
				unparser.WriteTag(Token.WHEN, false);
				unparser.WriteExpr(WhenExpr.ToString(), false);
				unparser.WriteTag(Token.SEP, false);
			}

			if (CaptionPos != Token.DEFAULT)
			{
				unparser.WriteTag(CaptionPos, false);
				unparser.WriteTag(Token.PROMPT, false);
			}

			unparser.WriteLine();

			unparser.IncTab();
			foreach (AskEntry item in entries)
				item.Unparse(unparser, this);
			unparser.DecTab();

			unparser.WriteEnd(true);
			unparser.DecTab();

			return true;
		}
	}

	//=============================================================================
	public static class AskStyle
	{ 
		public const int CHECK_BOX_BOOL_STYLE = 0x0000;
		public const int RADIO_BUTTON_BOOL_STYLE = 0x0001;
		public const int EDIT_BOOL_STYLE = 0x0002;
		public const int BOOL_BTN_LEFT_ALIGNED = 0x0004;
		public const int COMBO_STYLE = 0x0008;
        public const int BOOL_BTN_LEFT_TEXT = 0x0020;
    }

    /// <summary>
    /// AskDialog
    /// </summary>
    //=============================================================================
    [Serializable]
	[KnownType(typeof(List<AskGroup>))]
	[KnownType(typeof(AskGroup))]
	public class AskDialog : ISerializable
	{
		const string GROUPS = "Groups";
		const string LOCALIZEDFORMTITLE = "LocalizedFormTitle";
		
		private List<AskGroup> groups = new List<AskGroup>();
		private Token		captionPos = Token.DEFAULT;
		private AskGroup	askGroup = null;
		private AskEntry	askEntry = null;
		private AskEntry	activeAskEntry = null;
		private Report		report;
		private string		helpFileName;
		private long		helpData = 0;
		private bool		onAsk = false;

		public Block		BeforeActions;
		public Block		AfterActions;
		public Expression	OnExpr;
		public Expression	OnMessage;
		public Expression	WhenExpr;
		public string		FormName;
		public string		FormTitle;
		public bool			EditingScheduled = false;

		//----------------------------------------------------------------------------
		public StringCollection UserChanged = new StringCollection();

		//----------------------------------------------------------------------------
		public virtual string LocalizedFormTitle	
		{
			get
			{
				if (report == null) 
					return FormTitle;

				return report.Localizer.Translate(FormTitle); 
			}
		}

		public List<AskGroup>	Groups			{ get { return this.groups; } }
		public Report			Report			{ get { return report; }}
		public TbReportSession	Session			{ get { return Report.ReportSession; }}
		public AskEntry			ActiveAskEntry	{ get { return activeAskEntry; } set { activeAskEntry = value; }}
		public bool				OnAsk			{ get { return onAsk; }}

	
		//--------------------------------------------------------------------------
		public AskDialog(SerializationInfo info, StreamingContext context)
		{
			//TODO Silvano
		}

		//--------------------------------------------------------------------------
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(GROUPS, groups);
			info.AddValue(LOCALIZEDFORMTITLE, LocalizedFormTitle);
		}
	
		//----------------------------------------------------------------------------
		public AskDialog(Report report) 
		{
			this.report = report; 
		}

		// Verifica se AskEntry � utilizzato in una qualsiasi espressione di readOnly
		// di altri AskEntry della AskDialog o di Init nella dichiarazione di variabili
		// di AskEntry
		//--------------------------------------------------------------------------
		public bool IsReferenced(AskEntry askEntry)
		{
			foreach (AskGroup askGroup in Groups)
			{
				if (askGroup.WhenExpr != null && askGroup.WhenExpr.HasMember(askEntry.Field.Name))
					return true;

				foreach (AskEntry entry in askGroup.Entries)
				{
					if (entry == askEntry) continue;

					if (entry.ReadOnlyExpr != null && entry.ReadOnlyExpr.HasMember(askEntry.Field.Name))
						return true;

					if (entry.WhenExpr != null && entry.WhenExpr.HasMember(askEntry.Field.Name))
						return true;

					if (entry.Field.InitExpression != null && entry.Field.InitExpression.HasMember(askEntry.Field.Name))
						return true;

                    if (entry.CaptionExpr != null && entry.CaptionExpr.HasMember(askEntry.Field.Name))
                        return true;
                }
			}

			return false;
		}

		// ricalcola tutte le init expression sulla base del contenuto corrente degli AskEntry
		// e solo per quelli che non sono stati modificati dall'utente. La valutazione avviene
		// nell'ordine in cui gli AskEntry sono utilizzati nella Dialog
		//----------------------------------------------------------------------------
		public void EvalAllInitExpression(ArrayList initializedFields, bool check)
		{
			foreach (AskGroup askGroup in Groups)
				foreach (AskEntry entry in askGroup.Entries)
				{
					Field field = entry.Field;
                    if (field == null)
						continue;

					if (!UserChanged.Contains(field.Name) && field.InitExpression != null)
					{
                        if (check && initializedFields != null)
                            if (initializedFields.Contains(field))
                                continue;

						Value v = field.InitExpression.Eval();

						if (!field.InitExpression.Error)
                        {
							field.SetAllData(v.Data, v.Valid);
                        }
					}

                    if (initializedFields != null)
                        initializedFields.Add(field);
				}
		}

		// verifica se deve mettere i limiti superiori o inferiori. 
		// i campi stringa vengono sempre considerati paddati in fondo di zeta 
		// per poter considerare sempre tutti i dati che inizionano per il valore dato
		//----------------------------------------------------------------------------
        public void AssignUppeLowerLimit(ArrayList initializedFields, bool check)
		{
			foreach (AskGroup askGroup in Groups)
				foreach (AskEntry entry in askGroup.Entries)
				{
					Field field = entry.Field;

                    if (report.InitParameters != null && report.InitParameters[field.Name] != null)
                        continue;
                    
                    if (field.InputLimit == Token.UPPER_LIMIT && field.DataType == "String")
					{
						field.AskData = ObjectHelper.GetMaxString(ObjectHelper.CastString(field.AskData), report.MaxString, field.Len);
						continue;
					}

                    if (check && initializedFields != null)
                        if (initializedFields.Contains(field))
                            continue;

					if (field.InitExpression == null && !UserChanged.Contains(field.Name))
					{
						if (field.InputLimit == Token.LOWER_LIMIT) 
                            field.SetLowerLimit();
						else	if (field.InputLimit == Token.UPPER_LIMIT) 
                            field.SetUpperLimit();
					}
				}
		}

		//--------------------------------------------------------------------------
		public bool SetActiveAskEntry(string name, ReferenceObject.Action action)
		{
			foreach (AskGroup askGroup in Groups)
				foreach (AskEntry entry in askGroup.Entries)
					if (string.Compare(entry.Field.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
					{
						activeAskEntry = entry;
						activeAskEntry.Hotlink.CurrentAction = action;
						return true;
					}

			activeAskEntry = null;
			return false;
		}

		//----------------------------------------------------------------------------
		public void SetHelpData(string helpname, long helpID)
		{
			helpFileName	= helpname;
			helpData		= helpID;
		}

		//----------------------------------------------------------------------------
		public void SetCaptionPos(Token captionPos)
		{
			if (askEntry != null)
			{
				askEntry.CaptionPos = captionPos == Token.DEFAULT
					? askGroup.CaptionPos
					: captionPos;
			}
			else
				if (askGroup != null)
			{
				askGroup.CaptionPos = captionPos == Token.DEFAULT
					? this.captionPos
					: captionPos;
			}
			else
				this.captionPos = captionPos;
		}

		//----------------------------------------------------------------------------
		public void AddGroup()
		{
			askGroup = new AskGroup(this);
			this.groups.Add(askGroup);
		}

		//----------------------------------------------------------------------------
		public void SetGroupVisibility(bool show)
		{
			askGroup.IsVisible = show;
		}

		//----------------------------------------------------------------------------
		public void SetGroupTitle(string Title)
		{
			askGroup.Caption = Title;
		}

		//----------------------------------------------------------------------------
		public void SetCaption(string Title)
		{
			askEntry.Caption = Title;
			if (askEntry.Caption.Length > askGroup.MaxCaptionLen) 
				askGroup.MaxCaptionLen = askEntry.Caption.Length;
		}

		//----------------------------------------------------------------------------
		public void SetEntrySize(int len)
		{ 
			SetEntrySize(len, 0);
		}

		//----------------------------------------------------------------------------
		public void SetEntrySize(int len, int prec)
		{
			Debug.Assert(askEntry.Field!= null);
			if (len < 0) return;

			askEntry.Len = len;
			if (askEntry.Len > askGroup.MaxEntryLen) 
				askGroup.MaxEntryLen = askEntry.Len;

			// la precision per i dati bool viene usata per capire lo stile del control
			// porcata di Germano che non posso cambiare per compatibilit�
			if (askEntry.Field.DataType == "Boolean")
			{
				askEntry.ControlStyle = prec & ~AskStyle.BOOL_BTN_LEFT_ALIGNED;
				askEntry.LeftAligned = (prec & AskStyle.BOOL_BTN_LEFT_ALIGNED) == AskStyle.BOOL_BTN_LEFT_ALIGNED;
				return;
			}
				
			// per le stringe la precizione indica se devo andare su pi� linee e quante sono.
			askEntry.Rows = prec;
		}

		//---------------------------------------------------------------------------
		private bool ParseHotlink(Parser lex, AskEntry askEntry)
		{
			if (!lex.Parsed(Token.HOTLINK))
				return true;

			lex.Parsed(Token.ASSIGN);
	
			askEntry.Hotlink = new ReferenceObject(this);
			
			string name;
			if (lex.LookAhead() == Token.ID)
			{
				if (!lex.ParseID(out name))
					return false;
			}
			else
			{
				if (!lex.ParseString(out name))
					return false;
			}

			// accetta anche la sintassi senza parentesi purch� non ci siano parametri
			if (lex.LookAhead() != Token.SEP)
			{
				if (!lex.Parsed(Token.ROUNDOPEN) && !lex.Parsed(Token.SQUAREOPEN))	
					return false;
				
				while (lex.LookAhead() != Token.ROUNDCLOSE && lex.LookAhead() != Token.SQUARECLOSE)
				{
					Expression param = new Expression(Session, report.SymTable.Fields);
					param.StopTokens = new StopTokens(new Token[] {Token.ROUNDCLOSE, Token.SQUARECLOSE, Token.COMMA});
					
					askEntry.Hotlink.ActualParams.Add(param);

					if (!param.Compile(lex, CheckResultType.Ignore, ""))
					{ 
						lex.SetError(string.Format
							(
							WoormEngineStrings.BadReferenceObjectArgument,
							askEntry.Hotlink.ActualParams.Count,
							name
							));
						return false;
					}

					// se sono finiti i parametri esce altrimenti deve esserci un punto e virgola
					if (lex.LookAhead() == Token.ROUNDCLOSE || lex.LookAhead() == Token.SQUARECLOSE) 
						break;
					if (!lex.ParseTag(Token.COMMA))
						return false;
				}

				if (lex.Parsed(Token.ROUNDCLOSE) ? false : ! lex.Parsed(Token.SQUARECLOSE))
					return false;
			}

            askEntry.MultiSelections = lex.Parsed(Token.MULTI_SELECTIONS);

			// cerca il prototipo con quel numero di parametri.
			askEntry.Hotlink.Prototype = Session.Hotlinks.GetPrototype(name, askEntry.Hotlink.ActualParams.Count);
			if (askEntry.Hotlink.Prototype == null)
			{
				lex.SetError(string.Format(WoormEngineStrings.UndefinedReferenceObject, name));
				return false;
			}

			// controlla che i parametri attuali siano corretti come tipo definito nel prototipo
			for (int i = 0; i < askEntry.Hotlink.ActualParams.Count; i++)
			{
				Expression actualParam = (Expression)askEntry.Hotlink.ActualParams[i];
				Parameter  prototypeParam = askEntry.Hotlink.Prototype.Parameters[i];

				// potrei aver disabilitato il controllo di tipo (es: Localizer)
				if (actualParam.SkipTypeChecking)
					continue;

				if (!ObjectHelper.Compatible(actualParam.ResultType, prototypeParam.Type))
				{ 
					lex.SetError(string.Format
						(
							WoormEngineStrings.BadReferenceObjectArgument,
							i + 1,
							name
						));
					return false;
				}
			}

            if (askEntry.Hotlink.Prototype.IsDatafile)
                askEntry.Hotlink = null;    //hkl type unsupported

			// controlla il tipo del valore di ritorno
            if (askEntry.MultiSelections)
            {
                if (!ObjectHelper.Compatible(askEntry.Field.DataType, "String"))
                {
                    lex.SetError(string.Format(WoormEngineStrings.BadReferenceObjectResultType, name));
                    return false;
                } 
            }
            else if (askEntry.Hotlink != null && !ObjectHelper.Compatible(askEntry.Field.DataType, askEntry.Hotlink.Prototype.ReturnType))
			{
				lex.SetError(string.Format(WoormEngineStrings.BadReferenceObjectResultType, name));
				return false;
			}

			return true;
		}

		//---------------------------------------------------------------------------
		public bool ParseEntries(Parser lex)
		{
			// corpo del group (entries)
			do
			{
				askEntry = new AskEntry(askGroup);
				askGroup.Add(askEntry);

				string entryName ="";
				if (!lex.ParseID(out entryName)) 
                    return false;

				askEntry.Field = report.SymTable.Fields.Find(entryName);
				if (askEntry.Field == null)
				{
					lex.SetError(string.Format(ExpressionManagerStrings.UnknownField, entryName));
					return false;
				}

				if (!askEntry.Field.ReadOnly)
				{
					lex.SetError(string.Format(WoormEngineStrings.IllegalField, entryName));
					return false;
				}

				// informo il field di far parte di una AskDialog
				askEntry.Field.Ask = true;

				if (lex.Parsed(Token.HIDDEN))
				{
					SetEntrySize(-1);
					askEntry.StaticHidden = true;
				}
				else
				{
					int controlStyle = AskStyle.EDIT_BOOL_STYLE;
					if (askEntry.Field.DataType == "Boolean")
					{
						controlStyle = AskStyle.CHECK_BOX_BOOL_STYLE;
						if (lex.Parsed(Token.STYLE))
						{
							if (
								!lex.ParseTag(Token.ASSIGN) ||
								!lex.ParseInt(out controlStyle)
								)
								return false;

							int style = (controlStyle & ~AskStyle.BOOL_BTN_LEFT_ALIGNED);
							switch (style)
							{
								case AskStyle.CHECK_BOX_BOOL_STYLE:
								case AskStyle.RADIO_BUTTON_BOOL_STYLE:
								case AskStyle.EDIT_BOOL_STYLE:
									break;

								default:
									{
										lex.SetError(string.Format(WoormEngineStrings.BadControlStyle, controlStyle));
										return false;
									}

							}
						}
					}
					else
					{
						if (lex.Parsed(Token.STYLE))
						{
							if (
								!lex.ParseTag(Token.ASSIGN) ||
								!lex.ParseInt(out controlStyle)
								)
								return false;

							if (controlStyle != AskStyle.COMBO_STYLE)
							{
								lex.SetError(string.Format(WoormEngineStrings.BadControlStyle, controlStyle));
								return false;
							}
						}
					}

					captionPos = Token.DEFAULT;
					switch (lex.LookAhead())
					{
						case Token.LEFT:
						case Token.RIGHT:
						case Token.TOP:
							captionPos = lex.LookAhead();
							lex.SkipToken();
							break;
					}

					if (
						!lex.ParseTag(Token.PROMPT) ||
						!lex.ParseTag(Token.ASSIGN)
						)
						return false;

					if (lex.Parsed(Token.ROUNDOPEN))
					{
						Expression captionExpr = new Expression(Session, report.SymTable.Fields);
						captionExpr.StopTokens = new StopTokens(new Token[] { Token.ROUNDCLOSE });
						if (captionExpr.Compile(lex, CheckResultType.Match, "String"))
							askEntry.CaptionExpr = captionExpr;

						lex.ParseTag(Token.ROUNDCLOSE);
					}
					else
					{
						string caption = "";
						if (!lex.ParseString(out caption))
							return false;

						SetCaption(caption);
					}

					SetCaptionPos(captionPos);

					if (askEntry.Field.DataType == "Boolean")
						SetEntrySize(askEntry.Field.Len, controlStyle);
					else
					{
						SetEntrySize(askEntry.Field.Len, askEntry.Field.NumDec);

						if (lex.Parsed(Token.LOWER_LIMIT)) askEntry.Field.SetLowerLimit();
						else if (lex.Parsed(Token.UPPER_LIMIT)) askEntry.Field.SetUpperLimit();
					}

					if (lex.Parsed(Token.READ_ONLY))
					{
						// = facoltativo per compatibilit� con versioni precedenti
						lex.Parsed(Token.ASSIGN);

						Expression readOnlyExpr = new Expression(Session, report.SymTable.Fields);
						readOnlyExpr.StopTokens = new StopTokens(new Token[] { Token.HOTLINK, Token.WHEN, Token.DYNAMIC });
						if (readOnlyExpr.Compile(lex, CheckResultType.Match, "Boolean"))
							askEntry.ReadOnlyExpr = readOnlyExpr;
					}

					// se c'� il token DYNAMIC allora deve esserci il token WHEN
					// DYNAMIC serve solo al woorm C++ per gestire la scomparsa dinamica dei controls
					// in questo la scomparsa � sempre dinamica
					bool requireWhen = lex.Parsed(Token.DYNAMIC);
					if (lex.Parsed(Token.WHEN))
					{
						Expression WhenExpr = new Expression(Session, report.SymTable.Fields);
						WhenExpr.StopTokens = new StopTokens(new Token[] { Token.HOTLINK });
						if (WhenExpr.Compile(lex, CheckResultType.Match, "Boolean"))
							askEntry.WhenExpr = WhenExpr;
					}
					else if (requireWhen) return false;

					if (!ParseHotlink(lex, askEntry))
						return false;
				}

				if (!lex.ParseSep())
					return false;
			}
			while (!lex.Parsed(Token.END) && !lex.Error);

			return !lex.Error;
		}

		//---------------------------------------------------------------------------
		public bool ParseGroups(Parser lex)
		{
			do
			{
				AddGroup();

				if (!lex.ParseBegin()) return false;
		        
				string groupTitle = "";
				if (lex.LookAhead() == Token.TEXTSTRING && !lex.ParseString(out groupTitle))
					return false;
			
				bool groupVisible = !lex.Parsed(Token.HIDDEN);
				if (lex.Error)
					return false;
		       
				// se c'� il token DYNAMIC allora deve esserci il token WHEN
				// DYNAMIC serve solo al woorm C++ per gestire la scomparsa dinamica dei controls
				// in questo la scomparsa � sempre dinamica
				bool requireWhen = lex.Parsed(Token.DYNAMIC);
				if (lex.Parsed(Token.WHEN))
				{
					Expression WhenExpr = new Expression(Session, report.SymTable.Fields);
					WhenExpr.StopTokens = new StopTokens(new Token[] {Token.SEP});
					if (WhenExpr.Compile(lex, CheckResultType.Match, "Boolean"))
						askGroup.WhenExpr = WhenExpr;
					
					if (!lex.ParseSep())
						return false;
				}
				else if (requireWhen) return false;
				
				captionPos = Token.DEFAULT;
				switch (lex.LookAhead())
				{
					case Token.LEFT	:
					case Token.RIGHT:
					case Token.TOP	:
						captionPos = lex.LookAhead();
						lex.SkipToken();
						if (!lex.ParseTag(Token.PROMPT)) return false;
						break;
				}
				SetCaptionPos(captionPos);

				SetGroupTitle(groupTitle);
				SetGroupVisibility(groupVisible);

				ParseEntries(lex);
			}
			while (lex.LookAhead(Token.BEGIN) && !lex.Error);

			return !lex.Error;
		}

		//---------------------------------------------------------------------------
		public bool ParseControls(Parser lex)
		{
			// corpo della dialog
			bool ok =
				lex.ParseTag(Token.CONTROLS) &&
				lex.ParseBegin() &&
				ParseGroups(lex) &&
				lex.ParseEnd();

			return !lex.Error;
		}

		//---------------------------------------------------------------------------
		public bool ParseHeader(Parser lex)
		{
			// determina se la dialog � chiamata su esecuzione di codice oppure nella
			// sequenza iniziale di chiamate delle dialogs allo start-up del report
			if (lex.Parsed(Token.ON))
			{
				if (!lex.ParseTag(Token.ASK))
					return false;

				onAsk = true;
			}

			// nome
			if (!lex.ParseID(out FormName))
				return false;

			// titolo
			if (lex.LookAhead() == Token.TEXTSTRING && !lex.ParseString(out FormTitle))
				return false;

			// eventuale help
			if (lex.Parsed(Token.HELP))
			{
				if (!lex.ParseTag(Token.ASSIGN))
					return false;
				
				long helpData;
				lex.ParseLong(out helpData);

				SetHelpData(report.HelpFileName, helpData);
			}

			// default position
			Token captionPos = Token.DEFAULT;
			switch (lex.LookAhead())
			{
				case Token.LEFT	:
				case Token.RIGHT:
				case Token.TOP	:
					captionPos = lex.LookAhead();
					lex.SkipToken();
					if (!lex.ParseTag(Token.PROMPT)) return false;
					break;
			}

			SetCaptionPos(captionPos);
			return !lex.Error;
		}

		//---------------------------------------------------------------------------
		public bool ParseBefore(Parser lex)
		{
			if (lex.Parsed(Token.BEFORE))
			{
                BeforeActions = new Block(Report.Engine, null, null, true);
				BeforeActions.Parse(lex);
			}
			return !lex.Error;
		}

		//---------------------------------------------------------------------------
		public bool ParseAfter(Parser lex)
		{
			if (lex.Parsed(Token.AFTER))
			{
                AfterActions = new Block(Report.Engine, null, null, true);
				AfterActions.Parse(lex);
			}
			return !lex.Error;
		}

		//---------------------------------------------------------------------------
		public bool ParseOn(Parser lex)
		{
			if (lex.Parsed(Token.ON))
			{
				OnExpr = new Expression(Session, report.SymTable.Fields);
				OnExpr.StopTokens = new StopTokens(new Token[] {Token.ABORT});

				if (!OnExpr.Compile(lex, CheckResultType.Match, "Boolean"))
					return false;

				if (!lex.ParseTag(Token.ABORT))
					return false;

				// anche il messaggio pu� essere una string	
				OnMessage = new Expression(Session, report.SymTable.Fields);
				OnMessage.StopTokens = new StopTokens(new Token[] {Token.END});

				if (!OnMessage.Compile(lex, CheckResultType.Match, "String"))
					return false;
			}
			return !lex.Error;
		}

		//---------------------------------------------------------------------------
		public bool ParseWhen(Parser lex)
		{
			// eventuale espressione di WHEN che abilita il run della dialog
			if (lex.Parsed(Token.WHEN))
			{
				WhenExpr = new Expression(Session, report.SymTable.Fields);
				WhenExpr.StopTokens = new StopTokens((new Token[] {Token.CONTROLS}));

				if (!WhenExpr.Compile(lex, CheckResultType.Match, "Boolean"))
					return false;
			}
			return !lex.Error;
		}

		//---------------------------------------------------------------------------
		public bool Parse(Parser lex)
		{
			bool ok =
					ParseHeader(lex)		&&
					lex.ParseBegin()		&&
					ParseBefore		(lex)	&&
					ParseWhen		(lex)	&&
					ParseControls	(lex)	&&
					ParseAfter		(lex)	&&
					ParseOn			(lex)	&&
					lex.ParseEnd();
			
			return ok || !lex.Error;
		}

		//---------------------------------------------------------------------------
		public bool Unparse(Unparser unparser)
		{
			if (IsEmpty)
				return true;

			unparser.IncTab();
			unparser.WriteTag(Token.DIALOG, false);

			if (OnAsk)
			{
				unparser.WriteTag(Token.ON, false);
				unparser.WriteTag(Token.ASK, false);
			}

			unparser.WriteID(FormName, false);

			string strDefaultTitle = "Dialog Without Title";

			unparser.WriteBlank();

			if (!FormTitle.IsNullOrEmpty() && FormTitle.IndexOf(strDefaultTitle) == -1)
				unparser.WriteString(
					unparser.IsLocalizableTextInCurrentLanguage()
					? unparser.LoadReportString(FormTitle)
					: FormTitle, false);

			if (captionPos != Token.DEFAULT)
			{
				unparser.WriteTag(captionPos, false);
				unparser.WriteTag(Token.PROMPT, false);
			}
			unparser.WriteLine();

			unparser.IncTab();
			unparser.WriteBegin();

			if (BeforeActions != null)
			{
				unparser.WriteTag(Token.BEFORE, false);
				BeforeActions.Unparse(unparser);
			}

			// clausola When di abilitazione del run della dialog.
			if (WhenExpr != null)
			{
				unparser.WriteTag(Token.WHEN, false);
				unparser.WriteExpr(WhenExpr.ToString(), true);
			}

			unparser.IncTab();
			unparser.WriteTag(Token.CONTROLS);

			unparser.IncTab();
			unparser.WriteBegin();

			foreach (AskGroup group in groups)
				group.Unparse(unparser);

			unparser.WriteEnd();
			unparser.DecTab();

			if (AfterActions != null)
			{
				unparser.WriteTag(Token.AFTER, false);
				AfterActions.Unparse(unparser);
			}

			// clausola Check di abilitazione del run della dialog.
			if (OnExpr != null)
			{
				unparser.WriteTag(Token.ON, false);
				unparser.WriteExpr(OnExpr.ToString(), false);
				unparser.WriteTag(Token.ABORT, false);
				unparser.WriteExpr(OnMessage.ToString(), true);
			}
			unparser.DecTab();
			unparser.WriteEnd();

			unparser.DecTab();
			unparser.DecTab();
			return true;
		}

		//----------------------------------------------------------------------------
		public bool IsEmpty
		{
			get
			{
				foreach (AskGroup item in groups)
					if (!item.IsEmpty)
						return false;

				return true;
			}
		}
	}
}

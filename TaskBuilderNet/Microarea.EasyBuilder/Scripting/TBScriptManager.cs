using System;
using System.Text;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.Library.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.Lexan;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Woorm.ExpressionManager;

namespace Microarea.EasyBuilder.Scripting
{
    //================================================================================
    /// <remarks/>
    public class TBScriptManager
	{
		SymbolTable scriptingSymbolTable = null;
		private MDocument document;
		private EasyBuilderSession session;

		//--------------------------------------------------------------------------------
		/// <remarks/>
        public TBScriptManager(MDocument document)
		{
            this.document = document;
		}
		/// <summary>
		/// aggiunge tutti gli oggetti di contesto alla symboltable
		/// </summary>
		//--------------------------------------------------------------------------------
		public SymbolTable ScriptingSymbolTable
		{
			get
			{
				if (scriptingSymbolTable == null)
				{
					scriptingSymbolTable = new SymbolTable();
                    scriptingSymbolTable.Add(new Variable("Document", document));
				
				}
				return scriptingSymbolTable;
			}
		}

 
		//--------------------------------------------------------------------------------
		private EasyBuilderSession Session
		{
			get
			{
				if (session == null)
				{
					session = new EasyBuilderSession(new UserInfo());
					session.LoadSessionInfo();
					//session.Functions.LoadPrototypes();
				}
				return session;
			}
		}
		
		//-----------------------------------------------------------------------------
        /// <summary>
        /// esegue uno script
        /// </summary>
        public object ExecuteScript(string script, string returnType, params ScriptParameter[] args)
		{
			TbScript tbScript = CreateScriptingContext(returnType, args);

			using (Parser parser = new Parser(Parser.SourceType.FromString))
			{
				parser.Open(script);
				if (!tbScript.Parse(parser))
				{
					throw new ApplicationException(GetErrorMessage(parser));
				}
				if (!tbScript.Exec())
				{
					throw new ApplicationException(tbScript.ErrorText);
				}
			}

			return tbScript.ReturnValue;
			
		}

		//-----------------------------------------------------------------------------
		private TbScript CreateScriptingContext(string returnType, ScriptParameter[] args)
		{
			//creo il gestore dello script
			TbScript tbScript = new TbScript(Session, ScriptingSymbolTable, document.GetMessageProvider());
			//imposto il tipo di ritorno (può anche essere System.Void)
			tbScript.ReturnType = returnType;
			//prevalorizzo la symbol table dell'oggetto (che NON è quella globale, ma una sual figlia)
			//con i parametri della funzione
			if (args != null)
			{
				foreach (ScriptParameter par in args)
					tbScript.SymbolTable.Add(new Variable(par.Name, par.Value));
			}
			return tbScript;
		}

		

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public static string GetErrorMessage(Parser parser)
		{
			IDiagnosticItems items = parser.Diagnostic.AllMessages();
			StringBuilder errorMessage = new StringBuilder();
			if (items != null)
				foreach (IDiagnosticItem item in items)
				{
					if (!string.IsNullOrEmpty(item.FullExplain))
					{
						errorMessage.AppendLine(item.FullExplain);
						foreach (IExtendedInfoItem info in item.ExtendedInfo)
						{
							if (info.Info == null)
								continue;
							errorMessage.AppendLine(string.Format("{0}: {1}", info.Name, info.Info));
						}
					}
				}
			return errorMessage.ToString();
		}
	}

	//================================================================================
    /// <summary>
    /// Manages a session 
    /// </summary>
	public class EasyBuilderSession : TbReportSession
	{
        
		//--------------------------------------------------------------------------------
        /// <summary>
        /// Constructs an EasyBuilderSession
        /// </summary>
        /// <param name="userInfo"></param>
		public EasyBuilderSession(UserInfo userInfo)
			: base(userInfo)
		{
		}

		//--------------------------------------------------------------------------------
        /// <summary>
        /// returns if type checking has to be skipped
        /// </summary>
		public override bool SkipTypeChecking { get { return false; } }

		//--------------------------------------------------------------------------------
        /// <summary>
        /// gets tb client interface
        /// </summary>
		public override ITbLoaderClient GetTBClientInterface()
		{
			return new TbApplicationClientInterface(
					"",
					BasePathFinder.BasePathFinderInstance,
					0,
					CUtility.GetAuthenticationToken(),
					WCFBinding.None
				);

		}
	}

	
	//================================================================================
	/// <summary>
	/// Encapsluates a parameter to be used in a script.
	/// </summary>
	public class ScriptParameter
	{

		/// <summary>
		/// Initializes a new instance of ScriptParameter.
		/// </summary>
		/// <param name="name">The name of the parameter.</param>
		/// <param name="value">The value of the parameter.</param>
		public ScriptParameter(string name, object value)
		{
			this.Name = name;
			this.Value = value;
		}
		/// <summary>
		/// Gets or sets the name of the parameter.
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Gets or stest the value of the parameter.
		/// </summary>
		public object Value { get; set; }
	}
}

using Microarea.Common.NameSolver;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.Common.MenuLoader
{
	/// <summary>
	/// Summary description for MenuSearchEngine.
	/// </summary>
	public class MenuSearchEngine : IDisposable
	{
		#region Enums
		[Flags]
			public enum Criteria : ushort
		{
			Undefined				= 0x0000,
			CaseSensitive			= 0x0001,
			SearchDescriptions		= 0x0002,
			ExactWord				= 0x0004,
			SearchInPreviousResult	= 0x0008,
            SearchItemObjects       = 0x0010,
			All						= 0x001F
		}

		public enum ObjectTypes: ushort
		{
            Undefined		= 0x0000,		
            Report			= 0x0001,				
            Document		= 0x0002,
            Batch			= 0x0004,
            Function		= 0x0008,		
            Text			= 0x0010,
            Exe				= 0x0020,
            OfficeItem		= 0x0040,
            ExternalItem    = 0x0080,
            All				= 0X00FF

		}

		#endregion

		#region Private Data Member

		private PathFinder				pathFinder			= null;
		private MenuXmlParser			parser				= null;
		private MenuXmlNode				startSearchNode		= null;
		private Criteria				criteria			= Criteria.Undefined;
		private MenuXmlNodeCollection	lastResults			= null;
		private ObjectTypes				objectTypesToSearch	= ObjectTypes.Undefined;		

		private const char AndChar	= '&';
		private const char OrChar	= '|';
		private const char NotChar	= '!';

		private static char[] patternExpressionOperators	= new char[2]{AndChar, OrChar};
		private static char[] patternExpressionKeywords		= new char[5]{NotChar, AndChar, OrChar, '(', ')'};

		private const string AndKeyWord = "AND";
		private const string OrKeyWord	= "OR";
		private const string NotKeyWord = "NOT";
		private const string BlankSubstitutionToken = "[#Blank#]";
		private const string OpeningParenthesisSubstitutionToken = "[#OpeningParenthesis#]";
		private const string ClosingParenthesisSubstitutionToken = "[#ClosingParenthesis#]";


		private bool disposed = false;
		
		#endregion

		#region Properties
		//---------------------------------------------------------------------
		public PathFinder				PathFinder	{ get { return pathFinder; }}
		public MenuXmlParser			Parser		{ get { return parser; }}
		public MenuXmlNodeCollection	LastResults	{ get { return lastResults; } set { lastResults = value; }}
		
		//---------------------------------------------------------------------
		public MenuXmlNode StartSearchNode	
		{
			get { return startSearchNode;} 
			set 
			{
				if 
					(
					value != null && 
					value.Node != null &&
					value.OwnerDocument == Parser.MenuXmlDoc &&
					(value.IsRoot || value.IsApplication || value.IsGroup)
					)
					startSearchNode = value; 
			}
		}
			
		//---------------------------------------------------------------------
		public bool CaseSensitive
		{
			get { return (criteria & Criteria.CaseSensitive) == Criteria.CaseSensitive; }

			set
			{
				if (value)
					criteria |= Criteria.CaseSensitive;
				else
					criteria &= ~Criteria.CaseSensitive;
			}
		}
		
		//---------------------------------------------------------------------
		public bool ExactWord
		{
			get { return (criteria & Criteria.ExactWord) == Criteria.ExactWord; }

			set
			{
				if (value)
					criteria |= Criteria.ExactWord;
				else
					criteria &= ~Criteria.ExactWord;
			}
		}

		//---------------------------------------------------------------------
		public bool SearchDescriptions
		{
			get { return (criteria & Criteria.SearchDescriptions) == Criteria.SearchDescriptions; }

			set
			{
				if (value)
					criteria |= Criteria.SearchDescriptions;
				else
					criteria &= ~Criteria.SearchDescriptions;
			}
		}

        //---------------------------------------------------------------------
		public bool SearchItemObjects
        {
            get { return (criteria & Criteria.SearchItemObjects) == Criteria.SearchItemObjects; }

            set
            {
                if (value)
                    criteria |= Criteria.SearchItemObjects;
                else
                    criteria &= ~Criteria.SearchItemObjects;
            }
        }

		//---------------------------------------------------------------------
		public bool SearchInPreviousResult
		{
			get { return (criteria & Criteria.SearchInPreviousResult) == Criteria.SearchInPreviousResult; }

			set
			{
				if (value)
					criteria |= Criteria.SearchInPreviousResult;
				else
					criteria &= ~Criteria.SearchInPreviousResult;
			}
		}

		//---------------------------------------------------------------------
		public bool ExtractReports
		{
			get { return (objectTypesToSearch & ObjectTypes.Report) == ObjectTypes.Report; }

			set
			{
				if (value)
					objectTypesToSearch |= ObjectTypes.Report;
				else
					objectTypesToSearch &= ~ObjectTypes.Report;
			}
		}
		
		//---------------------------------------------------------------------
		public bool ExtractDocuments
		{
			get { return (objectTypesToSearch & ObjectTypes.Document) == ObjectTypes.Document; }

			set
			{
				if (value)
					objectTypesToSearch |= ObjectTypes.Document;
				else
					objectTypesToSearch &= ~ObjectTypes.Document;
			}
		}


		//---------------------------------------------------------------------
		public bool ExtractBatches
		{
			get { return (objectTypesToSearch & ObjectTypes.Batch) == ObjectTypes.Batch; }

			set
			{
				if (value)
					objectTypesToSearch |= ObjectTypes.Batch;
				else
					objectTypesToSearch &= ~ObjectTypes.Batch;
			}
		}

		//---------------------------------------------------------------------
		public bool ExtractFunctions
		{
			get { return (objectTypesToSearch & ObjectTypes.Function) == ObjectTypes.Function; }

			set
			{
				if (value)
					objectTypesToSearch |= ObjectTypes.Function;
				else
					objectTypesToSearch &= ~ObjectTypes.Function;
			}
		}

		//---------------------------------------------------------------------
		public bool ExtractTexts
		{
			get { return (objectTypesToSearch & ObjectTypes.Text) == ObjectTypes.Text; }

			set
			{
				if (value)
					objectTypesToSearch |= ObjectTypes.Text;
				else
					objectTypesToSearch &= ~ObjectTypes.Text;
			}
		}

		//---------------------------------------------------------------------
		public bool ExtractExecutables
		{
			get { return (objectTypesToSearch & ObjectTypes.Exe) == ObjectTypes.Exe; }

			set
			{
				if (value)
					objectTypesToSearch |= ObjectTypes.Exe;
				else
					objectTypesToSearch &= ~ObjectTypes.Exe;
			}
		}

		//---------------------------------------------------------------------
		public bool ExtractOfficeItems
		{
			get { return (objectTypesToSearch & ObjectTypes.OfficeItem) == ObjectTypes.OfficeItem; }

			set
			{
				if (value)
					objectTypesToSearch |= ObjectTypes.OfficeItem;
				else
					objectTypesToSearch &= ~ObjectTypes.OfficeItem;
			}
		}

		//---------------------------------------------------------------------
		public bool ExtractExternalItems
		{
            get { return (objectTypesToSearch & ObjectTypes.ExternalItem) == ObjectTypes.ExternalItem; }

			set
			{
                if (value)
                    objectTypesToSearch |= ObjectTypes.ExternalItem;
                else
                    objectTypesToSearch &= ~ObjectTypes.ExternalItem;
            }
		}

        //---------------------------------------------------------------------
        public bool ExtractAll
        {
            get { return (objectTypesToSearch & ObjectTypes.All) == ObjectTypes.All; }

            set
            {
                if (value)
                    objectTypesToSearch = ObjectTypes.All;
            }
        }
        
        #endregion

		#region Costructor

		//---------------------------------------------------------------------
		public MenuSearchEngine(PathFinder aPathFinder, MenuXmlParser aMenuXmlParser)
		{
			pathFinder			= aPathFinder;
			parser				= aMenuXmlParser;

			if (parser != null)
				startSearchNode = (MenuXmlNode) parser.Root;
		}

		#endregion

		#region Dispose

		//---------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
		}
		
		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the 
		// runtime from inside the finalizer and you should not reference 
		// other objects. Only unmanaged resources can be disposed.
		private void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if (!this.disposed)
			{
				if (lastResults != null)
					lastResults.Clear();

				if (disposing)
					GC.SuppressFinalize(this);
			}
			disposed = true;         
		}


		#endregion

		#region private methods
		//---------------------------------------------------------------------
		// La stringa per i ricercare i comandi all'interno del menù può essere
		// specifica oppure di un'espressione di tipo logico nella
		// quale si possono “combinare” più parole o stringhe in modo da effet-
		// tuare una ricerca più complessa.
		// La sintassi da adottare per la formulazione corretta di questa espressione prevede 
		// di usare:
		//	- Il carattere ‘&’ o stringa "AND  per concatenare due elementi "and". 
		//	- Il carattere ‘|’ o stringa "OR"  per concatenare due elementi in "or".
		//	- Il carattere ‘!’ o stringa "NOT" per negare un elemento.
		//	- Le parentesi tonde per raggruppare sotto-espressioni 
		//  - '' e "" per ricercare un intera frase
		// Esempi:
		//		sca
		//		!sca
		//		sca & sche
		//		sca & !sche
		//		(sca & !sche) | (sca & !bil)
		//		!(sca | & sche)
		//		"Schede"
		//		'Schede'
		//---------------------------------------------------------------------
		private MenuXmlNodeCollection SearchExpression(MenuXmlNodeCollection setTosearch, string searchExpression)
		{
			if (setTosearch == null || searchExpression == null || searchExpression.Trim().Length == 0)
				return null;

			char currentOperator = '\0';
			
			string expression = searchExpression.Trim();
			
			MenuXmlNodeCollection results = new MenuXmlNodeCollection();

			while (expression != null && expression.Length > 0)
			{		
				MenuXmlNodeCollection tokenResultSet = null;

				bool negateToken = (expression[0] == '!');
				if (negateToken)
					expression = expression.Substring(1).TrimStart();

				int firstKeyIndex = -1;
				// Se l'espressione comincia con i doppi apici allora devo prendere tutto 
				// ciò che sta fra apici senza cercare di interpretarne logicamente il nesso...
				// Lo stesso vale se prima degli apici c'è il simbolo di negazione.
				if (expression[0] == '"')
				{
					int closingDoubleQuoteIndex = (expression.Length > 1) ? expression.IndexOf('"', 1): -1;
					if (closingDoubleQuoteIndex == -1)
					{
						// errore di sintassi: l'espressione non può terminare con un'operatore di tipo '&', '|', ')'
						Debug.WriteLine("Search expression evaluation error encountered in MenuSearchEngine.SearchExpression.");
						// non potendo valutare correttamente l'espressione, sollevo un'eccezione
						throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.SearchExprDoubleQuoteMatchError, expression));
					}
					firstKeyIndex = expression.IndexOfAny(patternExpressionKeywords, closingDoubleQuoteIndex);
				}
				else
					firstKeyIndex = expression.IndexOfAny(patternExpressionKeywords);

				if (firstKeyIndex >= 0 )
				{
					if (firstKeyIndex == 0 && expression[0] != '(')
					{
						// errore di sintassi: l'espressione non può cominciare con un'operatore di tipo '&', '|', ')'
						Debug.WriteLine("Search expression evaluation error encountered in MenuSearchEngine.SearchExpression.");
						// non potendo valutare correttamente l'espressione, sollevo un'eccezione
						throw new MenuXmlParserException(MenuManagerLoaderStrings.SearchExprStartingCharError);
					}

					int doubleQuoteCount = 0;
					int doubleQuoteCharIndex = 0;

					while (doubleQuoteCharIndex < firstKeyIndex)
					{
						if	(expression[doubleQuoteCharIndex] == '\"')		
							doubleQuoteCount++;

						if	(doubleQuoteCount == 2)
							break;

						doubleQuoteCharIndex++;
					} 

					int openingParenthesisCount = 0;
					int charIndex = 0;

					do
					{
						if	(expression[charIndex] == '(')		
							openingParenthesisCount++;
						else if	(expression[charIndex] == ')')	
							openingParenthesisCount--;

						if	(openingParenthesisCount == 0)
							break;

						charIndex++;

					} while (charIndex < expression.Length);
					// esco dal while solo se l'espressione è terminata
					// o se ho chiuso tutte eventuali le parentesi tonde
					if (openingParenthesisCount != 0)
					{
						// errore di sintassi: non c'è un matching corretto di parentesi
						Debug.WriteLine("Search expression evaluation error encountered in MenuSearchEngine.SearchExpression.");
						// non potendo valutare correttamente l'espressione, sollevo un'eccezione
						throw new MenuXmlParserException(MenuManagerLoaderStrings.SearchExprParenthesisMatchError);
					}

					string token = String.Empty;

					if (charIndex > 0)// il token comincia con una parentesi e charIndex punta all'ultima parentesi chiusa
					{
						token		= expression.Substring(1, charIndex - 1);
						expression	= expression.Substring(charIndex + 1).Trim();
					}
					else if (doubleQuoteCount > 0)
					{
						token		= expression.Substring(1, doubleQuoteCharIndex - 1);
						expression	= expression.Substring(doubleQuoteCharIndex + 1).Trim();
					}
					else
					{
						if (negateToken)
						{
							int tokenLength = expression.IndexOfAny(patternExpressionOperators);
							if (tokenLength == -1)
							{
								token = expression;
								expression = String.Empty;
							}
							else
							{
								token = expression.Substring(0, tokenLength);
								expression = expression.Substring(tokenLength).Trim();
							}
						}
						else
						{
							token = expression.Substring(0, firstKeyIndex);
							expression = expression.Substring(firstKeyIndex).Trim();
						}
					}
					if (currentOperator != '\0' && (token == null || token.Length == 0)) // non è il primo operando !
					{
						// errore di sintassi: l'espressione non può terminare con un'operatore di tipo '&', '|', ')'
						Debug.WriteLine("Search expression evaluation error encountered in MenuSearchEngine.SearchExpression.");
						// non potendo valutare correttamente l'espressione, sollevo un'eccezione
						throw new MenuXmlParserException(MenuManagerLoaderStrings.SearchExprEndingCharError);
					}
					
					tokenResultSet = SearchExpression(setTosearch, token);
				}
				else
				{
					if (expression[0] == '\"' && expression.Length > 1)
					{
						int closingDoubleQuoteIndex = expression.IndexOf('\"', 1);
						if (closingDoubleQuoteIndex == -1)
						{
							// errore di sintassi: l'espressione non può terminare con un'operatore di tipo '&', '|', ')'
							Debug.WriteLine("Search expression evaluation error encountered in MenuSearchEngine.SearchExpression.");
							// non potendo valutare correttamente l'espressione, sollevo un'eccezione
							throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.SearchExprDoubleQuoteMatchError, expression));
						}
						
						expression	= expression.Substring(1, closingDoubleQuoteIndex - 1);
					}

					tokenResultSet = SearchSingleToken(setTosearch, expression);

					expression = String.Empty;
				}
				if (negateToken)
					tokenResultSet = setTosearch.Subtract(tokenResultSet);

				if (currentOperator == '&')
					results = results.Intersect(tokenResultSet);
				else if (currentOperator == '|')
					results = results.Union(tokenResultSet);
				else if (currentOperator == '\0' && tokenResultSet != null && tokenResultSet.Count > 0)
					results.AddRange(tokenResultSet); 
				
				if (expression == null || expression.Length == 0)
					break;

				currentOperator = expression[0];
				
				if (results.Count == 0 && currentOperator == '&')
					return results; // torno l'insieme vuoto
				
				expression = expression.Substring(1).TrimStart();
		
				if (currentOperator != '\0' && (expression == null || expression.Length == 0))
				{
					// errore di sintassi: l'espressione non può terminare con un'operatore di tipo '&', '|', ')'
					Debug.WriteLine("Search expression evaluation error encountered in MenuSearchEngine.SearchExpression.");
					// non potendo valutare correttamente l'espressione, sollevo un'eccezione
					throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.SearchExprEndingCharError, expression));
				}
			}
			
			return results;
		}

		//---------------------------------------------------------------------
		private  MenuXmlNodeCollection GetMenuNodesToSearch()
		{
			if (SearchInPreviousResult)
			{
				if (lastResults == null)
					return null;

				if (ExtractAll || objectTypesToSearch == ObjectTypes.Undefined)
					return lastResults;

				MenuXmlNodeCollection resultSubset = new MenuXmlNodeCollection();
				
				if (ExtractDocuments)
					resultSubset.AddRange(lastResults.GetDocumentNodes());

				if (ExtractReports)
					resultSubset.AddRange(lastResults.GetReportNodes());

				if (ExtractBatches)
					resultSubset.AddRange(lastResults.GetBatchNodes());

				if (ExtractFunctions)
					resultSubset.AddRange(lastResults.GetFunctionNodes());
			
				if (ExtractTexts)
					resultSubset.AddRange(lastResults.GetTextNodes());

				if (ExtractExecutables)
					resultSubset.AddRange(lastResults.GetExeNodes());
				
				if (ExtractOfficeItems)
					resultSubset.AddRange(lastResults.GetOfficeItemNodes());

                if (ExtractExternalItems)
                    resultSubset.AddRange(lastResults.GetExternalItemNodes());
                
                return resultSubset;
			}

			if (startSearchNode == null)
				return null;

			if (ExtractAll || objectTypesToSearch == ObjectTypes.Undefined)
				return startSearchNode.GetAllCommandDescendants();
	
			MenuXmlNodeCollection nodeCollection = new MenuXmlNodeCollection();

			if (ExtractDocuments)
				nodeCollection.AddRange(startSearchNode.GetDocumentDescendantNodes());

			if (ExtractReports)
				nodeCollection.AddRange(startSearchNode.GetReportDescendantNodes());

			if (ExtractBatches)
				nodeCollection.AddRange(startSearchNode.GetBatchDescendantNodes());

			if (ExtractFunctions)
				nodeCollection.AddRange(startSearchNode.GetFunctionDescendantNodes());
			
			if (ExtractTexts)
				nodeCollection.AddRange(startSearchNode.GetTextDescendantNodes());

			if (ExtractExecutables)
				nodeCollection.AddRange(startSearchNode.GetExeDescendantNodes());
			
			if (ExtractOfficeItems)
				nodeCollection.AddRange(startSearchNode.GetOfficeItemDescendantNodes());

            if (ExtractExternalItems)
                nodeCollection.AddRange(startSearchNode.GetExternalItemDescendantNodes());

			return nodeCollection;
		}
		
		//---------------------------------------------------------------------
		private bool CheckDoubleQuote(string searchExpression)
		{
			if (searchExpression == null || searchExpression.Length == 0)
				return true;

			int doubleQuoteCount	= 0;
			int charIndex			= 0;

			//Non ci sono doppi apici e quindi esco
			if (searchExpression.IndexOf('\"') == -1)
				return true;
			
			do
			{
				if	(searchExpression[charIndex] == '\"')		
					doubleQuoteCount++;
				
				charIndex++;

			} while (charIndex < searchExpression.Length);
			
			// I doppi apici devono necessariamente essere in numero pari
			return (doubleQuoteCount % 2 == 0);			
		}

		//---------------------------------------------------------------------
		private string ValidateExpression(string searchExpression)
		{
			if (searchExpression == null || searchExpression.Trim().Length == 0)
				return String.Empty;

			string tmpExpression = searchExpression.Trim();
			if (!CheckDoubleQuote(tmpExpression))
			{
				Debug.WriteLine("Search expression error in MenuSearchEngine.ValidateExpression: the double-quote character that terminates the string is not included.");
				// non potendo valutare correttamente l'espressione, sollevo un'eccezione
				throw new MenuXmlParserException(MenuManagerLoaderStrings.SearchExprDoubleQuoteMatchError);
			}

			// Elimino eventuali caratteri di separazione (blank e parentesi) nelle sottostringhe
			// quotate (li sostituisco con un corrispondente marker predefinito). In tal modo, 
			// quando poi splitto l'espressione, usando come separatori proprio quei caratteri,
			// non splitto gli elementi quotati.
			// All'atto di ricomporre, infine, l'intera espressione li risostituirò nuovamente con 
			// i caratteri originali.
			int firstDoubleQuoteIdx = tmpExpression.IndexOf('\"');
			while (firstDoubleQuoteIdx >= 0 && firstDoubleQuoteIdx < (tmpExpression.Length - 1))
			{
				int closingDoubleQuoteIdx = tmpExpression.IndexOf('\"', firstDoubleQuoteIdx + 1);
				if (closingDoubleQuoteIdx == - 1)
					break; 

				if (closingDoubleQuoteIdx > firstDoubleQuoteIdx + 1)
				{
					string quotedSubExpression = tmpExpression.Substring(firstDoubleQuoteIdx + 1, closingDoubleQuoteIdx - firstDoubleQuoteIdx - 1);
					
					quotedSubExpression = quotedSubExpression.Replace(" ", BlankSubstitutionToken);
					quotedSubExpression = quotedSubExpression.Replace("(", OpeningParenthesisSubstitutionToken);
					quotedSubExpression = quotedSubExpression.Replace(")", ClosingParenthesisSubstitutionToken);

					int addedCharsNum = quotedSubExpression.Length - (closingDoubleQuoteIdx - firstDoubleQuoteIdx - 1); 

					tmpExpression = tmpExpression.Substring(0, firstDoubleQuoteIdx + 1) + quotedSubExpression + tmpExpression.Substring(closingDoubleQuoteIdx, tmpExpression.Length - closingDoubleQuoteIdx);

					closingDoubleQuoteIdx += addedCharsNum;
				}

				if (firstDoubleQuoteIdx > 0 && tmpExpression[firstDoubleQuoteIdx - 1] != ' ')
				{
					tmpExpression = tmpExpression.Substring(0, firstDoubleQuoteIdx) + " " + tmpExpression.Substring(firstDoubleQuoteIdx, tmpExpression.Length - firstDoubleQuoteIdx);
					closingDoubleQuoteIdx++;
				}

				if (closingDoubleQuoteIdx == tmpExpression.Length - 1)
					break; // sono in fondo alla stringa
				
				if (tmpExpression[closingDoubleQuoteIdx + 1] != ' ')
					tmpExpression = tmpExpression.Substring(0, closingDoubleQuoteIdx + 1) + " " + tmpExpression.Substring(closingDoubleQuoteIdx + 1, tmpExpression.Length - closingDoubleQuoteIdx - 1);

				firstDoubleQuoteIdx = tmpExpression.IndexOf('\"', closingDoubleQuoteIdx + 1);
			}

            // Adesso occorre controllare se sono state usate parole chiave del tipo "AND", 
			// "OR" o "NOT" ed in tal caso sostituirle con i corrispondenti simboli '&', '|' 
			// e '!' che vengono poi interpretati successivamente dal parser 
			string correctedExpression = String.Empty;
			string[] expressionTokens = tmpExpression.Split(new char[] {' ', '(', ')'});

			for(int i=0; i < expressionTokens.Length; i++)
			{
				char previousChar = ' ';
				int tokenIndex = tmpExpression.IndexOf(expressionTokens[i]);
				if (tokenIndex > 0)
					previousChar = tmpExpression[tmpExpression.IndexOf(expressionTokens[i]) -1];
				
				tmpExpression = tmpExpression.Substring(tmpExpression.IndexOf(expressionTokens[i]) + expressionTokens[i].Length, tmpExpression.Length - tmpExpression.IndexOf(expressionTokens[i]) - expressionTokens[i].Length);

				string token = expressionTokens[i].ToUpper(CultureInfo.CurrentUICulture);

				if (token.Length == 0)
					continue;

				if (correctedExpression.Length > 0)
					correctedExpression += ' ';

				if (previousChar == '(')
					correctedExpression += '(';

				// Ripristino nelle sottostringhe quotate eventuali caratteri usati come separatori nello split 
				if (token[0] == '\"')
				{
					string validToken = expressionTokens[i].Replace(BlankSubstitutionToken, " ");
					validToken = validToken.Replace(OpeningParenthesisSubstitutionToken, "(");
					validToken = validToken.Replace(ClosingParenthesisSubstitutionToken, ")");
					correctedExpression += validToken;
					continue;
				}

				if (String.Compare(token, OrKeyWord) == 0)
					correctedExpression += OrChar;
				else if (String.Compare(token, AndKeyWord) == 0)
					correctedExpression += AndChar;			
				else if (String.Compare(token, NotKeyWord) == 0)
					correctedExpression += NotChar;
				else
					correctedExpression += expressionTokens[i];
		
				if (tmpExpression.Length > 0 && tmpExpression[0] == ')')
					correctedExpression += ')';
			}
			
			return correctedExpression;
		}

		//---------------------------------------------------------------------
		private MenuXmlNodeCollection SearchSingleToken(MenuXmlNodeCollection setTosearch, string aTokenToSearch)
		{
			if 
				(
				setTosearch == null || 
				setTosearch.Count == 0 || 
				aTokenToSearch == null || 
				aTokenToSearch.Length == 0
				)
				return null;

			if (!CaseSensitive)
				aTokenToSearch = aTokenToSearch.ToUpper(CultureInfo.CurrentUICulture);

			if (!ExactWord) 
				aTokenToSearch = aTokenToSearch.Trim();


			MenuXmlNodeCollection results = new MenuXmlNodeCollection();
			
			foreach(MenuXmlNode node in setTosearch)
			{
				if (node.Title != null && node.Title.Length > 0)
				{
					string aStringToSearch = CaseSensitive ? node.Title : node.Title.ToUpper(CultureInfo.CurrentUICulture);

                    int foundIdx = aStringToSearch.IndexOf(aTokenToSearch);
                    if (foundIdx != -1)
                    {
                        if
                            (
                            !ExactWord ||
                            (
                            (
                            foundIdx == 0 || aStringToSearch[foundIdx - 1] == ' ' || Char.IsPunctuation(aStringToSearch, foundIdx - 1)
                            ) &&
                            (
                            foundIdx + aTokenToSearch.Length == aStringToSearch.Length || aStringToSearch[foundIdx + aTokenToSearch.Length] == ' ' || Char.IsPunctuation(aStringToSearch, foundIdx + aTokenToSearch.Length)
                            )
                            )
                            )
                        {
                            results.Add(node);
                        }
                    }
                }
				
				if (SearchDescriptions && pathFinder != null)
				{
					string nodeDescription = MenuInfo.SetExternalDescription(pathFinder, node);
				
					if (nodeDescription != null && nodeDescription.Length > 0)
					{
						if (!CaseSensitive)
							nodeDescription = nodeDescription.ToUpper(CultureInfo.CurrentUICulture);

                        int foundIdx = nodeDescription.IndexOf(aTokenToSearch);
                        if (foundIdx != -1)
                        {
                            if
                                (
                                !ExactWord ||
                                (
                                (
                                foundIdx == 0 || nodeDescription[foundIdx - 1] == ' ' || Char.IsPunctuation(nodeDescription, foundIdx - 1)
                                ) &&
                                (
                                foundIdx + aTokenToSearch.Length == nodeDescription.Length || nodeDescription[foundIdx + aTokenToSearch.Length] == ' ' || Char.IsPunctuation(nodeDescription, foundIdx + aTokenToSearch.Length)
                                )
                                )
                                )
                            {
                                results.Add(node);
                            }
                        }
                    }
				}

                if (SearchItemObjects && !String.IsNullOrEmpty(node.ItemObject))
                {
                    string aStringToSearch = CaseSensitive ? node.ItemObject : node.ItemObject.ToUpper(CultureInfo.CurrentUICulture);
					int foundIdx = aStringToSearch.IndexOf(aTokenToSearch);
					if (foundIdx != -1)
					{
						if 
							(
							!ExactWord || 
							(
							(
							foundIdx == 0 || aStringToSearch[foundIdx - 1] ==  ' ' || Char.IsPunctuation(aStringToSearch, foundIdx - 1)
							)&&
							(
							foundIdx + aTokenToSearch.Length == aStringToSearch.Length || aStringToSearch[foundIdx + aTokenToSearch.Length] ==  ' '  || Char.IsPunctuation(aStringToSearch, foundIdx + aTokenToSearch.Length)
							)
							)
							)
						{
							results.Add(node);
						}
					}
                }
            }

			return results;	
		}
	
		//---------------------------------------------------------------------
		private MenuXmlNodeCollection SearchSingleToken(string aTokenToSearch)
		{
			if (startSearchNode == null)
				return null;

			return SearchSingleToken(startSearchNode.GetAllCommandDescendants(), aTokenToSearch);
		}
		
		#endregion

		#region public methods
		
		//---------------------------------------------------------------------
		public bool SearchExpression(string searchExpression)
		{
			if 
				(
				startSearchNode == null || 
				searchExpression == null ||  
				searchExpression.Trim().Length == 0
				)
				return false;

			string parsedSearchExpression = ValidateExpression(searchExpression);
			if (parsedSearchExpression == null || parsedSearchExpression.Length == 0)
				return false;

			lastResults = SearchExpression(GetMenuNodesToSearch(), parsedSearchExpression);

			return (lastResults != null && lastResults.Count > 0);
		}
		
		#endregion
	}
}

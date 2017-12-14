using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microarea.TaskBuilderNet.Licence.Licence.XmlSyntax;

namespace Microarea.TaskBuilderNet.Licence.Licence
{
	/// <summary>
	/// Summary description for DepExpEvaluator.
	/// </summary>
	public class DepExpEvaluator
	{
		//public DepExpEvaluator(){}
		
		//---------------------------------------------------------------------
		public static void GetDepExp(XmlElement parentElement, out string depexp, out bool onactivated)
		{
			depexp = null;
			onactivated = false;
			
			XmlAttribute onSelfactivatedAtt = parentElement.Attributes[Consts.AttributOnSelfactivated];
			if (onSelfactivatedAtt != null) 
				onactivated =  String.Compare(onSelfactivatedAtt.Value, bool.TrueString, true, CultureInfo.InvariantCulture) == 0;

			XmlNode n = parentElement.SelectSingleNode(Consts.TagArticleDependency);
			if (n == null)
				return;
			
			XmlAttribute att = n.Attributes[Consts.AttributeExpression];
			if (att != null)
				depexp =  att.Value;
		}
					
		
		// Il valore assegnato all'attibuto "activation" può essere dato semplicemente dalla
		// specifica di un'unica funzionalità oppure di un'espressione di tipo logico nella
		// quale si possono “combinare” più funzionalità assieme in modo da effettuare un 
		// controllo di attivazione più complesso.
		// La sintassi da adottare per la formulazione corretta di questa espressione prevede 
		// di usare:
		//	- Il carattere ‘&’ per concatenare due elementi in "and". 
		//	- Il carattere ‘|’ per concatenare due elementi in "or".
		//	- Il carattere ‘!’ per negare un elemento.
		//	- Le parentesi tonde per raggruppare sotto-espressioni 
		//
		// Esempi di valorizzazione dell'attributo "activation" son dati da:
		//		activation="App1.Func1"
		//		activation="!App1.Func1"
		//		activation="App1.Func1 & App1.Func2"
		//		activation="App1.Func1 & !App1.Func2"
		//		activation="(App1.Func1 & App1.Func2) | (App1.Func1 & App1.Func3)"
		//		activation="!(App1.Func1 & App1.Func2) & App1.Func3)"
		//		activation="(App1.Func1 & (App1.Func2 |& !App1.Func3)) | App1.Func4"
		//---------------------------------------------------------------------------
        public static bool CheckDependenciesExpression(string activationExpression, List<ModuleDependecies> moduleslist)
		{
			if (activationExpression == null || activationExpression.Trim().Length == 0)
				return true;


			char[] activationExpressionOperators = new char[2]{'&', '|'};
			char[] activationExpressionKeywords = new char[5]{'!', '&', '|', '(', ')'};
		
			char currentOperator = '\0';

			string expression = activationExpression.Trim();
			bool expressionvalue = true;

			while (expression != null && expression.Length > 0)
			{		
				bool tokenValue = true;

				int firstKeyIndex = expression.IndexOfAny(activationExpressionKeywords);

				if (firstKeyIndex >= 0)
				{
					if (firstKeyIndex == 0 && expression[0] != '!' && expression[0] != '(')
					{
						// errore di sintassi: l'espressione non può cominciare con un'operatore di tipo '&', '|', ')'
						throw new DependenciesExpressionException(/*Strings.ExceptionParsingDependencyMSGStartWhitOperator*/"ERR");
					}

					bool negateToken = (expression[0] == '!');
					if (negateToken)
						expression = expression.Substring(1).TrimStart();

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

					} while (charIndex < expression.Length);// esco dal while solo se l'espressione è terminata
					// o se ho chiuso tutte eventuali le parentesi tonde
					if (openingParenthesisCount != 0)
					{
						// errore di sintassi: non c'è un matching corretto di parentesi
						throw new DependenciesExpressionException(/*Strings.ExceptionParsingDependencyMSGParentheses*/"ERR");
					}
					
					string token = String.Empty;

					if (charIndex > 0)// il token comincia con una parentesi e charIndex punta all'ultima parentesi chiusa
					{
						token = expression.Substring(1, charIndex - 1).Trim();
						expression = expression.Substring(charIndex + 1).Trim();
					}
					else
					{
						if (negateToken)
						{
							int tokenLength = expression.IndexOfAny(activationExpressionOperators);
							if (tokenLength == -1)
							{
								token = expression;
								expression = String.Empty;
							}
							else
							{
								token = expression.Substring(0, tokenLength).Trim();
								expression = expression.Substring(tokenLength).Trim();
							}
						}
						else
						{
							token = expression.Substring(0, firstKeyIndex).Trim();
							expression = expression.Substring(firstKeyIndex).Trim();
						}
					}
					if (currentOperator != '\0' && (token == null || token.Length == 0)) // non è il primo operando !
					{
						// errore di sintassi: l'espressione non può terminare con un'operatore di tipo '&', '|', ')'
						throw new DependenciesExpressionException(/*Strings.ExceptionParsingDependencyMSGEndWhitOperator*/"ERR");
					}
					
					tokenValue = CheckDependenciesExpression(token, moduleslist);
					if (negateToken)
						tokenValue = !tokenValue;
				}
				else
				{
					tokenValue = CheckSingleDependecy(expression, moduleslist);
					expression = String.Empty;
				}

				if (currentOperator == '&')
					expressionvalue = expressionvalue && tokenValue;
				else if (currentOperator == '|')
					expressionvalue = expressionvalue || tokenValue;
				else if (currentOperator == '\0') // è il primo operando !
					expressionvalue = tokenValue;

				if (expression == null || expression.Length == 0)
					break;

				currentOperator = expression[0];

				if (!expressionvalue && currentOperator == '&')
					return false;

				if (expressionvalue && currentOperator == '|')
					return true;

				expression = expression.Substring(1).TrimStart();
		
				if (currentOperator != '\0' && (expression == null || expression.Length == 0))
				{
					// errore di sintassi: l'espressione non può terminare con un'operatore di tipo '&', '|', ')'
					throw new DependenciesExpressionException(/*Strings.ExceptionParsingDependencyMSGEndWhitOperator*/"ERR");
				}
			}
			return expressionvalue;
		}
		
		//---------------------------------------------------------------------------
        private static bool CheckSingleDependecy(string singleActivation, List<ModuleDependecies> moduleslist)
		{
			string activationToCheck = singleActivation.Trim();
			if (activationToCheck == null || activationToCheck.Length == 0)
				return true;
			if (moduleslist == null || moduleslist.Count == 0)
				return false;
			foreach (ModuleDependecies md in moduleslist)
			{
				if (String.Compare(md.name, singleActivation, true, CultureInfo.InvariantCulture) == 0 && md.licensed)
					return true;
				//verifico se sia in un package
				foreach (IncludedSMInfo s in md.includedSM)
					if (String.Compare(s.Name, singleActivation, true, CultureInfo.InvariantCulture) == 0 && md.licensed)
						return true;
			}
			return false;
		}
		
	}

	//=====================================================================
	public struct ModuleDependecies
	{
		public string	name;
		public string	localizedName;
		public bool		licensed;
		public string	expression;
		public IncludedSMInfo[] includedSM;

		//---------------------------------------------------------------------------
		public ModuleDependecies(string name, string localizedname, bool licensed, string exp, IncludedSMInfo[] includedSM)
		{
			this.name			= name;
			this.localizedName	= localizedname;
			this.licensed		= licensed;
			this.expression		= exp;
			this.includedSM		= includedSM;
		}

		//---------------------------------------------------------------------------
		public ModuleDependecies(string name)
		{
			this.name			= name;
			this.localizedName	= null;
			this.licensed		= true;
			this.expression		= null;
			this.includedSM		= new IncludedSMInfo[] { };
		}
	}
	
	//=====================================================================
	public class DependenciesExpressionException : Exception
	{
		public DependenciesExpressionException(string msg) : base(msg)
		{
		}
	}
}


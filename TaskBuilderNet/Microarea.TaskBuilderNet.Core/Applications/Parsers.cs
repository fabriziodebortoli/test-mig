using System;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Lexan;

namespace Microarea.TaskBuilderNet.Core.Applications
{
	//---------------------------------------------------------------------------
	public class DataTypeParser
	{
		//---------------------------------------------------------------------------
		static public bool Parse(Parser lex, Enums enums, out string type, out string woormType, out ushort tag)
		{
			tag = 0;
			type = "String";
			woormType = "String";
			Token tokenType = lex.LookAhead();
			try
			{
				woormType = Language.GetTokenString(tokenType);
				type = ObjectHelper.FromTBType(woormType);
			}
			catch (ObjectHelperException)
			{
				if (tokenType == Token.ID) 
					lex.ParseID(out type);
				lex.SetError(string.Format(ApplicationsStrings.IllegalDataType, type));
				return false;
			}
			lex.SkipToken();
			
			if (type != "DataEnum")
				return true;
			if (enums == null)
				return false;

			// sintassi per dichiarare  gli enumerativi: ENUM["TAGNAME"] NomeVariabile;
			// alla release 7 è accettata anche la segunete ENUM[ tag-value ] NomeVariabile;
			if (!lex.ParseTag(Token.SQUAREOPEN))
				return false;
			if (lex.LookAhead(Token.TEXTSTRING))
			{
				string tagName;
				if (!lex.ParseString(out tagName))
					return false;
				if (!enums.ExistTag(tagName))
				{
					lex.SetError(string.Format(ApplicationsStrings.ExpectedEnum, tagName));
					return false;
				}
				tag = enums.TagValue(tagName);				
			}
			else if (lex.LookAhead
						(
							new Token[] 
							{
								Token.INT, 
								Token.SBYTE,
								Token.BYTE,
								Token.SHORT,
								Token.USHORT,
								Token.INT,
								Token.UINT,
								Token.LONG
							}
						)
					)
			{
				int tagValue = 0;
				if (!lex.ParseInt(out tagValue))
					return false;
				if (!enums.ExistTag((ushort)tagValue))
				{
					lex.SetError(string.Format(ApplicationsStrings.ExpectedEnum, tagValue));
					return false;
				}
				tag = (ushort) tagValue;
			}
			else
			{
				lex.SetError(string.Format(ApplicationsStrings.ExpectedEnum, lex.CurrentLexeme));
				return false;
			}
			return lex.ParseTag(Token.SQUARECLOSE);
		}
		
		//---------------------------------------------------------------------------
		static public bool Parse(Parser lex, Enums enums, out string type, out string woormType, out ushort tag, out string baseType)
		{
			baseType = string.Empty;
			if (!Parse(lex, enums, out type, out woormType, out tag))
				return false;

			if (string.Compare(Language.GetTokenString(Token.ARRAY), woormType, true, System.Globalization.CultureInfo.InvariantCulture) == 0)
			{
				string dummyType = string.Empty;
				if (!DataTypeParser.Parse(lex, enums, out baseType, out dummyType, out tag))
					return false;

				if (string.Compare(Language.GetTokenString(Token.ARRAY), baseType, true, System.Globalization.CultureInfo.InvariantCulture) == 0)
					return false;
			}
			return true;
		}
	}
	
	//---------------------------------------------------------------------------
	public class ComplexDataParser
	{
		// attenzione devo creare data comunque e quindi ignoro errori di parse
		// fino a quando non esco. in caso di errore il data è a valore di default
		//---------------------------------------------------------------------------
		static public bool Parse(Parser lex, Enums enums, out object data, bool parseBraceOpen = true)
		{
			data = null;
			bool ok = true;
			if (parseBraceOpen && !lex.ParseTag(Token.BRACEOPEN))
				return false;

			switch (lex.ComplexData())
			{
				case Parser.ComplexDataType.DataEnum:
				{
					if (lex.LookAhead(Token.TEXTSTRING))
					{
						string tag;
						string item;
						ok = lex.ParseDataEnum(out tag, out item);
						DataEnum de = enums.DataEnumCreate(tag, item);
						data = de;
					}
					else if (lex.LookAhead
						(
							new Token[] 
							{
								Token.INT, 
								Token.SBYTE,
								Token.BYTE,
								Token.SHORT,
								Token.USHORT,
								Token.INT,
								Token.UINT,
								Token.LONG
							}
						)
					)
					{
						int tag;
						int item;
						ok = lex.ParseDataEnum(out tag, out item);
						DataEnum de = new DataEnum((ushort)tag, (ushort)item);
						data = de;
					}
					break;
				}

				case Parser.ComplexDataType.Time:
				case Parser.ComplexDataType.Date:
				case Parser.ComplexDataType.DateTime:
				{
					DateTime dt; 
					ok = lex.ParseDateTime(out dt);
					data = dt;
					break;
				}

				// l'ElapsedTime in TB C++ è rappresentato da un long
				case Parser.ComplexDataType.TimeSpan:
				{
					TimeSpan ts; 
					ok = lex.ParseTimeSpan(out ts);
					long millisecs = (long) ts.TotalMilliseconds;
					data = millisecs;
					break;
				}

				default :
				    lex.SetError(ApplicationsStrings.SyntaxError); 
					return false;
			}

			return ok && lex.ParseTag(Token.BRACECLOSE);
		}
	}
}
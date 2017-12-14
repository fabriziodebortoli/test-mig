using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Microarea.Library.SqlScriptUtility;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;

namespace Microarea.Library.TBWizardProjects
{
    #region CodeTemplateParser abstract class
    /// <summary>
    /// Summary description for CodeTemplateParser.
    /// </summary>
    internal abstract class CodeTemplateParser
    {
        public event TBWizardCodeGeneratorEventHandler SubstitutionInProgress = null;
        public event UnresolvedInjectionPointsEventHandler ResolveCustomInjectionPoints = null;

        private const char SubstitutionSymbol = '@';
        private static string BeginSubstitutionMarker = SubstitutionSymbol + "(";
        private const string EndSubstitutionMarker = ")";
        private const string ReplaceBlanksToken = "_RB";
        private const string DeleteDuplicatedTokensToken = "_DD";
        private const string SkipEmptyLinesToken = "_NOEMPTYLINES:";
        private const string EliminateInvalidLanguageIdCharactersToken = "_NOINVCHARS:";
        private const string ConcatenateFollowingLinesMarker = "@(+)";

        private const string DeleteDuplicatedLinesToken = "DeleteDuplicatedLines";

       /* internal enum DBType : short
        {
            Undefined = 0x0000,
            SQLServer = 0x0001,
            Oracle = 0x0002
        }*/

        private string currentFileName = String.Empty;

        //----------------------------------------------------------------------------
        public string CurrentFileName { get { return currentFileName; } }

        //----------------------------------------------------------------------------
        internal abstract string GetTokenValue(string aToken);

        //------------------------------------------------------------------------------
        internal string SubstituteTokens(string source, bool preserveUnresolvedTokens, bool substituteTokensXMLReservedCharacters, bool isDoubleQuotedText)
        {
            bool setUpperCase = false;
            string textToParse = source;
            string parsedText = String.Empty;

            while (textToParse != null && textToParse.Length > 0)
            {
                if (SubstitutionInProgress != null)
                    SubstitutionInProgress(this, parsedText);

                // cerco "@("
                int beginMarkerIndex = textToParse.IndexOf(BeginSubstitutionMarker);
                if (beginMarkerIndex == -1)
                {
                    parsedText += textToParse;
                    break;
                }

                int startTokenIndex = beginMarkerIndex + BeginSubstitutionMarker.Length;
                if (startTokenIndex >= textToParse.Length - 1)
                {
                    parsedText += textToParse;
                    break;
                }

                // controllo la sintassi "@@(" che significa "metti in maiuscolo")
                setUpperCase = (beginMarkerIndex > 0) && textToParse[beginMarkerIndex - 1] == SubstitutionSymbol;

                // copio la parte fino al "@("
                parsedText += textToParse.Substring(0, beginMarkerIndex - (setUpperCase ? 1 : 0));

                int nextDoubleQuoteIndex = -1;
                do
                {
                    nextDoubleQuoteIndex = parsedText.IndexOf("\"", nextDoubleQuoteIndex + 1);
                    if (nextDoubleQuoteIndex == -1 || nextDoubleQuoteIndex >= parsedText.Length)
                        break;

                    if (nextDoubleQuoteIndex > 0 && parsedText[nextDoubleQuoteIndex - 1] == '\\')
                        continue; // il doppio apice è preceduto da un escape e quindi non conta

                    isDoubleQuotedText = !isDoubleQuotedText;

                } while (true);

                // cerca la ) che ci deve assolutamente essere
                int endMarkerIndex = textToParse.IndexOf(EndSubstitutionMarker, startTokenIndex);

                int openParenthesisIdx = textToParse.IndexOf('(', startTokenIndex);
                while (openParenthesisIdx >= 0 && openParenthesisIdx < endMarkerIndex)
                {
                    if (endMarkerIndex == -1 || endMarkerIndex == textToParse.Length - 1)
                        break; // le parentesi non matchano

                    endMarkerIndex = textToParse.IndexOf(EndSubstitutionMarker, endMarkerIndex + 1);

                    openParenthesisIdx = textToParse.IndexOf('(', openParenthesisIdx + 1);
                }

                // Se fra il carattere appena seguente a "@(" e la prima parentesi 
                // chiusa che trovo ci sono altri parentesi aperte vuol dire che la
                // sintassi è del tipo
                // @(RepeatOnColumns([@(ColumnName)] @(ColumnDBFullDataType)))
                // oppure
                // @(RepeatOnColumns{[@(ColumnName)] @(ColumnDBFullDataType)})
                string tmpTokenText = textToParse.Substring(startTokenIndex);
                while (tmpTokenText != null && tmpTokenText.Length > 1 && Char.IsLetter(tmpTokenText[0]))
                    tmpTokenText = tmpTokenText.Substring(1);
                tmpTokenText = tmpTokenText.Replace('\t', ' ');
                tmpTokenText = tmpTokenText.Trim();
                if (tmpTokenText[0] == '{')
                {
                    int openCurlyBraceIdx = textToParse.IndexOf('{', startTokenIndex);
                    if (openCurlyBraceIdx >= 0 && openCurlyBraceIdx < endMarkerIndex)
                    {

                        int openingCurlyBraceCount = 1;
                        int charIndex = openCurlyBraceIdx + 1;
                        do
                        {
                            if (textToParse[charIndex] == '{')
                                openingCurlyBraceCount++;
                            else if (textToParse[charIndex] == '}')
                                openingCurlyBraceCount--;

                            if (openingCurlyBraceCount == 0)
                            {
                                endMarkerIndex = textToParse.IndexOf(EndSubstitutionMarker, charIndex);
                                break;
                            }

                            charIndex++;

                        } while (charIndex < textToParse.Length);
                    }
                }

                // se manca la ) salta alla prossima riga
                if (endMarkerIndex == -1)
                {
                    parsedText += textToParse;
                    break;
                }

                // skippo "@(" e recupero il token da sostituire
                string tokenToSubstitute = textToParse.Substring
                    (
                    startTokenIndex,
                    endMarkerIndex - startTokenIndex
                    );

                textToParse = textToParse.Substring(endMarkerIndex + EndSubstitutionMarker.Length);

                bool skipEmptyLines = false;
                if (tokenToSubstitute.StartsWith(SkipEmptyLinesToken))
                {
                    skipEmptyLines = true;
                    if (tokenToSubstitute.Length > SkipEmptyLinesToken.Length)
                        tokenToSubstitute = tokenToSubstitute.Substring(SkipEmptyLinesToken.Length);
                }

                bool replaceBlanks = false;
                char blankSubstitutionChar = ' ';
                if (tokenToSubstitute.StartsWith(ReplaceBlanksToken))
                {
                    if (tokenToSubstitute.Length > ReplaceBlanksToken.Length)
                    {
                        string innerToken = tokenToSubstitute.Substring(ReplaceBlanksToken.Length).Trim();
                        if
                        (
                            innerToken.Length >= 3 &&
                            (
                            (innerToken[0] == '(' && innerToken[2] == ')') ||
                            (innerToken[0] == '{' && innerToken[2] == '}')
                            )
                        )
                        {
                            replaceBlanks = true;
                            blankSubstitutionChar = innerToken[1];

                            tokenToSubstitute = tokenToSubstitute.Substring(ReplaceBlanksToken.Length);

                            int endReplaceBlanksStatementIdx = tokenToSubstitute.IndexOf(innerToken[2]);
                            if (blankSubstitutionChar == innerToken[2])
                                endReplaceBlanksStatementIdx++;

                            if (tokenToSubstitute.Length > endReplaceBlanksStatementIdx + 1)
                                tokenToSubstitute = tokenToSubstitute.Substring(endReplaceBlanksStatementIdx + 1);
                            else
                                tokenToSubstitute = String.Empty;
                        }
                    }
                }

                bool deleteDuplicatedTokens = false;
                char tokenToCheckSeparator = ' ';
                bool ignoreCaseCheck = false;
                if (tokenToSubstitute.StartsWith(DeleteDuplicatedTokensToken))
                {
                    if (tokenToSubstitute.Length > DeleteDuplicatedTokensToken.Length)
                    {
                        string innerToken = tokenToSubstitute.Substring(DeleteDuplicatedTokensToken.Length).Trim();
                        if
                            (
                            innerToken.Length >= 5 &&
                            innerToken[2] == ',' &&
                            (innerToken[3] == 't' || innerToken[3] == 'T' || innerToken[3] == 'f' || innerToken[3] == 'F') &&
                            (
                            (innerToken[0] == '(' && innerToken[4] == ')') ||
                            (innerToken[0] == '{' && innerToken[4] == '}')
                            )
                            )
                        {
                            deleteDuplicatedTokens = true;
                            tokenToCheckSeparator = innerToken[1];
                            ignoreCaseCheck = (innerToken[3] == 't' || innerToken[3] == 'T');

                            string tmpTokenToSubstitute = tokenToSubstitute.Substring(DeleteDuplicatedTokensToken.Length);

                            int tmpStartIndex = 0;
                            if (tokenToCheckSeparator == innerToken[4])
                                tmpStartIndex = 2;
                            int endDeleteDuplicatedTokensStatementIdx = tmpTokenToSubstitute.IndexOf(innerToken[4], tmpStartIndex);

                            int beginSubTokenIndex = endDeleteDuplicatedTokensStatementIdx + 1;
                            if (tmpTokenToSubstitute.Length > endDeleteDuplicatedTokensStatementIdx + 1)
                            {
                                if (tmpTokenToSubstitute[beginSubTokenIndex] == '(' || tmpTokenToSubstitute[beginSubTokenIndex] == '{')
                                {
                                    char matchingParenthesis = (tmpTokenToSubstitute[beginSubTokenIndex] == '(') ? ')' : '}';
                                    beginSubTokenIndex++;

                                    if (tmpTokenToSubstitute.Length > beginSubTokenIndex + 1)
                                    {
                                        tokenToSubstitute = tmpTokenToSubstitute.Substring(beginSubTokenIndex, tmpTokenToSubstitute.LastIndexOf(matchingParenthesis) - beginSubTokenIndex);
                                    }
                                    else
                                        tokenToSubstitute = String.Empty;
                                }
                            }
                        }
                    }
                }

                bool eliminateInvalidLanguageIdCharacters = false;
                if (tokenToSubstitute.StartsWith(EliminateInvalidLanguageIdCharactersToken))
                {
                    eliminateInvalidLanguageIdCharacters = true;
                    if (tokenToSubstitute.Length > EliminateInvalidLanguageIdCharactersToken.Length)
                        tokenToSubstitute = tokenToSubstitute.Substring(EliminateInvalidLanguageIdCharactersToken.Length);
                }

                string tokenValue = String.Empty;

                bool deleteDuplicatedLines = false;
                string subToken;
                if (deleteDuplicatedTokens)
                {
                    tokenValue = SubstituteTokens(tokenToSubstitute, preserveUnresolvedTokens, substituteTokensXMLReservedCharacters, isDoubleQuotedText);
                }
                else if
                    (
                    tokenToSubstitute.StartsWith(DeleteDuplicatedLinesToken) &&
                    ResolveFunctionToken(tokenToSubstitute, DeleteDuplicatedLinesToken, out subToken)
                    )
                {
                    deleteDuplicatedLines = true;
                    tokenValue = SubstituteTokens(subToken, preserveUnresolvedTokens, substituteTokensXMLReservedCharacters, isDoubleQuotedText);
                }
                else
                    tokenValue = GetTokenValue(tokenToSubstitute.Trim());

                if (tokenValue != null)
                {
                    if (tokenValue.Length > 0)
                    {
                        if (setUpperCase)
                            tokenValue = tokenValue.ToUpper();

                        if (replaceBlanks)
                            tokenValue = tokenValue.Replace(' ', blankSubstitutionChar);

                        if (deleteDuplicatedTokens)
                        {
                            string[] subTokensToCheck = tokenValue.Split(new char[] { tokenToCheckSeparator });

                            if (subTokensToCheck != null && subTokensToCheck.Length > 0)
                            {
                                ArrayList notDuplicatedTokens = new ArrayList();
                                foreach (string aSubToken in subTokensToCheck)
                                {
                                    if (aSubToken != null && aSubToken.Length > 0)
                                    {
                                        bool skipSubToken = false;
                                        if (notDuplicatedTokens.Count > 0)
                                        {
                                            foreach (string aNotDuplicatedToken in notDuplicatedTokens)
                                            {
                                                if (String.Compare(aNotDuplicatedToken, aSubToken, ignoreCaseCheck) == 0)
                                                {
                                                    skipSubToken = true;
                                                    break;
                                                }
                                            }
                                        }
                                        if (!skipSubToken)
                                            notDuplicatedTokens.Add(aSubToken);
                                    }
                                }
                                if (notDuplicatedTokens.Count > 0)
                                {
                                    tokenValue = String.Empty;
                                    foreach (string aNotDuplicatedToken in notDuplicatedTokens)
                                    {
                                        tokenValue += aNotDuplicatedToken;
                                        tokenValue += tokenToCheckSeparator;
                                    }
                                }
                            }
                        }

                        if (eliminateInvalidLanguageIdCharacters)
                            tokenValue = Generics.SubstitueInvalidCharacterInIdentifier(tokenValue);

                        if (substituteTokensXMLReservedCharacters)
                            tokenValue = Generics.SubstituteXMLReservedCharacters(tokenValue);

                        if (!isDoubleQuotedText)
                            ReplaceEscapedCharacters(ref tokenValue);

                        if (skipEmptyLines)
                        {
                            bool skipToken = true;
                            foreach (char aTokenCharacter in tokenValue)
                            {
                                if (aTokenCharacter != ' ' && aTokenCharacter != '\t' && aTokenCharacter != '\r' && aTokenCharacter != '\n')
                                {
                                    skipToken = false;
                                    break;
                                }
                            }
                            if (skipToken)
                                continue;

                            ArrayList notEmptyLines = new ArrayList();
                            string tmpTokenValue = tokenValue;
                            int newLineIndex = tmpTokenValue.IndexOf("\r\n");
                            char[] noTextCharacters = new char[] { ' ', '\t' };
                            while (newLineIndex >= 0)
                            {
                                if (tmpTokenValue.Length == 0)
                                    break;

                                string tokenLine = tmpTokenValue.Substring(0, newLineIndex);
                                tmpTokenValue = tmpTokenValue.Substring(newLineIndex + 2);
                                newLineIndex = tmpTokenValue.IndexOf("\r\n");

                                if
                                    (
                                    tokenLine == null ||
                                    tokenLine.Length == 0 ||
                                    tokenLine.Trim(noTextCharacters).Length == 0
                                    )
                                    continue;

                                notEmptyLines.Add(tokenLine);
                            }

                            tokenValue = String.Empty;
                            if (notEmptyLines.Count > 0)
                            {
                                foreach (string insertedLine in notEmptyLines)
                                {
                                    tokenValue += insertedLine;
                                    tokenValue += "\r\n";
                                }
                            }
                            tokenValue += tmpTokenValue;
                        }

                        if (deleteDuplicatedLines)
                        {
                            ArrayList cleanedLines = new ArrayList();
                            string tmpTokenValue = tokenValue;
                            int newLineIndex = tmpTokenValue.IndexOf("\r\n");
                            while (newLineIndex >= 0)
                            {
                                if (tmpTokenValue.Length == 0)
                                    break;

                                string tokenLine = tmpTokenValue.Substring(0, newLineIndex);
                                tmpTokenValue = tmpTokenValue.Substring(newLineIndex + 2);
                                newLineIndex = tmpTokenValue.IndexOf("\r\n");

                                if (tokenLine == null || tokenLine.Trim().Length == 0)
                                    continue;

                                bool addLine = true;
                                if (cleanedLines.Count > 0)
                                {
                                    foreach (string insertedLine in cleanedLines)
                                    {
                                        if (String.Compare(insertedLine, tokenLine) == 0)
                                        {
                                            addLine = false;
                                            break;
                                        }
                                    }
                                }
                                if (addLine)
                                    cleanedLines.Add(tokenLine);
                            }

                            tokenValue = String.Empty;
                            if (cleanedLines.Count > 0)
                            {
                                foreach (string insertedLine in cleanedLines)
                                {
                                    tokenValue += insertedLine;
                                    tokenValue += "\r\n";
                                }
                            }
                            tokenValue += tmpTokenValue;
                        }

                    }
                    else if (skipEmptyLines)
                        continue;

                    parsedText += tokenValue;
                }
                else if (preserveUnresolvedTokens)
                {
                    if (setUpperCase)
                        parsedText += SubstitutionSymbol;

                    parsedText += BeginSubstitutionMarker;

                    if (replaceBlanks)
                        parsedText += ReplaceBlanksToken + '(' + blankSubstitutionChar + ')';

                    if (eliminateInvalidLanguageIdCharacters)
                        parsedText += EliminateInvalidLanguageIdCharactersToken;

                    parsedText += tokenToSubstitute + EndSubstitutionMarker;
                }
            }

            return parsedText;
        }

        //------------------------------------------------------------------------------
        internal string SubstituteTokens(string source, bool preserveUnresolvedTokens, bool substituteTokensXMLReservedCharacters)
        {
            return SubstituteTokens(source, preserveUnresolvedTokens, substituteTokensXMLReservedCharacters, false);
        }

        //------------------------------------------------------------------------------
        internal string SubstituteTokens(string source, bool preserveUnresolvedTokens)
        {
            return SubstituteTokens(source, preserveUnresolvedTokens, false);
        }

        //------------------------------------------------------------------------------
        internal string SubstituteTokens(string source)
        {
            return SubstituteTokens(source, false, false);
        }

        //------------------------------------------------------------------------------
        internal bool ResolveFunctionToken(string aToken, string aFunctionName, out string subToken)
        {
            subToken = String.Empty;

            if (!aToken.StartsWith(aFunctionName) || aToken.Length == aFunctionName.Length)
                return false;

            aToken = aToken.Substring(aFunctionName.Length).Trim();
            if (aToken[0] != '(' && aToken[0] != '{') // può trattarsi di un altro token che inizia come aFunctionName
                return false;

            int lastClosedParenthesis = (aToken[0] == '{') ? aToken.LastIndexOf('}') : aToken.LastIndexOf(')');
            if (lastClosedParenthesis == -1)
                return false;	// la parentesi non è chiusa correttamente
            if (lastClosedParenthesis == 1)
                return true;// c'è scritto: @(aFunctionName())

            subToken = aToken.Substring(0, lastClosedParenthesis).Substring(1);

            return true;
        }

        //------------------------------------------------------------------------------
        internal bool ResolveFunctionToken(string aToken, string aFunctionName, out string subToken, out string linesSeparator)
        {
            linesSeparator = String.Empty;

            if (!ResolveFunctionToken(aToken, aFunctionName, out subToken))
                return false;

            if (subToken.Trim().EndsWith("\""))
            {
                int closingDoubleQuoteIndex = subToken.LastIndexOf('\"');
                int openingDoubleQuoteIndex = subToken.LastIndexOf('\"', closingDoubleQuoteIndex - 1);
                if (openingDoubleQuoteIndex != -1)
                {
                    if (subToken.Substring(0, openingDoubleQuoteIndex).Trim().EndsWith(","))
                    {
                        linesSeparator = subToken.Substring(openingDoubleQuoteIndex + 1, closingDoubleQuoteIndex - openingDoubleQuoteIndex - 1);
                        linesSeparator = linesSeparator.Replace("\\r\\n", "\r\n");
                        linesSeparator = linesSeparator.Replace("\\n", "\n");
                        linesSeparator = linesSeparator.Replace("\\t", "\t");
                        subToken = subToken.Substring(0, openingDoubleQuoteIndex);
                        int lastCommaIdx = subToken.LastIndexOf(',');
                        subToken = subToken.Substring(0, lastCommaIdx);
                    }
                }
            }
            return true;
        }

        //------------------------------------------------------------------------------
        internal bool AreTokensToSubstitute(string source)
        {
            return (source != null && source.Length > 0 && source.IndexOf(BeginSubstitutionMarker) >= 0);
        }

        //------------------------------------------------------------------------------
        internal bool WriteFileFromTemplate
            (
            string templateResource,
            string outputFileName,
            Encoding encoding,
            bool saveCodeInInjectionPoint,
            bool substituteTokensXMLReservedCharacters
            )
        {
            if (templateResource == null || templateResource.Length == 0 || outputFileName == null || outputFileName.Length == 0)
                return false;

            if (encoding == null)
                encoding = Encoding.UTF8;

            currentFileName = outputFileName;

            string temporaryOriginalFileCopy = String.Empty;
            InjectionPointsCollection injectionPoints = null;
            if (saveCodeInInjectionPoint)
            {
                // Il file viene sovrascritto (ne esiste già una versione precedente) e, quindi,
                // devo salvare dell'eventuale codice inserito nei punti di iniezione
                injectionPoints = GetCodeInInjectionPoints(outputFileName, encoding);

                if (File.Exists(outputFileName))
                {
                    temporaryOriginalFileCopy = Path.GetTempFileName();
                    File.Copy(outputFileName, temporaryOriginalFileCopy, true);
                }
            }

            bool ok =
                WriteFileFromTemplate(templateResource, outputFileName, encoding, substituteTokensXMLReservedCharacters) &&
                (
                injectionPoints == null ||
                injectionPoints.Count == 0 ||
                SetCodeInInjectionPoints(outputFileName, temporaryOriginalFileCopy, encoding, injectionPoints)
                );

            if (temporaryOriginalFileCopy != null && temporaryOriginalFileCopy.Length > 0)
                File.Delete(temporaryOriginalFileCopy);

            return ok;
        }

        //------------------------------------------------------------------------------
        internal bool WriteFileFromTemplate
            (
            string templateResource,
            string outputFileName,
            Encoding encoding,
            bool substituteTokensXMLReservedCharacters
            )
        {
            if (templateResource == null || templateResource.Length == 0 || outputFileName == null || outputFileName.Length == 0)
                return false;

            if (encoding == null)
                encoding = Encoding.UTF8;

            currentFileName = outputFileName;
			try
			{
                Stream templateStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Library.TBWizardProjects.CodeTemplates." + templateResource);
                if (templateStream == null || !templateStream.CanRead)
                    return false;
//BLEAH!!!! TODO ho messo true che fa append ma era false verificare..... il wizard sovrascrive, qindi vuole false
                System.IO.StreamWriter scriptStream = new StreamWriter(outputFileName, false, encoding);

                System.IO.StreamReader templateReader = new StreamReader(templateStream, encoding);

                string line = null;
                string lineToParse = String.Empty;
                // ReadLine returns the next line from the input stream, or a null reference if the end of the input stream is reached.
                while ((line = templateReader.ReadLine()) != null)
                {
                    if (line.Trim().Length == 0)
                    {
                        scriptStream.WriteLine(line);
                        continue;
                    }

                    // Se trovo il marker di a capo ("@(+)") devo concatenare la corrente linea con
                    // quelle seguenti, finché non trovo più tale marker.
                    // Così una singola linea da tradurre può venire splittata su più righe,
                    // rendendo più leggibile il file di template originale
                    if (line.Trim().EndsWith(ConcatenateFollowingLinesMarker))
                    {
                        lineToParse += line.Substring(0, line.LastIndexOf(ConcatenateFollowingLinesMarker)).TrimStart(" \t".ToCharArray());
                        continue;
                    }
                    else
                        lineToParse += line;

                    string trimmedLine = lineToParse.TrimStart(new char[] { ' ', '\t' });
                    bool skipEmptyLines = trimmedLine.StartsWith(BeginSubstitutionMarker + SkipEmptyLinesToken);

                    string parsedText = SubstituteTokens(lineToParse, false, substituteTokensXMLReservedCharacters);

                    if (parsedText != null && parsedText.Length > 0)
                    {
                        bool writeParsedText = true;
                        if (skipEmptyLines)
                        {
                            writeParsedText = false;
                            foreach (char aParsedCharacter in parsedText)
                            {
                                if (aParsedCharacter != ' ' && aParsedCharacter != '\t' && aParsedCharacter != '\r' && aParsedCharacter != '\n')
                                {
                                    writeParsedText = true;
                                    break;
                                }
                            }

                            ArrayList notEmptyLines = new ArrayList();
                            string tmpTokenValue = parsedText;
                            int newLineIndex = tmpTokenValue.IndexOf("\r\n");
                            char[] noTextCharacters = new char[] { ' ', '\t' };
                            while (newLineIndex >= 0)
                            {
                                if (tmpTokenValue.Length == 0)
                                    break;

                                string tokenLine = tmpTokenValue.Substring(0, newLineIndex);
                                tmpTokenValue = tmpTokenValue.Substring(newLineIndex + 2);
                                newLineIndex = tmpTokenValue.IndexOf("\r\n");

                                if
                                    (
                                    tokenLine == null ||
                                    tokenLine.Length == 0 ||
                                    tokenLine.Trim(noTextCharacters).Length == 0
                                    )
                                    continue;

                                notEmptyLines.Add(tokenLine);
                            }

                            parsedText = String.Empty;
                            if (notEmptyLines.Count > 0)
                            {
                                foreach (string insertedLine in notEmptyLines)
                                {
                                    parsedText += insertedLine;
                                    parsedText += "\r\n";
                                }
                            }
                            parsedText += tmpTokenValue;
                        }

                        if (writeParsedText)
                        {
                            if (String.Compare(parsedText, "\r\n") != 0)
                                scriptStream.WriteLine(parsedText);
                            else
                                scriptStream.WriteLine(String.Empty);
                        }
                    }
                    lineToParse = String.Empty;
                }

                templateReader.Close();

                scriptStream.Flush();
                scriptStream.Close();

                return true;
			}
			catch (Exception exception)
			{
				Debug.Fail("Exception raised in CodeTemplateParser.WriteFileFromTemplate: " + exception.Message);

				throw new TBWizardException(exception.Message);
			}
			finally
			{
				currentFileName = String.Empty;
			}
        }

        //------------------------------------------------------------------------------
        internal bool WriteFileFromTemplate(string templateResource, string outputFileName, bool saveCodeInInjectionPoint, bool substituteTokensXMLReservedCharacters)
        {
            return WriteFileFromTemplate(templateResource, outputFileName, Encoding.UTF8, saveCodeInInjectionPoint, substituteTokensXMLReservedCharacters);
        }

        //------------------------------------------------------------------------------
        internal bool WriteFileFromTemplate(string templateResource, string outputFileName, bool saveCodeInInjectionPoint)
        {
            return WriteFileFromTemplate(templateResource, outputFileName, Encoding.UTF8, saveCodeInInjectionPoint, false);
        }

        //------------------------------------------------------------------------------
        internal bool WriteFileFromTemplate(string templateResource, string outputFileName)
        {
            return WriteFileFromTemplate(templateResource, outputFileName, true);
        }

		///<summary>
		/// Funzione richiamata per la generazione on-demand di uno statement SQL
		///</summary>
		//------------------------------------------------------------------------------
		internal bool WriteStatementFromTemplate
			(
			string templateResource,
			out string statement,
			Encoding encoding
			)
		{
			statement = string.Empty;
			
			if (string.IsNullOrEmpty(templateResource))
				return false;

			if (encoding == null)
				encoding = Encoding.UTF8;
			
			try
			{
				Stream templateStream = 
					Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Library.TBWizardProjects.CodeTemplates." + templateResource);
				
				if (templateStream == null || !templateStream.CanRead)
					return false;
				
				StringBuilder stringBuilder = new StringBuilder();
				StreamReader templateReader = new StreamReader(templateStream, encoding);

				string line = null;
				string lineToParse = String.Empty;
				
				// ReadLine returns the next line from the input stream, or a null reference if the end 
				// of the input stream is reached.
				while ((line = templateReader.ReadLine()) != null)
				{
					if (line.Trim().Length == 0)
					{
						stringBuilder.AppendLine(line);
						continue;
					}

					// Se trovo il marker di a capo ("@(+)") devo concatenare la corrente linea con
					// quelle seguenti, finché non trovo più tale marker.
					// Così una singola linea da tradurre può venire splittata su più righe,
					// rendendo più leggibile il file di template originale
					if (line.Trim().EndsWith(ConcatenateFollowingLinesMarker))
					{
						lineToParse += line.Substring(0, line.LastIndexOf(ConcatenateFollowingLinesMarker)).TrimStart(" \t".ToCharArray());
						continue;
					}
					else
						lineToParse += line;

					string trimmedLine = lineToParse.TrimStart(new char[] { ' ', '\t' });
					bool skipEmptyLines = trimmedLine.StartsWith(BeginSubstitutionMarker + SkipEmptyLinesToken);

					string parsedText = SubstituteTokens(lineToParse, false, false);

					if (parsedText != null && parsedText.Length > 0)
					{
						bool writeParsedText = true;
						if (skipEmptyLines)
						{
							writeParsedText = false;
							foreach (char aParsedCharacter in parsedText)
							{
								if (aParsedCharacter != ' ' && aParsedCharacter != '\t' && 
									aParsedCharacter != '\r' && aParsedCharacter != '\n')
								{
									writeParsedText = true;
									break;
								}
							}

							ArrayList notEmptyLines = new ArrayList();
							string tmpTokenValue = parsedText;
							int newLineIndex = tmpTokenValue.IndexOf("\r\n");
							char[] noTextCharacters = new char[] { ' ', '\t' };
							while (newLineIndex >= 0)
							{
								if (tmpTokenValue.Length == 0)
									break;

								string tokenLine = tmpTokenValue.Substring(0, newLineIndex);
								tmpTokenValue = tmpTokenValue.Substring(newLineIndex + 2);
								newLineIndex = tmpTokenValue.IndexOf("\r\n");

								if (string.IsNullOrEmpty(tokenLine) || tokenLine.Trim(noTextCharacters).Length == 0)
									continue;

								notEmptyLines.Add(tokenLine);
							}

							parsedText = String.Empty;
							if (notEmptyLines.Count > 0)
							{
								foreach (string insertedLine in notEmptyLines)
								{
									parsedText += insertedLine;
									parsedText += "\r\n";
								}
							}
							parsedText += tmpTokenValue;
						}

						if (writeParsedText)
						{
							if (String.Compare(parsedText, "\r\n") != 0)
								stringBuilder.AppendLine(parsedText);
							else
								stringBuilder.AppendLine(String.Empty);
						}
					}
					lineToParse = String.Empty;
				}

				statement = stringBuilder.ToString();
				
				templateReader.Close();
				return true;
			}
			catch (Exception exception)
			{
				Debug.Fail("Exception raised in CodeTemplateParser.WriteFileFromTemplate: " + exception.Message);
				throw new TBWizardException(exception.Message);
			}
			finally
			{
				currentFileName = String.Empty;
			}
		}

        //------------------------------------------------------------------------------
        private InjectionPointsCollection GetCodeInInjectionPoints(string outputFileName, Encoding encoding)
        {
            if (outputFileName == null || outputFileName.Length == 0 || !File.Exists(outputFileName))
                return null;

            if (encoding == null)
                encoding = Encoding.UTF8;

            System.IO.FileStream codeFileReader = null;

            try
            {
                codeFileReader = new System.IO.FileStream(outputFileName, FileMode.Open, FileAccess.Read);

                return InjectionPoint.GetInjectionPoints(codeFileReader, true);
            }
            catch (Exception exception)
            {
                Debug.Fail("Exception raised in CodeTemplateParser.GetCodeInInjectionPoints: " + exception.Message);

                throw new TBWizardException(exception.Message);
            }
            finally
            {
                if (codeFileReader != null)
                    codeFileReader.Close();
            }
        }

        //------------------------------------------------------------------------------
        private bool SetCodeInInjectionPoints(string outputFileName, string temporaryOriginalFileCopy, Encoding encoding, InjectionPointsCollection injectionPoints)
        {
            if
                (
                injectionPoints == null ||
                injectionPoints.Count == 0
                )
                return true;

            if
                (
                outputFileName == null ||
                outputFileName.Length == 0 ||
                !File.Exists(outputFileName)
                )
                return false;

            if (encoding == null)
                encoding = Encoding.UTF8;

            currentFileName = outputFileName;

            string temporaryFile = Path.GetTempFileName();
            System.IO.StreamWriter tmpStreamWriter = null;
            System.IO.StreamReader codeFileReader = null;

            try
            {
                tmpStreamWriter = new StreamWriter(temporaryFile, false, encoding);
                codeFileReader = new StreamReader(outputFileName, encoding);

                string line = null;
                InjectionPoint currentInjectionPoint = null;

                // ReadLine returns the next line from the input stream, or a null reference if the end of the input stream is reached.
                while ((line = codeFileReader.ReadLine()) != null)
                {
                    string currentLine = line.TrimStart(new char[] { ' ', '\t' }).Trim();

                    if (currentLine.Length > 0)
                    {
                        if (currentInjectionPoint != null)
                        {
                            if (currentLine.StartsWith(InjectionPoint.EndInjectionPointMarker))
                            {
                                tmpStreamWriter.Write(currentInjectionPoint.CodeInside);

                                injectionPoints.Remove(currentInjectionPoint);

                                currentInjectionPoint = null;
                            }
                        }
                        else if (currentLine.StartsWith(InjectionPoint.BeginInjectionPointMarker))
                        {
                            string injectionPointHeader = currentLine.Substring(InjectionPoint.BeginInjectionPointMarker.Length);
                            if (injectionPointHeader != null && injectionPointHeader.Length > 0)
                                currentInjectionPoint = injectionPoints.GetInjectionPoint(outputFileName, injectionPointHeader);
                        }
                    }
                    tmpStreamWriter.WriteLine(line);
                }

                codeFileReader.Close();

                codeFileReader = null;

                tmpStreamWriter.Flush();
                tmpStreamWriter.Close();

                // Adesso posso analizzare il codice che è stato inserito in eventuali punti di iniezione
                // personalizzati (cioè definiti ex-novo dallo sviluppatore) e che, pertanto non mi è stato
                // possibile posizionare in automatico nel nuovo file di codice
                if
                    (
                    injectionPoints.Count > 0 &&
                    temporaryOriginalFileCopy != null &&
                    temporaryOriginalFileCopy.Trim().Length > 0
                    )
                {
                    if (ResolveCustomInjectionPoints != null)
                        ResolveCustomInjectionPoints(this, temporaryOriginalFileCopy, temporaryFile, injectionPoints, encoding);
                }

                File.Copy(temporaryFile, outputFileName, true);

                File.Delete(temporaryFile);

                return true;
            }
            catch (Exception exception)
            {
                Debug.Fail("Exception raised in CodeTemplateParser.GetCodeInInjectionPoints: " + exception.Message);

                throw new TBWizardException(exception.Message);
            }
            finally
            {
                currentFileName = String.Empty;

                if (codeFileReader != null)
                    codeFileReader.Close();

                if (tmpStreamWriter != null)
                    tmpStreamWriter.Close();

                if (temporaryFile != null && temporaryFile.Length > 0 && File.Exists(temporaryFile))
                    File.Delete(temporaryFile);
            }
        }

        //------------------------------------------------------------------------------
        private void ReplaceEscapedCharacters(ref string source)
        {
            // Devo rimpiazzare i caratteri speciali preceduti da un escape solo se non si trovano
            // tra doppi apici (cioè se non fanno parte di un literal string) 

            string transformedString = String.Empty;
            int lastOpenedDoubleQuoteIndex = -1;
            int lastClosedDoubleQuoteIndex = -1;
            int nextDoubleQuoteIndex = -1;
            bool isDoubleQuotedText = false;

            do
            {
                nextDoubleQuoteIndex = source.IndexOf("\"", nextDoubleQuoteIndex + 1);
                if (nextDoubleQuoteIndex == -1 || nextDoubleQuoteIndex >= source.Length)
                {
                    if (lastClosedDoubleQuoteIndex < (source.Length - 1))
                    {
                        string tail = source.Substring(lastClosedDoubleQuoteIndex + 1, source.Length - lastClosedDoubleQuoteIndex - 1);
                        tail = tail.Replace("\\t", "\t");
                        tail = tail.Replace("\\n", "\r\n");
                        transformedString += tail;
                    }
                    break;
                }

                if (nextDoubleQuoteIndex > 0 && source[nextDoubleQuoteIndex - 1] == '\\')
                    continue; // il doppio apice è preceduto da un escape e quindi non conta

                isDoubleQuotedText = !isDoubleQuotedText;

                string transformedSubString = String.Empty;
                if (isDoubleQuotedText) // inizio di una literal string
                {
                    transformedSubString = source.Substring(lastClosedDoubleQuoteIndex + 1, nextDoubleQuoteIndex - lastClosedDoubleQuoteIndex - 1);
                    if (transformedSubString != null && transformedSubString.Length > 0)
                    {
                        transformedSubString = transformedSubString.Replace("\\t", "\t");
                        transformedSubString = transformedSubString.Replace("\\n", "\r\n");
                    }
                    lastOpenedDoubleQuoteIndex = nextDoubleQuoteIndex;
                }
                else // chiusura di una literal string
                {
                    transformedSubString = source.Substring(lastOpenedDoubleQuoteIndex, nextDoubleQuoteIndex - lastOpenedDoubleQuoteIndex + 1);
                    lastClosedDoubleQuoteIndex = nextDoubleQuoteIndex;
                }

                transformedString += transformedSubString;

            } while (true);

            source = transformedString;
        }
    }


    #endregion

    #region WizardApplicationCodeTemplateParser class

    //============================================================================
    internal class WizardApplicationCodeTemplateParser : CodeTemplateParser
    {
        private const string ApplicationNameToken = "ApplicationName";
        private const string ApplicationPathToken = "ApplicationPath";
        private const string ApplicationGuidToken = "ApplicationGuid";
        private const string RepeatOnModulesToken = "RepeatOnModules";

        private WizardApplicationInfo applicationInfo = null;

        //----------------------------------------------------------------------------
        internal WizardApplicationCodeTemplateParser(WizardApplicationInfo aApplicationInfo)
        {
            applicationInfo = aApplicationInfo;
        }

        //----------------------------------------------------------------------------
        internal override string GetTokenValue(string aToken)
        {
            if (aToken == null)
                return null;

            aToken = aToken.Trim();
            if (aToken.Length == 0)
                return String.Empty;

            if (String.Compare(aToken, ApplicationNameToken) == 0)
                return (applicationInfo != null) ? applicationInfo.Name : String.Empty;

            if (String.Compare(aToken, ApplicationPathToken) == 0)
                return (applicationInfo != null) ? WizardCodeGenerator.GetStandardApplicationPath(null, applicationInfo, true) : String.Empty;

            // Guid.ToString returns a String representation of the value of
            // the Guid instance, according to the provided format specifier.
            // If the value of format specifier is "B" the returned string 
            // appears as 32 hexadecimal digits separated by hyphens, enclosed 
            // in brackets: {xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}
            if (String.Compare(aToken, ApplicationGuidToken) == 0)
                return (applicationInfo != null) ? applicationInfo.Guid.ToString("B").ToUpper() : String.Empty;

            if (aToken.StartsWith(RepeatOnModulesToken))
            {
                if (applicationInfo == null || applicationInfo.ModulesCount == 0)
                    return String.Empty;

                string subToken;
                string modulesLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnModulesToken, out subToken, out modulesLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string modulesLines = String.Empty;
                    foreach (WizardModuleInfo aModuleInfo in applicationInfo.ModulesInfo)
                    {
                        string moduleParsedText = this.SubstituteTokens(subToken, true);

                        if (AreTokensToSubstitute(moduleParsedText))
                        {
                            WizardModuleCodeTemplateParser moduleParser = new WizardModuleCodeTemplateParser(aModuleInfo);

                            moduleParsedText = moduleParser.SubstituteTokens(moduleParsedText);
                        }

                        if (moduleParsedText != null && moduleParsedText.Length > 0)
                        {
                            if (modulesLines.Length > 0)
                                modulesLines += modulesLinesSeparator;
                            modulesLines += moduleParsedText;
                        }
                    }
                    return modulesLines;
                }
            }

            return null;
        }
    }

    #endregion

    #region WizardModuleCodeTemplateParser class

    //============================================================================
    internal class WizardModuleCodeTemplateParser : CodeTemplateParser
    {
        private const string ApplicationNameToken = "ApplicationName";
        private const string ModuleNameToken = "ModuleName";
        private const string ModuleGuidToken = "ModuleGuid";
        private const string RepeatOnEnumsToken = "RepeatOnEnums";
        private const string RepeatOnLibrariesToken = "RepeatOnLibraries";

        private WizardModuleInfo moduleInfo = null;

        //----------------------------------------------------------------------------
        internal WizardModuleCodeTemplateParser(WizardModuleInfo aModuleInfo)
        {
            moduleInfo = aModuleInfo;
        }

        //----------------------------------------------------------------------------
        internal override string GetTokenValue(string aToken)
        {
            if (aToken == null)
                return null;

            aToken = aToken.Trim();
            if (aToken.Length == 0)
                return String.Empty;

            if (String.Compare(aToken, ApplicationNameToken) == 0)
                return (moduleInfo != null && moduleInfo.Application != null) ? moduleInfo.Application.Name : String.Empty;

            if (String.Compare(aToken, ModuleNameToken) == 0)
                return (moduleInfo != null) ? moduleInfo.Name : String.Empty;

            if (String.Compare(aToken, ModuleGuidToken) == 0)
                return (moduleInfo != null) ? moduleInfo.Guid.ToString("B").ToUpper() : String.Empty;

            if (aToken.StartsWith(RepeatOnLibrariesToken))
            {
                if (moduleInfo == null || moduleInfo.LibrariesCount == 0)
                    return String.Empty;

                string subToken;
                string librariesLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnLibrariesToken, out subToken, out librariesLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string librariesLines = String.Empty;
                    foreach (WizardLibraryInfo aLibraryInfo in moduleInfo.LibrariesInfo)
                    {
                        string libraryParsedText = this.SubstituteTokens(subToken, true);

                        if (AreTokensToSubstitute(libraryParsedText))
                        {
                            WizardLibraryCodeTemplateParser libraryParser = new WizardLibraryCodeTemplateParser(aLibraryInfo);

                            libraryParsedText = libraryParser.SubstituteTokens(libraryParsedText);
                        }

                        if (libraryParsedText != null && libraryParsedText.Length > 0)
                        {
                            if (librariesLines.Length > 0)
                                librariesLines += librariesLinesSeparator;
                            librariesLines += libraryParsedText;
                        }
                    }
                    return librariesLines;
                }
            }

            if (aToken.StartsWith(RepeatOnEnumsToken))
            {
                if (moduleInfo == null || moduleInfo.EnumsCount == 0)
                    return String.Empty;

                string subToken;
                string enumsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnEnumsToken, out subToken, out enumsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string enumsLines = String.Empty;
                    foreach (WizardEnumInfo aEnumInfo in moduleInfo.EnumsInfo)
                    {
                        string enumParsedText = this.SubstituteTokens(subToken, true);

                        if (AreTokensToSubstitute(enumParsedText))
                        {
                            WizardEnumCodeTemplateParser enumParser = new WizardEnumCodeTemplateParser(aEnumInfo);

                            enumParsedText = enumParser.SubstituteTokens(enumParsedText);
                        }

                        if (enumParsedText != null && enumParsedText.Length > 0)
                        {
                            if (enumsLines.Length > 0)
                                enumsLines += enumsLinesSeparator;
                            enumsLines += enumParsedText;
                        }
                    }
                    return enumsLines;
                }
            }

            return null;
        }
    }

    #endregion

    #region WizardLibraryCodeTemplateParser class

    //============================================================================
    internal class WizardLibraryCodeTemplateParser : CodeTemplateParser
    {
        private const string ApplicationNameToken = "ApplicationName";
        private const string ApplicationPathToken = "ApplicationPath";
        private const string ApplicationGuidToken = "ApplicationGuid";
        private const string ModuleNameToken = "ModuleName";
        private const string LibraryNameToken = "LibraryName";
        private const string LibraryNameLowerToken = "LibraryNameLower";
        private const string LibrarySourceFolderToken = "LibrarySourceFolder";
        private const string LibraryGuidToken = "LibraryGuid";
        private const string LibraryNameSpaceToken = "LibraryNameSpace";
        private const string DatabaseReleaseToken = "DatabaseRelease";
        private const string TablesCountToken = "TablesCount";
        private const string RepeatOnTablesToken = "RepeatOnTables";
        private const string RepeatOnDocumentsToken = "RepeatOnDocuments";
        private const string RelativeLibrarySourceFolderToken = "RelativeLibrarySourceFolder";

        private const string IfDefinesTablesToken = "IfDefinesTables";
        private const string IfTrapDSNChangedEventToken = "IfTrapDSNChangedEvent";
        private const string IfTrapApplicationDateChangedEventToken = "IfTrapApplicationDateChangedEvent";

        private const string DependenciesLibListToken = "DependenciesLibList";
        private const string DependenciesDebugLibPathListToken = "DependenciesDebugLibPathList";
        private const string DependenciesReleaseLibPathListToken = "DependenciesReleaseLibPathList";
        private const string RepeatOnDependenciesToken = "RepeatOnDependencies";
        private const string RepeatOnDependenciesApplicationToken = "RepeatOnDependenciesApplication";

        private const string IfDefinesClientDocumentsToken = "IfDefinesClientDocuments";
        private const string IfDefinesDirectClientDocumentsToken = "IfDefinesDirectClientDocuments";
        private const string IfDefinesFamilyClientDocumentsToken = "IfDefinesFamilyClientDocuments";
        private const string RepeatOnClientDocumentsToken = "RepeatOnClientDocuments";
        private const string RepeatOnServerDocumentsToken = "RepeatOnServerDocuments";
        private const string RepeatOnDirectServerDocumentsToken = "RepeatOnDirectServerDocuments";

        private const string IfDefinesAdditionalColumnsToken = "IfDefinesAdditionalColumns";
        private const string RepeatOnAdditionalColumnsInfoToken = "RepeatOnAdditionalColumnsInfo";

        private const string CultureLCID = "CultureLCID";

        private const string IfAreApplicationsReferredToken = "IfAreApplicationsReferred";
        private const string RepeatOnApplicationReferencesToken = "RepeatOnApplicationReferences";

        private WizardLibraryInfo libraryInfo = null;

        //----------------------------------------------------------------------------
        internal WizardLibraryCodeTemplateParser(WizardLibraryInfo aLibraryInfo)
        {
            libraryInfo = aLibraryInfo;
        }

        //----------------------------------------------------------------------------
        internal override string GetTokenValue(string aToken)
        {
            if (aToken == null)
                return null;

            aToken = aToken.Trim();
            if (aToken.Length == 0)
                return String.Empty;

            if (String.Compare(aToken, ApplicationNameToken) == 0)
                return (libraryInfo != null && libraryInfo.Module != null && libraryInfo.Module.Application != null) ? libraryInfo.Module.Application.Name : String.Empty;

            if (String.Compare(aToken, ApplicationPathToken) == 0)
                return (libraryInfo != null && libraryInfo.Module != null && libraryInfo.Module.Application != null) ? WizardCodeGenerator.GetStandardApplicationPath(null, libraryInfo.Module.Application, true) : String.Empty;

            if (String.Compare(aToken, ApplicationGuidToken) == 0)
                return (libraryInfo != null && libraryInfo.Module != null && libraryInfo.Module.Application != null) ? libraryInfo.Module.Application.Guid.ToString("B").ToUpper() : String.Empty;

            if (String.Compare(aToken, ModuleNameToken) == 0)
                return (libraryInfo != null && libraryInfo.Module != null) ? libraryInfo.Module.Name : String.Empty;

            if (String.Compare(aToken, LibraryNameToken) == 0)
                return (libraryInfo != null) ? libraryInfo.Name : String.Empty;

            if (String.Compare(aToken, LibraryNameLowerToken) == 0)
                return (libraryInfo != null) ? libraryInfo.Name.ToLower() : String.Empty;

            if (String.Compare(aToken, LibrarySourceFolderToken) == 0)
                return (libraryInfo != null) ? libraryInfo.SourceFolder : String.Empty;

            if (String.Compare(aToken, LibraryGuidToken) == 0)
                return (libraryInfo != null) ? libraryInfo.Guid.ToString("B").ToUpper() : String.Empty;

            if (String.Compare(aToken, LibraryNameSpaceToken) == 0)
                return (libraryInfo != null) ? libraryInfo.GetNameSpace() : String.Empty;

            if (String.Compare(aToken, DatabaseReleaseToken) == 0)
                return (libraryInfo != null && libraryInfo.Module != null) ? libraryInfo.Module.DbReleaseNumber.ToString() : String.Empty;

            if (String.Compare(aToken, TablesCountToken) == 0)
                return (libraryInfo != null) ? libraryInfo.TablesCount.ToString() : String.Empty;

            if (aToken.StartsWith(RepeatOnTablesToken))
            {
                if (libraryInfo == null || libraryInfo.TablesCount == 0)
                    return String.Empty;

                string subToken;
                string tablesLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnTablesToken, out subToken, out tablesLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string tablesLines = String.Empty;
                    foreach (WizardTableInfo aTableInfo in libraryInfo.TablesInfo)
                    {
                        string tableParsedText = this.SubstituteTokens(subToken, true);

                        if (AreTokensToSubstitute(tableParsedText))
                        {
                            WizardTableCodeTemplateParser tableParser = new WizardTableCodeTemplateParser(aTableInfo);

                            tableParsedText = tableParser.SubstituteTokens(tableParsedText);
                        }

                        if (tableParsedText != null && tableParsedText.Length > 0)
                        {
                            if (tablesLines.Length > 0)
                                tablesLines += tablesLinesSeparator;
                            tablesLines += tableParsedText;
                        }
                    }
                    return tablesLines;
                }
            }

            if (aToken.StartsWith(RepeatOnDocumentsToken))
            {
                if (libraryInfo == null || libraryInfo.DocumentsCount == 0)
                    return String.Empty;

                string subToken;
                string documentsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnDocumentsToken, out subToken, out documentsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string documentsLines = String.Empty;
                    foreach (WizardDocumentInfo aDocumentInfo in libraryInfo.DocumentsInfo)
                    {
                        string documentParsedText = this.SubstituteTokens(subToken, true);

                        if (AreTokensToSubstitute(documentParsedText))
                        {
                            WizardDocumentCodeTemplateParser documentParser = new WizardDocumentCodeTemplateParser(aDocumentInfo);

                            documentParsedText = documentParser.SubstituteTokens(documentParsedText);
                        }

                        if (documentParsedText != null && documentParsedText.Length > 0)
                        {
                            if (documentsLines.Length > 0)
                                documentsLines += documentsLinesSeparator;
                            documentsLines += documentParsedText;
                        }
                    }
                    return documentsLines;
                }
            }

            if (String.Compare(aToken, RelativeLibrarySourceFolderToken) == 0)
            {
                if (libraryInfo == null || libraryInfo.Module == null || libraryInfo.Module.Application == null)
                    return String.Empty;

                string relativePath = Generics.MakeRelativeTo(WizardCodeGenerator.GetStandardApplicationPath(libraryInfo.Module.Application), WizardCodeGenerator.GetStandardLibraryPath(libraryInfo));
                if (relativePath != null && relativePath.Length > 0)
                    return relativePath + Path.DirectorySeparatorChar;

                return String.Empty;
            }

            if (aToken.StartsWith(IfDefinesTablesToken))
            {
                if (libraryInfo == null || libraryInfo.TablesCount == 0)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfDefinesTablesToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfTrapDSNChangedEventToken))
            {
                if (libraryInfo == null || !libraryInfo.TrapDSNChangedEvent)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfTrapDSNChangedEventToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfTrapApplicationDateChangedEventToken))
            {
                if (libraryInfo == null || !libraryInfo.TrapApplicationDateChangedEvent)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfTrapApplicationDateChangedEventToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (String.Compare(aToken, DependenciesLibListToken) == 0)
            {
                if (libraryInfo == null || libraryInfo.Dependencies == null || libraryInfo.Dependencies.Count == 0)
                    return String.Empty;

                string dependenciesLibList = String.Empty;
                foreach (WizardLibraryInfo aDependency in libraryInfo.Dependencies)
                {
                    if (dependenciesLibList.Length > 0)
                        dependenciesLibList += ';';
                    dependenciesLibList += aDependency.Name + ".lib";
                }
                return dependenciesLibList;
            }

            if (String.Compare(aToken, DependenciesDebugLibPathListToken) == 0)
            {
                if (libraryInfo == null || libraryInfo.Dependencies == null || libraryInfo.Dependencies.Count == 0)
                    return String.Empty;

                ArrayList paths = new ArrayList();
                foreach (WizardLibraryInfo aDependency in libraryInfo.Dependencies)
                {
                    string libPath = WizardCodeGenerator.GetStandardLibraryPath(aDependency);
                    if (libPath == null || libPath.Length == 0)
                        continue;

                    libPath += Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + "Debug";

                    bool addLine = true;
                    if (paths.Count > 0)
                    {
                        foreach (string insertedPath in paths)
                        {
                            if (String.Compare(insertedPath, libPath, true) == 0)
                            {
                                addLine = false;
                                break;
                            }
                        }
                    }
                    if (addLine)
                        paths.Add(libPath);
                }
                if (paths.Count == 0)
                    return String.Empty;

                string dependenciesLibList = String.Empty;
                foreach (string aDependencyPath in paths)
                {
                    if (dependenciesLibList.Length > 0)
                        dependenciesLibList += ';';
                    dependenciesLibList += aDependencyPath;
                }
                return dependenciesLibList;
            }

            if (String.Compare(aToken, DependenciesReleaseLibPathListToken) == 0)
            {
                if (libraryInfo == null || libraryInfo.Dependencies == null || libraryInfo.Dependencies.Count == 0)
                    return String.Empty;

                ArrayList paths = new ArrayList();
                foreach (WizardLibraryInfo aDependency in libraryInfo.Dependencies)
                {
                    string libPath = WizardCodeGenerator.GetStandardLibraryPath(aDependency);
                    if (libPath == null || libPath.Length == 0)
                        continue;

                    libPath += Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + "Release";
                    bool addLine = true;
                    if (paths.Count > 0)
                    {
                        foreach (string insertedPath in paths)
                        {
                            if (String.Compare(insertedPath, libPath, true) == 0)
                            {
                                addLine = false;
                                break;
                            }
                        }
                    }
                    if (addLine)
                        paths.Add(libPath);
                }

                if (paths.Count == 0)
                    return String.Empty;

                string dependenciesLibList = String.Empty;
                foreach (string aDependencyPath in paths)
                {
                    if (dependenciesLibList.Length > 0)
                        dependenciesLibList += ';';
                    dependenciesLibList += aDependencyPath;
                }
                return dependenciesLibList;
            }

            if (aToken.StartsWith(RepeatOnDependenciesToken))
            {
                if (libraryInfo == null || libraryInfo.Dependencies == null || libraryInfo.Dependencies.Count == 0)
                    return String.Empty;

                string subToken;
                string dependenciesLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnDependenciesToken, out subToken, out dependenciesLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string dependenciesLines = String.Empty;
                    foreach (WizardLibraryInfo aDependency in libraryInfo.Dependencies)
                    {
                        WizardLibraryCodeTemplateParser dependencyParser = new WizardLibraryCodeTemplateParser(aDependency);

                        string dependencyParsedText = dependencyParser.SubstituteTokens(subToken);

                        if (dependencyParsedText != null && dependencyParsedText.Length > 0)
                        {
                            if (dependenciesLines.Length > 0)
                                dependenciesLines += dependenciesLinesSeparator;
                            dependenciesLines += dependencyParsedText;
                        }
                    }
                    return dependenciesLines;
                }
            }

            if (aToken.StartsWith(RepeatOnDependenciesApplicationToken))
            {
                if (libraryInfo == null || libraryInfo.Dependencies == null || libraryInfo.Dependencies.Count == 0)
                    return String.Empty;

                string subToken;
                string dependenciesLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnDependenciesApplicationToken, out subToken, out dependenciesLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string dependenciesLines = String.Empty;
                    foreach (WizardLibraryInfo aDependency in libraryInfo.Dependencies)
                    {
                        if (
                            libraryInfo.Application == null ||
                            aDependency.Application == null ||
                            String.Compare(aDependency.Application.Name, libraryInfo.Application.Name) == 0
                            )
                            continue;

                        WizardApplicationCodeTemplateParser dependencyAppParser = new WizardApplicationCodeTemplateParser(aDependency.Application);

                        string dependencyParsedText = dependencyAppParser.SubstituteTokens(subToken);

                        if (dependencyParsedText != null && dependencyParsedText.Length > 0)
                        {
                            if (dependenciesLines.Length > 0)
                                dependenciesLines += dependenciesLinesSeparator;
                            dependenciesLines += dependencyParsedText;
                        }
                    }
                    return dependenciesLines;
                }
            }

            if (aToken.StartsWith(IfDefinesClientDocumentsToken))
            {
                if (libraryInfo == null || libraryInfo.ClientDocumentsCount == 0)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfDefinesClientDocumentsToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfDefinesDirectClientDocumentsToken))
            {
                if (libraryInfo == null || !libraryInfo.AreDirectClientDocumentsDefined())
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfDefinesDirectClientDocumentsToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfDefinesFamilyClientDocumentsToken))
            {
                if (libraryInfo == null || !libraryInfo.AreFamilyClientDocumentsDefined())
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfDefinesFamilyClientDocumentsToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(RepeatOnServerDocumentsToken))
            {
                if (libraryInfo == null || libraryInfo.ClientDocumentsCount == 0)
                    return String.Empty;

                WizardDocumentInfoCollection serverDocuments = libraryInfo.GetServerDocuments();
                if (serverDocuments == null || serverDocuments.Count == 0)
                    return String.Empty;

                string subToken;
                string serverDocumentsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnServerDocumentsToken, out subToken, out serverDocumentsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string serverDocumentsLines = String.Empty;
                    foreach (WizardDocumentInfo aServerDocument in serverDocuments)
                    {
                        WizardServerDocumentCodeTemplateParser aServerDocumentParser = new WizardServerDocumentCodeTemplateParser(libraryInfo, aServerDocument);

                        string serverDocumentParsedText = aServerDocumentParser.SubstituteTokens(subToken);

                        if (serverDocumentParsedText != null && serverDocumentParsedText.Length > 0)
                        {
                            if (serverDocumentsLines.Length > 0)
                                serverDocumentsLines += serverDocumentsLinesSeparator;
                            serverDocumentsLines += serverDocumentParsedText;
                        }
                    }
                    return serverDocumentsLines;
                }
            }

            if (aToken.StartsWith(RepeatOnDirectServerDocumentsToken))
            {
                if (libraryInfo == null || libraryInfo.ClientDocumentsCount == 0)
                    return String.Empty;

                WizardDocumentInfoCollection serverDocuments = libraryInfo.GetDirectServerDocuments();
                if (serverDocuments == null || serverDocuments.Count == 0)
                    return String.Empty;

                string subToken;
                string serverDocumentsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnDirectServerDocumentsToken, out subToken, out serverDocumentsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string serverDocumentsLines = String.Empty;
                    foreach (WizardDocumentInfo aServerDocument in serverDocuments)
                    {
                        WizardServerDocumentCodeTemplateParser aServerDocumentParser = new WizardServerDocumentCodeTemplateParser(libraryInfo, aServerDocument);

                        string serverDocumentParsedText = aServerDocumentParser.SubstituteTokens(subToken);

                        if (serverDocumentParsedText != null && serverDocumentParsedText.Length > 0)
                        {
                            if (serverDocumentsLines.Length > 0)
                                serverDocumentsLines += serverDocumentsLinesSeparator;
                            serverDocumentsLines += serverDocumentParsedText;
                        }
                    }
                    return serverDocumentsLines;
                }
            }

            if (aToken.StartsWith(RepeatOnClientDocumentsToken))
            {
                if (libraryInfo == null || libraryInfo.ClientDocumentsCount == 0)
                    return String.Empty;

                string subToken;
                string clientDocumentsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnClientDocumentsToken, out subToken, out clientDocumentsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string clientDocumentsLines = String.Empty;
                    foreach (WizardClientDocumentInfo aClientDocument in libraryInfo.ClientDocumentsInfo)
                    {
                        WizardClientDocumentCodeTemplateParser aClientDocumentParser = new WizardClientDocumentCodeTemplateParser(aClientDocument);

                        string clientDocumentParsedText = aClientDocumentParser.SubstituteTokens(subToken);

                        if (clientDocumentParsedText != null && clientDocumentParsedText.Length > 0)
                        {
                            if (clientDocumentsLines.Length > 0)
                                clientDocumentsLines += clientDocumentsLinesSeparator;
                            clientDocumentsLines += clientDocumentParsedText;
                        }
                    }
                    return clientDocumentsLines;
                }
            }

            if (aToken.StartsWith(IfDefinesAdditionalColumnsToken))
            {
                if (libraryInfo == null || libraryInfo.ExtraAddedColumnsCount == 0)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfDefinesAdditionalColumnsToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(RepeatOnAdditionalColumnsInfoToken))
            {
                if (libraryInfo == null || libraryInfo.ExtraAddedColumnsCount == 0)
                    return String.Empty;

                string subToken;
                string additionalColumnsInfoLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnAdditionalColumnsInfoToken, out subToken, out additionalColumnsInfoLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string additionalColumnsInfoLines = String.Empty;
                    foreach (WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo in libraryInfo.ExtraAddedColumnsInfo)
                    {
                        WizardAdditionalColumnsCodeTemplateParser additionalColumnsParser = new WizardAdditionalColumnsCodeTemplateParser(aExtraAddedColumnsInfo);

                        string additionalColumnsParsedText = additionalColumnsParser.SubstituteTokens(subToken);

                        if (additionalColumnsParsedText != null && additionalColumnsParsedText.Length > 0)
                        {
                            if (additionalColumnsInfoLines.Length > 0)
                                additionalColumnsInfoLines += additionalColumnsInfoLinesSeparator;
                            additionalColumnsInfoLines += additionalColumnsParsedText;
                        }
                    }
                    return additionalColumnsInfoLines;
                }
            }

            if (String.Compare(aToken, CultureLCID) == 0)
                return (libraryInfo != null && libraryInfo.Application != null) ? libraryInfo.Application.CultureLCID.ToString() : String.Empty;

            if (aToken.StartsWith(IfAreApplicationsReferredToken))
            {
                if (libraryInfo == null || libraryInfo.Application == null || !libraryInfo.Application.HasReferences || (libraryInfo.ClientDocumentsCount == 0 && libraryInfo.ExtraAddedColumnsCount == 0))
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfAreApplicationsReferredToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(RepeatOnApplicationReferencesToken))
            {
                if (libraryInfo == null || libraryInfo.Application == null || !libraryInfo.Application.HasReferences || (libraryInfo.ClientDocumentsCount == 0 && libraryInfo.ExtraAddedColumnsCount == 0))
                    return String.Empty;

                string subToken;
                string referencesLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnApplicationReferencesToken, out subToken, out referencesLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    WizardApplicationInfoCollection referredApplications = new WizardApplicationInfoCollection();
                    // Costruisco la lista di applicazioni realmente referenziate dalla libreria corrente
                    if (libraryInfo.ClientDocumentsCount > 0)
                    {
                        foreach (WizardClientDocumentInfo aClientDocument in libraryInfo.ClientDocumentsInfo)
                        {
                            if
                                (
                                aClientDocument == null ||
                                aClientDocument.ServerDocumentInfo == null ||
                                aClientDocument.ServerDocumentInfo.Library == null ||
                                aClientDocument.ServerDocumentInfo.Library.Application == null
                                )
                                continue;
                            if (referredApplications.Count > 0 && referredApplications.Contains(aClientDocument.ServerDocumentInfo.Library.Application.Name))
                                continue;
                            referredApplications.Add(aClientDocument.ServerDocumentInfo.Library.Application);
                        }
                    }
                    if (libraryInfo.ExtraAddedColumnsCount > 0)
                    {
                        foreach (WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo in libraryInfo.ExtraAddedColumnsInfo)
                        {
                            if (aExtraAddedColumnsInfo == null)
                                continue;
                            WizardTableInfo originalTableInfo = aExtraAddedColumnsInfo.GetOriginalTableInfo();
                            if (
                                originalTableInfo == null ||
                                originalTableInfo.Library == null ||
                                originalTableInfo.Library.Application == null
                                )
                                continue;
                            if (referredApplications.Count > 0 && referredApplications.Contains(originalTableInfo.Library.Application.Name))
                                continue;
                            referredApplications.Add(originalTableInfo.Library.Application);
                        }
                    }

                    string referencesLines = String.Empty;
                    if (referredApplications.Count > 0)
                    {
                        foreach (WizardApplicationInfo aReferredApplication in referredApplications)
                        {
                            WizardApplicationCodeTemplateParser aApplicationParser = new WizardApplicationCodeTemplateParser(aReferredApplication);

                            string referenceParsedText = aApplicationParser.SubstituteTokens(subToken);

                            if (referenceParsedText != null && referenceParsedText.Length > 0)
                            {
                                if (referencesLines.Length > 0)
                                    referencesLines += referencesLinesSeparator;
                                referencesLines += referenceParsedText;
                            }
                        }
                    }
                    return referencesLines;
                }
            }

            return null;
        }

    }

    #endregion

    #region WizardDocumentCodeTemplateParser class

    //============================================================================
    internal class WizardDocumentCodeTemplateParser : CodeTemplateParser
    {
        private const string ModuleNameToken = "ModuleName";
        private const string LibraryNameToken = "LibraryName";
        private const string LibrarySourceFolderToken = "LibrarySourceFolder";
        private const string DocumentNameToken = "DocumentName";
        private const string DocumentNameSpaceToken = "DocumentNameSpace";
        private const string DocumentClassNameToken = "DocumentClassName";
        private const string DocumentViewClassNameToken = "DocumentViewClassName";
        private const string DocumentViewFileNameToken = "DocumentViewFileName";
        private const string DocumentTitleToken = "DocumentTitle";
        private const string DocumentFormIdToken = "DocumentFormId";

        private const string DocumentNextResourceValueToken = "DocumentNextResourceValue";
        private const string DocumentNextControlValueToken = "DocumentNextControlValue";
        private const string DocumentNextCommandValueToken = "DocumentNextCommandValue";
        private const string DocumentNextSymedValueToken = "DocumentNextSymedValue";

        private const string MasterDBTNameToken = "MasterDBTName";
        private const string MasterDBTClassNameToken = "MasterDBTClassName";
        private const string MasterTableNameToken = "MasterTableName";
        private const string MasterTableClassNameToken = "MasterTableClassName";
        private const string ActOnMasterToken = "ActOnMaster";
        private const string IfDBTSlavesPresentToken = "IfDBTSlavesPresent";
        private const string RepeatOnDBTsToken = "RepeatOnDBTs";
        private const string RepeatOnDBTSlavesToken = "RepeatOnDBTSlaves";
        private const string RepeatOnTabbedPanesToken = "RepeatOnTabbedPanes";
		private const string RepeatOnLabelsToken = "RepeatOnLabels";
		
        private const string CultureLanguageIdentifierToken = "CultureLanguageIdentifier";
        private const string CultureSubLanguageIdentifierToken = "CultureSubLanguageIdentifier";

        private const string DocumentDialogWidthToken = "DocumentDialogWidth";
        private const string DocumentDialogHeightToken = "DocumentDialogHeight";

        private const string IsTabberToCreateToken = "IsTabberToCreate";
        private const string TabberControlIdToken = "TabberControlId";
        private const string TabberLeftToken = "TabberLeft";
        private const string TabberTopToken = "TabberTop";
        private const string TabberWidthToken = "TabberWidth";
        private const string TabberHeightToken = "TabberHeight";

        private const string FontSizeToken = "FontSize";
        private const string FontNameToken = "FontName";
        private const string FontWeightToken = "FontWeight";
        private const string FontIsItalicToken = "FontIsItalic";
        private const string FontCharSetToken = "FontCharSet";

        private const string IfHKLDefinedToken = "IfHKLDefined";
        private const string DocumentHKLNameToken = "DocumentHKLName";
        private const string DocumentHKLClassNameToken = "DocumentHKLClassName";
        private const string HKLCodeColumnNameToken = "HKLCodeColumnName";
        private const string HKLDescriptionColumnNameToken = "HKLDescriptionColumnName";
        private const string HKLCodeColumnDataObjClassNameToken = "HKLCodeColumnDataObjClassName";
        private const string IfHKLCodeIsTextualToken = "IfHKLCodeIsTextual";
        private const string IfHKLCodeIsNotTextualToken = "IfHKLCodeIsNotTextual";

        private const string IfBatchDocumentToken = "IfBatchDocument";
        private const string IfNotBatchDocumentToken = "IfNotBatchDocument";


		private const string ContolTopToken = "ContolTop";
		private const string ContolLeftToken = "ContolLeft";
		private const string ControlWidthToken = "ContolWidth";
		private const string ControlHeightToken = "ContolHeight";
		private const string LabelTextToken = "LabelText";
		private const string ControlNameToken = "ControlName";
		private const string ControlStylesToken = "ControlStyles";

		private const string GenerateOnUI = "GenerateOnUI";
		private const string UIIDC = "UIIDC";
		private const string IfUIControlToken = "IfUIControl";
		private const string IfUIDialogToken  = "IfUIDialog";

		private const string ifUILabelToken = "ifUILabel";
		private const string ifUIControlToken = "ifUIControl";

		private const string BeginSubstitutionMarker = "@(";
		private const string EndSubstitutionMarker = ")";
		private const string ConcatenateFollowingLinesMarker = "@(+)";

		private const string BeginMacro = "[";
		private const string EndMacro = "]";
		
        private WizardDocumentInfo documentInfo = null;
        private Size dialogSize = Size.Empty;
		
        //----------------------------------------------------------------------------
        internal WizardDocumentCodeTemplateParser(WizardDocumentInfo aDocumentInfo)
        {
            documentInfo = aDocumentInfo;

            InitDialogSize();
        }

        //----------------------------------------------------------------------------
        internal override string GetTokenValue(string aToken)
        {
            if (aToken == null || aToken.Length == 0)
                return String.Empty;

            if (String.Compare(aToken, ModuleNameToken) == 0)
                return (documentInfo != null && documentInfo.Library != null && documentInfo.Library.Module != null) ? documentInfo.Library.Module.Name : String.Empty;

            if (String.Compare(aToken, LibraryNameToken) == 0)
                return (documentInfo != null && documentInfo.Library != null) ? documentInfo.Library.Name : String.Empty;

            if (String.Compare(aToken, LibrarySourceFolderToken) == 0)
                return (documentInfo != null && documentInfo.Library != null) ? documentInfo.Library.SourceFolder : String.Empty;

            if (String.Compare(aToken, DocumentNameToken) == 0)
                return (documentInfo != null) ? documentInfo.Name : String.Empty;

            if (String.Compare(aToken, DocumentNameSpaceToken) == 0)
                return (documentInfo != null) ? documentInfo.GetNameSpace() : String.Empty;

            if (String.Compare(aToken, DocumentClassNameToken) == 0)
                return (documentInfo != null) ? documentInfo.ClassName : String.Empty;

            if (String.Compare(aToken, DocumentViewClassNameToken) == 0)
            {
                if (documentInfo == null)
                    return String.Empty;
                // se il nome della classe del documento è "D" + namespace
                // allora imposta il nome come "C" + namespace + "View" (attuale standard)
                // altrimenti usa nome classe + "View" (vecchio standard)
                //if (documentInfo.ClassName == "D" + documentInfo.Name)
                //    return ("C" + documentInfo.Name) + "View";
                //else
                //    return documentInfo.ClassName + "View";

                // se il nome della classe inizia con "D"
                // allora imposta il nome come "C" + nomeclasse.mid(1) + "View"
                // altrimenti "C" + namespace + "View"
                if (documentInfo.ClassName.Substring(0, 1) == "D")
                    return "C" + documentInfo.ClassName.Substring(1) + "View";
                else
                    return "C" + documentInfo.Name + "View";
            }

            if (String.Compare(aToken, DocumentViewFileNameToken) == 0)
            {
                if (documentInfo == null)
                    return String.Empty;
                // se il nome della classe del documento è "D" + namespace
                // allora imposta il nome come "UI" + namespace (attuale standard)
                // altrimenti usa nome classe (vecchio standard)
                //if (documentInfo.ClassName == "D" + documentInfo.Name)
                    //return "UI" + documentInfo.Name;
                //else
                //    return documentInfo.ClassName + "View";

                // se il nome della classe inizia con "D"
                // allora imposta il nome come "UI" + nomeclasse.mid(1)
                // altrimenti "UI" + namespace
                if (documentInfo.ClassName.Substring(0, 1) == "D")
                    return "UI" + documentInfo.ClassName.Substring(1);
                else
                    return "UI" + documentInfo.Name;
            }

            if (String.Compare(aToken, DocumentTitleToken) == 0)
                return (documentInfo != null) ? documentInfo.Title : String.Empty;

            if (String.Compare(aToken, DocumentFormIdToken) == 0)
                return (documentInfo != null) ? documentInfo.GetDocumentFormId().ToString() : String.Empty;

            if (String.Compare(aToken, DocumentNextResourceValueToken) == 0)
                return (documentInfo != null) ? documentInfo.GetFirstAvailableResourceId().ToString() : String.Empty;

            if (String.Compare(aToken, DocumentNextControlValueToken) == 0)
                return (documentInfo != null) ? documentInfo.GetFirstAvailableControlId().ToString() : String.Empty;

            if (String.Compare(aToken, DocumentNextCommandValueToken) == 0)
                return (documentInfo != null) ? documentInfo.GetFirstAvailableCommandId().ToString() : String.Empty;

            if (String.Compare(aToken, DocumentNextSymedValueToken) == 0)
                return (documentInfo != null) ? documentInfo.GetFirstAvailableSymedId().ToString() : String.Empty;

            if (String.Compare(aToken, MasterDBTNameToken) == 0)
            {
                if (documentInfo == null)
                    return String.Empty;

                WizardDBTInfo masterDBT = documentInfo.DBTMaster;

                return (masterDBT != null) ? masterDBT.Name : String.Empty;
            }

            if (String.Compare(aToken, MasterDBTClassNameToken) == 0)
            {
                if (documentInfo == null)
                    return String.Empty;

                WizardDBTInfo masterDBT = documentInfo.DBTMaster;

                return (masterDBT != null) ? masterDBT.ClassName : String.Empty;
            }

            if (String.Compare(aToken, MasterTableNameToken) == 0)
            {
                if (documentInfo == null)
                    return String.Empty;

                WizardDBTInfo masterDBT = documentInfo.DBTMaster;

                return (masterDBT != null) ? masterDBT.TableName : String.Empty;
            }

            if (String.Compare(aToken, MasterTableClassNameToken) == 0)
            {
                if (documentInfo == null)
                    return String.Empty;

                WizardDBTInfo masterDBT = documentInfo.DBTMaster;
                if (masterDBT == null)
                    return String.Empty;

                WizardTableInfo tableInfo = masterDBT.GetTableInfo();

                return (tableInfo != null) ? tableInfo.ClassName : String.Empty;
            }

            if (aToken.StartsWith(ActOnMasterToken))
            {
                if (documentInfo == null || !documentInfo.IsDBTMasterDefined)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, ActOnMasterToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    return SubstituteDBTTokens(subToken, documentInfo.DBTMaster);
                }
            }

            if (aToken.StartsWith(IfDBTSlavesPresentToken))
            {
                if (documentInfo == null)
                    return String.Empty;

                WizardDBTInfoCollection slaves = documentInfo.DBTsSlaves;
                if (slaves == null || slaves.Count == 0)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfDBTSlavesPresentToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

			if (aToken.StartsWith(RepeatOnLabelsToken))
            {
				if (documentInfo == null)
					return string.Empty;

				string subToken;
				string columnsLinesSeparator;
				string columnsLines = String.Empty;

				if (ResolveFunctionToken(aToken, RepeatOnLabelsToken, out subToken, out columnsLinesSeparator))
				{
					if (subToken == null || subToken.Length == 0)
						return String.Empty;

					foreach (LabelInfo labelInfo in documentInfo.LabelInfoCollection)
					{
						if (labelInfo == null)
							continue;

						string columnParsedText = SubstituteLabelsTokens(subToken, labelInfo);
						columnsLines += columnParsedText;
					}
				}
				return columnsLines;
			}

            if (aToken.StartsWith(RepeatOnDBTsToken))
            {
                if (documentInfo == null || documentInfo.DBTsCount == 0)
                    return String.Empty;

                string subToken;
                string dbtsLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnDBTsToken, out subToken, out dbtsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string dbtsLines = String.Empty;
                    foreach (WizardDBTInfo aDBTInfo in documentInfo.DBTsInfo)
                    {
                        string dbtParsedText = SubstituteDBTTokens(subToken, aDBTInfo);

                        if (dbtParsedText != null && dbtParsedText.Length > 0)
                        {
                            if (dbtsLines.Length > 0)
                                dbtsLines += dbtsLinesSeparator;
                            dbtsLines += dbtParsedText;
                        }
                    }
                    return dbtsLines;
                }
            }

            if (aToken.StartsWith(RepeatOnDBTSlavesToken))
            {
                if (documentInfo == null)
                    return String.Empty;

                WizardDBTInfoCollection slaves = documentInfo.DBTsSlaves;
                if (slaves == null || slaves.Count == 0)
                    return String.Empty;

                string subToken;
                string slavesLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnDBTSlavesToken, out subToken, out slavesLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string slavesLines = String.Empty;
                    foreach (WizardDBTInfo aSlaveInfo in slaves)
                    {
                        string slaveParsedText = SubstituteDBTTokens(subToken, aSlaveInfo);

                        if (slaveParsedText != null && slaveParsedText.Length > 0)
                        {
                            if (slavesLines.Length > 0)
                                slavesLines += slavesLinesSeparator;
                            slavesLines += slaveParsedText;
                        }
                    }
                    return slavesLines;
                }
            }

            if (aToken.StartsWith(RepeatOnTabbedPanesToken))
            {
				if (documentInfo == null || documentInfo.TabbedPanesCount == 0)
					return String.Empty;

				string subToken;
				string dbtsLinesSeparator;

				if (ResolveFunctionToken(aToken, RepeatOnTabbedPanesToken, out subToken, out dbtsLinesSeparator))
				{
					if (subToken == null || subToken.Length == 0)
						return String.Empty;

					string tabbedPaneLines = String.Empty;
					foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in documentInfo.TabbedPanes)
					{
						string tabbedPaneParsedText = SubstituteTabbedPaneTokens(subToken, aTabbedPaneInfo);

						if (tabbedPaneParsedText != null && tabbedPaneParsedText.Length > 0)
						{
							if (tabbedPaneLines.Length > 0)
								tabbedPaneLines += dbtsLinesSeparator;
							tabbedPaneLines += tabbedPaneParsedText;
						}
					}
					return tabbedPaneLines;
				}
            }

            if (String.Compare(aToken, CultureLanguageIdentifierToken) == 0)
                return (documentInfo != null && documentInfo.Library != null && documentInfo.Library.Module != null && documentInfo.Library.Module.Application != null) ? documentInfo.Library.Module.Application.GetCultureLanguageIdentifierText() : String.Empty;

            if (String.Compare(aToken, CultureSubLanguageIdentifierToken) == 0)
                return (documentInfo != null && documentInfo.Library != null && documentInfo.Library.Module != null && documentInfo.Library.Module.Application != null) ? documentInfo.Library.Module.Application.GetCultureSubLanguageIdentifierText() : String.Empty;

            if (String.Compare(aToken, DocumentDialogWidthToken) == 0)
				return (dialogSize != Size.Empty) ? dialogSize.Width.ToString() : String.Empty;

            if (String.Compare(aToken, DocumentDialogHeightToken) == 0)
                return (dialogSize != Size.Empty) ? dialogSize.Height.ToString() : String.Empty;

            if (aToken.StartsWith(IsTabberToCreateToken))
            {
                if (!IsTabberToCreate())
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IsTabberToCreateToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (String.Compare(aToken, TabberControlIdToken) == 0)
            {
                if (!IsTabberToCreate())
                    return String.Empty;

                return documentInfo.GetTabberControlId().ToString();
            }

            if (String.Compare(aToken, TabberLeftToken) == 0)
            {
                if (!IsTabberToCreate())
                    return String.Empty;
				if (documentInfo.TabberSize.isSet) return documentInfo.TabberSize.Left.ToString();
				if (documentInfo != null) documentInfo.TabberSize.Left = WizardDBTCodeTemplateParser.FirstColumnControlLeftCoordinate;
                return WizardDBTCodeTemplateParser.FirstColumnControlLeftCoordinate.ToString();
            }

            if (String.Compare(aToken, TabberTopToken) == 0)
            {
                if (!IsTabberToCreate())
                    return String.Empty;
				if (documentInfo.TabberSize.isSet) return documentInfo.TabberSize.Top.ToString();
				if (documentInfo != null) documentInfo.TabberSize.Top = GetTabberTopCoordinate(documentInfo);
                return GetTabberTopCoordinate(documentInfo).ToString();
            }

            if (String.Compare(aToken, TabberWidthToken) == 0)
            {
                if (!IsTabberToCreate())
                    return String.Empty;
				if (documentInfo.TabberSize.isSet) return documentInfo.TabberSize.Width.ToString();
                System.Drawing.Font fontToUse = (documentInfo.Library != null && documentInfo.Library.Application != null) ? documentInfo.Library.Application.Font : WizardApplicationInfo.DefaultFont;
				if (documentInfo != null) documentInfo.TabberSize.Width = GetTabberAreaSize(documentInfo, fontToUse).Width;
                return GetTabberAreaSize(documentInfo, fontToUse).Width.ToString();
            }

            if (String.Compare(aToken, TabberHeightToken) == 0)
            {
                if (!IsTabberToCreate())
                    return String.Empty;
				if (documentInfo.TabberSize.isSet) return documentInfo.TabberSize.Height.ToString();
                System.Drawing.Font fontToUse = (documentInfo.Library != null && documentInfo.Library.Application != null) ? documentInfo.Library.Application.Font : WizardApplicationInfo.DefaultFont;

				if (documentInfo != null) documentInfo.TabberSize.Height = GetTabberAreaSize(documentInfo, fontToUse).Height;
                return GetTabberAreaSize(documentInfo, fontToUse).Height.ToString();
            }

            if (String.Compare(aToken, FontSizeToken) == 0)
            {
                if (documentInfo == null)
                    return String.Empty;

                System.Drawing.Font fontToUse = (documentInfo.Library != null && documentInfo.Library.Application != null) ? documentInfo.Library.Application.Font : WizardApplicationInfo.DefaultFont;
                return fontToUse.SizeInPoints.ToString(NumberFormatInfo.InvariantInfo);
            }

            if (String.Compare(aToken, FontNameToken) == 0)
            {
                if (documentInfo == null)
                    return String.Empty;

                System.Drawing.Font fontToUse = (documentInfo.Library != null && documentInfo.Library.Application != null) ? documentInfo.Library.Application.Font : WizardApplicationInfo.DefaultFont;
                return fontToUse.Name;
            }

            if (String.Compare(aToken, FontWeightToken) == 0)
            {
                if (documentInfo == null)
                    return String.Empty;

                System.Drawing.Font fontToUse = (documentInfo.Library != null && documentInfo.Library.Application != null) ? documentInfo.Library.Application.Font : WizardApplicationInfo.DefaultFont;
                return Generics.GetFontWeight(fontToUse).ToString();
            }

            if (String.Compare(aToken, FontIsItalicToken) == 0)
            {
                if (documentInfo == null)
                    return String.Empty;

                System.Drawing.Font fontToUse = (documentInfo.Library != null && documentInfo.Library.Application != null) ? documentInfo.Library.Application.Font : WizardApplicationInfo.DefaultFont;
                return fontToUse.Italic ? "1" : "0";
            }

            if (String.Compare(aToken, FontCharSetToken) == 0)
            {
                if (documentInfo == null)
                    return String.Empty;

                System.Drawing.Font fontToUse = (documentInfo.Library != null && documentInfo.Library.Application != null) ? documentInfo.Library.Application.Font : WizardApplicationInfo.DefaultFont;
                return Generics.GetFontCharSet(fontToUse).ToString();
            }

            if (aToken.StartsWith(IfHKLDefinedToken))
            {
                if (documentInfo == null || !documentInfo.IsHKLDefined)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfHKLDefinedToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (String.Compare(aToken, DocumentHKLNameToken) == 0)
                return (documentInfo != null && documentInfo.IsHKLDefined) ? documentInfo.HKLName : String.Empty;

            if (String.Compare(aToken, DocumentHKLClassNameToken) == 0)
                return (documentInfo != null && documentInfo.IsHKLDefined) ? documentInfo.HKLClassName : String.Empty;

            if (String.Compare(aToken, HKLCodeColumnNameToken) == 0)
                return (documentInfo != null && documentInfo.HKLCodeColumn != null) ? documentInfo.HKLCodeColumn.Name : String.Empty;

            if (String.Compare(aToken, HKLDescriptionColumnNameToken) == 0)
                return (documentInfo != null && documentInfo.HKLDescriptionColumn != null) ? documentInfo.HKLDescriptionColumn.Name : String.Empty;

            if (String.Compare(aToken, HKLCodeColumnDataObjClassNameToken) == 0)
                return (documentInfo != null && documentInfo.HKLCodeColumn != null) ? documentInfo.HKLCodeColumn.GetDataObjClassName() : String.Empty;

            if (aToken.StartsWith(IfHKLCodeIsTextualToken))
            {
                if (documentInfo == null || !documentInfo.IsHKLDefined || documentInfo.HKLCodeColumn == null || !documentInfo.HKLCodeColumn.DataType.IsTextual)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfHKLCodeIsTextualToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfHKLCodeIsNotTextualToken))
            {
                if (documentInfo == null || !documentInfo.IsHKLDefined || documentInfo.HKLCodeColumn == null || documentInfo.HKLCodeColumn.DataType.IsTextual)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfHKLCodeIsNotTextualToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfBatchDocumentToken))
            {
                if (documentInfo == null || !documentInfo.DefaultViewIsBatch)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfBatchDocumentToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfNotBatchDocumentToken))
            {
                if (documentInfo == null || documentInfo.DefaultViewIsBatch)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfNotBatchDocumentToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            return null;
        }

		//----------------------------------------------------------------------------
		private string SubstituteLabelsTokens(string source, LabelInfo labelInfo)
		{
			string labelParsedText = this.SubstituteTokens(source, true);
			if (AreTokensToSubstitute(labelParsedText))
			{
				WizardLabelTemplateParser labelParser = null;
				labelParser = new WizardLabelTemplateParser(labelInfo);
				labelParsedText = labelParser.SubstituteTokens(labelParsedText, true);
			}
			return labelParsedText;
		}

		//----------------------------------------------------------------------------
		private string SubstituteDBTTokens
			(
			string source,
			WizardDBTInfo aDBTInfo,
			bool preserveUnresolvedTokens
			)
		{
			if (source == null || aDBTInfo == null)
				return null;

			source = source.Trim();
			if (source.Length == 0)
				return String.Empty;

			string dbtParsedText = this.SubstituteTokens(source, true);

			if (AreTokensToSubstitute(dbtParsedText))
			{
				WizardDBTCodeTemplateParser dbtParser = new WizardDBTCodeTemplateParser(aDBTInfo, documentInfo);

				dbtParsedText = dbtParser.SubstituteTokens(dbtParsedText, true);
			}

			return dbtParsedText;
		}

		//----------------------------------------------------------------------------
        private string SubstituteDBTTokens(string source, WizardDBTInfo aDBTInfo)
        {
            return SubstituteDBTTokens(source, aDBTInfo, false);
        }

        //----------------------------------------------------------------------------
        private string SubstituteTabbedPaneTokens
            (
            string source,
            WizardDocumentTabbedPaneInfo aTabbedPaneInfo,
            bool preserveUnresolvedTokens
            )
        {
            if (source == null || aTabbedPaneInfo == null)
                return null;

            source = source.Trim();
            if (source.Length == 0)
                return String.Empty;

            string tabbedPaneParsedText = this.SubstituteTokens(source, true);

            if (AreTokensToSubstitute(tabbedPaneParsedText))
            {
                WizardTabbedPaneCodeTemplateParser tabbedPaneParser = new WizardTabbedPaneCodeTemplateParser(aTabbedPaneInfo, documentInfo);

                tabbedPaneParsedText = tabbedPaneParser.SubstituteTokens(tabbedPaneParsedText, true);
            }

            return tabbedPaneParsedText;
        }

        //----------------------------------------------------------------------------
        private string SubstituteTabbedPaneTokens(string source, WizardDocumentTabbedPaneInfo aTabbedPaneInfo)
        {
            return SubstituteTabbedPaneTokens(source, aTabbedPaneInfo, false);
        }

        //----------------------------------------------------------------------------
        internal static System.Drawing.Size GetDialogSize(WizardDocumentInfo aDocumentInfo, System.Drawing.Font aFont)
        {
            if (aDocumentInfo == null)
                return Size.Empty;

            if (aDocumentInfo.DefaultViewIsBatch)
                return GetBatchDocumentDialogSize();

            if (aDocumentInfo.DBTsCount == 0)
                return Size.Empty;

			if (aDocumentInfo.Width > 0 && aDocumentInfo.Height > 0)
				return new Size(aDocumentInfo.Width, aDocumentInfo.Height);

            int documentDialogWidth = 0;
            int documentDialogHeight = 0;
            WizardDBTInfo master = aDocumentInfo.DBTMaster;
            if (master != null)
            {
                System.Drawing.Size masterDialogSize = WizardDBTCodeTemplateParser.GetDialogSize(master, aFont);
                if (masterDialogSize != Size.Empty)
                {
                    documentDialogWidth = masterDialogSize.Width;
                    documentDialogHeight = masterDialogSize.Height;
                }
            }

            Size tabberAreaSize = GetTabberAreaSize(aDocumentInfo, aFont);

            int documentTmpWidth = tabberAreaSize.Width + (2 * WizardDBTCodeTemplateParser.FirstColumnControlLeftCoordinate);

            if (documentDialogWidth < documentTmpWidth)
                documentDialogWidth = documentTmpWidth;

            if (tabberAreaSize.Height > 0)
            {
                documentDialogHeight += WizardDBTCodeTemplateParser.FirstColumnControlTopCoordinate;
                documentDialogHeight += tabberAreaSize.Height;
                documentDialogHeight += WizardDBTCodeTemplateParser.FirstColumnControlTopCoordinate;
            }

			aDocumentInfo.Width = documentDialogWidth;
			aDocumentInfo.Height = documentDialogHeight;

			return new Size(documentDialogWidth, documentDialogHeight);
        }

        //----------------------------------------------------------------------------
        internal static bool IsTabberToCreate(WizardDocumentInfo aDocumentInfo)
        {
            return (aDocumentInfo != null && aDocumentInfo.IsTabberToCreate());
        }

        //----------------------------------------------------------------------------
        internal bool IsTabberToCreate()
        {
            return (documentInfo != null && documentInfo.IsTabberToCreate());
        }

        //----------------------------------------------------------------------------
        internal static System.Drawing.Size GetTabberAreaSize(WizardDocumentInfo aDocumentInfo, System.Drawing.Font aFont)
        {
            if (!IsTabberToCreate(aDocumentInfo))
                return Size.Empty;

            int masterAreaWidth = 0;
            WizardDBTInfo master = aDocumentInfo.DBTMaster;
            if (master != null)
                masterAreaWidth = WizardDBTCodeTemplateParser.GetDialogSize(master, aFont).Width;

            WizardDocumentTabbedPaneInfoCollection tabbedPanes = aDocumentInfo.TabbedPanes;
            if (tabbedPanes == null || tabbedPanes.Count == 0)
                return Size.Empty;

            int tabberAreaWidth = 0;
            int tabberAreaHeight = 0;

            int maxTabbedPaneWidth = 0;
            int maxTabbedPaneHeight = 0;

            foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in tabbedPanes)
            {
                System.Drawing.Size tabbedPaneSize = WizardTabbedPaneCodeTemplateParser.GetTabbedPaneSize(aTabbedPaneInfo, aFont);
                if (tabbedPaneSize == Size.Empty)
                    continue;

                if (maxTabbedPaneWidth < tabbedPaneSize.Width)
                    maxTabbedPaneWidth = tabbedPaneSize.Width;

                if (maxTabbedPaneHeight < tabbedPaneSize.Height)
                    maxTabbedPaneHeight = tabbedPaneSize.Height;
            }

            if (maxTabbedPaneWidth > 0)
                tabberAreaWidth = maxTabbedPaneWidth + (2 * WizardTabbedPaneCodeTemplateParser.FirstColumnControlLeftCoordinate);
            else
                tabberAreaWidth = WizardTabbedPaneCodeTemplateParser.DefaultTabberAreaWidth;

            if (maxTabbedPaneHeight > 0)
                tabberAreaHeight = maxTabbedPaneHeight + (2 * WizardTabbedPaneCodeTemplateParser.FirstColumnControlTopCoordinate);
            else
                tabberAreaHeight = WizardTabbedPaneCodeTemplateParser.DefaultTabberAreaHeight;

            if (tabberAreaWidth < masterAreaWidth)
                tabberAreaWidth = masterAreaWidth;

            return new Size(tabberAreaWidth, tabberAreaHeight);
        }

        //----------------------------------------------------------------------------
        internal static int GetTabberTopCoordinate(WizardDocumentInfo aDocumentInfo)
        {
            if (!IsTabberToCreate(aDocumentInfo))
                return -1;

            int tabberTopCoordinate = WizardDBTCodeTemplateParser.FirstColumnControlTopCoordinate;
            WizardDBTInfo master = aDocumentInfo.DBTMaster;
            if (master != null)
            {
                System.Drawing.Font fontToUse = (aDocumentInfo != null && aDocumentInfo.Library != null && aDocumentInfo.Library.Application != null) ? aDocumentInfo.Library.Application.Font : WizardApplicationInfo.DefaultFont;
                tabberTopCoordinate += WizardDBTCodeTemplateParser.GetDialogSize(master, fontToUse).Height;
            }
            return tabberTopCoordinate;
        }

        //----------------------------------------------------------------------------
        internal static System.Drawing.Size GetBatchDocumentDialogSize()
        {
            return new Size(156, 100);
        }

        //----------------------------------------------------------------------------
        private void InitDialogSize()
        {
            System.Drawing.Font fontToUse = (documentInfo != null && documentInfo.Library != null && documentInfo.Library.Application != null) ? documentInfo.Library.Application.Font : WizardApplicationInfo.DefaultFont;
            dialogSize = GetDialogSize(documentInfo, fontToUse);
        }
    }

    #endregion

    #region IWizardTableCodeTemplateParser interface

    internal interface IWizardTableCodeTemplateParser
    {
        //----------------------------------------------------------------------------
        string GetTokenValue(string aToken);

        //----------------------------------------------------------------------------
        string SubstituteTableColumnTokens
            (
            string source,
            WizardTableColumnInfo aColumnInfo,
            WizardTableColumnInfo aPreviousVersionColumnInfo,
            bool preserveUnresolvedTokens
            );

        //----------------------------------------------------------------------------
        string SubstituteTableColumnTokens
            (
            string source,
            WizardTableColumnInfo aColumnInfo,
            bool preserveUnresolvedTokens
            );
        //----------------------------------------------------------------------------
        string SubstituteTableColumnTokens
            (
            string source,
            WizardTableColumnInfo aColumnInfo,
            WizardTableColumnInfo aPreviousVersionColumnInfo
            );

        //----------------------------------------------------------------------------
        string SubstituteTableColumnTokens
            (
            string source,
            WizardTableColumnInfo aColumnInfo
            );

        //----------------------------------------------------------------------------
        string SubstituteForeignKeyTokens
            (
            string source,
            WizardForeignKeyInfo aForeignKeyInfo
            );
    }

    #endregion // IWizardTableCodeTemplateParser interface

    #region WizardTableCodeTemplateParser class

    //============================================================================
    internal class WizardTableCodeTemplateParser : CodeTemplateParser, IWizardTableCodeTemplateParser
    {
        private const string ApplicationNameToken = "ApplicationName";
        private const string ModuleNameToken = "ModuleName";
        private const string LibraryNameToken = "LibraryName";
        private const string LibrarySourceFolderToken = "LibrarySourceFolder";
        private const string TableNameToken = "TableName";
        private const string TableClassNameToken = "TableClassName";
        private const string DefinedColumnsCountToken = "DefinedColumnsCount";
        private const string ColumnsCountToken = "ColumnsCount";
        private const string RepeatOnDefinedColumnsToken = "RepeatOnDefinedColumns";
        private const string RepeatOnColumnsToken = "RepeatOnColumns";
        private const string IfPrimaryKeyDefinedToken = "IfPrimaryKeyDefined";
        private const string RepeatOnPrimaryKeySegmentsToken = "RepeatOnPrimaryKeySegments";
        private const string TablePrimaryKeyConstraintNameToken = "TablePrimaryKeyConstraintName";
		private const string IfConstraintDefinedToken = "IfConstraintDefined";

        private const string IfTRDefinedToken = "IfTRDefined";
        private const string TableTRClassNameToken = "TableTRClassName";

        private const string IfHKLDefinedToken = "IfHKLDefined";
        private const string TableHKLNameToken = "TableHKLName";
        private const string TableHKLClassNameToken = "TableHKLClassName";
        private const string HKLCodeColumnNameToken = "HKLCodeColumnName";
        private const string HKLCodeLikeToken = "HKLCodeLike";
        private const string HKLDescriptionColumnNameToken = "HKLDescriptionColumnName";
        private const string HKLDescriptionLikeToken = "HKLDescriptionLike";
        private const string RepeatOnHKLColumnsToken = "RepeatOnHKLColumns";
        private const string HKLCodeColumnDataObjClassNameToken = "HKLCodeColumnDataObjClassName";
        private const string IfHKLCodeIsTextualToken = "IfHKLCodeIsTextual";
        private const string IfHKLCodeIsNotTextualToken = "IfHKLCodeIsNotTextual";

        private const string IfContainsUpperCaseStringColumnsToken = "IfContainsUpperCaseStringColumns";
        private const string RepeatOnUpperCaseStringColumnsToken = "RepeatOnUpperCaseStringColumns";

        private const string IfContainsDataEnumsToken = "IfContainsDataEnums";
        private const string RepeatOnDataEnumColumnsToken = "RepeatOnDataEnumColumns";
        private const string RepeatOnReferencedEnumsToken = "RepeatOnReferencedEnums";
        private const string RepeatOnUsedEnumsModulesToken = "RepeatOnUsedEnumsModules";
		private const string IfPKNonClusteredToken = "IfPKNonClustered";
		
        private const string IfIsTBGuidColumnToAddToken = "IfIsTBGuidColumnToAdd";

        private const string IfIsNotReferencedToken = "IfIsNotReferenced";
        private const string IfIsReferencedToken = "IfIsReferenced";

		private const string IfIndexDefined = "IfIndexDefined";
		private const string RepeatOnIndex = "RepeatOnIndex";

        private const string IfForeignKeysDefinedToken = "IfForeignKeysDefined";
        private const string RepeatOnForeignKeysToken = "RepeatOnForeignKeys";

        private const string WoormColumnDefaultWidthToken = "WoormColumnDefaultWidth";

        protected WizardTableInfo tableInfo = null;
		private DBMSType dbType = DBMSType.UNKNOWN;

        private const int WoormTableDefaultTotalWidth = 760;

        //----------------------------------------------------------------------------
		internal WizardTableCodeTemplateParser(WizardTableInfo aTableInfo, DBMSType aDBType)
        {
            tableInfo = aTableInfo;
            dbType = aDBType;
        }

        //----------------------------------------------------------------------------
        internal WizardTableCodeTemplateParser(WizardTableInfo aTableInfo)
			: this(aTableInfo, DBMSType.UNKNOWN)
        {
        }

        //----------------------------------------------------------------------------
        internal override string GetTokenValue(string aToken)
        {
            if (aToken == null)
                return null;

            aToken = aToken.Trim();
            if (aToken.Length == 0)
                return String.Empty;

            if (String.Compare(aToken, ApplicationNameToken) == 0)
                return (tableInfo != null && tableInfo.Library != null && tableInfo.Library.Application != null) ? tableInfo.Library.Application.Name : String.Empty;

            if (String.Compare(aToken, ModuleNameToken) == 0)
                return (tableInfo != null && tableInfo.Library != null && tableInfo.Library.Module != null) ? tableInfo.Library.Module.Name : String.Empty;

            if (String.Compare(aToken, LibraryNameToken) == 0)
                return (tableInfo != null && tableInfo.Library != null) ? tableInfo.Library.Name : String.Empty;

            if (String.Compare(aToken, LibrarySourceFolderToken) == 0)
                return (tableInfo != null && tableInfo.Library != null) ? tableInfo.Library.SourceFolder : String.Empty;

            if (String.Compare(aToken, TableNameToken) == 0)
                return (tableInfo != null) ? tableInfo.Name : String.Empty;

            if (String.Compare(aToken, TableClassNameToken) == 0)
                return (tableInfo != null) ? tableInfo.ClassName : String.Empty;

            if (String.Compare(aToken, DefinedColumnsCountToken) == 0)
                return (tableInfo != null && tableInfo.ColumnsInfo != null) ? tableInfo.ColumnsInfo.Count.ToString() : String.Empty;

            if (String.Compare(aToken, ColumnsCountToken) == 0)
                return (tableInfo != null) ? tableInfo.ColumnsCount.ToString() : String.Empty;

            if (aToken.StartsWith(RepeatOnDefinedColumnsToken))
            {
                if (tableInfo == null || tableInfo.ColumnsInfo == null || tableInfo.ColumnsInfo.Count == 0)
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnDefinedColumnsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string columnsLines = String.Empty;
                    foreach (WizardTableColumnInfo aColumnInfo in tableInfo.ColumnsInfo)
                    {
                        string columnParsedText = ((IWizardTableCodeTemplateParser)this).SubstituteTableColumnTokens(subToken, aColumnInfo);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }
                    }
                    return columnsLines;
                }
            }

            if (aToken.StartsWith(RepeatOnColumnsToken))
            {
                if (tableInfo == null || tableInfo.ColumnsCount == 0)
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnColumnsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string columnsLines = String.Empty;
                    if (tableInfo.ColumnsInfo != null && tableInfo.ColumnsInfo.Count > 0)
                    {
                        foreach (WizardTableColumnInfo aColumnInfo in tableInfo.ColumnsInfo)
                        {
                            string columnParsedText = ((IWizardTableCodeTemplateParser)this).SubstituteTableColumnTokens(subToken, aColumnInfo);

                            if (columnParsedText != null && columnParsedText.Length > 0)
                            {
                                if (columnsLines.Length > 0)
                                    columnsLines += columnsLinesSeparator;
                                columnsLines += columnParsedText;
                            }
                        }
                    }

                    if (tableInfo.AddTBGuidColumn)
                    {
                        string guidColumnParsedText = ((IWizardTableCodeTemplateParser)this).SubstituteTableColumnTokens(subToken, tableInfo.GetTBGuidColumnInfo());
                        if (guidColumnParsedText != null && guidColumnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += guidColumnParsedText;
                        }
                    }

                    return columnsLines;
                }
            }

            if (aToken.StartsWith(IfPrimaryKeyDefinedToken))
            {
                if (tableInfo == null || !tableInfo.IsPrimaryKeyDefined)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfPrimaryKeyDefinedToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(RepeatOnPrimaryKeySegmentsToken))
            {
                if (tableInfo == null || !tableInfo.IsPrimaryKeyDefined)
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnPrimaryKeySegmentsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == String.Empty || subToken.Length == 0)
                        return String.Empty;

                    string columnsLines = String.Empty;
                    foreach (WizardTableColumnInfo aColumnInfo in tableInfo.ColumnsInfo)
                    {
                        if (!aColumnInfo.IsPrimaryKeySegment)
                            continue;

                        string columnParsedText = ((IWizardTableCodeTemplateParser)this).SubstituteTableColumnTokens(subToken, aColumnInfo);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }
                    }
                    return columnsLines;
                }
            }

			if (aToken.StartsWith(IfPKNonClusteredToken))
			{
				if (tableInfo == null || string.IsNullOrEmpty(tableInfo.PrimaryKeyConstraintName) || tableInfo.PrimaryKeyClustered)
					return string.Empty;
				string subToken;
				if (ResolveFunctionToken(aToken, IfPKNonClusteredToken, out subToken))
				{
					if (subToken == null || subToken.Length == 0)
						return String.Empty;

					return this.SubstituteTokens(subToken, true);
				}

			}

            if (String.Compare(aToken, TablePrimaryKeyConstraintNameToken) == 0)
                return (tableInfo != null) ? tableInfo.PrimaryKeyConstraintName : String.Empty;

			//Serve a gestire il corretto posizionamento della virgola nell'ultima colonna definita nello script
			if (aToken.StartsWith(IfConstraintDefinedToken))
			{
				if (tableInfo == null || (tableInfo.ForeignKeysCount == 0 && string.IsNullOrEmpty(tableInfo.PrimaryKeyConstraintName)))
					return string.Empty;
				string subToken;
				if (ResolveFunctionToken(aToken, IfConstraintDefinedToken, out subToken))
				{
					if (subToken == null || subToken.Length == 0)
						return String.Empty;

					return this.SubstituteTokens(subToken, true);
				}

			}

            //---------- TableReader
            if (aToken.StartsWith(IfTRDefinedToken))
            {
                if (tableInfo == null || !tableInfo.IsTRDefined)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfTRDefinedToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (String.Compare(aToken, TableTRClassNameToken) == 0)
                return (tableInfo != null && tableInfo.IsTRDefined) ? tableInfo.TRClassName : String.Empty;
            //---------- TableReader

            if (aToken.StartsWith(IfHKLDefinedToken))
            {
                if (tableInfo == null || !tableInfo.IsHKLDefined)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfHKLDefinedToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (String.Compare(aToken, TableHKLNameToken) == 0)
                return (tableInfo != null && tableInfo.IsHKLDefined) ? tableInfo.HKLName : String.Empty;

            if (String.Compare(aToken, TableHKLClassNameToken) == 0)
                return (tableInfo != null && tableInfo.IsHKLDefined) ? tableInfo.HKLClassName : String.Empty;

            if (String.Compare(aToken, HKLCodeColumnNameToken) == 0)
                return (tableInfo != null && tableInfo.HKLCodeColumn != null) ? tableInfo.HKLCodeColumn.Name : String.Empty;

            if (String.Compare(aToken, HKLCodeLikeToken) == 0)
                return (tableInfo != null && tableInfo.IsHKLCodeColumnTextual) ? "LIKE" : ">=";

            if (String.Compare(aToken, HKLDescriptionColumnNameToken) == 0)
                return (tableInfo != null && tableInfo.HKLDescriptionColumn != null) ? tableInfo.HKLDescriptionColumn.Name : String.Empty;

            if (String.Compare(aToken, HKLDescriptionLikeToken) == 0)
                return (tableInfo != null && tableInfo.IsHKLDescriptionColumnTextual) ? "LIKE" : ">=";

            if (aToken.StartsWith(RepeatOnHKLColumnsToken))
            {
                if (tableInfo == null || !tableInfo.IsHKLDefined)
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnHKLColumnsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string columnsLines = String.Empty;
                    if (tableInfo.HKLCodeColumn != null)
                    {
                        string columnParsedText = ((IWizardTableCodeTemplateParser)this).SubstituteTableColumnTokens(subToken, tableInfo.HKLCodeColumn);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                            columnsLines += columnParsedText;
                    }
                    if (tableInfo.HKLDescriptionColumn != null)
                    {
                        string columnParsedText = ((IWizardTableCodeTemplateParser)this).SubstituteTableColumnTokens(subToken, tableInfo.HKLDescriptionColumn);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }
                    }
                    return columnsLines;
                }
            }

            if (String.Compare(aToken, HKLCodeColumnDataObjClassNameToken) == 0)
                return (tableInfo != null && tableInfo.HKLCodeColumn != null) ? tableInfo.HKLCodeColumn.GetDataObjClassName() : String.Empty;

            if (aToken.StartsWith(IfHKLCodeIsTextualToken))
            {
                if (tableInfo == null || !tableInfo.IsHKLDefined || tableInfo.HKLCodeColumn == null || !tableInfo.HKLCodeColumn.DataType.IsTextual)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfHKLCodeIsTextualToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfHKLCodeIsNotTextualToken))
            {
                if (tableInfo == null || !tableInfo.IsHKLDefined || tableInfo.HKLCodeColumn == null || tableInfo.HKLCodeColumn.DataType.IsTextual)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfHKLCodeIsNotTextualToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfContainsUpperCaseStringColumnsToken))
            {
                if (tableInfo == null || !tableInfo.ContainsUpperCaseStringColumns())
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfContainsUpperCaseStringColumnsToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(RepeatOnUpperCaseStringColumnsToken))
            {
                if (tableInfo == null || !tableInfo.ContainsUpperCaseStringColumns())
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnUpperCaseStringColumnsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string columnsLines = String.Empty;
                    foreach (WizardTableColumnInfo aColumnInfo in tableInfo.ColumnsInfo)
                    {
                        if (!aColumnInfo.IsUpperCaseDataString)
                            continue;

                        string columnParsedText = ((IWizardTableCodeTemplateParser)this).SubstituteTableColumnTokens(subToken, aColumnInfo, true);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }
                    }
                    return columnsLines;
                }
            }

            if (aToken.StartsWith(IfContainsDataEnumsToken))
            {
                if (tableInfo == null || !tableInfo.ContainsDataEnumColumns())
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfContainsDataEnumsToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(RepeatOnDataEnumColumnsToken))
            {
                if (tableInfo == null || !tableInfo.ContainsDataEnumColumns())
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnDataEnumColumnsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string columnsLines = String.Empty;
                    foreach (WizardTableColumnInfo aColumnInfo in tableInfo.ColumnsInfo)
                    {
                        if (aColumnInfo.EnumInfo == null)
                            continue;

                        string columnParsedText = ((IWizardTableCodeTemplateParser)this).SubstituteTableColumnTokens(subToken, aColumnInfo, true);
                        if (AreTokensToSubstitute(columnParsedText))
                        {
                            WizardEnumCodeTemplateParser enumParser = new WizardEnumCodeTemplateParser(aColumnInfo.EnumInfo);

                            columnParsedText = enumParser.SubstituteTokens(columnParsedText);
                        }

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }
                    }
                    return columnsLines;
                }
            }

            if (aToken.StartsWith(RepeatOnReferencedEnumsToken))
            {
                if (tableInfo == null || tableInfo.Library == null || tableInfo.Library.Application == null || !tableInfo.ContainsDataEnumColumns())
                    return String.Empty;

                string subToken;
                string enumsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnReferencedEnumsToken, out subToken, out enumsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    WizardEnumInfoCollection referencedEnums = new WizardEnumInfoCollection();

                    foreach (WizardTableColumnInfo aColumnInfo in tableInfo.ColumnsInfo)
                    {
                        if
                            (
                            aColumnInfo.EnumInfo == null ||
                            tableInfo.Library.Application.GetEnumInfoByName(aColumnInfo.EnumInfo.Name) != null ||
                            (referencedEnums.Count > 0 && referencedEnums.GetEnumInfoByName(aColumnInfo.EnumInfo.Name) != null)
                            )
                            continue;

                        referencedEnums.Add(aColumnInfo.EnumInfo);
                    }

                    if (referencedEnums.Count == 0)
                        return String.Empty;

                    string enumsLines = String.Empty;
                    foreach (WizardEnumInfo aEnumInfo in referencedEnums)
                    {
                        string enumParsedText = this.SubstituteTokens(subToken, true);

                        if (AreTokensToSubstitute(enumParsedText))
                        {
                            WizardEnumCodeTemplateParser enumParser = new WizardEnumCodeTemplateParser(aEnumInfo);

                            enumParsedText = enumParser.SubstituteTokens(enumParsedText);
                        }

                        if (enumParsedText != null && enumParsedText.Length > 0)
                        {
                            if (enumsLines.Length > 0)
                                enumsLines += enumsLinesSeparator;
                            enumsLines += enumParsedText;
                        }
                    }
                    return enumsLines;
                }
            }

            if (aToken.StartsWith(RepeatOnUsedEnumsModulesToken))
            {
                if (tableInfo == null || tableInfo.Library == null || tableInfo.Library.Application == null || !tableInfo.ContainsDataEnumColumns())
                    return String.Empty;

                string subToken;
                string modulesLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnUsedEnumsModulesToken, out subToken, out modulesLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    WizardModuleInfoCollection enumsModules = new WizardModuleInfoCollection();

                    foreach (WizardTableColumnInfo aColumnInfo in tableInfo.ColumnsInfo)
                    {
                        if (aColumnInfo.EnumInfo == null)
                            continue;

                        WizardModuleInfo enumModule = tableInfo.Library.Application.GetModuleContainingEnum(aColumnInfo.EnumInfo.Name);
                        if
                            (
                            enumModule == null ||
                            (enumsModules.Count > 0 && enumsModules.GetModuleInfoByName(enumModule.Name) != null)
                            )
                            continue;

                        enumsModules.Add(enumModule);
                    }

                    if (enumsModules.Count == 0)
                        return String.Empty;

                    string modulesLines = String.Empty;
                    foreach (WizardModuleInfo aModuleInfo in enumsModules)
                    {
                        WizardModuleCodeTemplateParser moduleParser = new WizardModuleCodeTemplateParser(aModuleInfo);

                        string moduleParsedText = moduleParser.SubstituteTokens(subToken, true);

                        if (AreTokensToSubstitute(moduleParsedText))
                            moduleParsedText = this.SubstituteTokens(moduleParsedText);

                        if (moduleParsedText != null && moduleParsedText.Length > 0)
                        {
                            if (modulesLines.Length > 0)
                                modulesLines += modulesLinesSeparator;
                            modulesLines += moduleParsedText;
                        }
                    }
                    return modulesLines;
                }
            }

            if (aToken.StartsWith(IfIsTBGuidColumnToAddToken))
            {
                if (tableInfo == null || !tableInfo.AddTBGuidColumn)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfIsTBGuidColumnToAddToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfIsNotReferencedToken))
            {
                if (tableInfo == null || tableInfo.IsReferenced)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfIsNotReferencedToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken, true);
                }
            }

            if (aToken.StartsWith(IfIsReferencedToken))
            {
                if (tableInfo == null || !tableInfo.IsReferenced)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfIsReferencedToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken, true);
                }
            }

            if (aToken.StartsWith(IfForeignKeysDefinedToken))
            {
                if (tableInfo == null || tableInfo.ForeignKeysCount == 0)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfForeignKeysDefinedToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken, true);
                }
            }

            if (aToken.StartsWith(RepeatOnForeignKeysToken))
            {
                if (tableInfo == null || tableInfo.ForeignKeysCount == 0)
                    return String.Empty;

                string subToken;
                string foreignKeysLinesSeparator;
                
				if (ResolveFunctionToken(aToken, RepeatOnForeignKeysToken, out subToken, out foreignKeysLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string foreignKeysLines = String.Empty;
					int fkCounter = 0;

					foreach (WizardForeignKeyInfo aForeignKeyInfo in tableInfo.ForeignKeys)
                    {
                        WizardForeignKeyCodeTemplateParser foreignKeyParser = new WizardForeignKeyCodeTemplateParser(aForeignKeyInfo);

						fkCounter++;
						
						// se è già definita una PK oppure si sta analizzando l'ultima FK NON si aggiunge l'ultima virgola
						foreignKeyParser.IsCommaNeeded = 
							(!string.IsNullOrEmpty(tableInfo.PrimaryKeyConstraintName)) ||
							(fkCounter == tableInfo.ForeignKeysCount);

                        string foreignKeyParsedText = foreignKeyParser.SubstituteTokens(subToken, true);

                        if (AreTokensToSubstitute(foreignKeyParsedText))
                            foreignKeyParsedText = this.SubstituteTokens(foreignKeyParsedText);

                        if (foreignKeyParsedText != null && foreignKeyParsedText.Length > 0)
                        {
                            if (foreignKeysLines.Length > 0)
                                foreignKeysLines += foreignKeysLinesSeparator;
                            foreignKeysLines += foreignKeyParsedText;
                        }
                    }
                    return foreignKeysLines;
                }
            }
			//token index
			if (aToken.StartsWith(IfIndexDefined))
			{
				if (tableInfo == null || tableInfo.Indexes == null || tableInfo.Indexes.Count == 0)
					return String.Empty;
				

				string subToken;
				if (ResolveFunctionToken(aToken, IfIndexDefined, out subToken))
				{
					if (subToken == null || subToken.Length == 0)
						return "\r\n";
					return this.SubstituteTokens(subToken, true);
				}
				
			}

			if (aToken.StartsWith(RepeatOnIndex))
			{
				if (tableInfo == null || tableInfo.Indexes == null || tableInfo.Indexes.Count == 0)
					return String.Empty;


				string subToken;
                string linesSeparator;
				if (ResolveFunctionToken(aToken, RepeatOnIndex, out subToken, out linesSeparator))
				{
					if (subToken == null || subToken.Length == 0)
						return String.Empty;

					string indexLines = String.Empty;
					foreach (WizardTableIndexInfo aIndexInfo in tableInfo.Indexes)
					{
						WizardIndexCodeTemplateParser indexParser = new WizardIndexCodeTemplateParser(aIndexInfo);

						string indexParsedText = indexParser.SubstituteTokens(subToken, true);

						if (AreTokensToSubstitute(indexParsedText))
							indexParsedText = this.SubstituteTokens(indexParsedText);

						if (indexParsedText != null && indexParsedText.Length > 0)
						{
							if (indexLines.Length > 0)
								indexLines += linesSeparator;
							indexLines += indexParsedText;
						}
					}
					return indexLines;
				}

			}
            if (String.Compare(aToken, WoormColumnDefaultWidthToken) == 0)
                return (tableInfo != null && tableInfo.ColumnsCount > 0) ? (WoormTableDefaultTotalWidth / tableInfo.ColumnsCount).ToString() : String.Empty;

            return null;
        }

        //----------------------------------------------------------------------------
        string IWizardTableCodeTemplateParser.GetTokenValue(string aToken)
        {
            return GetTokenValue(aToken);
        }

        //----------------------------------------------------------------------------
        string IWizardTableCodeTemplateParser.SubstituteTableColumnTokens
            (
            string source,
            WizardTableColumnInfo aColumnInfo,
            WizardTableColumnInfo aPreviousVersionColumnInfo,
            bool preserveUnresolvedTokens
            )
        {
            if (source == null || aColumnInfo == null)
                return null;

            source = source.Trim();
            if (source.Length == 0)
                return String.Empty;

            int columnIndex = -1;
            if (tableInfo != null && tableInfo.ColumnsInfo.Contains(aColumnInfo))
                columnIndex = tableInfo.ColumnsInfo.IndexOf(aColumnInfo);

            string columnParsedText = this.SubstituteTokens(source, true);

            if (AreTokensToSubstitute(columnParsedText))
            {
                WizardTableColumnCodeTemplateParser columnParser = new WizardTableColumnCodeTemplateParser(aColumnInfo, dbType, columnIndex, tableInfo);

                columnParsedText = columnParser.SubstituteTokens(columnParsedText, preserveUnresolvedTokens || aPreviousVersionColumnInfo != null);

                if (aPreviousVersionColumnInfo != null && AreTokensToSubstitute(columnParsedText))
                {
                    WizardPreviousVersionColumnCodeTemplateParser previousVersionColumnParser = new WizardPreviousVersionColumnCodeTemplateParser(aPreviousVersionColumnInfo, dbType, columnIndex);

                    columnParsedText = previousVersionColumnParser.SubstituteTokens(columnParsedText, true);
                }

                if (AreTokensToSubstitute(columnParsedText))
                    return this.SubstituteTokens(columnParsedText, preserveUnresolvedTokens);
            }

            return columnParsedText;
        }

        //----------------------------------------------------------------------------
        string IWizardTableCodeTemplateParser.SubstituteTableColumnTokens
            (
            string source,
            WizardTableColumnInfo aColumnInfo,
            bool preserveUnresolvedTokens
            )
        {
            return ((IWizardTableCodeTemplateParser)this).SubstituteTableColumnTokens(source, aColumnInfo, null, preserveUnresolvedTokens);
        }

        //----------------------------------------------------------------------------
        string IWizardTableCodeTemplateParser.SubstituteTableColumnTokens
            (
            string source,
            WizardTableColumnInfo aColumnInfo,
            WizardTableColumnInfo aPreviousVersionColumnInfo
            )
        {
            return ((IWizardTableCodeTemplateParser)this).SubstituteTableColumnTokens(source, aColumnInfo, aPreviousVersionColumnInfo, false);
        }

        //----------------------------------------------------------------------------
        string IWizardTableCodeTemplateParser.SubstituteTableColumnTokens(string source, WizardTableColumnInfo aColumnInfo)
        {
            return ((IWizardTableCodeTemplateParser)this).SubstituteTableColumnTokens(source, aColumnInfo, false);
        }

        //----------------------------------------------------------------------------
        string IWizardTableCodeTemplateParser.SubstituteForeignKeyTokens(string source, WizardForeignKeyInfo aForeignKeyInfo)
        {
            if (source == null || aForeignKeyInfo == null)
                return null;

            source = source.Trim();
            if (source.Length == 0)
                return String.Empty;

            string foreignKeyParsedText = this.SubstituteTokens(source, true);

            if (AreTokensToSubstitute(foreignKeyParsedText))
            {
                WizardForeignKeyCodeTemplateParser foreignKeyParser = new WizardForeignKeyCodeTemplateParser(aForeignKeyInfo);

                foreignKeyParsedText = foreignKeyParser.SubstituteTokens(foreignKeyParsedText);
            }

            return foreignKeyParsedText;
        }
    }

    #endregion

    #region WizardAdditionalColumnsCodeTemplateParser class

    //============================================================================
    internal class WizardAdditionalColumnsCodeTemplateParser : CodeTemplateParser, IWizardTableCodeTemplateParser
    {
        private const string ModuleNameToken = "ModuleName";
        private const string LibraryNameToken = "LibraryName";
        private const string LibrarySourceFolderToken = "LibrarySourceFolder";
        private const string TableNameToken = "TableName";
        private const string TableNameSpaceToken = "TableNameSpace";
        private const string TableClassNameToken = "TableClassName";
        private const string TableLibraryRelativePathToken = "TableLibraryRelativePath";
        private const string TableModuleNameToken = "TableModuleName";
        private const string TableLibrarySourceFolderToken = "TableLibrarySourceFolder";
        private const string AdditionalColumnsClassNameToken = "AdditionalColumnsClassName";
        private const string AdditionalColumnsModuleNameToken = "AdditionalColumnsModuleName";
        private const string AdditionalColumnsLibrarySourceFolderToken = "AdditionalColumnsLibrarySourceFolder";
        private const string ColumnsCountToken = "ColumnsCount";

		private const string IfUpdateColumnNeededToken = "IfUpdateColumnNeeded";
        private const string IfExistsAlterTableToken = "IfExistsAlterTable";
        private const string IfExistsUnrelatedUpdatesToken = "IfExistsUnrelatedUpdates";

		private const string RepeatOnColumnsToken = "RepeatOnColumns";
        private const string RepeatOnPrimaryKeySegmentsToken = "RepeatOnPrimaryKeySegments";
        private const string TablePrimaryKeyConstraintNameToken = "TablePrimaryKeyConstraintName";

        private const string IfContainsDataEnumsToken = "IfContainsDataEnums";
        private const string RepeatOnDataEnumColumnsToken = "RepeatOnDataEnumColumns";
        private const string RepeatOnReferencedEnumsToken = "RepeatOnReferencedEnums";
        private const string RepeatOnUsedEnumsModulesToken = "RepeatOnUsedEnumsModules";

        private const string DBTLibraryRelativePathToken = "DBTLibraryRelativePath";
        private const string DBTModuleNameToken = "DBTModuleName";
        private const string DBTLibrarySourceFolderToken = "DBTLibrarySourceFolder";
        private const string DocumentLibraryRelativePathToken = "DocumentLibraryRelativePath";
        private const string DocumentModuleNameToken = "DocumentModuleName";
        private const string DocumentLibrarySourceFolderToken = "DocumentLibrarySourceFolder";
        private const string ClientDocumentLibraryRelativePathToken = "ClientDocumentLibraryRelativePath";
        private const string ClientDocumentModuleNameToken = "ClientDocumentModuleName";
        private const string ClientDocumentLibrarySourceFolderToken = "ClientDocumentLibrarySourceFolder";

        private const string IfIsNotTableReferencedToken = "IfIsNotTableReferenced";
        private const string IfIsTableReferencedToken = "IfIsTableReferenced";
        private const string ReferencedTableHeaderRelativePathToken = "ReferencedTableHeaderRelativePath";

        private const string WoormColumnDefaultWidthToken = "WoormColumnDefaultWidth";
        private const string WhereTableNameToken = "WhereTableName";
        private const string WhereColumnNameToken = "WhereColumnName";
        private const string WhereValueToken = "WhereValue"; 
        private const string SetValueToken = "SetValue";
        private const string IfExistsWhereClauseToken = "IfExistsWhereClause";
        private const string UpdateTableNameToken = "UpdateTableName";
        private const string UpdateColumnNameToken = "UpdateColumnName";

        protected WizardExtraAddedColumnsInfo additionalColumnsInfo = null;
		private DBMSType dbType = DBMSType.UNKNOWN;
        private WizardDBTInfo dbtInfo = null;
        private WizardDocumentInfo documentInfo = null;
        private WizardClientDocumentInfo clientDocumentInfo = null;
        private const int WoormTableDefaultTotalWidth = 760;
        private TableUpdate currentColumnUpdate = null; 

        //----------------------------------------------------------------------------
        public TableUpdate CurrentColumnUpdate { get { return currentColumnUpdate; } set { currentColumnUpdate = value; } }

        //----------------------------------------------------------------------------
        internal WizardAdditionalColumnsCodeTemplateParser
            (
            WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo,
			DBMSType aDBType,
            WizardDBTInfo aDbtInfo,
            WizardDocumentInfo aDocumentInfo
            )
        {
            additionalColumnsInfo = aExtraAddedColumnsInfo;
            dbType = aDBType;
            dbtInfo = aDbtInfo;
            documentInfo = aDocumentInfo;
        }

        //----------------------------------------------------------------------------
        internal WizardAdditionalColumnsCodeTemplateParser
            (
            WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo,
			DBMSType aDBType,
            WizardDBTInfo aDbtInfo,
            WizardClientDocumentInfo aClientDocumentInfo
            )
        {
            additionalColumnsInfo = aExtraAddedColumnsInfo;
            dbType = aDBType;
            dbtInfo = aDbtInfo;
            clientDocumentInfo = aClientDocumentInfo;
        }
        
        //----------------------------------------------------------------------------
        internal WizardAdditionalColumnsCodeTemplateParser
            (
            WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo,
            DBMSType aDBType,
            WizardDBTInfo aDbtInfo
            )
            :
            this(aExtraAddedColumnsInfo, aDBType, aDbtInfo, (WizardDocumentInfo)null)
        {
        }

        //----------------------------------------------------------------------------
        internal WizardAdditionalColumnsCodeTemplateParser(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo, DBMSType aDBType)
            : this(aExtraAddedColumnsInfo, aDBType, null)
        {
        }

        //----------------------------------------------------------------------------
        internal WizardAdditionalColumnsCodeTemplateParser(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo, WizardDBTInfo aDbtInfo)
			: this(aExtraAddedColumnsInfo, DBMSType.UNKNOWN, aDbtInfo)
        {
        }

        //----------------------------------------------------------------------------
        internal WizardAdditionalColumnsCodeTemplateParser(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo, WizardDBTInfo aDbtInfo, WizardDocumentInfo aDocumentInfo)
			: this(aExtraAddedColumnsInfo, DBMSType.UNKNOWN, aDbtInfo, aDocumentInfo)
        {
        }

        //----------------------------------------------------------------------------
        internal WizardAdditionalColumnsCodeTemplateParser(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo, WizardDBTInfo aDbtInfo, WizardClientDocumentInfo aClientDocumentInfo)
			: this(aExtraAddedColumnsInfo, DBMSType.UNKNOWN, aDbtInfo, aClientDocumentInfo)
        {
        }

        //----------------------------------------------------------------------------
        internal WizardAdditionalColumnsCodeTemplateParser(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo, WizardDocumentInfo aDocumentInfo)
			: this(aExtraAddedColumnsInfo, DBMSType.UNKNOWN, null, aDocumentInfo)
        {
        }

        //----------------------------------------------------------------------------
        internal WizardAdditionalColumnsCodeTemplateParser(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo, WizardClientDocumentInfo aClientDocumentInfo)
			: this(aExtraAddedColumnsInfo, DBMSType.UNKNOWN, null, aClientDocumentInfo)
        {
        }

        //----------------------------------------------------------------------------
        internal WizardAdditionalColumnsCodeTemplateParser(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo)
			: this(aExtraAddedColumnsInfo, DBMSType.UNKNOWN)
        {
        }

        //----------------------------------------------------------------------------
        internal override string GetTokenValue(string aToken)
        {
            if (aToken == null)
                return null;

            aToken = aToken.Trim();
            if (aToken.Length == 0)
                return String.Empty;

            if (String.Compare(aToken, ModuleNameToken) == 0)
                return (additionalColumnsInfo != null && additionalColumnsInfo.Library != null && additionalColumnsInfo.Library.Module != null) ? additionalColumnsInfo.Library.Module.Name : String.Empty;

            if (String.Compare(aToken, LibraryNameToken) == 0)
                return (additionalColumnsInfo != null && additionalColumnsInfo.Library != null) ? additionalColumnsInfo.Library.Name : String.Empty;

            if (String.Compare(aToken, LibrarySourceFolderToken) == 0)
                return (additionalColumnsInfo != null && additionalColumnsInfo.Library != null) ? additionalColumnsInfo.Library.SourceFolder : String.Empty;

            if (String.Compare(aToken, TableNameToken) == 0)
                return (additionalColumnsInfo != null) ? additionalColumnsInfo.TableName : String.Empty;

            if (String.Compare(aToken, TableNameSpaceToken) == 0)
                return (additionalColumnsInfo != null) ? additionalColumnsInfo.TableNameSpace : String.Empty;

            if (String.Compare(aToken, TableClassNameToken) == 0)
            {
                if (additionalColumnsInfo == null)
                    return String.Empty;

                WizardTableInfo originalTableInfo = additionalColumnsInfo.GetOriginalTableInfo();
                return (originalTableInfo != null) ? originalTableInfo.ClassName : String.Empty;
            }

            if (String.Compare(aToken, TableLibraryRelativePathToken) == 0)
            {
                if (additionalColumnsInfo == null || additionalColumnsInfo.Library == null)
                    return String.Empty;

                WizardTableInfo originalTableInfo = additionalColumnsInfo.GetOriginalTableInfo();

                if (originalTableInfo == null || originalTableInfo.Library == null)
                    return String.Empty;

                string relativePath = Generics.MakeRelativeTo(WizardCodeGenerator.GetStandardLibraryPath(additionalColumnsInfo.Library), WizardCodeGenerator.GetStandardLibraryPath(originalTableInfo.Library));
                if (relativePath == null || relativePath.Length == 0)
                    return String.Empty;

                return relativePath + Path.DirectorySeparatorChar;
            }

            if (String.Compare(aToken, TableModuleNameToken) == 0)
            {
                if (additionalColumnsInfo == null || additionalColumnsInfo.Library == null)
                    return String.Empty;

                WizardTableInfo originalTableInfo = additionalColumnsInfo.GetOriginalTableInfo();

                if (originalTableInfo == null || originalTableInfo.Library == null || originalTableInfo.Library.Module == null)
                    return String.Empty;

                return originalTableInfo.Library.Module.Name;
            }

            if (String.Compare(aToken, TableLibrarySourceFolderToken) == 0)
            {
                if (additionalColumnsInfo == null || additionalColumnsInfo.Library == null)
                    return String.Empty;

                WizardTableInfo originalTableInfo = additionalColumnsInfo.GetOriginalTableInfo();

                if (originalTableInfo == null || originalTableInfo.Library == null)
                    return String.Empty;

                return originalTableInfo.Library.SourceFolder;
            }

            if (String.Compare(aToken, AdditionalColumnsClassNameToken) == 0)
                return (additionalColumnsInfo != null) ? additionalColumnsInfo.ClassName : String.Empty;

            if (String.Compare(aToken, AdditionalColumnsModuleNameToken) == 0)
                return (additionalColumnsInfo != null && additionalColumnsInfo.Library != null && additionalColumnsInfo.Library.Module != null) ? additionalColumnsInfo.Library.Module.Name : String.Empty;

            if (String.Compare(aToken, AdditionalColumnsLibrarySourceFolderToken) == 0)
                return (additionalColumnsInfo != null && additionalColumnsInfo.Library != null) ? additionalColumnsInfo.Library.SourceFolder : String.Empty;

            if (String.Compare(aToken, ColumnsCountToken) == 0)
                return (additionalColumnsInfo != null) ? additionalColumnsInfo.ColumnsCount.ToString() : String.Empty;

            if (aToken.StartsWith(RepeatOnColumnsToken))
            {
                if (additionalColumnsInfo == null || additionalColumnsInfo.ColumnsCount == 0)
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnColumnsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string columnsLines = String.Empty;
                    if (additionalColumnsInfo.ColumnsInfo != null && additionalColumnsInfo.ColumnsInfo.Count > 0)
                    {
                        foreach (WizardTableColumnInfo aColumnInfo in additionalColumnsInfo.ColumnsInfo)
                        {
                            string columnParsedText = ((IWizardTableCodeTemplateParser)this).SubstituteTableColumnTokens(subToken, aColumnInfo);

                            if (columnParsedText != null && columnParsedText.Length > 0)
                            {
                                if (columnsLines.Length > 0)
                                    columnsLines += columnsLinesSeparator;
                                columnsLines += columnParsedText;
                            }
                        }
                    }

                    return columnsLines;
                }
            }

            if (aToken.StartsWith(IfExistsAlterTableToken))
			{
                if (additionalColumnsInfo == null ||
                    additionalColumnsInfo.ColumnsCount == 0 ||
                    additionalColumnsInfo.ColumnsInfo[0] == null)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfExistsAlterTableToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfExistsUnrelatedUpdatesToken))
            {
                if (
                    (additionalColumnsInfo == null ||
                    additionalColumnsInfo.ColumnsCount == 0 ||
                    additionalColumnsInfo.ColumnsInfo[0] == null ) &&
                    currentColumnUpdate != null
                    )
                {
                    string subToken;
                    if (ResolveFunctionToken(aToken, IfExistsUnrelatedUpdatesToken, out subToken))
                    {
                        if (subToken == null || subToken.Length == 0)
                            return String.Empty;

                        return this.SubstituteTokens(subToken);
                    }
                }
                else
                    return String.Empty;
            }
           
			if (aToken.StartsWith(IfUpdateColumnNeededToken))
			{
				if (currentColumnUpdate == null)
					return String.Empty;

				string subToken;
				if (ResolveFunctionToken(aToken, IfUpdateColumnNeededToken, out subToken))
				{
					if (subToken == null || subToken.Length == 0)
						return String.Empty;

					return this.SubstituteTokens(subToken);
				}
			}

			// qui ci entra anche con il token IfExistsWhereClauseToken incontrato nella prima UPDATE
			// quindi non bisogna testare il currentColumnUpdate, bensì guardare nella additionalColumnsInfo.ColumnsInfo[0].x
			// peccato che in quella classe non ci sono i set, ma solo il default, che andrebbe preso come riferimento
            if (aToken.StartsWith(IfExistsWhereClauseToken))
            {
                if (currentColumnUpdate == null || !currentColumnUpdate.ExistsWhereClause)
					return String.Empty;
                
				string subToken;
                if (ResolveFunctionToken(aToken, IfExistsWhereClauseToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
						return String.Empty;

                    return this.SubstituteTokens(subToken);
                }
            }

			// qui ci entra anche con il token SetValue incontrato nella prima UPDATE
			// quindi non bisogna testare il currentColumnUpdate, bensì guardare nella additionalColumnsInfo.ColumnsInfo[0].x
			// peccato che in quella classe non ci sono i set, ma solo il default, che andrebbe preso come riferimento
            if (aToken.StartsWith(SetValueToken))
            {
                if (currentColumnUpdate == null) 
					return String.Empty;
                if (dbType == DBMSType.SQLSERVER)
                    return currentColumnUpdate.SetValueForSql;
                if (dbType == DBMSType.ORACLE)
                    return currentColumnUpdate.SetValueForOracle;
            }

            if (aToken.StartsWith(UpdateTableNameToken))
            {
                return (currentColumnUpdate != null) ? currentColumnUpdate.TableName : String.Empty;
            }

            if (aToken.StartsWith(UpdateColumnNameToken))
            {
                return (currentColumnUpdate != null) ? currentColumnUpdate.SetColumnName : String.Empty;
            }

            if (aToken.StartsWith(WhereColumnNameToken))
            {
                return (currentColumnUpdate != null) ? currentColumnUpdate.WhereColumnName : String.Empty;
            }

            if (aToken.StartsWith(WhereTableNameToken))
            {
                return (currentColumnUpdate != null) ? currentColumnUpdate.WhereTableName : String.Empty;
            }

            if (aToken.StartsWith(WhereValueToken))
            {
                if (currentColumnUpdate == null) 
					return String.Empty;
                if (dbType == DBMSType.SQLSERVER)
                    return currentColumnUpdate.WhereValueForSql;
                if (dbType == DBMSType.ORACLE)
                    return currentColumnUpdate.WhereValueForOracle;
            }

			if (aToken.StartsWith(IfContainsDataEnumsToken))
            {
                if (additionalColumnsInfo == null || !additionalColumnsInfo.ContainsDataEnumColumns())
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfContainsDataEnumsToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(RepeatOnDataEnumColumnsToken))
            {
                if (additionalColumnsInfo == null || !additionalColumnsInfo.ContainsDataEnumColumns())
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnDataEnumColumnsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string columnsLines = String.Empty;
                    foreach (WizardTableColumnInfo aColumnInfo in additionalColumnsInfo.ColumnsInfo)
                    {
                        if (aColumnInfo.EnumInfo == null)
                            continue;

                        string columnParsedText = ((IWizardTableCodeTemplateParser)this).SubstituteTableColumnTokens(subToken, aColumnInfo, true);
                        if (AreTokensToSubstitute(columnParsedText))
                        {
                            WizardEnumCodeTemplateParser enumParser = new WizardEnumCodeTemplateParser(aColumnInfo.EnumInfo);

                            columnParsedText = enumParser.SubstituteTokens(columnParsedText);
                        }

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }
                    }
                    return columnsLines;
                }
            }

            if (aToken.StartsWith(RepeatOnReferencedEnumsToken))
            {
                if (additionalColumnsInfo == null || additionalColumnsInfo.Library == null || additionalColumnsInfo.Library.Application == null || !additionalColumnsInfo.ContainsDataEnumColumns())
                    return String.Empty;

                string subToken;
                string enumsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnReferencedEnumsToken, out subToken, out enumsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    WizardEnumInfoCollection referencedEnums = new WizardEnumInfoCollection();

                    foreach (WizardTableColumnInfo aColumnInfo in additionalColumnsInfo.ColumnsInfo)
                    {
                        if
                            (
                            aColumnInfo.EnumInfo == null ||
                            additionalColumnsInfo.Library.Application.GetEnumInfoByName(aColumnInfo.EnumInfo.Name) != null ||
                            (referencedEnums.Count > 0 && referencedEnums.GetEnumInfoByName(aColumnInfo.EnumInfo.Name) != null)
                            )
                            continue;

                        referencedEnums.Add(aColumnInfo.EnumInfo);
                    }

                    if (referencedEnums.Count == 0)
                        return String.Empty;

                    string enumsLines = String.Empty;
                    foreach (WizardEnumInfo aEnumInfo in referencedEnums)
                    {
                        string enumParsedText = this.SubstituteTokens(subToken, true);

                        if (AreTokensToSubstitute(enumParsedText))
                        {
                            WizardEnumCodeTemplateParser enumParser = new WizardEnumCodeTemplateParser(aEnumInfo);

                            enumParsedText = enumParser.SubstituteTokens(enumParsedText);
                        }

                        if (enumParsedText != null && enumParsedText.Length > 0)
                        {
                            if (enumsLines.Length > 0)
                                enumsLines += enumsLinesSeparator;
                            enumsLines += enumParsedText;
                        }
                    }
                    return enumsLines;
                }
            }

            if (aToken.StartsWith(RepeatOnUsedEnumsModulesToken))
            {
                if (additionalColumnsInfo == null || additionalColumnsInfo.Library == null || additionalColumnsInfo.Library.Application == null || !additionalColumnsInfo.ContainsDataEnumColumns())
                    return String.Empty;

                string subToken;
                string modulesLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnUsedEnumsModulesToken, out subToken, out modulesLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    WizardModuleInfoCollection enumsModules = new WizardModuleInfoCollection();

                    foreach (WizardTableColumnInfo aColumnInfo in additionalColumnsInfo.ColumnsInfo)
                    {
                        if (aColumnInfo.EnumInfo == null)
                            continue;

                        WizardModuleInfo enumModule = additionalColumnsInfo.Library.Application.GetModuleContainingEnum(aColumnInfo.EnumInfo.Name);
                        if
                            (
                            enumModule == null ||
                            (enumsModules.Count > 0 && enumsModules.GetModuleInfoByName(enumModule.Name) != null)
                            )
                            continue;

                        enumsModules.Add(enumModule);
                    }

                    if (enumsModules.Count == 0)
                        return String.Empty;

                    string modulesLines = String.Empty;
                    foreach (WizardModuleInfo aModuleInfo in enumsModules)
                    {
                        WizardModuleCodeTemplateParser moduleParser = new WizardModuleCodeTemplateParser(aModuleInfo);

                        string moduleParsedText = moduleParser.SubstituteTokens(subToken, true);

                        if (AreTokensToSubstitute(moduleParsedText))
                            moduleParsedText = this.SubstituteTokens(moduleParsedText);

                        if (moduleParsedText != null && moduleParsedText.Length > 0)
                        {
                            if (modulesLines.Length > 0)
                                modulesLines += modulesLinesSeparator;
                            modulesLines += moduleParsedText;
                        }
                    }
                    return modulesLines;
                }
            }

            if (String.Compare(aToken, DBTLibraryRelativePathToken) == 0)
            {
                if (additionalColumnsInfo == null || additionalColumnsInfo.Library == null || dbtInfo == null || dbtInfo.Library == null)
                    return String.Empty;

                string relativePath = Generics.MakeRelativeTo(WizardCodeGenerator.GetStandardLibraryPath(dbtInfo.Library), WizardCodeGenerator.GetStandardLibraryPath(additionalColumnsInfo.Library));
                if (relativePath != null && relativePath.Length > 0)
                    return relativePath + Path.DirectorySeparatorChar;
            }

            if (String.Compare(aToken, DBTModuleNameToken) == 0)
            {
                if (additionalColumnsInfo == null || additionalColumnsInfo.Library == null || dbtInfo == null || dbtInfo.Library == null || dbtInfo.Library.Module == null)
                    return String.Empty;

                return dbtInfo.Library.Module.Name;
            }

            if (String.Compare(aToken, DBTLibrarySourceFolderToken) == 0)
            {
                if (additionalColumnsInfo == null || additionalColumnsInfo.Library == null || dbtInfo == null || dbtInfo.Library == null)
                    return String.Empty;

                return dbtInfo.Library.SourceFolder;
            }

            if (String.Compare(aToken, DocumentLibraryRelativePathToken) == 0)
            {
                if (additionalColumnsInfo == null || additionalColumnsInfo.Library == null || documentInfo == null || documentInfo.Library == null)
                    return String.Empty;

                string relativePath = Generics.MakeRelativeTo(WizardCodeGenerator.GetStandardLibraryPath(documentInfo.Library), WizardCodeGenerator.GetStandardLibraryPath(additionalColumnsInfo.Library));
                if (relativePath != null && relativePath.Length > 0)
                    return relativePath + Path.DirectorySeparatorChar;
            }

            if (String.Compare(aToken, DocumentModuleNameToken) == 0)
            {
                if (additionalColumnsInfo == null || additionalColumnsInfo.Library == null || documentInfo == null || documentInfo.Library == null || documentInfo.Library.Module == null)
                    return String.Empty;

                return documentInfo.Library.Module.Name;
            }

            if (String.Compare(aToken, DocumentLibrarySourceFolderToken) == 0)
            {
                if (additionalColumnsInfo == null || additionalColumnsInfo.Library == null || documentInfo == null || documentInfo.Library == null)
                    return String.Empty;

                return documentInfo.Library.SourceFolder;
            }

            if (String.Compare(aToken, ClientDocumentLibraryRelativePathToken) == 0)
            {
                if (additionalColumnsInfo == null || additionalColumnsInfo.Library == null || clientDocumentInfo == null || clientDocumentInfo.Library == null)
                    return String.Empty;

                string relativePath = Generics.MakeRelativeTo(WizardCodeGenerator.GetStandardLibraryPath(clientDocumentInfo.Library), WizardCodeGenerator.GetStandardLibraryPath(additionalColumnsInfo.Library));
                if (relativePath != null && relativePath.Length > 0)
                    return relativePath + Path.DirectorySeparatorChar;
            }

            if (String.Compare(aToken, ClientDocumentModuleNameToken) == 0)
            {
                if (additionalColumnsInfo == null || additionalColumnsInfo.Library == null || clientDocumentInfo == null || clientDocumentInfo.Library == null || clientDocumentInfo.Library.Module == null)
                    return String.Empty;

                return clientDocumentInfo.Library.Module.Name;
            }

            if (String.Compare(aToken, ClientDocumentLibrarySourceFolderToken) == 0)
            {
                if (additionalColumnsInfo == null || additionalColumnsInfo.Library == null || clientDocumentInfo == null || clientDocumentInfo.Library == null)
                    return String.Empty;

                return clientDocumentInfo.Library.SourceFolder;
            }

            if (aToken.StartsWith(IfIsNotTableReferencedToken))
            {
                if (additionalColumnsInfo == null)
                    return String.Empty;

                WizardTableInfo originalTableInfo = additionalColumnsInfo.GetOriginalTableInfo();

                if (originalTableInfo == null || originalTableInfo.IsReferenced)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfIsNotTableReferencedToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken, true);
                }
            }

            if (aToken.StartsWith(IfIsTableReferencedToken))
            {
                if (additionalColumnsInfo == null)
                    return String.Empty;

                WizardTableInfo originalTableInfo = additionalColumnsInfo.GetOriginalTableInfo();

                if (originalTableInfo == null || !originalTableInfo.IsReferenced)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfIsTableReferencedToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken, true);
                }
            }

            if (String.Compare(aToken, ReferencedTableHeaderRelativePathToken) == 0)
            {
                if (additionalColumnsInfo == null || additionalColumnsInfo.Library == null)
                    return String.Empty;

                WizardTableInfo originalTableInfo = additionalColumnsInfo.GetOriginalTableInfo();

                if (originalTableInfo == null || !originalTableInfo.IsReferenced)
                    return String.Empty;

                string referencedTableIncludeFile = additionalColumnsInfo.ReferencedTableIncludeFile;
                if (referencedTableIncludeFile == null || referencedTableIncludeFile.Trim().Length == 0)
                    return String.Empty;

                return Generics.MakeRelativeTo(WizardCodeGenerator.GetStandardLibraryPath(additionalColumnsInfo.Library), referencedTableIncludeFile);
            }

            if (String.Compare(aToken, WoormColumnDefaultWidthToken) == 0)
                return (additionalColumnsInfo != null && additionalColumnsInfo.ColumnsCount > 0) ? (WoormTableDefaultTotalWidth / additionalColumnsInfo.ColumnsCount).ToString() : String.Empty;

            return null;
        }

        //----------------------------------------------------------------------------
        string IWizardTableCodeTemplateParser.GetTokenValue(string aToken)
        {
            return GetTokenValue(aToken);
        }

        //----------------------------------------------------------------------------
        string IWizardTableCodeTemplateParser.SubstituteTableColumnTokens
            (
            string source,
            WizardTableColumnInfo aColumnInfo,
            WizardTableColumnInfo aPreviousVersionColumnInfo,
            bool preserveUnresolvedTokens
            )
        {
            if (source == null || aColumnInfo == null)
                return null;

            source = source.Trim();
            if (source.Length == 0)
                return String.Empty;

            int columnIndex = -1;
            if (additionalColumnsInfo != null && additionalColumnsInfo.ColumnsInfo.Contains(aColumnInfo))
                columnIndex = additionalColumnsInfo.ColumnsInfo.IndexOf(aColumnInfo);

            string columnParsedText = this.SubstituteTokens(source, true);

            if (AreTokensToSubstitute(columnParsedText))
            {
                WizardTableColumnCodeTemplateParser columnParser = new WizardTableColumnCodeTemplateParser(aColumnInfo, dbType, columnIndex, additionalColumnsInfo);

                columnParsedText = columnParser.SubstituteTokens(columnParsedText, preserveUnresolvedTokens || aPreviousVersionColumnInfo != null);

                if (aPreviousVersionColumnInfo != null && AreTokensToSubstitute(columnParsedText))
                {
                    WizardPreviousVersionColumnCodeTemplateParser previousVersionColumnParser = new WizardPreviousVersionColumnCodeTemplateParser(aPreviousVersionColumnInfo, dbType, columnIndex);

                    columnParsedText = previousVersionColumnParser.SubstituteTokens(columnParsedText, true);
                }

                if (AreTokensToSubstitute(columnParsedText))
                    return this.SubstituteTokens(columnParsedText, preserveUnresolvedTokens);
            }

            return columnParsedText;
        }

        //----------------------------------------------------------------------------
        string IWizardTableCodeTemplateParser.SubstituteTableColumnTokens
            (
            string source,
            WizardTableColumnInfo aColumnInfo,
            bool preserveUnresolvedTokens
            )
        {
            return ((IWizardTableCodeTemplateParser)this).SubstituteTableColumnTokens(source, aColumnInfo, null, preserveUnresolvedTokens);
        }

        //----------------------------------------------------------------------------
        string IWizardTableCodeTemplateParser.SubstituteTableColumnTokens
            (
            string source,
            WizardTableColumnInfo aColumnInfo,
            WizardTableColumnInfo aPreviousVersionColumnInfo
            )
        {
            return ((IWizardTableCodeTemplateParser)this).SubstituteTableColumnTokens(source, aColumnInfo, aPreviousVersionColumnInfo, false);
        }

        //----------------------------------------------------------------------------
        string IWizardTableCodeTemplateParser.SubstituteTableColumnTokens(string source, WizardTableColumnInfo aColumnInfo)
        {
            return ((IWizardTableCodeTemplateParser)this).SubstituteTableColumnTokens(source, aColumnInfo, false);
        }

        //----------------------------------------------------------------------------
        string IWizardTableCodeTemplateParser.SubstituteForeignKeyTokens(string source, WizardForeignKeyInfo aForeignKeyInfo)
        {
            return null;
        }
    }

    #endregion

    #region WizardTableColumnCodeTemplateParser class

    //============================================================================
    internal class WizardTableColumnCodeTemplateParser : CodeTemplateParser
    {
        private const string ColumnNameToken = "ColumnName";
        private const string ColumnDBFullDataTypeToken = "ColumnDBFullDataType";
        private const string ColumnDBNullableAttributeToken = "ColumnDBNullableAttribute";
        private const string IfIsPrimaryKeySegmentToken = "IfIsPrimaryKeySegment";
        private const string IfIsNotPrimaryKeySegmentToken = "IfIsNotPrimaryKeySegment";
        private const string IfHasDefaultValueToken = "IfHasDefaultValue";
		private const string IfDefaultConstraintNameExistToken = "IfDefaultConstraintNameExist";
        private const string ColumnDBDefaultValueToken = "ColumnDBDefaultValue";
        private const string ColumnDataObjClassNameToken = "ColumnDataObjClassName";
        private const string ColumnDataLengthToken = "DataLength";
        private const string ColumnDefaultConstraintNameToken = "ColumnDefaultConstraintName";
		private const string IfIsCollateSensitiveToken = "IfIsCollateSensitive";
		private const string IfIsAutoIncrementToken = "IfIsAutoIncrement";
		private const string ColumnAutoIncrementSeedToken = "Seed";
		private const string ColumnAutoIncrementIncrementToken = "Increment";
        private const string WoormColumnTypeToken = "WoormColumnType";
        private const string WoormColumnDefaultLengthToken = "WoormColumnDefaultLength";
        private const string WoormColumnCounterToken = "WoormColumnCounter";
        private const string ColumnDefaultFormatStyleToken = "ColumnDefaultFormatStyle";

        private const string IfIsEnumTypeToken = "IfIsEnumType";
        private const string IfIsNotEnumTypeToken = "IfIsNotEnumType";

        private const string IfColumnLengthIsGreaterThanMaxBodyEditColumnWidthToken = "IfColumnLengthIsGreaterThanMaxBodyEditColumnWidth";
        private const string MaxBodyEditColumnWidthToken = "MaxBodyEditColumnWidth";
        private const string BodyEditColumnRowsNumberToken = "BodyEditColumnRowsNumber";

        protected WizardTableColumnInfo columnInfo = null;
		private DBMSType dbType = DBMSType.UNKNOWN;
        private int columnTableIndex = -1;
        protected WizardTableInfo tableInfo = null;
        protected WizardExtraAddedColumnsInfo additionalColumnsInfo = null;

        //----------------------------------------------------------------------------
		internal WizardTableColumnCodeTemplateParser(WizardTableColumnInfo aColumnInfo, DBMSType aDBType, int aIndex, WizardTableInfo aTableInfo)
        {
            columnInfo = aColumnInfo;
            dbType = aDBType;
            columnTableIndex = aIndex;
            tableInfo = aTableInfo;
            additionalColumnsInfo = null;
        }

        //----------------------------------------------------------------------------
		internal WizardTableColumnCodeTemplateParser(WizardTableColumnInfo aColumnInfo, DBMSType aDBType, int aIndex, WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo)
        {
            columnInfo = aColumnInfo;
            dbType = aDBType;
            columnTableIndex = aIndex;
            tableInfo = null;
            additionalColumnsInfo = aExtraAddedColumnsInfo;
        }

        //----------------------------------------------------------------------------
		internal WizardTableColumnCodeTemplateParser(WizardTableColumnInfo aColumnInfo, DBMSType aDBType, int aIndex)
            : this(aColumnInfo, aDBType, aIndex, (WizardTableInfo)null)
        {
        }

        //----------------------------------------------------------------------------
        internal WizardTableColumnCodeTemplateParser(WizardTableColumnInfo aColumnInfo, int aIndex, WizardTableInfo aTableInfo)
			: this(aColumnInfo, DBMSType.UNKNOWN, aIndex, aTableInfo)
        {
        }

        //----------------------------------------------------------------------------
        internal WizardTableColumnCodeTemplateParser(WizardTableColumnInfo aColumnInfo, int aIndex, WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo)
			: this(aColumnInfo, DBMSType.UNKNOWN, aIndex, aExtraAddedColumnsInfo)
        {
        }

        //----------------------------------------------------------------------------
        internal WizardTableColumnCodeTemplateParser(WizardTableColumnInfo aColumnInfo, int aIndex)
			: this(aColumnInfo, DBMSType.UNKNOWN, aIndex)
        {
        }

        //----------------------------------------------------------------------------
        internal override string GetTokenValue(string aToken)
        {
            if (aToken == null || aToken.Length == 0)
                return String.Empty;

            if (String.Compare(aToken, ColumnNameToken) == 0)
                return (columnInfo != null) ? columnInfo.Name : String.Empty;

            if (String.Compare(aToken, ColumnDBFullDataTypeToken) == 0)
            {
                if (columnInfo == null)
                    return String.Empty;

				if (dbType == DBMSType.SQLSERVER)
                    return WizardTableColumnDataType.GetSQLServerTranslation(columnInfo.DataType, columnInfo.DataLength);

				if (dbType == DBMSType.ORACLE)
                    return WizardTableColumnDataType.GetOracleTranslation(columnInfo.DataType, columnInfo.DataLength);

                return String.Empty;
            }

            if (String.Compare(aToken, ColumnDBNullableAttributeToken) == 0)
            {
                if (columnInfo == null)
                    return String.Empty;
				if (columnInfo.IsPrimaryKeySegment)
					return "NOT NULL";

				// le colonne di tipo IDENTITY (solo sql) devono essere obbligatoriamente NOT NULL
				if (dbType == DBMSType.SQLSERVER && columnInfo.IsAutoIncrement)
						return "NOT NULL";

				return columnInfo.IsNullable 
					? (dbType == DBMSType.SQLSERVER) ? "NULL" : String.Empty 
					: "NOT NULL";
            }

            if (aToken.StartsWith(IfIsPrimaryKeySegmentToken))
            {
                if (columnInfo == null || !columnInfo.IsPrimaryKeySegment)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfIsPrimaryKeySegmentToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken, true);
                }
            }

            if (aToken.StartsWith(IfIsNotPrimaryKeySegmentToken))
            {
                if (columnInfo == null || columnInfo.IsPrimaryKeySegment)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfIsNotPrimaryKeySegmentToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken, true);
                }
            }
			
			if (aToken.StartsWith(IfIsCollateSensitiveToken))
			{
				 if (columnInfo == null || columnInfo.IsCollateSensitive) return String.Empty;
				 string subToken;
				 if (ResolveFunctionToken(aToken, IfIsCollateSensitiveToken, out subToken))
				 {
					 if (subToken == null || subToken.Length == 0)
						 return String.Empty;

					 return this.SubstituteTokens(subToken, true);
				 }
			}

			if (aToken.StartsWith(IfIsAutoIncrementToken))
			{
				if (columnInfo == null || !columnInfo.IsAutoIncrement || columnInfo.Seed < 1 || columnInfo.Increment < 1)
					return String.Empty;

				string subToken;
				if (ResolveFunctionToken(aToken, IfIsAutoIncrementToken, out subToken))
				{
					if (subToken == null || subToken.Length == 0)
						return String.Empty;

					return this.SubstituteTokens(subToken, true);
				}
			}

			if (String.Compare(aToken, ColumnAutoIncrementSeedToken) == 0)
			{
				if (columnInfo == null || !columnInfo.IsAutoIncrement || columnInfo.Seed < 1) 
					return String.Empty;
				return columnInfo.Seed.ToString();
			}
	
			if (String.Compare(aToken, ColumnAutoIncrementIncrementToken) == 0)
			{
				if (columnInfo == null || !columnInfo.IsAutoIncrement || columnInfo.Increment < 1) 
					return String.Empty;
				return columnInfo.Increment.ToString();
			}

			if (aToken.StartsWith(IfDefaultConstraintNameExistToken))
            {
				if (columnInfo == null || 
					columnInfo.IsAutoIncrement /*le colonne IDENTITY non possono avere un CONSTRAINT di DEFAULT*/ ||
					string.IsNullOrEmpty(columnInfo.DefaultConstraintName))
					return String.Empty;

				string subToken;
				if (ResolveFunctionToken(aToken, IfDefaultConstraintNameExistToken, out subToken))
				{
					if (subToken == null || subToken.Length == 0)
						return String.Empty;

					return this.SubstituteTokens(subToken, true);
				}
			}

            if (aToken.StartsWith(IfHasDefaultValueToken))
            {
                if (columnInfo == null ||
                   //(columnInfo.IsPrimaryKeySegment && !columnInfo.HasSpecificDefaultValue) ||
					columnInfo.IsAutoIncrement /*le colonne IDENTITY non possono avere un DEFAULT*/ ||
                   (columnInfo.DefaultValue == null && columnInfo.DefaultExpressionValue == null))
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfHasDefaultValueToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken, true);
                }
            }

            if (String.Compare(aToken, ColumnDBDefaultValueToken) == 0)
            {
                if (columnInfo == null ||
                    columnInfo.DataType.Type == WizardTableColumnDataType.DataType.Undefined)
                    return String.Empty;

				if (!string.IsNullOrEmpty(columnInfo.DefaultExpressionValue))
					return GetDefaultExpressionValueForDBType();
				else 
					if (columnInfo.DefaultValue != null)
						return columnInfo.GetDBDefaultValueString(dbType);
                
				return String.Empty;
            }

            if (String.Compare(aToken, ColumnDataObjClassNameToken) == 0)
                return (columnInfo != null) ? columnInfo.GetDataObjClassName() : String.Empty;

            if (String.Compare(aToken, ColumnDataLengthToken) == 0)
                return (columnInfo != null) ? columnInfo.DataLength.ToString() : String.Empty;

            if (String.Compare(aToken, ColumnDefaultConstraintNameToken) == 0)
                return (columnInfo != null) ? columnInfo.DefaultConstraintName : String.Empty;

            if (String.Compare(aToken, WoormColumnTypeToken) == 0)
                return (columnInfo != null) ? columnInfo.GetWoormDataTypeText() : String.Empty;

            if (String.Compare(aToken, WoormColumnDefaultLengthToken) == 0)
                return (columnInfo != null) ? columnInfo.GetWoormDefaultDataLength().ToString() : String.Empty;

            if (String.Compare(aToken, WoormColumnCounterToken) == 0)
                return (columnTableIndex >= 0) ? (columnTableIndex + 2).ToString() : String.Empty;

            if (String.Compare(aToken, ColumnDefaultFormatStyleToken) == 0)
                return (columnTableIndex >= 0) ? columnInfo.GetDefaultFormatStyleName() : String.Empty;

            if (aToken.StartsWith(IfIsEnumTypeToken))
            {
                if
                    (
                    columnInfo == null ||
                    columnInfo.DataType.Type != WizardTableColumnDataType.DataType.Enum ||
                    columnInfo.EnumInfo == null
                    )
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfIsEnumTypeToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken, true);
                }
            }

            if (aToken.StartsWith(IfIsNotEnumTypeToken))
            {
                if
                    (
                    columnInfo == null ||
                    columnInfo.DataType.Type == WizardTableColumnDataType.DataType.Enum
                    )
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfIsNotEnumTypeToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken, true);
                }
            }

            if (aToken.StartsWith(IfColumnLengthIsGreaterThanMaxBodyEditColumnWidthToken))
            {
                if
                    (
                    columnInfo == null ||
                    columnInfo.DataType.Type != WizardTableColumnDataType.DataType.String ||
                    columnInfo.DataLength <= Generics.MaxBodyEditColumnWidth
                    )
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfColumnLengthIsGreaterThanMaxBodyEditColumnWidthToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken, true);
                }
            }

            if (String.Compare(aToken, MaxBodyEditColumnWidthToken) == 0)
                return Generics.MaxBodyEditColumnWidth.ToString();

            if (String.Compare(aToken, BodyEditColumnRowsNumberToken) == 0)
            {
                int columnRowsCount = 1;

                if (
                    columnInfo != null &&
                    columnInfo.DataType.Type == WizardTableColumnDataType.DataType.String &&
                    columnInfo.DataLength > Generics.MaxBodyEditColumnWidth
                    )
                {
                    columnRowsCount += ((int)columnInfo.DataLength / Generics.MaxBodyEditColumnWidth);
                }

                return columnRowsCount.ToString();
            }

            return null;
        }

		///<summary>
		/// In caso di valori espressione specificati nel constraint di DEFAULT, cerchiamo di "adattare" alla sintassi di
		/// Oracle le funzioni più utilizzate (GetDate e porzioni di data + Guid), x limitare il più possibile gli errori
		/// in fase di esecuzione degli script
		///</summary>
		//----------------------------------------------------------------------------
		private string GetDefaultExpressionValueForDBType()
		{
			if (this.dbType == DBMSType.ORACLE)
			{
				if (string.Compare(columnInfo.DefaultExpressionValue, "GetDate()", StringComparison.InvariantCultureIgnoreCase) == 0
					&& columnInfo.DataType.Type == WizardTableColumnDataType.DataType.Date)
					return "SYSDATE";

				if (string.Compare(columnInfo.DefaultExpressionValue, "YEAR(GetDate())", StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					return columnInfo.DataType.IsTextual
						? "TO_CHAR(SYSDATE, 'YYYY')"
						: "TO_NUMBER((TO_CHAR(SYSDATE, 'YYYY')))";
				}

				if (string.Compare(columnInfo.DefaultExpressionValue, "MONTH(GetDate())", StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					return columnInfo.DataType.IsTextual
							? "TO_CHAR(SYSDATE, 'MM')"
							: "TO_NUMBER((TO_CHAR(SYSDATE, 'MM')))";
				}

				if (string.Compare(columnInfo.DefaultExpressionValue, "DAY(GetDate())", StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					return columnInfo.DataType.IsTextual
							? "TO_CHAR(SYSDATE, 'DD')"
							: "TO_NUMBER((TO_CHAR(SYSDATE, 'DD')))";
				}

				if (string.Compare(columnInfo.DefaultExpressionValue, "NEWID()", StringComparison.InvariantCultureIgnoreCase) == 0)
					return "SYS_GUID()";
			}

			return columnInfo.DefaultExpressionValue;
		}
    }
    #endregion

    #region WizardPreviousVersionColumnCodeTemplateParser class

    //============================================================================
    internal class WizardPreviousVersionColumnCodeTemplateParser : WizardTableColumnCodeTemplateParser
    {
        private const string ApplyToPreviousVersionToken = "ApplyToPreviousVersion";
        private bool applying = false;

        //----------------------------------------------------------------------------
		internal WizardPreviousVersionColumnCodeTemplateParser(WizardTableColumnInfo aColumnInfo, DBMSType aDBType, int aIndex)
            : base(aColumnInfo, aDBType, aIndex)
        {
        }

        //----------------------------------------------------------------------------
        internal override string GetTokenValue(string aToken)
        {
            if (aToken == null || aToken.Length == 0)
                return String.Empty;

            if (applying)
                return base.GetTokenValue(aToken);

            if (aToken.StartsWith(ApplyToPreviousVersionToken))
            {
                if (columnInfo == null)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, ApplyToPreviousVersionToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    applying = true;

                    string parsedText = this.SubstituteTokens(subToken);

                    applying = false;

                    return parsedText;
                }
            }

            return null;
        }
    }

    #endregion

    #region WizardDBTCodeTemplateParser class

    //============================================================================
    internal class WizardDBTCodeTemplateParser : CodeTemplateParser
    {
        private const string ModuleNameToken = "ModuleName";
        private const string LibraryNameToken = "LibraryName";
        private const string LibrarySourceFolderToken = "LibrarySourceFolder";
        private const string DocumentNameToken = "DocumentName";
        private const string DocumentClassNameToken = "DocumentClassName";
        private const string ClientDocumentNameToken = "ClientDocumentName";
        private const string ClientDocumentClassNameToken = "ClientDocumentClassName";
        private const string ClientDocumentViewClassNameToken = "ClientDocumentViewClassName";
        private const string ClientDocumentViewFileNameToken = "ClientDocumentViewFileName";
        private const string ServerDocumentNameToken = "ServerDocumentName";
        private const string ServerDocumentClassNameToken = "ServerDocumentClassName";
        private const string DBTNameToken = "DBTName";
        private const string DBTClassNameToken = "DBTClassName";
        private const string DBTSlaveTabTitleToken = "DBTSlaveTabTitle";
        private const string DBTLibraryRelativePathToken = "DBTLibraryRelativePath";
        private const string DBTModuleNameToken = "DBTModuleName";
        private const string DBTLibrarySourceFolderToken = "DBTLibrarySourceFolder";
        private const string TableNameToken = "TableName";
        private const string TableClassNameToken = "TableClassName";
        private const string TableLibraryRelativePathToken = "TableLibraryRelativePath";
        private const string TableModuleNameToken = "TableModuleName";
        private const string TableLibrarySourceFolderToken = "TableLibrarySourceFolder";
        private const string MasterTableNameToken = "MasterTableName";
        private const string MasterTableClassNameToken = "MasterTableClassName";
        private const string MasterTableHeaderRelativePathToken = "MasterTableHeaderRelativePath";
        private const string FirstForeignKeySegmentNameToken = "FirstForeignKeySegmentName";
        private const string IfDBTMasterToken = "IfDBTMaster";
        private const string IfNotDBTMasterToken = "IfNotDBTMaster";
        private const string IfDBTSlaveBufferedToken = "IfDBTSlaveBuffered";
        private const string IfNotDBTSlaveBufferedToken = "IfNotDBTSlaveBuffered";
        private const string IfCreateRowFormToken = "IfCreateRowForm";
        private const string IfNotCreateRowFormToken = "IfNotCreateRowForm";

        private const string RepeatOnColumnsToken = "RepeatOnColumns";
        private const string RepeatOnPrimaryKeySegmentsToken = "RepeatOnPrimaryKeySegments";
        private const string RepeatOnVisibleColumnsToken = "RepeatOnVisibleColumns";
        private const string RepeatOnFindableColumnsToken = "RepeatOnFindableColumns";
		private const string RepeatOnLabelsToken = "RepeatOnLabels";
        private const string RepeatOnRowFormColumnsToken = "RepeatOnRowFormColumns";
        private const string RepeatOnForeignKeySegmentsToken = "RepeatOnForeignKeySegments";
        private const string RepeatOnHotLinkColumnsToken = "RepeatOnHotLinkColumns";
        private const string RepeatOnApplicationHotLinkColumnsToken = "RepeatOnApplicationHotLinkColumns";
        private const string RepeatOnReferencedHotLinkIncludeFilesToken = "RepeatOnReferencedHotLinkIncludeFiles";

        private const string DBTDialogWidthToken = "DBTDialogWidth";
        private const string DBTDialogHeightToken = "DBTDialogHeight";

        private const string BodyEditControlIdToken = "BodyEditControlId";
        private const string BodyEditLeftToken = "BodyEditLeft";
        private const string BodyEditTopToken = "BodyEditTop";
        private const string BodyEditWidthToken = "BodyEditWidth";
        private const string BodyEditHeightToken = "BodyEditHeight";

        private const string FontSizeToken = "FontSize";
        private const string FontNameToken = "FontName";
        private const string FontWeightToken = "FontWeight";
        private const string FontIsItalicToken = "FontIsItalic";
        private const string FontCharSetToken = "FontCharSet";

        private const string IfIsDBTRecordToEnhanceToken = "IfIsDBTRecordToEnhance";
        private const string IfIsDBTRecordNotToEnhanceToken = "IfIsDBTRecordNotToEnhance";
        private const string RepeatOnDocumentsUsingDBTToken = "RepeatOnDocumentsUsingDBT";
        private const string DocumentLibraryRelativePathToken = "DocumentLibraryRelativePath";
        private const string DocumentModuleNameToken = "DocumentModuleName";
        private const string DocumentLibrarySourceFolderToken = "DocumentLibrarySourceFolder";

        private const string DBTRowFormWidthToken = "DBTRowFormWidth";
        private const string DBTRowFormHeightToken = "DBTRowFormHeight";

        private const string IfShowsAdditionalColumnsToken = "IfShowsAdditionalColumns";
        private const string RepeatOnVisibleAdditionalColumnsInfoToken = "RepeatOnVisibleAdditionalColumnsInfo";

        private const string IfIsNotTableReferencedToken = "IfIsNotTableReferenced";
        private const string IfIsTableReferencedToken = "IfIsTableReferenced";
        private const string ReferencedTableHeaderRelativePathToken = "ReferencedTableHeaderRelativePath";

        private const string RepeatOnDBTTabbedPanesToken = "RepeatOnDBTTabbedPanes";

        private WizardDBTInfo dbtInfo = null;
        private WizardDocumentInfo documentInfo = null;
        private WizardClientDocumentInfo clientDocumentInfo = null;
        private System.Drawing.Size dialogSize = Size.Empty;

        internal const ushort FirstColumnControlLeftCoordinate = 8;
        internal const ushort FirstColumnControlTopCoordinate = 6;
        internal const ushort ColumnControlHorizontalSpacing = 10;
        internal const ushort ColumnControlVerticalSpacing = 2;
        internal const ushort ColumnControlDefaultHeight = 10;
        internal const ushort DefaultTabberAreaWidth = 200;
        internal const ushort DefaultTabberAreaHeight = 80;
        internal const ushort BodyEditMinimumWidth = 132;
        internal const ushort BodyEditMinimumHeight = 64;

        //----------------------------------------------------------------------------
        internal WizardDBTCodeTemplateParser(WizardDBTInfo aDBTInfo, WizardDocumentInfo aDocumentInfo)
        {
            dbtInfo = aDBTInfo;
            documentInfo = aDocumentInfo;
            clientDocumentInfo = null;

            InitDialogSize();
        }

        //----------------------------------------------------------------------------
        internal WizardDBTCodeTemplateParser(WizardDBTInfo aDBTInfo, WizardClientDocumentInfo aClientDocumentInfo)
        {
            dbtInfo = aDBTInfo;
            documentInfo = null;
            clientDocumentInfo = aClientDocumentInfo;

            InitDialogSize();
        }

        //----------------------------------------------------------------------------
        internal WizardDBTCodeTemplateParser(WizardDBTInfo aDBTInfo)
            : this(aDBTInfo, (WizardDocumentInfo)null)
        {
        }

        //----------------------------------------------------------------------------
        internal WizardDocumentInfo DocumentInfo { get { return documentInfo; } }

        //----------------------------------------------------------------------------
        internal WizardClientDocumentInfo ClientDocumentInfo { get { return clientDocumentInfo; } }

        //----------------------------------------------------------------------------
        internal System.Drawing.Size DialogSize { get { return dialogSize; } }

        //----------------------------------------------------------------------------
        internal System.Drawing.Font FontToUse
        {
            get
            {
                if (documentInfo != null && documentInfo.Library != null && documentInfo.Library.Application != null)
                    return documentInfo.Library.Application.Font;

                if (clientDocumentInfo != null && clientDocumentInfo.Library != null && clientDocumentInfo.Library.Application != null)
                    return clientDocumentInfo.Library.Application.Font;

                return WizardApplicationInfo.DefaultFont;
            }
        }

        //----------------------------------------------------------------------------
        internal override string GetTokenValue(string aToken)
        {
            if (aToken == null)
                return null;

            aToken = aToken.Trim();
            if (aToken.Length == 0)
                return String.Empty;

            if (String.Compare(aToken, ModuleNameToken) == 0)
                return (dbtInfo != null && dbtInfo.Library != null && dbtInfo.Library.Module != null) ? dbtInfo.Library.Module.Name : String.Empty;

            if (String.Compare(aToken, LibraryNameToken) == 0)
                return (dbtInfo != null && dbtInfo.Library != null) ? dbtInfo.Library.Name : String.Empty;

            if (String.Compare(aToken, LibrarySourceFolderToken) == 0)
                return (dbtInfo != null && dbtInfo.Library != null) ? dbtInfo.Library.SourceFolder : String.Empty;

            if (String.Compare(aToken, DocumentNameToken) == 0)
                return (documentInfo != null) ? documentInfo.Name : String.Empty;

            if (String.Compare(aToken, DocumentClassNameToken) == 0)
                return (documentInfo != null) ? documentInfo.ClassName : String.Empty;

            if (String.Compare(aToken, ClientDocumentNameToken) == 0)
                return (clientDocumentInfo != null) ? clientDocumentInfo.Name : String.Empty;

            if (String.Compare(aToken, ClientDocumentClassNameToken) == 0)
                return (clientDocumentInfo != null) ? clientDocumentInfo.ClassName : String.Empty;

            if (String.Compare(aToken, ClientDocumentViewClassNameToken) == 0)
            {
                if (clientDocumentInfo == null)
                    return String.Empty;
                // se il nome della classe del clientdoc è "CD" + namespace
                // allora imposta il nome come "C" + namespace + "View" (attuale standard)
                // altrimenti usa nome classe + "View" (vecchio standard)
                //if (clientDocumentInfo.ClassName == "CD" + clientDocumentInfo.Name)
                //    return ("C" + clientDocumentInfo.Name) + "View";
                //else
                //    return clientDocumentInfo.ClassName + "View";

                // se il nome della classe inizia con "CD"
                // allora imposta il nome come "CD" + nomeclasse.mid(2) + "View"
                // altrimenti "CD" + namespace + "View"
                if (clientDocumentInfo.ClassName.Substring(0, 2) == "CD")
                    return "C" + clientDocumentInfo.ClassName.Substring(2) + "View";
                else
                    return "C" + clientDocumentInfo.Name + "View";
            }

            if (String.Compare(aToken, ClientDocumentViewFileNameToken) == 0)
            {
                if (clientDocumentInfo == null)
                    return String.Empty;
                // se il nome della classe del clientdoc è "CD" + namespace
                // allora imposta il nome come "C" + namespace + "View" (attuale standard)
                // altrimenti usa nome classe + "View" (vecchio standard)
                //if (clientDocumentInfo.ClassName == "CD" + clientDocumentInfo.Name)
                    //return ("UI" + clientDocumentInfo.Name);
                //else
                //    return clientDocumentInfo.ClassName + "View";

                // se il nome della classe inizia con "CD"
                // allora imposta il nome come "UI" + nomeclasse.mid(2)
                // altrimenti "UI" + namespace
                if (clientDocumentInfo.ClassName.Substring(0, 2) == "CD")
                    return "UI" + clientDocumentInfo.ClassName.Substring(2);
                else
                    return "UI" + clientDocumentInfo.Name;
            }

            if (String.Compare(aToken, ServerDocumentNameToken) == 0)
                return (clientDocumentInfo != null && clientDocumentInfo.ServerDocumentInfo != null) ? clientDocumentInfo.ServerDocumentInfo.Name : String.Empty;

            if (String.Compare(aToken, ServerDocumentClassNameToken) == 0)
                return (clientDocumentInfo != null && clientDocumentInfo.ServerDocumentInfo != null) ? clientDocumentInfo.ServerDocumentInfo.ClassName : String.Empty;

            if (String.Compare(aToken, DBTNameToken) == 0)
                return (dbtInfo != null) ? dbtInfo.Name : String.Empty;

            if (String.Compare(aToken, DBTClassNameToken) == 0)
                return (dbtInfo != null) ? dbtInfo.ClassName : String.Empty;

            if (String.Compare(aToken, DBTSlaveTabTitleToken) == 0)
                return (dbtInfo != null) ? dbtInfo.SlaveTabTitle : String.Empty;

            if (String.Compare(aToken, TableNameToken) == 0)
                return (dbtInfo != null) ? dbtInfo.TableName : String.Empty;

            if (String.Compare(aToken, DBTLibraryRelativePathToken) == 0)
            {
                if (dbtInfo == null || dbtInfo.Library == null)
                    return String.Empty;

                WizardLibraryInfo currentLibrary = null;
                if (documentInfo != null)
                    currentLibrary = documentInfo.Library;
                else if (clientDocumentInfo != null)
                    currentLibrary = clientDocumentInfo.Library;

                if (currentLibrary == null)
                    return String.Empty;

                string relativePath = Generics.MakeRelativeTo(WizardCodeGenerator.GetStandardLibraryPath(currentLibrary), WizardCodeGenerator.GetStandardLibraryPath(dbtInfo.Library));
                if (relativePath != null && relativePath.Length > 0)
                    return relativePath + Path.DirectorySeparatorChar;

                return String.Empty;
            }

            if (String.Compare(aToken, DBTModuleNameToken) == 0)
            {
                if (dbtInfo == null || dbtInfo.Library == null || dbtInfo.Library.Module == null)
                    return String.Empty;

                return dbtInfo.Library.Module.Name;
            }

            if (String.Compare(aToken, DBTLibrarySourceFolderToken) == 0)
            {
                if (dbtInfo == null || dbtInfo.Library == null)
                    return String.Empty;

                return dbtInfo.Library.SourceFolder;
            }


            if (String.Compare(aToken, TableClassNameToken) == 0)
            {
                if (dbtInfo == null)
                    return String.Empty;

                WizardTableInfo tableInfo = dbtInfo.GetTableInfo();
                return (tableInfo != null) ? tableInfo.ClassName : String.Empty;
            }

            if (String.Compare(aToken, TableLibraryRelativePathToken) == 0)
            {
                if (dbtInfo == null)
                    return String.Empty;

                WizardTableInfo tableInfo = dbtInfo.GetTableInfo();
                if (tableInfo == null || tableInfo.Library == null)
                    return String.Empty;

                WizardLibraryInfo currentLibrary = null;
                if (documentInfo != null)
                    currentLibrary = documentInfo.Library;
                else if (clientDocumentInfo != null)
                    currentLibrary = clientDocumentInfo.Library;

                string relativePath = String.Empty;

                if (currentLibrary != null)
                    relativePath = Generics.MakeRelativeTo(WizardCodeGenerator.GetStandardLibraryPath(currentLibrary), WizardCodeGenerator.GetStandardLibraryPath(tableInfo.Library));
                else if (dbtInfo.Library != null)
                    relativePath = Generics.MakeRelativeTo(WizardCodeGenerator.GetStandardLibraryPath(dbtInfo.Library), WizardCodeGenerator.GetStandardLibraryPath(tableInfo.Library));
                if (relativePath != null && relativePath.Length > 0)
                    return relativePath + Path.DirectorySeparatorChar;

                return String.Empty;
            }

            if (String.Compare(aToken, TableModuleNameToken) == 0)
            {
                if (dbtInfo == null)
                    return String.Empty;

                WizardTableInfo tableInfo = dbtInfo.GetTableInfo();
                if (tableInfo == null || tableInfo.Library == null || tableInfo.Library.Module == null)
                    return String.Empty;

                return tableInfo.Library.Module.Name;
            }

            if (String.Compare(aToken, TableLibrarySourceFolderToken) == 0)
            {
                if (dbtInfo == null)
                    return String.Empty;

                WizardTableInfo tableInfo = dbtInfo.GetTableInfo();
                if (tableInfo == null || tableInfo.Library == null)
                    return String.Empty;

                return tableInfo.Library.SourceFolder;
            }

            if (aToken.StartsWith(IfDBTMasterToken))
            {
                if (dbtInfo == null || !dbtInfo.IsMaster)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfDBTMasterToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfNotDBTMasterToken))
            {
                if (dbtInfo == null || dbtInfo.IsMaster)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfNotDBTMasterToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfDBTSlaveBufferedToken))
            {
                if (dbtInfo == null || !dbtInfo.IsSlaveBuffered)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfDBTSlaveBufferedToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfNotDBTSlaveBufferedToken))
            {
                if (dbtInfo == null || dbtInfo.IsSlaveBuffered)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfNotDBTSlaveBufferedToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfCreateRowFormToken))
            {
                if (dbtInfo == null || !dbtInfo.CreateRowForm)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfCreateRowFormToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfNotCreateRowFormToken))
            {
                if (dbtInfo == null || dbtInfo.CreateRowForm)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfNotCreateRowFormToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(RepeatOnColumnsToken))
            {
                if (dbtInfo == null || dbtInfo.ColumnsCount == 0)
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnColumnsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string columnsLines = String.Empty;
                    foreach (WizardDBTColumnInfo aColumnInfo in dbtInfo.ColumnsInfo)
                    {
                        string columnParsedText = SubstituteDBTColumnTokens(subToken, aColumnInfo);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }
                    }
                    return columnsLines;
                }
            }

            if (aToken.StartsWith(RepeatOnPrimaryKeySegmentsToken))
            {
                if (dbtInfo == null || dbtInfo.ColumnsCount == 0)
                    return String.Empty;

                WizardTableInfo tableInfo = dbtInfo.GetTableInfo();
                if (tableInfo == null || !tableInfo.IsPrimaryKeyDefined)
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnPrimaryKeySegmentsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string columnsLines = String.Empty;
                    foreach (WizardDBTColumnInfo aColumnInfo in dbtInfo.ColumnsInfo)
                    {
                        if (!tableInfo.IsPrimaryKeySegment(aColumnInfo.ColumnName))
                            continue;

                        string columnParsedText = SubstituteDBTColumnTokens(subToken, aColumnInfo);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }
                    }
                    return columnsLines;
                }
            }

			if (aToken.StartsWith(RepeatOnLabelsToken))
			{
				return "HOOOOOOOOOOOOO!";
				if (documentInfo == null)
					return string.Empty;
			}
			
            if (aToken.StartsWith(RepeatOnVisibleColumnsToken))
            {
                if (dbtInfo == null || !dbtInfo.HasVisibleColums())
                    return String.Empty;

                WizardTableInfo tableInfo = dbtInfo.GetTableInfo();
                if (tableInfo == null)
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnVisibleColumnsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    ushort columnControlId = Generics.FirstValidControlId;
                    if (documentInfo != null)
                        columnControlId = (ushort)documentInfo.GetFirstDBTControlId(dbtInfo);
                    else if (clientDocumentInfo != null)
                        columnControlId = (ushort)clientDocumentInfo.GetFirstDBTControlId(dbtInfo);

                    string columnsLines = String.Empty;
                    int columnControlsTopCoordinate = FirstColumnControlTopCoordinate;
                    System.Drawing.Font fontToUse = this.FontToUse;

                    foreach (WizardDBTColumnInfo aColumnInfo in dbtInfo.ColumnsInfo)
                    {
                        if (!aColumnInfo.Visible)
                            continue;

                        string columnParsedText = SubstituteDBTColumnTokens(subToken, aColumnInfo, columnControlId, columnControlsTopCoordinate);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }

                        columnControlId++;
                        if (aColumnInfo.ShowHotKeyLinkDescription)
                            columnControlId++;

                        columnControlsTopCoordinate += GetColumnDialogLineHeight(aColumnInfo, fontToUse) + ColumnControlVerticalSpacing;
                    }
                    return columnsLines;
                }
            }

            if (aToken.StartsWith(RepeatOnFindableColumnsToken))
            {
                if (dbtInfo == null || !dbtInfo.HasFindableColums())
                    return String.Empty;

                WizardTableInfo tableInfo = dbtInfo.GetTableInfo();
                if (tableInfo == null)
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnFindableColumnsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string columnsLines = String.Empty;
                    foreach (WizardDBTColumnInfo aColumnInfo in dbtInfo.ColumnsInfo)
                    {
                        if (!aColumnInfo.Findable)
                            continue;

                        string columnParsedText = SubstituteDBTColumnTokens(subToken, aColumnInfo);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }
                    }
                    return columnsLines;
                }
            }

            if (aToken.StartsWith(RepeatOnRowFormColumnsToken))
            {
                if (dbtInfo == null || !dbtInfo.CreateRowForm)
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnRowFormColumnsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    ushort columnControlId = Generics.FirstValidControlId;
                    if (documentInfo != null)
                        columnControlId = (ushort)documentInfo.GetFirstBodyEditRowFormControlId(dbtInfo);
                    else if (clientDocumentInfo != null)
                        columnControlId = (ushort)clientDocumentInfo.GetFirstBodyEditRowFormControlId(dbtInfo);

                    string columnsLines = String.Empty;
                    int columnControlsTopCoordinate = FirstColumnControlTopCoordinate;
                    System.Drawing.Font fontToUse = this.FontToUse;

                    foreach (WizardDBTColumnInfo aColumnInfo in dbtInfo.ColumnsInfo)
                    {
                        // Nelle RowFormView dei BodyEdit vengono gestiti tutti i campi 
                        // della tabella alla quale è riferito il DBT (compresi quelli 
                        // che non sono visibili, cioè per i quali non è stata inserita
                        // alcuna colonna corrispondente nel BodyEdit), ma vengono comunque
                        // scartati i segmenti di chiave esterna utilizzati da questo DBT
                        // SlaveBuffered per "agganciarsi" alla tabella master
                        if (aColumnInfo.ForeignKeySegment)
                            continue;

                        WizardDBTColumnInfo aTmpDBTColumnInfo = new WizardDBTColumnInfo(aColumnInfo);
                        aTmpDBTColumnInfo.Visible = true;
                        if (aTmpDBTColumnInfo.Title == null || aTmpDBTColumnInfo.Title.Length == 0)
                            aTmpDBTColumnInfo.Title = aColumnInfo.ColumnName;
                        aTmpDBTColumnInfo.HotKeyLink = null;

                        string columnParsedText = SubstituteDBTColumnTokens(subToken, aTmpDBTColumnInfo, columnControlId, columnControlsTopCoordinate);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }

                        columnControlId++;

                        columnControlsTopCoordinate += GetColumnDialogLineHeight(aTmpDBTColumnInfo, fontToUse) + ColumnControlVerticalSpacing;
                    }
                    return columnsLines;
                }
            }

            if (String.Compare(aToken, MasterTableHeaderRelativePathToken) == 0)
            {
                if (dbtInfo == null || !(dbtInfo.IsSlave || dbtInfo.IsSlaveBuffered))
                    return String.Empty;

                string masterTableIncludeFile = dbtInfo.MasterTableIncludeFile;
                if (masterTableIncludeFile == null || masterTableIncludeFile.Trim().Length == 0)
                    return String.Empty;

                WizardLibraryInfo currentLibrary = null;
                if (documentInfo != null)
                    currentLibrary = documentInfo.Library;
                else if (clientDocumentInfo != null)
                    currentLibrary = clientDocumentInfo.Library;

                string relativePath = String.Empty;
                if (currentLibrary != null)
                    relativePath = Generics.MakeRelativeTo(WizardCodeGenerator.GetStandardLibraryPath(currentLibrary), masterTableIncludeFile);
                else if (dbtInfo.Library != null)
                    relativePath = Generics.MakeRelativeTo(WizardCodeGenerator.GetStandardLibraryPath(dbtInfo.Library), masterTableIncludeFile);

                return relativePath;
            }

            if (String.Compare(aToken, MasterTableNameToken) == 0)
            {
                if (dbtInfo == null || !(dbtInfo.IsSlave || dbtInfo.IsSlaveBuffered))
                    return String.Empty;

                WizardTableInfo masterTableInfo = dbtInfo.GetRelatedTableInfo();
                return (masterTableInfo != null) ? masterTableInfo.Name : String.Empty;
            }

            if (String.Compare(aToken, MasterTableClassNameToken) == 0)
            {
                if (dbtInfo == null || !(dbtInfo.IsSlave || dbtInfo.IsSlaveBuffered))
                    return String.Empty;

                WizardTableInfo masterTableInfo = dbtInfo.GetRelatedTableInfo();
                return (masterTableInfo != null) ? masterTableInfo.ClassName : String.Empty;
            }

            if (String.Compare(aToken, FirstForeignKeySegmentNameToken) == 0)
            {
                if (dbtInfo == null || dbtInfo.ColumnsCount == 0 || !(dbtInfo.IsSlave || dbtInfo.IsSlaveBuffered))
                    return String.Empty;

                foreach (WizardDBTColumnInfo aColumnInfo in dbtInfo.ColumnsInfo)
                {
                    if (aColumnInfo.ForeignKeySegment && aColumnInfo.TableColumn != null)
                        return aColumnInfo.TableColumn.Name;
                }
            }

            if (aToken.StartsWith(RepeatOnForeignKeySegmentsToken))
            {
                if (dbtInfo == null || dbtInfo.ColumnsCount == 0 || !(dbtInfo.IsSlave || dbtInfo.IsSlaveBuffered))
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnForeignKeySegmentsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string columnsLines = String.Empty;
                    foreach (WizardDBTColumnInfo aColumnInfo in dbtInfo.ColumnsInfo)
                    {
                        if (!aColumnInfo.ForeignKeySegment)
                            continue;

                        string columnParsedText = SubstituteDBTColumnTokens(subToken, aColumnInfo);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }
                    }
                    return columnsLines;
                }
            }

            if (String.Compare(aToken, DBTDialogWidthToken) == 0)
            {
                if (dbtInfo != null && dbtInfo.IsSlaveBuffered)
                {
                    if (documentInfo == null && clientDocumentInfo == null)
                        return String.Empty;

                    Size documentTabberAreaSize = Size.Empty;

                    if (documentInfo != null)
                        documentTabberAreaSize = WizardDocumentCodeTemplateParser.GetTabberAreaSize(documentInfo, this.FontToUse);
                    else if (clientDocumentInfo != null)
                        documentTabberAreaSize = WizardClientDocumentCodeTemplateParser.GetTabberAreaSize(clientDocumentInfo, this.FontToUse);

                    return (documentTabberAreaSize != Size.Empty) ? documentTabberAreaSize.Width.ToString() : String.Empty;

                }

                return (dialogSize != Size.Empty) ? dialogSize.Width.ToString() : String.Empty;
            }

            if (String.Compare(aToken, DBTDialogHeightToken) == 0)
            {
                if (dbtInfo != null && dbtInfo.IsSlaveBuffered)
                {
                    if (documentInfo == null && clientDocumentInfo == null)
                        return String.Empty;

                    Size documentTabberAreaSize = Size.Empty;

                    if (documentInfo != null)
                        documentTabberAreaSize = WizardDocumentCodeTemplateParser.GetTabberAreaSize(documentInfo, this.FontToUse);
                    else if (clientDocumentInfo != null)
                        documentTabberAreaSize = WizardClientDocumentCodeTemplateParser.GetTabberAreaSize(clientDocumentInfo, this.FontToUse);

                    return (documentTabberAreaSize != Size.Empty) ? documentTabberAreaSize.Height.ToString() : String.Empty;
                }

                return (dialogSize != Size.Empty) ? dialogSize.Height.ToString() : String.Empty;
            }

            if (String.Compare(aToken, BodyEditControlIdToken) == 0)
            {
                if (dbtInfo == null || !dbtInfo.IsSlaveBuffered)
                    return String.Empty;

                if (documentInfo != null)
                    return documentInfo.GetBodyEditControlId(dbtInfo).ToString();
                if (clientDocumentInfo != null)
                    return clientDocumentInfo.GetBodyEditControlId(dbtInfo).ToString();

                return String.Empty;
            }

            if (String.Compare(aToken, BodyEditLeftToken) == 0)
            {
                if (dbtInfo == null || !dbtInfo.IsSlaveBuffered || (documentInfo == null && clientDocumentInfo == null))
                    return String.Empty;
				if (dbtInfo.BodyEditPosition.isSet)
					return dbtInfo.BodyEditPosition.Left.ToString();
				dbtInfo.BodyEditPosition.Left = FirstColumnControlLeftCoordinate;
                return FirstColumnControlLeftCoordinate.ToString();
            }

            if (String.Compare(aToken, BodyEditTopToken) == 0)
            {
                if (dbtInfo == null || !dbtInfo.IsSlaveBuffered || (documentInfo == null && clientDocumentInfo == null))
                    return String.Empty;
				if (dbtInfo.BodyEditPosition.isSet)
					return dbtInfo.BodyEditPosition.Top.ToString();
				dbtInfo.BodyEditPosition.Top = FirstColumnControlTopCoordinate;
                return FirstColumnControlTopCoordinate.ToString();
            }

            if (String.Compare(aToken, BodyEditWidthToken) == 0)
            {
                if (dbtInfo == null || !dbtInfo.IsSlaveBuffered || (documentInfo == null && clientDocumentInfo == null))
                    return String.Empty;

                Size documentTabberAreaSize = Size.Empty;

                if (documentInfo != null)
                    documentTabberAreaSize = WizardDocumentCodeTemplateParser.GetTabberAreaSize(documentInfo, this.FontToUse);
                else if (clientDocumentInfo != null)
                    documentTabberAreaSize = WizardClientDocumentCodeTemplateParser.GetTabberAreaSize(clientDocumentInfo, this.FontToUse);
				if (dbtInfo.BodyEditPosition.isSet)
					return dbtInfo.BodyEditPosition.Width.ToString();
				dbtInfo.BodyEditPosition.Width = (documentTabberAreaSize != Size.Empty) ? (documentTabberAreaSize.Width - (2 * FirstColumnControlLeftCoordinate)) : 0;
                return (documentTabberAreaSize != Size.Empty) ? (documentTabberAreaSize.Width - (2 * FirstColumnControlLeftCoordinate)).ToString() : String.Empty;
            }

            if (String.Compare(aToken, BodyEditHeightToken) == 0)
            {
                if (dbtInfo == null || !dbtInfo.IsSlaveBuffered || (documentInfo == null && clientDocumentInfo == null))
                    return String.Empty;

                Size documentTabberAreaSize = Size.Empty;

                if (documentInfo != null)
                    documentTabberAreaSize = WizardDocumentCodeTemplateParser.GetTabberAreaSize(documentInfo, this.FontToUse);
                else if (clientDocumentInfo != null)
                    documentTabberAreaSize = WizardClientDocumentCodeTemplateParser.GetTabberAreaSize(clientDocumentInfo, this.FontToUse);
				if (dbtInfo.BodyEditPosition.isSet)
					return dbtInfo.BodyEditPosition.Height.ToString();
				dbtInfo.BodyEditPosition.Height = (documentTabberAreaSize != Size.Empty) ? (documentTabberAreaSize.Height - (2 * FirstColumnControlTopCoordinate)) : 0;
                return (documentTabberAreaSize != Size.Empty) ? (documentTabberAreaSize.Height - (2 * FirstColumnControlTopCoordinate)).ToString() : String.Empty;
            }

            if (String.Compare(aToken, FontSizeToken) == 0)
            {
                return this.FontToUse.SizeInPoints.ToString(NumberFormatInfo.InvariantInfo);
            }

            if (String.Compare(aToken, FontNameToken) == 0)
            {
                return this.FontToUse.Name;
            }

            if (String.Compare(aToken, FontWeightToken) == 0)
            {
                return Generics.GetFontWeight(this.FontToUse).ToString();
            }

            if (String.Compare(aToken, FontIsItalicToken) == 0)
            {
                return this.FontToUse.Italic ? "1" : "0";
            }

            if (String.Compare(aToken, FontCharSetToken) == 0)
            {
                return Generics.GetFontCharSet(this.FontToUse).ToString();
            }

            if (aToken.StartsWith(RepeatOnHotLinkColumnsToken))
            {
                if (dbtInfo == null || dbtInfo.ColumnsCount == 0 || !dbtInfo.HasHKLDefinedColumns())
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnHotLinkColumnsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string columnsLines = String.Empty;
                    foreach (WizardDBTColumnInfo aColumnInfo in dbtInfo.ColumnsInfo)
                    {
                        if (!aColumnInfo.IsHKLDefined)
                            continue;

                        string columnParsedText = SubstituteDBTColumnTokens(subToken, aColumnInfo);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }
                    }
                    return columnsLines;
                }
            }

            if (aToken.StartsWith(RepeatOnApplicationHotLinkColumnsToken))
            {
                if (dbtInfo == null || dbtInfo.ColumnsCount == 0 || !dbtInfo.HasHKLDefinedColumns())
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnApplicationHotLinkColumnsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string columnsLines = String.Empty;
                    foreach (WizardDBTColumnInfo aColumnInfo in dbtInfo.ColumnsInfo)
                    {
                        // Se l'Hotlink non fa parte delle definizioni di progetto (cioè
                        // se appartiene ad una libreria di un'applicazione referenziata
                        // anzichè all'applicazione del progetto, lo salto
                        if (!aColumnInfo.IsHKLDefined || aColumnInfo.HotKeyLink.IsReferenced)
                            continue;

                        string columnParsedText = SubstituteDBTColumnTokens(subToken, aColumnInfo);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }
                    }
                    return columnsLines;
                }
            }

            if (aToken.StartsWith(RepeatOnReferencedHotLinkIncludeFilesToken))
            {
                if (dbtInfo == null || dbtInfo.ColumnsCount == 0 || !dbtInfo.HasHKLDefinedColumns())
                    return String.Empty;

                string subToken;
                string includeFilesLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnReferencedHotLinkIncludeFilesToken, out subToken, out includeFilesLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string includeFilesLines = String.Empty;
                    foreach (WizardDBTColumnInfo aColumnInfo in dbtInfo.ColumnsInfo)
                    {
                        if (!aColumnInfo.IsHKLDefined || !aColumnInfo.HotKeyLink.IsReferenced)
                            continue;

                        string includeFileName = aColumnInfo.HotKeyLink.ExternalIncludeFile;
                        if (includeFileName == null || includeFileName.Length == 0)
                            continue;

                        WizardCodeFileTemplateParser aCodeFileParser = new WizardCodeFileTemplateParser(dbtInfo.Library, includeFileName);
                        string includeFilesParsedText = aCodeFileParser.SubstituteTokens(subToken);

                        if (includeFilesParsedText != null && includeFilesParsedText.Length > 0)
                        {
                            if (includeFilesLines.Length > 0)
                                includeFilesLines += includeFilesLinesSeparator;
                            includeFilesLines += includeFilesParsedText;
                        }
                    }
                    return includeFilesLines;
                }
            }

            if (aToken.StartsWith(IfIsDBTRecordToEnhanceToken))
            {
                if (dbtInfo == null || !dbtInfo.IsSlaveBuffered || !dbtInfo.HasHotLinkColumnsWithDescription())
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfIsDBTRecordToEnhanceToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfIsDBTRecordNotToEnhanceToken))
            {
                if (dbtInfo == null || (dbtInfo.IsSlaveBuffered && dbtInfo.HasHotLinkColumnsWithDescription()))
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfIsDBTRecordNotToEnhanceToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(RepeatOnDocumentsUsingDBTToken))
            {
                if
                    (
                    dbtInfo == null ||
                    dbtInfo.Library == null ||
                    dbtInfo.Library.Application == null
                    )
                    return String.Empty;

                WizardDocumentInfoCollection documentsFound = dbtInfo.Library.Application.GetDocumentsUsingDBT(dbtInfo);
                if (documentsFound == null || documentsFound.Count == 0)
                    return String.Empty;

                string subToken;
                string documentLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnDocumentsUsingDBTToken, out subToken, out documentLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string documentsLines = String.Empty;
                    foreach (WizardDocumentInfo aDocumentInfo in documentsFound)
                    {
                        string documentParsedText = String.Empty;

                        WizardDocumentCodeTemplateParser documentParser = new WizardDocumentCodeTemplateParser(aDocumentInfo);
                        documentParsedText = documentParser.SubstituteTokens(subToken, true);

                        if (AreTokensToSubstitute(documentParsedText))
                        {
                            WizardDBTCodeTemplateParser dbtParser = new WizardDBTCodeTemplateParser(dbtInfo, aDocumentInfo);
                            documentParsedText = dbtParser.SubstituteTokens(documentParsedText);
                        }

                        if (documentParsedText != null && documentParsedText.Length > 0)
                        {
                            if (documentsLines.Length > 0)
                                documentsLines += documentLinesSeparator;
                            documentsLines += documentParsedText;
                        }
                    }
                    return documentsLines;
                }
            }

            if (String.Compare(aToken, DocumentLibraryRelativePathToken) == 0)
            {
                if (dbtInfo == null || dbtInfo.Library == null || documentInfo == null || documentInfo.Library == null)
                    return String.Empty;

                string relativePath = Generics.MakeRelativeTo(WizardCodeGenerator.GetStandardLibraryPath(dbtInfo.Library), WizardCodeGenerator.GetStandardLibraryPath(documentInfo.Library));
                if (relativePath != null && relativePath.Length > 0)
                    return relativePath + Path.DirectorySeparatorChar;
            }

            if (String.Compare(aToken, DocumentModuleNameToken) == 0)
            {
                if (dbtInfo == null || dbtInfo.Library == null || documentInfo == null || documentInfo.Library == null || documentInfo.Library.Module == null)
                    return String.Empty;

                return documentInfo.Library.Module.Name;
            }

            if (String.Compare(aToken, DocumentLibrarySourceFolderToken) == 0)
            {
                if (dbtInfo == null || dbtInfo.Library == null || documentInfo == null || documentInfo.Library == null)
                    return String.Empty;

                return documentInfo.Library.SourceFolder;
            }

            if (String.Compare(aToken, DBTRowFormWidthToken) == 0)
            {
                if (dbtInfo == null || dbtInfo.ColumnsCount == 0 || !dbtInfo.CreateRowForm)
                    return String.Empty;

                System.Drawing.Size rowFormDialogSize = WizardDBTCodeTemplateParser.GetDBTSlaveBufferedRowFormSize(dbtInfo, this.FontToUse);
                if (rowFormDialogSize == Size.Empty)
                    return String.Empty;

                return rowFormDialogSize.Width.ToString();
            }

            if (String.Compare(aToken, DBTRowFormHeightToken) == 0)
            {
                if (dbtInfo == null || dbtInfo.ColumnsCount == 0 || !dbtInfo.CreateRowForm)
                    return String.Empty;

                System.Drawing.Size rowFormDialogSize = WizardDBTCodeTemplateParser.GetDBTSlaveBufferedRowFormSize(dbtInfo, this.FontToUse);
                if (rowFormDialogSize == Size.Empty)
                    return String.Empty;

                return rowFormDialogSize.Height.ToString();
            }

            if (aToken.StartsWith(IfShowsAdditionalColumnsToken))
            {
                if (dbtInfo == null || !dbtInfo.ShowsAdditionalColumns)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfShowsAdditionalColumnsToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(RepeatOnVisibleAdditionalColumnsInfoToken))
            {
                if
                    (
                    dbtInfo == null ||
                    dbtInfo.Library == null ||
                    dbtInfo.Library.Application == null
                    )
                    return String.Empty;

                WizardTableInfo tableInfo = dbtInfo.GetTableInfo();
                if (tableInfo == null)
                    return String.Empty;

                WizardExtraAddedColumnsInfoCollection additionalColumnsInfo = dbtInfo.Library.GetAllAvailableTableAdditionalColumnsInfo(tableInfo.GetNameSpace());
                if (additionalColumnsInfo == null || additionalColumnsInfo.Count == 0)
                    return String.Empty;

                string subToken;
                string additionalColumnsInfoLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnVisibleAdditionalColumnsInfoToken, out subToken, out additionalColumnsInfoLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string additionalColumnsInfoLines = String.Empty;
                    foreach (WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo in additionalColumnsInfo)
                    {
                        if (!dbtInfo.HasVisibleAdditionalColumns(aExtraAddedColumnsInfo))
                            continue;

                        string additionalColumnsInfoParsedText = String.Empty;

                        WizardAdditionalColumnsCodeTemplateParser additionalColumnsParser = null;
                        if (clientDocumentInfo != null)
                            additionalColumnsParser = new WizardAdditionalColumnsCodeTemplateParser(aExtraAddedColumnsInfo, dbtInfo, clientDocumentInfo);
                        else
                            additionalColumnsParser = new WizardAdditionalColumnsCodeTemplateParser(aExtraAddedColumnsInfo, dbtInfo, documentInfo);

                        additionalColumnsInfoParsedText = additionalColumnsParser.SubstituteTokens(subToken, true);

                        if (additionalColumnsInfoParsedText != null && additionalColumnsInfoParsedText.Length > 0)
                        {
                            if (additionalColumnsInfoLines.Length > 0)
                                additionalColumnsInfoLines += additionalColumnsInfoLinesSeparator;
                            additionalColumnsInfoLines += additionalColumnsInfoParsedText;
                        }
                    }
                    return additionalColumnsInfoLines;
                }
            }

            if (aToken.StartsWith(IfIsNotTableReferencedToken))
            {
                if (dbtInfo == null)
                    return String.Empty;

                WizardTableInfo tableInfo = dbtInfo.GetTableInfo();

                if (tableInfo == null || tableInfo.IsReferenced)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfIsNotTableReferencedToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken, true);
                }
            }

            if (aToken.StartsWith(IfIsTableReferencedToken))
            {
                if (dbtInfo == null)
                    return String.Empty;

                WizardTableInfo tableInfo = dbtInfo.GetTableInfo();

                if (tableInfo == null || !tableInfo.IsReferenced)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfIsTableReferencedToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken, true);
                }
            }

            if (String.Compare(aToken, ReferencedTableHeaderRelativePathToken) == 0)
            {
                if (dbtInfo == null)
                    return String.Empty;

                WizardTableInfo tableInfo = dbtInfo.GetTableInfo();

                if (tableInfo == null || !tableInfo.IsReferenced)
                    return String.Empty;

                string referencedTableIncludeFile = dbtInfo.ReferencedTableIncludeFile;
                if (referencedTableIncludeFile == null || referencedTableIncludeFile.Trim().Length == 0)
                    return String.Empty;

                WizardLibraryInfo currentLibrary = null;
                if (documentInfo != null)
                    currentLibrary = documentInfo.Library;
                else if (clientDocumentInfo != null)
                    currentLibrary = clientDocumentInfo.Library;

                string relativePath = String.Empty;
                if (currentLibrary != null)
                    relativePath = Generics.MakeRelativeTo(WizardCodeGenerator.GetStandardLibraryPath(currentLibrary), referencedTableIncludeFile);
                else if (dbtInfo.Library != null)
                    relativePath = Generics.MakeRelativeTo(WizardCodeGenerator.GetStandardLibraryPath(dbtInfo.Library), referencedTableIncludeFile);

                return relativePath;
            }

            if (aToken.StartsWith(RepeatOnDBTTabbedPanesToken))
            {
                if
                    (
                    dbtInfo == null ||
                    (documentInfo == null && clientDocumentInfo == null)
                    )
                    return String.Empty;

                WizardDocumentTabbedPaneInfoCollection tabbedPanes = null;
                if (documentInfo != null)
                    tabbedPanes = documentInfo.GetAllDBTTabbedPanes(dbtInfo);
                else if (clientDocumentInfo != null)
                    tabbedPanes = clientDocumentInfo.GetAllDBTTabbedPanes(dbtInfo);

                if (tabbedPanes == null || tabbedPanes.Count == 0)
                    return String.Empty;

                string subToken;
                string tabbedPanesLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnDBTTabbedPanesToken, out subToken, out tabbedPanesLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string tabbedPanesLines = String.Empty;
                    foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in tabbedPanes)
                    {
                        string tabbedPaneParsedText = String.Empty;

                        WizardTabbedPaneCodeTemplateParser tabbedPaneParser = null;
                        if (documentInfo != null)
                            tabbedPaneParser = new WizardTabbedPaneCodeTemplateParser(aTabbedPaneInfo, documentInfo);
                        else if (clientDocumentInfo != null)
                            tabbedPaneParser = new WizardTabbedPaneCodeTemplateParser(aTabbedPaneInfo, clientDocumentInfo);

                        tabbedPaneParsedText = tabbedPaneParser.SubstituteTokens(subToken, true);

                        if (tabbedPaneParsedText != null && tabbedPaneParsedText.Length > 0)
                        {
                            if (tabbedPanesLines.Length > 0)
                                tabbedPanesLines += tabbedPanesLinesSeparator;
                            tabbedPanesLines += tabbedPaneParsedText;
                        }
                    }
                    return tabbedPanesLines;
                }
            }

            return null;
        }

        //----------------------------------------------------------------------------
        private string SubstituteDBTColumnTokens
            (
            string source,
            WizardDBTColumnInfo aColumnInfo,
            ushort aControlId,
            int aYCoordinate,
            bool preserveUnresolvedTokens
            )
        {
            if (source == null || aColumnInfo == null)
                return null;

            source = source.Trim();
            if (source.Length == 0)
                return String.Empty;

            string columnParsedText = this.SubstituteTokens(source, true);

            if (AreTokensToSubstitute(columnParsedText))
            {
                WizardDBTColumnCodeTemplateParser columnParser = null;

                if (documentInfo != null)
                    columnParser = new WizardDBTColumnCodeTemplateParser(aColumnInfo, this.dbtInfo, documentInfo, aControlId, aYCoordinate);
                else if (clientDocumentInfo != null)
                    columnParser = new WizardDBTColumnCodeTemplateParser(aColumnInfo, this.dbtInfo, clientDocumentInfo, aControlId, aYCoordinate);
                else
                    columnParser = new WizardDBTColumnCodeTemplateParser(aColumnInfo, this.dbtInfo, aControlId, aYCoordinate);

                columnParsedText = columnParser.SubstituteTokens(columnParsedText, true);

                if (AreTokensToSubstitute(columnParsedText))
                {
                    int columnIndex = -1;
                    WizardTableColumnInfo tableColumnInfo = dbtInfo.GetTableColumnInfoByName(aColumnInfo.ColumnName, ref columnIndex);
                    if (tableColumnInfo != null)
                    {
                        WizardTableColumnCodeTemplateParser tableColumnParser = new WizardTableColumnCodeTemplateParser(tableColumnInfo, columnIndex, dbtInfo.GetTableInfo());

                        columnParsedText = tableColumnParser.SubstituteTokens(columnParsedText, preserveUnresolvedTokens);
                    }
                }
            }
            return columnParsedText;
        }

        //----------------------------------------------------------------------------
        private string SubstituteDBTColumnTokens(string source, WizardDBTColumnInfo aColumnInfo, ushort aControlId, int aYCoordinate)
        {
            return SubstituteDBTColumnTokens(source, aColumnInfo, aControlId, aYCoordinate, false);
        }

        //----------------------------------------------------------------------------
        private string SubstituteDBTColumnTokens(string source, WizardDBTColumnInfo aColumnInfo)
        {
            return SubstituteDBTColumnTokens(source, aColumnInfo, 0, 0, false);
        }

        //----------------------------------------------------------------------------
        internal static System.Drawing.Size GetSlaveBufferedDialogMinimumSize()
        {
            return new Size(BodyEditMinimumWidth + 2 * FirstColumnControlLeftCoordinate, BodyEditMinimumHeight + 2 * FirstColumnControlTopCoordinate);
        }

        //----------------------------------------------------------------------------
        internal static System.Drawing.Size GetDialogSize(WizardDBTInfo aDbtInfo, System.Drawing.Font aFont)
        {
            if (aDbtInfo == null || aDbtInfo.ColumnsCount == 0 || !aDbtInfo.HasVisibleColums())
                return Size.Empty;

            if (aDbtInfo.IsSlaveBuffered)
                return GetSlaveBufferedDialogMinimumSize();

            if (aFont == null || aFont.FontFamily == null)
                aFont = WizardApplicationInfo.DefaultFont;

            int maxColumnLineWidth = 0;
            int dialogHeight = FirstColumnControlTopCoordinate;

            foreach (WizardDBTColumnInfo aColumnInfo in aDbtInfo.ColumnsInfo)
            {
                if (!aColumnInfo.Visible)
                    continue;

                Size columnLineSize = GetColumnDialogLineSize(aColumnInfo, aFont);
                if (columnLineSize == Size.Empty)
                    continue;

                int columnLineWidth = columnLineSize.Width + (2 * FirstColumnControlLeftCoordinate);

                if (maxColumnLineWidth < columnLineWidth)
                    maxColumnLineWidth = columnLineWidth;

                dialogHeight += columnLineSize.Height + ColumnControlVerticalSpacing;
            }
            return (maxColumnLineWidth > 0) ? new Size(maxColumnLineWidth, dialogHeight) : Size.Empty;
        }

        //----------------------------------------------------------------------------
        internal static System.Drawing.Size GetColumnDialogLineSize(WizardDBTColumnInfo aColumnInfo, System.Drawing.Font aFont)
        {
            if (aColumnInfo == null || !aColumnInfo.Visible)
                return Size.Empty;

            int columnLineWidth = 0;
            Size labelSize = Size.Empty;
            if (aColumnInfo.IsLabelVisible && aColumnInfo.Title != null && aColumnInfo.Title.Length > 0)
            {
                labelSize = Generics.GDI32.GetTextDisplaySize(aColumnInfo.Title, aFont.FontFamily.Name, aFont.SizeInPoints);
                columnLineWidth = labelSize.Width;
            }
            Size controlSize = aColumnInfo.GetColumnControlDefaultSize(aFont.FontFamily.Name, aFont.SizeInPoints);

            if (columnLineWidth > 0)
                columnLineWidth += ColumnControlHorizontalSpacing;

            columnLineWidth += controlSize.Width;

            if (aColumnInfo.ShowHotKeyLinkDescription)
            {
                WizardTableColumnInfo hotLinkDescriptionColumnInfo = aColumnInfo.HotKeyLink.DescriptionColumn;
                if (hotLinkDescriptionColumnInfo != null)
                {
                    Size hotLinkDescriptionControlSize = WizardDBTColumnInfo.GetStringColumnControlDefaultSize(hotLinkDescriptionColumnInfo, aFont.FontFamily.Name, aFont.SizeInPoints);
                    columnLineWidth += ColumnControlHorizontalSpacing;
                    columnLineWidth += hotLinkDescriptionControlSize.Width;

                    if (controlSize.Height < hotLinkDescriptionControlSize.Height)
                        controlSize.Height = hotLinkDescriptionControlSize.Height;
                }
            }

            return new Size(columnLineWidth, Math.Max(labelSize.Height, controlSize.Height));
        }

        //----------------------------------------------------------------------------
        internal static int GetColumnDialogLineHeight(WizardDBTColumnInfo aColumnInfo, System.Drawing.Font aFont)
        {
            return GetColumnDialogLineSize(aColumnInfo, aFont).Height;
        }

        //----------------------------------------------------------------------------
        internal static System.Drawing.Size GetDBTSlaveBufferedRowFormSize(WizardDBTInfo aDbtInfo, System.Drawing.Font aFont)
        {
            if (aDbtInfo == null || aDbtInfo.ColumnsCount == 0 || !aDbtInfo.CreateRowForm)
                return Size.Empty;

            if (aFont == null || aFont.FontFamily == null)
                aFont = WizardApplicationInfo.DefaultFont;

            int maxColumnLineWidth = 0;
            int dialogHeight = FirstColumnControlTopCoordinate;

            foreach (WizardDBTColumnInfo aColumnInfo in aDbtInfo.ColumnsInfo)
            {
                if (aColumnInfo.ForeignKeySegment)
                    continue;

                WizardDBTColumnInfo aTmpDBTColumnInfo = new WizardDBTColumnInfo(aColumnInfo);
                aTmpDBTColumnInfo.Visible = true;
                if (aTmpDBTColumnInfo.Title == null || aTmpDBTColumnInfo.Title.Length == 0)
                    aTmpDBTColumnInfo.Title = aColumnInfo.ColumnName;
                aTmpDBTColumnInfo.HotKeyLink = null;

                Size columnLineSize = GetColumnDialogLineSize(aTmpDBTColumnInfo, aFont);
                if (columnLineSize == Size.Empty)
                    continue;

                int columnLineWidth = columnLineSize.Width + (2 * FirstColumnControlLeftCoordinate);

                if (maxColumnLineWidth < columnLineWidth)
                    maxColumnLineWidth = columnLineWidth;

                dialogHeight += columnLineSize.Height + ColumnControlVerticalSpacing;
            }
            return (maxColumnLineWidth > 0) ? new Size(maxColumnLineWidth, dialogHeight) : Size.Empty;
        }

        //----------------------------------------------------------------------------
        private void InitDialogSize()
        {
            dialogSize = GetDialogSize(dbtInfo, this.FontToUse);
        }

    }

    #endregion

    #region WizardDBTColumnCodeTemplateParser class

	//============================================================================
	internal class WizardLabelTemplateParser : CodeTemplateParser
	{
		private const string ColumnLabelTitleToken = "ColumnLabelTitle";
		private const string ColumnLabelLeftToken = "ColumnLabelLeft";
		private const string ColumnLabelTopToken = "ColumnLabelTop";
		private const string ColumnLabelWidthToken = "ColumnLabelWidth";
		private const string ColumnLabelHeightToken = "ColumnLabelHeight";

		LabelInfo labelinfo;

		//----------------------------------------------------------------------------
		internal WizardLabelTemplateParser(LabelInfo labelinfo)
		{
			this.labelinfo = labelinfo;
		}

		//----------------------------------------------------------------------------
		internal override string GetTokenValue(string aToken)
		{
			if (aToken == null || aToken.Length == 0)
				return String.Empty;

			if (String.Compare(aToken, ColumnLabelTitleToken) == 0)
				return (this.labelinfo != null) ? this.labelinfo.Label : String.Empty;

			if (String.Compare(aToken, ColumnLabelLeftToken) == 0)
				return (this.labelinfo != null) ? this.labelinfo.Position.Left.ToString() : String.Empty;

			if (String.Compare(aToken, ColumnLabelTopToken) == 0)
				return (this.labelinfo != null) ? this.labelinfo.Position.Top.ToString() : String.Empty;

			if (String.Compare(aToken, ColumnLabelWidthToken) == 0)
				return (this.labelinfo != null) ? this.labelinfo.Position.Width.ToString() : String.Empty;

			if (String.Compare(aToken, ColumnLabelHeightToken) == 0)
				return (this.labelinfo != null) ? this.labelinfo.Position.Height.ToString() : String.Empty;

			return string.Empty;
		}


	}

    //============================================================================
    internal class WizardDBTColumnCodeTemplateParser : CodeTemplateParser
    {
        private const string DocumentNameToken = "DocumentName";
        private const string DocumentClassNameToken = "DocumentClassName";
        private const string ClientDocumentNameToken = "ClientDocumentName";
        private const string ClientDocumentClassNameToken = "ClientDocumentClassName";
        private const string ClientDocumentViewClassNameToken = "ClientDocumentViewClassName";
        private const string ClientDocumentViewFileNameToken = "ClientDocumentViewFileName";
        private const string DBTNameToken = "DBTName";
        private const string DBTClassNameToken = "DBTClassName";
        private const string IfDBTSlaveBufferedToken = "IfDBTSlaveBuffered";
        private const string IfNotDBTSlaveBufferedToken = "IfNotDBTSlaveBuffered";
        private const string TableNameToken = "TableName";
        private const string TableClassNameToken = "TableClassName";
        private const string ColumnNameToken = "ColumnName";
        private const string ColumnTitleToken = "ColumnTitle";
        private const string ColumnDataObjClassNameToken = "ColumnDataObjClassName";

		private const string ColumnLabelTitleToken = "ColumnLabelTitle";
        private const string ColumnLabelLeftToken = "ColumnLabelLeft";
        private const string ColumnLabelTopToken = "ColumnLabelTop";
        private const string ColumnLabelWidthToken = "ColumnLabelWidth";
        private const string ColumnLabelHeightToken = "ColumnLabelHeight";

        private const string ColumnControlNameToken = "ColumnControlName";
        private const string ColumnControlIdToken = "ColumnControlId";
        private const string ColumnControlStylesToken = "ColumnControlStyles";
        private const string ColumnControlLeftToken = "ColumnControlLeft";
        private const string ColumnControlTopToken = "ColumnControlTop";
        private const string ColumnControlWidthToken = "ColumnControlWidth";
        private const string ColumnControlHeightToken = "ColumnControlHeight";

        private const string ColumnParsedControlClassNameToken = "ColumnParsedControlClassName";

        private const string IfBoolColumnToken = "IfBoolColumn";
        private const string IfNotBoolColumnToken = "IfNotBoolColumn";

        private const string RelatedColumnNameToken = "RelatedColumnName";

        private const string IfHKLDefinedColumnToken = "IfHKLDefinedColumn";
        private const string ColumnHKLClassNameToken = "ColumnHKLClassName";
        private const string ColumnHKLTableNameToken = "ColumnHKLTableName";
        private const string ColumnHKLTableClassNameToken = "ColumnHKLTableClassName";
        private const string ColumnHKLCodeColumnNameToken = "ColumnHKLCodeColumnName";
        private const string ColumnHKLDescriptionColumnNameToken = "ColumnHKLDescriptionColumnName";
        private const string ColumnHKLDescriptionColumnDataObjClassNameToken = "ColumnHKLDescriptionColumnDataObjClassName";
        private const string ColumnHKLDescriptionColumnDataLengthToken = "ColumnHKLDescriptionColumnDataLength";
        private const string ColumnHotLinkLibraryRelativePathToken = "ColumnHotLinkLibraryRelativePath";
        private const string ColumnHotLinkModuleNameToken = "ColumnHotLinkModuleName";
        private const string ColumnHotLinkLibrarySourceFolderToken = "ColumnHotLinkLibrarySourceFolder";
        private const string ColumnHotLinkParentFileNameToken = "ColumnHotLinkParentFileName";

        private const string IfColumnShowsHotLinkDescriptionToken = "IfColumnShowsHotLinkDescription";
        private const string ColumnHotLinkDescriptionLabelIdToken = "ColumnHotLinkDescriptionLabelId";
        private const string ColumnHKLDescriptionParsedStaticNameToken = "ColumnHKLDescriptionParsedStaticName";
        private const string HotLinkDescriptionLabelLeftToken = "HotLinkDescriptionLabelLeft";
        private const string HotLinkDescriptionLabelWidthToken = "HotLinkDescriptionLabelWidth";
        private const string HotLinkDescriptionLabelHeigthToken = "HotLinkDescriptionLabelHeigth";
        private const string IfColumnHotLinkDescriptionIsStringToken = "IfColumnHotLinkDescriptionIsString";
        private const string IfColumnHotLinkDescriptionIsNotStringToken = "IfColumnHotLinkDescriptionIsNotString";

        private const string IfBaseTableColumnToken = "IfBaseTableColumn";
        private const string IfAdditionalColumnToken = "IfAdditionalColumn";

        private const string AdditionalColumnsClassNameToken = "AdditionalColumnsClassName";
        private const string AdditionalColumnsModuleNameToken = "AdditionalColumnsModuleName";
        private const string AdditionalColumnsLibrarySourceFolderToken = "AdditionalColumnsLibrarySourceFolder";

        private const string IfIsNotTableReferencedToken = "IfIsNotTableReferenced";
        private const string IfIsTableReferencedToken = "IfIsTableReferenced";

        private const string TabbedPaneNameToken = "TabbedPaneName";

        private WizardDBTColumnInfo columnInfo = null;
        private WizardDBTInfo dbtInfo = null;
		private LabelInfo labelInfo = null;
        private WizardDocumentInfo documentInfo = null;
        private WizardClientDocumentInfo clientDocumentInfo = null;
        private WizardDocumentTabbedPaneInfo tabbedPaneInfo = null;
        private int controlsTopCoordinate = 0;
        private ushort columnControlId = 0;

        //----------------------------------------------------------------------------
        internal WizardDBTColumnCodeTemplateParser(WizardDBTColumnInfo aColumnInfo, WizardDBTInfo aDBTInfo, WizardDocumentInfo aDocumentInfo, ushort aControlId, int aYCoordinate)
        {
            columnInfo = aColumnInfo;
            dbtInfo = aDBTInfo;
            documentInfo = aDocumentInfo;
            clientDocumentInfo = null;
            columnControlId = aControlId;
            controlsTopCoordinate = aYCoordinate;
        }

        //----------------------------------------------------------------------------
        internal WizardDBTColumnCodeTemplateParser(WizardDBTColumnInfo aColumnInfo, WizardDBTInfo aDBTInfo, WizardClientDocumentInfo aClientDocumentInfo, ushort aControlId, int aYCoordinate)
        {
            columnInfo = aColumnInfo;
            dbtInfo = aDBTInfo;
            documentInfo = null;
            clientDocumentInfo = aClientDocumentInfo;
            columnControlId = aControlId;
            controlsTopCoordinate = aYCoordinate;
        }

        //----------------------------------------------------------------------------
        internal WizardDBTColumnCodeTemplateParser(WizardDBTColumnInfo aColumnInfo, WizardDBTInfo aDBTInfo, ushort aControlId, int aYCoordinate)
            : this(aColumnInfo, aDBTInfo, (WizardDocumentInfo)null, aControlId, aYCoordinate)
        {
        }

        //----------------------------------------------------------------------------
        internal WizardDBTColumnCodeTemplateParser(WizardDBTColumnInfo aColumnInfo, WizardDBTInfo aDBTInfo, WizardDocumentInfo aDocumentInfo, ushort aControlId)
            : this(aColumnInfo, aDBTInfo, aDocumentInfo, aControlId, 0)
        {
        }

        //----------------------------------------------------------------------------
        internal WizardDBTColumnCodeTemplateParser(WizardDBTColumnInfo aColumnInfo, WizardDBTInfo aDBTInfo, WizardClientDocumentInfo aClientDocumentInfo, ushort aControlId)
            : this(aColumnInfo, aDBTInfo, aClientDocumentInfo, aControlId, 0)
        {
        }

        //----------------------------------------------------------------------------
        internal WizardDBTColumnCodeTemplateParser
            (
            WizardDBTColumnInfo aColumnInfo,
            WizardDocumentTabbedPaneInfo aTabbedPaneInfo,
            WizardDocumentInfo aDocumentInfo,
            ushort aControlId,
            int aYCoordinate
            )
            :
            this(aColumnInfo, (aTabbedPaneInfo != null) ? aTabbedPaneInfo.DBTInfo : null, aDocumentInfo, aControlId, aYCoordinate)
        {
            tabbedPaneInfo = aTabbedPaneInfo;
        }

        //----------------------------------------------------------------------------
        internal WizardDBTColumnCodeTemplateParser
            (
            WizardDBTColumnInfo aColumnInfo,
            WizardDocumentTabbedPaneInfo aTabbedPaneInfo,
            WizardClientDocumentInfo aClientDocumentInfo,
            ushort aControlId,
            int aYCoordinate
            )
            :
            this(aColumnInfo, (aTabbedPaneInfo != null) ? aTabbedPaneInfo.DBTInfo : null, aClientDocumentInfo, aControlId, aYCoordinate)
        {
            tabbedPaneInfo = aTabbedPaneInfo;
        }

        //----------------------------------------------------------------------------
        internal WizardDBTColumnCodeTemplateParser
            (
            WizardDBTColumnInfo aColumnInfo,
            WizardDocumentTabbedPaneInfo aTabbedPaneInfo,
            ushort aControlId,
            int aYCoordinate
            )
            :
            this(aColumnInfo, (aTabbedPaneInfo != null) ? aTabbedPaneInfo.DBTInfo : null, aControlId, aYCoordinate)
        {
            tabbedPaneInfo = aTabbedPaneInfo;
        }

        //----------------------------------------------------------------------------
        internal WizardDBTColumnCodeTemplateParser(WizardDBTColumnInfo aColumnInfo, ushort aControlId)
            : this(aColumnInfo, null, (WizardDocumentInfo)null, aControlId)
        {
        }

        //----------------------------------------------------------------------------
        internal WizardDBTColumnCodeTemplateParser(WizardDBTColumnInfo aColumnInfo)
            : this(aColumnInfo, 0)
        {
        }

        //----------------------------------------------------------------------------
        internal System.Drawing.Font FontToUse
        {
            get
            {
                if (documentInfo != null && documentInfo.Library != null && documentInfo.Library.Application != null)
                    return documentInfo.Library.Application.Font;

                if (clientDocumentInfo != null && clientDocumentInfo.Library != null && clientDocumentInfo.Library.Application != null)
                    return clientDocumentInfo.Library.Application.Font;

                return WizardApplicationInfo.DefaultFont;
            }
        }

        //----------------------------------------------------------------------------
        internal override string GetTokenValue(string aToken)
        {
            if (aToken == null || aToken.Length == 0)
                return String.Empty;

            if (String.Compare(aToken, DocumentNameToken) == 0)
                return (documentInfo != null) ? documentInfo.Name : String.Empty;

            if (String.Compare(aToken, DocumentClassNameToken) == 0)
                return (documentInfo != null) ? documentInfo.ClassName : String.Empty;

            if (String.Compare(aToken, ClientDocumentNameToken) == 0)
                return (clientDocumentInfo != null) ? clientDocumentInfo.Name : String.Empty;

            if (String.Compare(aToken, ClientDocumentClassNameToken) == 0)
                return (clientDocumentInfo != null) ? clientDocumentInfo.ClassName : String.Empty;

            if (String.Compare(aToken, ClientDocumentViewClassNameToken) == 0)
            {
                if (clientDocumentInfo == null)
                    return String.Empty;
                // se il nome della classe del clientdoc è "CD" + namespace
                // allora imposta il nome come "C" + namespace + "View" (attuale standard)
                // altrimenti usa nome classe + "View" (vecchio standard)
                //if (clientDocumentInfo.ClassName == "CD" + clientDocumentInfo.Name)
                //    return ("C" + clientDocumentInfo.Name) + "View";
                //else
                //    return clientDocumentInfo.ClassName + "View";

                // se il nome della classe inizia con "CD"
                // allora imposta il nome come "CD" + nomeclasse.mid(2) + "View"
                // altrimenti "CD" + namespace + "View"
                if (clientDocumentInfo.ClassName.Substring(0, 2) == "CD")
                    return "C" + clientDocumentInfo.ClassName.Substring(2) + "View";
                else
                    return "C" + clientDocumentInfo.Name + "View";
            }

            if (String.Compare(aToken, ClientDocumentViewFileNameToken) == 0)
            {
                if (clientDocumentInfo == null)
                    return String.Empty;
                // se il nome della classe del clientdoc è "CD" + namespace
                // allora imposta il nome come "C" + namespace + "View" (attuale standard)
                // altrimenti usa nome classe + "View" (vecchio standard)
                //if (clientDocumentInfo.ClassName == "CD" + clientDocumentInfo.Name)
                    //return ("UI" + clientDocumentInfo.Name);
                //else
                //    return clientDocumentInfo.ClassName + "View";

                // se il nome della classe inizia con "CD"
                // allora imposta il nome come "UI" + nomeclasse.mid(2)
                // altrimenti "UI" + namespace
                if (clientDocumentInfo.ClassName.Substring(0, 2) == "CD")
                    return "UI" + clientDocumentInfo.ClassName.Substring(2);
                else
                    return "UI" + clientDocumentInfo.Name;
            }

            if (String.Compare(aToken, DBTNameToken) == 0)
                return (dbtInfo != null) ? dbtInfo.Name : String.Empty;

            if (String.Compare(aToken, DBTClassNameToken) == 0)
                return (dbtInfo != null) ? dbtInfo.ClassName : String.Empty;

            if (aToken.StartsWith(IfDBTSlaveBufferedToken))
            {
                if (dbtInfo == null || !dbtInfo.IsSlaveBuffered)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfDBTSlaveBufferedToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfNotDBTSlaveBufferedToken))
            {
                if (dbtInfo == null || dbtInfo.IsSlaveBuffered)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfNotDBTSlaveBufferedToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (String.Compare(aToken, TableNameToken) == 0)
                return (dbtInfo != null) ? dbtInfo.TableName : String.Empty;

            if (String.Compare(aToken, TableClassNameToken) == 0)
            {
                if (dbtInfo == null)
                    return String.Empty;

                WizardTableInfo tableInfo = dbtInfo.GetTableInfo();

                return (tableInfo != null) ? tableInfo.ClassName : String.Empty;
            }

            if (String.Compare(aToken, ColumnNameToken) == 0)
                return (columnInfo != null) ? columnInfo.ColumnName : String.Empty;

            if (String.Compare(aToken, ColumnTitleToken) == 0)
                return (columnInfo != null) ? columnInfo.Title : String.Empty;

            if (String.Compare(aToken, ColumnDataObjClassNameToken) == 0)
                return (columnInfo != null) ? columnInfo.GetDataObjClassName() : String.Empty;

            if (aToken.StartsWith(IfBoolColumnToken))
            {
                if (columnInfo == null || columnInfo.ColumnDataType.Type != WizardTableColumnDataType.DataType.Boolean)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfBoolColumnToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }
			
            if (aToken.StartsWith(IfNotBoolColumnToken))
            {
                if (columnInfo == null || columnInfo.ColumnDataType.Type == WizardTableColumnDataType.DataType.Boolean)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfNotBoolColumnToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

			if (String.Compare(aToken, ColumnLabelLeftToken) == 0)
			{
				return (this.labelInfo != null) ? this.labelInfo.Position.Left.ToString() : String.Empty;
			}

			if (String.Compare(aToken, ColumnLabelTopToken) == 0)
			{
				return (this.labelInfo != null) ? this.labelInfo.Position.Top.ToString() : String.Empty; ;
			}

            if (String.Compare(aToken, ColumnLabelWidthToken) == 0)
            {
                System.Drawing.Font fontToUse = this.FontToUse;
                int columnLabelWidth = (columnInfo != null) ? Generics.GDI32.GetTextDisplaySize(columnInfo.Title, fontToUse.FontFamily.Name, fontToUse.SizeInPoints).Width : 0;

				return (this.labelInfo != null) ? this.labelInfo.Position.Width.ToString() : String.Empty;
            }

            if (String.Compare(aToken, ColumnLabelHeightToken) == 0)
				return (this.labelInfo != null) ? this.labelInfo.Position.Height.ToString() : String.Empty;

			if (String.Compare(aToken, ColumnControlNameToken) == 0)
			{
				// Auto Make the Label
				// ---------------------------------------------------------------------------
				if (dbtInfo == null || columnInfo == null)
					return String.Empty;

				int columnLabelWidth = (columnInfo != null) ? Generics.GDI32.GetTextDisplaySize(columnInfo.Title, this.FontToUse.FontFamily.Name,
					this.FontToUse.SizeInPoints).Width : 0;

				this.labelInfo = null;
				if (dbtInfo.IsSlave || dbtInfo.IsSlaveBuffered)
				{
					foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in documentInfo.TabbedPanes)
					{
						if (dbtInfo.Name == aTabbedPaneInfo.DBTName && !columnInfo.LabelAdded)
						{
							this.labelInfo = new LabelInfo(columnInfo,
								controlsTopCoordinate, WizardDBTCodeTemplateParser.FirstColumnControlLeftCoordinate,
								columnLabelWidth, WizardDBTCodeTemplateParser.ColumnControlDefaultHeight);

							aTabbedPaneInfo.LabelInfoCollection.Add(this.labelInfo);
							columnInfo.LabelAdded = true;
							break;
						}
					}
				}
				else if (dbtInfo.IsMaster && !columnInfo.LabelAdded)
				{
					this.labelInfo = new LabelInfo(columnInfo,
						controlsTopCoordinate, WizardDBTCodeTemplateParser.FirstColumnControlLeftCoordinate,
						columnLabelWidth, WizardDBTCodeTemplateParser.ColumnControlDefaultHeight);

					documentInfo.LabelInfoCollection.Add(this.labelInfo);
					columnInfo.LabelAdded = true;
				}
				// ---------------------------------------------------------------------------

				return (columnInfo != null) ? columnInfo.GetColumnControlName() : String.Empty;
			}

            if (String.Compare(aToken, ColumnControlIdToken) == 0)
                return columnControlId.ToString();

            if (String.Compare(aToken, ColumnControlStylesToken) == 0)
                return (columnInfo != null) ? columnInfo.GetColumnControlStyles() : String.Empty;

            if (String.Compare(aToken, ColumnControlLeftToken) == 0)
            {
                int controlLeft = WizardDBTCodeTemplateParser.FirstColumnControlLeftCoordinate;

                if (columnInfo != null && columnInfo.IsLabelVisible && columnInfo.Title != null && columnInfo.Title.Length > 0)
                {
                    System.Drawing.Font fontToUse = this.FontToUse;
                    int labelWidth = Generics.GDI32.GetTextDisplaySize(columnInfo.Title, fontToUse.FontFamily.Name, fontToUse.SizeInPoints).Width;
                    if (labelWidth > 0)
                    {
                        controlLeft += labelWidth;
                        controlLeft += WizardDBTCodeTemplateParser.ColumnControlHorizontalSpacing;
                    }
                }

				if (columnInfo.Position.isSet) 
					return columnInfo.Position.Left.ToString();
				if (columnInfo != null) columnInfo.Position.Left = controlLeft;
                return controlLeft.ToString();
            }


			if (String.Compare(aToken, ColumnControlTopToken) == 0)
			{
				if (columnInfo.Position.isSet) 
					return columnInfo.Position.Top.ToString();
				if (columnInfo != null) columnInfo.Position.Top = controlsTopCoordinate;
				return controlsTopCoordinate.ToString();
			}
                
            if (String.Compare(aToken, ColumnControlWidthToken) == 0)
            {
                System.Drawing.Font fontToUse = this.FontToUse;
				if (columnInfo.Position.isSet) 
					return columnInfo.Position.Width.ToString();
				columnInfo.Position.Width = (columnInfo != null) ? columnInfo.GetColumnControlDefaultWidth(fontToUse.FontFamily.Name, fontToUse.SizeInPoints) : 0;
                return (columnInfo != null) ? columnInfo.GetColumnControlDefaultWidth(fontToUse.FontFamily.Name, fontToUse.SizeInPoints).ToString() : String.Empty;
            }

            if (String.Compare(aToken, ColumnControlHeightToken) == 0)
            {
                System.Drawing.Font fontToUse = this.FontToUse;
				if (columnInfo.Position.isSet)
					return columnInfo.Position.Height.ToString();
				columnInfo.Position.Height = (columnInfo != null) ? columnInfo.GetColumnControlDefaultHeight(fontToUse.FontFamily.Name, fontToUse.SizeInPoints) : 0;
                return (columnInfo != null) ? columnInfo.GetColumnControlDefaultHeight(fontToUse.FontFamily.Name, fontToUse.SizeInPoints).ToString() : String.Empty;
            }

            if (String.Compare(aToken, ColumnParsedControlClassNameToken) == 0)
                return (columnInfo != null) ? columnInfo.GetColumnParsedControlClassName() : String.Empty;

            if (String.Compare(aToken, RelatedColumnNameToken) == 0)
                return (columnInfo != null && columnInfo.ForeignKeySegment) ? columnInfo.ForeignKeyRelatedColumn : String.Empty;

            if (aToken.StartsWith(IfHKLDefinedColumnToken))
            {
                if (columnInfo == null || !columnInfo.IsHKLDefined)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfHKLDefinedColumnToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (String.Compare(aToken, ColumnHKLClassNameToken) == 0)
                return (columnInfo != null && columnInfo.IsHKLDefined) ? columnInfo.HotKeyLink.ClassName : String.Empty;

            if (String.Compare(aToken, ColumnHKLTableNameToken) == 0)
                return (columnInfo != null && columnInfo.IsHKLDefined) ? columnInfo.HotKeyLink.Table.Name : String.Empty;

            if (String.Compare(aToken, ColumnHKLTableClassNameToken) == 0)
                return (columnInfo != null && columnInfo.IsHKLDefined) ? columnInfo.HotKeyLink.Table.ClassName : String.Empty;

            if (String.Compare(aToken, ColumnHKLDescriptionColumnNameToken) == 0)
                return (columnInfo != null && columnInfo.IsHKLDefined) ? columnInfo.HotKeyLink.DescriptionColumnName : String.Empty;

            if (String.Compare(aToken, ColumnHKLCodeColumnNameToken) == 0)
                return (columnInfo != null && columnInfo.IsHKLDefined) ? columnInfo.HotKeyLink.CodeColumnName : String.Empty;

            if (String.Compare(aToken, ColumnHKLDescriptionColumnDataLengthToken) == 0)
            {
                if (columnInfo == null || !columnInfo.IsHKLDefined)
                    return String.Empty;

                WizardTableColumnInfo hotLinkDescriptionColumnInfo = columnInfo.HotKeyLink.DescriptionColumn;
                if (hotLinkDescriptionColumnInfo == null)
                    return String.Empty;

                return hotLinkDescriptionColumnInfo.DataLength.ToString();
            }

            if (String.Compare(aToken, ColumnHKLDescriptionColumnDataObjClassNameToken) == 0)
            {
                if (columnInfo == null || !columnInfo.IsHKLDefined)
                    return String.Empty;

                WizardTableColumnInfo hotLinkDescriptionColumnInfo = columnInfo.HotKeyLink.DescriptionColumn;
                if (hotLinkDescriptionColumnInfo == null)
                    return String.Empty;

                return hotLinkDescriptionColumnInfo.GetDataObjClassName();
            }

            if (String.Compare(aToken, ColumnHotLinkLibraryRelativePathToken) == 0)
            {
                if (dbtInfo == null || dbtInfo.Library == null || columnInfo == null || !columnInfo.IsHKLDefined)
                    return String.Empty;

                WizardLibraryInfo currentLibrary = null;
                if (documentInfo != null)
                    currentLibrary = documentInfo.Library;
                else if (clientDocumentInfo != null)
                    currentLibrary = clientDocumentInfo.Library;

                if (currentLibrary == null)
                    return String.Empty;

                WizardLibraryInfo hotLinkLibrary = dbtInfo.GetHotKeyLinkLibrary(columnInfo.HotKeyLink);

                if (hotLinkLibrary == null)
                    return String.Empty;

                string relativePath = Generics.MakeRelativeTo(WizardCodeGenerator.GetStandardLibraryPath(currentLibrary), WizardCodeGenerator.GetStandardLibraryPath(hotLinkLibrary));
                if (relativePath == null || relativePath.Length == 0)
                    return String.Empty;

                return relativePath + Path.DirectorySeparatorChar;
            }

            if (String.Compare(aToken, ColumnHotLinkModuleNameToken) == 0)
            {
                if (dbtInfo == null || dbtInfo.Library == null || columnInfo == null || !columnInfo.IsHKLDefined)
                    return String.Empty;

                WizardLibraryInfo currentLibrary = null;
                if (documentInfo != null)
                    currentLibrary = documentInfo.Library;
                else if (clientDocumentInfo != null)
                    currentLibrary = clientDocumentInfo.Library;

                if (currentLibrary == null)
                    return String.Empty;

                WizardLibraryInfo hotLinkLibrary = dbtInfo.GetHotKeyLinkLibrary(columnInfo.HotKeyLink);

                if (hotLinkLibrary == null || hotLinkLibrary.Module == null)
                    return String.Empty;

                return hotLinkLibrary.Module.Name;
            }

            if (String.Compare(aToken, ColumnHotLinkLibrarySourceFolderToken) == 0)
            {
                if (dbtInfo == null || dbtInfo.Library == null || columnInfo == null || !columnInfo.IsHKLDefined)
                    return String.Empty;

                WizardLibraryInfo currentLibrary = null;
                if (documentInfo != null)
                    currentLibrary = documentInfo.Library;
                else if (clientDocumentInfo != null)
                    currentLibrary = clientDocumentInfo.Library;

                if (currentLibrary == null)
                    return String.Empty;

                WizardLibraryInfo hotLinkLibrary = dbtInfo.GetHotKeyLinkLibrary(columnInfo.HotKeyLink);

                if (hotLinkLibrary == null)
                    return String.Empty;

                return hotLinkLibrary.SourceFolder;
            }

            if (String.Compare(aToken, ColumnHotLinkParentFileNameToken) == 0)
            {
                if (dbtInfo == null || dbtInfo.Library == null || columnInfo == null || !columnInfo.IsHKLDefined)
                    return String.Empty;

                object hotLinkParent = dbtInfo.GetHotKeyLinkParent(columnInfo.HotKeyLink);

                if (hotLinkParent != null)
                {
                    if (hotLinkParent is WizardTableInfo)
                        return ((WizardTableInfo)hotLinkParent).ClassName;
                    if (hotLinkParent is WizardDocumentInfo)
                        return ((WizardDocumentInfo)hotLinkParent).ClassName;
                }
                else
                {
                }
                return String.Empty;
            }

            if (aToken.StartsWith(IfColumnShowsHotLinkDescriptionToken))
            {
                if (columnInfo == null || !columnInfo.Visible || !columnInfo.ShowHotKeyLinkDescription)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfColumnShowsHotLinkDescriptionToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (String.Compare(aToken, ColumnHotLinkDescriptionLabelIdToken) == 0)
                return (columnInfo != null && columnInfo.ShowHotKeyLinkDescription) ? (columnControlId + 1).ToString() : String.Empty;

            if (String.Compare(aToken, ColumnHKLDescriptionParsedStaticNameToken) == 0)
            {
                if (columnInfo == null || !columnInfo.ShowHotKeyLinkDescription)
                    return String.Empty;

                WizardTableColumnInfo hotLinkDescriptionColumnInfo = columnInfo.HotKeyLink.DescriptionColumn;
                if (hotLinkDescriptionColumnInfo == null)
                    return String.Empty;

                return hotLinkDescriptionColumnInfo.GetParsedStaticClassName();
            }

            if (String.Compare(aToken, HotLinkDescriptionLabelLeftToken) == 0)
            {
                if (columnInfo == null || !columnInfo.ShowHotKeyLinkDescription)
                    return String.Empty;

                int hotLinkDescriptionLabelLeft = WizardDBTCodeTemplateParser.FirstColumnControlLeftCoordinate;

                System.Drawing.Font fontToUse = this.FontToUse;
                if (columnInfo != null && columnInfo.IsLabelVisible && columnInfo.Title != null && columnInfo.Title.Length > 0)
                {
                    int labelWidth = Generics.GDI32.GetTextDisplaySize(columnInfo.Title, fontToUse.FontFamily.Name, fontToUse.SizeInPoints).Width;
                    if (labelWidth > 0)
                    {
                        hotLinkDescriptionLabelLeft += labelWidth;
                        hotLinkDescriptionLabelLeft += WizardDBTCodeTemplateParser.ColumnControlHorizontalSpacing;
                    }
                }

                hotLinkDescriptionLabelLeft += columnInfo.GetColumnControlDefaultWidth(fontToUse.FontFamily.Name, fontToUse.SizeInPoints);
                hotLinkDescriptionLabelLeft += WizardDBTCodeTemplateParser.ColumnControlHorizontalSpacing;

                return hotLinkDescriptionLabelLeft.ToString();
            }

            if (String.Compare(aToken, HotLinkDescriptionLabelWidthToken) == 0)
            {
                if (columnInfo == null || !columnInfo.ShowHotKeyLinkDescription)
                    return String.Empty;

                WizardTableColumnInfo hotLinkDescriptionColumnInfo = columnInfo.HotKeyLink.DescriptionColumn;
                if (hotLinkDescriptionColumnInfo == null)
                    return String.Empty;

                System.Drawing.Font fontToUse = this.FontToUse;

                return WizardDBTColumnInfo.GetStringColumnControlDefaultSize(hotLinkDescriptionColumnInfo, fontToUse.FontFamily.Name, fontToUse.SizeInPoints).Width.ToString();
            }

            if (String.Compare(aToken, HotLinkDescriptionLabelHeigthToken) == 0)
            {
                if (columnInfo == null || !columnInfo.ShowHotKeyLinkDescription)
                    return String.Empty;

                WizardTableColumnInfo hotLinkDescriptionColumnInfo = columnInfo.HotKeyLink.DescriptionColumn;
                if (hotLinkDescriptionColumnInfo == null)
                    return String.Empty;

                System.Drawing.Font fontToUse = this.FontToUse;

                return WizardDBTColumnInfo.GetStringColumnControlDefaultSize(hotLinkDescriptionColumnInfo, fontToUse.FontFamily.Name, fontToUse.SizeInPoints).Height.ToString();
            }

            if (aToken.StartsWith(IfColumnHotLinkDescriptionIsStringToken))
            {
                if (columnInfo == null || !columnInfo.ShowHotKeyLinkDescription)
                    return String.Empty;

                WizardTableColumnInfo hotLinkDescriptionColumnInfo = columnInfo.HotKeyLink.DescriptionColumn;
                if (hotLinkDescriptionColumnInfo == null || hotLinkDescriptionColumnInfo.DataType.Type != WizardTableColumnDataType.DataType.String)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfColumnHotLinkDescriptionIsStringToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfColumnHotLinkDescriptionIsNotStringToken))
            {
                if (columnInfo == null || !columnInfo.ShowHotKeyLinkDescription)
                    return String.Empty;

                WizardTableColumnInfo hotLinkDescriptionColumnInfo = columnInfo.HotKeyLink.DescriptionColumn;
                if (hotLinkDescriptionColumnInfo == null || hotLinkDescriptionColumnInfo.DataType.Type == WizardTableColumnDataType.DataType.String)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfColumnHotLinkDescriptionIsStringToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfBaseTableColumnToken))
            {
                if (columnInfo == null || dbtInfo == null)
                    return String.Empty;

                WizardTableInfo tableInfo = dbtInfo.GetTableInfo();
                if (tableInfo.GetColumnInfoByName(columnInfo.ColumnName) == null)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfBaseTableColumnToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfAdditionalColumnToken))
            {
                if (columnInfo == null || dbtInfo == null || dbtInfo.Library == null)
                    return String.Empty;

                WizardTableInfo tableInfo = dbtInfo.GetTableInfo();
                if (
                    tableInfo.GetColumnInfoByName(columnInfo.ColumnName) != null ||
                    dbtInfo.Library.GetExtraAddedColumnInfo(tableInfo.GetNameSpace(), columnInfo.ColumnName) == null
                    )
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfAdditionalColumnToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (String.Compare(aToken, AdditionalColumnsClassNameToken) == 0)
            {
                if (columnInfo == null || dbtInfo == null || dbtInfo.Library == null)
                    return String.Empty;

                WizardTableInfo tableInfo = dbtInfo.GetTableInfo();
                if (tableInfo.GetColumnInfoByName(columnInfo.ColumnName) != null)
                    return String.Empty;

                WizardExtraAddedColumnsInfo currentAdditionalColumnsInfo = dbtInfo.Library.GetExtraAddedColumnInfo(tableInfo.GetNameSpace(), columnInfo.ColumnName);
                return (currentAdditionalColumnsInfo != null) ? currentAdditionalColumnsInfo.ClassName : String.Empty;
            }

            if (String.Compare(aToken, AdditionalColumnsModuleNameToken) == 0)
            {
                if (columnInfo == null || dbtInfo == null || dbtInfo.Library == null)
                    return String.Empty;

                WizardTableInfo tableInfo = dbtInfo.GetTableInfo();
                if (tableInfo.GetColumnInfoByName(columnInfo.ColumnName) != null)
                    return String.Empty;

                WizardExtraAddedColumnsInfo currentAdditionalColumnsInfo = dbtInfo.Library.GetExtraAddedColumnInfo(tableInfo.GetNameSpace(), columnInfo.ColumnName);
                return (currentAdditionalColumnsInfo != null && currentAdditionalColumnsInfo.Library != null && currentAdditionalColumnsInfo.Library.Module != null) ? currentAdditionalColumnsInfo.Library.Module.Name : String.Empty;
            }

            if (String.Compare(aToken, AdditionalColumnsLibrarySourceFolderToken) == 0)
            {
                if (columnInfo == null || dbtInfo == null || dbtInfo.Library == null)
                    return String.Empty;

                WizardTableInfo tableInfo = dbtInfo.GetTableInfo();
                if (tableInfo.GetColumnInfoByName(columnInfo.ColumnName) != null)
                    return String.Empty;

                WizardExtraAddedColumnsInfo currentAdditionalColumnsInfo = dbtInfo.Library.GetExtraAddedColumnInfo(tableInfo.GetNameSpace(), columnInfo.ColumnName);
                return (currentAdditionalColumnsInfo != null && currentAdditionalColumnsInfo.Library != null) ? currentAdditionalColumnsInfo.Library.SourceFolder : String.Empty;
            }

            if (aToken.StartsWith(IfIsNotTableReferencedToken))
            {
                if (dbtInfo == null)
                    return String.Empty;

                WizardTableInfo tableInfo = dbtInfo.GetTableInfo();

                if (tableInfo == null || tableInfo.IsReferenced)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfIsNotTableReferencedToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken, true);
                }
            }

            if (aToken.StartsWith(IfIsTableReferencedToken))
            {
                if (dbtInfo == null)
                    return String.Empty;

                WizardTableInfo tableInfo = dbtInfo.GetTableInfo();

                if (tableInfo == null || !tableInfo.IsReferenced)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfIsTableReferencedToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken, true);
                }
            }

            if (String.Compare(aToken, TabbedPaneNameToken) == 0)
            {
                if (tabbedPaneInfo == null || tabbedPaneInfo.DBTInfo == null)
                    return String.Empty;

                object tabbedPaneContext = (documentInfo != null) ? (object)documentInfo : clientDocumentInfo;

                return tabbedPaneInfo.GetInternalName(tabbedPaneContext);
            }

            return null;
        }
    }

    #endregion

    #region WizardEnumCodeTemplateParser class

    //============================================================================
    internal class WizardEnumCodeTemplateParser : CodeTemplateParser
    {
        private const string EnumNameToken = "EnumName";
        private const string EnumValueToken = "EnumValue";
        private const string ItemsCountToken = "ItemsCount";
        private const string EnumDefaultItemNameToken = "EnumDefaultItemName";
        private const string EnumDefaultItemValueToken = "EnumDefaultItemValue";

        private const string RepeatOnEnumItemsToken = "RepeatOnEnumItems";
        private const string IfDefaultItemDefinedToken = "IfDefaultItemDefined";

        private WizardEnumInfo enumInfo = null;

        //----------------------------------------------------------------------------
        internal WizardEnumCodeTemplateParser(WizardEnumInfo aEnumInfo)
        {
            enumInfo = aEnumInfo;
        }

        //----------------------------------------------------------------------------
        internal override string GetTokenValue(string aToken)
        {
            if (aToken == null)
                return null;

            aToken = aToken.Trim();
            if (aToken.Length == 0)
                return String.Empty;

            if (String.Compare(aToken, EnumNameToken) == 0)
                return (enumInfo != null) ? enumInfo.Name : String.Empty;

            if (String.Compare(aToken, EnumValueToken) == 0)
                return (enumInfo != null) ? enumInfo.Value.ToString() : String.Empty;

            if (String.Compare(aToken, ItemsCountToken) == 0)
                return (enumInfo != null) ? enumInfo.ItemsCount.ToString() : String.Empty;

            if (aToken.StartsWith(IfDefaultItemDefinedToken))
            {
                if (enumInfo == null || enumInfo.DefaultItem == null)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfDefaultItemDefinedToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (String.Compare(aToken, EnumDefaultItemNameToken) == 0)
            {
                if (enumInfo == null)
                    return String.Empty;

                WizardEnumItemInfo defaultItemInfo = enumInfo.DefaultItem;
                return (defaultItemInfo != null) ? defaultItemInfo.Name : String.Empty;
            }

            if (String.Compare(aToken, EnumDefaultItemValueToken) == 0)
            {
                if (enumInfo == null)
                    return String.Empty;

                WizardEnumItemInfo defaultItemInfo = enumInfo.DefaultItem;
                return (defaultItemInfo != null) ? defaultItemInfo.Value.ToString(NumberFormatInfo.InvariantInfo) : String.Empty;
            }

            if (aToken.StartsWith(RepeatOnEnumItemsToken))
            {
                if (enumInfo == null || enumInfo.ItemsCount == 0)
                    return String.Empty;

                string subToken;
                string itemsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnEnumItemsToken, out subToken, out itemsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string itemsLines = String.Empty;
                    foreach (WizardEnumItemInfo aItemInfo in enumInfo.ItemsInfo)
                    {
                        string itemParsedText = SubstituteEnumItemTokens(subToken, aItemInfo);

                        if (itemParsedText != null && itemParsedText.Length > 0)
                        {
                            if (itemsLines.Length > 0)
                                itemsLines += itemsLinesSeparator;
                            itemsLines += itemParsedText;
                        }
                    }
                    return itemsLines;
                }
            }

            return null;
        }

        //----------------------------------------------------------------------------
        private string SubstituteEnumItemTokens
            (
            string source,
            WizardEnumItemInfo aItemInfo,
            bool preserveUnresolvedTokens
            )
        {
            if (source == null || aItemInfo == null)
                return null;

            source = source.Trim();
            if (source.Length == 0)
                return String.Empty;

            string itemParsedText = this.SubstituteTokens(source, true);
            if (AreTokensToSubstitute(itemParsedText))
            {
                WizardEnumItemCodeTemplateParser itemParser = new WizardEnumItemCodeTemplateParser(aItemInfo);

                itemParsedText = itemParser.SubstituteTokens(itemParsedText, preserveUnresolvedTokens);
            }
            return itemParsedText;
        }

        //----------------------------------------------------------------------------
        private string SubstituteEnumItemTokens(string source, WizardEnumItemInfo aItemInfo)
        {
            return SubstituteEnumItemTokens(source, aItemInfo, false);
        }
    }

    #endregion

    #region WizardEnumItemCodeTemplateParser class

    //============================================================================
    internal class WizardEnumItemCodeTemplateParser : CodeTemplateParser
    {
        private const string EnumItemNameToken = "EnumItemName";
        private const string EnumItemValueToken = "EnumItemValue";

        private WizardEnumItemInfo itemInfo = null;

        //----------------------------------------------------------------------------
        internal WizardEnumItemCodeTemplateParser(WizardEnumItemInfo aEnumItemInfo)
        {
            itemInfo = aEnumItemInfo;
        }

        //----------------------------------------------------------------------------
        internal override string GetTokenValue(string aToken)
        {
            if (aToken == null || aToken.Length == 0)
                return String.Empty;

            if (String.Compare(aToken, EnumItemNameToken) == 0)
                return (itemInfo != null) ? itemInfo.Name : String.Empty;

            if (String.Compare(aToken, EnumItemValueToken) == 0)
                return (itemInfo != null) ? itemInfo.Value.ToString() : String.Empty;

            return null;
        }
    }

    #endregion

    #region WizardServerDocumentCodeTemplateParser class

    //============================================================================
    internal class WizardServerDocumentCodeTemplateParser : CodeTemplateParser
    {
        private const string ServerDocumentClassNameToken = "ServerDocumentClassName";
        private const string ServerDocumentLibraryNameToken = "ServerDocumentLibraryName";
        private const string RepeatOnAttachedClientDocumentsToken = "RepeatOnAttachedClientDocuments";
        private const string IfBelongsToExternLibraryToken = "IfBelongsToExternLibrary";
        private const string LibFileDebugPathToken = "LibFileDebugPath";
        private const string LibFileReleasePathToken = "LibFileReleasePath";

        private WizardLibraryInfo libraryInfo = null;
        private WizardDocumentInfo serverDocumentInfo = null;

        //----------------------------------------------------------------------------
        internal WizardServerDocumentCodeTemplateParser(WizardLibraryInfo aLibraryInfo, WizardDocumentInfo aServerDocument)
        {
            libraryInfo = aLibraryInfo;
            serverDocumentInfo = aServerDocument;
        }

        //----------------------------------------------------------------------------
        internal override string GetTokenValue(string aToken)
        {
            if (aToken == null)
                return null;

            aToken = aToken.Trim();
            if (aToken.Length == 0)
                return String.Empty;

            if (String.Compare(aToken, ServerDocumentClassNameToken) == 0)
                return (serverDocumentInfo != null) ? serverDocumentInfo.ClassName : String.Empty;

            if (String.Compare(aToken, ServerDocumentLibraryNameToken) == 0)
                return (serverDocumentInfo != null && serverDocumentInfo.Library != null) ? serverDocumentInfo.Library.AggregateName : String.Empty;

            if (aToken.StartsWith(RepeatOnAttachedClientDocumentsToken))
            {
                if (serverDocumentInfo == null || libraryInfo == null || libraryInfo.ClientDocumentsCount == 0)
                    return String.Empty;

                string subToken;
                string clientDocumentsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnAttachedClientDocumentsToken, out subToken, out clientDocumentsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string clientDocumentsLines = String.Empty;
                    foreach (WizardClientDocumentInfo aClientDocument in libraryInfo.ClientDocumentsInfo)
                    {
                        if (aClientDocument.ServerDocumentInfo == null || !serverDocumentInfo.Equals(aClientDocument.ServerDocumentInfo))
                            continue;

                        WizardClientDocumentCodeTemplateParser aClientDocumentParser = new WizardClientDocumentCodeTemplateParser(aClientDocument);

                        string clientDocumentParsedText = aClientDocumentParser.SubstituteTokens(subToken);

                        if (clientDocumentParsedText != null && clientDocumentParsedText.Length > 0)
                        {
                            if (clientDocumentsLines.Length > 0)
                                clientDocumentsLines += clientDocumentsLinesSeparator;
                            clientDocumentsLines += clientDocumentParsedText;
                        }
                    }
                    return clientDocumentsLines;
                }
            }

            if (aToken.StartsWith(IfBelongsToExternLibraryToken))
            {
                if
                    (
                    serverDocumentInfo == null ||
                    serverDocumentInfo.Library == null ||
                    WizardLibraryInfo.Equals(serverDocumentInfo.Library, libraryInfo) ||
                    (libraryInfo != null && libraryInfo.DependsOn(serverDocumentInfo.Library))
                    )
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfBelongsToExternLibraryToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (String.Compare(aToken, LibFileDebugPathToken) == 0)
                return (serverDocumentInfo != null && serverDocumentInfo.Library != null) ? WizardCodeGenerator.GetStandardLibraryPath(serverDocumentInfo.Library) + "\\Bin\\Debug" : String.Empty;

            if (String.Compare(aToken, LibFileReleasePathToken) == 0)
                return (serverDocumentInfo != null && serverDocumentInfo.Library != null) ? WizardCodeGenerator.GetStandardLibraryPath(serverDocumentInfo.Library) + "\\Bin\\Release" : String.Empty;

            return null;
        }
    }

    #endregion

    #region WizardClientDocumentCodeTemplateParser class

    //============================================================================
    internal class WizardClientDocumentCodeTemplateParser : CodeTemplateParser
    {
        private const string ClientDocumentNameToken = "ClientDocumentName";
        private const string ClientDocumentClassNameToken = "ClientDocumentClassName";
        private const string ClientDocumentViewClassNameToken = "ClientDocumentViewClassName";
        private const string ClientDocumentViewFileNameToken = "ClientDocumentViewFileName";
        private const string ClientDocumentTitleToken = "ClientDocumentTitle";
        private const string ServerDocumentClassNameToken = "ServerDocumentClassName";
        private const string ServerDocumentFamilyClassNameToken = "ServerDocumentFamilyClassName";
        private const string ModuleNameToken = "ModuleName";
        private const string LibrarySourceFolderToken = "LibrarySourceFolder";
        private const string LibraryNameSpaceToken = "LibraryNameSpace";
        private const string IfAttachedDirectlyToken = "IfAttachedDirectly";
        private const string IfAttachedToFamilyToken = "IfAttachedToFamily";
        private const string IfExcludeUnattededModeToken = "IfExcludeUnattededMode";
        private const string IfIncludeUnattededModeToken = "IfIncludeUnattededMode";
        private const string IfExcludeBatchToken = "IfExcludeBatch";
        private const string IfIncludeBatchToken = "IfIncludeBatch";
        private const string IfMsgRoutingModeBothToken = "IfMsgRoutingModeBoth";
        private const string IfNoMsgRoutingModeBothToken = "IfNoMsgRoutingModeBoth";
        private const string IfMsgRoutingModeBeforeToken = "IfMsgRoutingModeBefore";
        private const string IfMsgRoutingModeAfterToken = "IfMsgRoutingModeAfter";

        private const string IfDBTsPresentToken = "IfDBTsPresent";
        private const string RepeatOnDBTsToken = "RepeatOnDBTs";
        private const string RepeatOnTabbedPanesToken = "RepeatOnTabbedPanes";
		private const string RepeatOnServerDocumentIncludeFilesToken = "RepeatOnServerDocumentIncludeFiles";

        private const string CultureLanguageIdentifierToken = "CultureLanguageIdentifier";
        private const string CultureSubLanguageIdentifierToken = "CultureSubLanguageIdentifier";

        private const string IsTabberToCreateToken = "IsTabberToCreate";
        private const string TabberControlIdToken = "TabberControlId";
        private const string TabberLeftToken = "TabberLeft";
        private const string TabberTopToken = "TabberTop";
        private const string TabberWidthToken = "TabberWidth";
        private const string TabberHeightToken = "TabberHeight";

        private const string FontSizeToken = "FontSize";
        private const string FontNameToken = "FontName";
        private const string FontWeightToken = "FontWeight";
        private const string FontIsItalicToken = "FontIsItalic";
        private const string FontCharSetToken = "FontCharSet";

        private const string IfInterfacePresentToken = "IfInterfacePresent";
        private const string AddTabsToServerDocumentTabberToken = "AddTabsToServerDocumentTabber";
        private const string IsSlaveFormViewToCreateToken = "IsSlaveFormViewToCreate";
        private const string SlaveFormViewWidthToken = "SlaveFormViewWidth";
        private const string SlaveFormViewHeightToken = "SlaveFormViewHeight";

        private const string ClientDocumentSlaveFormViewIdToken = "ClientDocumentSlaveFormViewId";
        private const string ClientDocumentCommandIdToken = "ClientDocumentCommandId";
        private const string ToolBarButtonLargeIdToken = "ToolBarButtonLargeId";
        private const string ToolBarButtonSmallIdToken = "ToolBarButtonSmallId";
        private const string ToolBarButtonLargeDisabledIdToken = "ToolBarButtonLargeDisabledId";
        private const string ToolBarButtonSmallDisabledIdToken = "ToolBarButtonSmallDisabledId";

        private const string ClientDocumentNextResourceValueToken = "ClientDocumentNextResourceValue";
        private const string ClientDocumentNextControlValueToken = "ClientDocumentNextControlValue";
        private const string ClientDocumentNextCommandValueToken = "ClientDocumentNextCommandValue";
        private const string ClientDocumentNextSymedValueToken = "ClientDocumentNextSymedValue";

        private WizardClientDocumentInfo clientDocumentInfo = null;
        private System.Drawing.Size dialogSize = Size.Empty;

        //----------------------------------------------------------------------------
        internal WizardClientDocumentCodeTemplateParser(WizardClientDocumentInfo aClientDocument)
        {
            clientDocumentInfo = aClientDocument;

            InitDialogSize();
        }

        //----------------------------------------------------------------------------
        internal override string GetTokenValue(string aToken)
        {
            if (aToken == null)
                return null;

            aToken = aToken.Trim();
            if (aToken.Length == 0)
                return String.Empty;

            if (String.Compare(aToken, ClientDocumentNameToken) == 0)
                return (clientDocumentInfo != null) ? clientDocumentInfo.Name : String.Empty;

            if (String.Compare(aToken, ClientDocumentClassNameToken) == 0)
                return (clientDocumentInfo != null) ? clientDocumentInfo.ClassName : String.Empty;

            if (String.Compare(aToken, ClientDocumentViewClassNameToken) == 0)
            {
                if (clientDocumentInfo == null)
                    return String.Empty;
                // se il nome della classe del clientdoc è "CD" + namespace
                // allora imposta il nome come "C" + namespace + "View" (attuale standard)
                // altrimenti usa nome classe + "View" (vecchio standard)
                //if (clientDocumentInfo.ClassName == "CD" + clientDocumentInfo.Name)
                //    return ("C" + clientDocumentInfo.Name) + "View";
                //else
                //    return clientDocumentInfo.ClassName + "View";

                // se il nome della classe inizia con "CD"
                // allora imposta il nome come "CD" + nomeclasse.mid(2) + "View"
                // altrimenti "CD" + namespace + "View"
                if (clientDocumentInfo.ClassName.Substring(0, 2) == "CD")
                    return "C" + clientDocumentInfo.ClassName.Substring(2) + "View";
                else
                    return "C" + clientDocumentInfo.Name + "View";
            }

            if (String.Compare(aToken, ClientDocumentViewFileNameToken) == 0)
            {
                if (clientDocumentInfo == null)
                    return String.Empty;
                // se il nome della classe del clientdoc è "CD" + namespace
                // allora imposta il nome come "C" + namespace + "View" (attuale standard)
                // altrimenti usa nome classe + "View" (vecchio standard)
                //if (clientDocumentInfo.ClassName == "CD" + clientDocumentInfo.Name)
                    //return ("UI" + clientDocumentInfo.Name);
                //else
                //    return clientDocumentInfo.ClassName + "View";

                // se il nome della classe inizia con "CD"
                // allora imposta il nome come "UI" + nomeclasse.mid(2)
                // altrimenti "UI" + namespace
                if (clientDocumentInfo.ClassName.Substring(0, 2) == "CD")
                    return "UI" + clientDocumentInfo.ClassName.Substring(2);
                else
                    return "UI" + clientDocumentInfo.Name;
            }

            if (String.Compare(aToken, ClientDocumentTitleToken) == 0)
                return (clientDocumentInfo != null) ? clientDocumentInfo.Title : String.Empty;

            if (String.Compare(aToken, ServerDocumentClassNameToken) == 0)
                return (clientDocumentInfo != null && clientDocumentInfo.ServerDocumentInfo != null) ? clientDocumentInfo.ServerDocumentInfo.ClassName : String.Empty;

            if (String.Compare(aToken, ServerDocumentFamilyClassNameToken) == 0)
                return (clientDocumentInfo != null) ? clientDocumentInfo.FamilyToAttachClassName : String.Empty;

            if (String.Compare(aToken, ModuleNameToken) == 0)
                return (clientDocumentInfo != null && clientDocumentInfo.Library != null && clientDocumentInfo.Library.Module != null) ? clientDocumentInfo.Library.Module.Name : String.Empty;

            if (String.Compare(aToken, LibrarySourceFolderToken) == 0)
                return (clientDocumentInfo != null && clientDocumentInfo.Library != null) ? clientDocumentInfo.Library.SourceFolder : String.Empty;

            if (String.Compare(aToken, LibraryNameSpaceToken) == 0)
                return (clientDocumentInfo != null && clientDocumentInfo.Library != null) ? clientDocumentInfo.Library.GetNameSpace() : String.Empty;

            if (aToken.StartsWith(IfAttachedDirectlyToken))
            {
                if (clientDocumentInfo == null || clientDocumentInfo.AttachToFamily)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfAttachedDirectlyToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfAttachedToFamilyToken))
            {
                if (clientDocumentInfo == null || !clientDocumentInfo.AttachToFamily)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfAttachedToFamilyToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfExcludeUnattededModeToken))
            {
                if (clientDocumentInfo == null || !clientDocumentInfo.ExcludeUnattendedMode)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfExcludeUnattededModeToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfIncludeUnattededModeToken))
            {
                if (clientDocumentInfo == null || clientDocumentInfo.ExcludeUnattendedMode)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfIncludeUnattededModeToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfExcludeBatchToken))
            {
                if (clientDocumentInfo == null || !clientDocumentInfo.ExcludeBatchMode)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfExcludeBatchToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfIncludeBatchToken))
            {
                if (clientDocumentInfo == null || clientDocumentInfo.ExcludeBatchMode)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfIncludeBatchToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfMsgRoutingModeBothToken))
            {
                if (clientDocumentInfo == null || !clientDocumentInfo.AreEventsRoutedBefore || !clientDocumentInfo.AreEventsRoutedAfter)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfMsgRoutingModeBothToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfNoMsgRoutingModeBothToken))
            {
                if (clientDocumentInfo == null || (clientDocumentInfo.AreEventsRoutedBefore && clientDocumentInfo.AreEventsRoutedAfter))
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfNoMsgRoutingModeBothToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfMsgRoutingModeBeforeToken))
            {
                if (clientDocumentInfo == null || !clientDocumentInfo.AreEventsRoutedBefore)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfMsgRoutingModeBeforeToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfMsgRoutingModeAfterToken))
            {
                if (clientDocumentInfo == null || !clientDocumentInfo.AreEventsRoutedAfter)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfMsgRoutingModeAfterToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfDBTsPresentToken))
            {
                if (clientDocumentInfo == null || clientDocumentInfo.DBTsCount == 0)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfDBTsPresentToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(RepeatOnDBTsToken))
            {
                if (clientDocumentInfo == null || clientDocumentInfo.DBTsCount == 0)
                    return String.Empty;

                string subToken;
                string dbtsLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnDBTsToken, out subToken, out dbtsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string dbtsLines = String.Empty;
                    foreach (WizardDBTInfo aDBTInfo in clientDocumentInfo.DBTsInfo)
                    {
                        string dbtParsedText = SubstituteDBTTokens(subToken, aDBTInfo);

                        if (dbtParsedText != null && dbtParsedText.Length > 0)
                        {
                            if (dbtsLines.Length > 0)
                                dbtsLines += dbtsLinesSeparator;
                            dbtsLines += dbtParsedText;
                        }
                    }
                    return dbtsLines;
                }
            }

            if (aToken.StartsWith(RepeatOnTabbedPanesToken))
            {
                if (clientDocumentInfo == null || clientDocumentInfo.TabbedPanesCount == 0)
                    return String.Empty;

                string subToken;
                string dbtsLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnTabbedPanesToken, out subToken, out dbtsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string tabbedPaneLines = String.Empty;
                    foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in clientDocumentInfo.TabbedPanes)
                    {
                        string tabbedPaneParsedText = SubstituteTabbedPaneTokens(subToken, aTabbedPaneInfo);

                        if (tabbedPaneParsedText != null && tabbedPaneParsedText.Length > 0)
                        {
                            if (tabbedPaneLines.Length > 0)
                                tabbedPaneLines += dbtsLinesSeparator;
                            tabbedPaneLines += tabbedPaneParsedText;
                        }
                    }
                    return tabbedPaneLines;
                }
            }

            if (aToken.StartsWith(RepeatOnServerDocumentIncludeFilesToken))
            {
                if (clientDocumentInfo == null || clientDocumentInfo.ServerHeaderFilesToincludeCount == 0)
                    return String.Empty;

                string subToken;
                string includeFilesLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnServerDocumentIncludeFilesToken, out subToken, out includeFilesLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string includeFilesLines = String.Empty;
                    foreach (string includeFileName in clientDocumentInfo.ServerHeaderFilesToinclude)
                    {
                        if (includeFileName == null || includeFileName.Trim().Length == 0 || !Generics.IsValidFullPathName(includeFileName))
                            continue;

                        WizardCodeFileTemplateParser aCodeFileParser = new WizardCodeFileTemplateParser(clientDocumentInfo.Library, includeFileName);

                        string includeFilesParsedText = aCodeFileParser.SubstituteTokens(subToken);

                        if (includeFilesParsedText != null && includeFilesParsedText.Length > 0)
                        {
                            if (includeFilesLines.Length > 0)
                                includeFilesLines += includeFilesLinesSeparator;
                            includeFilesLines += includeFilesParsedText;
                        }
                    }
                    return includeFilesLines;
                }
            }

            if (String.Compare(aToken, CultureLanguageIdentifierToken) == 0)
                return (clientDocumentInfo != null && clientDocumentInfo.Library != null && clientDocumentInfo.Library.Module != null && clientDocumentInfo.Library.Module.Application != null) ? clientDocumentInfo.Library.Module.Application.GetCultureLanguageIdentifierText() : String.Empty;

            if (String.Compare(aToken, CultureSubLanguageIdentifierToken) == 0)
                return (clientDocumentInfo != null && clientDocumentInfo.Library != null && clientDocumentInfo.Library.Module != null && clientDocumentInfo.Library.Module.Application != null) ? clientDocumentInfo.Library.Module.Application.GetCultureSubLanguageIdentifierText() : String.Empty;

            if (String.Compare(aToken, SlaveFormViewWidthToken) == 0)
                return (dialogSize != Size.Empty) ? dialogSize.Width.ToString() : String.Empty;

            if (String.Compare(aToken, SlaveFormViewHeightToken) == 0)
                return (dialogSize != Size.Empty) ? dialogSize.Height.ToString() : String.Empty;

            if (aToken.StartsWith(IsTabberToCreateToken))
            {
                if (!IsTabberToCreate())
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IsTabberToCreateToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (String.Compare(aToken, TabberControlIdToken) == 0)
            {
                if (!IsTabberToCreate())
                    return String.Empty;

                return clientDocumentInfo.GetTabberControlId().ToString();
            }

            if (String.Compare(aToken, TabberLeftToken) == 0)
            {
                if (!IsTabberToCreate())
                    return String.Empty;

                return WizardDBTCodeTemplateParser.FirstColumnControlLeftCoordinate.ToString();
            }

            if (String.Compare(aToken, TabberTopToken) == 0)
            {
                if (!IsTabberToCreate())
                    return String.Empty;

                return WizardDBTCodeTemplateParser.FirstColumnControlTopCoordinate.ToString();
            }

            if (String.Compare(aToken, TabberWidthToken) == 0)
            {
                if (!IsTabberToCreate())
                    return String.Empty;

                System.Drawing.Font fontToUse = (clientDocumentInfo.Library != null && clientDocumentInfo.Library.Application != null) ? clientDocumentInfo.Library.Application.Font : WizardApplicationInfo.DefaultFont;
                return GetTabberAreaSize(clientDocumentInfo, fontToUse).Width.ToString();
            }

            if (String.Compare(aToken, TabberHeightToken) == 0)
            {
                if (!IsTabberToCreate())
                    return String.Empty;

                System.Drawing.Font fontToUse = (clientDocumentInfo.Library != null && clientDocumentInfo.Library.Application != null) ? clientDocumentInfo.Library.Application.Font : WizardApplicationInfo.DefaultFont;
                return GetTabberAreaSize(clientDocumentInfo, fontToUse).Height.ToString();
            }

            if (String.Compare(aToken, FontSizeToken) == 0)
            {
                if (clientDocumentInfo == null)
                    return String.Empty;

                System.Drawing.Font fontToUse = (clientDocumentInfo.Library != null && clientDocumentInfo.Library.Application != null) ? clientDocumentInfo.Library.Application.Font : WizardApplicationInfo.DefaultFont;
                return fontToUse.SizeInPoints.ToString(NumberFormatInfo.InvariantInfo);
            }

            if (String.Compare(aToken, FontNameToken) == 0)
            {
                if (clientDocumentInfo == null)
                    return String.Empty;

                System.Drawing.Font fontToUse = (clientDocumentInfo.Library != null && clientDocumentInfo.Library.Application != null) ? clientDocumentInfo.Library.Application.Font : WizardApplicationInfo.DefaultFont;
                return fontToUse.Name;
            }

            if (String.Compare(aToken, FontWeightToken) == 0)
            {
                if (clientDocumentInfo == null)
                    return String.Empty;

                System.Drawing.Font fontToUse = (clientDocumentInfo.Library != null && clientDocumentInfo.Library.Application != null) ? clientDocumentInfo.Library.Application.Font : WizardApplicationInfo.DefaultFont;
                return Generics.GetFontWeight(fontToUse).ToString();
            }

            if (String.Compare(aToken, FontIsItalicToken) == 0)
            {
                if (clientDocumentInfo == null)
                    return String.Empty;

                System.Drawing.Font fontToUse = (clientDocumentInfo.Library != null && clientDocumentInfo.Library.Application != null) ? clientDocumentInfo.Library.Application.Font : WizardApplicationInfo.DefaultFont;
                return fontToUse.Italic ? "1" : "0";
            }

            if (String.Compare(aToken, FontCharSetToken) == 0)
            {
                if (clientDocumentInfo == null)
                    return String.Empty;

                System.Drawing.Font fontToUse = (clientDocumentInfo.Library != null && clientDocumentInfo.Library.Application != null) ? clientDocumentInfo.Library.Application.Font : WizardApplicationInfo.DefaultFont;
                return Generics.GetFontCharSet(fontToUse).ToString();
            }

            if (aToken.StartsWith(IfInterfacePresentToken))
            {
                if (!IsInterfacePresent())
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfInterfacePresentToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(AddTabsToServerDocumentTabberToken))
            {
                if (!AddTabDialogs())
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, AddTabsToServerDocumentTabberToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IsSlaveFormViewToCreateToken))
            {
                if (!CreateSlaveFormView())
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IsSlaveFormViewToCreateToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (String.Compare(aToken, ClientDocumentSlaveFormViewIdToken) == 0)
                return CreateSlaveFormView() ? clientDocumentInfo.GetSlaveFormViewId().ToString() : String.Empty;

            if (String.Compare(aToken, ClientDocumentCommandIdToken) == 0)
                return CreateSlaveFormView() ? clientDocumentInfo.GetSlaveFormViewCommandId().ToString() : String.Empty;

            if (String.Compare(aToken, ToolBarButtonLargeIdToken) == 0)
                return CreateSlaveFormView() ? clientDocumentInfo.GetToolBarButtonLargeId().ToString() : String.Empty;

            if (String.Compare(aToken, ToolBarButtonSmallIdToken) == 0)
                return CreateSlaveFormView() ? clientDocumentInfo.GetToolBarButtonSmallId().ToString() : String.Empty;

            if (String.Compare(aToken, ToolBarButtonLargeDisabledIdToken) == 0)
                return CreateSlaveFormView() ? clientDocumentInfo.GetToolBarButtonLargeDisabledId().ToString() : String.Empty;

            if (String.Compare(aToken, ToolBarButtonSmallDisabledIdToken) == 0)
                return CreateSlaveFormView() ? clientDocumentInfo.GetToolBarButtonSmallDisabledId().ToString() : String.Empty;

            if (String.Compare(aToken, ClientDocumentNextResourceValueToken) == 0)
                return (clientDocumentInfo != null) ? clientDocumentInfo.GetFirstAvailableResourceId().ToString() : String.Empty;

            if (String.Compare(aToken, ClientDocumentNextControlValueToken) == 0)
                return (clientDocumentInfo != null) ? clientDocumentInfo.GetFirstAvailableControlId().ToString() : String.Empty;

            if (String.Compare(aToken, ClientDocumentNextCommandValueToken) == 0)
                return (clientDocumentInfo != null) ? clientDocumentInfo.GetFirstAvailableCommandId().ToString() : String.Empty;

            if (String.Compare(aToken, ClientDocumentNextSymedValueToken) == 0)
                return (clientDocumentInfo != null) ? clientDocumentInfo.GetFirstAvailableSymedId().ToString() : String.Empty;

            return null;
        }

        //----------------------------------------------------------------------------
        private string SubstituteDBTTokens
            (
            string source,
            WizardDBTInfo aDBTInfo,
            bool preserveUnresolvedTokens
            )
        {
            if (source == null || aDBTInfo == null)
                return null;

            source = source.Trim();
            if (source.Length == 0)
                return String.Empty;

            string dbtParsedText = this.SubstituteTokens(source, true);

            if (AreTokensToSubstitute(dbtParsedText))
            {
                WizardDBTCodeTemplateParser dbtParser = new WizardDBTCodeTemplateParser(aDBTInfo, clientDocumentInfo);

                dbtParsedText = dbtParser.SubstituteTokens(dbtParsedText, true);
            }

            return dbtParsedText;
        }

        //----------------------------------------------------------------------------
        private string SubstituteDBTTokens(string source, WizardDBTInfo aDBTInfo)
        {
            return SubstituteDBTTokens(source, aDBTInfo, false);
        }

        //----------------------------------------------------------------------------
        private string SubstituteTabbedPaneTokens
            (
            string source,
            WizardDocumentTabbedPaneInfo aTabbedPaneInfo,
            bool preserveUnresolvedTokens
            )
        {
            if (source == null || aTabbedPaneInfo == null)
                return null;

            source = source.Trim();
            if (source.Length == 0)
                return String.Empty;

            string tabbedPaneParsedText = this.SubstituteTokens(source, true);

            if (AreTokensToSubstitute(tabbedPaneParsedText))
            {
                WizardTabbedPaneCodeTemplateParser tabbedPaneParser = new WizardTabbedPaneCodeTemplateParser(aTabbedPaneInfo, clientDocumentInfo);

                tabbedPaneParsedText = tabbedPaneParser.SubstituteTokens(tabbedPaneParsedText, true);
            }

            return tabbedPaneParsedText;
        }

        //----------------------------------------------------------------------------
        private string SubstituteTabbedPaneTokens(string source, WizardDocumentTabbedPaneInfo aTabbedPaneInfo)
        {
            return SubstituteTabbedPaneTokens(source, aTabbedPaneInfo, false);
        }

        //----------------------------------------------------------------------------
        internal static System.Drawing.Size GetDialogSize(WizardClientDocumentInfo aClientDocumentInfo, System.Drawing.Font aFont)
        {
            if (aClientDocumentInfo == null || aClientDocumentInfo.DBTsCount == 0)
                return Size.Empty;

            int clientDocumentDialogWidth = 0;
            int clientDocumentDialogHeight = 0;

            Size tabberAreaSize = GetTabberAreaSize(aClientDocumentInfo, aFont);

            clientDocumentDialogWidth = tabberAreaSize.Width;

            if (tabberAreaSize.Height > 0)
            {
                clientDocumentDialogHeight = WizardDBTCodeTemplateParser.FirstColumnControlTopCoordinate;
                clientDocumentDialogHeight += tabberAreaSize.Height;
                clientDocumentDialogHeight += WizardDBTCodeTemplateParser.FirstColumnControlTopCoordinate;
            }

            return new Size(clientDocumentDialogWidth, clientDocumentDialogHeight);
        }

        //----------------------------------------------------------------------------
        internal static bool IsTabberToCreate(WizardClientDocumentInfo aClientDocumentInfo)
        {
            return (aClientDocumentInfo != null && aClientDocumentInfo.IsTabberToCreate());
        }

        //----------------------------------------------------------------------------
        internal bool IsTabberToCreate()
        {
            return (clientDocumentInfo != null && clientDocumentInfo.IsTabberToCreate());
        }

        //----------------------------------------------------------------------------
        internal static bool IsInterfacePresent(WizardClientDocumentInfo aClientDocumentInfo)
        {
            return (aClientDocumentInfo != null && aClientDocumentInfo.IsInterfacePresent);
        }

        //----------------------------------------------------------------------------
        internal bool IsInterfacePresent()
        {
            return (clientDocumentInfo != null && clientDocumentInfo.IsInterfacePresent);
        }

        //----------------------------------------------------------------------------
        internal static bool CreateSlaveFormView(WizardClientDocumentInfo aClientDocumentInfo)
        {
            return (aClientDocumentInfo != null && aClientDocumentInfo.CreateSlaveFormView);
        }

        //----------------------------------------------------------------------------
        internal bool CreateSlaveFormView()
        {
            return CreateSlaveFormView(clientDocumentInfo);
        }

        //----------------------------------------------------------------------------
        internal static bool AddTabDialogs(WizardClientDocumentInfo aClientDocumentInfo)
        {
            return (aClientDocumentInfo != null && aClientDocumentInfo.AddTabDialogs);
        }

        //----------------------------------------------------------------------------
        internal bool AddTabDialogs()
        {
            return AddTabDialogs(clientDocumentInfo);
        }

        //----------------------------------------------------------------------------
        internal static System.Drawing.Size GetTabberAreaSize(WizardClientDocumentInfo aClientDocumentInfo, System.Drawing.Font aFont)
        {
            WizardDocumentTabbedPaneInfoCollection tabbedPanes = aClientDocumentInfo.TabbedPanes;
            if (tabbedPanes == null || tabbedPanes.Count == 0)
                return Size.Empty;

            int tabberAreaWidth = 0;
            int tabberAreaHeight = 0;

            int maxTabbedPaneWidth = 0;
            int maxTabbedPaneHeight = 0;

            foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in tabbedPanes)
            {
                System.Drawing.Size tabbedPaneSize = WizardTabbedPaneCodeTemplateParser.GetTabbedPaneSize(aTabbedPaneInfo, aFont);
                if (tabbedPaneSize == Size.Empty)
                    continue;

                if (maxTabbedPaneWidth < tabbedPaneSize.Width)
                    maxTabbedPaneWidth = tabbedPaneSize.Width;

                if (maxTabbedPaneHeight < tabbedPaneSize.Height)
                    maxTabbedPaneHeight = tabbedPaneSize.Height;
            }

            if (maxTabbedPaneWidth > 0)
                tabberAreaWidth = maxTabbedPaneWidth + (2 * WizardTabbedPaneCodeTemplateParser.FirstColumnControlLeftCoordinate);
            else
                tabberAreaWidth = WizardTabbedPaneCodeTemplateParser.DefaultTabberAreaWidth;

            if (maxTabbedPaneHeight > 0)
                tabberAreaHeight = maxTabbedPaneHeight + (2 * WizardTabbedPaneCodeTemplateParser.FirstColumnControlTopCoordinate);
            else
                tabberAreaHeight = WizardTabbedPaneCodeTemplateParser.DefaultTabberAreaHeight;

            return new Size(tabberAreaWidth, tabberAreaHeight);
        }

        //----------------------------------------------------------------------------
        private void InitDialogSize()
        {
            System.Drawing.Font fontToUse = (clientDocumentInfo != null && clientDocumentInfo.Library != null && clientDocumentInfo.Library.Application != null) ? clientDocumentInfo.Library.Application.Font : WizardApplicationInfo.DefaultFont;
            dialogSize = GetDialogSize(clientDocumentInfo, fontToUse);
        }
    }

    #endregion

    #region WizardCodeFileTemplateParser class

    //============================================================================
    internal class WizardCodeFileTemplateParser : CodeTemplateParser
    {
        private const string RelativeFileNameToken = "RelativeFileName";

        private WizardLibraryInfo libraryInfo = null;
        private string filename = String.Empty;

        //----------------------------------------------------------------------------
        internal WizardCodeFileTemplateParser(WizardLibraryInfo aLibraryInfo, string aFilename)
        {
            libraryInfo = aLibraryInfo;
            if (Generics.IsValidFullPathName(aFilename))
                filename = aFilename;
            else
                Debug.Fail("Invalid file name in WizardCodeFileTemplateParser constructor.");
        }

        //----------------------------------------------------------------------------
        internal override string GetTokenValue(string aToken)
        {
            if (aToken == null)
                return null;

            aToken = aToken.Trim();
            if (aToken.Length == 0)
                return String.Empty;

            if (String.Compare(aToken, RelativeFileNameToken) == 0)
            {
                if (libraryInfo == null || filename == null || filename.Trim().Length == 0)
                    return String.Empty;

                string libraryPath = WizardCodeGenerator.GetStandardLibraryPath(libraryInfo);
                if (libraryPath == null || libraryPath.Length == 0)
                    return String.Empty;

                return Generics.MakeRelativeTo(libraryPath, filename);
            }

            return null;
        }
    }

    #endregion

    #region WizardHistoryStepCodeTemplateParser class

    //============================================================================
    internal class WizardHistoryStepCodeTemplateParser : CodeTemplateParser
    {
        private const string IfAreColumnsToAddToken = "IfAreColumnsToAdd";
        private const string RepeatOnAddedColumnsToken = "RepeatOnAddedColumns";
        private const string IfAreColumnsToAlterToken = "IfAreColumnsToAlter";
        private const string RepeatOnColumnsToAlterToken = "RepeatOnColumnsToAlter";
        private const string RepeatOnAlteredColumnsToken = "RepeatOnAlteredColumns";
        private const string RepeatOnDefaultChangedColumnsToken = "RepeatOnDefaultChangedColumns";
        private const string IfAreColumnsToDropToken = "IfAreColumnsToDrop";
        private const string RepeatOnDroppedColumnsToken = "RepeatOnDroppedColumns";
        private const string IfAreColumnsRenamedToken = "IfAreColumnsRenamed";
        private const string RepeatOnRenamedColumnsToken = "RepeatOnRenamedColumns";
        private const string IfAreForeignKeysToAddToken = "IfAreForeignKeysToAdd";
        private const string RepeatOnForeignKeysToAddToken = "RepeatOnForeignKeysToAdd";
        private const string IfAreForeignKeysToDropToken = "IfAreForeignKeysToDrop";
        private const string RepeatOnForeignKeysToDropToken = "RepeatOnForeignKeysToDrop";

        private TableHistoryStep historyStep = null;
        private IWizardTableCodeTemplateParser tableCodeTemplateParser = null;

        #region AlteredColumnInfo class

        internal class AlteredColumnInfo
        {
            private WizardTableColumnInfo columnInfo = null;
            private WizardTableColumnInfo previousColumnInfo = null;

            internal AlteredColumnInfo(WizardTableColumnInfo aColumnInfo, WizardTableColumnInfo aPreviousColumnInfo)
            {
                columnInfo = aColumnInfo;
                previousColumnInfo = aPreviousColumnInfo;
            }

            internal WizardTableColumnInfo ColumnInfo { get { return columnInfo; } }
            internal WizardTableColumnInfo PreviousColumnInfo { get { return previousColumnInfo; } }
        };

        #endregion // AlteredColumnInfo class

        //----------------------------------------------------------------------------
        internal WizardHistoryStepCodeTemplateParser(TableHistoryStep aHistoryStep, IWizardTableCodeTemplateParser aTableCodeTemplateParser)
        {
            historyStep = aHistoryStep;
            tableCodeTemplateParser = aTableCodeTemplateParser;
        }

        //----------------------------------------------------------------------------
        internal override string GetTokenValue(string aToken)
        {
            if (aToken == null)
                return null;

            if (aToken.StartsWith(IfAreColumnsToAddToken))
            {
                if (historyStep == null || !historyStep.HasAddColumnEvents())
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfAreColumnsToAddToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(RepeatOnAddedColumnsToken))
            {
                if (tableCodeTemplateParser == null || historyStep == null || !historyStep.HasAddColumnEvents())
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnAddedColumnsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == String.Empty || subToken.Length == 0)
                        return String.Empty;

                    string columnsLines = String.Empty;
                    foreach (ColumnHistoryEvent aColumnEvent in historyStep.ColumnsEvents)
                    {
                        if (aColumnEvent.Type != TableHistoryStep.EventType.AddColumn || aColumnEvent.ColumnInfo == null)
                            continue;

                        string columnParsedText = tableCodeTemplateParser.SubstituteTableColumnTokens(subToken, aColumnEvent.ColumnInfo);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }
                    }
                    return columnsLines;
                }
            }

            if (aToken.StartsWith(IfAreColumnsToAlterToken))
            {
                if (historyStep == null || !historyStep.HasAlterColumnEvents())
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfAreColumnsToAlterToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(RepeatOnColumnsToAlterToken))
            {
                if (tableCodeTemplateParser == null || historyStep == null || !historyStep.HasAlterColumnEvents())
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnColumnsToAlterToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == String.Empty || subToken.Length == 0)
                        return String.Empty;

                    string columnsLines = String.Empty;
                    foreach (ColumnHistoryEvent aColumnEvent in historyStep.ColumnsEvents)
                    {
                        if
                            (
                            (aColumnEvent.Type != TableHistoryStep.EventType.AddColumn && aColumnEvent.Type != TableHistoryStep.EventType.AlterColumnType && aColumnEvent.Type != TableHistoryStep.EventType.ChangeColumnDefaultValue) ||
                            aColumnEvent.PreviousColumnInfo == null
                            )
                            continue;

                        string columnParsedText = tableCodeTemplateParser.SubstituteTableColumnTokens(subToken, aColumnEvent.PreviousColumnInfo);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }
                    }
                    return columnsLines;
                }
            }

            if (aToken.StartsWith(RepeatOnAlteredColumnsToken))
            {
                if (tableCodeTemplateParser == null || historyStep == null || !historyStep.HasAlterColumnEvents())
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnAlteredColumnsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == String.Empty || subToken.Length == 0)
                        return String.Empty;

                    WizardTableColumnInfoCollection alteredColumns = new WizardTableColumnInfoCollection();
                    foreach (ColumnHistoryEvent aColumnEvent in historyStep.ColumnsEvents)
                    {
                        if
                            (
                            (aColumnEvent.Type != TableHistoryStep.EventType.AlterColumnType && aColumnEvent.Type != TableHistoryStep.EventType.ChangeColumnDefaultValue) ||
                            aColumnEvent.ColumnInfo == null ||
                            aColumnEvent.ColumnInfo.Name == null ||
                            aColumnEvent.ColumnInfo.Name.Length == 0
                            )
                            continue;

                        // Se la colonna alterata è una colonna che viene aggiunta in questo step
                        // non la devo comprendere nel ciclo corrente
                        if (historyStep.GetColumnEventByNameAndType(aColumnEvent.ColumnInfo.Name, TableHistoryStep.EventType.AddColumn) != null)
                            continue;

                        WizardTableColumnInfo alreadyFoundColumn = null;
                        if (alteredColumns.Count > 0)
                        {
                            for (int i = 0; i < alteredColumns.Count; i++)
                            {
                                if (String.Compare(aColumnEvent.ColumnInfo.Name, alteredColumns[i].Name, true) == 0)
                                {
                                    alreadyFoundColumn = alteredColumns[i];
                                    break;
                                }
                            }
                            if (alreadyFoundColumn != null)
                            {
                                if (aColumnEvent.Type == TableHistoryStep.EventType.AlterColumnType)
                                {
                                    alreadyFoundColumn.DataType = aColumnEvent.ColumnInfo.DataType;
                                    alreadyFoundColumn.DataLength = aColumnEvent.ColumnInfo.DataLength;
                                }
                                else if (aColumnEvent.Type == TableHistoryStep.EventType.ChangeColumnDefaultValue)
                                {
                                    alreadyFoundColumn.DefaultValue = aColumnEvent.ColumnInfo.DefaultValue;
                                }
                                continue;
                            }
                        }
                        alteredColumns.Add(aColumnEvent.ColumnInfo);
                    }

                    if (alteredColumns.Count == 0)
                        return String.Empty;

                    string columnsLines = String.Empty;
                    foreach (WizardTableColumnInfo aAlteredColumn in alteredColumns)
                    {
                        string columnParsedText = tableCodeTemplateParser.SubstituteTableColumnTokens(subToken, aAlteredColumn);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }
                    }
                    return columnsLines;
                }
            }

            if (aToken.StartsWith(RepeatOnDefaultChangedColumnsToken))
            {
                if (tableCodeTemplateParser == null || historyStep == null || !historyStep.HasAlterColumnEvents())
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnDefaultChangedColumnsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == String.Empty || subToken.Length == 0)
                        return String.Empty;

                    ArrayList alteredColumns = new ArrayList();
                    foreach (ColumnHistoryEvent aColumnEvent in historyStep.ColumnsEvents)
                    {
                        if
                            (
                            aColumnEvent.Type != TableHistoryStep.EventType.ChangeColumnDefaultValue ||
                            aColumnEvent.ColumnInfo == null ||
                            aColumnEvent.ColumnInfo.Name == null ||
                            aColumnEvent.ColumnInfo.Name.Length == 0
                            )
                            continue;

                        // Se la colonna alterata è una colonna che viene aggiunta in questo step
                        // non la devo comprendere nel ciclo corrente
                        if (historyStep.GetColumnEventByNameAndType(aColumnEvent.ColumnInfo.Name, TableHistoryStep.EventType.AddColumn) != null)
                            continue;

                        WizardTableColumnInfo alreadyFoundColumn = null;
                        if (alteredColumns.Count > 0)
                        {
                            foreach (AlteredColumnInfo aAlteredColumnInfo in alteredColumns)
                            {
                                if (String.Compare(aColumnEvent.ColumnInfo.Name, aAlteredColumnInfo.ColumnInfo.Name, true) == 0)
                                {
                                    alreadyFoundColumn = aAlteredColumnInfo.ColumnInfo;
                                    break;
                                }
                            }
                            if (alreadyFoundColumn != null)
                            {
                                alreadyFoundColumn.DefaultValue = aColumnEvent.ColumnInfo.DefaultValue;
                                continue;
                            }
                        }
                        alteredColumns.Add(new AlteredColumnInfo(aColumnEvent.ColumnInfo, aColumnEvent.PreviousColumnInfo));
                    }

                    if (alteredColumns.Count == 0)
                        return String.Empty;

                    string columnsLines = String.Empty;
                    foreach (AlteredColumnInfo aAlteredColumnInfo in alteredColumns)
                    {
                        string columnParsedText = tableCodeTemplateParser.SubstituteTableColumnTokens(subToken, aAlteredColumnInfo.ColumnInfo, aAlteredColumnInfo.PreviousColumnInfo);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }
                    }
                    return columnsLines;
                }
            }

            if (aToken.StartsWith(IfAreColumnsToDropToken))
            {
                if (historyStep == null || !historyStep.HasDropColumnEvents())
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfAreColumnsToDropToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(RepeatOnDroppedColumnsToken))
            {
                if (tableCodeTemplateParser == null || historyStep == null || !historyStep.HasDropColumnEvents())
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnDroppedColumnsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == String.Empty || subToken.Length == 0)
                        return String.Empty;

                    string columnsLines = String.Empty;
                    foreach (ColumnHistoryEvent aColumnEvent in historyStep.ColumnsEvents)
                    {
                        if (aColumnEvent.Type != TableHistoryStep.EventType.DropColumn || aColumnEvent.ColumnInfo == null)
                            continue;

                        // Mi ricavo le informazioni della colonna relative alla precedente versione di database
                        WizardTableColumnInfo previousColumnInfo = aColumnEvent.PreviousColumnInfo;
                        if (previousColumnInfo == null && tableCodeTemplateParser is WizardTableHistoryStepCodeTemplateParser)
                            previousColumnInfo = ((WizardTableHistoryStepCodeTemplateParser)tableCodeTemplateParser).GetPreviousColumnInfo(aColumnEvent.ColumnInfo.Name);

                        string columnParsedText = tableCodeTemplateParser.SubstituteTableColumnTokens(subToken, aColumnEvent.ColumnInfo, previousColumnInfo);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }
                    }
                    return columnsLines;
                }
            }

            if (aToken.StartsWith(IfAreColumnsRenamedToken))
            {
                if (historyStep == null || !historyStep.HasRenameColumnEvents())
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfAreColumnsRenamedToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(RepeatOnRenamedColumnsToken))
            {
                if (tableCodeTemplateParser == null || historyStep == null || !historyStep.HasRenameColumnEvents())
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnRenamedColumnsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == String.Empty || subToken.Length == 0)
                        return String.Empty;

                    string columnsLines = String.Empty;
                    foreach (ColumnHistoryEvent aColumnEvent in historyStep.ColumnsEvents)
                    {
                        if
                            (
                            aColumnEvent.Type != TableHistoryStep.EventType.RenameColumn ||
                            aColumnEvent.ColumnInfo == null ||
                            aColumnEvent.PreviousColumnInfo == null ||
                            aColumnEvent.ColumnInfo.DataType.Type != aColumnEvent.PreviousColumnInfo.DataType.Type
                            )
                            continue;

                        string columnParsedText = tableCodeTemplateParser.SubstituteTableColumnTokens(subToken, aColumnEvent.ColumnInfo, aColumnEvent.PreviousColumnInfo);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }
                    }
                    return columnsLines;
                }
            }

            if (aToken.StartsWith(IfAreForeignKeysToAddToken))
            {
                if (historyStep == null || !historyStep.AreForeignKeysToAdd())
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfAreForeignKeysToAddToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(RepeatOnForeignKeysToAddToken))
            {
                if (tableCodeTemplateParser == null || historyStep == null || historyStep.ForeignKeyEventsCount == 0)
                    return String.Empty;

                string subToken;
                string foreignKeysLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnForeignKeysToAddToken, out subToken, out foreignKeysLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string foreignKeysLines = String.Empty;
                    foreach (ForeignKeyHistoryEvent aForeignKeyEvent in historyStep.ForeignKeyEvents)
                    {
                        if
                            (
                            aForeignKeyEvent.Type != TableHistoryStep.EventType.CreateConstraint ||
                            aForeignKeyEvent.ForeignKeyInfo == null
                            )
                            continue;

                        string foreignKeyParsedText = tableCodeTemplateParser.SubstituteForeignKeyTokens(subToken, aForeignKeyEvent.ForeignKeyInfo);

                        if (foreignKeyParsedText != null && foreignKeyParsedText.Length > 0)
                        {
                            if (foreignKeysLines.Length > 0)
                                foreignKeysLines += foreignKeysLinesSeparator;
                            foreignKeysLines += foreignKeyParsedText;
                        }
                    }
                    return foreignKeysLines;
                }
            }

            if (aToken.StartsWith(IfAreForeignKeysToDropToken))
            {
                if (historyStep == null || !historyStep.AreForeignKeysToDrop())
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfAreForeignKeysToDropToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(RepeatOnForeignKeysToDropToken))
            {
                if (tableCodeTemplateParser == null || historyStep == null || historyStep.ForeignKeyEventsCount == 0)
                    return String.Empty;

                string subToken;
                string foreignKeysLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnForeignKeysToDropToken, out subToken, out foreignKeysLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string foreignKeysLines = String.Empty;
                    foreach (ForeignKeyHistoryEvent aForeignKeyEvent in historyStep.ForeignKeyEvents)
                    {
                        if
                            (
                            aForeignKeyEvent.Type != TableHistoryStep.EventType.DropConstraint ||
                            aForeignKeyEvent.ForeignKeyInfo == null
                            )
                            continue;

                        string foreignKeyParsedText = tableCodeTemplateParser.SubstituteForeignKeyTokens(subToken, aForeignKeyEvent.ForeignKeyInfo);

                        if (foreignKeyParsedText != null && foreignKeyParsedText.Length > 0)
                        {
                            if (foreignKeysLines.Length > 0)
                                foreignKeysLines += foreignKeysLinesSeparator;
                            foreignKeysLines += foreignKeyParsedText;
                        }
                    }
                    return foreignKeysLines;
                }
            }

            return (tableCodeTemplateParser != null) ? tableCodeTemplateParser.GetTokenValue(aToken) : null;
        }
    }

    #endregion // WizardHistoryStepCodeTemplateParser class

    #region WizardTableHistoryStepCodeTemplateParser class

    //============================================================================
    internal class WizardTableHistoryStepCodeTemplateParser : WizardTableCodeTemplateParser, IWizardTableCodeTemplateParser
    {
        private const string IfIsPrimaryKeyToModifyToken = "IfIsPrimaryKeyToModify";
        private const string ApplyToPreviousTableVersionToken = "ApplyToPreviousTableVersion";

        TableHistoryStep historyStep = null;
        private WizardHistoryStepCodeTemplateParser historyStepParser = null;

        //----------------------------------------------------------------------------
		internal WizardTableHistoryStepCodeTemplateParser(WizardTableInfo aTableInfo, uint aDbReleaseNumber, DBMSType aDBType)
            :
            base(aTableInfo, aDBType)
        {
            if (aTableInfo != null && aTableInfo.History != null)
            {
                historyStep = aTableInfo.History.GetDbReleaseStep(aDbReleaseNumber);
                if (historyStep != null)
                    historyStepParser = new WizardHistoryStepCodeTemplateParser(historyStep, this);
            }
        }

        //----------------------------------------------------------------------------
        string IWizardTableCodeTemplateParser.GetTokenValue(string aToken)
        {
            return base.GetTokenValue(aToken);
        }

        //----------------------------------------------------------------------------
        internal override string GetTokenValue(string aToken)
        {
            if (aToken == null)
                return null;

            if (aToken.StartsWith(IfIsPrimaryKeyToModifyToken))
            {
                if (historyStep == null || !historyStep.IsPrimaryKeyToModify())
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfIsPrimaryKeyToModifyToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(ApplyToPreviousTableVersionToken))
            {
                if (tableInfo == null || historyStep == null || historyStep.DbReleaseNumber == 1)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, ApplyToPreviousTableVersionToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    WizardTableInfo previousTable = new WizardTableInfo(tableInfo);
                    previousTable.RollbackToDbRelease(historyStep.DbReleaseNumber - 1);

                    WizardTableCodeTemplateParser previousTableParser = new WizardTableCodeTemplateParser(previousTable);

                    return previousTableParser.SubstituteTokens(subToken);
                }
            }

            string parsedToken = base.GetTokenValue(aToken);
            if (parsedToken != null)
                return parsedToken;

            return (historyStepParser != null) ? historyStepParser.GetTokenValue(aToken) : null;
        }

        //----------------------------------------------------------------------------
        internal WizardTableColumnInfo GetPreviousColumnInfo(string aColumnName)
        {
            if
                (
                base.tableInfo == null ||
                historyStep == null ||
                aColumnName == null ||
                aColumnName.Length == 0
                )
                return null;

            WizardTableInfo previousTable = new WizardTableInfo(base.tableInfo);
            previousTable.RollbackToDbRelease(historyStep.DbReleaseNumber - 1);

            return previousTable.GetColumnInfoByName(aColumnName);
        }

    }

    #endregion // WizardTableHistoryStepCodeTemplateParser class

    #region WizardAdditionalColumnsHistoryStepCodeTemplateParser class

    //============================================================================
    internal class WizardAdditionalColumnsHistoryStepCodeTemplateParser : WizardAdditionalColumnsCodeTemplateParser, IWizardTableCodeTemplateParser
    {
        private const string ApplyToPreviousTableVersionToken = "ApplyToPreviousTableVersion";

        TableHistoryStep historyStep = null;
        private WizardHistoryStepCodeTemplateParser historyStepParser = null;

        //----------------------------------------------------------------------------
		internal WizardAdditionalColumnsHistoryStepCodeTemplateParser(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo, uint aDbReleaseNumber, DBMSType aDBType)
            :
            base(aExtraAddedColumnsInfo, aDBType)
        {
            if (aExtraAddedColumnsInfo != null && aExtraAddedColumnsInfo.History != null)
            {
                historyStep = aExtraAddedColumnsInfo.History.GetDbReleaseStep(aDbReleaseNumber);
                if (historyStep != null)
                    historyStepParser = new WizardHistoryStepCodeTemplateParser(historyStep, this);
            }
        }

        //----------------------------------------------------------------------------
        string IWizardTableCodeTemplateParser.GetTokenValue(string aToken)
        {
            return base.GetTokenValue(aToken);
        }

        //----------------------------------------------------------------------------
        internal override string GetTokenValue(string aToken)
        {
            if (aToken == null)
                return null;

            if (aToken.StartsWith(ApplyToPreviousTableVersionToken))
            {
                if (additionalColumnsInfo == null || historyStep == null || historyStep.DbReleaseNumber == 1)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, ApplyToPreviousTableVersionToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    WizardExtraAddedColumnsInfo previousAdditionalColums = new WizardExtraAddedColumnsInfo(additionalColumnsInfo);
                    previousAdditionalColums.RollbackToDbRelease(historyStep.DbReleaseNumber - 1);

                    WizardAdditionalColumnsCodeTemplateParser previousTableParser = new WizardAdditionalColumnsCodeTemplateParser(previousAdditionalColums);

                    return previousTableParser.SubstituteTokens(subToken);
                }
            }

            string parsedToken = base.GetTokenValue(aToken);
            if (parsedToken != null)
                return parsedToken;

            return (historyStepParser != null) ? historyStepParser.GetTokenValue(aToken) : null;
        }
    }

    #endregion // WizardAdditionalColumnsHistoryStepCodeTemplateParser class

    #region WizardForeignKeyCodeTemplateParser class

    //============================================================================
    internal class WizardForeignKeyCodeTemplateParser : CodeTemplateParser
    {
        private const string ConstraintNameToken = "ConstraintName";
        private const string ReferencedTableNameToken = "ReferencedTableName";
		
		private bool isCommaNeeded = true; // gestione virgole tra i constraint di FK
        
		private const string RepeatOnForeignKeySegmentsToken = "RepeatOnForeignKeySegments";
		private const string IfCommaNeededToken = "IfCommaNeeded";
		private const string IfUpdateCascadeToken = "IfUpdateCascade";
		private const string IfDeleteCascadeToken = "IfDeleteCascade";

        private WizardForeignKeyInfo foreignKeyInfo = null;

        //----------------------------------------------------------------------------
        internal WizardForeignKeyCodeTemplateParser(WizardForeignKeyInfo aForeignKeyInfo)
        {
            foreignKeyInfo = aForeignKeyInfo;
        }

		//----------------------------------------------------------------------------
		public bool IsCommaNeeded { set { isCommaNeeded = value; } get { return isCommaNeeded; } }

        //----------------------------------------------------------------------------
        internal override string GetTokenValue(string aToken)
        {
            if (aToken == null)
                return null;

            aToken = aToken.Trim();
            if (aToken.Length == 0)
                return String.Empty;

            if (String.Compare(aToken, ConstraintNameToken) == 0)
                return (foreignKeyInfo != null) ? foreignKeyInfo.ConstraintName : String.Empty;

            if (String.Compare(aToken, ReferencedTableNameToken) == 0)
                return (foreignKeyInfo != null) ? foreignKeyInfo.ReferencedTableName : String.Empty;

			if (aToken.StartsWith(IfCommaNeededToken))
			{
				if (isCommaNeeded)
					return string.Empty;
				else
				{
					string subToken;
					if (ResolveFunctionToken(aToken, IfCommaNeededToken, out subToken))
					{
						if (subToken == null || subToken.Length == 0)
							return String.Empty;

						return this.SubstituteTokens(subToken);
					}
				}
			}

			if (aToken.StartsWith(IfDeleteCascadeToken))
			{
				if (!foreignKeyInfo.OnDeleteCascade)
					return string.Empty;
				string subToken;
				if (ResolveFunctionToken(aToken, IfDeleteCascadeToken, out subToken))
				{
					if (subToken == null || subToken.Length == 0)
						return String.Empty;

					return this.SubstituteTokens(subToken);
				}
			}

			if (aToken.StartsWith(IfUpdateCascadeToken))
			{
				if (!foreignKeyInfo.OnUpdateCascade)
					return string.Empty;
				string subToken;
				if (ResolveFunctionToken(aToken, IfUpdateCascadeToken, out subToken))
				{
					if (subToken == null || subToken.Length == 0)
						return String.Empty;

					return this.SubstituteTokens(subToken);
				}
			}

            if (aToken.StartsWith(RepeatOnForeignKeySegmentsToken))
            {
                if (foreignKeyInfo == null || foreignKeyInfo.SegmentsCount == 0)
                    return String.Empty;

                string subToken;
                string segmentsLinesSeparator;
                if (ResolveFunctionToken(aToken, RepeatOnForeignKeySegmentsToken, out subToken, out segmentsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string segmentsLines = String.Empty;
                    foreach (WizardForeignKeyInfo.KeySegment aSegment in foreignKeyInfo.Segments)
                    {
                        WizardForeignKeySegmentCodeTemplateParser segmentParser = new WizardForeignKeySegmentCodeTemplateParser(aSegment);

                        string segmentParsedText = segmentParser.SubstituteTokens(subToken, true);

                        if (AreTokensToSubstitute(segmentParsedText))
                            segmentParsedText = this.SubstituteTokens(segmentParsedText);

                        if (segmentParsedText != null && segmentParsedText.Length > 0)
                        {
                            if (segmentsLines.Length > 0)
                                segmentsLines += segmentsLinesSeparator;
                            segmentsLines += segmentParsedText;
                        }
                    }
                    return segmentsLines;
                }
            }

            return null;
        }

    }

    #endregion

    #region WizardForeignKeySegmentCodeTemplateParser class

    //============================================================================
    internal class WizardForeignKeySegmentCodeTemplateParser : CodeTemplateParser
    {
        private const string ColumnNameToken = "ColumnName";
        private const string ReferencedColumnNameToken = "ReferencedColumnName";

        private WizardForeignKeyInfo.KeySegment keySegment = null;

        //----------------------------------------------------------------------------
        internal WizardForeignKeySegmentCodeTemplateParser(WizardForeignKeyInfo.KeySegment aKeySegment)
        {
            keySegment = aKeySegment;
        }

        //----------------------------------------------------------------------------
        internal override string GetTokenValue(string aToken)
        {
            if (aToken == null)
                return null;

            aToken = aToken.Trim();
            if (aToken.Length == 0)
                return String.Empty;

            if (String.Compare(aToken, ColumnNameToken) == 0)
                return (keySegment != null) ? keySegment.ColumnName : String.Empty;

            if (String.Compare(aToken, ReferencedColumnNameToken) == 0)
                return (keySegment != null) ? keySegment.ReferencedColumnName : String.Empty;

            return null;
        }

    }

    #endregion

    #region WizardTabbedPaneCodeTemplateParser class

    //============================================================================
    internal class WizardTabbedPaneCodeTemplateParser : WizardDBTCodeTemplateParser
    {
        private WizardDocumentTabbedPaneInfo tabbedPaneInfo = null;
        private System.Drawing.Size tabbedPaneSize = Size.Empty;

        private const string TabbedPaneNameToken = "TabbedPaneName";
        private const string TabTitleToken = "TabTitle";
        private const string FormIdToken = "FormId";
        private const string RepeatOnColumnsToken = "RepeatOnColumns";
        private const string RepeatOnVisibleColumnsToken = "RepeatOnVisibleColumns";
        private const string RepeatOnRowFormColumnsToken = "RepeatOnRowFormColumns";
		private const string RepeatOnLabelsToken = "RepeatOnLabels";
        private const string TabbedPaneWidthToken = "TabbedPaneWidth";
        private const string TabbedPaneHeightToken = "TabbedPaneHeight";
        private const string BodyEditControlIdToken = "BodyEditControlId";
        private const string BodyEditLeftToken = "BodyEditLeft";
        private const string BodyEditTopToken = "BodyEditTop";
        private const string BodyEditWidthToken = "BodyEditWidth";
        private const string BodyEditHeightToken = "BodyEditHeight";
        private const string RepeatOnHotLinkColumnsToken = "RepeatOnHotLinkColumns";
        private const string RepeatOnApplicationHotLinkColumnsToken = "RepeatOnApplicationHotLinkColumns";
        private const string RepeatOnReferencedHotLinkIncludeFilesToken = "RepeatOnReferencedHotLinkIncludeFiles";
        private const string DBTRowFormWidthToken = "DBTRowFormWidth";
        private const string DBTRowFormHeightToken = "DBTRowFormHeight";
        private const string IfShowsAdditionalColumnsToken = "IfShowsAdditionalColumns";
        private const string RepeatOnVisibleAdditionalColumnsInfoToken = "RepeatOnVisibleAdditionalColumnsInfo";
        private const string ReferencedTableHeaderRelativePathToken = "ReferencedTableHeaderRelativePath";
        private const string IfDBTMasterToken = "IfDBTMaster";
        private const string IfNotDBTMasterToken = "IfNotDBTMaster";
        private const string IfDBTSlaveBufferedToken = "IfDBTSlaveBuffered";
        private const string IfNotDBTSlaveBufferedToken = "IfNotDBTSlaveBuffered";
        private const string IfCreateRowFormToken = "IfCreateRowForm";
        private const string IfNotCreateRowFormToken = "IfNotCreateRowForm";
        private const string BodyEditRowFormDialogIdToken = "BodyEditRowFormDialogId";

        //----------------------------------------------------------------------------
        internal WizardTabbedPaneCodeTemplateParser(WizardDocumentTabbedPaneInfo aTabbedPaneInfo, WizardDocumentInfo aDocumentInfo)
            :
            base((aTabbedPaneInfo != null) ? aTabbedPaneInfo.DBTInfo : null, aDocumentInfo)
        {
            tabbedPaneInfo = aTabbedPaneInfo;

            InitTabbedPaneSize();
        }

        //----------------------------------------------------------------------------
        internal WizardTabbedPaneCodeTemplateParser(WizardDocumentTabbedPaneInfo aTabbedPaneInfo, WizardClientDocumentInfo aClientDocumentInfo)
            :
            base((aTabbedPaneInfo != null) ? aTabbedPaneInfo.DBTInfo : null, aClientDocumentInfo)
        {
            tabbedPaneInfo = aTabbedPaneInfo;

            InitTabbedPaneSize();
        }

        //----------------------------------------------------------------------------
        internal WizardTabbedPaneCodeTemplateParser(WizardDocumentTabbedPaneInfo aTabbedPaneInfo)
            : this(aTabbedPaneInfo, (WizardDocumentInfo)null)
        {
        }

        //----------------------------------------------------------------------------
        internal override string GetTokenValue(string aToken)
        {
            if (aToken == null)
                return null;

            aToken = aToken.Trim();
            if (aToken.Length == 0)
                return String.Empty;

            if (String.Compare(aToken, TabbedPaneNameToken) == 0)
            {
                if (tabbedPaneInfo == null || tabbedPaneInfo.DBTInfo == null)
                    return String.Empty;

                object tabbedPaneContext = (this.DocumentInfo != null) ? (object)this.DocumentInfo : this.ClientDocumentInfo;

                return tabbedPaneInfo.GetInternalName(tabbedPaneContext);
            }

            if (String.Compare(aToken, TabTitleToken) == 0)
                return (tabbedPaneInfo != null) ? tabbedPaneInfo.Title : String.Empty;

            if (String.Compare(aToken, FormIdToken) == 0)
            {
                int formId = this.GetTabbedPaneFormId();
                return (formId != -1) ? formId.ToString() : String.Empty;
            }

            if (aToken.StartsWith(RepeatOnColumnsToken))
            {
                if (tabbedPaneInfo == null || tabbedPaneInfo.ColumnsCount == 0)
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnColumnsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string columnsLines = String.Empty;
                    foreach (WizardDBTColumnInfo aColumnInfo in tabbedPaneInfo.ColumnsInfo)
                    {
                        string columnParsedText = SubstituteTabbedPaneColumnTokens(subToken, aColumnInfo);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }
                    }
                    return columnsLines;
                }
            }

			if (aToken.StartsWith(RepeatOnLabelsToken))
			{
				if (tabbedPaneInfo == null)
					return string.Empty;

				string subToken;
				string columnsLinesSeparator;
				string columnsLines = String.Empty;

				if (ResolveFunctionToken(aToken, RepeatOnLabelsToken, out subToken, out columnsLinesSeparator))
				{
					if (subToken == null || subToken.Length == 0)
						return String.Empty;

					foreach (LabelInfo labelInfo in tabbedPaneInfo.LabelInfoCollection)
					{
						if (labelInfo == null)
							continue;

						string columnParsedText = SubstituteLabelsTokens(subToken, labelInfo);
						columnsLines += columnParsedText;
					}
				}
				return columnsLines;
			}

            if (aToken.StartsWith(RepeatOnVisibleColumnsToken))
            {
                if (tabbedPaneInfo == null || tabbedPaneInfo.DBTInfo == null || tabbedPaneInfo.ColumnsCount == 0)
                    return String.Empty;

                WizardTableInfo tableInfo = tabbedPaneInfo.GetTableInfo();
                if (tableInfo == null)
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnVisibleColumnsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    ushort columnControlId = Generics.FirstValidControlId;
                    if (this.DocumentInfo != null)
                        columnControlId = (ushort)this.DocumentInfo.GetFirstTabbedPaneControlId(tabbedPaneInfo);
                    else if (this.ClientDocumentInfo != null)
                        columnControlId = (ushort)this.ClientDocumentInfo.GetFirstTabbedPaneControlId(tabbedPaneInfo);

                    string columnsLines = String.Empty;
                    int columnControlsTopCoordinate = FirstColumnControlTopCoordinate;
                    System.Drawing.Font fontToUse = this.FontToUse;

                    foreach (WizardDBTColumnInfo aColumnInfo in tabbedPaneInfo.ColumnsInfo)
                    {
                        if (!aColumnInfo.Visible)
                            continue;

                        string columnParsedText = SubstituteTabbedPaneColumnTokens(subToken, aColumnInfo, columnControlId, columnControlsTopCoordinate);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }

                        columnControlId++;
                        if (aColumnInfo.ShowHotKeyLinkDescription)
                            columnControlId++;

                        columnControlsTopCoordinate += GetColumnDialogLineHeight(aColumnInfo, fontToUse) + ColumnControlVerticalSpacing;
                    }
                    return columnsLines;
                }
            }

            if (aToken.StartsWith(RepeatOnRowFormColumnsToken))
            {
                if (tabbedPaneInfo == null || tabbedPaneInfo.DBTInfo == null || !tabbedPaneInfo.CreateRowForm)
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnRowFormColumnsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    ushort columnControlId = Generics.FirstValidControlId;
                    if (this.DocumentInfo != null)
                        columnControlId = (ushort)this.DocumentInfo.GetFirstBodyEditRowFormControlId(tabbedPaneInfo);
                    else if (this.ClientDocumentInfo != null)
                        columnControlId = (ushort)this.ClientDocumentInfo.GetFirstBodyEditRowFormControlId(tabbedPaneInfo);

                    string columnsLines = String.Empty;
                    int columnControlsTopCoordinate = FirstColumnControlTopCoordinate;
                    System.Drawing.Font fontToUse = this.FontToUse;

                    foreach (WizardDBTColumnInfo aColumnInfo in tabbedPaneInfo.ColumnsInfo)
                    {
                        // Nelle RowFormView dei BodyEdit vengono gestiti tutti i campi 
                        // della tabella alla quale è riferito il DBT (compresi quelli 
                        // che non sono visibili, cioè per i quali non è stata inserita
                        // alcuna colonna corrispondente nel BodyEdit), ma vengono comunque
                        // scartati i segmenti di chiave esterna utilizzati da questo DBT
                        // SlaveBuffered per "agganciarsi" alla tabella master
                        if (aColumnInfo.ForeignKeySegment)
                            continue;

                        WizardDBTColumnInfo aTmpDBTColumnInfo = new WizardDBTColumnInfo(aColumnInfo);
                        aTmpDBTColumnInfo.Visible = true;
                        if (aTmpDBTColumnInfo.Title == null || aTmpDBTColumnInfo.Title.Length == 0)
                            aTmpDBTColumnInfo.Title = aColumnInfo.ColumnName;
                        aTmpDBTColumnInfo.HotKeyLink = null;

                        string columnParsedText = SubstituteTabbedPaneColumnTokens(subToken, aTmpDBTColumnInfo, columnControlId, columnControlsTopCoordinate);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }

                        columnControlId++;

                        columnControlsTopCoordinate += GetColumnDialogLineHeight(aTmpDBTColumnInfo, fontToUse) + ColumnControlVerticalSpacing;
                    }
                    return columnsLines;
                }
            }

            if (String.Compare(aToken, TabbedPaneWidthToken) == 0)
            {
                if (tabbedPaneInfo != null && tabbedPaneInfo.DBTInfo != null && tabbedPaneInfo.DBTInfo.IsSlaveBuffered)
                {
                    if (this.DocumentInfo == null && this.ClientDocumentInfo == null)
                        return String.Empty;

                    Size documentTabberAreaSize = Size.Empty;

                    if (this.DocumentInfo != null)
                        documentTabberAreaSize = WizardDocumentCodeTemplateParser.GetTabberAreaSize(this.DocumentInfo, this.FontToUse);
                    else if (this.ClientDocumentInfo != null)
                        documentTabberAreaSize = WizardClientDocumentCodeTemplateParser.GetTabberAreaSize(this.ClientDocumentInfo, this.FontToUse);

					tabbedPaneInfo.Width = (documentTabberAreaSize != Size.Empty) ? documentTabberAreaSize.Width : 0;
                    return (documentTabberAreaSize != Size.Empty) ? documentTabberAreaSize.Width.ToString() : String.Empty;

                }

				return (tabbedPaneSize != Size.Empty) ? tabbedPaneSize.Width.ToString() : String.Empty;
            }

            if (String.Compare(aToken, TabbedPaneHeightToken) == 0)
            {
                if (tabbedPaneInfo != null && tabbedPaneInfo.DBTInfo != null && tabbedPaneInfo.DBTInfo.IsSlaveBuffered)
                {
                    if (this.DocumentInfo == null && this.ClientDocumentInfo == null)
                        return String.Empty;

                    Size documentTabberAreaSize = Size.Empty;

                    if (this.DocumentInfo != null)
                        documentTabberAreaSize = WizardDocumentCodeTemplateParser.GetTabberAreaSize(this.DocumentInfo, this.FontToUse);
                    else if (this.ClientDocumentInfo != null)
                        documentTabberAreaSize = WizardClientDocumentCodeTemplateParser.GetTabberAreaSize(this.ClientDocumentInfo, this.FontToUse);

                    return (documentTabberAreaSize != Size.Empty) ? documentTabberAreaSize.Height.ToString() : String.Empty;
                }
				tabbedPaneInfo.Height = (tabbedPaneSize != Size.Empty) ? tabbedPaneSize.Height : 0;
                return (tabbedPaneSize != Size.Empty) ? tabbedPaneSize.Height.ToString() : String.Empty;
            }

            if (String.Compare(aToken, BodyEditControlIdToken) == 0)
            {
                if (tabbedPaneInfo == null || tabbedPaneInfo.DBTInfo == null || !tabbedPaneInfo.DBTInfo.IsSlaveBuffered)
                    return String.Empty;

                if (this.DocumentInfo != null)
                    return this.DocumentInfo.GetBodyEditControlId(tabbedPaneInfo).ToString();
                if (this.ClientDocumentInfo != null)
                    return this.ClientDocumentInfo.GetBodyEditControlId(tabbedPaneInfo).ToString();

                return String.Empty;
            }

            if (String.Compare(aToken, BodyEditLeftToken) == 0)
            {
                if (
                    tabbedPaneInfo == null ||
                    tabbedPaneInfo.DBTInfo == null ||
                    !tabbedPaneInfo.DBTInfo.IsSlaveBuffered ||
                    (this.DocumentInfo == null && this.ClientDocumentInfo == null)
                    )
                    return String.Empty;
				if (tabbedPaneInfo.DBTInfo.BodyEditPosition.isSet)
					return tabbedPaneInfo.DBTInfo.BodyEditPosition.Left.ToString();

				tabbedPaneInfo.DBTInfo.BodyEditPosition.Left = FirstColumnControlLeftCoordinate;
                return FirstColumnControlLeftCoordinate.ToString();
            }

            if (String.Compare(aToken, BodyEditTopToken) == 0)
            {
                if (
                    tabbedPaneInfo == null ||
                    tabbedPaneInfo.DBTInfo == null ||
                    !tabbedPaneInfo.DBTInfo.IsSlaveBuffered ||
                    (this.DocumentInfo == null && this.ClientDocumentInfo == null)
                    )
                    return String.Empty;
				
				if (tabbedPaneInfo.DBTInfo.BodyEditPosition.isSet)
					return tabbedPaneInfo.DBTInfo.BodyEditPosition.Top.ToString();

				tabbedPaneInfo.DBTInfo.BodyEditPosition.Top = FirstColumnControlTopCoordinate;
                return FirstColumnControlTopCoordinate.ToString();
            }

            if (String.Compare(aToken, BodyEditWidthToken) == 0)
            {
                if (
                    tabbedPaneInfo == null ||
                    tabbedPaneInfo.DBTInfo == null ||
                    !tabbedPaneInfo.DBTInfo.IsSlaveBuffered ||
                    (this.DocumentInfo == null && this.ClientDocumentInfo == null)
                    )
                    return String.Empty;

                Size documentTabberAreaSize = Size.Empty;

                if (this.DocumentInfo != null)
                    documentTabberAreaSize = WizardDocumentCodeTemplateParser.GetTabberAreaSize(this.DocumentInfo, this.FontToUse);
                else if (this.ClientDocumentInfo != null)
                    documentTabberAreaSize = WizardClientDocumentCodeTemplateParser.GetTabberAreaSize(this.ClientDocumentInfo, this.FontToUse);

				if (tabbedPaneInfo.DBTInfo.BodyEditPosition.isSet)
					return tabbedPaneInfo.DBTInfo.BodyEditPosition.Width.ToString();


				tabbedPaneInfo.DBTInfo.BodyEditPosition.Width = (documentTabberAreaSize != Size.Empty) ? 
					(documentTabberAreaSize.Width - (2 * FirstColumnControlLeftCoordinate)) : 0;
                return (documentTabberAreaSize != Size.Empty) ? (documentTabberAreaSize.Width - (2 * FirstColumnControlLeftCoordinate)).ToString() : String.Empty;
            }

            if (String.Compare(aToken, BodyEditHeightToken) == 0)
            {
                if (
                    tabbedPaneInfo == null ||
                    tabbedPaneInfo.DBTInfo == null ||
                    !tabbedPaneInfo.DBTInfo.IsSlaveBuffered ||
                    (this.DocumentInfo == null && this.ClientDocumentInfo == null)
                    )
                    return String.Empty;

                Size documentTabberAreaSize = Size.Empty;

                if (this.DocumentInfo != null)
                    documentTabberAreaSize = WizardDocumentCodeTemplateParser.GetTabberAreaSize(this.DocumentInfo, this.FontToUse);
                else if (this.ClientDocumentInfo != null)
                    documentTabberAreaSize = WizardClientDocumentCodeTemplateParser.GetTabberAreaSize(this.ClientDocumentInfo, this.FontToUse);
				if (tabbedPaneInfo.DBTInfo.BodyEditPosition.isSet)
					return tabbedPaneInfo.DBTInfo.BodyEditPosition.Height.ToString();
				tabbedPaneInfo.DBTInfo.BodyEditPosition.Height = (documentTabberAreaSize != Size.Empty) ? 
					(documentTabberAreaSize.Height - (2 * FirstColumnControlTopCoordinate)) : 0;
                return (documentTabberAreaSize != Size.Empty) ? (documentTabberAreaSize.Height - (2 * FirstColumnControlTopCoordinate)).ToString() : String.Empty;
            }


            if (aToken.StartsWith(RepeatOnHotLinkColumnsToken))
            {
                if (tabbedPaneInfo == null || tabbedPaneInfo.ColumnsCount == 0 || !tabbedPaneInfo.HasHKLDefinedColumns())
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnHotLinkColumnsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string columnsLines = String.Empty;
                    foreach (WizardDBTColumnInfo aColumnInfo in tabbedPaneInfo.ColumnsInfo)
                    {
                        if (!aColumnInfo.IsHKLDefined)
                            continue;

                        string columnParsedText = SubstituteTabbedPaneColumnTokens(subToken, aColumnInfo);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }
                    }
                    return columnsLines;
                }
            }

            if (aToken.StartsWith(RepeatOnApplicationHotLinkColumnsToken))
            {
                if (tabbedPaneInfo == null || tabbedPaneInfo.ColumnsCount == 0 || !tabbedPaneInfo.HasHKLDefinedColumns())
                    return String.Empty;

                string subToken;
                string columnsLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnApplicationHotLinkColumnsToken, out subToken, out columnsLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string columnsLines = String.Empty;
                    foreach (WizardDBTColumnInfo aColumnInfo in tabbedPaneInfo.ColumnsInfo)
                    {
                        // Se l'Hotlink non fa parte delle definizioni di progetto (cioè
                        // se appartiene ad una libreria di un'applicazione referenziata
                        // anzichè all'applicazione del progetto, lo salto
                        if (!aColumnInfo.IsHKLDefined || aColumnInfo.HotKeyLink.IsReferenced)
                            continue;

                        string columnParsedText = SubstituteTabbedPaneColumnTokens(subToken, aColumnInfo);

                        if (columnParsedText != null && columnParsedText.Length > 0)
                        {
                            if (columnsLines.Length > 0)
                                columnsLines += columnsLinesSeparator;
                            columnsLines += columnParsedText;
                        }
                    }
                    return columnsLines;
                }
            }

            if (aToken.StartsWith(RepeatOnReferencedHotLinkIncludeFilesToken))
            {
                if (tabbedPaneInfo == null || tabbedPaneInfo.ColumnsCount == 0 || !tabbedPaneInfo.HasHKLDefinedColumns() || (this.DocumentInfo == null && this.ClientDocumentInfo == null))
                    return String.Empty;

                string subToken;
                string includeFilesLinesSeparator;

                WizardLibraryInfo currentLibrary = null;
                if (this.DocumentInfo != null)
                    currentLibrary = this.DocumentInfo.Library;
                else if (this.ClientDocumentInfo != null)
                    currentLibrary = this.ClientDocumentInfo.Library;

                if (ResolveFunctionToken(aToken, RepeatOnReferencedHotLinkIncludeFilesToken, out subToken, out includeFilesLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string includeFilesLines = String.Empty;
                    foreach (WizardDBTColumnInfo aColumnInfo in tabbedPaneInfo.ColumnsInfo)
                    {
                        if (!aColumnInfo.IsHKLDefined || !aColumnInfo.HotKeyLink.IsReferenced)
                            continue;

                        string includeFileName = aColumnInfo.HotKeyLink.ExternalIncludeFile;
                        if (includeFileName == null || includeFileName.Length == 0)
                            continue;

                        WizardCodeFileTemplateParser aCodeFileParser = new WizardCodeFileTemplateParser(currentLibrary, includeFileName);
                        string includeFilesParsedText = aCodeFileParser.SubstituteTokens(subToken);

                        if (includeFilesParsedText != null && includeFilesParsedText.Length > 0)
                        {
                            if (includeFilesLines.Length > 0)
                                includeFilesLines += includeFilesLinesSeparator;
                            includeFilesLines += includeFilesParsedText;
                        }
                    }
                    return includeFilesLines;
                }
            }

            if (String.Compare(aToken, DBTRowFormWidthToken) == 0)
            {
                if (tabbedPaneInfo == null || tabbedPaneInfo.ColumnsCount == 0 || !tabbedPaneInfo.CreateRowForm)
                    return String.Empty;

                System.Drawing.Size rowFormDialogSize = WizardTabbedPaneCodeTemplateParser.GetDBTSlaveBufferedRowFormSize(tabbedPaneInfo.DBTInfo, this.FontToUse);
                if (rowFormDialogSize == Size.Empty)
                    return String.Empty;

                return rowFormDialogSize.Width.ToString();
            }

            if (String.Compare(aToken, DBTRowFormHeightToken) == 0)
            {
                if (tabbedPaneInfo == null || tabbedPaneInfo.ColumnsCount == 0 || !tabbedPaneInfo.CreateRowForm)
                    return String.Empty;

                System.Drawing.Size rowFormDialogSize = WizardTabbedPaneCodeTemplateParser.GetDBTSlaveBufferedRowFormSize(tabbedPaneInfo.DBTInfo, this.FontToUse);
                if (rowFormDialogSize == Size.Empty)
                    return String.Empty;

                return rowFormDialogSize.Height.ToString();
            }

            if (aToken.StartsWith(IfShowsAdditionalColumnsToken))
            {
                if (tabbedPaneInfo == null || !tabbedPaneInfo.ShowsAdditionalColumns)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfShowsAdditionalColumnsToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(RepeatOnVisibleAdditionalColumnsInfoToken))
            {
                if (
                    tabbedPaneInfo == null ||
                    tabbedPaneInfo.DBTInfo == null ||
                    tabbedPaneInfo.DBTInfo.Library == null ||
                    tabbedPaneInfo.DBTInfo.Library.Application == null
                    )
                    return String.Empty;

                WizardTableInfo tableInfo = tabbedPaneInfo.GetTableInfo();
                if (tableInfo == null)
                    return String.Empty;

                WizardExtraAddedColumnsInfoCollection additionalColumnsInfo = tabbedPaneInfo.DBTInfo.Library.GetAllAvailableTableAdditionalColumnsInfo(tableInfo.GetNameSpace());
                if (additionalColumnsInfo == null || additionalColumnsInfo.Count == 0)
                    return String.Empty;

                string subToken;
                string additionalColumnsInfoLinesSeparator;

                if (ResolveFunctionToken(aToken, RepeatOnVisibleAdditionalColumnsInfoToken, out subToken, out additionalColumnsInfoLinesSeparator))
                {
                    if (subToken == null || subToken.Length == 0)
                        return String.Empty;

                    string additionalColumnsInfoLines = String.Empty;
                    foreach (WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo in additionalColumnsInfo)
                    {
                        if (!tabbedPaneInfo.HasVisibleAdditionalColumns(aExtraAddedColumnsInfo))
                            continue;

                        string additionalColumnsInfoParsedText = String.Empty;

                        WizardAdditionalColumnsCodeTemplateParser additionalColumnsParser = null;
                        if (this.ClientDocumentInfo != null)
                            additionalColumnsParser = new WizardAdditionalColumnsCodeTemplateParser(aExtraAddedColumnsInfo, this.ClientDocumentInfo);
                        else
                            additionalColumnsParser = new WizardAdditionalColumnsCodeTemplateParser(aExtraAddedColumnsInfo, this.DocumentInfo);

                        additionalColumnsInfoParsedText = additionalColumnsParser.SubstituteTokens(subToken, true);

                        if (additionalColumnsInfoParsedText != null && additionalColumnsInfoParsedText.Length > 0)
                        {
                            if (additionalColumnsInfoLines.Length > 0)
                                additionalColumnsInfoLines += additionalColumnsInfoLinesSeparator;
                            additionalColumnsInfoLines += additionalColumnsInfoParsedText;
                        }
                    }
                    return additionalColumnsInfoLines;
                }
            }

            if (String.Compare(aToken, ReferencedTableHeaderRelativePathToken) == 0)
            {
                if (tabbedPaneInfo == null || tabbedPaneInfo.DBTInfo == null)
                    return String.Empty;

                WizardTableInfo tableInfo = tabbedPaneInfo.GetTableInfo();

                if (tableInfo == null || !tableInfo.IsReferenced)
                    return String.Empty;

                string referencedTableIncludeFile = tabbedPaneInfo.DBTInfo.ReferencedTableIncludeFile;
                if (referencedTableIncludeFile == null || referencedTableIncludeFile.Trim().Length == 0)
                    return String.Empty;

                WizardLibraryInfo currentLibrary = null;
                if (this.DocumentInfo != null)
                    currentLibrary = this.DocumentInfo.Library;
                else if (this.ClientDocumentInfo != null)
                    currentLibrary = this.ClientDocumentInfo.Library;

                string relativePath = String.Empty;
                if (currentLibrary != null)
                    relativePath = Generics.MakeRelativeTo(WizardCodeGenerator.GetStandardLibraryPath(currentLibrary), referencedTableIncludeFile);

                return relativePath;
            }

            if (aToken.StartsWith(IfDBTMasterToken))
            {
                if (tabbedPaneInfo == null || tabbedPaneInfo.DBTInfo == null || !tabbedPaneInfo.DBTInfo.IsMaster)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfDBTMasterToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfNotDBTMasterToken))
            {
                if (tabbedPaneInfo == null || tabbedPaneInfo.DBTInfo == null || tabbedPaneInfo.DBTInfo.IsMaster)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfNotDBTMasterToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfDBTSlaveBufferedToken))
            {
                if (tabbedPaneInfo == null || tabbedPaneInfo.DBTInfo == null || !tabbedPaneInfo.DBTInfo.IsSlaveBuffered)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfDBTSlaveBufferedToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfNotDBTSlaveBufferedToken))
            {
                if (tabbedPaneInfo == null || tabbedPaneInfo.DBTInfo == null || tabbedPaneInfo.DBTInfo.IsSlaveBuffered)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfNotDBTSlaveBufferedToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfCreateRowFormToken))
            {
                if (tabbedPaneInfo == null || tabbedPaneInfo.DBTInfo == null || !tabbedPaneInfo.CreateRowForm)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfCreateRowFormToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (aToken.StartsWith(IfNotCreateRowFormToken))
            {
                if (tabbedPaneInfo == null || tabbedPaneInfo.DBTInfo == null || tabbedPaneInfo.CreateRowForm)
                    return String.Empty;

                string subToken;
                if (ResolveFunctionToken(aToken, IfNotCreateRowFormToken, out subToken))
                {
                    if (subToken == null || subToken.Length == 0)
                        return "\r\n";

                    return this.SubstituteTokens(subToken);
                }
            }

            if (String.Compare(aToken, BodyEditRowFormDialogIdToken) == 0)
            {
                int formId = GetTabbedPaneRowFormId();
                return (formId != -1) ? formId.ToString() : String.Empty;
            }

            return base.GetTokenValue(aToken);
        }

		//----------------------------------------------------------------------------
		private string SubstituteLabelsTokens(string source, LabelInfo labelInfo)
		{
			string labelParsedText = this.SubstituteTokens(source, true);
			if (AreTokensToSubstitute(labelParsedText))
			{
				WizardLabelTemplateParser labelParser = null;
				labelParser = new WizardLabelTemplateParser(labelInfo);
				labelParsedText = labelParser.SubstituteTokens(labelParsedText, true);
			}
			return labelParsedText;
		}

        //----------------------------------------------------------------------------
        private string SubstituteTabbedPaneColumnTokens
            (
            string source,
            WizardDBTColumnInfo aColumnInfo,
            ushort aControlId,
            int aYCoordinate,
            bool preserveUnresolvedTokens
            )
        {
            if (source == null || aColumnInfo == null)
                return null;

            source = source.Trim();
            if (source.Length == 0)
                return String.Empty;

            string columnParsedText = this.SubstituteTokens(source, true);

            if (AreTokensToSubstitute(columnParsedText))
            {
                WizardDBTColumnCodeTemplateParser columnParser = null;

                if (this.DocumentInfo != null)
                    columnParser = new WizardDBTColumnCodeTemplateParser(aColumnInfo, tabbedPaneInfo, this.DocumentInfo, aControlId, aYCoordinate);
                else if (this.ClientDocumentInfo != null)
                    columnParser = new WizardDBTColumnCodeTemplateParser(aColumnInfo, tabbedPaneInfo, this.ClientDocumentInfo, aControlId, aYCoordinate);
                else
                    columnParser = new WizardDBTColumnCodeTemplateParser(aColumnInfo, tabbedPaneInfo, aControlId, aYCoordinate);

                columnParsedText = columnParser.SubstituteTokens(columnParsedText, true);

                if (AreTokensToSubstitute(columnParsedText))
                {
                    int columnIndex = -1;
                    WizardTableColumnInfo tableColumnInfo = tabbedPaneInfo.GetTableColumnInfoByName(aColumnInfo.ColumnName, ref columnIndex);
                    if (tableColumnInfo != null)
                    {
                        WizardTableColumnCodeTemplateParser tableColumnParser = new WizardTableColumnCodeTemplateParser(tableColumnInfo, columnIndex, tabbedPaneInfo.GetTableInfo());

                        columnParsedText = tableColumnParser.SubstituteTokens(columnParsedText, preserveUnresolvedTokens);
                    }
                }
            }
            return columnParsedText;
        }

        //----------------------------------------------------------------------------
        private string SubstituteTabbedPaneColumnTokens(string source, WizardDBTColumnInfo aColumnInfo, ushort aControlId, int aYCoordinate)
        {
            return SubstituteTabbedPaneColumnTokens(source, aColumnInfo, aControlId, aYCoordinate, false);
        }

        //----------------------------------------------------------------------------
        private string SubstituteTabbedPaneColumnTokens(string source, WizardDBTColumnInfo aColumnInfo)
        {
            return SubstituteTabbedPaneColumnTokens(source, aColumnInfo, 0, 0, false);
        }

        //----------------------------------------------------------------------------
        internal static System.Drawing.Size GetTabbedPaneSize(WizardDocumentTabbedPaneInfo aTabbedPaneInfo, System.Drawing.Font aFont)
        {
            if (aTabbedPaneInfo == null || aTabbedPaneInfo.DBTInfo == null)
                return Size.Empty;

            if (aTabbedPaneInfo.DBTInfo.IsSlaveBuffered)
                return GetSlaveBufferedDialogMinimumSize();

            if (aFont == null || aFont.FontFamily == null)
                aFont = WizardApplicationInfo.DefaultFont;

            int maxColumnLineWidth = 0;
            int tabbedPaneHeight = FirstColumnControlTopCoordinate;

            foreach (WizardDBTColumnInfo aColumnInfo in aTabbedPaneInfo.ColumnsInfo)
            {
                if (!aColumnInfo.Visible)
                    continue;

                Size columnLineSize = GetColumnDialogLineSize(aColumnInfo, aFont);
                if (columnLineSize == Size.Empty)
                    continue;

                int columnLineWidth = columnLineSize.Width + (2 * FirstColumnControlLeftCoordinate);

                if (maxColumnLineWidth < columnLineWidth)
                    maxColumnLineWidth = columnLineWidth;

                tabbedPaneHeight += columnLineSize.Height + ColumnControlVerticalSpacing;
            }
            return (maxColumnLineWidth > 0) ? new Size(maxColumnLineWidth, tabbedPaneHeight) : Size.Empty;
        }

        //----------------------------------------------------------------------------
        private int GetTabbedPaneFormId()
        {
            if (tabbedPaneInfo == null)
                return -1;

            if (this.DocumentInfo != null)
                return this.DocumentInfo.GetTabbedPaneFormId(tabbedPaneInfo);

            if (this.ClientDocumentInfo != null)
                return this.ClientDocumentInfo.GetTabbedPaneFormId(tabbedPaneInfo);

            return -1;
        }

        //----------------------------------------------------------------------------
        private int GetTabbedPaneRowFormId()
        {
            if (tabbedPaneInfo == null)
                return -1;

            if (this.DocumentInfo != null)
                return this.DocumentInfo.GetTabbedPaneRowFormId(tabbedPaneInfo);

            if (this.ClientDocumentInfo != null)
                return this.ClientDocumentInfo.GetTabbedPaneRowFormId(tabbedPaneInfo);

            return -1;
        }

        //----------------------------------------------------------------------------
        private void InitTabbedPaneSize()
        {
            tabbedPaneSize = GetTabbedPaneSize(tabbedPaneInfo, this.FontToUse);
        }

    }

    #endregion

	# region WizardIndexCodeTemplateParser class
	//============================================================================
	internal class WizardIndexCodeTemplateParser : CodeTemplateParser
	{
		private const string IndexNameToken = "IndexName";
		private const string TableNameToken = "TableName";

		private const string RepeatOnIndexSegmentsToken = "RepeatOnIndexSegments";
		private const string IfIndexClusteredToken = "IfIndexClustered";
		private const string IfIndexUniqueToken = "IfIndexUnique";

		private WizardTableIndexInfo indexInfo = null;

		//----------------------------------------------------------------------------
		internal WizardIndexCodeTemplateParser(WizardTableIndexInfo aIndexInfo)
		{
			indexInfo = aIndexInfo;
		}

		//----------------------------------------------------------------------------
		internal override string GetTokenValue(string aToken)
		{
			if (aToken == null)
				return null;

			aToken = aToken.Trim();
			if (aToken.Length == 0)
				return String.Empty;

			if (String.Compare(aToken, IndexNameToken) == 0)
				return (indexInfo != null) ? indexInfo.Name : String.Empty;

			if (String.Compare(aToken, TableNameToken) == 0)
				return (indexInfo != null) ? indexInfo.TableName : String.Empty;
			
			if (aToken.StartsWith(IfIndexUniqueToken))
			{
				if (indexInfo.Unique)
				{
					string subToken;
					if (ResolveFunctionToken(aToken, IfIndexUniqueToken, out subToken))
					{
						if (subToken == null || subToken.Length == 0)
							return String.Empty;

						return this.SubstituteTokens(subToken, true);
					}
				}
				return string.Empty;

			}

			if (aToken.StartsWith(IfIndexClusteredToken))
			{
				if (!indexInfo.NonClustered)
				{
					string subToken;
					if (ResolveFunctionToken(aToken, IfIndexClusteredToken, out subToken))
					{
						if (subToken == null || subToken.Length == 0)
							return String.Empty;

						return this.SubstituteTokens(subToken, true);
					}
				}
				return string.Empty;
			}

			if (aToken.StartsWith(RepeatOnIndexSegmentsToken))
			{
				if (indexInfo == null || indexInfo.SegmentsCount == 0)
					return String.Empty;

				string subToken;
				string segmentsLinesSeparator;
				if (ResolveFunctionToken(aToken, RepeatOnIndexSegmentsToken, out subToken, out segmentsLinesSeparator))
				{
					if (subToken == null || subToken.Length == 0)
						return String.Empty;

					string segmentsLines = String.Empty;
					foreach (WizardTableColumnInfo aSegment in indexInfo.Segments)
					{
						WizardIndexSegmentCodeTemplateParser segmentParser = new WizardIndexSegmentCodeTemplateParser(aSegment);

						string segmentParsedText = segmentParser.SubstituteTokens(subToken, true);

						if (AreTokensToSubstitute(segmentParsedText))
							segmentParsedText = this.SubstituteTokens(segmentParsedText);

						if (segmentParsedText != null && segmentParsedText.Length > 0)
						{
							if (segmentsLines.Length > 0)
								segmentsLines += segmentsLinesSeparator;
							segmentsLines += segmentParsedText;
						}
					}
					return segmentsLines;
				}
			}

			return null;
		}
	}
	# endregion

	# region WizardIndexSegmentCodeTemplateParser class
	//============================================================================
	internal class WizardIndexSegmentCodeTemplateParser : CodeTemplateParser
	{
		private const string ColumnNameToken = "ColumnName";

		private WizardTableColumnInfo keySegment = null;

		//----------------------------------------------------------------------------
		internal WizardIndexSegmentCodeTemplateParser(WizardTableColumnInfo aKeySegment)
		{
			keySegment = aKeySegment;
		}

		//----------------------------------------------------------------------------
		internal override string GetTokenValue(string aToken)
		{
			if (aToken == null)
				return null;

			aToken = aToken.Trim();
			if (aToken.Length == 0)
				return String.Empty;

			if (String.Compare(aToken, ColumnNameToken) == 0)
				return (keySegment != null) ? keySegment.Name : String.Empty;

			return null;
		}
	}
	# endregion

	# region WizardViewCodeTemplateParser class
	///<summary>
	/// Template per la scrittura dello script di una view
	///</summary>
	//============================================================================
	internal class WizardViewCodeTemplateParser : CodeTemplateParser
	{
		private const string ViewNameToken = "ViewName";
		private const string SqlViewDefinitionToken = "SqlViewDefinition";
		private const string OracleViewDefinitionToken = "OracleViewDefinition";

		private SqlView view = null;
		private DBMSType dbType = DBMSType.UNKNOWN;

		//----------------------------------------------------------------------------
		internal WizardViewCodeTemplateParser(SqlView aView, DBMSType aDBType)
		{
			view = aView;
			dbType = aDBType;
		}

		//----------------------------------------------------------------------------
		internal override string GetTokenValue(string aToken)
		{
			if (aToken == null)
				return null;

			aToken = aToken.Trim();
			if (aToken.Length == 0)
				return String.Empty;

			if (String.Compare(aToken, ViewNameToken) == 0)
				return view.Name;

			if (String.Compare(aToken, SqlViewDefinitionToken) == 0)
			{ 
				if (dbType == DBMSType.SQLSERVER)
					return view.SqlDefinition;
				return string.Empty;
			}

			if (String.Compare(aToken, OracleViewDefinitionToken) == 0)
			{ 
				if (dbType == DBMSType.ORACLE)
					return view.OracleDefinition;
				return string.Empty;
			}

			return null;
		}
	}
	# endregion

	# region WizardProcedureCodeTemplateParser class
	///<summary>
	/// Template per la scrittura dello script di una procedure
	///</summary>
	//============================================================================
	internal class WizardProcedureCodeTemplateParser : CodeTemplateParser
	{
		private const string ProcedureNameToken = "ProcedureName";
		private const string SqlProcedureDefinitionToken = "SqlProcedureDefinition";
		private const string OracleProcedureDefinitionToken = "OracleProcedureDefinition";

		private SqlProcedure procedure = null;
		private DBMSType dbType = DBMSType.UNKNOWN;

		//----------------------------------------------------------------------------
		internal WizardProcedureCodeTemplateParser(SqlProcedure aProcedure, DBMSType aDBType)
		{
			procedure = aProcedure;
			dbType = aDBType;
		}

		//----------------------------------------------------------------------------
		internal override string GetTokenValue(string aToken)
		{
			if (aToken == null)
				return null;

			aToken = aToken.Trim();
			if (aToken.Length == 0)
				return String.Empty;

			if (String.Compare(aToken, ProcedureNameToken) == 0)
				return procedure.Name;

			if (String.Compare(aToken, SqlProcedureDefinitionToken) == 0)
			{
				if (dbType == DBMSType.SQLSERVER)
					return procedure.SqlDefinition;
				return string.Empty;
			}

			if (String.Compare(aToken, OracleProcedureDefinitionToken) == 0)
			{
				if (dbType == DBMSType.ORACLE)
					return procedure.OracleDefinition;
				return string.Empty;
			}

			return null;
		}
	}
	# endregion

}

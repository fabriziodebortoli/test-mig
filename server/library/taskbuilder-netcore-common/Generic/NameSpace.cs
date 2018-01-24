using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.Common.Generic
{
    /// <summary>
    /// Associazione tra tipo namespace e numero di token
    /// </summary>
    //=========================================================================
    [Serializable]
    public class NameSpaceType : INameSpaceType
    {
        private NameSpaceObjectType type;
        private int tokenNumber;
        private string publicName;

        /// <summary>
        /// tipo del namespace
        /// </summary>
        public NameSpaceObjectType Type { get { return type; } }

        /// <summary>
        /// Numero di token
        /// </summary>
        public int TokenNumber { get { return tokenNumber; } }

        /// <summary>
        /// nome completo
        /// </summary>
        public string PublicName { get { return publicName; } }

        /// <summary>
        /// not argoment costructor
        /// </summary>
        public NameSpaceType()
        {
            // DP20120627
        }

        /// <summary>
        /// Costruttore
        /// </summary>
        /// <param name="aNameSpaceType">tipo</param>
        /// <param name="aTokenNumber">numero token</param>
        /// <param name="aPublicName">nome</param>
        //---------------------------------------------------------------------
        public NameSpaceType(NameSpaceObjectType aNameSpaceType, int aTokenNumber, string aPublicName)
        {
            type = aNameSpaceType;
            tokenNumber = aTokenNumber;
            publicName = aPublicName;
        }
    }

    /// <summary>
    /// mappa statica che contiene per ogni tipo di namespace il numero di tag che deve avere
    /// </summary>
    //=========================================================================
    internal sealed class NameSpaceTable
    {
        private static readonly IDictionary<string, INameSpaceType> nameNameSpaceTypeTable =
            new Dictionary<string, INameSpaceType>(StringComparer.OrdinalIgnoreCase);

        private static readonly IDictionary<NameSpaceObjectType, INameSpaceType> nameSpaceObjectTypeNameSpaceTypeTable =
            new Dictionary<NameSpaceObjectType, INameSpaceType>(EqualityComparer<NameSpaceObjectType>.Default);

        //---------------------------------------------------------------------
        private NameSpaceTable()
        { }

        //---------------------------------------------------------------------
        static NameSpaceTable()
        {
            NameSpaceType aNameSpaceType = new NameSpaceType(NameSpaceObjectType.Application, 1, NameSpaceSegment.Application);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.Module, 2, NameSpaceSegment.Module);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.Report, 3, NameSpaceSegment.Report);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.NotValid, 3, NameSpaceSegment.NotValid);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.Library, 3, NameSpaceSegment.Library);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.ExcelDocument, 3, NameSpaceSegment.ExcelDocument);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.ExcelTemplate, 3, NameSpaceSegment.ExcelTemplate);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.WordDocument, 3, NameSpaceSegment.WordDocument);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.WordTemplate, 3, NameSpaceSegment.WordTemplate);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.ExcelDocument2007, 3, NameSpaceSegment.ExcelDocument2007);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.ExcelTemplate2007, 3, NameSpaceSegment.ExcelTemplate2007);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.WordDocument2007, 3, NameSpaceSegment.WordDocument2007);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.WordTemplate2007, 3, NameSpaceSegment.WordTemplate2007);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.ReportSchema, 3, NameSpaceSegment.ReportSchema);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.Image, 4, NameSpaceSegment.Image);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.Text, 4, NameSpaceSegment.Text);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.File, 4, NameSpaceSegment.File);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.DataFile, 3, NameSpaceSegment.DataFile);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.View, 4, NameSpaceSegment.View);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.Document, 4, NameSpaceSegment.Document);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.Hotlink, 4, NameSpaceSegment.Hotlink);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.HotKeyLink, 4, NameSpaceSegment.HotKeyLink);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.Table, 4, NameSpaceSegment.Table);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.Procedure, 4, NameSpaceSegment.Procedure);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.Function, 4, NameSpaceSegment.Function);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.Dbt, 5, NameSpaceSegment.Dbt);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.DocumentSchema, 4, NameSpaceSegment.DocumentSchema);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.Setting, 3, NameSpaceSegment.Setting);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.Control, 6, NameSpaceSegment.Control);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.Form, 5, NameSpaceSegment.Form);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.Customization, 5, NameSpaceSegment.Customization);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.Component, 4, NameSpaceSegment.Component);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.Toolbar, 6, NameSpaceSegment.Toolbar);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);

            aNameSpaceType = new NameSpaceType(NameSpaceObjectType.ToolbarButton, 7, NameSpaceSegment.ToolbarButton);
            nameNameSpaceTypeTable.Add(aNameSpaceType.PublicName, aNameSpaceType);
            nameSpaceObjectTypeNameSpaceTypeTable.Add(aNameSpaceType.Type, aNameSpaceType);
        }

        /// <summary>
        /// Restituisce il NameSpaceType relativo al nome di token specificato
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //---------------------------------------------------------------------
        internal static INameSpaceType GetNameSpaceTypeByName(string name)
        {
            if (name == null || name.Trim().Length == 0)
                return null;

            INameSpaceType aNameSpaceType = null;

            return nameNameSpaceTypeTable.TryGetValue(name, out aNameSpaceType)
                ?
                aNameSpaceType
                :
                null;
        }

        /// <summary>
        /// Restituisce il NameSpaceType relativo al nome di token specificato
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //---------------------------------------------------------------------
        internal static INameSpaceType GetNameSpaceTypeByType(NameSpaceObjectType aNameSpaceObjectType)
        {
            INameSpaceType aNameSpaceType = null;

            return nameSpaceObjectTypeNameSpaceTypeTable.TryGetValue(aNameSpaceObjectType, out aNameSpaceType)
                ?
                aNameSpaceType
                :
                null;
        }

        //---------------------------------------------------------------------
        internal static int GetTokenNumberByType(NameSpaceObjectType aNameSpaceObjectType)
        {
            INameSpaceType aNameSpaceType = null;

            return nameSpaceObjectTypeNameSpaceTypeTable.TryGetValue(aNameSpaceObjectType, out aNameSpaceType)
                ?
                aNameSpaceType.TokenNumber
                :
                -1;
        }

        //---------------------------------------------------------------------
        internal static NameSpaceObjectType GetTokenTypeByName(string tokenName)
        {
            if (tokenName == null || tokenName.Trim().Length == 0)
                return NameSpaceObjectType.NotValid;

            INameSpaceType aNameSpaceType = null;

            return nameNameSpaceTypeTable.TryGetValue(tokenName, out aNameSpaceType)
                ?
                aNameSpaceType.Type
                :
                NameSpaceObjectType.NotValid;
        }
    }

    /// <summary>
    /// Risolutore dei NameSpace
    /// </summary>
    [DefaultPropertyAttribute("FullNameSpace")]
    [Serializable]
    //=========================================================================
    public class NameSpace : INameSpace
    {
        private INameSpaceType nameSpaceType;

        private StringCollection tokens = new StringCollection();
        private string fullNameSpace;

        public const char TokenSeparator = '.';

        //-----------------------------------------------------------------------------------------
        public INameSpaceType NameSpaceType { get { return nameSpaceType; } }

        public string FullNameSpace { get { return fullNameSpace; } }

        public string Application
        {
            get { return (tokens == null || tokens.Count == 0) ? string.Empty : tokens[1]; }
            set { SetTokenValue(NameSpaceObjectType.Application, value); }
        }
        public string Module
        {
            get { return GetTokenValue(NameSpaceObjectType.Module); }
            set { SetTokenValue(NameSpaceObjectType.Module, value); }
        }

        public string Document
        {
            get { return GetTokenValue(NameSpaceObjectType.Document); }
            set { SetTokenValue(NameSpaceObjectType.Document, value); }
        }
        public string Library
        {
            get { return GetTokenValue(NameSpaceObjectType.Library); }
            set { SetTokenValue(NameSpaceObjectType.Library, value); }
        }

        public string ObjectName
        {
            get { return GetTokenValue(nameSpaceType.Type); }
        }

        public string Report { get { return GetTokenValue(NameSpaceObjectType.Report); } }
        public string Dbt { get { return GetTokenValue(NameSpaceObjectType.Dbt); } }
        public string Hotlink { get { return GetTokenValue(NameSpaceObjectType.Hotlink); } }
        public string HotKeyLink { get { return GetTokenValue(NameSpaceObjectType.HotKeyLink); } }
        public string Table { get { return GetTokenValue(NameSpaceObjectType.Table); } }
        public string Function { get { return GetTokenValue(NameSpaceObjectType.Function); } }
        public string Image { get { return GetTokenValue(NameSpaceObjectType.Image); } }
        public string Text { get { return GetTokenValue(NameSpaceObjectType.Text); } }
        public string File { get { return GetTokenValue(NameSpaceObjectType.File); } }
        public string DataFile { get { return GetTokenValue(NameSpaceObjectType.DataFile); } }
        public string View { get { return GetTokenValue(NameSpaceObjectType.View); } }
        public string ExcelDocument { get { return GetTokenValue(NameSpaceObjectType.ExcelDocument); } }
        public string ExcelTemplate { get { return GetTokenValue(NameSpaceObjectType.ExcelTemplate); } }
        public string WordDocument { get { return GetTokenValue(NameSpaceObjectType.WordDocument); } }
        public string WordTemplate { get { return GetTokenValue(NameSpaceObjectType.WordTemplate); } }
        public string ReportSchema { get { return GetTokenValue(NameSpaceObjectType.ReportSchema); } }
        public string DocumentSchema { get { return GetTokenValue(NameSpaceObjectType.DocumentSchema); } }
        public string Setting { get { return GetTokenValue(NameSpaceObjectType.Setting); } }
        public string ExcelDocument2007 { get { return GetTokenValue(NameSpaceObjectType.ExcelDocument2007); } }
        public string ExcelTemplate2007 { get { return GetTokenValue(NameSpaceObjectType.ExcelTemplate2007); } }
        public string WordDocument2007 { get { return GetTokenValue(NameSpaceObjectType.WordDocument2007); } }
        public string WordTemplate2007 { get { return GetTokenValue(NameSpaceObjectType.WordTemplate2007); } }

        private static NameSpace empty = new NameSpace("");
        public static NameSpace Empty { get { return empty; } }

        /// <summary>
        /// Restituisce il comando di menu appropriato in base al tipo del namespace
        /// </summary>
        //---------------------------------------------------------------------
        public string Command
        {
            get
            {
                switch (nameSpaceType.Type)
                {
                    case NameSpaceObjectType.Document:
                        return Document;

                    case NameSpaceObjectType.Function:
                        return Function;

                    case NameSpaceObjectType.Report:
                        return Report;

                    case NameSpaceObjectType.Text:
                        return Text;

                    case NameSpaceObjectType.Image:
                        return Image;

                    case NameSpaceObjectType.ExcelDocument:
                        return ExcelDocument;

                    case NameSpaceObjectType.ExcelTemplate:
                        return ExcelTemplate;

                    case NameSpaceObjectType.WordDocument:
                        return WordDocument;

                    case NameSpaceObjectType.WordTemplate:
                        return WordTemplate;

                    case NameSpaceObjectType.ExcelDocument2007:
                        return ExcelDocument2007;

                    case NameSpaceObjectType.ExcelTemplate2007:
                        return ExcelTemplate2007;

                    case NameSpaceObjectType.WordDocument2007:
                        return WordDocument2007;

                    case NameSpaceObjectType.WordTemplate2007:
                        return WordTemplate2007;

                    case NameSpaceObjectType.ReportSchema:
                        return ReportSchema;

                    case NameSpaceObjectType.DocumentSchema:
                        return DocumentSchema;
                }

                return string.Empty;
            }
        }

        //---------------------------------------------------------------------
        public override string ToString()
        {
            return fullNameSpace;
        }

        //---------------------------------------------------------------------
        public override bool Equals(object obj)
        {
            return fullNameSpace.Equals(((NameSpace)obj).FullNameSpace);
        }

        //---------------------------------------------------------------------
        public override int GetHashCode()
        {
            return fullNameSpace.GetHashCode();
        }
        //---------------------------------------------------------------------
        public NameSpace() : this(NameSpace.Empty)
        {

        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Costruttore contenente un namespace senza il tipo come primo segmento ma viene passato
        /// </summary>
        /// <param name="aFullNameSpace">Il name space completo</param>
        //---------------------------------------------------------------------
        public NameSpace(string nameSpaceWithOutType, NameSpaceObjectType nameSpaceObjectType)
        {
            string typeNs = nameSpaceObjectType.ToString() + TokenSeparator;

            if (string.Compare(nameSpaceWithOutType, 0, typeNs, 0, typeNs.Length, StringComparison.OrdinalIgnoreCase) != 0)
                CreateNameSpace(typeNs + nameSpaceWithOutType);
            else //c'e' già il prefisso del tipo
                CreateNameSpace(nameSpaceWithOutType);
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Costruttore contenente un namespace completo del primo segmento contenente
        /// il tipo dell'oggetto definito dal namespace stesso (function, module etc...)
        /// </summary>
        /// <param name="aFullNameSpace">Il name space completo</param>
        //---------------------------------------------------------------------
        public NameSpace(string fullNameSpace)
        {
            CreateNameSpace(fullNameSpace);
        }

        //---------------------------------------------------------------------
        private string InsertSegment(string ns, string seg, int pos)
        {
            int idx = ns.IndexOfOccurrence(".", pos - 1, 0);
            if (idx > 0)
                ns = ns.InsertSub("." + seg, idx);
            return ns;
        }

        //---------------------------------------------------------------------
        private string ReplaceSegment(string ns, string seg, int pos)
        {
            int idx = ns.IndexOfOccurrence(".", pos - 1, 0);
            if (idx > 0)
            {
                int idx2 = ns.IndexOf(".", idx + 1);
                if (idx2 > 0)
                    ns = ns.Substring(0, idx + 1) + seg + ns.Substring(idx2);
            }
            return ns;
        }

        //---------------------------------------------------------------------
        private string RemoveSegment(string ns, int pos)
        {
            int idx = ns.IndexOfOccurrence(".", pos - 1, 0);
            if (idx > 0)
            {
                int idx2 = ns.IndexOf(".", idx + 1);
                if (idx2 > 0)
                    ns = ns.Substring(0, idx) + ns.Substring(idx2);
                else
                    ns = ns.Substring(0, idx);
            }
            return ns;
        }

        //---------------------------------------------------------------------
        public void CreateNameSpace(string fullNameSpace)
        {
            this.fullNameSpace = fullNameSpace;

            string[] tokensAr = fullNameSpace.Split(TokenSeparator);
            if (tokensAr != null && tokensAr.Length > 0)
            {
                nameSpaceType = NameSpaceTable.GetNameSpaceTypeByName(tokensAr[0]);
                if (nameSpaceType != null)
                {
                    if
                         (
                             nameSpaceType.Type == NameSpaceObjectType.Text && tokensAr.Length > 3 &&
                             !("texts".Equals(tokensAr[3], StringComparison.OrdinalIgnoreCase))
                         )
                    {
                        this.fullNameSpace = InsertSegment(this.fullNameSpace, "texts", 4);
                        tokensAr = this.fullNameSpace.Split(TokenSeparator);
                    }
                    if
                        (
                            nameSpaceType.Type == NameSpaceObjectType.Image && tokensAr.Length > 3 &&
                             !("images".Equals(tokensAr[3], StringComparison.OrdinalIgnoreCase))
                        )
                    {
                        this.fullNameSpace = InsertSegment(this.fullNameSpace, "images", 4);
                        tokensAr = this.fullNameSpace.Split(TokenSeparator);
                    }
                    if
                        (
                            nameSpaceType.Type == NameSpaceObjectType.File && tokensAr.Length > 4 &&
                            ("files".Equals(tokensAr[3], StringComparison.OrdinalIgnoreCase)) &&
                            ("others".Equals(tokensAr[4], StringComparison.OrdinalIgnoreCase))
                        )
                    {
                        this.fullNameSpace = RemoveSegment(this.fullNameSpace, 4);
                        tokensAr = this.fullNameSpace.Split(TokenSeparator);
                    }
                    if
                        (
                            nameSpaceType.Type == NameSpaceObjectType.File && tokensAr.Length > 3 &&
                             !("others".Equals(tokensAr[3], StringComparison.OrdinalIgnoreCase))
                        )
                    {
                        this.fullNameSpace = InsertSegment(this.fullNameSpace, "others", 4);
                        tokensAr = this.fullNameSpace.Split(TokenSeparator);
                    }
                    //se dei token del namespace contengono dei . in più rispetto al previsto
                    //ed il tipo del namespace è un file metto i token in eccesso all'interno dell'ultimo 
                    //token valido e rimuovo gli altri.
                    if (nameSpaceType.TokenNumber + 1 < tokensAr.Length)
                    {
                        for (int i = 0; i < nameSpaceType.TokenNumber + 1; i++)
                            tokens.Add(tokensAr[i]);

                        for (int n = nameSpaceType.TokenNumber + 1; n < tokensAr.Length; n++)
                            tokens[nameSpaceType.TokenNumber] += TokenSeparator + tokensAr[n];
                        return;
                    }

                    tokens.AddRange(tokensAr);
                    return;
                }
                return;
            }

            tokensAr = null;
            Debug.Fail("Il Namespace " + fullNameSpace + " è errato");
            fullNameSpace = string.Empty;
        }

        /// <summary>
        /// Indica se un name space è valido
        /// </summary>
        /// <returns>true se è valido</returns>
        //---------------------------------------------------------------------
        public bool IsValid()
        {
            if (nameSpaceType == null)
                return false;

            // Il numero di segmenti deve essere almeno quello del tipo di namespace
            return tokens.Count > nameSpaceType.TokenNumber;
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Restituisce il valore di un token in base al tipo specificato
        /// </summary>
        /// <param name="tokenType">Tipo del tocke, es. document</param>
        /// <returns></returns>
        //---------------------------------------------------------------------
        public string GetTokenValue(string tokenType)
        {
            NameSpaceObjectType aNameSpaceObjectType = NameSpaceTable.GetTokenTypeByName(tokenType);

            if (aNameSpaceObjectType == NameSpaceObjectType.NotValid)
            {
                Debug.Fail("Error in NameSpace.GetTokenValue");
                return string.Empty;
            }

            return GetTokenValue(aNameSpaceObjectType);
        }

        /// <summary>
        /// Restituisce il valore di un token in base al tipo specificato
        /// </summary>
        /// <param name="tokenType">Tipo del tocke, es. document</param>
        /// <returns></returns>
        //---------------------------------------------------------------------
        public string GetTokenValue(NameSpaceObjectType tokenType)
        {
            //il numero del token che contiene il tipo richiesto
            int tokenIndex = GetTokenNumberByType(tokenType);

            if (tokenIndex < 0 || tokenIndex >= tokens.Count)
            {
                //Debug.Fail("NameSpace errato");
                return string.Empty;
            }

            string token = tokens[tokenIndex];
            if (tokenType == NameSpaceObjectType.Leaf)
            {
                int sepPos = token.LastIndexOf(TokenSeparator);
                if (sepPos > 0)
                    return token.Substring(sepPos + 1);
            }
            return token;
        }

        //---------------------------------------------------------------------
        private int GetTokenNumberByType(NameSpaceObjectType tokenType)
        {
            //caso particolare: se voglio l'ultimo token, l'informazione è dinamica 
            //e non posso farla calcolare alla namespacetable
            if (tokenType == NameSpaceObjectType.Leaf)
                return tokens.Count - 1;

            return NameSpaceTable.GetTokenNumberByType(tokenType);
        }

        //---------------------------------------------------------------------
        private void SetTokenValue(NameSpaceObjectType tokenType, string value)
        {
            //il numero del token che contiene il tipo richiesto
            int tokenIndex = GetTokenNumberByType(tokenType);

            if (tokenIndex >= tokens.Count)
            {
                // throw new ArgumentException("tokenType");
                // DP20120626
                // append new array element blank 
                for (int i = 0; i <= tokenIndex - tokens.Count + 1; i++)
                {
                    tokens.Add("");
                }

            }

            tokens[tokenIndex] = value;

            RebuildFullNamespaceString();
        }

        //---------------------------------------------------------------------
        private void RebuildFullNamespaceString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < tokens.Count; i++)
            {
                if (sb.Length > 0)
                    sb.Append(TokenSeparator);
                sb.Append(tokens[i]);
            }
            fullNameSpace = sb.ToString();
        }


        /// <summary>
        /// Restituisce il valore dell'ultimo token di namespace
        /// </summary>
        /// <remarks>
        /// Ad esempio, questo metodo invocato su un namespace
        /// token1.token2.token3 deve ritornare la stringa token3.
        /// </remarks>
        //---------------------------------------------------------------------
        public string Leaf
        {
            get { return GetTokenValue(NameSpaceObjectType.Leaf); }
            set { SetTokenValue(NameSpaceObjectType.Leaf, value); }
        }

        //---------------------------------------------------------------------
        public string GetNameSpaceWithoutType()
        {
            string nameSpaceString = tokens[1];

            for (int i = 2; i < tokens.Count; i++)
            {
                nameSpaceString = nameSpaceString + TokenSeparator + tokens[i];
            }
            return nameSpaceString;
        }

        //---------------------------------------------------------------------
        public string GetNameSpaceWithoutLastToken()
        {
            string ns = string.Empty;
            for (int i = 0; i < tokens.Count - 1; i++)
            {
                ns += string.IsNullOrEmpty(ns)
                    ? tokens[i]
                    : TokenSeparator + tokens[i];
            }
            return ns;
        }

        //---------------------------------------------------------------------
        public string GetEndPointNameSpace()
        {
            string nameSpaceString = tokens[1];

            for (int i = 2; i < tokens.Count - 1; i++)
            {
                nameSpaceString = nameSpaceString + TokenSeparator + tokens[i];
            }
            return nameSpaceString;
        }

        //---------------------------------------------------------------------
        public string GetReportFileName()
        {
            if (this.NameSpaceType.Type != NameSpaceObjectType.Report)
                return String.Empty;

            string reportName = this.Report;
            if (reportName == null || reportName == String.Empty)
                return String.Empty;

            if (reportName.Length > NameSolverStrings.WrmExtension.Length)
            {
                string localExt = reportName.Substring(reportName.Length - NameSolverStrings.WrmExtension.Length);
                if (string.Compare(localExt, NameSolverStrings.WrmExtension, StringComparison.OrdinalIgnoreCase) != 0)
                    reportName += NameSolverStrings.WrmExtension;
            }
            else
                reportName += NameSolverStrings.WrmExtension;

            return reportName;
        }

        //---------------------------------------------------------------------
        public string GetExcelDocumentFileName()
        {
            if (this.NameSpaceType.Type != NameSpaceObjectType.ExcelDocument)
                return String.Empty;

            string documentName = this.ExcelDocument;
            if (documentName == null || documentName == String.Empty)
                return String.Empty;


            if (documentName.Length > NameSolverStrings.ExcelDocumentExtension.Length)
            {
                string localExt = documentName.Substring(documentName.Length - NameSolverStrings.ExcelDocumentExtension.Length);
                if (string.Compare(localExt, NameSolverStrings.ExcelDocumentExtension, StringComparison.OrdinalIgnoreCase) != 0)
                    documentName += NameSolverStrings.ExcelDocumentExtension;
            }
            else
                documentName += NameSolverStrings.ExcelDocumentExtension;

            return documentName;
        }

        //---------------------------------------------------------------------
        public string GetExcelTemplateFileName()
        {
            if (this.NameSpaceType.Type != NameSpaceObjectType.ExcelTemplate)
                return String.Empty;

            string templateName = this.ExcelTemplate;
            if (templateName == null || templateName == String.Empty)
                return String.Empty;

            if (templateName.Length > NameSolverStrings.ExcelTemplateExtension.Length)
            {
                string localExt = templateName.Substring(templateName.Length - NameSolverStrings.ExcelTemplateExtension.Length);
                if (string.Compare(localExt, NameSolverStrings.ExcelTemplateExtension, StringComparison.OrdinalIgnoreCase) != 0)
                    templateName += NameSolverStrings.ExcelTemplateExtension;
            }
            else
                templateName += NameSolverStrings.ExcelTemplateExtension;

            return templateName;
        }

        //---------------------------------------------------------------------
        public string GetWordDocumentFileName()
        {
            if (this.NameSpaceType.Type != NameSpaceObjectType.WordDocument)
                return String.Empty;

            string documentName = this.WordDocument;
            if (documentName == null || documentName == String.Empty)
                return String.Empty;

            if (documentName.Length > NameSolverStrings.WordDocumentExtension.Length)
            {
                string localExt = documentName.Substring(documentName.Length - NameSolverStrings.WordDocumentExtension.Length);
                if (string.Compare(localExt, NameSolverStrings.WordDocumentExtension, StringComparison.OrdinalIgnoreCase) != 0)
                    documentName += NameSolverStrings.WordDocumentExtension;
            }
            else
                documentName += NameSolverStrings.WordDocumentExtension;

            return documentName;
        }

        //---------------------------------------------------------------------
        public string GetWordTemplateFileName()
        {
            if (this.NameSpaceType.Type != NameSpaceObjectType.WordTemplate)
                return String.Empty;

            string templateName = this.WordTemplate;
            if (templateName == null || templateName == String.Empty)
                return String.Empty;

            if (templateName.Length > NameSolverStrings.WordTemplateExtension.Length)
            {
                string localExt = templateName.Substring(templateName.Length - NameSolverStrings.WordTemplateExtension.Length);
                if (string.Compare(localExt, NameSolverStrings.WordTemplateExtension, StringComparison.OrdinalIgnoreCase) != 0)
                    templateName += NameSolverStrings.WordTemplateExtension;
            }
            else
                templateName += NameSolverStrings.WordTemplateExtension;

            return templateName;
        }

        //---------------------------------------------------------------------
        public string GetExcel2007DocumentFileName()
        {
            if (this.NameSpaceType.Type != NameSpaceObjectType.ExcelDocument2007)
                return String.Empty;

            string documentName = this.ExcelDocument2007;
            if (documentName == null || documentName == String.Empty)
                return String.Empty;

            if (documentName.Length > NameSolverStrings.Excel2007DocumentExtension.Length)
            {
                string localExt = documentName.Substring(documentName.Length - NameSolverStrings.Excel2007DocumentExtension.Length);
                if (string.Compare(localExt, NameSolverStrings.Excel2007DocumentExtension, StringComparison.OrdinalIgnoreCase) != 0)
                    documentName += NameSolverStrings.Excel2007DocumentExtension;
            }
            else
                documentName += NameSolverStrings.Excel2007DocumentExtension;

            return documentName;
        }

        //---------------------------------------------------------------------
        public string GetExcel2007TemplateFileName()
        {
            if (this.NameSpaceType.Type != NameSpaceObjectType.ExcelTemplate2007)
                return String.Empty;

            string templateName = this.ExcelTemplate2007;
            if (templateName == null || templateName == String.Empty)
                return String.Empty;

            if (templateName.Length > NameSolverStrings.Excel2007TemplateExtension.Length)
            {
                string localExt = templateName.Substring(templateName.Length - NameSolverStrings.Excel2007TemplateExtension.Length);
                if (string.Compare(localExt, NameSolverStrings.Excel2007TemplateExtension, StringComparison.OrdinalIgnoreCase) != 0)
                    templateName += NameSolverStrings.Excel2007TemplateExtension;
            }
            else
                templateName += NameSolverStrings.Excel2007TemplateExtension;

            return templateName;
        }

        //---------------------------------------------------------------------
        public string GetWord2007DocumentFileName()
        {
            if (this.NameSpaceType.Type != NameSpaceObjectType.WordDocument2007)
                return String.Empty;

            string documentName = this.WordDocument2007;
            if (documentName == null || documentName == String.Empty)
                return String.Empty;

            if (documentName.Length > NameSolverStrings.Word2007DocumentExtension.Length)
            {
                string localExt = documentName.Substring(documentName.Length - NameSolverStrings.Word2007DocumentExtension.Length);
                if (string.Compare(localExt, NameSolverStrings.Word2007DocumentExtension, StringComparison.OrdinalIgnoreCase) != 0)
                    documentName += NameSolverStrings.Word2007DocumentExtension;
            }
            else
                documentName += NameSolverStrings.Word2007DocumentExtension;

            return documentName;
        }

        //---------------------------------------------------------------------
        public string GetWord2007TemplateFileName()
        {
            if (this.NameSpaceType.Type != NameSpaceObjectType.WordTemplate2007)
                return String.Empty;

            string templateName = this.WordTemplate2007;
            if (templateName == null || templateName == String.Empty)
                return String.Empty;

            if (templateName.Length > NameSolverStrings.Word2007TemplateExtension.Length)
            {
                string localExt = templateName.Substring(templateName.Length - NameSolverStrings.Word2007TemplateExtension.Length);
                if (string.Compare(localExt, NameSolverStrings.Word2007TemplateExtension, StringComparison.OrdinalIgnoreCase) != 0)
                    templateName += NameSolverStrings.Word2007TemplateExtension;
            }
            else
                templateName += NameSolverStrings.Word2007TemplateExtension;

            return templateName;
        }

        //---------------------------------------------------------------------
        public static implicit operator NameSpace(String aNamespace)
        {
            if (aNamespace == null || aNamespace.Trim().Length == 0)
                return NameSpace.Empty;

            return new NameSpace(aNamespace);
        }

        //---------------------------------------------------------------------
        public static implicit operator String(NameSpace aNamespace)
        {
            if (aNamespace == null)
                return String.Empty;

            return aNamespace.FullNameSpace;
        }
    }
}

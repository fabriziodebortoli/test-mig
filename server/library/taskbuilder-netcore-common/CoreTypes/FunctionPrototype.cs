using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;



using Microarea.Common.NameSolver;
using Microarea.Common.StringLoader;
using Microarea.Common.Generic;
using Microarea.Common.Lexan;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.Common.CoreTypes
{
    //=========================================================================
    public class Parameter : IParameter
    {
        private static readonly Object staticLockTicket = new Object();

        public const string ParameterTag = "Param";

        private string name = String.Empty;
        protected string title = String.Empty;

        private string type = String.Empty;
        private string baseType = String.Empty;
        private ushort enumTag = 0;
        private string tbType = String.Empty;
        private string tbBaseType = String.Empty;

        private bool optional = false;
        private ParameterModeType mode = ParameterModeType.In;

        protected string valueString;
        protected ArrayList values;

        public string ValueString
        {
            get { return valueString; }

            set { valueString = value; }
        }
        public ArrayList Values
        {
            get
            {
                if (values is null)
                {
                    values = new ArrayList();
                }
                return values;
            }
        }

        //---------------------------------------------------------------------
        public virtual string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                if (name == null)
                    name = String.Empty;
            }
        }

        //---------------------------------------------------------------------
        public string Title
        {
            get
            {
                return title;
            }
        }

        //---------------------------------------------------------------------
        public virtual string Type
        {
            get
            {
                return type;
            }
            //set
            //{
            //    if (value == null)
            //        throw new ArgumentNullException("value", "Type cannot be null");

            //    if (value.Trim().Length == 0)
            //        throw new ArgumentException("Type cannot be empty", "value");

            //    type = value;
            //}
        }

        //---------------------------------------------------------------------
        public virtual string BaseType
        {
            get
            {
                return baseType;
            }
            set
            {
                if (value == null)
                    value = String.Empty;

                baseType = value;
            }
        }

        //---------------------------------------------------------------------
        public virtual string TbType
        {
            get
            {
                return tbType.IsNullOrEmpty() ? type : tbType; ;
            }
            set
            {
                tbType = value;
            }
        }

        //---------------------------------------------------------------------
        public virtual string TbBaseType
        {
            get
            {
                return tbBaseType.IsNullOrEmpty() ? baseType : tbBaseType;
            }
            set
            {
                tbBaseType = value;
            }
        }

        //---------------------------------------------------------------------
        public ushort EnumTag { get { return enumTag; } }

        //---------------------------------------------------------------------
        public void SetType(string typ, string baseTyp, ushort tag)
        {
            this.type = typ;
            this.baseType = baseTyp;
            this.enumTag = tag;
        }

        //---------------------------------------------------------------------
        public virtual bool Optional
        {
            get
            {
                return optional;
            }
            set
            {
                optional = value;
            }
        }

        //---------------------------------------------------------------------
        public ParameterModeType Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        //---------------------------------------------------------------------
        public Parameter()
        {
        }

        //---------------------------------------------------------------------
        public Parameter(string nam, string type, ParameterModeType mode = ParameterModeType.In)
        {
            this.name = nam;
            if (name == null)
                name = String.Empty;

            this.type = type;
            this.mode = mode;
        }

        //-----------------------------------------------------------------------------
        public Parameter(string nam, string type, string baseType, ParameterModeType mode = ParameterModeType.In)
        {
            this.name = nam;
            if (name == null)
                name = String.Empty;

            this.type = type;
            this.baseType = baseType;

            this.mode = mode;
        }

        //-----------------------------------------------------------------------------
        public static ParameterModeType ParseMode(string mode)
        {
            if (String.Compare(mode, "out", StringComparison.OrdinalIgnoreCase) == 0)
                return ParameterModeType.Out;

            if (String.Compare(mode, "in out", StringComparison.OrdinalIgnoreCase) == 0)
                return ParameterModeType.InOut;

            return ParameterModeType.In;
        }

        //-----------------------------------------------------------------
        public bool Parse(XmlElement pElement)
        {
            if (pElement == null)
            {
                Debug.Fail("Error in ParameterInfo.Parse");
                return false;
            }

            //namespace delle function
            name = pElement.GetAttribute(WebMethodsXML.Attribute.Name);

            //setto il titolo del mio oggetto
            title = pElement.GetAttribute(WebMethodsXML.Attribute.Localize);

            //setto il tipo del mio oggetto
            tbType = pElement.GetAttribute(WebMethodsXML.Attribute.Type);
            type = ObjectHelper.FromTBType(tbType);

            tbBaseType = pElement.GetAttribute(WebMethodsXML.Attribute.BaseType);
            baseType = tbBaseType;
            if (!baseType.IsNullOrEmpty() && tbType == "array")
            {
                baseType = ObjectHelper.FromTBType(tbBaseType);
            }

            if (tbType == "array")
            {
                if (pElement.HasChildNodes)
                {
                    foreach (XmlNode child in pElement.ChildNodes)
                    {
                        if (child.Name.CompareNoCase(WebMethodsXML.Attribute.Value))
                        {
                            valueString += child.InnerText;
                            object to = ObjectHelper.CreateObject(baseType);
                            to = ObjectHelper.CastFromDBData(child.InnerText, to);
                            Values.Add(to);
                        }
                    }
                }
            }
            else
                valueString = pElement.GetAttribute(WebMethodsXML.Attribute.Value);



            string temp = pElement.GetAttribute(WebMethodsXML.Attribute.Optional);
            if (temp != null && temp.Length > 0)
            {
                try
                {
                    optional = Boolean.Parse(temp);
                }
                catch (FormatException)
                {
                    throw;
                }
            }
            else
                optional = false;

            temp = pElement.GetAttribute(WebMethodsXML.Attribute.Mode);
            if (temp != null && temp.Length > 0)
                mode = ParseMode(temp);
            else
                mode = ParameterModeType.In;

            return true;
        }

        //-----------------------------------------------------------------
        public bool Unparse(XmlElement pElement)
        {
            if (pElement == null)
            {
                Debug.Fail("Error in ParameterInfo.Unparse");
                return false;
            }

            //namespace delle function
            if (name != null)
                pElement.SetAttribute(WebMethodsXML.Attribute.Name, name);

            //setto il titolo del mio oggetto
            if (title != null)
                pElement.SetAttribute(WebMethodsXML.Attribute.Localize, title);

            //setto il tipo del mio oggetto
            if (!TbType.IsNullOrEmpty())
                pElement.SetAttribute(WebMethodsXML.Attribute.Type, TbType);

            //setto il tipo base del mio oggetto presente solo negli enumnerativi ed array
            if (!TbBaseType.IsNullOrEmpty())
                pElement.SetAttribute(WebMethodsXML.Attribute.BaseType, TbBaseType);

            if (this.mode == ParameterModeType.In)
                pElement.SetAttribute(WebMethodsXML.Attribute.Mode, "in");
            else if (this.mode == ParameterModeType.Out)
                pElement.SetAttribute(WebMethodsXML.Attribute.Mode, "out");
            else //if (this.mode == ParameterModeType.InOut)
                pElement.SetAttribute(WebMethodsXML.Attribute.Mode, "in out");

            if (optional)
                pElement.SetAttribute(WebMethodsXML.Attribute.Optional, optional ? "true" : "false");

            if (type == "array" && values != null)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    XmlElement value = pElement.OwnerDocument.CreateElement(WebMethodsXML.Attribute.Value);
                    value.InnerText = values[i].ToString();
                    pElement.AppendChild(value);
                }
            }
            else if (valueString != null)
                pElement.SetAttribute(WebMethodsXML.Attribute.Value, valueString);

            return true;
        }

    }

    //=========================================================================
    public class ParametersList : List<Parameter>
    {
        //---------------------------------------------------------------------------
        public Parameter this[string name]
        {
            get
            {
                foreach (Parameter info in this)
                {
                    if (name.CompareNoCase(info.Name))
                        return info;
                }
                return null;
            }
            set
            {
                Parameter existingInfo = null;
                foreach (Parameter info in this)
                {
                    if (string.Compare(info.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        existingInfo = info;
                        break;
                    };
                }

                if (existingInfo != null)
                    Remove(existingInfo);
                Add(value);
            }
        }

        //---------------------------------------------------------------------------
        public string[] ParamNames
        {
            get
            {
                string[] list = new string[this.Count];
                int i = 0;
                foreach (Parameter info in this)
                    list[i++] = info.Name;
                return list;
            }
        }

        //---------------------------------------------------------------------------
        public void Parse(XmlNodeList paramNodes)
        {
            foreach (XmlElement paramElement in paramNodes)
            {
                Parameter pi = new Parameter();

                if (pi.Parse(paramElement))
                    Add(pi);
            }
        }

        //-------------------------------------------------------------------------
        public void Unparse(XmlElement rootElement)
        {
            foreach (Parameter param in this)
            {
                XmlElement paramElement = rootElement.OwnerDocument.CreateElement(WebMethodsXML.Element.Param);
                param.Unparse(paramElement);
                rootElement.AppendChild(paramElement);
            }
        }

        //-------------------------------------------------------------------------
        public string Unparse()
        {
            XmlDocument d = new XmlDocument();
            d.AppendChild(d.CreateElement(WebMethodsXML.Element.Arguments));
            Unparse(d.DocumentElement);
            return d.OuterXml;
        }


    }

    /// <summary>
    /// Summary description for FunctionPrototype.
    /// </summary>
    ///=============================================================================
    public class FunctionPrototype : IFunctionPrototype
    {
        protected ModuleInfo moduleInfo = null;

        public ModuleInfo ModuleInfo
        {
            get { return moduleInfo; }
            set
            {
                moduleInfo = value;
            }
        }
        //----

        protected NameSpace nameSpace;
        public INameSpace NameSpace { get { return nameSpace; } }

        private string name;

        public string FullName
        {
            get { return nameSpace != null && nameSpace.IsValid() ? nameSpace.GetNameSpaceWithoutType() : name; }
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
            }
        }

        //usata in una combobox
        public override string ToString()
        {
            return Name;
        }

        //----
        protected ParametersList parameters = new ParametersList();
        public ParametersList Parameters
        {
            get { return parameters; }
            set
            {
                parameters = value;
            }
        }

        // informazioni specifiche per WebService esterni (come funzione da chiamare uso solo il Name)
        //-----------------------------------------------------------------------------
        private string server = "";
        private int port = 80;
        private string service = "";
        private string serviceNamespace = "";

        public string Server { get { return server; } set { server = value; } }
        public int Port { get { return port; } set { port = value; } }
        public string Service { get { return service; } set { service = value; } }
        public string ServiceNamespace { get { return serviceNamespace; } set { serviceNamespace = value; } }

        //-----------------------------------------------------------------------------
        private string returnType = String.Empty;
        private string returnBaseType = String.Empty;	 //used only in functions which returns a DataArray/handle
        private ushort returnEnumTag = 0;
        private string longDescription = String.Empty;

        public string LongDescription { get { return longDescription; } set { longDescription = value; } }
        public string ReturnType { get { return returnType; } }
        public string ReturnBaseType { get { return returnBaseType; } }
        public ushort ReturnEnumTag { get { return returnEnumTag; } }

        private string returnTbType = String.Empty;
        private string returnTbBaseType = String.Empty;
        public string ReturnTbType
        {
            get { return returnTbType.IsNullOrEmpty() ? returnType : returnTbType; }
            set { returnTbType = value; }
        }
        public string ReturnTbBaseType { get { return returnTbBaseType.IsNullOrEmpty() ? returnBaseType : returnTbBaseType; } }

        //---------------------------------------------------------------------
        public void SetReturnType(string typ, string baseTyp, ushort tag)
        {
            this.returnType = typ;
            this.returnBaseType = baseTyp;
            this.returnEnumTag = tag;
        }

        //----
        protected string title;
        protected string sourceInfo;
        protected string wcf;
        public string WCF { get { return wcf; } set { wcf = value; } }
        public virtual string Title
        {
            get
            {
                return
                    moduleInfo == null
                    ? title
                    : LocalizableXmlDocument.LoadXMLString
                    (
                        title,
                        NameSolverStrings.WebMethodsXml,
                        moduleInfo.DictionaryFilePath
                    );
            }
        }

        //----
        public bool ReportAllowed = true;
        public bool IsEvent = false;
        public string ClassType = String.Empty;

        //----
        protected string defaultSecurityRoles;
        private bool isSecurityhidden = false;
        private bool inEasyStudio = false;
        public CoreTypes.Value ReturnValue = null;

        public string SourceInfo
        {
            get
            {
                return sourceInfo;
            }
            set
            {
                sourceInfo = value;
            }
        }

        public string DefaultSecurityRoles { get { return defaultSecurityRoles; } }
        public bool IsSecurityhidden { get { return isSecurityhidden; } set { isSecurityhidden = value; } }
        public bool InEasyStudio { get { return inEasyStudio; } set { inEasyStudio = value; } }

        //-----------------------------------------------------------------------------
        // Costruttore usato per le funzioni interne
        public FunctionPrototype
            (
                string aName,
                string retType,
                string[] paramsType = null
            )
        {
            name = aName;
            nameSpace = new NameSpace(aName, NameSpaceObjectType.Function);

            this.returnType = retType;

            // non ci sono parametri
            if (paramsType == null || paramsType.Length <= 0)
                return;

            // costruisco tutti i parametri per la funzione da prototipare
            foreach (string paramType in paramsType)
                Parameters.Add(new Parameter("", paramType, ParameterModeType.In));
        }

        //-----------------------------------------------------------------------------
        public FunctionPrototype
            (
                NameSpace aNameSpace,
                string aTitle,
                string retType,
                string retBaseType
            )
        {
            name = aNameSpace.Function;
            nameSpace = aNameSpace;

            title = aTitle;

            returnType = retType;
            returnBaseType = retBaseType;
        }

        //-----------------------------------------------------------------------------
        public FunctionPrototype()
        {
        }

        //-----------------------------------------------------------------------------
        public IParameter GetParameter(int i)
        {
            return (IParameter)Parameters[i];
        }

        //-----------------------------------------------------------------------------
        public string ParameterType(int i)
        {
            //			Debug.Assert(i <= Parameters.Count);
            Parameter parameter = Parameters[i];

            return parameter.Type;
        }

        //-----------------------------------------------------------------------------
        public int NrParameters { get { return Parameters.Count; } }

        //-----------------------------------------------------------------------------
        public int ParamIndex(string name)
        {
            for (int i = 0; i < Parameters.Count; i++)
                if (string.Compare(name, Parameters[i].Name, StringComparison.OrdinalIgnoreCase) == 0) return i;

            return -1;
        }

        //-------------------------------------------------------------------------
        public static FunctionPrototype ParseFunction(XmlElement functionElement)
        {
            string name = functionElement.GetAttribute(WebMethodsXML.Attribute.Name);
            //namespace delle function
            string fullNameSpaceString = functionElement.GetAttribute(WebMethodsXML.Attribute.Namespace);
            //Creo il Namespace da passare nel costruttore
            NameSpace nameSpace = new NameSpace(fullNameSpaceString, NameSpaceObjectType.Function);

            //setto il titolo del mio oggetto
            string title = functionElement.GetAttribute(WebMethodsXML.Attribute.Localize);
            if (title == string.Empty || string.Compare(title, "") == 0)
            {
                string[] token = fullNameSpaceString.Split('.');
                title = token[token.Length - 1];
            }

            //setto il tipo del mio oggetto
            string tbType = functionElement.GetAttribute(WebMethodsXML.Attribute.Type);
            string type = ObjectHelper.FromTBType(tbType);

            string tbBaseType = functionElement.GetAttribute(WebMethodsXML.Attribute.BaseType);
            string baseType = tbBaseType;
            if (!baseType.IsNullOrEmpty() && tbType == "array")
            {
                baseType = ObjectHelper.FromTBType(tbBaseType);
            }

            string aDefaultSecurityRoles = functionElement.GetAttribute(WebMethodsXML.Attribute.DefaultSecurityRoles);
            string securityhidden = functionElement.GetAttribute(WebMethodsXML.Attribute.Securityhidden);

            bool isSecurityhidden = false;
            if (securityhidden != null && securityhidden.Length > 0)
            {
                try
                {
                    isSecurityhidden = Convert.ToBoolean(securityhidden);
                }
                catch (FormatException)
                {
                }
            }

            string inEasyStudio = functionElement.GetAttribute(WebMethodsXML.Attribute.InEasyStudio);
            bool isInEasyStudio = false;
            if (inEasyStudio != null && inEasyStudio.Length > 0)
            {
                try
                {
                    isInEasyStudio = Convert.ToBoolean(inEasyStudio);
                }
                catch (FormatException)
                {
                }
            }

            bool bReport = true;
            string report = functionElement.GetAttribute(WebMethodsXML.Attribute.Report);
            if (report != null && string.Compare(bool.FalseString, report, StringComparison.OrdinalIgnoreCase) == 0)
                bReport = false;

            int port;
            if (!int.TryParse(functionElement.GetAttribute(WebMethodsXML.Attribute.Port), out port))
                port = 80;

            FunctionPrototype functionInfo = new FunctionPrototype(nameSpace, title, type, baseType);
            if (!name.IsNullOrWhiteSpace())
                functionInfo.Name = name;

            functionInfo.defaultSecurityRoles = aDefaultSecurityRoles;
            functionInfo.IsSecurityhidden = isSecurityhidden;
            functionInfo.InEasyStudio = isInEasyStudio;
            functionInfo.ReportAllowed = bReport;

            functionInfo.ClassType = functionElement.GetAttribute(WebMethodsXML.Attribute.ClassType);
            functionInfo.SourceInfo = functionElement.GetAttribute(WebMethodsXML.Attribute.SourceInfo);
            functionInfo.Server = functionElement.GetAttribute(WebMethodsXML.Attribute.Server);
            functionInfo.Port = port;
            functionInfo.Service = functionElement.GetAttribute(WebMethodsXML.Attribute.Service);
            functionInfo.ServiceNamespace = functionElement.GetAttribute(WebMethodsXML.Attribute.ServiceNamespace);
            functionInfo.LongDescription = functionElement.InnerText;
            functionInfo.returnTbBaseType = tbBaseType;
            functionInfo.returnTbType = tbType;

            ParseParameters(functionElement, functionInfo);

            return functionInfo;
        }

        //-------------------------------------------------------------------------
        public static void ParseParameters(XmlElement functionElement, FunctionPrototype functionInfo)
        {
            XmlNodeList paramNodes = null;
            //Parsing dei parametri.
            paramNodes = functionElement.GetElementsByTagName(WebMethodsXML.Element.Param);
            if (paramNodes == null)
                return;

            functionInfo.parameters = new ParametersList();

            functionInfo.parameters.Parse(paramNodes);
        }

        //--------------------------------------------------------------------------------
        public string GetFunctionDescription()
        {
            StringBuilder strHelp = new StringBuilder();

            strHelp.Append(Title);
            strHelp.Append("\r\n");

            strHelp.Append(ReturnTbType);
            if (!returnTbBaseType.IsNullOrEmpty())
            {
                strHelp.Append(" ");
                strHelp.Append(" [");
                strHelp.Append(returnTbBaseType);
                strHelp.Append("]");
            }

            strHelp.Append(" ");
            strHelp.Append(Name);

            strHelp.Append(" (");
            for (int i = 0; i < NrParameters; i++)
            {
                Parameter par = parameters[i];
                switch (par.Mode)
                {
                    case ParameterModeType.In:
                        strHelp.Append("in ");
                        break;
                    case ParameterModeType.Out:
                        strHelp.Append("out ");
                        break;
                    case ParameterModeType.InOut:
                        strHelp.Append("in out ");
                        break;
                    default:
                        break;
                }

                strHelp.Append(par.TbType);
                if (!par.TbBaseType.IsNullOrEmpty())
                {
                    strHelp.Append(" ");
                    strHelp.Append(" [");
                    strHelp.Append(par.TbBaseType);
                    strHelp.Append("]");
                }

                strHelp.Append(" ");

                strHelp.Append(par.Name);
                strHelp.Append(" [");
                strHelp.Append(par.Title);
                strHelp.Append("] ");

                if (i < (NrParameters - 1))
                    strHelp.Append(", ");
            }
            strHelp.Append(" )");

            return strHelp.ToString();
        }
    }

    /// <summary>
    /// Summary description for Functions (WebMethods).
    /// </summary>
    ///=============================================================================
    public class FunctionsList : IFunctions
    {
        private List<IFunctionPrototype> prototypes = new List<IFunctionPrototype>();

        public IDictionary<string, List<IFunctionPrototype>> ClassMethods =
            new Dictionary<string, List<IFunctionPrototype>>(StringComparer.OrdinalIgnoreCase);

        //private List<IFunctionPrototype> internalFunctions = new List<IFunctionPrototype>();

        public List<IFunctionPrototype> Prototypes
        {
            get { return prototypes; }
        }

        //-----------------------------------------------------------------------------
        public FunctionsList()
        {
            LoadPrototypes();
            AddInternalWoormFunction();
        }

        //-----------------------------------------------------------------------------
        public void Add(string fnName, string rt, string[] prs)
        {
            FunctionPrototype fd = new FunctionPrototype(fnName, rt, prs);
            //internalFunctions.Add(fd);
            Add(fd);
        }

        //-----------------------------------------------------------------------------
        public void Add(FunctionPrototype fd)
        {
            prototypes.Add(fd);

            string classType = fd.ClassType;

            if (classType.IsNullOrEmpty())
                return;

            int idx = classType.IndexOf(',');
            if (idx > 0)
                classType = classType.Left(idx);

            classType = classType.Trim();

            List<IFunctionPrototype> par = null;
            if (!ClassMethods.TryGetValue(classType, out par))
            {
                par = new List<IFunctionPrototype>();

                ClassMethods.Add(classType, par);
            }
            par.Add(fd);
        }

        //-----------------------------------------------------------------------------
        private bool LoadPrototypes(ModuleInfo mi)
        {
            string path = (mi == null)
                ? string.Empty
                : mi.GetWebMethodsPath();

            if (!PathFinder.PathFinderInstance.FileSystemManager.ExistFile(path))
                return false;

            XmlDocument dom = null;
            dom = PathFinder.PathFinderInstance.FileSystemManager.LoadXmlDocument(dom, path);

            // cerca con XPath solo le funzioni con un dato nome per poi selezionare quella con i parametri giusti
            XmlNode root = dom.DocumentElement;
            string xpath = string.Format
                (
                    "/{0}/{1}/{2}[@{3}]",
                    WebMethodsXML.Element.FunctionObjects,
                    WebMethodsXML.Element.Functions,
                    WebMethodsXML.Element.Function,
                    WebMethodsXML.Attribute.Namespace
                );

            // se non esiste la sezione allora la funzione � Undefined
            XmlNodeList functions = root.SelectNodes(xpath);
            if (functions == null)
                return false;

            foreach (XmlElement function in functions)
            {
                FunctionPrototype fp = FunctionPrototype.ParseFunction(function);

                fp.ModuleInfo = mi;

                mi.WebMethods.Add(fp);

                Add(fp);
            }

            return true;
        }

        //-----------------------------------------------------------------------------
        protected bool LoadPrototypeFromXml(string name)
        {
            // se il file delle funzioni esterne non esiste allora la funzione � indefinita
            NameSpace ns = new NameSpace(name, NameSpaceObjectType.Function);
            ModuleInfo mi = PathFinder.PathFinderInstance.GetModuleInfo(ns);

            return LoadPrototypes(mi);
        }

        //-----------------------------------------------------------------------------
        protected void LoadPrototypes(ApplicationInfo ai)
        {
            foreach (ModuleInfo mi in ai.Modules)
                LoadPrototypes(mi);
        }

        //-----------------------------------------------------------------------------
        protected bool fullLoaded = false;
        public void LoadPrototypes()
        {
            if (fullLoaded)
                return;
            fullLoaded = true;  //NB: flag anticipato altrimenti alg. entra in ricorsione

            foreach (ApplicationInfo ai in  PathFinder.PathFinderInstance.ApplicationInfos)
                LoadPrototypes(ai);

            //fullLoaded = true;
        }

        //-----------------------------------------------------------------------------
        private void Add(Token fn, string rt, string[] prs)
        {
            this.Add(Language.GetTokenString(fn), rt, prs);
        }

        //-----------------------------------------------------------------------------
        private void AddInternalWoormFunction()
        {
            Add(Token.ABS, "Double", new string[] { "Double" });
            Add(Token.APPDATE, "DateTime", new string[0]);
            Add(Token.APPYEAR, "Int32", new string[0]);

            Add(Token.ARRAY_ATTACH, "Boolean", new string[] { "Array", "Array" });
            Add(Token.ARRAY_CLEAR, "Int64", new string[] { "Array" });
            Add(Token.ARRAY_COPY, "Boolean", new string[] { "Array", "Array" });
            Add(Token.ARRAY_DETACH, "Boolean", new string[] { "Array" });
            Add(Token.ARRAY_FIND, "Int64", new string[] { "Array", "Object" });
            Add(Token.ARRAY_FIND, "Int64", new string[] { "Array", "Object", "Int64" });
            Add(Token.ARRAY_GETAT, "Object", new string[] { "Array", "Int64" });
            Add(Token.ARRAY_SETAT, "Object", new string[] { "Array", "Int64", "Object" });
            Add(Token.ARRAY_SIZE, "Int64", new string[] { "Array" });
            Add(Token.ARRAY_SORT, "Boolean", new string[] { "Array" });
            Add(Token.ARRAY_SORT, "Boolean", new string[] { "Array", "Boolean" });
            Add(Token.ARRAY_SORT, "Boolean", new string[] { "Array", "Boolean", "Int64" });
            Add(Token.ARRAY_SORT, "Boolean", new string[] { "Array", "Boolean", "Int64", "Int64" });
            Add(Token.ARRAY_ADD, "Boolean", new string[] { "Array", "Object" });
            Add(Token.ARRAY_APPEND, "Boolean", new string[] { "Array", "Array" });
            Add(Token.ARRAY_INSERT, "Boolean", new string[] { "Array", "Int64", "Object" });
            Add(Token.ARRAY_REMOVE, "Object", new string[] { "Array", "Int64" });
            Add(Token.ARRAY_CONTAINS, "Boolean", new string[] { "Array", "Object" });
            Add(Token.ARRAY_CREATE, "Array", new string[] { "Object" });     //*
            Add(Token.ARRAY_SUM, "Object", new string[] { "Array" });     //*

            Add(Token.ASC, "Int32", new string[] { "String" });
            Add(Token.CDOW, "String", new string[] { "DateTime" });
            Add(Token.CEILING, "Double", new string[] { "Double" });
            Add(Token.CHR, "String", new string[] { "Int32" });
            Add(Token.CMONTH, "String", new string[] { "DateTime" });
            Add(Token.CONTENTOF, "Boolean", new string[] { "String" });
            Add(Token.CTOD, "DateTime", new string[] { "String" });
            Add(Token.DATE, "DateTime", new string[0]);
            Add(Token.DATE, "DateTime", new string[] { "Int32", "Int32", "Int32" });
            Add(Token.DATETIME, "DateTime", new string[0]);
            Add(Token.DATETIME, "DateTime", new string[] { "Int32", "Int32", "Int32", "Int32", "Int32", "Int32" });
            Add(Token.DAY, "Int32", new string[] { "DateTime" });
            Add(Token.DAYOFWEEK, "Int32", new string[] { "DateTime" });
            Add(Token.DAYOFYEAR, "Int32", new string[] { "DateTime" });

            Add(Token.DECODE, "Object", new string[] { "Object", "Object", "Object" });  //*
            Add(Token.CHOOSE, "Object", new string[] { "Int32", "Object" });  //*
            Add(Token.IIF, "Object", new string[] { "Boolean", "Object", "Object" });  //*

            Add(Token.DTOC, "String", new string[] { "DateTime" });
            Add(Token.ELAPSED_TIME, "Int64", new string[0]);
            Add(Token.ELAPSED_TIME, "Int64", new string[] { "DateTime", "DateTime" });
            Add(Token.FILEEXISTS, "Boolean", new string[] { "String" });
            Add(Token.FIND, "Int32", new string[] { "String", "String" });
            Add(Token.FIND, "Int32", new string[] { "String", "String", "Int32" });
            Add(Token.FIND, "Int32", new string[] { "String", "String", "Int32", "Int32" });
            Add(Token.FINT, "Int32", new string[] { "Double" });
            Add(Token.FLONG, "Int64", new string[] { "Double" });
            Add(Token.FLOOR, "Double", new string[] { "Double" });
            Add(Token.FORMAT, "String", new string[] { null, "String" });
            Add(Token.FORMAT, "String", new string[] { null });
            Add(Token.GETAPPTITLE, "String", new string[] { "String" });
            Add(Token.GETCOMPANYNAME, "String", new string[0]);
            Add(Token.GETCOMPUTERNAME, "String", new string[0]);
            Add(Token.GETCOMPUTERNAME, "String", new string[] { "Boolean" });
            Add(Token.GETCULTURE, "String", new string[0]);
            Add(Token.GETCULTURE, "String", new string[] { "String" });
            Add(Token.GETCULTURE, "String", new string[] { "String", "Boolean" });
            Add(Token.GETDATABASETYPE, "String", new string[0]);
            Add(Token.GETDOCTITLE, "String", new string[] { "String" });
            Add(Token.GETEDITION, "String", new string[0]);
            Add(Token.GETMODTITLE, "String", new string[] { "String" });
            Add(Token.GETNSFROMPATH, "String", new string[] { "String" });
            Add(Token.GETPATHFROMNS, "String", new string[] { "String" });
            Add(Token.GETPATHFROMNS, "String", new string[] { "String", "String" });
            Add(Token.GETINSTALLATIONNAME, "String", new string[0]);
            Add(Token.GETINSTALLATIONPATH, "String", new string[0]);
            Add(Token.GETINSTALLATIONVERSION, "String", new string[0]);
            Add(Token.GETLOGINNAME, "String", new string[0]);
            Add(Token.GETUSERDESCRIPTION, "String", new string[0]);
            Add(Token.GETUSERDESCRIPTION, "String", new string[] { "String" });
            //non sono gestiti overloading con uno stesso numero di parametri
            //			Add(functions, Token.GETUSERDESCRIPTION,	"String",	new string[] {"Int32"});
            Add(Token.GETNEWGUID, "String", new string[0]);
            Add(Token.GETOWNERNAMESPACE, "String", new string[] { "Int32" });
            Add(Token.GETPRODUCTLANGUAGE, "String", new string[0]);
            Add(Token.GETREPORTMODULENAMESPACE, "String", new string[0]);
            Add(Token.GETREPORTNAMESPACE, "String", new string[0]);
            Add(Token.GETREPORTPATH, "String", new string[0]);
            Add(Token.GETSETTING, "Object", new string[] { "String", "String", "String", "Object" });

            Add(Token.GETTHREADCONTEXT, "Boolean", new string[] { "String", "Object" });
            Add(Token.OWNTHREADCONTEXT, "Boolean", new string[] { "Object" });
            Add(Token.GETTITLE, "String", new string[] { "String" });

            Add(Token.GETWINDOWUSER, "String", new string[0]);
            Add(Token.GIULIANDATE, "Int64", new string[] { "DateTime" });
            Add(Token.GETUPPERLIMIT, "String", new string[] { "Int32" });

            Add(Token.ISACTIVATED, "Boolean", new string[] { "String", "String" });
            Add(Token.ISADMIN, "Boolean", new string[0]);
            Add(Token.ISAUTOPRINT, "Boolean", new string[0]);
            Add(Token.ISDATABASEUNICODE, "Boolean", new string[0]);
            Add(Token.ISEMPTY, "Boolean", new string[] { "Object" });
            Add(Token.ISNULL, "Object", new string[] { "Object", "Object" });
            Add(Token.ISREMOTEINTERFACE, "Boolean", new string[0]);
            Add(Token.ISRUNNINGFROMEXTERNALCONTROLLER, "Boolean", new string[0]);
            Add(Token.ISWEB, "Boolean", new string[0]);

            Add(Token.LAST_MONTH_DAY, "DateTime", new string[] { "DateTime" });
            Add(Token.LEFT, "String", new string[] { "String", "Int32" });
            Add(Token.LEN, "Int32", new string[] { "String" });
            Add(Token.LOADTEXT, "String", new string[] { "String" });
            Add(Token.LOCALIZE, "String", new string[] { "String" });
            Add(Token.LOCALIZE, "String", new string[] { "String", "String" });
            Add(Token.LOWER, "String", new string[] { "String" });
            Add(Token.LOWER, "String", new string[] { "String", "Boolean" });
            Add(Token.LTRIM, "String", new string[] { "String" });
            Add(Token.LTRIM, "String", new string[] { "String", "String" });
            Add(Token.MAX, "Object", new string[] { "Object", });   //array
            Add(Token.MAX, "Object", new string[] { "Object", "Object" });
            Add(Token.MAKELOWERLIMIT, "String", new string[] { "String" });
            Add(Token.MAKEUPPERLIMIT, "String", new string[] { "String" });
            Add(Token.MIN, "Object", new string[] { "Object" });   //array
            Add(Token.MIN, "Object", new string[] { "Object", "Object" });
            Add(Token.MOD, "Double", new string[] { "Double", "Double" });
            Add(Token.MONTH, "Int32", new string[] { "DateTime" });
            Add(Token.MONTH_DAYS, "Int32", new string[] { "Int32", "Int32" });
            Add(Token.MONTH_NAME, "String", new string[] { "Int32" });
            Add(Token.PADLEFT, "String", new string[] { "String", "Int32", "String" });
            Add(Token.PADRIGHT, "String", new string[] { "String", "Int32", "String" });
            Add(Token.RAND, "Double", new string[0]);
            Add(Token.REMOVENEWLINE, "String", new string[] { "String" });
            Add(Token.REPLACE, "String", new string[] { "String", "String", "String" });
            Add(Token.REPLICATE, "String", new string[] { "String", "Int32" });
            Add(Token.REVERSEFIND, "Int32", new string[] { "String", "String" });
            Add(Token.REVERSEFIND, "Int32", new string[] { "String", "String", "Int32" });
            Add(Token.REVERSEFIND, "Int32", new string[] { "String", "String", "Int32", "Int32" });
            Add(Token.RGB, "Int32", new string[] { "Int32", "Int32", "Int32" });
            Add(Token.RIGHT, "String", new string[] { "String", "Int32" });
            Add(Token.ROUND, "Double", new string[] { "Double" });
            Add(Token.ROUND, "Double", new string[] { "Double", "Int32" });
            Add(Token.RTRIM, "String", new string[] { "String" });
            Add(Token.RTRIM, "String", new string[] { "String", "String" });
            Add(Token.SAVETEXT, "Boolean", new string[] { "String", "String" });
            Add(Token.SAVETEXT, "Boolean", new string[] { "String", "String", "Int32" });
            Add(Token.SETCULTURE, "String", new string[] { "String" });
            Add(Token.SETSETTING, "Object", new string[] { "String", "String", "String", "Object" });
            Add(Token.SIGN, "Int32", new string[] { "Double" });
            Add(Token.SPACE, "String", new string[] { "Int32" });
            Add(Token.STR, "String", new string[] { "Double", "Int32" });
            Add(Token.STR, "String", new string[] { "Double", "Int32", "Int32" });
            Add(Token.SUBSTR, "String", new string[] { "String", "Int32" });
            Add(Token.SUBSTR, "String", new string[] { "String", "Int32", "Int32" });
            Add(Token.SUBSTRWW, "String", new string[] { "String", "Int32", "Int32" });
            Add(Token.TABLEEXISTS, "Boolean", new string[] { "String" });
            Add(Token.TABLEEXISTS, "Boolean", new string[] { "String", "String" });
            Add(Token.TIME, "DateTime", new string[0]);
            Add(Token.TIME, "DateTime", new string[] { "Int32", "Int32", "Int32" });
            Add(Token.TRIM, "String", new string[] { "String" });
            Add(Token.TRIM, "String", new string[] { "String", "String" });
            Add(Token.TYPED_BARCODE, "String", new string[] { "String", "Int32" });
            Add(Token.TYPED_BARCODE, "String", new string[] { "String", "Int32", "Int32" });
            Add(Token.TYPED_BARCODE, "String", new string[] { "String", "Int32", "Int32", "String" });
            Add(Token.GETBARCODE_ID, "Int32", new string[] { "Int64" });
            Add(Token.GETBARCODE_ID, "Int32", new string[] { "Int64", "String" });
            Add(Token.UPPER, "String", new string[] { "String" });
            Add(Token.UPPER, "String", new string[] { "String", "Boolean" });
            Add(Token.VAL, "Double", new string[] { "String" });
            Add(Token.VALUEOF, "String", new string[] { "Object" });
            Add(Token.WEEKOFMONTH, "Int32", new string[] { "DateTime" });
            Add(Token.WEEKOFMONTH, "Int32", new string[] { "DateTime", "Int32" });
            Add(Token.WEEKOFYEAR, "Int32", new string[] { "DateTime" });
            Add(Token.YEAR, "Int32", new string[] { "DateTime" });

            Add(Token.DateAdd, "DateTime", new string[] { "DateTime", "Int32", "Int32", "Int32" });
            Add(Token.DateAdd, "DateTime", new string[] { "DateTime", "Int32", "Int32", "Int32", "Int32" });
            Add(Token.DateAdd, "DateTime", new string[] { "DateTime", "Int32", "Int32", "Int32", "Int32", "Int32" });
            Add(Token.DateAdd, "DateTime", new string[] { "DateTime", "Int32", "Int32", "Int32", "Int32", "Int32", "Int32" });
            Add(Token.WeekStartDate, "DateTime", new string[] { "Int32", "Int32" });
            Add(Token.IsLeapYear, "Boolean", new string[] { "Int32" });
            Add(Token.EasterSunday, "DateTime", new string[] { "Int32" });

            Add(Token.WILDCARD_MATCH, "Boolean", new string[] { "String", "String" });
            Add(Token.COMPARE_NO_CASE, "Int32", new string[] { "String", "String" });

            Add(Token.SendBalloon, "Boolean", new string[] { "String", "String", "Int32", "DateTime", "Boolean", "Boolean", "Int64" });
            Add(Token.FormatTbLink, "String", new string[] { "String", "String", "Object" });

            Add(Token.CONVERT, "Object", new string[] { "Object", "String" });
            Add(Token.TYPEOF, "String", new string[] { "Object" });
            Add(Token.ADDRESSOF, "Int64", new string[] { "String", "Object" });
            Add(Token.EXECUTESCRIPT, "Boolean", new string[] { "String" });
        }


        //-----------------------------------------------------------------------------
        public FunctionPrototype GetPrototype(string name, int paramNo = -1)
        {
            foreach (FunctionPrototype fp in prototypes)
                if (string.Compare(fp.FullName, name, StringComparison.OrdinalIgnoreCase) == 0 && (paramNo == -1 || fp.NrParameters == paramNo))
                    return fp;

            //if (fullLoaded)
            //    return null;

            //non dovrebbe mai passarci perch� vengona caricate sempre tutte
            //if (LoadPrototypeFromXml(name))
            //{
            //    foreach (FunctionPrototype fp in prototypes)
            //        if (string.Compare(fp.FullName, name, true, CultureInfo.InvariantCulture) == 0 && (paramNo == -1 || fp.NrParameters == paramNo))
            //            return fp;
            //}
            return null;
        }
    }

}


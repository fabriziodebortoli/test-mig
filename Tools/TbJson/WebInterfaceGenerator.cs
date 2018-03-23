using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Xml;
using System.Diagnostics;
using Microarea.TbJson.Exceptions;
using Microarea.TbJson.Properties;
using Microarea.TbJson.Utils;
using Newtonsoft.Json.Linq;
using SharedCode;
using static Microarea.TbJson.Helpers;
using System.Globalization;

namespace Microarea.TbJson
{
    public class WebInterfaceGenerator
    {
        private readonly StringBuilder toAppendToDefinition = new StringBuilder();
        private readonly StringBuilder toAppendToDeclaration = new StringBuilder();
        private List<string> loggedTokens = new List<string>();
        //private readonly Dictionary<string, List<string>> modelStructure = new Dictionary<string, List<string>>();
        private int indent = 0;
        private int logIndent = 0;
        private HtmlTextWriter htmlWriter = null;
        private readonly JsonParser parser = new JsonParser();
        private readonly Dictionary<string, string> iconsConversionDictionary = new Dictionary<string, string>();
        private Dictionary<string, JToken> constants = new Dictionary<string, JToken>();
        private Dictionary<string, StringBuilder> contextMenus = new Dictionary<string, StringBuilder>();

        private bool verboseOutput = false;

        //-----------------------------------------------------------------------------------------
        public WebInterfaceGenerator(bool verbose)
        {
            LoadIconsConversionTable();
            verboseOutput = verbose;
        }

        //-----------------------------------------------------------------------------------------
        private void LoadIconsConversionTable()
        {
            string xml = Resources.iconsMatch;
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            foreach (XmlNode currentIcon in doc.SelectNodes("//icons/icon"))
            {
                if (currentIcon.Attributes["iconNs"] == null || currentIcon.Attributes["icon"] == null)
                    continue;

                string iconNs = currentIcon.Attributes["iconNs"].Value.ToLower();
                string icon = currentIcon.Attributes["icon"].Value.ToLower();
                if (iconsConversionDictionary.TryGetValue(iconNs, out string value))
                    continue;

                iconsConversionDictionary.Add(iconNs, icon);
            }
        }

        //-----------------------------------------------------------------------------------------
        internal void Generate(string fileOrFolder, string mergedJsonDir, bool onlyMerged)
        {
            if (!verboseOutput)
                Console.Out.WriteLineAsync("Generate Start");

            if (fileOrFolder == ".")
                fileOrFolder = Directory.GetCurrentDirectory();

            FileAttributes attr = File.GetAttributes(fileOrFolder);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                //non ho estensione: � una cartella, quindi applico ricorsivamente a tutti i file rc trovati nella cartella e nelle sottocartelle
                string[] files = Directory.GetFiles(fileOrFolder, "*.tbjson", SearchOption.AllDirectories);
                Array.Sort(files);
                foreach (var file in files)
                {
                    if (file.EndsWith(Constants.mergedExt, StringComparison.InvariantCultureIgnoreCase) ||
                        file.EndsWith(".merged.tbjson", StringComparison.InvariantCultureIgnoreCase))
                        continue;
                    GenerateFromFile(file, mergedJsonDir, onlyMerged);
                }
            }
            else
            {
                GenerateFromFile(fileOrFolder, mergedJsonDir, onlyMerged);
            }

            if (!verboseOutput)
                Console.Out.WriteLineAsync("Generate End");
        }
        //-----------------------------------------------------------------------------
        private void GenerateFromFile(string tbJsonFile, string mergedJsonDir, bool onlyMerged)
        {
            //modelStructure.Clear();
            toAppendToDefinition.Clear();
            toAppendToDeclaration.Clear();
            contextMenus.Clear();

            if (!parser.ExtractPathInfo(tbJsonFile, out string standardFolder, out string app, out string mod, out string container, out string name))
            {
                Console.Out.WriteLineAsync("Cannot extract needed information, invalid file path: " + tbJsonFile);
                return;
            }

            JToken jToken = parser.Parse(standardFolder, tbJsonFile, true);
            if (jToken == null)
            {
                Console.Out.WriteLineAsync("Invalid json file: " + tbJsonFile);
                return;
            }
            JObject jRoot = jToken as JObject;
            if (jRoot == null || (jRoot.GetWndObjType() != WndObjType.Frame && jRoot.GetWndObjType() != WndObjType.Dialog))
                return;
            bool slave = jRoot.GetWndObjType() == WndObjType.Dialog;
            AdjustStructure(jRoot);
            //CheckDuplicates(jRoot);
            string file = null;
            if (!string.IsNullOrEmpty(mergedJsonDir))
            {
                if (!Directory.Exists(mergedJsonDir))
                    Directory.CreateDirectory(mergedJsonDir);
                file = Path.Combine(mergedJsonDir, name.ToLower() + Constants.mergedExt);
                Console.Out.WriteLineAsync("Generating " + file);
                using (StreamWriter sw = new StreamWriter(file, false, Encoding.UTF8))
                {
                    sw.Write(jRoot.ToString());
                }
            }
            if (onlyMerged)
                return;
            string appsPath = Path.Combine(standardFolder, Constants.tsAppsPath);
            string modulePath = Path.Combine(appsPath, app.ToLower(), mod.ToLower());
            if (!Directory.Exists(modulePath))
                Directory.CreateDirectory(modulePath);

            string formsPath = Path.Combine(modulePath, container.ToLower());
            if (!Directory.Exists(formsPath))
                Directory.CreateDirectory(formsPath);

            file = Path.Combine(formsPath, name + ".component.html");
            GenerateLogInfo(appsPath, file);
            using (StreamWriter sw = new StreamWriter(file, false, Encoding.UTF8))
            {
                using (HtmlTextWriter writer = new HtmlTextWriter(sw))
                {
                    this.htmlWriter = writer;
                    this.indent = 0;
                    GenerateHtml(jRoot, WndObjType.Undefined, false);
                }
            }
            if (slave)
            {
                file = Path.Combine(formsPath, name + ".component.ts");

                GenerateLogInfo(appsPath, file);
                using (StreamWriter sw = new StreamWriter(file, false, Encoding.UTF8))
                {
                    string code = Encoding.UTF8.GetString(Properties.Resources.IDDSlave_ts);
                    code = Regex.Replace(code, "@@NAME@@", name);
                    code = Regex.Replace(code, "@@INITCODE@@", GetModelInitCode());
                    code = code.Replace("/*dichiarazione variabili*/", toAppendToDeclaration.ToString());
                    code = code.Replace("/*definizione variabili*/", toAppendToDefinition.ToString());
                    sw.Write(code);
                }
            }
            else
            {
                file = Path.Combine(formsPath, name + ".service.ts");

                string serviceFileRelPath = string.Concat(
                    "./",
                    name,
                    ".service"
                    );

                GenerateLogInfo(appsPath, file);
                using (StreamWriter sw = new StreamWriter(file, false, Encoding.UTF8))
                {
                    string code = Encoding.UTF8.GetString(Properties.Resources.Service_ts);
                    code = Regex.Replace(code, "@@NAME@@", name);

                    string userPath = Path.GetDirectoryName(file.Replace(Constants.applications, Constants.userCode));
                    StringBuilder constructorCode = new StringBuilder();
                    StringBuilder userImport = new StringBuilder();
                    if (Directory.Exists(userPath))
                    {
                        string[] files = Directory.GetFiles(userPath, "*." + name + ".ts");
                        foreach (var userFile in files)
                        {
                            string boFile = Path.GetFileNameWithoutExtension(userFile);
                            string boContent = File.ReadAllText(userFile);
                            //cerco quest ainfo:
                            //class BOName extends BOClient
                            Match m = Regex.Match(boContent, "class\\s+(?<BOName>\\w+)\\s+extends\\s+BOClient");
                            if (!m.Success)
                                continue;
                            string boName = m.Groups["BOName"].Value;
                            string import = string.Concat(
                                    "import { ",
                                    boName,
                                    " } from './../../../../",
                                    Constants.userCode,
                                    '/',
                                    app,
                                    '/',
                                    mod,
                                    '/',
                                    container,
                                    '/',
                                    boFile,
                                    "';\r\n");
                            //import { PYMTTERMClientBO } from './../../../../applications.usercode/erp/paymentterms/paymentterms/PYMTTERMClientBO.IDD_TD_PYMTTERM';

                            userImport.Append(import);
                            //this.boClients.push(new PYMTTERMClientBO(this));
                            constructorCode.Append("\t\t\tthis.boClients.push(new ");
                            constructorCode.Append(boName);
                            constructorCode.Append("(this));\r\n");
                        }
                    }

                    code = Regex.Replace(code, "@@CONSTRUCTORCODE@@", constructorCode.ToString());
                    code = userImport + code;

                    sw.Write(code);
                }

                file = Path.Combine(formsPath, name + ".component.ts");

                GenerateLogInfo(appsPath, file);
                using (StreamWriter sw = new StreamWriter(file, false, Encoding.UTF8))
                {
                    string code = Encoding.UTF8.GetString(Properties.Resources.IDD_ts);
                    code = Regex.Replace(code, "@@NAME@@", name);
                    code = Regex.Replace(code, "@@SERVICEFILE@@", serviceFileRelPath);
                    code = Regex.Replace(code, "@@INITCODE@@", GetModelInitCode());
                    code = code.Replace("/*dichiarazione variabili*/", toAppendToDeclaration.ToString());
                    code = code.Replace("/*definizione variabili*/", toAppendToDefinition.ToString());
                    sw.Write(code);
                }
            }

            UpdateModuleFile(modulePath, mod, container, name);
            UpdateRoutingFile(appsPath, app, mod);
        }
        private void CheckDuplicates(JObject jRoot)
        {
            var tokens = jRoot.SelectTokens("..id");
            var query = tokens.GroupBy(x => x)
              .Where(g => g.Count() > 1 && g.Key.Parent.Parent.GetWndObjType() != WndObjType.ToolbarButton)
              .ToList();

            if (query.Count > 0)
            {
                foreach (var t in query)
                    Console.Out.WriteLineAsync("Duplicate ID: " + t.Key);
            }
        }

        private string GetModelInitCode()
        {
            StringBuilder constructorCode = new StringBuilder();
            /* constructorCode.Append("\t\tthis.bo.appendToModelStructure({");
             var needComma = false;
             foreach (var item in modelStructure)
             {
                 if (needComma)
                     constructorCode.Append(",");
                 else
                     needComma = true;
                 constructorCode.Append("'");
                 constructorCode.Append(item.Key);
                 constructorCode.Append("':[");
                 var needComma1 = false;
                 foreach (var field in item.Value)
                 {
                     if (needComma1)
                         constructorCode.Append(",");
                     else
                         needComma1 = true;
                     constructorCode.Append("'");
                     constructorCode.Append(field);
                     constructorCode.Append("'");
                 }

                 constructorCode.Append("]");
             }
             constructorCode.Append("});\r\n");*/
            return constructorCode.ToString();
        }

        //-----------------------------------------------------------------------------
        private void GenerateLogInfo(string appsPath, string file)
        {
            if (!verboseOutput)
                return;

            List<string> tokens = GetTokens(appsPath, file);
            logIndent = 0;
            for (int i = 0; i < tokens.Count; i++)
            {
                string t = tokens[i];
                if (i >= loggedTokens.Count || !loggedTokens[i].Equals(t, StringComparison.InvariantCultureIgnoreCase))
                    Console.Out.WriteLineAsync(new string(' ', logIndent) + t + '\\');
                logIndent += t.Length + 1;
            }
            loggedTokens = tokens;
        }

        //-----------------------------------------------------------------------------
        private List<string> GetTokens(string appsPath, string file)
        {
            List<string> tokens = new List<string>();
            //tokens.Add(appsPath);
            file = file.Substring(appsPath.Length + 1);
            tokens.AddRange(file.Split(Path.DirectorySeparatorChar));
            return tokens;
        }
        //-----------------------------------------------------------------------------
        private void AdjustItem(JToken jItem)
        {
            JObject jBinding = jItem.GetObject(Constants.binding);
            if (jBinding != null)
            {
                JValue hkl = jBinding[Constants.hotLink] as JValue;
                string hklName = hkl?.ToString();

                if (!string.IsNullOrEmpty(hklName))
                {
                    var jHKL = new JObject { [Constants.name] = hklName };
                    MoveProperty(jBinding, jHKL, Constants.hotLinkNS, Constants.tbNamespace);
                    MoveProperty(jBinding, jHKL, Constants.mustExistData, Constants.mustExistData);
                    MoveProperty(jBinding, jHKL, Constants.enableAddOnFly, Constants.enableAddOnFly);
                    MoveProperty(jBinding, jHKL, Constants.addOnFlyNamespace, Constants.addOnFlyNamespace);
                    MoveProperty(jBinding, jHKL, Constants.enableLink, Constants.enableLink);
                    MoveProperty(jBinding, jHKL, Constants.enableHotLink, Constants.enableHotLink);
                    MoveProperty(jBinding, jHKL, Constants.autoFind, Constants.autoFind);
                    jBinding[Constants.hotLink] = jHKL;
                }
            }
            JArray ar = jItem.GetItems();
            if (ar != null)
            {
                foreach (var child in ar)
                {
                    AdjustItem(child);
                }
            }
        }

        private void MoveProperty(JObject jFrom, JObject jTo, string propNameFrom, string propNameTo)
        {
            JToken jProp = jFrom[propNameFrom];
            if (jProp != null)
            {
                jFrom.Remove(propNameFrom);
                jTo[propNameTo] = jProp;
            }
            else
            {
                jProp = jTo[propNameTo];
            }

            if (jProp != null)
            {
                string text = jProp.ToString();
                if (Helpers.AdjustExpression(ref text))
                    jTo[propNameTo] = text;

            }
        }

        //-----------------------------------------------------------------------------
        private void AdjustStructure(JObject jRoot)
        {
            JArray ar = jRoot.GetItems();
            JObject jViewContainer = CreateDummy(WndObjType.ViewContainer);
            JObject jDockPaneContainer = CreateDummy(WndObjType.DockPaneContainer);
            JObject jFrameContent = CreateDummy(WndObjType.FrameContent);
            jFrameContent.GetItems().Add(jViewContainer);
            jFrameContent.GetItems().Add(jDockPaneContainer);
            if (ar == null)
                return;
            if (jRoot.GetWndObjType() == WndObjType.Dialog)
            {
                for (int i = 0; i < ar.Count; i++)
                {
                    JToken item = ar[i];
                    AdjustItem(item);
                    jViewContainer.GetItems().Add(item);
                }
                ar.RemoveAll();
            }
            else
            {
                for (int i = ar.Count - 1; i >= 0; i--)
                {
                    JToken item = ar[i];
                    AdjustItem(item);
                    if (item.GetWndObjType() == WndObjType.TabbedToolbar)
                    {
                        foreach (JObject toolbar in item.GetItems())
                        {
                            ToolbarInfo.ToolbarToMap(ar, toolbar);
                        }
                        ar.RemoveAt(i);
                    }
                    else if (item.GetWndObjType() == WndObjType.Toolbar)
                    {
                        ToolbarInfo.ToolbarToMap(ar, (JObject)item);
                        ar.RemoveAt(i);
                    }
                    else if (item.GetWndObjType() == WndObjType.View)
                    {
                        jViewContainer.GetItems().Add(item);
                        ar.RemoveAt(i);
                    }
                    else if (item.GetWndObjType() == WndObjType.DockingPane)
                    {
                        jDockPaneContainer.GetItems().Add(item);
                        ar.RemoveAt(i);
                    }
                }
            }
            jRoot.GetItems().Add(jFrameContent);
            jRoot.ReplaceEnums();
            jRoot.SortItems();
        }

        private JObject CreateDummy(WndObjType type)
        {
            JObject jObj = new JObject();
            jObj[Constants.type] = type.ToString();
            jObj[Constants.items] = new JArray();
            return jObj;
        }



        //-----------------------------------------------------------------------------
        private void UpdateModuleFile(string modulePath, string moduleName, string container, string componentName)
        {
            string file = Path.Combine(modulePath, moduleName.ToLower() + ".module.ts");

            try
            {
                using (MyCriticalSession session = new MyCriticalSession(file))
                {
                    string content;
                    if (File.Exists(file))
                        content = File.ReadAllText(file);
                    else
                    {
                        content = Encoding.UTF8.GetString(Properties.Resources.Module_ts) + "\r\nexport class " + moduleName + "Module { };";
                    }
                    string componentClass = componentName + "Component";
                    if (Regex.IsMatch(content, "\\b" + componentClass + "\\b"))
                        return;
                    //import
                    string factoryClass = componentName + "FactoryComponent";
                    Match m = Regex.Match(content, "RouterModule\\s*.\\s*forChild\\s*\\(\\s*\\[");
                    if (!m.Success)
                        throw new InvalidFileException();
                    StringBuilder sb = new StringBuilder();
                    int index = m.Index + m.Length;

                    //importazione componente
                    sb.Append("import { ");
                    sb.Append(componentClass);
                    sb.Append(", ");
                    sb.Append(factoryClass);
                    sb.Append(" } from './");
                    sb.Append(container.ToLower());
                    sb.Append("/");
                    sb.Append(componentName);
                    sb.Append(".component';\r\n");
                    sb.Append(content.Substring(0, index));
                    content = content.Substring(index);

                    //route
                    sb.Append("\r\n");
                    AppendTab(sb, 3);
                    sb.Append("{ path: '");
                    sb.Append(componentName);
                    sb.Append("', component: ");
                    sb.Append(factoryClass);
                    sb.Append(" },");

                    //declaration
                    m = Regex.Match(content, "declarations\\s*:\\s*\\[");
                    if (!m.Success)
                        throw new InvalidFileException();
                    index = m.Index + m.Length;
                    sb.Append(content.Substring(0, index));
                    content = content.Substring(index);

                    sb.Append("\r\n");
                    AppendTab(sb, 3);
                    sb.Append(componentClass);
                    sb.Append(", ");
                    sb.Append(factoryClass);
                    sb.Append(",");

                    //export
                    m = Regex.Match(content, "exports\\s*:\\s*\\[");
                    if (!m.Success)
                        throw new InvalidFileException();
                    index = m.Index + m.Length;
                    sb.Append(content.Substring(0, index));
                    content = content.Substring(index);
                    sb.Append("\r\n");
                    AppendTab(sb, 3);
                    sb.Append(factoryClass);
                    sb.Append(",");

                    //entryComponent
                    m = Regex.Match(content, "entryComponents\\s*:\\s*\\[");
                    if (!m.Success)
                        throw new InvalidFileException();
                    index = m.Index + m.Length;
                    sb.Append(content.Substring(0, index));
                    content = content.Substring(index);

                    sb.Append("\r\n");
                    AppendTab(sb, 3);
                    sb.Append(componentClass);
                    sb.Append(",");

                    sb.Append(content);
                    using (StreamWriter sw = new StreamWriter(file, false, Encoding.UTF8))
                    {
                        sw.Write(sb.ToString());
                    }
                }
            }
            catch (InvalidFileException)
            {
                File.Delete(file);
                UpdateModuleFile(modulePath, moduleName, container, componentName);
            }
        }
        public void ResetRoutes(string standardFolder)
        {
            if (!verboseOutput)
                Console.Out.WriteLineAsync("Reset Routes");
            string appsPath = Path.Combine(standardFolder, Constants.tsAppsPath);
            if (Directory.Exists(appsPath))
                Directory.Delete(appsPath, true);
            Directory.CreateDirectory(appsPath);
            string file = Path.Combine(appsPath, "app.routing.ts");
            using (MyCriticalSession session = new MyCriticalSession(file))
            {
                string content = Encoding.UTF8.GetString(Properties.Resources.AppRouting_ts);
                using (StreamWriter sw = new StreamWriter(file, false, Encoding.UTF8))
                {
                    sw.Write(content);
                }
            }
        }
        //-----------------------------------------------------------------------------
        private void UpdateRoutingFile(string appsPath, string app, string mod)
        {
            string file = Path.Combine(appsPath, "app.routing.ts");
            try
            {
                using (MyCriticalSession session = new MyCriticalSession(file))
                {
                    string content;
                    if (File.Exists(file))
                        content = File.ReadAllText(file);
                    else
                    {
                        content = Encoding.UTF8.GetString(Properties.Resources.AppRouting_ts);
                    }
                    string route = string.Concat(app, "/", mod);
                    if (Regex.IsMatch(content, "\\b" + route.ToLower() + "\\b"))
                        return;//esiste gi�

                    //string pattern = "RouterModule\\s*.\\s*forRoot\\s*\\=\\s*\\[";

                    string pattern = "export const appRoutes \\= \\[";

                    Match m = Regex.Match(content, pattern);
                    if (!m.Success)
                        throw new InvalidFileException();
                    StringBuilder sb = new StringBuilder();
                    int index = m.Index + m.Length;

                    //route
                    sb.Append(content.Substring(0, index));
                    content = content.Substring(index);

                    //route
                    sb.Append("\r\n");
                    AppendTab(sb, 1);
                    sb.Append("{ path: '");
                    sb.Append(route.ToLower());
                    sb.Append("', loadChildren: 'app/applications/");
                    sb.Append(route.ToLower());
                    sb.Append("/");
                    sb.Append(mod.ToLower());
                    sb.Append(".module#");
                    sb.Append(mod);
                    sb.Append("Module' },");

                    //parte rimanente del file originario
                    sb.Append(content);

                    using (StreamWriter sw = new StreamWriter(file, false, Encoding.UTF8))
                    {
                        sw.Write(sb.ToString());
                    }
                }
            }
            catch (InvalidFileException)
            {
                File.Delete(file);
                UpdateRoutingFile(appsPath, app, mod);
            }
        }

        //-----------------------------------------------------------------------------
        private void AppendTab(StringBuilder sb, int tabs)
        {
            for (int i = 0; i < tabs; i++)
                sb.Append(' ', 4);
        }

        //-----------------------------------------------------------------------------
        private void GenerateHtml(JObject jObj, WndObjType parentType, bool insideRowView)
        {
            WndObjType type = jObj.GetWndObjType();
            switch (type)
            {
                case WndObjType.Constants:
                    {
                        foreach (JObject c in jObj.GetItems())
                            constants[c.GetFlatString(Constants.name)] = c[Constants.value];
                        break;
                    }
                case WndObjType.Frame:
                    {
                        using (OpenCloseTagWriter w = new OpenCloseTagWriter(Constants.tbFrame, this, false))
                        {
                            htmlWriter.Write(" tb");
                            htmlWriter.Write(jObj.GetViewCategory().ToString());
                            htmlWriter.Write(" ");

                            WriteActivationAttribute(jObj);
                            w.CloseBeginTag();

                            GenerateHtmlChildren(jObj, type, insideRowView);
                        }

                        break;
                    }

                case WndObjType.Dialog:
                    {
                        using (OpenCloseTagWriter w = new OpenCloseTagWriter(Constants.tbFrame, this, false))
                        {
                            htmlWriter.Write(" tb");
                            htmlWriter.Write(jObj.GetViewCategory().ToString());
                            htmlWriter.Write(" ");

                            WriteActivationAttribute(jObj);
                            w.CloseBeginTag();

                            GenerateHtmlChildren(jObj, type, insideRowView);
                        }

                        break;
                    }

                case WndObjType.FrameContent:
                    {
                        using (OpenCloseTagWriter w = new OpenCloseTagWriter(Constants.tbFrameContent, this, false))
                        {
                            w.CloseBeginTag();
                            GenerateHtmlChildren(jObj, type, insideRowView);
                        }
                        break;
                    }

                case WndObjType.ViewContainer:
                    {
                        using (OpenCloseTagWriter w = new OpenCloseTagWriter(Constants.tbViewContainer, this, false))
                        {
                            w.CloseBeginTag();
                            GenerateHtmlChildren(jObj, type, insideRowView);
                        }
                        break;
                    }

                case WndObjType.DockPaneContainer:
                    {
                        //using (OpenCloseTagWriter w = new OpenCloseTagWriter(Constants.tbDockPaneContainer, this, false))
                        //{
                        //    w.CloseBeginTag();
                        //    GenerateHtmlChildren(jObj, type, insideRowView);
                        //}
                        break;
                    }



                case WndObjType.View:
                    {
                        using (OpenCloseTagWriter w = new OpenCloseTagWriter(Constants.tbView, this, false))
                        {
                            WriteActivationAttribute(jObj);
                            w.CloseBeginTag();
                            GenerateHtmlChildren(jObj, type, insideRowView);
                        }

                        break;
                    }

                case WndObjType.DockingPane:
                    {
                        //using (OpenCloseTagWriter w = new OpenCloseTagWriter(Constants.tbDockPane, this, false))
                        //{
                        //    string activation = jObj.GetFlatString(Constants.activation);
                        //    string id = jObj.GetId();
                        //    if (!string.IsNullOrEmpty(activation) && !string.IsNullOrEmpty(id))
                        //        htmlWriter.WriteAttribute("[activated]", "eventData?.activation?." + id);

                        //    htmlWriter.WriteAttribute("[title]", jObj.GetLocalizableString(Constants.text));

                        //    string iconType = jObj.GetFlatString(Constants.iconType);
                        //    iconType = string.IsNullOrEmpty(iconType) ? "M4" : iconType;
                        //    htmlWriter.WriteAttribute(Constants.iconType, iconType);

                        //    string icon = jObj.GetFlatString(Constants.icon);
                        //    icon = string.IsNullOrEmpty(icon) ? "tb-openpane" : icon;
                        //    htmlWriter.WriteAttribute(Constants.icon, icon);

                        //    w.CloseBeginTag();

                        //    // wrappo tutto il conenuto del tilegroup in un ng-template
                        //    using (var w2 = new OpenCloseTagWriter(Constants.ngTemplate, this, false))
                        //    {
                        //        w2.CloseBeginTag();

                        //        GenerateHtmlChildren(jObj, type, insideRowView);
                        //    }

                        //}

                        break;
                    }
                case WndObjType.Toolbar:
                    {
                        string tag = jObj.GetFlatString(Constants.ngTag);
                        string sClass = jObj.GetFlatString(Constants.ngClass);
                        if (!string.IsNullOrEmpty(tag))
                        {
                            if (tag == Constants.tbToolbarTop)
                                htmlWriter.Write("  <ng-container #radar></ng-container>\r\n");

                            //se ho una div ma nessuna classe, è inutile metterla
                            if (tag == Constants.div && string.IsNullOrEmpty(sClass))
                            {
                                GenerateHtmlChildren(jObj, type, insideRowView);
                            }
                            else
                            {
                                using (OpenCloseTagWriter w = new OpenCloseTagWriter(tag, this, false))
                                {
                                    if (!string.IsNullOrEmpty(sClass))
                                    {
                                        htmlWriter.WriteAttribute(Constants.sClass, sClass);
                                        WriteHideAttributeByCategory(sClass);
                                    }
                                    w.CloseBeginTag();
                                    GenerateHtmlChildren(jObj, type, insideRowView);
                                }
                            }
                        }


                        JObject obj = jObj?.GetParentItem()?.GetParentItem();
                        if (obj != null && obj.GetWndObjType() == WndObjType.BodyEdit)
                        {
                            using (OpenCloseTagWriter w = new OpenCloseTagWriter(Constants.tbBodyEditToolbar, this, false))
                            {
                                w.CloseBeginTag();

                                GenerateHtmlChildren(jObj, type, insideRowView);
                            }
                        }

                        JObject jParentObject = jObj?.GetParentItem();
                        if (jParentObject != null && jParentObject.GetWndObjType() == WndObjType.BodyEdit)
                        {
                            using (OpenCloseTagWriter w = new OpenCloseTagWriter(Constants.tbBodyEditToolbar, this, false))
                            {
                                w.CloseBeginTag();

                                GenerateHtmlChildren(jObj, type, insideRowView);
                            }
                        }

                        break;
                    }
                case WndObjType.ToolbarButton:
                    {
                        bool? isSeparator = jObj[Constants.isSeparator]?.Value<bool>();

                        if (!(isSeparator == true))
                        {
                            /*	using (OpenCloseTagWriter w = new OpenCloseTagWriter(Constants.tbToolbarSeparator, this, true, true))
                            {

                            }
                        else*/


                            using (OpenCloseTagWriter w = new OpenCloseTagWriter(jObj.GetToolbarButtonTag(), this, true))
                            {
                                WriteActivationAttribute(jObj);
                                AddIconAttribute(jObj);
                                WriteButtonInfo(jObj);

                                string caption = jObj.GetLocalizableString(Constants.text);
                                if (!string.IsNullOrEmpty(caption))
                                    htmlWriter.WriteAttribute(Square(Constants.caption), caption);
                                string cmpId = jObj.GetId();
                                if (!string.IsNullOrEmpty(cmpId))
                                    htmlWriter.WriteAttribute(Constants.cmpId, cmpId);
                                var cmd = jObj.GetClick();
                                if (!string.IsNullOrEmpty(cmd))
                                    htmlWriter.WriteAttribute(Square(Constants.buttonClick), cmd);
                                w.CloseBeginTag();
                                GenerateHtmlChildren(jObj, type, false);
                            }
                        }

                        break;
                    }

                case WndObjType.TileManager:
                    {
                        using (OpenCloseTagWriter w = new OpenCloseTagWriter(Constants.tbTileManager, this, false))
                        {
                            WriteActivationAttribute(jObj);
                            w.CloseBeginTag();
                            GenerateHtmlChildren(jObj, type, insideRowView);
                        }

                        break;
                    }

                case WndObjType.BodyEdit:
                    {
                        using (OpenCloseTagWriter w = new OpenCloseTagWriter(Constants.tbBodyEdit, this, false))
                        {
                            WebControl wc = GetWebControl(jObj);

                            WriteControlAttributes(jObj, wc, insideRowView);
                            WriteAttribute(jObj, Constants.height, Constants.height);

                            JObject jBinding = jObj[Constants.binding] as JObject;
                            if (jBinding != null)
                            {
                                string ds = jBinding.GetDataSource();
                                if (!string.IsNullOrEmpty(ds))
                                {
                                    string alias = CheckAlias(ds, "");
                                    if (string.IsNullOrEmpty(alias))
                                        htmlWriter.WriteAttribute(Constants.bodyEditName, ds);
                                    else
                                        htmlWriter.WriteAttribute(Square(Constants.bodyEditName), alias);
                                }
                            }


                            jObj.GetString(Constants.id, out string cmpId);

                            WriteActivationAttribute(jObj);
                            w.CloseBeginTag();

                            GenerateHtmlChildren(jObj, type, insideRowView);

                            JObject jRowViewForm = jObj[Constants.rowViewForm] as JObject;

                            if (jRowViewForm != null)
                            {
                                using (OpenCloseTagWriter wRow = new OpenCloseTagWriter(Constants.tbRowViewForm, this, false))
                                {
                                    wRow.CloseBeginTag();

                                    using (OpenCloseTagWriter wTemplate = new OpenCloseTagWriter(Constants.ngTemplate, this, false))
                                    {
                                        htmlWriter.WriteAttribute("let-currentRow", "currentRow");
                                        wTemplate.CloseBeginTag();

                                        GenerateHtmlChildren(jRowViewForm, parentType, true);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case WndObjType.ColTitle:
                    {
                        WebControl wCol = GetWebControl(jObj);
                        if (jObj == null)
                            break;

                        string bodyEditColumnType = string.IsNullOrEmpty(wCol.ColumnName) ? Constants.tbBodyEditColumn : wCol.ColumnName;
                        using (var w = new OpenCloseTagWriter(bodyEditColumnType, this, false))
                        {
                            WriteActivationAttribute(jObj);
                            WriteColumnAttributes(jObj, wCol, true);

                            WriteAttribute(jObj, Constants.rows, Constants.rows);
                            WriteAttribute(jObj, Constants.chars, Constants.chars);
                            WriteAttribute(jObj, Constants.width, Constants.width);

                            w.CloseBeginTag();

                            using (var w2 = new OpenCloseTagWriter(Constants.ngTemplate, this, false))
                            {
                                htmlWriter.WriteAttribute("let-columnModel", "columnModel");
                                w2.CloseBeginTag();

                                using (OpenCloseTagWriter w1 = new OpenCloseTagWriter(wCol.Name, this, true))
                                {
                                    WriteAttribute(jObj, Constants.rows, Constants.rows);
                                    WriteAttribute(jObj, Constants.chars, Constants.chars);

                                    WriteColumnBindingAttributes(jObj, true);

                                    var propagateSelectionChange = jObj[Constants.propagateSelectionChange];
                                    if (propagateSelectionChange != null)
                                    {
                                        htmlWriter.WriteAttribute(Constants.propagateSelectionChange, propagateSelectionChange.ToString());
                                    }
                                    w1.CloseBeginTag();
                                }
                            }
                        }
                        break;
                    }
                case WndObjType.TileGroup:
                    {
                        if (parentType == WndObjType.TileManager)
                        {
                            using (var tmTab = new OpenCloseTagWriter(Constants.tbTileManagerTab, this, false))
                            {
                                WriteActivationAttribute(jObj);
                                string title = jObj.GetLocalizableString(Constants.text);
                                if (!string.IsNullOrEmpty(title))
                                {
                                    //rimuovo & acceleratore c++ dal text  //TODOLUCA
                                    htmlWriter.WriteAttribute(Square(Constants.title), title);
                                }

                                AddIconAttribute(jObj);

                                tmTab.CloseBeginTag();

                                // wrappo tutto il conenuto del tilegroup in un ng-template
                                using (var w2 = new OpenCloseTagWriter(Constants.ngTemplate, this, false))
                                {
                                    w2.CloseBeginTag();

                                    GenerateTileGroup(jObj, Constants.tbTileGroup, type, insideRowView);
                                }

                            }

                            break;
                        }

                        string tag = getTileGroupType(jObj);
                        GenerateTileGroup(jObj, tag, type, insideRowView);

                        break;
                    }

                case WndObjType.Tile:
                    {
                        string title = jObj.GetLocalizableString(Constants.text);

                        string tag = Constants.tbTile;

                        if (!string.IsNullOrEmpty(title) || jObj.GetParentItem().GetWndObjType() == WndObjType.TileGroup)
                            tag = Constants.tbPanel;

                        if (jObj.GetParentItem().GetDialogStyle() == TileDialogStyle.Header || jObj.GetParentItem().GetWndObjType() == WndObjType.TilePanel)
                            tag = Constants.tbTile;

                        if (jObj.GetDialogStyle() == TileDialogStyle.Filter)
                            tag = Constants.tbFilter;

                        using (OpenCloseTagWriter w = new OpenCloseTagWriter(tag, this, false))
                        {
                            if (!string.IsNullOrEmpty(title) && tag != Constants.tbTile)
                                htmlWriter.WriteAttribute(Square(Constants.title), title);

                            htmlWriter.Write(" tbTile");
                            htmlWriter.Write(jObj.GetTileDialogSize().ToString());
                            htmlWriter.Write(" ");

                            if (tag.Equals(Constants.tbPanel))
                            {
                                WriteAttribute(jObj, Constants.collapsible, Constants.isCollapsible);
                                WriteAttribute(jObj, Constants.collapsed, Constants.isCollapsed);
                            }

                            WriteActivationAttribute(jObj);

                            w.CloseBeginTag();
                            GenerateTileChildren(jObj, type, insideRowView);

                        }

                        break;
                    }

                case WndObjType.TilePanel:
                    {
                        using (OpenCloseTagWriter w = new OpenCloseTagWriter(Constants.tbTilePanel, this, false))
                        {
                            string title = jObj.GetLocalizableString(Constants.text);
                            if (!string.IsNullOrEmpty(title))
                                htmlWriter.WriteAttribute(Square(Constants.title), title);

                            WriteAttribute(jObj, Constants.collapsible, Constants.isCollapsible);
                            WriteAttribute(jObj, Constants.collapsed, Constants.isCollapsed);
                            WriteAttribute(jObj, Constants.showAsTile, Constants.showAsTile);

                            WriteActivationAttribute(jObj);

                            if (!string.IsNullOrEmpty(jObj.GetParentItem().GetFlatString("hasPinnableTiles")))
                                htmlWriter.WriteAttribute(Constants.hasPinnableTiles, "true");

                            w.CloseBeginTag();
                            GenerateHtmlChildren(jObj, type, insideRowView);
                        }

                        break;
                    }

                case WndObjType.LayoutContainer:
                    {
                        using (OpenCloseTagWriter w = new OpenCloseTagWriter(Constants.tbLayoutContainer, this, false))
                        {
                            string title = jObj.GetLocalizableString(Constants.text);

                            /*if (!string.IsNullOrEmpty(title))
                                htmlWriter.WriteAttribute(Square(Constants.title), title);

                            WriteAttribute(jObj, Constants.collapsible, Constants.isCollapsible);
                            WriteAttribute(jObj, Constants.collapsed, Constants.isCollapsed);*/

                            htmlWriter.Write(" tbLayoutType");
                            htmlWriter.Write(jObj.GetLayoutType().ToString());
                            htmlWriter.Write(" ");

                            WriteActivationAttribute(jObj);

                            w.CloseBeginTag();
                            GenerateHtmlChildren(jObj, type, insideRowView);
                        }

                        break;
                    }
                case WndObjType.Edit:
                    {
                        WebControl wc = GetWebControl(jObj);
                        using (OpenCloseTagWriter w = new OpenCloseTagWriter(wc.Name, this, true))
                        {
                            WriteActivationAttribute(jObj);
                            WriteControlAttributes(jObj, wc, insideRowView);
                            w.CloseBeginTag();
                        }
                        break;
                    }
                case WndObjType.Label:
                    {
                        WebControl wc = GetWebControl(jObj);
                        using (OpenCloseTagWriter w = new OpenCloseTagWriter(wc.Name, this, true))
                        {
                            WriteActivationAttribute(jObj);
                            WriteControlAttributes(jObj, wc, insideRowView);
                            string caption = jObj.GetLocalizableString(Constants.text);
                            if (!string.IsNullOrEmpty(caption))
                                htmlWriter.WriteAttribute(Square(Constants.caption), caption);
                            w.CloseBeginTag();
                        }
                        break;
                    }
                case WndObjType.Combo:
                    {
                        WebControl wc = GetWebControl(jObj);
                        using (var w = new OpenCloseTagWriter(wc.Name, this, true))
                        {
                            WriteActivationAttribute(jObj);

                            WriteControlAttributes(jObj, wc, insideRowView);
                            var propagateSelectionChange = jObj[Constants.propagateSelectionChange];
                            if (propagateSelectionChange != null)
                            {
                                htmlWriter.WriteAttribute(Constants.propagateSelectionChange, propagateSelectionChange.ToString());
                            }

                            w.CloseBeginTag();
                        }
                        break;
                    }
                case WndObjType.Radio:
                    {
                        WebControl wc = GetWebControl(jObj);
                        using (OpenCloseTagWriter w = new OpenCloseTagWriter(wc.Name, this, true))
                        {
                            WriteActivationAttribute(jObj);
                            WriteControlAttributes(jObj, wc, insideRowView);

                            string text = jObj.GetLocalizableString(Constants.text);
                            if (!string.IsNullOrEmpty(text))
                                htmlWriter.WriteAttribute(Square(Constants.caption), text);

                            w.CloseBeginTag();
                        }
                        break;
                    }
                case WndObjType.Check:
                    {
                        WebControl wc = GetWebControl(jObj);
                        using (OpenCloseTagWriter w = new OpenCloseTagWriter(wc.Name, this, true))
                        {
                            WriteActivationAttribute(jObj);

                            WriteControlAttributes(jObj, wc, insideRowView);

                            string caption = jObj.GetLocalizableString(Constants.text);
                            if (!String.IsNullOrEmpty(caption))
                                htmlWriter.WriteAttribute(Square(Constants.caption), caption);

                            w.CloseBeginTag();
                        }
                        break;
                    }
                case WndObjType.Button:
                    {
                        WebControl wc = GetWebControl(jObj);
                        using (OpenCloseTagWriter w = new OpenCloseTagWriter(wc.Name, this, true))
                        {
                            WriteActivationAttribute(jObj);
                            WriteControlAttributes(jObj, wc, insideRowView);
                            string caption = jObj.GetLocalizableString(Constants.text);
                            if (!string.IsNullOrEmpty(caption))
                                htmlWriter.WriteAttribute(Square(Constants.caption), caption);
                            w.CloseBeginTag();
                        }
                        break;
                    }

                case WndObjType.TreeAdv:
                    {
                        WebControl wc = GetWebControl(jObj);
                        using (OpenCloseTagWriter w = new OpenCloseTagWriter(wc.Name, this, true))
                        {
                            WriteActivationAttribute(jObj);
                            WriteControlAttributes(jObj, wc, insideRowView);
                            string caption = jObj.GetLocalizableString(Constants.text);
                            if (!string.IsNullOrEmpty(caption))
                                htmlWriter.WriteAttribute(Square(Constants.caption), caption);

                            WriteTreeAttributes(jObj, wc);

                            w.CloseBeginTag();
                        }
                        break;
                    }

                case WndObjType.StatusTilePanel:
                    {
                        using (OpenCloseTagWriter w = new OpenCloseTagWriter(Constants.tbStatusTilePanel, this, false))
                        {
                            WriteActivationAttribute(jObj);
                            w.CloseBeginTag();

                            GenerateHtmlChildren(jObj, type, insideRowView);
                        }
                        break;
                    }

                case WndObjType.StatusTile:
                    {
                        using (OpenCloseTagWriter w = new OpenCloseTagWriter(Constants.tbStatusTile, this, false))
                        {
                            WriteActivationAttribute(jObj);
                            w.CloseBeginTag();

                            GenerateHtmlChildren(jObj, type, insideRowView);
                        }
                        break;
                    }

                case WndObjType.PropertyGrid:
                    {
                        var wc = GetWebControl(jObj);
                        using (var w = new OpenCloseTagWriter(Constants.tbPropertyGrid, this, false))
                        {
                            WriteActivationAttribute(jObj);
                            WriteControlAttributes(jObj, wc, insideRowView);
                            w.CloseBeginTag();

                            GenerateHtmlChildren(jObj, type, insideRowView);
                        }
                        break;
                    }

                case WndObjType.List:
                    {
                        var wc = GetWebControl(jObj);
                        using (OpenCloseTagWriter w = new OpenCloseTagWriter(wc.Name, this, true))
                        {
                            WriteActivationAttribute(jObj);
                            WriteControlAttributes(jObj, wc, insideRowView);
                            w.CloseBeginTag();
                        }
                        break;
                    }

                case WndObjType.PropertyGridItem:
                    {
                        var wc = GetWebControl(jObj);

                        //la propriet� name non deve finire nel testo
                        var text = jObj.GetLocalizableString(Constants.text);
                        var hint = jObj.GetLocalizableString(Constants.hint);
                        JArray jItems = jObj.GetItems();
                        if (jItems != null && jItems.Count > 0)
                        {
                            using (var w = new OpenCloseTagWriter(Constants.tbPropertyGridItemGroup, this, false))
                            {
                                WriteControlAttributes(jObj, wc, insideRowView);
                                if (!string.IsNullOrEmpty(text))
                                    htmlWriter.WriteAttribute("[text]", text);
                                if (!string.IsNullOrEmpty(hint))
                                    htmlWriter.WriteAttribute("[hint]", hint);
                                w.CloseBeginTag();

                                GenerateHtmlChildren(jObj, type, insideRowView);
                            }
                        }
                        else
                        {
                            using (var w = new OpenCloseTagWriter(Constants.tbPropertyGridItem, this, false))
                            {
                                if (!string.IsNullOrEmpty(text))
                                    htmlWriter.WriteAttribute("[text]", text);
                                if (!string.IsNullOrEmpty(hint))
                                    htmlWriter.WriteAttribute("[hint]", hint);
                                w.CloseBeginTag();

                                using (var writer = new OpenCloseTagWriter(wc.Name, this, true))
                                {
                                    WriteControlAttributes(jObj, wc, insideRowView);
                                    writer.CloseBeginTag();
                                }
                            }
                        }

                        //WriteTextHint(jObj);
                        break;
                    }

                default:
                    {
                        GenerateHtmlChildren(jObj, type, insideRowView);
                        break;
                    }
            }
        }

        
        private string CheckAlias(string table, string field)
        {
            if (string.IsNullOrEmpty(table))
            {
                if (string.IsNullOrEmpty(field))
                    return "";
                if (field[0] == '@')
                    return string.Concat("alias('", field, "')");
                else
                    return "";
            }
            else
            {
                if (table[0] == '@')
                    return string.Concat("alias('", table, "', '", field, "')");
                else
                    return "";
            }
        }
        private string GetBodyEditColumnType(WebControl wCol)
        {

            //ColumnControls

            switch (wCol.Name)
            {
                case "tb-bool-edit":
                    return Constants.tbBodyEditColumn;
                case "tb-enum-combo":
                    return Constants.tbBodyEditColumn;
                default:
                    return Constants.tbBodyEditColumn;
            }

        }

        private void GenerateTileGroup(JObject jObj, String tag, WndObjType type, bool insideRowView)
        {
            using (var w = new OpenCloseTagWriter(tag, this, false))
            {
                htmlWriter.Write(" tbLayoutType");
                htmlWriter.Write(jObj.GetLayoutType().ToString());
                htmlWriter.Write(" ");

                if (jObj.GetDialogStyle() != TileDialogStyle.None)
                {
                    htmlWriter.Write("tds");
                    htmlWriter.Write(jObj.GetDialogStyle().ToString());
                    htmlWriter.Write(" ");
                }

                WriteActivationAttribute(jObj);

                w.CloseBeginTag();

                GenerateHtmlChildren(jObj, type, insideRowView);
            }
        }

        private string getTileGroupType(JObject jObj)
        {
            if (jObj.GetDialogStyle() == TileDialogStyle.Header)
                return Constants.tbHeader;

            return Constants.tbTileGroup;
        }

        void WriteAttribute(JObject jObj, string jsonPropName, string tsPropName)
        {

            string val;
            ValueType t = jObj.GetString(jsonPropName, out val);
            switch (t)
            {
                case ValueType.NOT_FOUND:
                    return;
                case ValueType.PLAIN:
                case ValueType.EXPRESSION:
                    tsPropName = Square(tsPropName);
                    break;
                case ValueType.CONSTANT:
                    tsPropName = Square(tsPropName);
                    JToken jVal;
                    if (constants.TryGetValue(val, out jVal))
                    {
                        val = jVal.GetSafeJsonString();
                    }
                    else
                    {
                        Console.Out.WriteLineAsync("Constant not found: " + val);
                    }
                    break;
                default:
                    break;
            }
            htmlWriter.WriteAttribute(tsPropName, val);
        }

        //-----------------------------------------------------------------------------------------
        private void AddIconAttribute(JObject jObj)
        {
            string icon = jObj.GetFlatString(Constants.icon);
            if (!string.IsNullOrEmpty(icon))
            {
                string tempIcon = icon.ToLower();
                if (tempIcon.StartsWith("image."))
                    tempIcon = tempIcon.Substring("image.".Length, icon.Length - "image.".Length);

                if (iconsConversionDictionary.TryGetValue(tempIcon, out string convertedValue))
                    htmlWriter.WriteAttribute(Constants.icon, convertedValue);
                else
                {
                    if (icon.IndexOf("{{") >= 0)
                    {
                        icon = icon.ResolveInterplation();
                        htmlWriter.WriteAttribute(Square(Constants.icon), icon);
                    }
                    else
                    {
                        Console.Out.WriteLineAsync(string.Format("Warning: icon {0} not found in iconfont", icon));
                        htmlWriter.WriteAttribute(Constants.icon, icon);
                    }
                }
            }
            else
            {
                htmlWriter.WriteAttribute(Constants.icon, "tb-productinfo");
            }
        }

        //-----------------------------------------------------------------------------------------

        private void WriteActivationAttribute(JObject jObj)
        {
            string activation = jObj.GetFlatString(Constants.activation);
            if (!string.IsNullOrEmpty(activation))
                htmlWriter.WriteAttribute("*ngIf", "eventData?.activation?." + GetSafeActivationString(activation));
        }

        //-----------------------------------------------------------------------------------------
        private string GetSafeActivationString(string activation)
        {
            activation = activation.Replace("&&", "And")
                .Replace("&", "And")
                .Replace("<", "_")
                .Replace(">", "_")
                .Replace("'", "_")
                .Replace("(", "_")
                .Replace(")", "_")
                .Replace(" ", "")
                .Replace("!", "Not")
                .Replace("||", "Or")
                .Replace("|", "Or")
                .Replace(".", "_")
                .Replace("\"", "_");
            return "_" + activation;
        }

        //-----------------------------------------------------------------------------------------
        private void WriteHideAttributeByCategory(string sClass)
        {
            if (!sClass.Contains(' ')) return;
            var cat = sClass.Substring(sClass.LastIndexOf(' ') + 1);
            if (string.IsNullOrWhiteSpace(cat)) return;
            htmlWriter.WriteAttribute(Constants.ngIf, "!hide" +
                CultureInfo.InvariantCulture.TextInfo.ToTitleCase(cat.ToLower()) +
                "Toolbar");
        }

        //-----------------------------------------------------------------------------------------
        private void WriteColumnAttributes(JObject jObj, WebControl wc, bool writeHtml)
        {
            JObject jBinding = jObj[Constants.binding] as JObject;
            if (jBinding == null)
                return;

            string ds = jBinding.GetDataSource();
            if (string.IsNullOrEmpty(ds))
                return;

            ds = ResolveGetParentNameFunction(ds, jObj);

            int idx = ds.IndexOf('.');
            string table = "", field = "";
            if (idx == -1)
            {
                field = ds;
            }
            else
            {
                table = ds.Substring(0, idx);
                field = ds.Substring(idx + 1);
            }

            JObject jParentObject = null;
            //se l'owner � nullo...ma si tratta di un binding di uno slave buffered, risalgo la catena di parentela per trovare il bodyedit
            if (string.IsNullOrEmpty(table) && (jParentObject = jObj.GetParentItem()) != null)
            {
                if (jParentObject != null && jParentObject.GetWndObjType() == WndObjType.BodyEdit)
                {
                    //una volte trovato il bodyedit, ne prendo il binding source e lo uso come owner implicito
                    var jBodyEditBinding = jParentObject[Constants.binding] as JObject;
                    if (jBodyEditBinding != null)
                    {
                        table = jBodyEditBinding.GetDataSource();
                    }
                }
            }

            //RegisterModelField(owner, field);

            foreach (var arg in wc.Args)
            {
                if (string.IsNullOrEmpty(arg.Value))
                {
                    htmlWriter.Write(' ');
                    htmlWriter.Write(arg.Key);
                }
                else
                {
                    String value = arg.Value;
                    if (value.StartsWith("[", StringComparison.CurrentCulture) && value.EndsWith("]", StringComparison.CurrentCulture))
                    {
                        value = jObj.GetFlatString(value.Substring(1, value.Length - 2));
                    }

                    htmlWriter.WriteAttribute(arg.Key, value);
                }
            }

            string caption = jObj.GetLocalizableString(Constants.controlCaption);
            if (!string.IsNullOrEmpty(caption))
                htmlWriter.WriteAttribute(Square(Constants.caption), caption);

            WriteAttribute(jObj, Constants.width, Constants.width);
            string alias = CheckAlias(table, field);
            if (string.IsNullOrEmpty(alias))
                htmlWriter.WriteAttribute(Constants.columnName, field);
            else
                htmlWriter.WriteAttribute(Square(Constants.columnName), alias);
            string title = jObj.GetLocalizableString(Constants.text);
            if (!string.IsNullOrEmpty(title))
                htmlWriter.WriteAttribute(Square(Constants.title), title);
        }

        //-----------------------------------------------------------------------------------------
        private void WriteColumnBindingAttributes(JObject jObj, bool writeHtml)
        {
            JObject jBinding = jObj[Constants.binding] as JObject;
            if (jBinding == null)
                return;

            var cmpId = getControlId(jObj);
            if (!string.IsNullOrEmpty(cmpId))
                htmlWriter.WriteAttribute(Constants.cmpId, cmpId);

            //lo scrivo nell'html in tutti i casi, ma non quando sto generando il binding dello slavebuffered
            if (writeHtml)
            {
                htmlWriter.WriteAttribute("[model]", "columnModel");
                JObject jHKL = jBinding.GetObject(Constants.hotLink);
                if (jHKL != null)
                {
                    string hkl = jHKL.ToString();
                    hkl = hkl.Replace("\r\n", "").Replace("\"", "'").Replace(" ", "");

                    if (hkl.IndexOf(Constants.getParentNameFunction) != -1)
                    {
                        string hklExpr = jHKL.GetValue("name").ToString().Replace(" ", "");
                        string hklValue = ResolveGetParentNameFunction(hklExpr, jObj);
                        hkl = hkl.Replace(hklExpr, hklValue);
                    }
                    htmlWriter.WriteAttribute("[hotLink]", hkl.ResolveInterplation());
                }

                var jItem = jObj[Constants.itemSource] as JObject;
                if (jItem != null)
                {
                    var strItemSource = $"{cmpId}_{Constants.itemSource}";

                    htmlWriter.Write($" [{Constants.itemSource}]=\"{strItemSource}\"");

                    toAppendToDeclaration.AppendIfNotExist($"public {strItemSource}: any;\r\n");
                    toAppendToDefinition.AppendIfNotExist($"this.{strItemSource} = {jItem}; \r\n");
                }
            }
        }
        //-----------------------------------------------------------------------------------------
        private string AdjustModelExpression(string model)
        {
            return model.Replace(".", "?.");
        }
        //-----------------------------------------------------------------------------------------
        private void WriteBindingAttributes(JObject jObj, bool writeHtml, bool insideRowView)
        {
            JObject jBinding = jObj[Constants.binding] as JObject;
            if (jBinding == null)
                return;

            string ds = jBinding.GetDataSource();
            if (string.IsNullOrEmpty(ds))
                return;

            ds = ResolveGetParentNameFunction(ds, jObj);

            //lo scrivo nell'html in tutti i casi, ma non quando sto generando il binding dello slavebuffered
            if (writeHtml)
            {
                string[] tokens = ds.Split('.');
                if (tokens.Length == 1 || tokens.Length == 2)
                {
                    StringBuilder alias = new StringBuilder();
                    string table = tokens.Length == 1 ? "" : tokens[0];
                    string field = tokens.Length == 1 ? tokens[0] : tokens[1];
                    if (!string.IsNullOrEmpty(table))
                    {
                        string s = CheckAlias("", table);
                        if (!string.IsNullOrEmpty(s))
                        {
                            alias.Append('[');
                            alias.Append(s);
                            alias.Append(']');
                        }

                    }
                    StringBuilder fieldAlias = new StringBuilder();
                    if (!string.IsNullOrEmpty(field))
                    {
                        string s = CheckAlias(table, field);
                        if (!string.IsNullOrEmpty(s))
                        {
                            fieldAlias.Append('[');
                            fieldAlias.Append(s);
                            fieldAlias.Append(']');
                            alias.Append(fieldAlias);
                        }
                    }
                    if (insideRowView)
                    {
                        if (alias.Length == 0 || fieldAlias.Length == 0)
                            htmlWriter.WriteAttribute(Square(Constants.model), string.Concat("currentRow?.", field));
                        else
                            htmlWriter.WriteAttribute(Square(Constants.model), string.Concat("currentRow", fieldAlias));
                    }
                    else
                    {
                        if (alias.Length == 0)
                            //[model]="eventData?.data?.DBT?.Languages?.Language"
                            htmlWriter.WriteAttribute(Square(Constants.model), string.Concat("eventData?.model?.", AdjustModelExpression(ds)));
                        else
                            htmlWriter.WriteAttribute(Square(Constants.model), string.Concat("eventData?.model", alias));
                    }
                }
                else
                {
                    throw new Exception("Invalid datasource: " + ds);
                }
                JObject jHKL = jBinding.GetObject(Constants.hotLink);
                if (jHKL != null)
                {
                    JToken tile = jObj.Parent.Parent.Parent;
                    if (tile?.GetWndObjType() == WndObjType.HotFilter)
                    {
                        //se sono un hotfilter, devo integrare le informazioni di namespace scritte una sola volta 
                        //nel parent (l'hot filter possiede il namespace per tutti i campi di filtro figli)
                        JObject jHFBinding = tile[Constants.binding] as JObject;
                        if (jHFBinding != null)
                        {
                            JObject jHFHKL = jHFBinding.GetObject(Constants.hotLink);
                            if (jHFHKL == null)
                            {
                                jHKL[Constants.tbNamespace] = jHFBinding[Constants.hotLinkNS];
                            }
                            else
                            {
                                jHKL = (JObject)jHKL.DeepClone();
                                jHKL[Constants.tbNamespace] = jHFHKL[Constants.tbNamespace];

                            }

                        }
                    }
                    string hkl = jHKL.ToString();
                    hkl = hkl.Replace("\r\n", "").Replace("\"", "'").Replace(" ", "");

                    if (hkl.IndexOf(Constants.getParentNameFunction) != -1)
                    {
                        string hklExpr = jHKL.GetValue("name").ToString().Replace(" ", "");
                        string hklValue = ResolveGetParentNameFunction(hklExpr, jObj);
                        hkl = hkl.Replace(hklExpr, hklValue);
                    }
                    htmlWriter.WriteAttribute("[hotLink]", hkl.ResolveInterplation());
                }

                JObject jSD = jObj.GetObject(Constants.stateData);
                if (jSD != null)
                {
                    JObject jDSBinding = jSD.GetObject(Constants.binding);
                    if (jDSBinding != null)
                    {
                        string dsBinding = jDSBinding.GetValue("datasource").ToString().Replace(" ", "");
                        string[] dsBindingParts = dsBinding.Split('.');

                        string stateData = "{" + string.Concat(
                        "'model': '",
                        dsBinding,
                        "'");

                        if (jSD.GetValue("invertState") != null)
                        {
                            string invertStateVal = jSD.GetValue("invertState").ToString().Replace(" ", "");
                            invertStateVal = invertStateVal.IndexOf("{") > 0 ? invertStateVal : invertStateVal.ToLower();
                            stateData += ", 'invertState': " + invertStateVal;
                        }

                        stateData += "}";

                        htmlWriter.WriteAttribute("[stateData]", stateData.ResolveInterplation());
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------
        private void WriteTreeAttributes(JObject jObj, WebControl wc)
        {
            WriteAttribute(jObj, Constants.border, Constants.border);
            WriteAttribute(jObj, Constants.hScroll, Constants.hScroll);
            WriteAttribute(jObj, Constants.disableDragDrop, Constants.disableDragDrop);
            WriteAttribute(jObj, Constants.hasButtons, Constants.hasButtons);
            WriteAttribute(jObj, Constants.hasLines, Constants.hasLines);
            WriteAttribute(jObj, Constants.linesAtRoot, Constants.linesAtRoot);
            WriteAttribute(jObj, Constants.alwaysShowSelection, Constants.alwaysShowSelection);
        }

        //-----------------------------------------------------------------------------------------
        string getControlId(JObject jObj)
        {
            var cmpId = jObj.GetId();
            JObject jParent = jObj.GetParentItem();

            if (jParent?.GetBool(Constants.userControl) == true)
            {
                cmpId = jParent.GetId() + '_' + cmpId;
            }
            return cmpId;
        }

        //-----------------------------------------------------------------------------------------
        private void WriteControlAttributes(JObject jObj, WebControl wc, bool insideRowView)
        {
            var cmpId = getControlId(jObj);

            if (!string.IsNullOrEmpty(cmpId))
                htmlWriter.WriteAttribute(Constants.cmpId, cmpId);

            htmlWriter.Write(" tbControl");

            string anchor = jObj.GetFlatString(Constants.anchor);
            if (!string.IsNullOrEmpty(anchor) && anchor.IndexOf("COL",StringComparison.InvariantCultureIgnoreCase) < 0)
                htmlWriter.WriteAttribute(Square("staticArea"), "false");

            string marginLeft = jObj.GetFlatString(Constants.marginLeft);
            if (!string.IsNullOrEmpty(marginLeft))
                htmlWriter.WriteAttribute("marginLeft", marginLeft);

            string textAlign = jObj.GetTextAlign();
            if (!string.IsNullOrEmpty(textAlign)) {
                htmlWriter.WriteAttribute("textAlign", textAlign);
            }


            var font = jObj[Constants.font];
            if (font != null)
            {
                string underline = font.GetFlatString(Constants.underline);
                if (!string.IsNullOrEmpty(underline))
                {
                    bool bUnderline = false;
                    bool.TryParse(underline, out bUnderline);
                    if (bUnderline)
                        htmlWriter.WriteAttribute(Square("underline"), "true");
                }

                string bold = font.GetFlatString(Constants.bold);
                if (!string.IsNullOrEmpty(bold))
                {
                    bool bBold = false;
                    bool.TryParse(bold, out bBold);
                    if (bBold)
                        htmlWriter.WriteAttribute(Square("bold"), "true");
                }

                string italic = font.GetFlatString(Constants.italic);
                if (!string.IsNullOrEmpty(italic))
                {
                    bool bItalic = false;
                    bool.TryParse(italic, out bItalic);
                    if (bItalic)
                        htmlWriter.WriteAttribute(Square("italic"), "true");
                }
            }

            foreach (var arg in wc.Args)
            {
                if (string.IsNullOrEmpty(arg.Value))
                {
                    htmlWriter.Write(' ');
                    htmlWriter.Write(arg.Key);
                }
                else
                {
                    String value = arg.Value;
                    if (value.StartsWith("[", StringComparison.CurrentCulture) && value.EndsWith("]", StringComparison.CurrentCulture))
                    {
                        value = jObj.GetFlatString(value.Substring(1, value.Length - 2));
                    }

                    htmlWriter.WriteAttribute(arg.Key, value);
                }
            }

            // se il selettore � descritto nel tbjson uso quello, altrimenti lo cerco nell'xml
            if (jObj[Constants.selector] is JObject jSelector)
            {
                WriteSelector(cmpId, $"{{{string.Join(",\r\n", jSelector.Properties().Select(x => $"{x.Name}: '{x.Value}'"))}}}", jObj);
            }
            else if (!(string.IsNullOrEmpty(wc.Selector.value) || string.IsNullOrEmpty(cmpId)))
            {
                WriteSelector(cmpId, wc.Selector.value, jObj);
            }

            string caption = jObj.GetLocalizableString(Constants.controlCaption);
            if (!string.IsNullOrEmpty(caption))
                htmlWriter.WriteAttribute(Square(Constants.caption), caption);


            WriteAttribute(jObj, Constants.decimals, Constants.decimals);
            WriteAttribute(jObj, Constants.numberDecimal, Constants.decimals);
            WriteAttribute(jObj, Constants.width, Constants.width);
            WriteAttribute(jObj, Constants.height, Constants.height);
            WriteAttribute(jObj, Constants.maxValue, Constants.maxValue);
            WriteAttribute(jObj, Constants.minValue, Constants.minValue);

            if (wc.Name == Constants.tbText)
            {
                WriteAttribute(jObj, Constants.rows, Constants.rows);
                WriteAttribute(jObj, Constants.multiline, Constants.multiline);
                WriteAttribute(jObj, Constants.textlimit, Constants.textlimit);
            }

            WriteBindingAttributes(jObj, true, insideRowView);

            var jItem = jObj[Constants.itemSource] as JObject;
            if (jItem != null)
            {
                var strItemSource = $"{cmpId}_{Constants.itemSource}";

                htmlWriter.Write($" [{Constants.itemSource}]=\"{strItemSource}\"");

                toAppendToDeclaration.AppendIfNotExist($"public {strItemSource}: any;\r\n");
                toAppendToDefinition.AppendIfNotExist($"this.{strItemSource} = {jItem}; \r\n");
            }

            JArray jArray = jObj[Constants.validators] as JArray;
            if (jArray != null)
            {
                var strValidators = $"{cmpId}_{Constants.validators}";

                htmlWriter.Write($" [{Constants.validators}]=\"{strValidators}\"");

                toAppendToDeclaration.AppendIfNotExist($"public {strValidators}: any;\r\n");
                toAppendToDefinition.AppendIfNotExist($"this.{strValidators} = {jArray}; \r\n");
            }

            jItem = jObj[Constants.contextMenu] as JObject;
            if (jItem != null)
            {
                var strContextMenu = $"{cmpId}_{Constants.contextMenu}";

                htmlWriter.Write($" [{Constants.tbContextMenu}]=\"{strContextMenu}\"");

                toAppendToDeclaration.AppendIfNotExist($"public {strContextMenu}: any;\r\n");
                toAppendToDefinition.AppendIfNotExist($"this.{strContextMenu} = {jItem}; \r\n");
            }

            jItem = jObj[Constants.dataAdapter] as JObject;
            if (jItem != null)
            {
                var strDataAdapter = $"{cmpId}_{Constants.dataAdapter}";

                htmlWriter.Write($" [{Constants.dataAdapter}]=\"{strDataAdapter}\"");

                toAppendToDeclaration.AppendIfNotExist($"public {strDataAdapter}: any;\r\n");
                toAppendToDefinition.AppendIfNotExist($"this.{strDataAdapter} = {jItem}; \r\n");
            }

            jItem = jObj[Constants.menu] as JObject;
            if (jItem != null)
            {
                /*"menu": {
               "items": [
                 {
                   "type": "MenuItem",
                   "text": "Search by Code",
                   "id": "ID_MENU_SEARCH_ABICAB"
                   }
                   ]
               }*/
                var strContextMenu = $"cm_{cmpId}";
                htmlWriter.WriteAttribute(Square(Constants.contextMenu), strContextMenu);
                bool needComma = false;
                StringBuilder sbVal = new StringBuilder();
                foreach (var menuItem in jItem.GetItems())
                {
                    if (menuItem.GetWndObjType() != WndObjType.MenuItem)
                        continue;
                    if (needComma)
                        sbVal.AppendLine(",");
                    else
                        needComma = true;
                    string id = menuItem.GetId();
                    string text = menuItem.GetLocalizableString(Constants.text);

                    sbVal.Append($"new ContextMenuItem(this.{text}, '{id}')");
                }
                StringBuilder sbExistingVal;
                if (contextMenus.TryGetValue(strContextMenu, out sbExistingVal))
                {
                    if (!sbExistingVal.Equals(sbVal))
                        throw new Exception(string.Format("Controls with same id {0} but different content found", cmpId));
                }
                else
                {
                    toAppendToDeclaration.AppendLine();
                    toAppendToDeclaration.AppendLine($"public {strContextMenu} = [");
                    toAppendToDeclaration.Append(sbVal);
                    toAppendToDeclaration.AppendLine();
                    toAppendToDeclaration.AppendLine($"];");

                    contextMenus.Add(strContextMenu, sbVal);
                }

            }

        }
        //-----------------------------------------------------------------------------
        /*private void RegisterModelField(string owner, string field)
        {
            if (string.IsNullOrEmpty(owner))
                owner = "global";
            if (!modelStructure.TryGetValue(owner, out List<string> fields))
            {
                fields = new List<string>();
                modelStructure[owner] = fields;
            }
            fields.Add(field);
        }*/

        //-----------------------------------------------------------------------------
        private void GenerateTileChildren(JToken jObj, WndObjType parentType, bool insideRowView)
        {
            Dictionary<string, List<JObject>> anchorMap = new Dictionary<string, List<JObject>>();
            JArray jItems = jObj.GetItems();
            if (jItems == null)
                return;

            List<JObject> l = null;
            JEnumerable<JObject> children = jItems.Children<JObject>();
            foreach (JObject obj in children)
            {
                string anchor = obj.GetFlatString(Constants.anchor);
                if (string.IsNullOrEmpty(anchor))
                    anchor = Constants.COL1;

                if (!anchorMap.TryGetValue(anchor, out l))
                {
                    l = new List<JObject>();
                    anchorMap[anchor] = l;
                }
                l.Add(obj);

            }

            if (anchorMap.TryGetValue(Constants.COL1, out l))
            {
                using (OpenCloseTagWriter w = new OpenCloseTagWriter(Constants.div, this, false))
                {
                    htmlWriter.Write(" class=\"col col1\"");
                    w.CloseBeginTag();
                    foreach (JObject obj in l)
                    {
                        GenerateInlineControls(obj, parentType, anchorMap, true, insideRowView);

                    }
                }
            }

            if (anchorMap.TryGetValue(Constants.COL2, out l))
            {
                using (OpenCloseTagWriter w = new OpenCloseTagWriter(Constants.div, this, false))
                {
                    htmlWriter.Write(" class=\"col col2\"");
                    w.CloseBeginTag();
                    foreach (JObject obj in l)
                    {
                        GenerateInlineControls(obj, parentType, anchorMap, true, insideRowView);

                    }
                }
            }

        }

        private void GenerateInlineControls(JObject obj, WndObjType parentType, Dictionary<string, List<JObject>> anchorMap, bool needOuterDiv, bool insideRowView)
        {
            List<JObject> l = null;
            string id = obj.GetId();

            if (!string.IsNullOrEmpty(id) && anchorMap.TryGetValue(id, out l))
            {
                OpenCloseTagWriter w = null;
                if (needOuterDiv)
                {
                    w = new OpenCloseTagWriter(Constants.div, this, false);
                    htmlWriter.Write(" class=\"anchored\"");
                    w.CloseBeginTag();
                }
                GenerateHtml(obj, parentType, insideRowView);
                foreach (JObject objSameLine in l)
                    GenerateInlineControls(objSameLine, parentType, anchorMap, false, insideRowView);

                if (w != null)
                    w.Dispose();

            }
            else
            {
                GenerateHtml(obj, parentType, insideRowView);
            }
        }


        //-----------------------------------------------------------------------------
        private void GenerateHtmlChildren(JToken jObj, WndObjType parentType, bool insideRowView)
        {
            JArray jItems = jObj.GetItems();
            if (jItems != null)
            {
                foreach (JObject obj in jItems.Children<JObject>())
                {
                    GenerateHtml(obj, parentType, insideRowView);
                }
            }
        }

        //-----------------------------------------------------------------------------
        internal void BeginTag(string tag, bool full, bool newLine)
        {
            indent++;

            htmlWriter.WriteIndent(indent);
            if (full)
                htmlWriter.WriteFullBeginTag(tag);
            else
                htmlWriter.WriteBeginTag(tag);
            if (newLine)
                htmlWriter.Write("\r\n");
        }
        //-----------------------------------------------------------------------------
        internal void EndTag(string tag, bool writeIndent)
        {
            if (writeIndent)
                htmlWriter.WriteIndent(indent);
            htmlWriter.WriteEndTag(tag);
            htmlWriter.Write("\r\n");
            indent--;
        }

        internal void CloseBeginTag(bool newLine)
        {
            htmlWriter.Write(HtmlTextWriter.TagRightChar);
            if (newLine)
                htmlWriter.Write("\r\n");
        }

        private void WriteSelector(string cmpId, string value, JObject jObj)
        {
            var slice = $"{cmpId}_Slice$";
            var selector = $"{cmpId}_Selector";

            toAppendToDeclaration.AppendIfNotExist($"{slice}: any;\r\n");
            toAppendToDeclaration.AppendIfNotExist($"{selector}: any;\r\n");

            var selectorValue = value;

            var binding = jObj[Constants.binding];
            if (binding != null)
            {
                var ds = binding[Constants.datasource];
                if (ds != null)
                {
                    selectorValue = selectorValue.Replace("@datasource", ResolveGetParentNameFunction(ds.ToString(), jObj));
                }
            }

            toAppendToDefinition.AppendIfNotExist($"this.{selector} = createSelectorByMap({selectorValue});\r\n");
            toAppendToDefinition.AppendIfNotExist($"this.{slice} = this.store.select(this.{selector});\r\n");

            htmlWriter.WriteAttribute(Square("slice"), $"{slice} | async");
            htmlWriter.WriteAttribute(Square("selector"), $"{selector}");
        }

        private void WriteButtonInfo(JObject jObj)
        {
            if (jObj[Constants.ngClass] == null)
                return;

            if (!string.IsNullOrWhiteSpace(jObj[Constants.ngClass].ToString()))
                htmlWriter.WriteAttribute(Constants.sClass, jObj[Constants.ngClass].ToString());
        }
    }
}

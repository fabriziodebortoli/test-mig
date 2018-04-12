﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedCode;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Microarea.TbJson
{
    class JsonParserInfo
    {
        private JToken jRoot;
        private DateTime mostRecentFileDate;

        public JsonParserInfo(JToken jRoot, DateTime mostRecentFileDate)
        {
            this.jRoot = jRoot;
            this.mostRecentFileDate = mostRecentFileDate;
        }

        public JToken JRoot { get => jRoot.DeepClone(); set => jRoot = value; }
        public DateTime MostRecentFileDate { get => mostRecentFileDate; set => mostRecentFileDate = value; }
    }


    class JsonParser
    {
        Dictionary<string, JsonParserInfo> jsonList = new Dictionary<string, JsonParserInfo>(StringComparer.InvariantCultureIgnoreCase);
        private static List<ClientForm> emptyClients = new List<ClientForm>();
        public DateTime mostRecentFileDate = DateTime.MinValue;
        //-----------------------------------------------------------------------------
        internal JsonParserInfo Parse(string standardFolder, string tbJsonFile, bool root)
        {
            JsonParserInfo jsonParserInfo = null;
            JToken jRoot = null;
            if (jsonList.TryGetValue(tbJsonFile, out jsonParserInfo))
                return jsonParserInfo;

            try
            {
                FileInfo fi = new FileInfo(tbJsonFile);
                if (fi.LastWriteTime > mostRecentFileDate)
                    mostRecentFileDate = fi.LastWriteTime;

                using (StreamReader sr = new StreamReader(tbJsonFile))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        jRoot = JToken.ReadFrom(reader);
                    }
                }
                string resourcePath = Path.GetDirectoryName(tbJsonFile);
                //parso gli href contenuti nel file
                ParseHRefs(standardFolder, resourcePath, jRoot);
                ParseRowView(standardFolder, resourcePath, jRoot);
                //se sono un file root, parso anche i client forms

                if (root)
                {
                    List<string> ids = new List<string>();
                    ids.Add(Path.GetFileNameWithoutExtension(tbJsonFile));
                    JArray hier = jRoot[Constants.HrefHierarchy] as JArray;
                    if (hier != null)
                        foreach (string id in hier)
                            if (!ids.Contains(id))
                                ids.Add(id);

                    List<ClientForm> clients = new List<ClientForm>();
                    foreach (var id in ids)
                        AddClientForms(clients, GetClientForms(id));
                    foreach (ClientForm cf in clients)
                        if (!cf.exclude)
                            ParseHRef(standardFolder, resourcePath, (JObject)jRoot, cf.name, true);
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLineAsync("Error parsing file " + tbJsonFile);
                Console.Out.WriteLineAsync(ex.Message);
                jsonList[tbJsonFile] = null;
                return null;
            }


            JsonParserInfo info = new JsonParserInfo(jRoot.DeepClone(), mostRecentFileDate);
            jsonList[tbJsonFile] = info;
            return info;
        }

        //-----------------------------------------------------------------------------
        private void ParseRowView(string standardFolder, string resourcePath, JToken jRoot)
        {
            List<JToken> list = new List<JToken>();
            list.AddRange(jRoot.SelectTokens("..rowView"));
            foreach (JToken jRW in list)
            {
                string href = jRW.ToString();
                if (string.IsNullOrEmpty(href))
                    continue;
                string resourceName;
                string file = GetFile(standardFolder, resourcePath, href, out resourceName);
                if (!File.Exists(file))
                    throw new Exception(string.Concat("Invalid href: ", href, " - File not found!"));
                //JObject jHref = (JObject)Parse(standardFolder, file, false);
                JsonParserInfo jsonParse = Parse(standardFolder, file, false);
                JObject jHref = (JObject)jsonParse.JRoot;
                TrimNoWebSections(jHref);
                JToken jBody = jRW.Parent.Parent;
                foreach (JObject row in jBody.GetItems())
                    if (row.GetWndObjType() == WndObjType.ColTitle)
                        FillMissingColumnProps(jHref, row);
                jRW.Parent.Parent[Constants.rowViewForm] = jHref;
                jRW.Parent.Remove();
            }
        }

        //-----------------------------------------------------------------------------
        internal void TrimNoWebSections(JToken jRoot)
        {
            List<JToken> toRemove = new List<JToken>();
            foreach (JToken t in jRoot.SelectTokens("..environment"))
            {
                if (t.ToString() == "desktop")
                    toRemove.Add(t.Parent.Parent);
            }
            foreach (JToken t in toRemove)
            {
                t.Remove();
            }
        }
        //-----------------------------------------------------------------------------
        void FillMissingColumnProps(JObject rowViewDesc, JObject colDesc)
        {
            if (rowViewDesc == null)
                return;
            JObject other = rowViewDesc.Find(colDesc.GetId());
            if (other == null)
                return;
            CopyProp(colDesc, other, Constants.activation);
            CopyProp(colDesc, other, Constants.name);
            CopyProp(colDesc, other, Constants.controlClass);
            if (string.IsNullOrEmpty(colDesc.GetFlatString(Constants.text)))
            {
                string controlCaption = other.GetFlatString(Constants.controlCaption);
                if (!string.IsNullOrEmpty(controlCaption))

                    colDesc[Constants.text] = controlCaption;
                else
                    colDesc[Constants.text] = other[Constants.text];
            }

            CopyProp(colDesc, other, Constants.chars);
            CopyProp(colDesc, other, Constants.rows);

            CopyProp(colDesc, other, Constants.textLimit);

            CopyProp(colDesc, other, Constants.minValue);
            CopyProp(colDesc, other, Constants.maxValue);
            CopyProp(colDesc, other, Constants.numberDecimal);

            CopyProp(colDesc, other, Constants.controlBehaviour);
            CopyProp(colDesc, other, Constants.menu);

            CopyProp(colDesc, other, Constants.sort);
            CopyProp(colDesc, other, Constants.itemSource);
            CopyProp(colDesc, other, Constants.validators);

            CopyProp(colDesc, other, Constants.binding, true);
            CopyProp(colDesc, other, Constants.stateData, true);
        }

        //-----------------------------------------------------------------------------
        private static void CopyProp(JObject colDesc, JToken other, string propName, bool preserveObject = false)
        {
            JToken jToken = colDesc[propName];
            if (jToken == null)
            {
                JToken otherProp = other[propName];
                if (otherProp != null)
                    colDesc[propName] = otherProp.DeepClone();
            }
            else if (preserveObject && jToken is JObject)
            {
                foreach (JProperty prop in ((JObject)jToken).Properties())
                {
                    JToken otherProp = other[propName];
                    if (otherProp != null)
                        CopyProp((JObject)jToken, otherProp, prop.Name, preserveObject);
                }
            }
        }

        //-----------------------------------------------------------------------------
        private void AddClientForms(List<ClientForm> clients, List<ClientForm> sources)
        {
            foreach (var cfs in sources)
            {
                bool existing = false;
                foreach (var cf in clients)
                {
                    if (cf.name == cfs.name)
                    {
                        existing = true;
                        if (cfs.exclude)
                            cf.exclude = true;
                        break;
                    }
                }
                if (!existing)
                    clients.Add(cfs);
            }

        }

        //-----------------------------------------------------------------------------
        private List<ClientForm> GetClientForms(string server)
        {
            ClientFormMap clientForms = CacheConnector.GetClientForms();
            List<ClientForm> clients;
            clientForms.TryGetValue(server, out clients);
            return clients == null ? emptyClients : clients;
        }

        //-----------------------------------------------------------------------------
        private void ParseHRefs(string standardFolder, string resourcePath, JToken jRoot)
        {
            //clono la lista originaria, per poter iterare senza problemi; il processo potrebbe infatti 
            //modificarmi la lista sotto il sedere 
            List<JToken> tokens = new List<JToken>();
            foreach (JToken t in jRoot.AsJEnumerable())
                tokens.Add(t);
            foreach (JToken t in tokens)
            {
                switch (t.Type)
                {
                    case JTokenType.Object:
                    case JTokenType.Array:
                        ParseHRefs(standardFolder, resourcePath, t);
                        break;
                    case JTokenType.Property:
                        {
                            JProperty jProp = (JProperty)t;
                            if (jProp.Name == Constants.href)
                                //hrefs.Add(jProp.Value.ToString());
                                ParseHRef(standardFolder, resourcePath, (JObject)jRoot, jProp.Value.ToString(), false);
                            else
                                ParseHRefs(standardFolder, resourcePath, t);
                            break;
                        }
                }
            }

        }
        //-----------------------------------------------------------------------------
        private string GetFile(string standardFolder, string resourcePath, string href, out string name)
        {
            name = href;
            string[] tokens = href.Split('.');
            if (tokens.Length == 1)
                return Path.Combine(resourcePath, href + ".tbjson");
            int nPos = 0;
            string type = tokens[nPos++];
            bool modOwner = false, docOwner = type == "D"; //posso trovare 'M' (modulo) o 'D' (documento), o il nome dell'applicazione
            if (!docOwner)
                modOwner = type == "M";
            bool unknownOwner = !docOwner && !modOwner;
            if (unknownOwner)
            {
                docOwner = true;
                nPos--;
            }
            string app = tokens[nPos++];
            string mod = nPos == -1 ? "" : tokens[nPos++];
            string category = nPos == -1 ? "" : tokens[nPos++];
            name = nPos == -1 ? "" : tokens[nPos++];
            string appPath = app.Equals("Framework", StringComparison.InvariantCultureIgnoreCase) || app.Equals("Extensions", StringComparison.InvariantCultureIgnoreCase)
                ? Path.Combine(standardFolder, "TaskBuilder", app)
                : Path.Combine(standardFolder, "Applications", app);
            if (docOwner) //se l'owner è il documento, oppure non lo so, lo cerco nella descrizione del documento stesso
            {
                string candidateFilePath = Path.Combine(new string[] { appPath, mod, Constants.moduleObjects, category, Constants.jsonForms, name + ".tbjson" });
                if (File.Exists(candidateFilePath))
                {
                    return candidateFilePath;
                }
            }

            //altrimenti in una sottocartella della jsonforms di modulo
            return Path.Combine(new string[] { appPath, mod, Constants.jsonForms, category, name + ".tbjson" });
        }
        //-----------------------------------------------------------------------------
        private void ParseHRef(string standardFolder, string resourcePath, JObject jRoot, string href, bool forClientDoc)
        {
            if (jRoot[Constants.href] != null)
            {
                Debug.Assert(jRoot[Constants.href].Value<string>().Equals(href));
                jRoot.Remove(Constants.href);
            }

            string resourceName;
            string file = GetFile(standardFolder, resourcePath, href, out resourceName);
            if (!File.Exists(file))
                throw new Exception(string.Concat("Invalid href: ", href, " - File not found!"));

            string activation = forClientDoc ? ActivationFromFilePath(file) : "";
            //JToken jHref = Parse(standardFolder, file, false);

            JsonParserInfo jsonParseInfo = Parse(standardFolder, file, false);
            JToken jHref = jsonParseInfo.JRoot;

            if (jsonParseInfo.MostRecentFileDate > mostRecentFileDate)
                mostRecentFileDate = jsonParseInfo.MostRecentFileDate;

            if (jHref == null)
                throw new Exception(string.Concat("Invalid href: ", href));


            if (jHref.GetWndObjType() == WndObjType.Frame)
            {
                JArray hier = jRoot[Constants.HrefHierarchy] as JArray;
                if (hier == null)
                {
                    hier = new JArray();
                    jRoot[Constants.HrefHierarchy] = hier;
                }
                hier.Add(resourceName);
            }
            //nel caso di client document, devo mergiare il contenuto del file parsato all'oggetto a cui si riferisce by id, non alla root
            if (forClientDoc)
            {
                string id = jHref.GetId();
                if (string.IsNullOrEmpty(id))
                    throw new Exception(string.Concat("Invalid href: ", href, " - Root element has no id, cannot merge client form"));
                List<JObject> list = new List<JObject>();
                jRoot.FindAll(id, list);
                if (list.Count == 0)
                    throw new Exception(string.Concat("Id '", id, "'not found, cannot merge client form"));
                foreach (JObject jInner in list)
                    ParseHref(jInner, activation, jHref);
            }
            else
            {
                ParseHref(jRoot, activation, jHref);
            }

        }

        private void ParseHref(JObject jRoot, string activation, JToken jHref)
        {
            if (jHref is JObject)
            {
                //prima di tutto effettuo il merge fra il nodo che contiene href, e il file referenziato
                Merge(jRoot, (JObject)jHref, activation);
                string id = jHref.GetId();
                if (!string.IsNullOrEmpty(id))
                {
                    //poi vado a vedere se esiste qualche fratello con lo stesso id, proveniente da
                    //altri href indiretti; in caso affermativo, fondo i due nodi (quello più recente va a fondersi 
                    //col vecchio)
                    if (jRoot.Parent is JArray)
                    {
                        foreach (JObject child in jRoot.Parent)
                        {
                            if (child != jRoot)
                            {
                                List<JObject> list = new List<JObject>();
                                child.FindAll(id, list);
                                if (list.Count > 0)
                                {
                                    jRoot.Remove();
                                    string existingActivation = jRoot.GetFlatString(Constants.activation);
                                    if (!string.IsNullOrEmpty(existingActivation))
                                    {
                                        jRoot.Remove(Constants.activation);
                                        //se sono in un href con attivazione, tutti gli elementi referenziati ereditano l'attivazione
                                        foreach (JObject item in jRoot.GetItems())
                                            AddActivationAttribute(existingActivation, item);
                                    }
                                    foreach (JObject jObj in list)
                                        Merge(jObj, (JObject)jRoot.DeepClone(), activation);
                                    break;
                                }
                            }
                        }
                    }
                }

            }
            if (jHref is JArray)
                Merge(jRoot, (JArray)jHref);
        }


        //-----------------------------------------------------------------------------
        private string ActivationFromFilePath(string file)
        {
            string standardFolder, app, mod, container, name;
            if (!ExtractPathInfo(file, out standardFolder, out app, out mod, out container, out name))
            {
                Console.Out.WriteLineAsync("Cannot extract needed information, invalid file path: " + file);
                return "";
            }
            return string.Concat(app, '.', mod);
        }
        //-----------------------------------------------------------------------------
        internal bool ExtractPathInfo(string tbJsonFile, out string standardFolder, out string app, out string mod, out string container, out string name)
        {
            string[] tokens = tbJsonFile.Split(Path.DirectorySeparatorChar);
            standardFolder = app = mod = container = name = "";
            // \standard\taskbuilder\Framework\TbGes\JsonForms\UITileDialog\file.tbjson
            // oppure
            // D:\development\standard\taskbuilder\Framework\TbGes\ModuleObjects\TestJson\JsonForms\file.tbjson
            int i = tokens.Length - 1;
            if (i < 0)
                return false;
            name = Path.GetFileNameWithoutExtension(tokens[i--]);
            if (i < 0)
                return false;
            container = tokens[i--];

            if (container.Equals(Constants.jsonForms, StringComparison.InvariantCultureIgnoreCase))
            {
                if (i < 0)
                    return false;
                container = tokens[i--];
            }
            else
            {
                i--;//salto jsonforms 
            }
            if (i < 0)
                return false;
            mod = tokens[i--];
            if (mod.Equals(Constants.moduleObjects, StringComparison.InvariantCultureIgnoreCase))
            {
                if (i < 0)
                    return false;
                mod = tokens[i--];
            }
            if (i < 0)
                return false;
            app = tokens[i--];

            while (i >= 0)
            {
                if (tokens[i].Equals(Constants.standard, StringComparison.InvariantCultureIgnoreCase))
                {
                    break;
                }
                if (tokens[i].Equals(Constants.custom, StringComparison.InvariantCultureIgnoreCase))
                {
                    tokens[i] = Constants.standard;
                    break;
                }

                i--;
            }
            if (i < 0)
                return false;
            Array.Resize(ref tokens, i + 1);
            tokens[0] += "\\";//buco della Path.Combine?

            standardFolder = Path.Combine(tokens);
            return true;
        }

        //-----------------------------------------------------------------------------
        private void Merge(JToken jRoot, JArray jHref)
        {
            JArray ar = jRoot.Parent as JArray;
            if (ar == null)
                return;
            ar.Remove(jRoot);
            string activation = jRoot.GetFlatString(Constants.activation);
            foreach (JObject objExternal in jHref.Children<JObject>())
            {
                if (!string.IsNullOrEmpty(activation) && objExternal[Constants.activation] == null)
                    objExternal[Constants.activation] = activation;

                ar.Add(objExternal);

            }
        }
        //-----------------------------------------------------------------------------
        private void Merge(JToken jRoot, JObject jHref, string activation)
        {
            foreach (JProperty pExternal in jHref.Properties())
            {
                JToken t = jRoot[pExternal.Name];
                if (pExternal.Name == Constants.rowViewForm)
                {
                    //il form di row view non va mergiato ma sostituito integralmente, per simmetria
                    //col motore c++
                    jRoot[Constants.rowViewForm] = jHref[Constants.rowViewForm].DeepClone();
                }
                else if (t?.Type == JTokenType.Object)
                {
                    Merge(t, (JObject)pExternal.Value, activation);
                }
                else if (t?.Type == JTokenType.Array)
                {
                    JArray arExternal = (JArray)pExternal.Value;
                    JArray arCurrent = (JArray)t;
                    foreach (JToken objExternal in arExternal.Children<JToken>())
                    {
                        string id = objExternal.GetId();
                        JToken toMerge = null;
                        if (id == null || (toMerge = arCurrent.Find(id)) == null)
                        {
                            AddActivationAttribute(activation, objExternal);
                            arCurrent.Add(objExternal);

                        }
                        else
                        {
                            Merge(toMerge, (JObject)objExternal, activation);
                        }
                    }
                }
                else
                {
                    // il merge di singole proprietà non riesco a gestirlo col tag activation
                    if (string.IsNullOrEmpty(activation))
                    {
                        if (jRoot[pExternal.Name] == null)
                        {
                            jRoot[pExternal.Name] = pExternal.Value;
                        }
                        else if (pExternal.Name == Constants.id)
                        {
                            JArray hier = jRoot[Constants.idHierarchy] as JArray;
                            if (hier == null)
                            {
                                hier = new JArray();
                                jRoot[Constants.idHierarchy] = hier;
                            }
                            hier.Add(pExternal.Value);
                        }
                    }
                }
            }
        }

        private static void AddActivationAttribute(string activation, JToken objExternal)
        {
            if (!string.IsNullOrEmpty(activation))
            {
                string s = objExternal[Constants.activation]?.ToString();
                if (string.IsNullOrEmpty(s))
                    s = activation;
                else
                    s = string.Concat('(', activation, ")&(", s, ')');
                objExternal[Constants.activation] = s;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using SharedCode;

namespace ClientFormsProvider
{
    class Parser
    {
        ClientFormMap clientForms;
        ControlClassMap controlClasses;
        IconsMatchMap iconsMatch;

        //-----------------------------------------------------------------------------
        public ClientFormMap ClientForms
        {
            get
            {
                return clientForms;
            }
        }

        //-----------------------------------------------------------------------------
        public ControlClassMap ControlClasses
        {
            get
            {
                return controlClasses;
            }
        }

        //-----------------------------------------------------------------------------
        public Dictionary<string, string> IconsMatchMap
        {
            get
            {
                return iconsMatch;
            }
        }

        //-----------------------------------------------------------------------------
        public void Parse(string folder)
        {
            clientForms = new ClientFormMap();
            controlClasses = new ControlClassMap();
            iconsMatch = new IconsMatchMap();
            string[] files = Directory.GetFiles(folder, "*.xml", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                if (Path.GetFileNameWithoutExtension(file).Equals("clientdocumentobjects", StringComparison.InvariantCultureIgnoreCase))
                    ExtractClientForms(file);
                else if (Path.GetFileNameWithoutExtension(file).Equals("webcontrols", StringComparison.InvariantCultureIgnoreCase))
                    ExtractControlClasses(file);
                else if (Path.GetFileNameWithoutExtension(file).Equals("iconsmatch", StringComparison.InvariantCultureIgnoreCase))
                    LoadIconsConversionTable(file);
            }
        }

        //-----------------------------------------------------------------------------------------
        public void LoadIconsConversionTable(string file)
        {
            var doc = new XmlDocument();
            doc.Load(file);

            foreach (XmlNode currentIcon in doc.SelectNodes("//icons/icon"))
            {
                if (currentIcon.Attributes["iconNs"] == null || currentIcon.Attributes["icon"] == null)
                    continue;

                string iconNs = currentIcon.Attributes["iconNs"].Value.ToLower();
                string icon = currentIcon.Attributes["icon"].Value.ToLower();
                if (iconsMatch.TryGetValue(iconNs, out string value))
                    continue;

                iconsMatch.Add(iconNs, icon);
            }
        }

        //-----------------------------------------------------------------------------
        private void ExtractControlClasses(string file)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(file);
                foreach (XmlElement node in doc.SelectNodes("WebControls/Associations/WebControl"))
                {
                    string name = node.GetAttribute("control");
                    if (string.IsNullOrEmpty(name))
                        continue;

                    string controlClass = node.GetAttribute("controlClass");
                    if (string.IsNullOrEmpty(controlClass))
                        continue;

                    string columnControlName = node.GetAttribute("columnControl");
                    var ctrl = new WebControl(name, columnControlName);

                    string query = string.Concat("WebControls/Properties/WebControl[@name='", name, "']");
                    XmlElement propNode = doc.SelectSingleNode(query) as XmlElement;
                    if (propNode != null)
                        foreach (XmlElement prop in propNode.GetElementsByTagName("Property"))
                        {
                            string jsonProp = prop.GetAttribute("jsonName");
                            if (string.IsNullOrEmpty(jsonProp))
                            {
                                Debug.Fail("Property name not specified for WebControl " + name);
                                continue;
                            }
                            string tsProp = prop.GetAttribute("tsName");
                            if (string.IsNullOrEmpty(tsProp))
                            {
                                tsProp = jsonProp;
                            }
                            ctrl.Properties[jsonProp] = tsProp;
                        }
                    foreach (XmlElement arg in node.GetElementsByTagName("arg"))
                    {
                        name = arg.GetAttribute("name");
                        if (string.IsNullOrEmpty(name))
                            continue;
                        ctrl.Args[name] = arg.GetAttribute("value");
                    }

                    var selector = ((XmlElement)node.GetElementsByTagName("selector")?.Item(0));
                    if (selector != null)
                    {
                        name = selector.GetAttribute("name");
                        if (string.IsNullOrEmpty(name))
                            continue;
                        ctrl.Selector = (name, selector.GetAttribute("value"));
                    }

                   
                    ControlClasses[controlClass] = ctrl;
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLineAsync(ex.Message);
            }
        }
        //-----------------------------------------------------------------------------
        private void ExtractClientForms(string file)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(file);
                foreach (XmlElement node in doc.SelectNodes("ClientDocumentObjects/ClientForms/ClientForm"))
                {
                    string name = node.GetAttribute("name");
                    string server = node.GetAttribute("server");
                    if (!string.IsNullOrEmpty(server))
                        AddClientForm(name, server, false);
                    foreach (XmlElement serverEl in node.GetElementsByTagName("Server"))
                    {
                        server = serverEl.GetAttribute("name");
                        if (!string.IsNullOrEmpty(server))
                            AddClientForm(name, server, false);
                    }
                    foreach (XmlElement serverEl in node.GetElementsByTagName("Exclude"))
                    {
                        server = serverEl.GetAttribute("name");
                        if (!string.IsNullOrEmpty(server))
                            AddClientForm(name, server, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLineAsync(ex.Message);
            }
        }


        //-----------------------------------------------------------------------------
        private void AddClientForm(string name, string server, bool exclude)
        {
            List<ClientForm> clients;
            if (!ClientForms.TryGetValue(server, out clients))
            {
                clients = new List<ClientForm>();
                ClientForms[server] = clients;
            }
            clients.Add(new ClientForm(name, exclude));
        }
    }


}

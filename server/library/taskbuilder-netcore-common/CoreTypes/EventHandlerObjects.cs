using Microarea.Common.NameSolver;
using Microarea.Common.StringLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.Common.CoreTypes
{

    class EventHandlerObjects
    {
        private ModuleInfo parentModuleInfo;
        private List<Function> functions = new List<Function>();

        public IList Functions { get { return functions; } }

        //---------------------------------------------------------------------
        public EventHandlerObjects(ModuleInfo aParentModuleInfo)
        {
            parentModuleInfo = aParentModuleInfo;
        }

        //---------------------------------------------------------------------
        public bool Parse(string filePath)
        {
            LocalizableXmlDocument eventHandlerDocument = null;
            if (parentModuleInfo != null)
            {
                if (!PathFinder.PathFinderInstance.ExistFile(filePath))
                    return false;

                eventHandlerDocument = new LocalizableXmlDocument
                                            (
                                                parentModuleInfo.ParentApplicationInfo.Name,
                                                parentModuleInfo.Name,
                                                parentModuleInfo.CurrentPathFinder
                                            );

                //leggo il file
                eventHandlerDocument.Load(filePath);
            }
            return Parse(eventHandlerDocument);
        }
        ////---------------------------------------------------------------------
        //public bool Parse(Stream fileStream)
        //{
        //    XmlDocument document = new XmlDocument();
        //    //leggo il file
        //    document.Load(fileStream);
        //    return Parse(document);
        //}
        //---------------------------------------------------------------------
        public bool Parse(XmlDocument documentObjectsDocument)
        {
            try
            {
                XmlElement root = documentObjectsDocument.DocumentElement;

                XmlNodeList functionsElements = root.GetElementsByTagName("Functions");
                if (functionsElements != null && functionsElements.Count == 1)
                {
                    //array dei documenti
                    XmlNodeList functionElements = ((XmlElement)functionsElements[0]).GetElementsByTagName("Function");
                    if (functionElements == null)
                        return true;

                    ParseFunctions(functionElements);
                }


            }
            catch (XmlException e)
            {
                Debug.Fail(e.Message);
                return false;
            }
            catch (Exception err)
            {
                Debug.Fail(err.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Legge il file e crea gli array di document e clientDocument in memoria
        /// </summary>
        /// <returns>true se la lettura ha avuto successo</returns>
       
        /// <summary>
        /// Parsa tutti i documenti nell'xml
        /// </summary>
        /// <param name="documentElements"></param>
        /// <returns></returns>
        //---------------------------------------------------------------------
        bool ParseFunctions(XmlNodeList functionElements)
        {
            if (functionElements == null)
                return false;

            Function function;
            foreach (XmlElement functionElement in functionElements)
            {
                //namespace del documento
                string nameSpaceString = functionElement.GetAttribute(DocumentsObjectsXML.Attribute.Namespace);
                string title = functionElement.GetAttribute(DocumentsObjectsXML.Attribute.Localize);
                string type = functionElement.GetAttribute("type");

                function = new Function(nameSpaceString, title, type);

                functions.Add(function);
            }

            return true;
        }

    }

    //=========================================================================
    class Function
    {
        protected string nameSpace;
        protected string localize;
        protected string type;

        public string NameSpace { get { return nameSpace; } }
        public string Localize { get { return localize; } }
        public string Type { get { return type; } }
        //---------------------------------------------------------------------
        public Function(string nameSpace, string localize, string type)
        {
            this.nameSpace = nameSpace;
            this.localize = localize;
            this.type = type;
        }

    }
}

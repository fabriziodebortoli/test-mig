using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using Microarea.Common.NameSolver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microarea.Common.MenuLoader
{
    //===============================================================================================================
    public static class NewMenuSaver
    {
        //---------------------------------------------------------------------
        public static void InsertAt(this XmlNode node, XmlNode insertingNode, int index = 0)
        {
            if (insertingNode == null)
                return;
            if (index < 0)
                index = 0;

            var childNodes = node.ChildNodes;
            var childrenCount = childNodes.Count;

            if (index >= childrenCount)
            {
                node.AppendChild(insertingNode);
                return;
            }

            var followingNode = childNodes[index];

            node.InsertBefore(insertingNode, followingNode);
        }

        //---------------------------------------------------------------------
        private static void RemoveAllMostRecentBeyondMaxLimit(XmlDocument doc, int maxElements)
        {
            try
            {
                XmlNodeList allMostUsed = doc.SelectNodes("//MostUsed");
                XmlNode root = doc.SelectSingleNode("Root");
                if (allMostUsed.Count > maxElements)
                {
                    for (int i = allMostUsed.Count - 1; i >= maxElements; i--)
                    {
                        root.RemoveChild(allMostUsed[i]);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        //---------------------------------------------------------------------	
        public static void UpdateAllFavoritesAndMostUsed(string favorites, string mostUsed, string user, string company)
        {
            UpdateFavorites(favorites, user, company);
            UpdateMostUsed(mostUsed, user, company);
        }

        //---------------------------------------------------------------------	
        public static void UpdateFavorites(string favorites, string user, string company)
        {
            PathFinder pf = new PathFinder(company, user);
            string file = NewMenuFunctions.GetCustomUserFavoriteFile(pf);

            XmlDocument doc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(file, pf);

            JArray favs = JsonConvert.DeserializeObject<JArray>(favorites);
            if (favs == null)
                return;

            doc.DocumentElement.RemoveAll();

            foreach (JObject current in favs)
            {
                XmlNode node = doc.CreateElement("Favorite");

                XmlAttribute objectAttribute = doc.CreateAttribute("objectType");
                objectAttribute.Value = current["objectType"].ToString();
                node.Attributes.Append(objectAttribute);

                XmlAttribute targetAttribute = doc.CreateAttribute("target");
                targetAttribute.Value = current["target"].ToString();
                node.Attributes.Append(targetAttribute);

                JObject objName = current["objectName"] as JObject;

                if (objName != null && !string.IsNullOrEmpty(objName.ToString()))
                {
                    XmlAttribute objectNameAttribute = doc.CreateAttribute("objectName");
                    objectNameAttribute.Value = objName.ToString();
                    node.Attributes.Append(objectNameAttribute);
                }

                doc.DocumentElement.AppendChild(node);
            }


            UpdateAllFavoritesPositions(doc);

            SaveXml(doc, file);

            // return true;
        }

        //---------------------------------------------------------------------	
        public static void UpdateMostUsed(string mostUsed, string user, string company)
        {
            PathFinder pf = new PathFinder(company, user);
            string file = NewMenuFunctions.GetCustomUserMostUsedFile(pf);

            XmlDocument doc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(file, pf);

            JArray most = JsonConvert.DeserializeObject<JArray>(mostUsed);
            if (most == null)
                return;

            doc.DocumentElement.RemoveAll();

            foreach (JObject current in most)
            {
                //va creato in cima
                XmlNode node = doc.CreateElement("MostUsed");

                XmlAttribute objectAttribute = doc.CreateAttribute("objectType");
                objectAttribute.Value = current["objectType"].ToString();
                node.Attributes.Append(objectAttribute);

                XmlAttribute targetAttribute = doc.CreateAttribute("target");
                targetAttribute.Value = current["target"].ToString();
                node.Attributes.Append(targetAttribute);

                JObject objName = current["target"] as JObject;

                if (objName != null && !string.IsNullOrEmpty(objName.ToString()))
                {
                    XmlAttribute objectNameAttribute = doc.CreateAttribute("objectName");
                    objectNameAttribute.Value = objName.ToString();
                    node.Attributes.Append(objectNameAttribute);
                }

                XmlAttribute lastModifiedAttribute = doc.CreateAttribute("lastModified");
                lastModifiedAttribute.Value = current["lastModified"].ToString();
                node.Attributes.Append(lastModifiedAttribute);

                doc.DocumentElement.InsertAt(node, 0);
            }

            //limito a maxElements (default 50) il numero di elementi salvabili, se ne ho di più, gli ultimi li levo
            //RemoveAllMostRecentBeyondMaxLimit(doc, maxElements);

            SaveXml(doc, file);
        }

        //---------------------------------------------------------------------	
        private static bool UpdateShowNrElements(string file, string nrElements, PathFinder pf)
        {
            XmlDocument doc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(file, pf);

            XmlNode node = doc.SelectSingleNode("/Root");

            XmlAttribute attributeNrElementToShow = null;
            attributeNrElementToShow = node.Attributes["nrElementToShow"];
            if (attributeNrElementToShow == null)
            {
                attributeNrElementToShow = doc.CreateAttribute("nrElementToShow");
                node.Attributes.Append(attributeNrElementToShow);
            }
            attributeNrElementToShow.Value = nrElements;

            SaveXml(doc, file);
            return true;
        }

        //---------------------------------------------------------------------	
        public static bool UpdateMostUsedShowNrElements(string nrElements, string user, string company)
        {
            PathFinder pf = new PathFinder(company, user);
            string file = NewMenuFunctions.GetCustomUserMostUsedFile(pf);
            return UpdateShowNrElements(file, nrElements, pf);
        }

        //---------------------------------------------------------------------	
        public static bool UpdateHistoryShowNrElements(string nrElements, string user, string company)
        {
            PathFinder pf = new PathFinder(company, user);
            string file = NewMenuFunctions.GetCustomUserHistoryFile(pf);
            return UpdateShowNrElements(file, nrElements, pf);
        }

        //---------------------------------------------------------------------	
        public static bool SetPreference(string preferenceName, string preferenceValue, string user, string company)
        {
            PathFinder pf = new PathFinder(company, user);
            string file = NewMenuFunctions.GetCustomUserPreferencesFile(pf);

            XmlDocument doc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(file, pf);

            //cerco il nodo per namespace
            XmlNode node = doc.SelectSingleNode(
            string.Concat(
                "//Preference[",
                string.Format(MenuTranslatorStrings.translateTemplate, "name", preferenceName) + " ] "));

            if (node != null)
            {
                XmlAttribute valueAttribute = node.Attributes["value"];
                if (valueAttribute == null)
                {
                    valueAttribute = doc.CreateAttribute("value");
                    node.Attributes.Append(valueAttribute);
                }

                valueAttribute.Value = preferenceValue;
            }
            else
            {
                XmlElement element = doc.CreateElement("Preference");

                XmlAttribute nameAttribute = doc.CreateAttribute("name");
                nameAttribute.Value = preferenceName;
                element.Attributes.Append(nameAttribute);

                XmlAttribute valueAttribute = doc.CreateAttribute("value");
                valueAttribute.Value = preferenceValue;
                element.Attributes.Append(valueAttribute);

                doc.DocumentElement.AppendChild(element);
            }

            SaveXml(doc, file);
            return true;
        }

        //---------------------------------------------------------------------	
        public static bool SetLeftGroupVisibility(string groupName, string visible, string user, string company)
        {
            PathFinder pf = new PathFinder(company, user);
            string file = NewMenuFunctions.GetCustomUserPreferencesFile(pf);

            XmlDocument doc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(file, pf);

            XmlNode node = doc.SelectSingleNode(
            string.Concat(
                "//LeftGroupVisibility[",
                string.Format(MenuTranslatorStrings.translateTemplate, "name", groupName) + " ] "));

            if (node != null)
            {
                XmlAttribute valueAttribute = node.Attributes["visible"];
                if (valueAttribute == null)
                {
                    valueAttribute = doc.CreateAttribute("visible");
                    node.Attributes.Append(valueAttribute);
                }

                valueAttribute.Value = visible;
            }
            else
            {
                XmlElement element = doc.CreateElement("LeftGroupVisibility");

                XmlAttribute nameAttribute = doc.CreateAttribute("name");
                nameAttribute.Value = groupName;
                element.Attributes.Append(nameAttribute);

                XmlAttribute valueAttribute = doc.CreateAttribute("visible");
                valueAttribute.Value = visible;
                element.Attributes.Append(valueAttribute);

                doc.DocumentElement.AppendChild(element);
            }

            SaveXml(doc, file);
            return true;

        }

        //---------------------------------------------------------------------
        public static bool ClearHistory(string user, string company)
        {
            PathFinder pf = new PathFinder(company, user);
            string file = NewMenuFunctions.GetCustomUserHistoryFile(pf);

            XmlDocument doc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(file, pf);

            doc.DocumentElement.RemoveAll();

            SaveXml(doc, file);


            return true;
        }

        //---------------------------------------------------------------------
        public static bool ClearMostUsed(string user, string company)
        {
            PathFinder pf = new PathFinder(company, user);
            string file = NewMenuFunctions.GetCustomUserMostUsedFile(pf);

            XmlDocument doc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(file, pf);
            doc.DocumentElement.RemoveAll();

            SaveXml(doc, file);
            return true;
        }

        //---------------------------------------------------------------------
        private static bool UpdateAllFavoritesPositions(XmlDocument doc)
        {
            XmlNodeList list = doc.SelectNodes("//Favorite");

            XmlAttribute positionAttribute = null;
            XmlNode current = null;

            for (int i = 0; i < list.Count; i++)
            {
                current = list[i];

                positionAttribute = current.Attributes["position"];
                if (positionAttribute == null)
                {
                    positionAttribute = doc.CreateAttribute("position");
                    current.Attributes.Append(positionAttribute);
                }

                positionAttribute.Value = i.ToString();
            }

            return true;
        }

        //---------------------------------------------------------------------
        public static bool UpdateFavoritesPosition(string user, string company, string target1, string objectType1, string objectName1, string target2, string objectType2, string objectName2)
        {
            PathFinder pf = new PathFinder(company, user);
            string file = NewMenuFunctions.GetCustomUserFavoriteFile(pf);

            XmlDocument doc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(file, pf);

            string startSearch = string.Concat(
                "//Favorite[",
                string.Format(MenuTranslatorStrings.translateTemplate, "target", target1) + " and ",
                string.Format(MenuTranslatorStrings.translateTemplate, "objectType", objectType1));

            if (!string.IsNullOrEmpty(objectName1))

                startSearch += " and " + string.Format(MenuTranslatorStrings.translateTemplate, "objectName", objectName1);
            else
                startSearch += "]";

            XmlNode startingNode = doc.SelectSingleNode(startSearch);
            if (startingNode == null)
                return false;

            string destinationSearch = string.Concat(
                "//Favorite[",
                string.Format(MenuTranslatorStrings.translateTemplate, "target", target2) + " and ",
                string.Format(MenuTranslatorStrings.translateTemplate, "objectType", objectType2));

            if (!string.IsNullOrEmpty(objectName2))

                destinationSearch += " and " + string.Format(MenuTranslatorStrings.translateTemplate, "objectName", objectName2);
            else
                destinationSearch += "]";

            XmlNode destinationNode = doc.SelectSingleNode(destinationSearch);
            if (destinationNode == null)
                return false;

            RepositionFavorites(doc, startingNode, destinationNode);

            UpdateAllFavoritesPositions(doc);

            SaveXml(doc, file);
            return true;
        }


        //---------------------------------------------------------------------
        private static bool RepositionFavorites(XmlDocument doc, XmlNode fromNode, XmlNode toNode)
        {
            int index = -1;
            for (int i = 0; i < doc.DocumentElement.ChildNodes.Count; i++)
            {
                if (doc.DocumentElement.ChildNodes[i] == toNode)
                    index = i;
            }

            if (index < 0)
                return true;

            doc.DocumentElement.RemoveChild(fromNode);

            doc.DocumentElement.InsertAt(fromNode, index);

            return true;
        }

        //---------------------------------------------------------------------
        public static bool AddToHiddenTiles(string user, string company, string appName, string groupName, string menuName, string tileName)
        {
            PathFinder pf = new PathFinder(company, user);
            string file = NewMenuFunctions.GetCustomUserHiddenTilesFile(pf);

            XmlDocument doc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(file, pf);

            XmlNode node = doc.SelectSingleNode(
                string.Concat(
                    "//HiddenTile[",
                    string.Format(MenuTranslatorStrings.translateTemplate, "applicationName", appName) + " and ",
                    string.Format(MenuTranslatorStrings.translateTemplate, "groupName", groupName) + " and ",
                    string.Format(MenuTranslatorStrings.translateTemplate, "menuName", menuName) + " and ",
                    string.Format(MenuTranslatorStrings.translateTemplate, "tileName", tileName) + " ] "));

            if (node != null)
                return true;
            else
            {
                //va creato in cima
                node = doc.CreateElement("HiddenTile");

                XmlAttribute applicationNameAttribute = doc.CreateAttribute("applicationName");
                applicationNameAttribute.Value = appName;
                node.Attributes.Append(applicationNameAttribute);

                XmlAttribute groupNameAttribute = doc.CreateAttribute("groupName");
                groupNameAttribute.Value = groupName;
                node.Attributes.Append(groupNameAttribute);

                XmlAttribute menuNameAttribute = doc.CreateAttribute("menuName");
                menuNameAttribute.Value = menuName;
                node.Attributes.Append(menuNameAttribute);

                XmlAttribute tileNameAttribute = doc.CreateAttribute("tileName");
                tileNameAttribute.Value = tileName;
                node.Attributes.Append(tileNameAttribute);

                doc.DocumentElement.AppendChild(node);
            }

            SaveXml(doc, file);

            return true;
        }

        //---------------------------------------------------------------------
        public static bool RemoveFromHiddenTiles(string user, string company, string appName, string groupName, string menuName, string tileName)
        {
            PathFinder pf = new PathFinder(company, user);
            string file = NewMenuFunctions.GetCustomUserHiddenTilesFile(pf);

            XmlDocument doc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(file, pf);
            XmlNode node = doc.SelectSingleNode(
                            string.Concat(
                            "//HiddenTile[",
                            string.Format(MenuTranslatorStrings.translateTemplate, "applicationName", appName) + " and ",
                            string.Format(MenuTranslatorStrings.translateTemplate, "groupName", groupName) + " and ",
                            string.Format(MenuTranslatorStrings.translateTemplate, "menuName", menuName) + " and ",
                            string.Format(MenuTranslatorStrings.translateTemplate, "tileName", tileName) + " ] "));

            if (node == null)
                return true;

            doc.DocumentElement.RemoveChild(node);

            SaveXml(doc, file);

            return true;
        }

        //---------------------------------------------------------------------
        public static bool RemoveAllHiddenTiles(string user, string company)
        {
            PathFinder pf = new PathFinder(company, user);
            string file = NewMenuFunctions.GetCustomUserHiddenTilesFile(pf);

            XmlDocument doc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(file, pf);
            doc.DocumentElement.RemoveAll();

            SaveXml(doc, file);

            return true;
        }

        //---------------------------------------------------------------------
        public static void SaveXml(XmlDocument doc, string file)
        {
            bool result = PathFinder.PathFinderInstance.SaveTextFileFromXml(file, doc);
        }
    }
}

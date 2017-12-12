using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.SecurityLayer.SecurityLightObjects
{
	/// <summary>
	/// Summary description for SLDenyFile.
	/// </summary>
	public class SLDenyFile
	{
		private FileInfo fileInfo = null;

		public const string SecurityLightCustomMenuFolderName	= "SecurityLight";
		public const string SecurityLightDenyMenuFileName		= "SLDeny";

		private const string XML_SLDENY_ROOT_TAG				= "DeniedAccesses";
		private const string XML_SLDENY_DENIED_ACCESS_TAG		= "DeniedAccess";
		private const string XML_SLDENY_COMMAND_TYPE_ATTRIBUTE	= "command_type";

		//---------------------------------------------------------------------
		public SLDenyFile(string fileName)
		{
			if (fileName != null && fileName.Trim().Length > 0)
			{
				try
				{
					fileInfo = new FileInfo(fileName);
				}
				catch(Exception e)
				{
					Debug.Fail("Exception raised in SLDenyFile constructor: " + e.Message);
					fileInfo = null;
				}
			}
		}

		//---------------------------------------------------------------------
		public SLDenyFile(FileInfo aFileInfo)
		{
			fileInfo = aFileInfo;
		}

		#region SLDenyFile private methods

		//---------------------------------------------------------------------
		private XmlElement GetDeniedAccessNode(XmlDocument aXmlDocument, string aNameSpaceTextWithoutType, SecuredCommandType aSecuredCommandType)
		{
			try
			{
				if 
					(
					aXmlDocument == null ||
					aXmlDocument.DocumentElement == null || 
					String.Compare(aXmlDocument.DocumentElement.Name, XML_SLDENY_ROOT_TAG) != 0 ||
					aNameSpaceTextWithoutType == null || 
					aNameSpaceTextWithoutType.Length == 0 || 
					aSecuredCommandType == SecuredCommandType.Undefined
					)
					return null;

				string xpathExpression = "child::" + XML_SLDENY_DENIED_ACCESS_TAG + "[. = '" + aNameSpaceTextWithoutType + "' and @" + XML_SLDENY_COMMAND_TYPE_ATTRIBUTE + " = '" + aSecuredCommandType.ToString() + "']";

				XmlNode deniedAccessNode = aXmlDocument.DocumentElement.SelectSingleNode(xpathExpression);

				return (deniedAccessNode != null && (deniedAccessNode is XmlElement)) ? (XmlElement)deniedAccessNode : null;
			}
			catch(Exception e) 
			{
				Debug.Fail("Exception raised in SLDenyFile.GetDeniedAccessNode: " + e.Message);
				return null;
			}		
		}

		//---------------------------------------------------------------------
		private bool ExistDeniedAccessNode(XmlDocument aXmlDocument, string aNameSpaceTextWithoutType, SecuredCommandType aSecuredCommandType)
		{
			return (GetDeniedAccessNode(aXmlDocument, aNameSpaceTextWithoutType, aSecuredCommandType) != null);
		}
		
		//---------------------------------------------------------------------
		private void CleanDeniedAccessesByType
			(
			MenuXmlParser			aMenuXmlParser, 
			XmlDocument				aXmlDocument, 
			SecuredCommandType		aSecuredCommandType
			)
		{
			try
			{
				if 
					(
					aMenuXmlParser == null || 				
					aXmlDocument == null ||
					aXmlDocument.DocumentElement == null || 
					String.Compare(aXmlDocument.DocumentElement.Name, XML_SLDENY_ROOT_TAG) != 0 ||
					aSecuredCommandType == SecuredCommandType.Undefined
					)
					return;
			
				string xpathExpression = "child::" + XML_SLDENY_DENIED_ACCESS_TAG + "[@" + XML_SLDENY_COMMAND_TYPE_ATTRIBUTE + " = '" + aSecuredCommandType.ToString() + "']";

				XmlNodeList deniedAccessesNodeList = aXmlDocument.DocumentElement.SelectNodes(xpathExpression);
				if (deniedAccessesNodeList == null || deniedAccessesNodeList.Count == 0)
					return;

				foreach (XmlNode aDeniedAccessNode in deniedAccessesNodeList)
				{
					if (aDeniedAccessNode == null || !(aDeniedAccessNode is XmlElement))
						continue;

					MenuXmlNodeCollection menuCommandsToHide = aMenuXmlParser.GetAllCommands(aDeniedAccessNode.InnerText, SecuredCommand.GetMenuCommandTypeToLoad(aSecuredCommandType));
					if (menuCommandsToHide == null || menuCommandsToHide.Count == 0)
						continue;

					foreach (MenuXmlNode aCommandToHideNode in menuCommandsToHide)
						aMenuXmlParser.RemoveNode(aCommandToHideNode);
				}
			}
			catch(Exception e) 
			{
				Debug.Fail("Exception raised in SLDenyFile.CleanDeniedAccessesByType: " + e.Message);
			}		
		}
		
		#endregion // SLDenyFile private methods

		#region SLDenyFile public methods
		
		//---------------------------------------------------------------------
		public bool ExistDeniedAccessNode(string aNameSpaceTextWithoutType, SecuredCommandType aSecuredCommandType)
		{
			try
			{
				if 
					(
					fileInfo == null ||
					fileInfo.FullName == null ||
					fileInfo.FullName.Length == 0 ||
					!Path.IsPathRooted(fileInfo.FullName) ||
					!File.Exists(fileInfo.FullName) ||
					aNameSpaceTextWithoutType == null || 
					aNameSpaceTextWithoutType.Length == 0 || 
					aSecuredCommandType == SecuredCommandType.Undefined
					)
					return false;

				XmlDocument denyXmlDocument = new XmlDocument();

				denyXmlDocument.Load(fileInfo.FullName);

				return ExistDeniedAccessNode(denyXmlDocument, aNameSpaceTextWithoutType, aSecuredCommandType);
			}
			catch(Exception e) 
			{
				Debug.Fail("Exception raised in SLDenyFile.ExistDeniedAccessNode: " + e.Message);
				return false;
			}		
		}

		//---------------------------------------------------------------------
		public bool AddDeniedAccess(string aNameSpaceTextWithoutType, SecuredCommandType aSecuredCommandType)
		{
			try
			{
				if 
					(
					fileInfo == null ||
					fileInfo.FullName == null ||
					fileInfo.FullName.Length == 0 ||
					!Path.IsPathRooted(fileInfo.FullName) ||
					aNameSpaceTextWithoutType == null || 
					aNameSpaceTextWithoutType.Length == 0 || 
					aSecuredCommandType == SecuredCommandType.Undefined
					)
					return false;

				XmlDocument denyXmlDocument = new XmlDocument();

				if (!File.Exists(fileInfo.FullName))
				{
					if (!Directory.Exists(fileInfo.DirectoryName))
						Directory.CreateDirectory(fileInfo.DirectoryName);
			
					XmlDeclaration declaration = denyXmlDocument.CreateXmlDeclaration(NameSolverStrings.XmlDeclarationVersion, NameSolverStrings.XmlDeclarationEncoding, null);		
					denyXmlDocument.AppendChild(declaration);

					XmlElement root = denyXmlDocument.CreateElement(XML_SLDENY_ROOT_TAG);
					if (root == null || denyXmlDocument.AppendChild(root) == null)
						return false;
				}
				else
				{
					denyXmlDocument.Load(fileInfo.FullName);
					if (denyXmlDocument.DocumentElement == null || String.Compare(denyXmlDocument.DocumentElement.Name, XML_SLDENY_ROOT_TAG) != 0)
						return false;

					// Vedo se esiste già l'istruzione di rimozione del comando da menù
					if (ExistDeniedAccessNode(denyXmlDocument, aNameSpaceTextWithoutType, aSecuredCommandType))
						return false;
				}
				
				XmlElement deniedAccessNode = denyXmlDocument.CreateElement(XML_SLDENY_DENIED_ACCESS_TAG);

				deniedAccessNode.SetAttribute(XML_SLDENY_COMMAND_TYPE_ATTRIBUTE, aSecuredCommandType.ToString());
				deniedAccessNode.InnerText = aNameSpaceTextWithoutType;

				denyXmlDocument.DocumentElement.AppendChild(deniedAccessNode);

				denyXmlDocument.Save(fileInfo.FullName);

				return true;
			}
			catch(Exception e) 
			{
				Debug.Fail("Exception raised in SLDenyFile.AddDeniedAccess: " + e.Message);
				return false;
			}		
		}

		//---------------------------------------------------------------------
		public bool RemoveDeniedAccess(string aNameSpaceTextWithoutType, SecuredCommandType aSecuredCommandType)
		{
			try
			{
				if 
					(
					fileInfo == null ||
					fileInfo.FullName == null ||
					fileInfo.FullName.Length == 0 ||
					!Path.IsPathRooted(fileInfo.FullName) ||
					!File.Exists(fileInfo.FullName) ||
					aNameSpaceTextWithoutType == null || 
					aNameSpaceTextWithoutType.Length == 0 || 
					aSecuredCommandType == SecuredCommandType.Undefined
					)
					return false;

				XmlDocument denyXmlDocument = new XmlDocument();
				
				denyXmlDocument.Load(fileInfo.FullName);

				// Vedo se esiste l'istruzione di rimozione del comando da menù che si vuole cancellare
				XmlElement deniedAccessNode = GetDeniedAccessNode(denyXmlDocument, aNameSpaceTextWithoutType, aSecuredCommandType);
				if (deniedAccessNode == null)
					return false;

				denyXmlDocument.DocumentElement.RemoveChild(deniedAccessNode);

				if (denyXmlDocument.DocumentElement.HasChildNodes)
					denyXmlDocument.Save(fileInfo.FullName);
				else
					File.Delete(fileInfo.FullName);
				
				return true;
			}
			catch(Exception e) 
			{
				Debug.Fail("Exception raised in SLDenyFile.RemoveDeniedAccess: " + e.Message);
				return false;
			}		
		}

        //---------------------------------------------------------------------
        public void RemoveAllDeniedAccesses(SecuredCommandType aSecuredCommandType)
        {
            try
            {
                if
                    (
                    fileInfo == null ||
                    fileInfo.FullName == null ||
                    fileInfo.FullName.Length == 0 ||
                    !Path.IsPathRooted(fileInfo.FullName) ||
                    !File.Exists(fileInfo.FullName) ||
                    aSecuredCommandType == SecuredCommandType.Undefined
                    )
                    return;

                XmlDocument denyXmlDocument = new XmlDocument();

                denyXmlDocument.Load(fileInfo.FullName);

                string xpathExpression = "child::" + XML_SLDENY_DENIED_ACCESS_TAG + "[@" + XML_SLDENY_COMMAND_TYPE_ATTRIBUTE + " = '" + aSecuredCommandType.ToString() + "']";

                XmlNodeList deniedAccessNodes = denyXmlDocument.DocumentElement.SelectNodes(xpathExpression);

                if (deniedAccessNodes != null && deniedAccessNodes.Count > 0)
                {
                    foreach(XmlNode aNodeToRemove in deniedAccessNodes)
                    {
                        if (aNodeToRemove == null || !(aNodeToRemove is XmlElement))
                            continue;

                        denyXmlDocument.DocumentElement.RemoveChild(aNodeToRemove);
                    }
                }

                if (denyXmlDocument.DocumentElement.HasChildNodes)
                    denyXmlDocument.Save(fileInfo.FullName);
                else
                    File.Delete(fileInfo.FullName);
            }
            catch (Exception e)
            {
                Debug.Fail("Exception raised in SLDenyFile.RemoveAllDeniedAccesses: " + e.Message);
            }
        }
        
        //---------------------------------------------------------------------
		public void CleanDeniedAccesses
			(
			MenuXmlParser					aMenuXmlParser,
			MenuLoader.CommandsTypeToLoad	commandTypesToClean
			)
		{
			try
			{
				if 
					(
					aMenuXmlParser == null ||
					commandTypesToClean == MenuLoader.CommandsTypeToLoad.Undefined ||
					fileInfo == null ||
					fileInfo.FullName == null ||
					fileInfo.FullName.Length == 0 ||
					!Path.IsPathRooted(fileInfo.FullName) ||
					!File.Exists(fileInfo.FullName)
					)
					return;

				XmlDocument denyXmlDocument = new XmlDocument();
			
				denyXmlDocument.Load(fileInfo.FullName);

				if (denyXmlDocument.DocumentElement == null || String.Compare(denyXmlDocument.DocumentElement.Name, XML_SLDENY_ROOT_TAG) != 0)
					return;

				if ((commandTypesToClean & MenuLoader.CommandsTypeToLoad.Form) == MenuLoader.CommandsTypeToLoad.Form)
					CleanDeniedAccessesByType(aMenuXmlParser, denyXmlDocument, SecuredCommandType.Form);
				if ((commandTypesToClean & MenuLoader.CommandsTypeToLoad.Batch) == MenuLoader.CommandsTypeToLoad.Batch)
					CleanDeniedAccessesByType(aMenuXmlParser, denyXmlDocument, SecuredCommandType.Batch);
				if ((commandTypesToClean & MenuLoader.CommandsTypeToLoad.Report) == MenuLoader.CommandsTypeToLoad.Report)
					CleanDeniedAccessesByType(aMenuXmlParser, denyXmlDocument, SecuredCommandType.Report);
				if ((commandTypesToClean & MenuLoader.CommandsTypeToLoad.Function) == MenuLoader.CommandsTypeToLoad.Function)
					CleanDeniedAccessesByType(aMenuXmlParser, denyXmlDocument, SecuredCommandType.Function);
				if ((commandTypesToClean & MenuLoader.CommandsTypeToLoad.ExcelItem) == MenuLoader.CommandsTypeToLoad.ExcelItem)
				{
					CleanDeniedAccessesByType(aMenuXmlParser, denyXmlDocument, SecuredCommandType.ExcelDocument);
					CleanDeniedAccessesByType(aMenuXmlParser, denyXmlDocument, SecuredCommandType.ExcelTemplate);
				}
				if ((commandTypesToClean & MenuLoader.CommandsTypeToLoad.WordItem) == MenuLoader.CommandsTypeToLoad.WordItem)
				{
					CleanDeniedAccessesByType(aMenuXmlParser, denyXmlDocument, SecuredCommandType.WordDocument);
					CleanDeniedAccessesByType(aMenuXmlParser, denyXmlDocument, SecuredCommandType.WordTemplate);
				}
			}
			catch(Exception e) 
			{
				Debug.Fail("Exception raised in SLDenyFile.CleanDeniedAccesses: " + e.Message);
			}		
		}
		
		#endregion // SLDenyFile public methods
	}
}

using System.Collections;
namespace Microarea.TaskBuilderNet.Interfaces
{
	public interface IMenuXmlNodeCollection
	{
		/*int Add(IMenuXmlNode aNodeToAdd);
		void AddRange(IMenuXmlNodeCollection aNodeCollectionToAdd);
		void Clear();
		bool Contains(IMenuXmlNode aNodeToSearch);
		bool ContainsSameNode(IMenuXmlNode aNodeToSearch);
		IMenuXmlNodeCollection GetBatchNodes();
		IMenuXmlNodeCollection GetDocumentNodes();
		IMenuXmlNodeCollection GetExeNodes();
		IMenuXmlNodeCollection GetExternalItemNodes();
		IMenuXmlNodeCollection GetFunctionNodes();
		IMenuXmlNodeCollection GetOfficeItemNodes();
		IMenuXmlNodeCollection GetReportNodes();
		IMenuXmlNodeCollection GetTextNodes();
		int IndexOf(IMenuXmlNode aNodeToSearch);
		void Insert(IMenuXmlNode beforeNode, IMenuXmlNode aNodeToInsert);
		void Insert(int index, IMenuXmlNode aNodeToInsert);
		IMenuXmlNodeCollection Intersect(IMenuXmlNodeCollection otherNodes);
		void Remove(IMenuXmlNode aNodeToRemove);
		void RemoveAt(int index);
		void SortByTitles();
		IMenuXmlNodeCollection Subtract(IMenuXmlNodeCollection otherNodes);
		IMenuXmlNode this[int index] { get; }
		IMenuXmlNodeCollection Union(IMenuXmlNodeCollection otherNodes);*/
	}
	public interface IMenuXmlNode
	{
		/*
		bool AccessAllowedState { get; set; }
		bool AccessDeniedState { get; set; }
		bool AccessInUnattendedModeAllowedState { get; set; }
		bool AccessPartiallyAllowedState { get; set; }
		bool ApplyStateToAllDescendants { get; set; }
		System.Collections.ArrayList ArgumentsItems { get; }
		string ArgumentsOuterXml { get; }
		bool CheckMagicDocumentsInstallation();
		bool CheckMagicDocumentsMacroSupport();
		void Clear();
		System.Collections.ArrayList CommandItems { get; }
		bool CreateArgumentsChild(string aArgumentsOuterXml);
		bool CreateDescriptionChild(string descriptionText);
		bool CreateGuidChild(string guidText);
		bool CreateObjectChild(string cmdText);
		bool CreateTitleChild(string titleText, string originalTitleText);
		string Description { get; set; }
		string DifferentCommandImage { get; }
		bool ExistsOfficeMenuCommand(string aModuleName, string aOfficeFileName);
		bool ExistsReportMenuCommand(string aModuleName, string aReportFileName);
		string ExternalDescription { get; set; }
		int ExternalItemImageIndex { get; set; }
		string ExternalItemType { get; set; }
		string GetActionApplicationImageLink();
		string GetActionApplicationName();
		string GetActionApplicationTitle();
		System.Collections.ArrayList GetActionCommandItems();
		string GetActionCommandPath();
		System.Collections.ArrayList GetActionCommandShortcutNodes();
		IMenuXmlNode GetActionCommandsNode();
		string GetActionGroupImageLink();
		string GetActionGroupName();
		string GetActionGroupTitle();
		string GetActionMenuNamesPath();
		IMenuXmlNode GetActionMenuPathNode();
		string GetActionMenuTitlesPath();
		IMenuXmlNode GetActionNode();
		string GetActivationAttribute();
		IMenuXmlNodeCollection GetAllCommandDescendants();
		System.Collections.ArrayList GetApplicationEquivalentCommandsList(IMenuXmlNode aCommandNodeToFind);
		System.Collections.ArrayList GetApplicationEquivalentExternalItemsList(IMenuXmlNode aExternalItemNodeToFind);
		System.Collections.ArrayList GetApplicationEquivalentExternalItemsList(string aExternalItemType, string aExternalItemObject);
		IMenuXmlNode GetApplicationNode();
		IMenuXmlNode GetApplicationNodeByName(string aApplicationName);
		string GetArgumentName();
		string GetArgumentPassingMode();
		string GetArgumentTitle();
		string GetArgumentType();
		string GetArgumentValue();
		IMenuXmlNodeCollection GetBatchDescendantNodes();
		IMenuXmlNodeCollection GetBatchDescendantNodesByObjectName(string aCommandName);
		IMenuXmlNodeCollection GetCommandDescendantNodesByObjectName(string aCommandName);
		IMenuXmlNode GetCommandNodeByObjectName(string aCommandName);
		IMenuXmlNode GetCommandNodeByTitle(string aCommandTitle);
		System.Collections.ArrayList GetCommandsHierarchyList();
		string GetCommandsHierarchyTitlesString();
		IMenuXmlNode GetCommandShortcutsNode();
		IMenuXmlNodeCollection GetDocumentDescendantNodes();
		IMenuXmlNodeCollection GetDocumentDescendantNodesByObjectName(string aCommandName);
		IMenuXmlNode GetDocumentShortcutNodeByName(string aDocumentShortcutName);
		IMenuXmlNodeCollection GetExeDescendantNodes();
		IMenuXmlNodeCollection GetExeDescendantNodesByObjectName(string aCommandName);
		IMenuXmlNodeCollection GetExternalItemDescendantNodes();
		IMenuXmlNodeCollection GetExternalItemDescendantNodesByObjectName(string aCommandName);
		System.Collections.ArrayList GetExternalItemDescendantNodesByTypeAttribute(string typeAttribute);
		IMenuXmlNode GetFirstLevelAncestorCommand();
		IMenuXmlNode GetFirstLevelAncestorMenu();
		IMenuXmlNodeCollection GetFunctionDescendantNodes();
		IMenuXmlNodeCollection GetFunctionDescendantNodesByObjectName(string aCommandName);
		IMenuXmlNode GetGroupNode();
		IMenuXmlNode GetGroupNodeByName(string aGroupName);
		string GetImageFileName();
		string GetInsertAfterAttribute();
		string GetInsertBeforeAttribute();
		System.Collections.ArrayList GetMenuHierarchyList();
		string GetMenuHierarchyTitlesString();
		string GetMenuName();
		IMenuXmlNode GetMenuNodeByName(string aMenuName);
		IMenuXmlNode GetMenuNodeByTitle(string aMenuTitle);
		IMenuXmlNode GetMenuRoot();
		IMenuXmlNodeCollection GetOfficeItemDescendantNodes();
		System.Collections.ArrayList GetOfficeItemDescendantNodesByApplicationAttribute(string applicationAttribute);
		IMenuXmlNodeCollection GetOfficeItemDescendantNodesByObjectName(string aCommandName);
		string[] GetOtherTitles();
		IMenuXmlNode GetParentMenu();
		string GetParentMenuName();
		string GetParentMenuTitle();
		IMenuXmlNode GetParentNode();
		IMenuXmlNodeCollection GetReportDescendantNodes();
		IMenuXmlNodeCollection GetReportDescendantNodesByObjectName(string reportName);
		IMenuXmlNode GetReportShortcutNodeByName(string aReportShortcutName);
		string GetShortcutArguments();
		string GetShortcutCommand();
		string GetShortcutCommandSubType();
		string GetShortcutDescription();
		string GetShortcutImageLink();
		string GetShortcutName();
		IMenuXmlNode GetShortcutNodeByNameAndType(string aShortcutName, string aShortcutType);
		string GetShortcutTypeXmlTag();
		IMenuXmlNodeCollection GetTextDescendantNodes();
		Guid GuidValue { get; }
		bool HasAllCommandDescendantsInProtectedState { get; }
		bool HasAllCommandDescendantsInTracedState { get; }
		bool HasAtLeastOneCommandDescendantInProtectedState { get; }
		bool HasAtLeastOneCommandDescendantInTracedState { get; }
		bool HasChildNodes { get; }
		bool HasCommandChildNodes();
		bool HasMenuChildNodes();
		bool HasMenuCommandImagesToSearch { get; }
		bool HasNoCommandDescendantsInProtectedState { get; }
		bool HasNoCommandDescendantsInTracedState { get; }
		bool HasNoEmptyGuid { get; }
		bool HasOtherTitles { get; }
		bool HasSchedulableDescendantNodes();
		string ImageLink { get; set; }
		IMenuXmlNode InsertXmlNodeChild(System.Xml.XmlNode aXmlNodeToInsert);
		bool IsAction { get; }
		bool IsActionCommandsNode { get; }
		bool IsAddAction { get; }
		bool IsApplication { get; }
		bool IsArgument { get; }
		bool IsArgumentsNode { get; }
		bool IsBatchShortcut { get; }
		bool IsCommand { get; }
		bool IsCommandImageToSearch { get; set; }
		bool IsCommandShortcutsNode { get; }
		bool IsDocumentShortcut { get; }
		bool IsExcelDocument { get; }
		bool IsExcelDocumentShortcut { get; }
		bool IsExcelItem { get; }
		bool IsExcelItemShortcut { get; }
		bool IsExcelTemplate { get; }
		bool IsExcelTemplateShortcut { get; }
		bool IsExeShortcut { get; }
		bool IsExternalItem { get; }
		bool IsExternalItemShortcut { get; }
		bool IsFunctionShortcut { get; }
		bool IsGroup { get; }
		bool IsMenu { get; }
		bool IsMenuActions { get; }
		bool IsOfficeDocument { get; }
		bool IsOfficeDocumentShortcut { get; }
		bool IsOfficeItem { get; }
		bool IsOfficeItemShortcut { get; }
		bool IsOfficeTemplate { get; }
		bool IsOfficeTemplateShortcut { get; }
		bool IsRemoveAction { get; }
		bool IsReportShortcut { get; }
		bool IsRoot { get; }
		bool IsRunBatch { get; }
		bool IsRunBatchFunction { get; }
		bool IsRunDocument { get; }
		bool IsRunDocumentFunction { get; }
		bool IsRunExecutable { get; }
		bool IsRunExecutableFunction { get; }
		bool IsRunFunction { get; }
		bool IsRunReport { get; }
		bool IsRunReportFunction { get; }
		bool IsRunText { get; }
		bool IsRunTextFunction { get; }
		bool IsSameApplicationAs(IMenuXmlNode aNodeToCompare);
		bool IsSameCommandAs(IMenuXmlNode aNodeToCompare);
		bool IsSameGroupAs(IMenuXmlNode aNodeToCompare);
		bool IsSameMenuAs(IMenuXmlNode aNodeToCompare);
		bool IsSameMenuNodeAs(IMenuXmlNode aNodeToCompare);
		bool IsSameShortcutAs(IMenuXmlNode aNodeToCompare);
		bool IsShortcut { get; }
		bool IsStandardBatch { get; }
		bool IsTextShortcut { get; }
		bool IsTitleLocalizable { get; set; }
		bool IsWizardBatch { get; }
		bool IsWordDocument { get; }
		bool IsWordDocumentShortcut { get; }
		bool IsWordItem { get; }
		bool IsWordItemShortcut { get; }
		bool IsWordTemplate { get; }
		bool IsWordTemplateShortcut { get; }
		string ItemObject { get; }
		System.Collections.ArrayList MenuActionsItems { get; }
		string MenuGuid { get; set; }
		System.Collections.ArrayList MenuItems { get; }
		string Name { get; }
		IMenuXmlNode NextSibling { get; }
		System.Xml.XmlNode Node { get; set; }
		string OriginalTitle { get; set; }
		System.Xml.XmlDocument OwnerDocument { get; }
		bool ProtectedState { get; set; }
		bool RemoveChild(IMenuXmlNode aMenuNodeToRemove);
		bool ReplaceArguments(string aArgumentsOuterXml);
		bool ReplaceDescription(string newDescription);
		void ReplaceShortcutNodeData(IMenuXmlNode aShortcutNodeToCopy);
		bool ReplaceTitle(string newTitle);
		DateTime ReportFileCreationTime { get; set; }
		DateTime ReportFileLastWriteTime { get; set; }
		IMenuXmlNodeCollection SelectMenuNodes(string xpathExpression);
		IMenuXmlNode SelectSingleMenuNode(string xpathExpression);
		void SetImageFileName(string aImageFileName);
		bool SetOtherTitle(string anotherTitle);
		System.Collections.ArrayList ShortcutsItems { get; }
		string Title { get; set; }
		bool TracedState { get; set; }
		bool UserOfficeFilesGroup { get; set; }
		bool UserReportsGroup { get; set; }*/
		bool HasExcelItemsDescendantsNodes { get; }
		bool HasCommandDescendantsNodes { get; }
		bool HasApplicationChildNodes { get; }
		string GetGroupName();
		string GetApplicationName();
		ArrayList GroupItems { get; }
		string GetNameAttribute();
		string Title { get; set; }
		bool HasOfficeItemsDescendantsNodes { get; }
		bool HasWordItemsDescendantsNodes { get; }
		IMenuXmlNode GetMenuActionsNode();

		ArrayList ApplicationsItems { get; }
	}

	public interface IMenuXmlParser
	{
		/*int AddCommandImageInfo(IMenuXmlNode aCommandNode, string aFileName);
		IMenuXmlNode AddExternalItemNodeToExistingNode(IMenuXmlNode parentNode, string itemTitle, string itemType, string itemObject, string itemGuidText, string arguments, int imageIndex);
		IMenuXmlNode AddMenuNodeToExistingNode(IMenuXmlNode parentNode, IMenuXmlNode aMenuNodeToAdd, bool deep);
		int ApplicationsCount { get; }
		bool AreGroupsPresent { get; }
		IMenuXmlNode CommandShortcutsNode { get; }
		void CopyImageInfos(IMenuXmlParser aParser);
		IMenuXmlNode CreateApplicationNode(string applicationName, string appTitle, string originalAppTitle);
		IMenuXmlNode CreateApplicationNode(string applicationName, string appTitle, string originalAppTitle, IMenuXmlNode referenceAppNode, bool insertBeforeRef);
		IMenuXmlNode CreateApplicationNode(string applicationName, string appTitle);
		IMenuXmlNode CreateBatchCommandNode(IMenuXmlNode parentNode, string commandTitle, string originalCommandTitle, string commandDescription, string command, string arguments, IMenuXmlNode referenceCommandNode, bool insertBeforeRef);
		IMenuXmlNode CreateBatchCommandNode(IMenuXmlNode parentNode, string commandTitle, string commandDescription, string command, string arguments);
		IMenuXmlNode CreateBatchCommandNode(IMenuXmlNode parentNode, string commandTitle, string originalCommandTitle, string commandDescription, string command, string arguments);
		bool CreateCommandShortcutsNode();
		IMenuXmlNode CreateDocumentCommandNode(IMenuXmlNode parentNode, string commandTitle, string commandDescription, string command, string arguments);
		IMenuXmlNode CreateDocumentCommandNode(IMenuXmlNode parentNode, string commandTitle, string originalCommandTitle, string commandDescription, string command, string arguments);
		IMenuXmlNode CreateDocumentCommandNode(IMenuXmlNode parentNode, string commandTitle, string originalCommandTitle, string commandDescription, string command, string arguments, IMenuXmlNode referenceCommandNode, bool insertBeforeRef);
		IMenuXmlNode CreateFunctionCommandNode(IMenuXmlNode parentNode, string commandTitle, string originalCommandTitle, string commandDescription, string command, string arguments, IMenuXmlNode referenceCommandNode, bool insertBeforeRef);
		IMenuXmlNode CreateFunctionCommandNode(IMenuXmlNode parentNode, string commandTitle, string originalCommandTitle, string commandDescription, string command, string arguments);
		IMenuXmlNode CreateFunctionCommandNode(IMenuXmlNode parentNode, string commandTitle, string commandDescription, string command, string arguments);
		IMenuXmlNode CreateGroupNode(IMenuXmlNode applicationNode, string groupName, string groupTitle, string originalGroupTitle);
		IMenuXmlNode CreateGroupNode(IMenuXmlNode applicationNode, string groupName, string groupTitle);
		IMenuXmlNode CreateGroupNode(IMenuXmlNode applicationNode, string groupName, string groupTitle, string originalGroupTitle, IMenuXmlNode referenceGroupNode, bool insertBeforeRef);
		IMenuXmlNode CreateGroupNode(IMenuXmlNode applicationNode, string groupName, string groupTitle, string originalGroupTitle, string referenceGroupNodeName, bool insertBeforeRef);
		IMenuXmlNode CreateGroupNodeAfterAll(IMenuXmlNode applicationNode, string groupName, string groupTitle);
		IMenuXmlNode CreateGroupNodeAfterAll(IMenuXmlNode applicationNode, string groupName, string groupTitle, string originalGroupTitle);
		bool CreateMenuActionsNode();
		IMenuXmlNode CreateMenuNode(IMenuXmlNode parentNode, string menuName, string menuTitle, string originalMenuTitle, IMenuXmlNode referenceMenuNode, bool insertBeforeRef);
		IMenuXmlNode CreateMenuNode(IMenuXmlNode parentNode, string menuName);
		IMenuXmlNode CreateMenuNode(IMenuXmlNode parentNode, string menuName, string menuTitle);
		IMenuXmlNode CreateMenuNode(IMenuXmlNode parentNode, string menuName, string menuTitle, string originalMenuTitle);
		IMenuXmlNode CreateOfficeFileCommandNode(IMenuXmlNode parentNode, string aModuleName, string officeFullFilename);
		IMenuXmlNode CreateReportCommandNode(IMenuXmlNode parentNode, string commandTitle, string originalCommandTitle, string commandDescription, string command, string arguments);
		IMenuXmlNode CreateReportCommandNode(IMenuXmlNode parentNode, string commandTitle, string commandDescription, string command, string arguments);
		IMenuXmlNode CreateReportCommandNode(IMenuXmlNode parentNode, string commandTitle, string originalCommandTitle, string commandDescription, string command, string arguments, IMenuXmlNode referenceCommandNode, bool insertBeforeRef);
		bool CreateRoot();
		IMenuXmlNode CurrentApplication { get; }
		IMenuXmlNode CurrentGroup { get; }
		IMenuXmlNode FindMatchingNode(IMenuXmlNode aNode);
		IMenuXmlNodeCollection GetAllCommandDescendants(string aApplicationName);
		IMenuXmlNodeCollection GetAllCommandDescendants(string aApplicationName, string aGroupName);
		IMenuXmlNodeCollection GetAllCommands();
		IMenuXmlNode GetApplicationNodeByName(string applicationName);
		IMenuXmlNodeCollection GetBatchNodesByObjectName(string aCommandName);
		IMenuXmlNodeCollection GetCommandDescendantNodesByObjectName(string aCommandName);
		string GetCommandImageFileName(string aApplicationName, string aCommandItemObject);
		string GetCurrentApplicationName();
		string GetCurrentApplicationTitle();
		string GetCurrentGroupName();
		string GetCurrentGroupTitle();
		IMenuXmlNodeCollection GetDocumentNodesByObjectName(string aCommandName);
		IMenuXmlNode GetDocumentShortcutNodeByName(string aDocumentShortcutName);
		System.Collections.ArrayList GetEquivalentCommandsList(IMenuXmlNode aCommandNodeToFind);
		System.Collections.ArrayList GetEquivalentExternalItemsList(string aExternalItemType, string aExternalItemObject);
		System.Collections.ArrayList GetEquivalentExternalItemsList(IMenuXmlNode aExternalItemNodeToFind);
		IMenuXmlNodeCollection GetExeNodesByObjectName(string aCommandName);
		IMenuXmlNode GetFirstApplicationNode();
		IMenuXmlNode GetFirstGroupNode();
		IMenuXmlNodeCollection GetFunctionNodesByObjectName(string aCommandName);
		IMenuXmlNode GetNextApplicationNode();
		IMenuXmlNode GetNextGroupNode();
		string GetNodeImageFileName(IMenuXmlNode aNode);
		IMenuXmlNodeCollection GetReportNodesByObjectName(string reportName);
		IMenuXmlNode GetReportShortcutNodeByName(string aReportShortcutName);
		int GroupsCount { get; }
		bool HasApplicationChildNodes { get; }
		System.Collections.ArrayList LoadErrorMessages { get; }
		System.Collections.ArrayList MenuActionsItems { get; }
		IMenuXmlNode MenuActionsNode { get; }
		bool MoveToNextApplicationNode();
		bool MoveToNextGroupNode();
		void RemoveApplicationImageInfo(string aApplicationName, string aFileName);
		void RemoveCommandImageInfo(string aApplicationName, string aCommandObject, string aFileName);
		bool RemoveCommandNodeFromExistingNode(IMenuXmlNode parentNode, IMenuXmlNode commandToRemove);
		void RemoveGroupImageInfo(string aApplicationName, string aGroupName, string aFileName);
		bool RemoveNode(IMenuXmlNode aMenuNodeToRemove);
		bool RemoveShortcutNode(IMenuXmlNode shortcutToRemove);
		bool SeekToFirstApplicationNode();
		bool SeekToFirstGroupNode();
		bool SetApplication(IMenuXmlNode applicationNode);
		bool SetApplication(string applicationName);
		bool SetGroup(IMenuXmlNode groupNode);
		bool SetGroup(string groupName);
		System.Collections.ArrayList ShortcutsItems { get; }
		System.Xml.XmlDocument XmlDocument { get; }*/
		bool HasExcelItemsDescendantsNodes { get; }
		string GetGroupImageFileName(string aApplicationName, string aGroupName);
		IMenuXmlNode Root { get; }
		bool HasCommandDescendantsNodes { get; }
		string GetApplicationImageFileName(string aApplicationName);
		bool HasOfficeItemsDescendantsNodes { get; }
		bool HasWordItemsDescendantsNodes { get; }


	}
}

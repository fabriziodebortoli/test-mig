using System;
using System.Data.SqlClient;
using System.IO;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;


namespace Microarea.TaskBuilderNet.Core.SecurityLayer.SecurityLightObjects
{
	public enum SecuredCommandType
	{
		Undefined		= 0x0000,
		Form			= 0x0001,
		Batch			= 0x0002,
		Report			= 0x0003,
		ExcelDocument	= 0x0004,
		ExcelTemplate	= 0x0005,
		WordDocument	= 0x0006,
		WordTemplate	= 0x0007,
		Function		= 0x0008 // "Function" command type protection is supported starting from the 2.4.2 version
	};
	
	/// <summary>
	/// Summary description for SecuredCommand.
	/// </summary>
	public class SecuredCommand : SecuredCommandDBInfo
	{
		#region SecuredCommand private data members

		private SecurityLightMenuLoader menuLoader = null;

		#endregion // SecuredCommand private data members

		//---------------------------------------------------------------------
		public SecuredCommand
			(
			SecurityLightMenuLoader	aMenuLoader, 
			string					aObjectNameSpace, 
			SecuredCommandType		aObjectType, 
			SqlConnection			aConnection, 
			string[]				aUserNamesToSkipList
			)
			:
			base(aObjectNameSpace, aObjectType, aConnection, aUserNamesToSkipList)
		{
			menuLoader = aMenuLoader;
		}
		
		//---------------------------------------------------------------------
		public SecuredCommand
			(
			SecurityLightMenuLoader aMenuLoader, 
			MenuXmlNode				aCommandNode, 
			SqlConnection			aConnection, 
			string[]				aUserNamesToSkipList
			)
			:
			this
			(
			aMenuLoader,
			(aCommandNode != null  && aCommandNode.IsCommand) ? aCommandNode.ItemObject : String.Empty,
			(aCommandNode != null  && aCommandNode.IsCommand) ? GetSecuredCommandType(aCommandNode) : SecuredCommandType.Undefined, 
			aConnection,
			aUserNamesToSkipList
			)
		{
			if (aMenuLoader != null && aCommandNode != null && (aMenuLoader.CurrentMenuParser == null || aCommandNode.OwnerDocument != aMenuLoader.CurrentMenuParser.MenuXmlDoc))
				throw new ArgumentException("Invalid arguments passed to the SecurityManager constructor.");
		}
		
		#region SecuredCommand protected overridden methods

		//---------------------------------------------------------------------
		protected override void OnAccessDenied(int aCompanyId, int aUserId)
		{
			// Gestione del file di menù per la scomparsa da menù delle voci relative al comando
			if (menuLoader == null || menuLoader.PathFinder == null)
				return;

			// Quando si vieta l’accesso ad un certo comando applicativo, è necessario rimuovere
			// automaticamente dal menù tutte le voci riferite allo stesso oggetto, cioè che puntino 
			// al medesimo namespace. Infatti, nei file SLDeny.menu devo andare ad inserire l'istruzione
			// che provoca la rimozione dal menù caricato di tutti i comandi della tipologia corrente e 
			// che fanno riferimento allo stesso namespace.
			FileInfo[] customMenuFiles = SecurityLightManager.GetAllCustomMenuFiles(menuLoader.PathFinder, aCompanyId, aUserId, SystemDBConnection, this.UserNamesToSkip);
			if (customMenuFiles == null || customMenuFiles.Length == 0)
				return;

			foreach (FileInfo aCustomFileInfo in customMenuFiles)
			{
				if (aCustomFileInfo == null)
					continue;

				SLDenyFile denyFile = new SLDenyFile(aCustomFileInfo);
				denyFile.AddDeniedAccess(this.NameSpace, this.Type);
			}
		}

		//---------------------------------------------------------------------
		protected override void OnAccessAllowed(int aCompanyId, int aUserId)
		{
			//Gestione dei file di menù per la scomparsa da menù delle voci relative al comando
			if (menuLoader == null || menuLoader.PathFinder == null)
				return;

            FileInfo[] customMenuFiles = SecurityLightManager.GetAllExistingCustomMenuFiles(menuLoader.PathFinder, aCompanyId, aUserId, SystemDBConnection, this.UserNamesToSkip);
			if (customMenuFiles == null || customMenuFiles.Length == 0)
				return;

			foreach (FileInfo aCustomFileInfo in customMenuFiles)
			{
				if (aCustomFileInfo == null)
					continue;

				SLDenyFile denyFile = new SLDenyFile(aCustomFileInfo);
				
				denyFile.RemoveDeniedAccess(this.NameSpace, this.Type);
			}
		}

		#endregion // SecuredCommand protected overridden methods

		#region SecuredCommand public overridden methods

		//---------------------------------------------------------------------
		public override bool DenyAccess(int aCompanyId, int aUserId)
		{
			if (this.NameSpace == null || this.NameSpace.Length == 0 || this.Type == SecuredCommandType.Undefined)
				return false;

			// Se l'accesso al comando risulta già vietato non occorre fare nulla
			if (aCompanyId != -1 && aUserId != -1 && IsAccessDenied(aCompanyId, aUserId))
				return true;

			return base.DenyAccess(aCompanyId, aUserId);
		}

		//---------------------------------------------------------------------
		public override bool AllowAccess(int aCompanyId, int aUserId)
		{
			if (this.NameSpace == null || this.NameSpace.Length == 0 || this.Type == SecuredCommandType.Undefined)
				return false;

			// Se l'accesso al comando non risulta vietato non occorre fare nulla
			if (aCompanyId != -1 && aUserId != -1 && !IsAccessDenied(aCompanyId, aUserId))
				return true;

			return base.AllowAccess(aCompanyId, aUserId);
		}

		//---------------------------------------------------------------------
		public override bool IsAccessInUnattendedModeDefined(int aCompanyId, int aUserId)
		{
			if (this.NameSpace == null || this.NameSpace.Length == 0 || this.Type == SecuredCommandType.Undefined)
				return false;

			return base.IsAccessInUnattendedModeDefined(aCompanyId, aUserId);
		}

		//---------------------------------------------------------------------
		public override bool SetAccessInUnattendedMode(int aCompanyId, int aUserId, bool allowed)
		{
			if (this.NameSpace == null || this.NameSpace.Length == 0 || this.Type == SecuredCommandType.Undefined)
				return false;

			// Se l'accesso al comando non risulta vietato non occorre fare nulla
			if (aCompanyId != -1 && aUserId != -1 && !IsAccessDenied(aCompanyId, aUserId))
				return true;

			return base.SetAccessInUnattendedMode(aCompanyId, aUserId, allowed);
		}

		#endregion // SecuredCommand public overridden methods

		#region SecuredCommand public static methods
		
		//-----------------------------------------------------------------------
		public static bool IsDeniableCommand(MenuXmlNode aMenuNode)
		{
			return 
				(
				aMenuNode != null && 
				aMenuNode.IsCommand && 
				(aMenuNode.IsRunDocument || aMenuNode.IsRunBatch || aMenuNode.IsRunReport || aMenuNode.IsRunFunction || aMenuNode.IsOfficeItem)
				);
		}
		
		//---------------------------------------------------------------------
		public static SecuredCommandType GetSecuredCommandType(MenuXmlNode aMenuNode)
		{
			if (aMenuNode.IsRunDocument)
				return SecuredCommandType.Form;

			if (aMenuNode.IsRunBatch)
				return SecuredCommandType.Batch;

			if (aMenuNode.IsRunReport)
				return SecuredCommandType.Report;

			if (aMenuNode.IsRunFunction)
				return SecuredCommandType.Function;

			if (aMenuNode.IsOfficeItem)
			{
                if (aMenuNode.IsExcelDocument || aMenuNode.IsExcelDocument2007)
					return SecuredCommandType.ExcelDocument;
                if (aMenuNode.IsExcelTemplate || aMenuNode.IsExcelTemplate2007)
					return SecuredCommandType.ExcelTemplate;
                if (aMenuNode.IsWordDocument || aMenuNode.IsWordDocument2007)
					return SecuredCommandType.WordDocument;
                if (aMenuNode.IsWordTemplate || aMenuNode.IsWordTemplate2007)
					return SecuredCommandType.WordTemplate;
			}
			return SecuredCommandType.Undefined;
		}

		//---------------------------------------------------------------------
		public static MenuLoader.CommandsTypeToLoad GetMenuCommandTypeToLoad(SecuredCommandType aSecuredCommandType)
		{
			switch (aSecuredCommandType)
			{
				case SecuredCommandType.Form:
					return MenuLoader.CommandsTypeToLoad.Form;

				case SecuredCommandType.Batch:
					return MenuLoader.CommandsTypeToLoad.Batch;

				case SecuredCommandType.Report:
					return MenuLoader.CommandsTypeToLoad.Report;

				case SecuredCommandType.Function:
					return MenuLoader.CommandsTypeToLoad.Function;

				case SecuredCommandType.ExcelDocument:
				case SecuredCommandType.ExcelTemplate:
                     return MenuLoader.CommandsTypeToLoad.ExcelItem;

				case SecuredCommandType.WordDocument:
				case SecuredCommandType.WordTemplate:
                     return MenuLoader.CommandsTypeToLoad.WordItem;

				default:
					break;
			}
			
			return MenuLoader.CommandsTypeToLoad.Undefined;
		}
		
		#endregion // SecuredCommand public static methods

	}
}

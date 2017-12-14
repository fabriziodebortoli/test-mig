
namespace Microarea.Console.Core.SecurityLibrary
{
	//=========================================================================
    public class GrantsString
	{
		public static string Grants { get { return SecurityLibraryStrings.Grants; } }
		//---------------------------------------------------------------------
		public static string GetGrantDescription (string grant)
		{
			if (string.Compare(grant, GrantConstString.Browse)==0)
				return SecurityLibraryStrings.Browse;
			if (string.Compare(grant, GrantConstString.CustomizeForm)==0)
				return SecurityLibraryStrings.CustomizeForm;
			if (string.Compare(grant, GrantConstString.Delete)==0)
				return SecurityLibraryStrings.Delete;
			if (string.Compare(grant, GrantConstString.Edit)==0)
				return SecurityLibraryStrings.Edit;
			if (string.Compare(grant, GrantConstString.EditQuery)==0)
				return SecurityLibraryStrings.EditQuery;
			if (string.Compare(grant, GrantConstString.Execute)==0)
				return SecurityLibraryStrings.Execute;
			if (string.Compare(grant, GrantConstString.Export)==0)
				return SecurityLibraryStrings.Export;
			if (string.Compare(grant, GrantConstString.Import)==0)
				return SecurityLibraryStrings.Import;
			if (string.Compare(grant, GrantConstString.New)==0)
				return SecurityLibraryStrings.New;
			if (string.Compare(grant, GrantConstString.SilentMode)==0)
				return SecurityLibraryStrings.SilentMode;
			if (string.Compare(grant, GrantConstString.DeleteRow)==0)
				return SecurityLibraryStrings.DeleteRow;
			if (string.Compare(grant, GrantConstString.AddRow)==0)
				return SecurityLibraryStrings.AddRow;
			if (string.Compare(grant, GrantConstString.ShowRowView)==0)
				return SecurityLibraryStrings.ShowRowView;
			if (string.Compare(grant, GrantConstString.ExtendedBrowse)==0)
				return SecurityLibraryStrings.ExtendedBrowse;
			return "";
		}
		//---------------------------------------------------------------------
	}
	//=========================================================================
	public class GrantConstString
	{
		public const string SecurityLibraryNamespace = "Microarea.Console.Core.SecurityLibrary";

		public const string Browse			= "Browse";
		public const string CustomizeForm   = "Customize Form";
		public const string Delete			= "Delete";
		public const string Edit			= "Edit";
		public const string EditQuery		= "Edit Query";
		public const string Execute			= "Execute";
		public const string Export			= "Export";
		public const string Import			= "Import";
		public const string New				= "New";
		public const string SilentMode		= "Silent Mode";
		public const string DeleteRow		= "Delete Row";
		public const string AddRow			= "Add Row";
		public const string ShowRowView		= "Show Row View";
		public const string ExtendedBrowse	= "Extended Browse";
	}
	//==========================================================================
	public class securityGrants
	{
		//Non Volevano cablare i nomi delle colonne della DataTable nel codice 
		//codì hanno fatto le costanti
		public const string Grant				= "Permesso";
		public const string Inherit				= "Eredita";
		public const string Role				= "Role";
		public const string Roles				= "Roles";
		public const string	User				= "User";
		public const string Total				= "Effettivo";
		public const string Assign				= "Assegna";
		public const string OldValue			= "VecchioValore";
		public const string GrantMask			= "MascheraGrant";
		public const string GrantDescription	= "DescrizionePermesso";
		public const string RoleDescription		= "DescrizioneRuolo";
		public const string RoleTableValue		= "ValoreRuoloTabRuoli";
	}

}

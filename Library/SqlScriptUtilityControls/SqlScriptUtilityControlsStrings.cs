using System;
using System.Resources;
using System.Reflection;

namespace Microarea.Library.SqlScriptUtilityControls
{
	/// <summary>
	/// Summary description for SqlScriptUtilityStrings.
	/// </summary>
	public class SqlScriptUtilityControlsStrings
	{
		private static ResourceManager rm = new ResourceManager("Microarea.Library.SqlScriptUtilityControls.Strings", Assembly.GetExecutingAssembly());

		public static string NewTableListBoxItem { get { return rm.GetString ("NewTableListBoxItem"); }}
		public static string NewFieldNameItem	 { get { return rm.GetString ("NewFieldNameItem"); }}

		public static string VoidFieldNameErrorMessage				{ get { return rm.GetString ("VoidFieldNameErrorMessage"); }}
		public static string FieldNameAlreadyUsedErrorMessage		{ get { return rm.GetString ("FieldNameAlreadyUsedErrorMessage"); }}			
		public static string InvalidFieldNameLengthErrorMessage		{ get { return rm.GetString ("InvalidFieldNameLengthErrorMessage"); }}	
		public static string MissingFieldTypeErrorMessage			{ get { return rm.GetString ("MissingFieldTypeErrorMessage"); }}	
		public static string InvalidFieldLengthErrorMessage			{ get { return rm.GetString ("InvalidFieldLengthErrorMessage"); }}	
		public static string MissingDefaultConstraintErrorMessage	{ get { return rm.GetString ("MissingDefaultConstraintErrorMessage"); }}	

		public static string VoidConstraintNameErrorMessage				{ get { return rm.GetString ("VoidConstraintNameErrorMessage"); }}
		public static string InvalidConstraintNameLengthErrorMessage	{ get { return rm.GetString ("InvalidConstraintNameLengthErrorMessage"); }}
		public static string MissingConstraintColumnsErrorMessage		{ get { return rm.GetString ("MissingConstraintColumnsErrorMessage"); }}
		public static string NullableFieldMessageFormat					{ get { return rm.GetString ("NullableFieldMessageFormat"); }}
	}
}

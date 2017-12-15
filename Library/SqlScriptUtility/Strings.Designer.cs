﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microarea.Library.SqlScriptUtility
{


	/// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microarea.Library.SqlScriptUtility.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The constraints check is failed because the number of constraints defined for Oracle differs from the number that has been defined for SQL Server..
        /// </summary>
        internal static string DifferentConstraintsNumberErrorMessage {
            get {
                return ResourceManager.GetString("DifferentConstraintsNumberErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The definition of the filenames for the scripts that are to be generated is not complete..
        /// </summary>
        internal static string MissingScriptFileNameErrorMessage {
            get {
                return ResourceManager.GetString("MissingScriptFileNameErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to It is impossible to retrieve the definition of the table &apos;{0}&apos; in the current script..
        /// </summary>
        internal static string MissingTableDefinitionErrorMessage {
            get {
                return ResourceManager.GetString("MissingTableDefinitionErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Errors have been encountered during the name checking of table &apos;{0}&apos;:.
        /// </summary>
        internal static string TableNameCheckingErrorsMessageFormat {
            get {
                return ResourceManager.GetString("TableNameCheckingErrorsMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 	The constraint name &apos;{0}&apos; is too long and therefore it has been truncated. The new constraint name is now &apos;{1}&apos;..
        /// </summary>
        internal static string TruncatedConstraintNameMessageFormat {
            get {
                return ResourceManager.GetString("TruncatedConstraintNameMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 	The index name &apos;{0}&apos; is too long and therefore it has been truncated. The new index name is now &apos;{1}&apos;..
        /// </summary>
        internal static string TruncatedIndexNameMessageFormat {
            get {
                return ResourceManager.GetString("TruncatedIndexNameMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 	The table name &apos;{0}&apos; is too long and therefore it has been truncated. The new table name is now &apos;{1}&apos;..
        /// </summary>
        internal static string TruncatedTableNameMessageFormat {
            get {
                return ResourceManager.GetString("TruncatedTableNameMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Errors have been encountered during the unparsing process..
        /// </summary>
        internal static string UnparsingErrorsEncounteredMessage {
            get {
                return ResourceManager.GetString("UnparsingErrorsEncounteredMessage", resourceCulture);
            }
        }
    }
}
﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TaskBuilderNetCore.Documents.Controllers {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class LoaderMessagesStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal LoaderMessagesStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("TaskBuilderNetCore.Documents.Controllers.LoaderMessagesStrings", typeof(LoaderMessagesStrings).Assembly);
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
        ///   Looks up a localized string similar to Assembly {0} not loaded.
        /// </summary>
        internal static string AssemblyNotLoaded {
            get {
                return ResourceManager.GetString("AssemblyNotLoaded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The component {0} class type has not been found.
        /// </summary>
        internal static string ClassNotFound {
            get {
                return ResourceManager.GetString("ClassNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &quot;The document {0} is not declared into DocumentObjects.xml&quot;.
        /// </summary>
        internal static string DocumentNotDeclared {
            get {
                return ResourceManager.GetString("DocumentNotDeclared", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Document {0} class cannot be loaded.
        /// </summary>
        internal static string DocumentNotLoaded {
            get {
                return ResourceManager.GetString("DocumentNotLoaded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to LoadAssemblyFormNamespace {0} exception {1}.
        /// </summary>
        internal static string LoadAssemblyError {
            get {
                return ResourceManager.GetString("LoadAssemblyError", resourceCulture);
            }
        }
    }
}
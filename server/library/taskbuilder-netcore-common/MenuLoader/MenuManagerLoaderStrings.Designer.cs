﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microarea.Common.MenuLoader {
    using System;
    using System.Reflection;
    
    
    /// <summary>
    ///    A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class MenuManagerLoaderStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        internal MenuManagerLoaderStrings() {
        }
        
        /// <summary>
        ///    Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("taskbuilder-netcore-common.MenuLoader.MenuManagerLoaderStrings", typeof(MenuManagerLoaderStrings).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///    Overrides the current thread's CurrentUICulture property for all
        ///    resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to Custom documents.
        /// </summary>
        public static string DocumentsGroupTitle {
            get {
                return ResourceManager.GetString("DocumentsGroupTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to An exception is occurred in {0}..
        /// </summary>
        public static string GenericExceptionRaisedFmtMsg {
            get {
                return ResourceManager.GetString("GenericExceptionRaisedFmtMsg", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to {0} is not valid menu file, its contents will be ignored..
        /// </summary>
        public static string InvalidMenuFileMsg {
            get {
                return ResourceManager.GetString("InvalidMenuFileMsg", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to Invalid menu file XML syntax..
        /// </summary>
        public static string InvalidMenuXmlMsg {
            get {
                return ResourceManager.GetString("InvalidMenuXmlMsg", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to A xml node null or not valid was specified..
        /// </summary>
        public static string InvalidXmlNodeTypeErrMsg {
            get {
                return ResourceManager.GetString("InvalidXmlNodeTypeErrMsg", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to Error loading file {0}:
        ///{1}.
        /// </summary>
        public static string LoadMenuFileErrFmtMsg {
            get {
                return ResourceManager.GetString("LoadMenuFileErrFmtMsg", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to User Office files.
        /// </summary>
        public static string OfficeFilesGroupTitle {
            get {
                return ResourceManager.GetString("OfficeFilesGroupTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to Syntax error: expression ends without closing a sub-expression between double quote..
        /// </summary>
        public static string SearchExprDoubleQuoteMatchError {
            get {
                return ResourceManager.GetString("SearchExprDoubleQuoteMatchError", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to Syntax error: expression cannot end with operator like &apos;&amp;&apos;, &apos;|&apos;, &apos;)&apos;.
        /// </summary>
        public static string SearchExprEndingCharError {
            get {
                return ResourceManager.GetString("SearchExprEndingCharError", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to Syntax error: expression ends without closing a sub-expression between brackets..
        /// </summary>
        public static string SearchExprParenthesisMatchError {
            get {
                return ResourceManager.GetString("SearchExprParenthesisMatchError", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to Syntax error: expression cannot start with operator like &apos;&amp;&apos;, &apos;|&apos;, &apos;)&apos;.
        /// </summary>
        public static string SearchExprStartingCharError {
            get {
                return ResourceManager.GetString("SearchExprStartingCharError", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to User Report.
        /// </summary>
        public static string UserReportsGroupTitle {
            get {
                return ResourceManager.GetString("UserReportsGroupTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to An error is occurred processing the following XPath expression: &quot;{0}&quot;..
        /// </summary>
        public static string XPathExceptionErrFmtMsg {
            get {
                return ResourceManager.GetString("XPathExceptionErrFmtMsg", resourceCulture);
            }
        }
    }
}

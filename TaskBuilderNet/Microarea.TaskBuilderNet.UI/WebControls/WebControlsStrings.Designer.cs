﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microarea.TaskBuilderNet.UI.WebControls {
    using System;
    
    
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
    internal class WebControlsStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal WebControlsStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microarea.TaskBuilderNet.UI.WebControls.WebControlsStrings", typeof(WebControlsStrings).Assembly);
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
        ///   Looks up a localized string similar to &lt;!DOCTYPE HTML PUBLIC &quot;-//W3C//DTD HTML 4.0 Transitional//EN&quot;&gt;
        ///&lt;html&gt;
        ///&lt;head&gt;
        ///	&lt;title&gt;Loading...&lt;/title&gt;
        ///	&lt;style type=&quot;text/css&quot;&gt;
        ///		div.progressContent
        ///		{
        ///			background-color: Blue;
        ///			padding: 0px;
        ///			width: 0px;
        ///			height: 10px;
        ///			border: 1px solid;
        ///			position: absolute;
        ///			top: 50%;
        ///			left: 20%;
        ///			visibility: hidden;
        ///		}
        ///		div.progressBorder
        ///		{
        ///			padding: 0px;
        ///			width: 60%;
        ///			height: 10px;
        ///			border: 1px solid;
        ///			position: absolute;
        ///			top: 50%;
        ///			left: 20%;
        ///		}
        ///		div [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string LinkDocument {
            get {
                return ResourceManager.GetString("LinkDocument", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Loading....
        /// </summary>
        internal static string Loading {
            get {
                return ResourceManager.GetString("Loading", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///	if (!this.openedDocs)
        ///		this.openedDocs = new Array();
        ///
        ///    function linkDocument(action, parameters, menuBar) {													 
        ///		var now = new Date();
        ///
        ///		if (action.substring(0, 1) == &apos;/&apos;)
        ///			action = action.substring(1, action.length);
        ///
        ///		var	windowName = &apos;doc&apos; + now.getHours() + now.getMinutes() + now.getSeconds() + now.getMilliseconds(); 
        ///		var windowStyle = &apos;height=700,	width=1024, status=yes, scrollbars=yes, resizable=yes&apos;;
        ///		if (menuBar == &apos;true&apos;)
        ///		    windowStyle += &apos;, menubar=yes&apos;;
        /// [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string OpenDocumentJScript {
            get {
                return ResourceManager.GetString("OpenDocumentJScript", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The number must be including between allowed value.
        /// </summary>
        internal static string OutOfRangeValueAlertMsg {
            get {
                return ResourceManager.GetString("OutOfRangeValueAlertMsg", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please Wait.....
        /// </summary>
        internal static string PleaseWait {
            get {
                return ResourceManager.GetString("PleaseWait", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot open a new popup window. Check your browser settings and retry..
        /// </summary>
        internal static string PopupDenied {
            get {
                return ResourceManager.GetString("PopupDenied", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Decrement the value of {0}.
        /// </summary>
        internal static string UpDownDecrementValue {
            get {
                return ResourceManager.GetString("UpDownDecrementValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Increment the value of {0}.
        /// </summary>
        internal static string UpDownIncrementValue {
            get {
                return ResourceManager.GetString("UpDownIncrementValue", resourceCulture);
            }
        }
    }
}

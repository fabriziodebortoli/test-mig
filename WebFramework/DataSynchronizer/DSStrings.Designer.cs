﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microarea.WebServices.DataSynchronizer {
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
    internal class DSStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal DSStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microarea.WebServices.DataSynchronizer.DSStrings", typeof(DSStrings).Assembly);
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
        ///   Looks up a localized string similar to DataSynchronizer web service initializing completed!.
        /// </summary>
        internal static string DSSynchroInit {
            get {
                return ResourceManager.GetString("DSSynchroInit", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error occurred in Notify method (provider {0}, message {1}).
        /// </summary>
        internal static string ErrorInNotify {
            get {
                return ResourceManager.GetString("ErrorInNotify", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to LoginManager Init.
        /// </summary>
        internal static string LMInit {
            get {
                return ResourceManager.GetString("LMInit", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} ended with errors.
        /// </summary>
        internal static string MethodEndedWithError {
            get {
                return ResourceManager.GetString("MethodEndedWithError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} error: {1}.
        /// </summary>
        internal static string MethodErrorWithDetail {
            get {
                return ResourceManager.GetString("MethodErrorWithDetail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} exception: {1}.
        /// </summary>
        internal static string MethodExceptionWithDetail {
            get {
                return ResourceManager.GetString("MethodExceptionWithDetail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Called CleanActionsLog for provider {0} and company {1}.
        /// </summary>
        internal static string MsgCleanActionsLog {
            get {
                return ResourceManager.GetString("MsgCleanActionsLog", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Provider {0} Notify failed (LogID = {1}).
        /// </summary>
        internal static string MsgNotifyEndedWithError {
            get {
                return ResourceManager.GetString("MsgNotifyEndedWithError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Provider {0} Notify successfully ended (LogID = {1}).
        /// </summary>
        internal static string MsgNotifyEndedWithSuccess {
            get {
                return ResourceManager.GetString("MsgNotifyEndedWithSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Called SynchronizeInbound for provider {0} and company {1}.
        /// </summary>
        internal static string MsgSynchronizeInbound {
            get {
                return ResourceManager.GetString("MsgSynchronizeInbound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Called SynchronizeOutbound for provider {0} and company {1}.
        /// </summary>
        internal static string MsgSynchronizeOutbound {
            get {
                return ResourceManager.GetString("MsgSynchronizeOutbound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to change provider parameters because the procedure of synchronization is running! Try again later..
        /// </summary>
        internal static string MsgUnableToChangeProviderParams {
            get {
                return ResourceManager.GetString("MsgUnableToChangeProviderParams", resourceCulture);
            }
        }
    }
}

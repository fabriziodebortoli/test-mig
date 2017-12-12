﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microarea.WebServices.EasyAttachmentSync {
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
    internal class EASyncStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal EASyncStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microarea.WebServices.EasyAttachmentSync.EASyncStrings", typeof(EASyncStrings).Assembly);
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
        ///   Looks up a localized string similar to Unable to prepare the document {0} for SOS, it needs to be resent! ({1}).
        /// </summary>
        internal static string ArchiveDocToResend {
            get {
                return ResourceManager.GetString("ArchiveDocToResend", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Check that Brain Business is correctly installed and all services are started.
        /// </summary>
        internal static string BBCheckBBServices {
            get {
                return ResourceManager.GetString("BBCheckBBServices", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Check that you have registered the connection on BrainBusiness Studio for this company ({0}).
        /// </summary>
        internal static string BBCheckCompanyConnection {
            get {
                return ResourceManager.GetString("BBCheckCompanyConnection", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error n°{0} while retrieving dmsInfo by CompanyId.
        /// </summary>
        internal static string BBDmsInfoRetrievalError {
            get {
                return ResourceManager.GetString("BBDmsInfoRetrievalError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error n°{0} while sending the event.
        /// </summary>
        internal static string BBSendEventError {
            get {
                return ResourceManager.GetString("BBSendEventError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please change the Identity of application pool, in which the EasyAttachmentSync application runs, to Windows Authentication user.(Probably the dbowner of database &apos;{0}&apos; is a Windows Authentication login).
        /// </summary>
        internal static string ChangeIdentityAppPool {
            get {
                return ResourceManager.GetString("ChangeIdentityAppPool", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please check if database &apos;{0}&apos; on server &apos;{1}&apos; exists or if the server is responding.
        /// </summary>
        internal static string CheckDBExist {
            get {
                return ResourceManager.GetString("CheckDBExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please check if database &apos;{0}&apos; on server &apos;{1}&apos; contains valid tables.
        /// </summary>
        internal static string CheckTablesExist {
            get {
                return ResourceManager.GetString("CheckTablesExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to EasyAttachmentSync web service initializing completed!.
        /// </summary>
        internal static string EASyncInit {
            get {
                return ResourceManager.GetString("EASyncInit", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Checking Full-Text Search options for database &apos;{0}&apos; on server &apos;{1}&apos;.
        /// </summary>
        internal static string FTSChecking {
            get {
                return ResourceManager.GetString("FTSChecking", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to initialize DMS databases information! Please try again in a few minutes..
        /// </summary>
        internal static string FTSClearDmsListWithError {
            get {
                return ResourceManager.GetString("FTSClearDmsListWithError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DMS databases information successfully initialized!.
        /// </summary>
        internal static string FTSClearDmsListWithSuccess {
            get {
                return ResourceManager.GetString("FTSClearDmsListWithSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error occurred during enabling of full-text component for database &apos;{0}&apos; on server &apos;{1}&apos;.
        /// </summary>
        internal static string FTSEnableError {
            get {
                return ResourceManager.GetString("FTSEnableError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Full-Text Search component is not installed or cannot be loaded (for database &apos;{0}&apos; on server &apos;{1}&apos;).
        /// </summary>
        internal static string FTSNotInstalled {
            get {
                return ResourceManager.GetString("FTSNotInstalled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Full-Text Search options update for database &apos;{0}&apos; on server &apos;{1}&apos; has been successfully completed.
        /// </summary>
        internal static string FTSUpdating {
            get {
                return ResourceManager.GetString("FTSUpdating", resourceCulture);
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
        ///   Looks up a localized string similar to Run Synchronize process for database &apos;{0}&apos; on server &apos;{1}&apos;.
        /// </summary>
        internal static string RunSynchronizeDB {
            get {
                return ResourceManager.GetString("RunSynchronizeDB", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error occurred during Spedizione Frammentata of file {0} (chunk nr. {1}).
        /// </summary>
        internal static string SOSErrorFrammentata {
            get {
                return ResourceManager.GetString("SOSErrorFrammentata", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error occurred during Spedizione Frammentata of file {0} (chunk nr. {1} - attempt nr. {2}).
        /// </summary>
        internal static string SOSErrorFrammentataAttempt {
            get {
                return ResourceManager.GetString("SOSErrorFrammentataAttempt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error occurred during Spedizione Semplice of file {0}.
        /// </summary>
        internal static string SOSErrorSemplice {
            get {
                return ResourceManager.GetString("SOSErrorSemplice", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error occurred calling Sequenza webmethod.
        /// </summary>
        internal static string SOSErrorSequenza {
            get {
                return ResourceManager.GetString("SOSErrorSequenza", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error occurred calling StatoSpedizione webmethod for file {0}.
        /// </summary>
        internal static string SOSErrorStatoSpedizione {
            get {
                return ResourceManager.GetString("SOSErrorStatoSpedizione", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Last dispatch status update performed on {0}.
        /// </summary>
        internal static string SOSLastUpdateDocRequested {
            get {
                return ResourceManager.GetString("SOSLastUpdateDocRequested", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Run Update SOS documents status process for database &apos;{0}&apos; on server &apos;{1}&apos;.
        /// </summary>
        internal static string SOSRunUpdateDocumentStatus {
            get {
                return ResourceManager.GetString("SOSRunUpdateDocumentStatus", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Update SOS documents status process completed with error (Database &apos;{0}&apos; - Server &apos;{1}&apos;).
        /// </summary>
        internal static string SOSRunUpdateDocumentStatusEndedWithError {
            get {
                return ResourceManager.GetString("SOSRunUpdateDocumentStatusEndedWithError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to  Update SOS documents status process successfully completed (Database &apos;{0}&apos; - Server &apos;{1}&apos;).
        /// </summary>
        internal static string SOSRunUpdateDocumentStatusSuccessfullyCompleted {
            get {
                return ResourceManager.GetString("SOSRunUpdateDocumentStatusSuccessfullyCompleted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error occurred during dispatch status update.
        /// </summary>
        internal static string SOSUpdateDocWithError {
            get {
                return ResourceManager.GetString("SOSUpdateDocWithError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to EasyAttachmentSync stopping successfully completed.
        /// </summary>
        internal static string StoppingEASync {
            get {
                return ResourceManager.GetString("StoppingEASync", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to EasyAttachmentSync: Synchronize operation completed with error (Database &apos;{0}&apos; - Server &apos;{1}&apos;).
        /// </summary>
        internal static string SynchronizeEndedWithError {
            get {
                return ResourceManager.GetString("SynchronizeEndedWithError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to EasyAttachmentSync: Synchronize operation successfully completed (Database &apos;{0}&apos; - Server &apos;{1}&apos;).
        /// </summary>
        internal static string SynchronizeSuccessfullyCompleted {
            get {
                return ResourceManager.GetString("SynchronizeSuccessfullyCompleted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to stop EasyAttachmentSync because Synchronize process is running! Please try again in a few minutes..
        /// </summary>
        internal static string UnableToStopEASync {
            get {
                return ResourceManager.GetString("UnableToStopEASync", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to EasyAttachmentSync: UpdateDefaultSearchIndexes completed (Database &apos;{0}&apos; - Server: &apos;{1}&apos;).
        /// </summary>
        internal static string UpdateDefaultSearchIndexes {
            get {
                return ResourceManager.GetString("UpdateDefaultSearchIndexes", resourceCulture);
            }
        }
    }
}

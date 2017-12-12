﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.1.
// 
#pragma warning disable 1591

namespace Tester.Crypter30 {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.ComponentModel;
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="Crypter30Soap", Namespace="http://www.microarea.it/")]
    public partial class Crypter30 : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback CryptOperationCompleted;
        
        private System.Threading.SendOrPostCallback IntegratedSolutionExistsOperationCompleted;
        
        private System.Threading.SendOrPostCallback RegisterIntegratedSolutionOperationCompleted;
        
        private System.Threading.SendOrPostCallback LoginOperationCompleted;
        
        private System.Threading.SendOrPostCallback Login1OperationCompleted;
        
        private System.Threading.SendOrPostCallback ProcessModuleOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public Crypter30() {
            this.Url = global::Tester.Properties.Settings.Default.Tester_Crypter30_Crypter30;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event CryptCompletedEventHandler CryptCompleted;
        
        /// <remarks/>
        public event IntegratedSolutionExistsCompletedEventHandler IntegratedSolutionExistsCompleted;
        
        /// <remarks/>
        public event RegisterIntegratedSolutionCompletedEventHandler RegisterIntegratedSolutionCompleted;
        
        /// <remarks/>
        public event LoginCompletedEventHandler LoginCompleted;
        
        /// <remarks/>
        public event Login1CompletedEventHandler Login1Completed;
        
        /// <remarks/>
        public event ProcessModuleCompletedEventHandler ProcessModuleCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.microarea.it/Crypt", RequestNamespace="http://www.microarea.it/", ResponseNamespace="http://www.microarea.it/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string Crypt(string toCrypt) {
            object[] results = this.Invoke("Crypt", new object[] {
                        toCrypt});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginCrypt(string toCrypt, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("Crypt", new object[] {
                        toCrypt}, callback, asyncState);
        }
        
        /// <remarks/>
        public string EndCrypt(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void CryptAsync(string toCrypt) {
            this.CryptAsync(toCrypt, null);
        }
        
        /// <remarks/>
        public void CryptAsync(string toCrypt, object userState) {
            if ((this.CryptOperationCompleted == null)) {
                this.CryptOperationCompleted = new System.Threading.SendOrPostCallback(this.OnCryptOperationCompleted);
            }
            this.InvokeAsync("Crypt", new object[] {
                        toCrypt}, this.CryptOperationCompleted, userState);
        }
        
        private void OnCryptOperationCompleted(object arg) {
            if ((this.CryptCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.CryptCompleted(this, new CryptCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.microarea.it/IntegratedSolutionExists", RequestNamespace="http://www.microarea.it/", ResponseNamespace="http://www.microarea.it/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool IntegratedSolutionExists(string solutionName) {
            object[] results = this.Invoke("IntegratedSolutionExists", new object[] {
                        solutionName});
            return ((bool)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginIntegratedSolutionExists(string solutionName, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("IntegratedSolutionExists", new object[] {
                        solutionName}, callback, asyncState);
        }
        
        /// <remarks/>
        public bool EndIntegratedSolutionExists(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((bool)(results[0]));
        }
        
        /// <remarks/>
        public void IntegratedSolutionExistsAsync(string solutionName) {
            this.IntegratedSolutionExistsAsync(solutionName, null);
        }
        
        /// <remarks/>
        public void IntegratedSolutionExistsAsync(string solutionName, object userState) {
            if ((this.IntegratedSolutionExistsOperationCompleted == null)) {
                this.IntegratedSolutionExistsOperationCompleted = new System.Threading.SendOrPostCallback(this.OnIntegratedSolutionExistsOperationCompleted);
            }
            this.InvokeAsync("IntegratedSolutionExists", new object[] {
                        solutionName}, this.IntegratedSolutionExistsOperationCompleted, userState);
        }
        
        private void OnIntegratedSolutionExistsOperationCompleted(object arg) {
            if ((this.IntegratedSolutionExistsCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.IntegratedSolutionExistsCompleted(this, new IntegratedSolutionExistsCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.microarea.it/RegisterIntegratedSolution", RequestNamespace="http://www.microarea.it/", ResponseNamespace="http://www.microarea.it/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool RegisterIntegratedSolution(string password, string login, string solutionName, string description, string webServiceUrl) {
            object[] results = this.Invoke("RegisterIntegratedSolution", new object[] {
                        password,
                        login,
                        solutionName,
                        description,
                        webServiceUrl});
            return ((bool)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginRegisterIntegratedSolution(string password, string login, string solutionName, string description, string webServiceUrl, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("RegisterIntegratedSolution", new object[] {
                        password,
                        login,
                        solutionName,
                        description,
                        webServiceUrl}, callback, asyncState);
        }
        
        /// <remarks/>
        public bool EndRegisterIntegratedSolution(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((bool)(results[0]));
        }
        
        /// <remarks/>
        public void RegisterIntegratedSolutionAsync(string password, string login, string solutionName, string description, string webServiceUrl) {
            this.RegisterIntegratedSolutionAsync(password, login, solutionName, description, webServiceUrl, null);
        }
        
        /// <remarks/>
        public void RegisterIntegratedSolutionAsync(string password, string login, string solutionName, string description, string webServiceUrl, object userState) {
            if ((this.RegisterIntegratedSolutionOperationCompleted == null)) {
                this.RegisterIntegratedSolutionOperationCompleted = new System.Threading.SendOrPostCallback(this.OnRegisterIntegratedSolutionOperationCompleted);
            }
            this.InvokeAsync("RegisterIntegratedSolution", new object[] {
                        password,
                        login,
                        solutionName,
                        description,
                        webServiceUrl}, this.RegisterIntegratedSolutionOperationCompleted, userState);
        }
        
        private void OnRegisterIntegratedSolutionOperationCompleted(object arg) {
            if ((this.RegisterIntegratedSolutionCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.RegisterIntegratedSolutionCompleted(this, new RegisterIntegratedSolutionCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.microarea.it/Login", RequestNamespace="http://www.microarea.it/", ResponseNamespace="http://www.microarea.it/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool Login(string password, [System.Xml.Serialization.XmlElementAttribute("login")] string login1) {
            object[] results = this.Invoke("Login", new object[] {
                        password,
                        login1});
            return ((bool)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginLogin(string password, string login1, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("Login", new object[] {
                        password,
                        login1}, callback, asyncState);
        }
        
        /// <remarks/>
        public bool EndLogin(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((bool)(results[0]));
        }
        
        /// <remarks/>
        public void LoginAsync(string password, string login1) {
            this.LoginAsync(password, login1, null);
        }
        
        /// <remarks/>
        public void LoginAsync(string password, string login1, object userState) {
            if ((this.LoginOperationCompleted == null)) {
                this.LoginOperationCompleted = new System.Threading.SendOrPostCallback(this.OnLoginOperationCompleted);
            }
            this.InvokeAsync("Login", new object[] {
                        password,
                        login1}, this.LoginOperationCompleted, userState);
        }
        
        private void OnLoginOperationCompleted(object arg) {
            if ((this.LoginCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.LoginCompleted(this, new LoginCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.WebMethodAttribute(MessageName="Login1")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.microarea.it/Login1", RequestNamespace="http://www.microarea.it/", ResponseNamespace="http://www.microarea.it/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool Login(string password, string login, out IntegratedSolution[] solutions, out string companyCode) {
            object[] results = this.Invoke("Login1", new object[] {
                        password,
                        login});
            solutions = ((IntegratedSolution[])(results[1]));
            companyCode = ((string)(results[2]));
            return ((bool)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginLogin1(string password, string login, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("Login1", new object[] {
                        password,
                        login}, callback, asyncState);
        }
        
        /// <remarks/>
        public bool EndLogin1(System.IAsyncResult asyncResult, out IntegratedSolution[] solutions, out string companyCode) {
            object[] results = this.EndInvoke(asyncResult);
            solutions = ((IntegratedSolution[])(results[1]));
            companyCode = ((string)(results[2]));
            return ((bool)(results[0]));
        }
        
        /// <remarks/>
        public void Login1Async(string password, string login) {
            this.Login1Async(password, login, null);
        }
        
        /// <remarks/>
        public void Login1Async(string password, string login, object userState) {
            if ((this.Login1OperationCompleted == null)) {
                this.Login1OperationCompleted = new System.Threading.SendOrPostCallback(this.OnLogin1OperationCompleted);
            }
            this.InvokeAsync("Login1", new object[] {
                        password,
                        login}, this.Login1OperationCompleted, userState);
        }
        
        private void OnLogin1OperationCompleted(object arg) {
            if ((this.Login1Completed != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.Login1Completed(this, new Login1CompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.microarea.it/ProcessModule", RequestNamespace="http://www.microarea.it/", ResponseNamespace="http://www.microarea.it/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool ProcessModule(ref string a, ref string b, ref string c) {
            object[] results = this.Invoke("ProcessModule", new object[] {
                        a,
                        b,
                        c});
            a = ((string)(results[1]));
            b = ((string)(results[2]));
            c = ((string)(results[3]));
            return ((bool)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginProcessModule(string a, string b, string c, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("ProcessModule", new object[] {
                        a,
                        b,
                        c}, callback, asyncState);
        }
        
        /// <remarks/>
        public bool EndProcessModule(System.IAsyncResult asyncResult, out string a, out string b, out string c) {
            object[] results = this.EndInvoke(asyncResult);
            a = ((string)(results[1]));
            b = ((string)(results[2]));
            c = ((string)(results[3]));
            return ((bool)(results[0]));
        }
        
        /// <remarks/>
        public void ProcessModuleAsync(string a, string b, string c) {
            this.ProcessModuleAsync(a, b, c, null);
        }
        
        /// <remarks/>
        public void ProcessModuleAsync(string a, string b, string c, object userState) {
            if ((this.ProcessModuleOperationCompleted == null)) {
                this.ProcessModuleOperationCompleted = new System.Threading.SendOrPostCallback(this.OnProcessModuleOperationCompleted);
            }
            this.InvokeAsync("ProcessModule", new object[] {
                        a,
                        b,
                        c}, this.ProcessModuleOperationCompleted, userState);
        }
        
        private void OnProcessModuleOperationCompleted(object arg) {
            if ((this.ProcessModuleCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.ProcessModuleCompleted(this, new ProcessModuleCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.microarea.it/")]
    public partial class IntegratedSolution {
        
        private string usernameField;
        
        private string companyCodeField;
        
        private string solutionNameField;
        
        private bool isActiveField;
        
        private bool isNewField;
        
        private string descriptionField;
        
        private string webServiceUrlField;
        
        private SolutionType solutionTypeField;
        
        private string[] productCodesField;
        
        private string selectedFreeCharsField;
        
        /// <remarks/>
        public string Username {
            get {
                return this.usernameField;
            }
            set {
                this.usernameField = value;
            }
        }
        
        /// <remarks/>
        public string CompanyCode {
            get {
                return this.companyCodeField;
            }
            set {
                this.companyCodeField = value;
            }
        }
        
        /// <remarks/>
        public string SolutionName {
            get {
                return this.solutionNameField;
            }
            set {
                this.solutionNameField = value;
            }
        }
        
        /// <remarks/>
        public bool IsActive {
            get {
                return this.isActiveField;
            }
            set {
                this.isActiveField = value;
            }
        }
        
        /// <remarks/>
        public bool IsNew {
            get {
                return this.isNewField;
            }
            set {
                this.isNewField = value;
            }
        }
        
        /// <remarks/>
        public string Description {
            get {
                return this.descriptionField;
            }
            set {
                this.descriptionField = value;
            }
        }
        
        /// <remarks/>
        public string WebServiceUrl {
            get {
                return this.webServiceUrlField;
            }
            set {
                this.webServiceUrlField = value;
            }
        }
        
        /// <remarks/>
        public SolutionType SolutionType {
            get {
                return this.solutionTypeField;
            }
            set {
                this.solutionTypeField = value;
            }
        }
        
        /// <remarks/>
        public string[] ProductCodes {
            get {
                return this.productCodesField;
            }
            set {
                this.productCodesField = value;
            }
        }
        
        /// <remarks/>
        public string SelectedFreeChars {
            get {
                return this.selectedFreeCharsField;
            }
            set {
                this.selectedFreeCharsField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.microarea.it/")]
    public enum SolutionType {
        
        /// <remarks/>
        None,
        
        /// <remarks/>
        VerticalIntegration,
        
        /// <remarks/>
        Embedded,
        
        /// <remarks/>
        StandAlone,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    public delegate void CryptCompletedEventHandler(object sender, CryptCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class CryptCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal CryptCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    public delegate void IntegratedSolutionExistsCompletedEventHandler(object sender, IntegratedSolutionExistsCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class IntegratedSolutionExistsCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal IntegratedSolutionExistsCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public bool Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((bool)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    public delegate void RegisterIntegratedSolutionCompletedEventHandler(object sender, RegisterIntegratedSolutionCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class RegisterIntegratedSolutionCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal RegisterIntegratedSolutionCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public bool Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((bool)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    public delegate void LoginCompletedEventHandler(object sender, LoginCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class LoginCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal LoginCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public bool Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((bool)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    public delegate void Login1CompletedEventHandler(object sender, Login1CompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Login1CompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal Login1CompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public bool Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((bool)(this.results[0]));
            }
        }
        
        /// <remarks/>
        public IntegratedSolution[] solutions {
            get {
                this.RaiseExceptionIfNecessary();
                return ((IntegratedSolution[])(this.results[1]));
            }
        }
        
        /// <remarks/>
        public string companyCode {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[2]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    public delegate void ProcessModuleCompletedEventHandler(object sender, ProcessModuleCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ProcessModuleCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal ProcessModuleCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public bool Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((bool)(this.results[0]));
            }
        }
        
        /// <remarks/>
        public string a {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[1]));
            }
        }
        
        /// <remarks/>
        public string b {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[2]));
            }
        }
        
        /// <remarks/>
        public string c {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[3]));
            }
        }
    }
}

#pragma warning restore 1591
namespace Microarea.EasyBuilder.Licence
{


	/// <remarks/>
	//=========================================================================
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public partial class SalesModule {
        
        private SalesModuleShortName[] shortNames;
        
        private SalesModuleApplication[] applications;
        
        private SalesModuleIncludeModulesPath[] includeModulesPaths;
        
        private string localize;
        
        private CalType calType;
        
        private bool namedCal;
        
        private string edition;
        
        private string internalCode;
        
        private string prodId;
        
        /// <remarks/>
        //---------------------------------------------------------------------
		[System.Xml.Serialization.XmlArrayItemAttribute("ShortName", IsNullable=false)]
        public SalesModuleShortName[] ShortNames {
            get {
                return this.shortNames;
            }
            set {
                this.shortNames = value;
            }
        }
        
        /// <remarks/>
		//---------------------------------------------------------------------
		[System.Xml.Serialization.XmlElementAttribute("Application")]
        public SalesModuleApplication[] Applications {
            get {
                return this.applications;
            }
            set {
                this.applications = value;
            }
        }
        
        /// <remarks/>
		//---------------------------------------------------------------------
		[System.Xml.Serialization.XmlElementAttribute("IncludeModulesPath")]
        public SalesModuleIncludeModulesPath[] IncludeModulesPaths {
            get {
                return this.includeModulesPaths;
            }
            set {
                this.includeModulesPaths = value;
            }
        }
        
        /// <remarks/>
		//---------------------------------------------------------------------
		[System.Xml.Serialization.XmlAttributeAttribute("localize")]
        public string Localize {
            get {
                return this.localize;
            }
            set {
                this.localize = value;
            }
        }
        
        /// <remarks/>
		//---------------------------------------------------------------------
		[System.Xml.Serialization.XmlAttributeAttribute("caltype")]
        public CalType CalType {
            get {
                return this.calType;
            }
            set {
                this.calType = value;
            }
        }
        
        /// <remarks/>
		//---------------------------------------------------------------------
		[System.Xml.Serialization.XmlAttributeAttribute("namedcal")]
        public bool NamedCal {
            get {
                return this.namedCal;
            }
            set {
                this.namedCal = value;
            }
        }
        
        /// <remarks/>
		//---------------------------------------------------------------------
		[System.Xml.Serialization.XmlAttributeAttribute("edition")]
        public string Edition {
            get {
                return this.edition;
            }
            set {
                this.edition = value;
            }
        }
        
        /// <remarks/>
		//---------------------------------------------------------------------
		[System.Xml.Serialization.XmlAttributeAttribute("internalcode")]
        public string InternalCode {
            get {
                return this.internalCode;
            }
            set {
                this.internalCode = value;
            }
        }
        
        /// <remarks/>
		//---------------------------------------------------------------------
		[System.Xml.Serialization.XmlAttributeAttribute("prodid")]
        public string ProdId {
            get {
                return this.prodId;
            }
            set {
                this.prodId = value;
            }
        }
    }
    
    /// <remarks/>
	//=========================================================================
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class SalesModuleShortName {
        
        private string name;
        
        /// <remarks/>
		//---------------------------------------------------------------------
		[System.Xml.Serialization.XmlAttributeAttribute("name")]
        public string Name {
            get {
                return this.name;
            }
            set {
                this.name = value;
            }
        }
    }
    
    /// <remarks/>
	//=========================================================================
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class SalesModuleApplication {
        
        private object[] items;
        
        private string container;
        
        private string name;
        
        /// <remarks/>
		//---------------------------------------------------------------------
		[System.Xml.Serialization.XmlElementAttribute("Functionality", typeof(SalesModuleApplicationFunctionality))]
        [System.Xml.Serialization.XmlElementAttribute("Module", typeof(SalesModuleApplicationModule))]
        public object[] Items {
            get {
                return this.items;
            }
            set {
                this.items = value;
            }
        }
        
        /// <remarks/>
		//---------------------------------------------------------------------
		[System.Xml.Serialization.XmlAttributeAttribute("container")]
        public string Container {
            get {
                return this.container;
            }
            set {
                this.container = value;
            }
        }
        
        /// <remarks/>
		//---------------------------------------------------------------------
		[System.Xml.Serialization.XmlAttributeAttribute("name")]
        public string Name {
            get {
                return this.name;
            }
            set {
                this.name = value;
            }
        }
    }
    
    /// <remarks/>
	//=========================================================================
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class SalesModuleApplicationFunctionality {
        
        private SalesModuleApplicationFunctionalityAllow allow;
        
        private string name;
        
        /// <remarks/>
		//---------------------------------------------------------------------
		public SalesModuleApplicationFunctionalityAllow Allow
		{
            get {
                return this.allow;
            }
            set {
                this.allow = value;
            }
        }
        
        /// <remarks/>
		//---------------------------------------------------------------------
		[System.Xml.Serialization.XmlAttributeAttribute("name")]
        public string Name {
            get {
                return this.name;
            }
            set {
                this.name = value;
            }
        }
    }
    
    /// <remarks/>
	//=========================================================================
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class SalesModuleApplicationFunctionalityAllow {
        
        private string iso;
        
        /// <remarks/>
		//---------------------------------------------------------------------
		[System.Xml.Serialization.XmlAttributeAttribute("iso")]
        public string Iso {
            get {
                return this.iso;
            }
            set {
                this.iso = value;
            }
        }
    }
    
    /// <remarks/>
	//=========================================================================
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class SalesModuleApplicationModule {
        
        private SalesModuleApplicationModuleAllow allow;
        
        private string name;
        
        /// <remarks/>
		//---------------------------------------------------------------------
		public SalesModuleApplicationModuleAllow Allow
		{
            get {
                return this.allow;
            }
            set {
                this.allow = value;
            }
        }
        
        /// <remarks/>
		//---------------------------------------------------------------------
		[System.Xml.Serialization.XmlAttributeAttribute("name")]
        public string Name {
            get {
                return this.name;
            }
            set {
                this.name = value;
            }
        }
    }
    
    /// <remarks/>
	//=========================================================================
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class SalesModuleApplicationModuleAllow {
        
        private string iso;
        
        /// <remarks/>
		//---------------------------------------------------------------------
		[System.Xml.Serialization.XmlAttributeAttribute("iso")]
        public string Iso {
            get {
                return this.iso;
            }
            set {
                this.iso = value;
            }
        }
    }
    
    /// <remarks/>
	//=========================================================================
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class SalesModuleIncludeModulesPath {
        
        private SalesModuleIncludeModulesPathAllow allow;
        
        private string path;
        
        private bool onSelfActivated;
        
        private bool onSelfActivatedSpecified;
        
        /// <remarks/>
		//---------------------------------------------------------------------
		public SalesModuleIncludeModulesPathAllow Allow
		{
            get {
                return this.allow;
            }
            set {
                this.allow = value;
            }
        }
        
        /// <remarks/>
		//---------------------------------------------------------------------
		[System.Xml.Serialization.XmlAttributeAttribute("path")]
        public string Path {
            get {
                return this.path;
            }
            set {
                this.path = value;
            }
        }
        
        /// <remarks/>
		//---------------------------------------------------------------------
		[System.Xml.Serialization.XmlAttributeAttribute("onselfactivated")]
        public bool OnSelfActivated {
            get {
                return this.onSelfActivated;
            }
            set {
                this.onSelfActivated = value;
            }
        }
        
        /// <remarks/>
		//---------------------------------------------------------------------
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OnSelfActivatedSpecified {
            get {
                return this.onSelfActivatedSpecified;
            }
            set {
                this.onSelfActivatedSpecified = value;
            }
        }
    }
    
    /// <remarks/>
	//=========================================================================
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class SalesModuleIncludeModulesPathAllow {
        
        private string iso;
        
        /// <remarks/>
		//---------------------------------------------------------------------
		[System.Xml.Serialization.XmlAttributeAttribute("iso")]
        public string Iso {
            get {
                return this.iso;
            }
            set {
                this.iso = value;
            }
        }
    }
    
    /// <remarks/>
	//=========================================================================
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    public enum CalType {
        
        /// <remarks/>
        None,
        
        /// <remarks/>
        Auto,
    }
}

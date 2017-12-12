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
    public partial class Product {
        
        private ProductSalesModule[] salesModules;
        
        private string localize;
        
        private byte activationVersion;
        
        private string prodId;
        
        private string editionId;
        
        /// <remarks/>
		//---------------------------------------------------------------------
        [System.Xml.Serialization.XmlElementAttribute("SalesModule")]
        public ProductSalesModule[] SalesModules {
            get {
                return this.salesModules;
            }
            set {
                this.salesModules = value;
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
		[System.Xml.Serialization.XmlAttributeAttribute("activationversion")]
        public byte ActivationVersion {
            get {
                return this.activationVersion;
            }
            set {
                this.activationVersion = value;
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
        
        /// <remarks/>
		//---------------------------------------------------------------------
		[System.Xml.Serialization.XmlAttributeAttribute("editionid")]
        public string EditionId {
            get {
                return this.editionId;
            }
            set {
                this.editionId = value;
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
    public partial class ProductSalesModule {
        
        private ProductSalesModuleAllow allow;
        
        private ProductSalesModuleDeny deny;
        
        private string name;
        
        /// <remarks/>
		//---------------------------------------------------------------------
		public ProductSalesModuleAllow Allow
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
		public ProductSalesModuleDeny Deny
		{
            get {
                return this.deny;
            }
            set {
                this.deny = value;
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
    public partial class ProductSalesModuleAllow {
        
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
    public partial class ProductSalesModuleDeny {

		private string iso;

		/// <remarks/>
		//---------------------------------------------------------------------
		[System.Xml.Serialization.XmlAttributeAttribute("iso")]
		public string Iso
		{
			get {
				return this.iso;
			}
			set {
				this.iso = value;
			}
		}
    }
}

﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=4.6.1055.0.
// 
namespace Microarea.Snap.Core {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://schemas.microarea.it/setup")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://schemas.microarea.it/setup", IsNullable=false)]
    public partial class PackageDefinition {
        
        private FileSystemStructure fileSystemStructureField;
        
        private string idField;
        
        private string versionField;
        
        private string titleField;
        
        
        private string producerField;
        
        private string productField;
        
        private string productversionField;
        
        private string descriptionField;
        
        private string iconUrlField;
        
        private string releaseNotesUrlField;
        
        /// <remarks/>
        public FileSystemStructure FileSystemStructure {
            get {
                return this.fileSystemStructureField;
            }
            set {
                this.fileSystemStructureField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "id")]
        public string Id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer", AttributeName = "version")]
        public string Version {
            get {
                return this.versionField;
            }
            set {
                this.versionField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "title")]
        public string Title {
            get {
                return this.titleField;
            }
            set {
                this.titleField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "producer")]
        public string Producer {
            get {
                return this.producerField;
            }
            set {
                this.producerField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "product")]
        public string Product {
            get {
                return this.productField;
            }
            set {
                this.productField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "productversion")]
        public string ProductVersion {
            get {
                return this.productversionField;
            }
            set {
                this.productversionField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "description")]
        public string Description {
            get {
                return this.descriptionField;
            }
            set {
                this.descriptionField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "iconUrl")]
        public string IconUrl {
            get {
                return this.iconUrlField;
            }
            set {
                this.iconUrlField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "releaseNotesUrl")]
        public string ReleaseNotesUrl {
            get {
                return this.releaseNotesUrlField;
            }
            set {
                this.releaseNotesUrlField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://schemas.microarea.it/setup")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://schemas.microarea.it/setup", IsNullable=false)]
    public partial class FileSystemStructure {
        
        private IncludeFolder[] includeFoldersField;
        
        private ExcludeFolder[] excludeFoldersField;
        
        private IncludeFile[] includeFilesField;
        
        private ExcludeFile[] excludeFilesField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("IncludeFolder")]
        public IncludeFolder[] IncludeFolders {
            get {
                return this.includeFoldersField;
            }
            set {
                this.includeFoldersField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ExcludeFolder")]
        public ExcludeFolder[] ExcludeFolders {
            get {
                return this.excludeFoldersField;
            }
            set {
                this.excludeFoldersField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("IncludeFile")]
        public IncludeFile[] IncludeFiles {
            get {
                return this.includeFilesField;
            }
            set {
                this.includeFilesField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ExcludeFile")]
        public ExcludeFile[] ExcludeFiles {
            get {
                return this.excludeFilesField;
            }
            set {
                this.excludeFilesField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://schemas.microarea.it/setup")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://schemas.microarea.it/setup", IsNullable=false)]
    public partial class IncludeFolder {
        
        private IncludeFolder[] includeFoldersField;
        
        private ExcludeFolder[] excludeFoldersField;
        
        private IncludeFile[] includeFilesField;
        
        private ExcludeFile[] excludeFilesField;
        
        private string nameField;
        
        private RecursiveType recursiveField;
        
        private bool recursiveFieldSpecified;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("IncludeFolder")]
        public IncludeFolder[] IncludeFolders {
            get {
                return this.includeFoldersField;
            }
            set {
                this.includeFoldersField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ExcludeFolder")]
        public ExcludeFolder[] ExcludeFolders {
            get {
                return this.excludeFoldersField;
            }
            set {
                this.excludeFoldersField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("IncludeFile")]
        public IncludeFile[] IncludeFiles {
            get {
                return this.includeFilesField;
            }
            set {
                this.includeFilesField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ExcludeFile")]
        public ExcludeFile[] ExcludeFiles {
            get {
                return this.excludeFilesField;
            }
            set {
                this.excludeFilesField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("name")]
        public string Name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("recursive")]
        public RecursiveType Recursive {
            get {
                return this.recursiveField;
            }
            set {
                this.recursiveField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RecursiveSpecified {
            get {
                return this.recursiveFieldSpecified;
            }
            set {
                this.recursiveFieldSpecified = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://schemas.microarea.it/setup")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://schemas.microarea.it/setup", IsNullable=false)]
    public partial class ExcludeFolder {
        
        private string nameField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("name")]
        public string Name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://schemas.microarea.it/setup")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://schemas.microarea.it/setup", IsNullable=false)]
    public partial class IncludeFile {
        
        private string nameField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("name")]
        public string Name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://schemas.microarea.it/setup")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://schemas.microarea.it/setup", IsNullable=false)]
    public partial class ExcludeFile {
        
        private string nameField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("name")]
        public string Name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.microarea.it/setup")]
    public enum RecursiveType {
        
        /// <remarks/>
        no,
        
        /// <remarks/>
        yes,
    }
}

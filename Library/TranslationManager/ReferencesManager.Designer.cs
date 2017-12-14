using System;
using System.Collections.Generic;
using System.Text;

namespace Microarea.Library.TranslationManager
{
    partial class ReferencesManager
    {
        private System.Windows.Forms.ComboBox CMBApplicazione;
        private System.Windows.Forms.ComboBox CMBModulo;
        private System.Windows.Forms.ComboBox CMBLibrary;
        private System.Windows.Forms.ListBox LSTLibs;
        private System.Windows.Forms.ComboBox CMDConfiguration;
        private System.Windows.Forms.RichTextBox TxtReader = new System.Windows.Forms.RichTextBox();
        private System.Windows.Forms.ListBox LSTReferences;
        private System.Windows.Forms.Button CMDRefToLib;
        private System.Windows.Forms.Button CMDLibToRef;
        private System.Windows.Forms.Button CMDAddLib;
        private System.Windows.Forms.Button CMDAddRef;
        private System.Windows.Forms.Button CMDSalva;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReferencesManager));
            this.CMBApplicazione = new System.Windows.Forms.ComboBox();
            this.CMBModulo = new System.Windows.Forms.ComboBox();
            this.CMBLibrary = new System.Windows.Forms.ComboBox();
            this.LSTLibs = new System.Windows.Forms.ListBox();
            this.CMDConfiguration = new System.Windows.Forms.ComboBox();
            this.LSTReferences = new System.Windows.Forms.ListBox();
            this.CMDRefToLib = new System.Windows.Forms.Button();
            this.CMDLibToRef = new System.Windows.Forms.Button();
            this.CMDAddLib = new System.Windows.Forms.Button();
            this.CMDAddRef = new System.Windows.Forms.Button();
            this.CMDSalva = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CMBApplicazione
            // 
            resources.ApplyResources(this.CMBApplicazione, "CMBApplicazione");
            this.CMBApplicazione.Name = "CMBApplicazione";
            this.CMBApplicazione.SelectedIndexChanged += new System.EventHandler(this.CMBApplicazione_SelectedIndexChanged);
            // 
            // CMBModulo
            // 
            this.CMBModulo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.CMBModulo, "CMBModulo");
            this.CMBModulo.Name = "CMBModulo";
            this.CMBModulo.SelectedIndexChanged += new System.EventHandler(this.CMBModulo_SelectedIndexChanged);
            // 
            // CMBLibrary
            // 
            this.CMBLibrary.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.CMBLibrary, "CMBLibrary");
            this.CMBLibrary.Name = "CMBLibrary";
            this.CMBLibrary.SelectedIndexChanged += new System.EventHandler(this.CMBLibrary_SelectedIndexChanged);
            // 
            // LSTLibs
            // 
            resources.ApplyResources(this.LSTLibs, "LSTLibs");
            this.LSTLibs.Name = "LSTLibs";
            // 
            // CMDConfiguration
            // 
            resources.ApplyResources(this.CMDConfiguration, "CMDConfiguration");
            this.CMDConfiguration.Name = "CMDConfiguration";
            this.CMDConfiguration.SelectedIndexChanged += new System.EventHandler(this.CMDConfiguration_SelectedIndexChanged);
            // 
            // LSTReferences
            // 
            resources.ApplyResources(this.LSTReferences, "LSTReferences");
            this.LSTReferences.Name = "LSTReferences";
            // 
            // CMDRefToLib
            // 
            resources.ApplyResources(this.CMDRefToLib, "CMDRefToLib");
            this.CMDRefToLib.Name = "CMDRefToLib";
            this.CMDRefToLib.Click += new System.EventHandler(this.CMDRefToLib_Click);
            // 
            // CMDLibToRef
            // 
            resources.ApplyResources(this.CMDLibToRef, "CMDLibToRef");
            this.CMDLibToRef.Name = "CMDLibToRef";
            this.CMDLibToRef.Click += new System.EventHandler(this.CMDLibToRef_Click);
            // 
            // CMDAddLib
            // 
            resources.ApplyResources(this.CMDAddLib, "CMDAddLib");
            this.CMDAddLib.Name = "CMDAddLib";
            // 
            // CMDAddRef
            // 
            resources.ApplyResources(this.CMDAddRef, "CMDAddRef");
            this.CMDAddRef.Name = "CMDAddRef";
            // 
            // CMDSalva
            // 
            resources.ApplyResources(this.CMDSalva, "CMDSalva");
            this.CMDSalva.Name = "CMDSalva";
            this.CMDSalva.Click += new System.EventHandler(this.CMDSalva_Click);
            // 
            // ReferencesManager
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.CMDSalva);
            this.Controls.Add(this.CMDAddRef);
            this.Controls.Add(this.CMDAddLib);
            this.Controls.Add(this.CMDLibToRef);
            this.Controls.Add(this.CMDRefToLib);
            this.Controls.Add(this.LSTReferences);
            this.Controls.Add(this.CMDConfiguration);
            this.Controls.Add(this.LSTLibs);
            this.Controls.Add(this.CMBLibrary);
            this.Controls.Add(this.CMBModulo);
            this.Controls.Add(this.CMBApplicazione);
            this.Name = "ReferencesManager";
            this.Load += new System.EventHandler(this.ReferencesManager_Load);
            this.ResumeLayout(false);

        }
        #endregion
    }
}

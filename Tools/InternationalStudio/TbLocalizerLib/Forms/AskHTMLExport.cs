using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.Forms
{
    public partial class AskHTMLExport : Form
    {
        private bool separateFiles = false;
        private bool on3Columns = false;
        private bool fillTargetWithSupport = false;
        private bool skipUntranslatable = false;
        private bool baseStringAsAttribute = false;

        public bool SeparateFiles { get { return separateFiles;}}
        public bool On3Columns { get { return on3Columns; } }
        public bool FillTargetWithSupport { get { return fillTargetWithSupport; } }
        public bool SkipUntranslatable { get { return skipUntranslatable; } }
        public bool BaseStringAsAttribute { get { return baseStringAsAttribute; } }

        public AskHTMLExport()
        {
            InitializeComponent();
            if (!SolutionDocument.LocalInfo.UseSupportDictionaryWhenAvailable)
            {
                rbtOn2Columns.Checked = true;
                rbtOn3Columns.Enabled = false;
                rbtTargetFillWithSupport.Text = "fill target with base";
                chkSkipUntranslatable.Enabled = false;
            }
        }

        private void AskHTMLExport_FormClosing(object sender, FormClosingEventArgs e)
        {
            on3Columns = rbtOn3Columns.Checked;
            separateFiles = chkSeparateFiles.Checked;
            fillTargetWithSupport = rbtTargetFillWithSupport.Checked;
            skipUntranslatable = chkSkipUntranslatable.Checked;
            baseStringAsAttribute = chkBaseStringAsAttribute.Checked;
        }

    }
}

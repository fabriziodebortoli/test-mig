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
    public class HTMLImportFlags
    {
        internal bool overwriteExisting = false;
        internal bool skipIfBaseDoNotMatch = true;
        internal bool verboseOutput = false;

        public bool OverwriteExisting { get { return overwriteExisting; } }
        public bool SkipIfBaseDoNotMatch { get { return skipIfBaseDoNotMatch; } }
        public bool VerboseOutput { get { return verboseOutput; } }
    }

    public partial class AskHTMLImport : Form
    {
        private HTMLImportFlags importFlags = new HTMLImportFlags();

        public HTMLImportFlags ImportFlags { get { return importFlags; } }

        public AskHTMLImport()
        {
            InitializeComponent();
            chkOverwiteExisting.Checked = importFlags.overwriteExisting;
            chkSkipIfBaseDoNotMatch.Checked = importFlags.skipIfBaseDoNotMatch;
            chkVerboseOutput.Checked = importFlags.verboseOutput;
        }

        private void AskHTMLImport_FormClosing(object sender, FormClosingEventArgs e)
        {
            importFlags.overwriteExisting = chkOverwiteExisting.Checked;
            importFlags.skipIfBaseDoNotMatch = chkSkipIfBaseDoNotMatch.Checked;
            importFlags.verboseOutput = chkVerboseOutput.Checked;
        }
    }
}

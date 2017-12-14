using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
    /// <summary>
    /// MenuManagerStatusBar
    /// </summary>
    public class MenuManagerStatusStrip : StatusStrip
    {
        ToolStripStatusLabel infoLabel;
        ToolStripStatusLabel spaceLabel;
        ToolStripProgressBar infoProgress;

        //---------------------------------------------------------------------------
        [Browsable(false)]
        public string InfoText
        {
            get
            {
                return String.Empty;
            }
            set
            {
                if (infoLabel != null && !infoLabel.IsDisposed)
                    SetInfoLabelText(value);
            }
        }

        //---------------------------------------------------------------------------
        [Browsable(false)]
        public bool IsProgressBarVisible
        {
            get 
            {
                return (infoProgress != null && !infoProgress.IsDisposed && infoProgress.Visible);
            }
            set
            { 
                if (infoProgress != null && !infoProgress.IsDisposed)
                    infoProgress.Visible = value; 
            }
        }

        //---------------------------------------------------------------------------
        [Browsable(false)]
        public int InfoProgressStep
        {
            get
            {
                return (infoProgress != null && !infoProgress.IsDisposed) ? infoProgress.Step : 0; 
            }
            set 
            {
                SetInfoProgressBarStep(value);
            }
        }

        //---------------------------------------------------------------------------
        [Browsable(false)]
        public int InfoProgressMinimum
        {
            get 
            { 
                return (infoProgress != null && !infoProgress.IsDisposed) ? infoProgress.Minimum : 0;
            }
            set
            { 
                SetInfoProgressBarMinimum(value); 
            }
        }

        //---------------------------------------------------------------------------
        [Browsable(false)]
        public int InfoProgressMaximum
        {
            get
            { 
                return (infoProgress != null && !infoProgress.IsDisposed) ? infoProgress.Maximum : 0;
            }
            set 
            { 
                SetInfoProgressBarMaximum(value); 
            }
        }

        //---------------------------------------------------------------------------
        [Browsable(false)]
        public int InfoProgressValue
        {
            get 
            { 
                return (infoProgress != null && !infoProgress.IsDisposed) ? infoProgress.Value : 0; 
            }
            set
            {
                SetInfoProgressBarValue(value);
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void SetInfoLabelText(string infoLabelText)
        {
            infoLabel.Text = infoLabelText;
            infoLabel.Visible = !String.IsNullOrWhiteSpace(infoLabelText);
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public void SetInfoLabelSize()
        {
            if (infoLabel == null)
                return;

            //if (this.Panels["MessagesStatusBarPanel"] != null)
            //{
            //    int w = this.Panels["MessagesStatusBarPanel"].Width;
            //    //this.Panels["MessagesStatusBarPanel"].Width = w - 3;
            //    infoLabel.Size = new System.Drawing.Size(w - 5, this.ClientRectangle.Height - 12);
            //}
            SetInfoProgressBarSize();
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public void SetInfoProgressBarSize()
        {
            if (infoLabel == null || infoProgress == null)
                return;

            int offset = SizingGrip ? 20 : 2;
            infoProgress.Size = new System.Drawing.Size(this.ClientRectangle.Width / 3 - offset, this.ClientRectangle.Height - 8);
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public void CreateInfoProgressControls(bool createProgressBar = true, ProgressBarStyle progressBarStyle = ProgressBarStyle.Blocks)
        {
            int labelWidth = createProgressBar ? (this.ClientRectangle.Width * 4) / 7 : (this.ClientRectangle.Width - 8);

            if (infoLabel == null || infoLabel.IsDisposed)
            {
                infoLabel = new System.Windows.Forms.ToolStripStatusLabel();
                infoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                infoLabel.BorderStyle = Border3DStyle.Flat;
                SetInfoLabelSize();
                infoLabel.Name = "infoLabel";
                infoLabel.Visible = true;
                infoLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            }

            if (createProgressBar && (infoProgress == null || infoProgress.IsDisposed))
            {
                if (spaceLabel == null || spaceLabel.IsDisposed)
                {
                    //trucco per mettere la progress a destra
                    spaceLabel = new ToolStripStatusLabel();
                    spaceLabel.Text = String.Empty;
                    spaceLabel.Name = "spaceLabelToPushProgressToTheRightEdge";
                    spaceLabel.Spring = true;
                }

                infoProgress = new System.Windows.Forms.ToolStripProgressBar();
                SetInfoProgressBarSize();
                infoProgress.Step = 1;
                infoProgress.Name = "infoProgress";
                infoProgress.Enabled = true;
                infoProgress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                infoProgress.Alignment = ToolStripItemAlignment.Right;
                infoProgress.ControlAlign = System.Drawing.ContentAlignment.MiddleRight;
                infoProgress.Style = progressBarStyle;
                infoProgress.MarqueeAnimationSpeed = 20;
            }

            if (!this.Items.Contains(infoLabel))
                this.Items.Add(infoLabel);

            if (createProgressBar && !this.Items.Contains(spaceLabel))
                this.Items.Add(spaceLabel);
            
            if (createProgressBar && !this.Items.Contains(infoProgress))
                this.Items.Add(infoProgress);
        }


        //--------------------------------------------------------------------------------------------------------------------------------
        public void DestroyInfoProgressControls()
        {
            //modifica ilaria: la label sovrascrive quello che sta sotto (alcune status bar ospitano pannelli)
            //invece di distruggerla la rendo invisibile e poi visibile quando qualcuno vuole scriverci.
            this.SuspendLayout();
            if (infoLabel != null)
            {
                infoLabel.Visible = false;
                if (this.Items.Contains(infoLabel))
                    this.Items.Remove(infoLabel);
                infoLabel.Dispose();
                infoLabel = null;
            }
            if (infoProgress != null)
            {
                if (this.Items.Contains(infoProgress))
                    this.Items.Remove(infoProgress);
                infoProgress.Dispose();
                infoProgress = null;
            }
            if (spaceLabel != null)
            {
                spaceLabel.Visible = false;
                if (this.Items.Contains(spaceLabel))
                    this.Items.Remove(spaceLabel);
                spaceLabel.Dispose();
                spaceLabel = null;
            }
            this.ResumeLayout();
        }

        //--------------------------------------------------------------------
        protected override void OnResize(EventArgs e)
        {
            // Invoke base class implementation
            base.OnResize(e);

            if (infoLabel != null && !infoLabel.IsDisposed)
                SetInfoLabelSize();
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void SetInfoProgressBarStep(int stepValue)
        {
            if (infoProgress != null && !infoProgress.IsDisposed)
                infoProgress.Step = stepValue;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void SetInfoProgressBarMinimum(int minimumValue)
        {
            if (infoProgress != null && !infoProgress.IsDisposed)
                infoProgress.Minimum = minimumValue;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void SetInfoProgressBarMaximum(int maximumValue)
        {
            if (infoProgress != null && !infoProgress.IsDisposed)
                infoProgress.Maximum = maximumValue;
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        private void SetInfoProgressBarValue(int barValue)
        {
            if (infoProgress != null && !infoProgress.IsDisposed && infoProgress.Maximum >= barValue && infoProgress.Minimum <= barValue)
                infoProgress.Value = barValue;
        }
    }
}

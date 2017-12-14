
using Microarea.TaskBuilderNet.UI.WinControls.AnimatedControls;
namespace Microarea.TaskBuilderNet.UI.WinControls.Labels
{
    partial class TimeIsEnding
    {
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelGoneTime;
        private System.Windows.Forms.Label labelLeftTime;
        private System.Windows.Forms.Label labelTotalTime;
        private AnimatedControl animatedControl1;
        private System.ComponentModel.IContainer components;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            timer1.Stop();

            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TimeIsEnding));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.labelGoneTime = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelLeftTime = new System.Windows.Forms.Label();
            this.labelTotalTime = new System.Windows.Forms.Label();
            this.animatedControl1 = new AnimatedControls.AnimatedControl();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // labelGoneTime
            // 
            resources.ApplyResources(this.labelGoneTime, "labelGoneTime");
            this.labelGoneTime.Name = "labelGoneTime";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // labelLeftTime
            // 
            resources.ApplyResources(this.labelLeftTime, "labelLeftTime");
            this.labelLeftTime.Name = "labelLeftTime";
            // 
            // labelTotalTime
            // 
            resources.ApplyResources(this.labelTotalTime, "labelTotalTime");
            this.labelTotalTime.Name = "labelTotalTime";
            // 
            // animatedControl1
            // 
            this.animatedControl1.AnimatedImage = ((System.Drawing.Bitmap)(resources.GetObject("animatedControl1.AnimatedImage")));
            resources.ApplyResources(this.animatedControl1, "animatedControl1");
            this.animatedControl1.Name = "animatedControl1";
            // 
            // TimeIsEnding
            // 
            this.Controls.Add(this.animatedControl1);
            this.Controls.Add(this.labelTotalTime);
            this.Controls.Add(this.labelLeftTime);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.labelGoneTime);
            this.Name = "TimeIsEnding";
            resources.ApplyResources(this, "$this");
            this.ResumeLayout(false);

        }
        #endregion
    }
}

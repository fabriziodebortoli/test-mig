using Microarea.TaskBuilderNet.Core.Generic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.GenericForms;

namespace Microarea.EasyBuilder.UI
{
    internal partial class EBSplash : ThemedForm, IUserFeedback
    {
        SynchronizationContext syncCtx;
		Pen borderPen = new Pen(Color.Black, 1);

		//-----------------------------------------------------------------------------------------
		public EBSplash()
        {
            syncCtx = SynchronizationContext.Current;
            if (syncCtx == null)
            {
                syncCtx = new WindowsFormsSynchronizationContext();
            }
            InitializeComponent();
        }

		//-----------------------------------------------------------------------------------------
		public void SetBackground(Image backgroundImage)
        {
            syncCtx.Send(new SendOrPostCallback((obj) =>
                {
                    this.pictureBox.BackgroundImage = backgroundImage;
                }
                ), null);
        }

		//-----------------------------------------------------------------------------------------
		public void SetMessage(string message)
        {
            syncCtx.Send(new SendOrPostCallback((obj) => this.lblMessage.Text = message), null);
        }

		//-----------------------------------------------------------------------------------------
		public void SetTitle(string title)
		{
			syncCtx.Send(new SendOrPostCallback((obj) => this.lblTitle.Text = title), null);
		}

		//-----------------------------------------------------------------------------------------
		public static EBSplash StartSplash()
        {
            return StartSplash(Properties.Resources.Splash);
        }

		//-----------------------------------------------------------------------------------------
		public static EBSplash StartSplash(Image backgroundImage)
        {
            var mainFormHandle = CUtility.GetMainFormHandle();
            var mainForm = Form.FromHandle(mainFormHandle);

            Screen screen = null;
            Rectangle workingAreaBounds = Rectangle.Empty;
            mainForm.Invoke(new Action(() =>
            {
                screen = Screen.FromControl(mainForm);
                workingAreaBounds = screen.WorkingArea;
            }));

            ManualResetEvent semaphore = new ManualResetEvent(false);

            EBSplash ebSplash = null;
            ThreadPool.QueueUserWorkItem(
                new WaitCallback(
                    (obj) =>
                    {
                        ebSplash = new EBSplash();
						ExternalAPI.SetWindowLong(ebSplash.Handle,/*GWL_HWNDPARENT*/-8, (int)mainFormHandle);
                        ebSplash.SetBackground(backgroundImage);
                        ebSplash.CenterAndSize(workingAreaBounds);

                        semaphore.Set();
                        Application.Run(ebSplash);
                    }
                    )
                );

            semaphore.WaitOne();

            return ebSplash;
        }

		//-----------------------------------------------------------------------------------------
		private void CenterAndSize(Rectangle workingAreaBounds)
        {
            this.Size = new Size(workingAreaBounds.Width / 3, workingAreaBounds.Height / 3);
            int x = workingAreaBounds.Left + (workingAreaBounds.Width - this.Width) / 2;
            int y = (workingAreaBounds.Height - this.Height) / 2;

            this.Location = new Point(x, y);

        }

		//-----------------------------------------------------------------------------------------
		private void EBSplash_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(this.ClientRectangle.Location, this.ClientRectangle.Size);
            rect.Inflate(-1, -1);
            e.Graphics.DrawRectangle(
                borderPen,
                rect
                );
        }

		//-----------------------------------------------------------------------------------------
		public void Show(bool show)
		{
			this.Visible = show;
		}
	}
}

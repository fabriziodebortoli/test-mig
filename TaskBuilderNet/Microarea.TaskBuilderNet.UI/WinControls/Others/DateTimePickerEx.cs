using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
    public class DateTimePickerWithWeek : DateTimePicker
    {
        [DllImport("User32.dll")]
        private static extern int GetWindowLong(IntPtr h, int index);

        [DllImport("User32.dll")]
        private static extern int SetWindowLong(IntPtr h, int index, int value);

        [DllImport("User32.dll")]
        private static extern IntPtr SendMessage(IntPtr h, int msg, int param, int data);

        [DllImport("User32.dll")]
        private static extern int SendMessage(IntPtr h, int msg, int param, ref Rectangle data);

        [DllImport("User32.dll")]
        private static extern int MoveWindow(IntPtr h, int x, int y, int width, int height, bool repaint);

        [DllImport("User32.dll")]
        private static extern IntPtr GetParent(IntPtr h);

        private const int GWL_STYLE = (-16);
        private const int MCM_FIRST = 0x1000;
        private const int MCM_GETMINREQRECT = (MCM_FIRST + 9);
        private const int MCS_WEEKNUMBERS = 0x4;
        private const int DTM_FIRST = 0x1000;
        private const int DTM_GETMONTHCAL = (DTM_FIRST + 8);

        private bool m_ShowWeekNumbers = true;

        public bool ShowWeekNumbers
        {
            get
            {
                return m_ShowWeekNumbers;
            }
            set
            {
                m_ShowWeekNumbers = value;
            }
        }

        protected override void OnDropDown(EventArgs e)
        {
            IntPtr monthView = SendMessage(Handle, DTM_GETMONTHCAL, 0, 0);
            //IntPtr monthViewParent = GetParent(monthView);

            int style = GetWindowLong(monthView, GWL_STYLE);
            if (ShowWeekNumbers)
            {
                style = style | MCS_WEEKNUMBERS;
            }
            else
            {
                style = style & ~MCS_WEEKNUMBERS;
            }
            Rectangle rect = new Rectangle();
            SetWindowLong(monthView, GWL_STYLE, style);

            SendMessage(monthView, MCM_GETMINREQRECT, 0, ref rect);
            MoveWindow(monthView, rect.Left, rect.Top, rect.Right + 2, rect.Bottom + 2, true);
            //MoveWindow(monthViewParent, rect.Left, rect.Top, rect.Right + 2, rect.Bottom + 2, true);

            base.OnDropDown(e);
        }
    }

}

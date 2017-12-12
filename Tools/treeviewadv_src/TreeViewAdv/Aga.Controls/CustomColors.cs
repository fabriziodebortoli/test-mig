using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Collections;

namespace Aga.Controls
{
    public class CustomColor : IDisposable
    {
        private Brush theBrush;
        private Color theColor;
        private bool isMyBrush;

        public Brush TheBrush
        {
            get
            {
                return theBrush;
            }
        }

        public Color TheColor
        {
            get
            {
                return theColor;
            }

            set
            {
                // dispose del mio precedente
                if (isMyBrush && theBrush != null)
                    theBrush.Dispose();

                theBrush = null;
                theColor = value;

                isMyBrush = !theColor.IsSystemColor;

                if (isMyBrush)
                    theBrush = new SolidBrush(value);
                else
                    theBrush = SystemBrushes.FromSystemColor(theColor);
            }
        }

        public CustomColor()
        {
            theBrush = null;
            theColor = Color.Empty;
            isMyBrush = false;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (isMyBrush && theBrush != null)
                        theBrush.Dispose();
                    theBrush = null;
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
    public class CustomColors : IDisposable
    {
        private Hashtable customColors;

        public CustomColors()
        {
            customColors = new Hashtable();
            HightLightBkgColor.TheColor = SystemColors.Highlight;
            HightLightForeColor.TheColor = SystemColors.HighlightText;
            InactiveBorderBkgColor.TheColor = SystemColors.InactiveBorder;
            ScrollbarColor.TheColor = SystemColors.Control;
        }

        public CustomColor HightLightBkgColor
        {
            get {
                    return GetCustomColor("HightLightBkgColor");
                }
        }
        public CustomColor HightLightForeColor
        {
            get
            {
                return GetCustomColor("HightLightForeColor");
            }
        }

        public CustomColor InactiveBorderBkgColor
        {
            get
            {
                return GetCustomColor("InactiveBorderBkgColor");
            }
        }
        public CustomColor ScrollbarColor
        {
            get
            {
                return GetCustomColor("ScrollbarColor");
            }
        }
        public CustomColor GetCustomColor(string name)
        {
            CustomColor customColor = customColors[name] as CustomColor;
            if (customColor == null)
            {
                customColor = new CustomColor();
                customColors.Add(name, customColor);
            }

            return customColor;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    customColors = null;
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}

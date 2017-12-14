using System;
using System.ComponentModel;
using System.Drawing;
using Microarea.TaskBuilderNet.Interfaces.View;
using Telerik.WinControls;
using Telerik.WinControls.Primitives;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	public enum UIBorderStyle { Single, Fixed3D, None }
	public class UILabel : RadLabel, IUIControl, ITBBindableObject
	{
        TBWFCUIControl cui;

        [Browsable(false)]
        virtual public ITBCUI CUI { get { return cui; } }

		UIBorderStyle borderStyle = UIBorderStyle.None;


		//-------------------------------------------------------------------------
		public UILabel()
		{
            ThemeClassName = typeof(RadLabel).ToString();
            cui = new TBWFCUIControl(this, Interfaces.NameSpaceObjectType.Control); 
		}

        //-------------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (cui != null)
            {
                cui.Dispose();
                cui = null;
            }
        }
		[Browsable(false)]
		[Obsolete("do not use RootElement")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//-------------------------------------------------------------------------
		public new RootRadElement RootElement { get { return base.RootElement; } }

        //-------------------------------------------------------------------------
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public object UIValue { get { return base.Text; } }

		//-------------------------------------------------------------------------
		public string DefaultBindingProperty
		{
			get { return "Text"; }
		}

   		//-------------------------------------------------------------------------
		public UIBorderStyle BorderStyle
		{
			get
			{
				return borderStyle;
			}
			set
			{
                borderStyle = value;
                this.BorderVisible = borderStyle != UIBorderStyle.None;
     
                BorderPrimitive bp = new BorderPrimitive();
						
				switch (value)
				{
					case UIBorderStyle.Single:
                        bp = ((BorderPrimitive)(this.LabelElement.Children[1]));
						bp.BoxStyle = BorderBoxStyle.SingleBorder;
						break;
					case UIBorderStyle.Fixed3D:
                        bp = ((BorderPrimitive)(this.LabelElement.Children[1]));
						bp.BoxStyle = BorderBoxStyle.FourBorders;
						bp.BottomColor = Color.White;
						bp.RightColor = Color.White;
						bp.TopColor = Color.FromArgb(160, 160, 160);
						bp.LeftColor = Color.FromArgb(160, 160, 160);
						break;
					default:
						break;
				}
			}
		}
	}
}

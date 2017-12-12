using System.Collections;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	//----------------------------------------------------------------------------
	public class ButtonDesigner : System.Windows.Forms.Design.ControlDesigner 
	{
		//---------------------------------------------------------------------------
		public ButtonDesigner()
		{
		}

		//---------------------------------------------------------------------------
		protected override void PostFilterProperties( IDictionary Properties )
		{
			Properties.Remove( "AllowDrop" );
			Properties.Remove( "BackgroundImage" );
			Properties.Remove( "ForeColor" );
			Properties.Remove( "ImageAlign" );
			Properties.Remove( "ImageIndex" );
			Properties.Remove( "ImageList" );
			Properties.Remove( "FlatStyle" );
		}
	}
}

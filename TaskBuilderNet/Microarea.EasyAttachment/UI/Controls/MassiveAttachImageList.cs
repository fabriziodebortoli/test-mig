using System.Drawing;
using Microarea.EasyAttachment.Components;

namespace Microarea.EasyAttachment.UI.Controls
{
	public partial class MassiveAttachImageList 
	{	

		//--------------------------------------------------------------------
		public static Image GetResultImage(MassiveResult result)
		{
			switch (result)
			{
				case MassiveResult.Failed:
					return Microarea.EasyAttachment.Properties.Resources.KO; 
				case MassiveResult.WithError:
					return Microarea.EasyAttachment.Properties.Resources.infoSmall; 
				case MassiveResult.Done:
                case MassiveResult.Ignored:
					return Microarea.EasyAttachment.Properties.Resources.OK; 
			}

			return Microarea.EasyAttachment.Properties.Resources.KO; 
		}

		//--------------------------------------------------------------------
		public static Image GetStatusImage(MassiveStatus stat)
		{
			switch (stat)
			{
				case MassiveStatus.OnlyBC:
					return Microarea.EasyAttachment.Properties.Resources.Barcode16x16; 
				case MassiveStatus.Papery:
					return Microarea.EasyAttachment.Properties.Resources.Airplane;
				case MassiveStatus.NoBC:
					return Microarea.EasyAttachment.Properties.Resources.Warning;
				case MassiveStatus.BCDuplicated:
					return Microarea.EasyAttachment.Properties.Resources.BarcodeSubstitution16x16;
			}
			return Microarea.EasyAttachment.Properties.Resources.KO;  
		}
	}
}

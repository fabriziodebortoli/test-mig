using System;
using System.Drawing;

using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Woorm.WoormViewer;
using Microarea.TaskBuilderNet.Woorm.ExpressionManager;

namespace Microarea.TaskBuilderNet.Woorm.WoormWebControl
{
	
	//================================================================================        
	
	public class BarCodeViewer : FileProvider
	{
		private BarCode			barCode;
		private	BarCodeWrapper	barCodeWrapper;
		private WoormDocument	woormDocument;

	//--------------------------------------------------------------------------------
		public BarCodeViewer(WoormDocument woorm, BarCode barCode)
			: base(woorm, "bmp")
		{

			if (woorm == null || barCode == null || woorm.ReportSession.PathFinder == null)
				throw new NullReferenceException(WoormWebControlStrings.CostructorError);

			woormDocument = woorm;
			barCodeWrapper = new BarCodeWrapper(woorm.ReportSession.PathFinder);
			this.barCode = barCode;
		}

		//--------------------------------------------------------------------------------
		public string GetBarcodeImageFile(RSjson.WoormValue v, FontElement fe, Rectangle inside, string humanReadeableText)
		{
			if (fe == null) 
				return string.Empty;

			int handle = 0;
			try
			{
				BarCodeWrapper.Type barCodeType;
				string barCodeValue;
				int type;
                int realCheckSum;
                string humanReadable;
                if (Expression.DecodeBarCode(v.FormattedData, out barCodeValue, out type, out realCheckSum, out humanReadable))
					barCodeType = (BarCodeWrapper.Type) type;
				else
					barCodeType = barCode.BarCodeType;

                if (humanReadable != string.Empty)
                    humanReadeableText = humanReadable;

				if (barCodeType == BarCodeWrapper.Type.BC_DEFAULT)
					barCodeType = barCode.BCDefaultType;

				//se l'alias e' diverso da 0, il tipo di barcode e' contenuto nella variabile di woorm con questo alias
				if (barCode.BarCodeTypeAlias != 0)
				{
					string bcType = woormDocument.GetFormattedDataFromAlias(barCode.BarCodeTypeAlias);
					barCodeType = BarCodeWrapper.GetBarCodeType(bcType);
				}
				 
				bool italic = (FontStyle.Italic & fe.FontStyle) == FontStyle.Italic;
				bool bold = (FontStyle.Bold & fe.FontStyle) == FontStyle.Bold;
			
				int x, y, height;
				if
					(	// in questi casi le dimensioni del font vengono comunque decise dalla libreria
					barCodeType == BarCodeWrapper.Type.BC_EAN13	||
					barCodeType == BarCodeWrapper.Type.BC_EAN8	||	
					barCodeType == BarCodeWrapper.Type.BC_UPCA	||
					barCodeType == BarCodeWrapper.Type.BC_UPCE	||
					barCodeType == BarCodeWrapper.Type.BC_UPCE0	||
					barCodeType == BarCodeWrapper.Type.BC_UPCE1
					)
				{
					x = ((inside.Right + inside.Left) / 2);
					y = ((inside.Bottom + inside.Top) / 2);
					height = barCode.Vertical ? BarCodeWrapper.MulDiv(inside.Width, 2, 3) : BarCodeWrapper.MulDiv(inside.Height, 2, 3);
				}
				else 
				{
					x = ((inside.Right + inside.Left + (barCode.Vertical ? fe.Size : 0)) / 2);
					y = ((inside.Bottom + inside.Top + (barCode.Vertical ? 0 : fe.Size)) / 2);
					height = barCode.Vertical ? BarCodeWrapper.MulDiv(inside.Width + fe.Size, 2, 3) : BarCodeWrapper.MulDiv(inside.Height + fe.Size, 2, 3);
				}

				BarCodeWrapper.FontStyle fontStyle = italic 
					? bold 
					? BarCodeWrapper.FontStyle.HS_BOLDITALIC 
					: BarCodeWrapper.FontStyle.HS_ITALIC
					: bold
					? BarCodeWrapper.FontStyle.HS_BOLD 
					: BarCodeWrapper.FontStyle.HS_NORMAL;

				// EAN128 Customizations
				BarCodeWrapper.Type realBarCodeType =	
									(	
										barCodeType == BarCodeWrapper.Type.BC_EAN128 ? 
										BarCodeWrapper.Type.BC_UCC128 : 
										barCodeType
									);	

				int realHeight =	(
										barCode.CustomBarHeight > 0 ?
										barCode.CustomBarHeight : 
										height
									);	


				// EAN128 default Module 103
                //if (barCodeType == BarCodeWrapper.Type.BC_EAN128 && barCode.CheckSumType != 0)
                //    realCheckSum = 1;

				handle = BarCodeWrapper.CreateBarCode
					(
					Convert.ToInt16(realBarCodeType), 
					Convert.ToInt16(barCode.Vertical ? 1 : 0),					// int rotation : 0, 1, 2, 3 corrispondono a 0, +90, +180, +270
					v.TextColor.ToArgb() & 0x00FFFFFF,							//neutralizzo i due byte di ordine superiore per avere la sola componente RGB		
					barCode.NarrowBar,
					Convert.ToInt16(realHeight),								// int barHeight: length of a bar in millimetri
					Convert.ToInt16(barCode.ShowLabel ? BarCodeWrapper.LabelType.HR_BELOW : BarCodeWrapper.LabelType.HR_OFF),
					fe.FaceName,	
					Convert.ToInt16(fe.Size),	
					Convert.ToInt16(fontStyle),			
					0,											
																	
					0,											
					Convert.ToInt16(realCheckSum),
					0											
					);

				string barCodeFile = base.GenericTmpFile;
			
				string humanText = humanReadeableText;
				if (humanText == string.Empty)
					humanText = barCodeValue;
				
				short result = BarCodeWrapper.DrawBarCodeToFile
					(
					barCodeFile,
					0,				//bmp (unico formato disponibile)
					0,				
					0,				
					handle,
					barCodeValue,
					barCode.ShowLabel ? humanText : string.Empty
					) ;
				
				if (result != 0)
					return string.Empty;

				return "~\\" + base.GenericTmpFileRelPath;
			}
			catch (Exception ex)
			{
				throw new Exception(WoormWebControlStrings.CreatingError, ex);
			}
			finally
			{
				if (handle != 0)
					BarCodeWrapper.DeleteBarCode(handle);		
			}
		}
	}
}

using System;

namespace Microarea.TaskBuilderNet.Core.CoreTypes
{
	/// <summary>
	/// Summary description for UnitConvertions.
	/// </summary>
	/// ================================================================================
	public class UnitConvertions
	{
		public enum MeasureUnits { CM = 0, INCH = 1, STD_MU = 999 };

		// scaling factor used in scaling object from video to preview and printer device.
		// Can be modified by application but in consistent way.
		//
		public const int SCALING_FACTOR	= 96;

		//----------------------------------------------------------------------------
		public UnitConvertions()
		{
		}

		//----------------------------------------------------------------------------
		public static int MUtoLP(double muv, MeasureUnits baseMu, double scale, int nDec)
		{         
			// convert to inch if user works in cm
			if (baseMu == MeasureUnits.CM)
				muv /= 2.54;

			// round to nDec decimal digits		
			muv = Math.Round(muv, nDec);
	
			// convert to base user mesurement units (inch or cm)
			muv /= scale;

			int sign = (muv < 0.0 ? -1 : 1);
		
			// convert from inch to logical units
			if (Math.Abs(muv) * SCALING_FACTOR <= int.MaxValue)
			{
				muv *= SCALING_FACTOR;
				return sign * (int) Math.Floor(Math.Abs(muv) + 0.5);
			}	
			else
				return sign * int.MaxValue;
		}

		//----------------------------------------------------------------------------
		public static double LPtoMU(int lpv, MeasureUnits baseMu, double scale, int nDec)
		{                      
			// convert from logical units to inch
			double muv = (double) lpv / SCALING_FACTOR;
	
			// convert to cm if user works in cm
			if (baseMu == MeasureUnits.CM)
				muv *= 2.54;
               
			// convert to derived user mesurement units (INCH/scale or CM/scale)
			muv *= scale;
    
			// round to nDec decimal digits
			muv = Math.Round(muv, nDec);
	
			return muv;
		}
	}
}

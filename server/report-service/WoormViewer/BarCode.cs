using System;
using System.Runtime.InteropServices;
using Microarea.Common.NameSolver;

using Microarea.Common.Generic;
using Microarea.Common.Lexan;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.RSWeb.WoormViewer
{
    /// <summary>
    /// Summary description for TextObj.
    /// </summary>
    /// ================================================================================
    public class BarCode
	{
		public string					BarCodeTypeName;
		public BarCodeWrapper.Type		BarCodeType = BarCodeWrapper.Type.BC_DEFAULT;
		public BarCodeWrapper.Type		BCDefaultType = BarCodeWrapper.Type.BC_EAN13;
		public short					NarrowBar = 1;			// thickness of a narrow bar,
		public bool						Vertical = false;
		public bool						ShowLabel = true;
		public int						CheckSumType = -1;		// checsum type value
		public int						CustomBarHeight = -1;	// customizable bar height in logical units
		public ushort					HumanTextAlias = 0;		// human text is contained into the associated alias
		public ushort					BarCodeTypeAlias = 0;	// bar code type is contained into the associated alias 



		//--------------------------------------------------------------------------------
		public BarCode(PathFinder pathFinder)
		{
			string defaultBarCode = ReadSetting.GetSettings(pathFinder, "Framework.TbGenlib.Settings", "Environment", "BarCodeType", BarCodeWrapper.GetBarCodeDescription(BarCodeWrapper.Type.BC_EAN13)) as string;
			if (!string.IsNullOrWhiteSpace(defaultBarCode))
			{
				BCDefaultType =  BarCodeWrapper.GetBarCodeType(defaultBarCode as string);
			}
		}

		//--------------------------------------------------------------------------------
		public bool Unparse(Unparser unparser, bool newLine = true)
		{
			unparser.WriteTag  (Token.BARCODE, false);
			unparser.WriteOpen(false);
	
			string temp = string.Empty;
			if (BarCodeTypeAlias == 0)
			{
				foreach (BarCodeWrapper.Type item in  Enum.GetValues(typeof(BarCodeWrapper.Type)))
				{
					if (BarCodeType == item)
					{
						temp = BarCodeWrapper.GetBarCodeDescription(item);
						break;
					}
				}
			    unparser.WriteString (temp,	false);
			}
			else
			    unparser.Write (BarCodeTypeAlias,	false);

			unparser.WriteComma(false);
			unparser.Write  (NarrowBar, false);
			unparser.WriteComma(false);
			unparser.Write (Vertical, false);
			unparser.WriteComma(false);
			unparser.Write (ShowLabel, false);

			//DataObj* pSetting = AfxGetSettingValue(snsTbGenlib, szEnvironment, szBarCodeType, DataStr(), szTbDefaultSettingFileName);
			//CString sDefaultBarcode = pSetting ? pSetting->Str() : _T("");	
			string defaultBarCode = string.Empty; //TODOLUCA    manca la lettura del setting
			BarCodeWrapper.Type barcodeDefaultType = 0;
			
			if (!defaultBarCode.IsNullOrEmpty()) 
			{
				foreach (BarCodeWrapper.Type item in  Enum.GetValues(typeof(BarCodeWrapper.Type)))
				{	
					temp = BarCodeWrapper.GetBarCodeDescription(item);
					if (defaultBarCode.CompareNoCase(temp))
			        {
						barcodeDefaultType = item;
			            break;
			        }
				}
			}

			// EAN128 barcode has other addon parameters
			if	(
					BarCodeType == BarCodeWrapper.Type.BC_EAN128 || 
					(BarCodeType == BarCodeWrapper.Type.BC_DEFAULT &&
					barcodeDefaultType == BarCodeWrapper.Type.BC_EAN128) ||
					BarCodeTypeAlias > 0
				)
			{
				unparser.WriteComma(false);
				unparser.Write(CheckSumType, false);

				unparser.WriteComma(false);
				unparser.Write(CustomBarHeight, false);

				if (HumanTextAlias > 0)
				{
					unparser.WriteComma(false);
					unparser.Write(HumanTextAlias, false);
				}
			}
			else if (CustomBarHeight > 0 || HumanTextAlias > 0)
			{
				unparser.WriteComma(false);
				unparser.Write(-1, false);

				unparser.WriteComma(false);
				unparser.Write(CustomBarHeight,false);

				if (HumanTextAlias > 0)
				{
					unparser.WriteComma(false);
					unparser.Write(HumanTextAlias,false);
				}
			}

			unparser.WriteClose(newLine);
			return true;
		}

        public string ToJson()
        {
            string s = "\"barcode\":{" +
                (BarCodeType == BarCodeWrapper.Type.BC_DEFAULT ? BCDefaultType : BarCodeType).ToJson("type") + "," +
                Vertical.ToJson("rotate") + "," +
                ShowLabel.ToJson("includetext") +
                (CustomBarHeight == -1 ? "" : "," + CustomBarHeight.ToJson("custom-height")) +
                "}";

            return s;
        }

    }

	//================================================================================
	public class BarCodeWrapper
	{
        public enum Type
        {
            // costanti prese dal file bclw.h della libreri, del bar code
            BC_DEFAULT=0,
            BC_UPCA = 1,
            BC_UPCE = 2,
            BC_EAN13 = 3,
            BC_EAN8 = 4,
            BC_EANJAN13 = BC_EAN13,
            BC_EANJAN8 = BC_EAN8,
            BC_CODE39 = 5,
            BC_EXT39 = 6,
            BC_INT25 = 7,
            BC_CODE128 = 8,
            BC_CODABAR = 9,
            BC_ZIP = 10,
            BC_MSIPLESSEY = 11,
            BC_CODE93 = 12,
            BC_EXT93 = 13,
            BC_UCC128 = 14,
            BC_HIBC = 15,
            BC_PDF417 = 16,
            BC_UPCE0 = 17,
            BC_UPCE1 = 18,
            BC_CODE128A = 19,
            BC_CODE128B = 20,
            BC_CODE128C = 21,
            BC_EAN128 = 22,
            BC_DATAMATRIX = 23,
            BC_MICROQR = 24,
            BC_QR = 25

        }


        public enum LabelType
		{
			HR_OFF			= 0,
			HR_BELOWLEFT	= 1,
			HR_BELOW		= 2,
			HR_BELOWRIGHT	= 3,
			HR_ABOVELEFT	= 4,
			HR_ABOVE		= 5,
			HR_ABOVERIGHT	= 6
		}

		private const string	dllName				= "BCLW32.DLL";
		private static IntPtr	dllHandle			= IntPtr.Zero;
		private static int		references			= 0;

		//--------------------------------------------------------------------------------
		public BarCodeWrapper(PathFinder pf)
		{
			lock (typeof(BarCodeWrapper))
			{
				if (dllHandle == IntPtr.Zero)
				{
                    //TODO RSWEB
					//string dllSourceFile = Path.Combine(Functions.GetExecutingAssemblyFolderPath(), dllName);
					//dllHandle = LoadLibrary(dllSourceFile);
				}
				references++;
			}
		}

		//--------------------------------------------------------------------------------
		~BarCodeWrapper()
		{
			lock (typeof(BarCodeWrapper))
			{
				references--;
				if (references == 0)
				{
					FreeLibrary(dllHandle);
					dllHandle = IntPtr.Zero;
				}
			}
		}

		//--------------------------------------------------------------------------------
		[DllImport("Kernel32.dll", EntryPoint="LoadLibraryW", CharSet=CharSet.Unicode)]
		private static extern IntPtr LoadLibrary(string path);

		//--------------------------------------------------------------------------------
		[DllImport("Kernel32.dll", EntryPoint="FreeLibrary", CharSet=CharSet.Unicode)]
		private static extern void FreeLibrary(IntPtr handle);
 	
		//--------------------------------------------------------------------------------
		[DllImport("Kernel32.DLL", CharSet=CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern int MulDiv (int number, int numerator, int denominator);

		//--------------------------------------------------------------------------------
		[DllImport("BCLW32.DLL", EntryPoint="CreateBarCode", CharSet=CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern int CreateBarCode (
			short barCodeType, 
			short rotation, 
			int color,
			short barWidth,
			short barHeight,
	   
			short humanWhere,
			string humanFaceName,
			short humanSize,
			short humanStyle,

			short supplement,
			short ratio,
			short checkSum,
			short pdfSecurity
			);
		
		//--------------------------------------------------------------------------------
		[DllImport("BCLW32.DLL", EntryPoint = "DeleteBarCode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern short DeleteBarCode (int handle);
		
		//--------------------------------------------------------------------------------
		[DllImport("BCLW32.DLL", EntryPoint="DrawBarCodeToFile", CharSet=CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern short DrawBarCodeToFile (
			string lpFileName,
			short fileType,
			short xdpi,				//   to override granularity for wysiwyg - !!not a scale!!
			short ydpi,

			int handle,				// bar code definition
			string lpCodeText,		// text to encode
			string lpHumanText		// text to use in the human readable
			);

        

        //todo ilaria barcode  UNIFICARE CON GESTIONE BARCODE EASYATTACHMENT CHE USA STESSO ENUMERATIVO MA DUPLICATO PER NON INCLUDERE WOORM
        //(FIND GetBarCodeType in EA)
        //--------------------------------------------------------------------------------
        public static Type GetBarCodeType(string description)
		{							 
			switch(description)		 
			{							 
				case	"<predefinito>"	:	return  Type.BC_DEFAULT			; 
				case	"UPCA"			:	return  Type.BC_UPCA			; 
				case	"UPCE"			:	return  Type.BC_UPCE			;
				case	"EAN13"			:	return  Type.BC_EAN13			;
				case	"EAN8"			:	return  Type.BC_EAN8			;
				case	"CODE39"		:	return  Type.BC_CODE39			;
				case	"EXT39"			:	return  Type.BC_EXT39			;
				case	"INT25"			:	return  Type.BC_INT25			;
				case	"CODE128"		:	return  Type.BC_CODE128			;
				case	"CODABAR"		:	return  Type.BC_CODABAR			;
				case	"ZIP"			:	return  Type.BC_ZIP				;
				case	"MSIPLESSEY"	:	return  Type.BC_MSIPLESSEY		;
				case	"CODE93"		:	return  Type.BC_CODE93			;
				case	"EXT93"			:	return  Type.BC_EXT93			;
				case	"UCC128"		:	return  Type.BC_UCC128			;
				case	"HIBC"			:	return  Type.BC_HIBC			;
				case	"PDF417"		:	return  Type.BC_PDF417			;
				case	"UPCE0"			:	return  Type.BC_UPCE0			;
				case	"UPCE1"			:	return  Type.BC_UPCE1			;
				case	"CODE128A"		:	return  Type.BC_CODE128A		;
				case	"CODE128B"		:	return  Type.BC_CODE128B		; 
				case	"CODE128C"		:	return  Type.BC_CODE128C		;
				case	"EAN128"		:	return  Type.BC_EAN128			;
                case    "DataMatrix"    :   return  Type.BC_DATAMATRIX      ;
                case    "MicroQR"        :   return  Type.BC_MICROQR           ;
                case    "QR"            :   return  Type.BC_QR               ;            
                default					:	return	Type.BC_DEFAULT			;
			}							
		}

		//--------------------------------------------------------------------------------
		public static string GetBarCodeDescription(BarCodeWrapper.Type barcodeType)
		{
			switch (barcodeType)
			{
				case Type.BC_DEFAULT: return "<predefinito>";
				case Type.BC_UPCA: return "UPCA";
				case Type.BC_UPCE: return "UPCE";
				case Type.BC_EAN13: return "EAN13";
				case Type.BC_EAN8: return "EAN8";
				case Type.BC_CODE39: return "CODE39";
				case Type.BC_EXT39: return "EXT39";
				case Type.BC_INT25: return "INT25";
				case Type.BC_CODE128: return "CODE128";
				case Type.BC_CODABAR: return "CODABAR";
				case Type.BC_ZIP: return "ZIP";
				case Type.BC_MSIPLESSEY: return "MSIPLESSEY";
				case Type.BC_CODE93: return "CODE93";
				case Type.BC_EXT93: return "EXT93";
				case Type.BC_UCC128: return "UCC128";
				case Type.BC_HIBC: return "HIBC";
				case Type.BC_PDF417: return "PDF417";
				case Type.BC_UPCE0: return "UPCE0";
				case Type.BC_UPCE1: return "UPCE1";
				case Type.BC_CODE128A: return "CODE128A";
				case Type.BC_CODE128B: return "CODE128B";
				case Type.BC_CODE128C: return "CODE128C";
				case Type.BC_EAN128: return "EAN128";
                case Type.BC_DATAMATRIX: return "DataMatrix";
                case Type.BC_MICROQR: return "MicroQR";
                case Type.BC_QR: return "QR";
                default: return "<predefinito>";
			}

		}	
	}
}

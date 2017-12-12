using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Microarea.Library.RTPatchWrapper
{
	/// <summary>
	/// Questa classe è un wrap sulla dll "patchw32.dll" che permette di
	/// effettuare dei patch apply invocando direttamente l'API senza dovere
	/// usare un process a parte.
	/// </summary>
	/// <remarks>
	/// [Federico:]
	/// La dll "patchw32.dll" deve essere manualmente resa disponibile al
	/// programma che usa questa wrapping class, visto che VS.NET non permette
	/// di esprimere nel progetto dei reference a dll che non siano COM.
	/// Consiglio di copiarla sia in bin\Release che in bin\Debug
	/// 
	/// Il codice è un port dell'originale scritto in VB.NET trovato nei sample
	/// dell'installazione di RTPatch (l'ho tradotto per semplificarne la manutenzione).
	/// 
	/// Oltre a modificare talvolta i nomi di variabili per conformarmi agli
	/// standard di C#, la modifica più ecclatante è stata l'uso di degli
	/// StringBuilder in luogo degli String usati nel codice VB.NET per copiarvi
	/// il contenuto di aree di memoria; questo perché in .Net le stringhe sono
	/// memorizzate in modo da non essere modificabili, due stringhe uguali in
	/// realtà puntano alla stessa area di memoria. Modificarle coercitivamente
	/// sarebbe una violazione delle logiche di memorizzazione delle stringhe.
	/// 
	/// Nel sample la funzione di callback è definita come static (Shared in VB.NET),
	/// e tale l'ho lasciata, e si usava un file scrivere l'output, per cui
	/// implicitamente si intendeva usare la funzione non in situazioni di concorrenza;
	/// ho mantenuto tale approccio usando come output una string property, adottando
	/// la piccola accortezza di mascherare l'accesso alla API nativa (oltre che per
	/// rendere l'uso della classe più semplice) per impedire l'uso del parametro
	/// waitFlag = false che effettuarebbe la chiamata in modo asincrono. Nulla
	/// tuttavia impedirebbe l'uso concorrente del metodo (non so se l'API sia thread safe)
	/// 
	/// TODO:
	///		*	uso di metodo e output di istanza e non statici o alternativamente
	///			semaforizzazione della chiamata alla API
	///		*	eventi (meglio se di istanza) legati ai passi di callback
	///		*	uso di enumerativo parlante come valore di ritorno in luogo di int
	///		*	gestione da codice degli errori di RTPatch (ora gli faccio scrivere patch.err)
	/// </remarks>
	//=========================================================================
	public class RTPatch
	{
		private static StringBuilder outputMessage = new StringBuilder();
		public static string OutputMessage	{ get { return outputMessage.ToString(); } }
		
		[DllImport("kernel32.dll", EntryPoint="lstrcpyA", CharSet = CharSet.Ansi)]
		private static extern int lstrcpy(StringBuilder lpDest, IntPtr lpSource);
		
		[DllImport("kernel32.dll", EntryPoint="RtlMoveMemory")]
		private static extern void CopyMemory(ref int pdst, IntPtr pSrc, int nBytes);
		
		[DllImport("kernel32.dll", EntryPoint="lstrlenA")]
		private static extern int lstrlenByNum(IntPtr lpString);
		
		[DllImport("kernel32.dll", EntryPoint="RtlMoveMemory")]
		private static extern void CopyMemory2(ref IntPtr pdst, IntPtr pSrc, int nBytes);
		
		// Declare the apply entry point
		[DllImport("patchw32.dll", EntryPoint="RTPatchApply32@12", CharSet=CharSet.Ansi)]
		private static extern int RTPatchApply32(string cmdLine, CallBack patchCallBack, bool waitFlag);

		//---------------------------------------------------------------------
		public static int PatchApply(string dirToPatchPath, string patchFileFullName)
		{
			// NOTE:	-e[rrorfile]		Causes patch to log all error and warning messages to the file patch.err
			//			-not[zcheck]		Causes patch to apply patches for files with timestamps that differ from
			//								the original timestamps by an integral number of hours.  This can happen 
			//								when files are transferred across time zones on file systems (such as NTFS)
			//								that maintain a file time as “Greenwich Mean Time + Local Offset”. 
			//			-i[gnoremissing]	Informs patch that it is not an error condition if an original file is missing.
			//			-nos[ubdirsearch]	Prevents patch from searching the subdirectories of the update directory.
			string cmdLineMask = "-i -e -not -nos \"{0}\" \"{1}\"";
			string cmdLine = string.Format(CultureInfo.InvariantCulture, cmdLineMask, dirToPatchPath, patchFileFullName);

			CallBack callbackDelegateInstance = new CallBack(RTPatch.RTPatchCallBack);
			int res;
			outputMessage = new StringBuilder();
			res = RTPatch.RTPatchApply32(cmdLine, callbackDelegateInstance, true);
			return res;
		}
		
		// Declare the CallBack function
		//---------------------------------------------------------------------
		private static IntPtr RTPatchCallBack(int Id, IntPtr Parm)
		{
			int lRetCode;
			int lLength;
			bool abort = false;
			string sYesString;
			StringBuilder sBuffer;
			sYesString = new String('y', 1);

			IntPtr result = Marshal.StringToHGlobalAnsi("");

			string msgLine;		//by Fred
			msgLine = string.Empty;

			int perComplete = 0;	// by Fred: lo inizializzo
			int numPatchFiles = 0;	// by Fred: lo inizializzo
			StringBuilder sysName;
			StringBuilder sysLoc;
			switch (Id)
			{
				case 1 :
					//print warning and error messages
					lLength = lstrlenByNum(Parm);
					sBuffer = new StringBuilder(lLength);
					lRetCode = lstrcpy(sBuffer, Parm);
					//msgLine = Parm.ToString();	// by fred: mi sembra sbagliato...
					msgLine = sBuffer.ToString();	// by fred
					Debug.WriteLine(msgLine);
					outputMessage.Append(msgLine);
					outputMessage.Append(Environment.NewLine);
//					OutputMessage += msgLine + Environment.NewLine;
					break;
				case 2 :	goto case 1;
				case 3 :	goto case 1;
				case 4 :	goto case 1;

				case 5 :	//percent complete this file
					// Parm points to a 16-bit Int, thus do
					// a memcopy at the address given in Parm
					// for 16 bits, or 2 bytes.

					//Note that the perComplete ranges from 0x0000 to
					// 0x8000 (or 0 to 32768 in Base10).
					CopyMemory(ref perComplete, Parm, 2);
					break;

				case 6 :	//number of patch files to process
					// Parm points to a 32-bit Int, thus do
					// a memcopy at the address given in Parm
					// for 32 bits, or 4 bytes.
					CopyMemory(ref numPatchFiles, Parm, 4);
					break;

				case 7 :	//patch file start
					break;

				case 8 :	//current patch file complete
					break;

				case 9 :
					//print progress messages
					lLength = lstrlenByNum(Parm);
					sBuffer = new StringBuilder(lLength);
					lRetCode = lstrcpy(sBuffer, Parm);
					msgLine = sBuffer.ToString();
					Debug.Write(msgLine);
					outputMessage.Append(msgLine);
					break;
				case 10 :	goto case 9;
				case 11 :	goto case 9;
				case 12 :	goto case 9;

				case 13 :
					//patch file dialog, return pointer to
					// the full path of the patch file
					msgLine = "Aborted on Patch File Loaction Dialog (ID #13)";
					Debug.WriteLine(msgLine);
					outputMessage.Append(msgLine);
					outputMessage.Append(Environment.NewLine);
					abort = true;
					break;
				
				case 14 :
					//invalid patch file
					msgLine = "Aborted on Invalid Patch File (ID #14)";
					Debug.WriteLine(msgLine);
					outputMessage.Append(msgLine);
					outputMessage.Append(Environment.NewLine);
					abort = true;
					break;

				case 15 :
					//password dialog. 
					// Use the following syntax
					// RTPatchCallBack = Runtime.InteropServices.Marshal.StringToHGlobalAnsi("password")
					msgLine = "Aborted on Password Dialog (ID #15)";
					Debug.WriteLine(msgLine);
					outputMessage.Append(msgLine);
					outputMessage.Append(Environment.NewLine);
					//abort = true;
					break;

				case 16 :
					//invalid password
					msgLine = "Aborted on Invalid Password (ID #16)";
					Debug.WriteLine(msgLine);
					outputMessage.Append(msgLine);
					outputMessage.Append(Environment.NewLine);
					abort = true;
					break;

				case 17 :
					//next disk dialog
					msgLine = "Aborted on Next Disk Dialog (ID #17)";
					Debug.WriteLine(msgLine);
					outputMessage.Append(msgLine);
					outputMessage.Append(Environment.NewLine);
					abort = true;
					break;

				case 18 :
					//invalid disk alert
					msgLine = "Aborted on Invalid Disk Alert (ID #18)";
					Debug.WriteLine(msgLine);
					outputMessage.Append(msgLine);
					outputMessage.Append(Environment.NewLine);
					abort = true;
					break;

				case 19 :
					//location confirm

					// Sample code provided demonstates how to access
					// the systemName and systemLocation data.
					// sysName and sysLoc hold this information after
					// execution of the code below.  At this point, you may
					// wish to prompt the user to confirm this location.

					// We simply accept any location.

					IntPtr[] buffer = new IntPtr[2];

					// Copy array of string pointers into an array of integers
					CopyMemory2(ref buffer[0], Parm, 2 * 4);

					//Copy the systemName info into sysName
					lLength = lstrlenByNum(buffer[0]);
					sysName = new StringBuilder(lLength);
					lRetCode = lstrcpy(sysName, buffer[0]);

					//Copy the systemLocation info into sysLoc
					lLength = lstrlenByNum(buffer[1]);
					sysLoc = new StringBuilder(lLength);
					lRetCode = lstrcpy(sysLoc, buffer[1]);

					// Return the address of a "y" string.  Note above comment
					result = Marshal.StringToHGlobalAnsi(sYesString);
					break;

				case 20 :
					// location dialog
					msgLine = "Aborted on Location Dialog (ID #20)";
					Debug.WriteLine(msgLine);
					outputMessage.Append(msgLine);
					outputMessage.Append(Environment.NewLine);
					abort = true;
					break;

				case 21 :	//idle callBack
					break;
				case 22 :	//searching for system location
					break;
			}

			if (abort)
				// This will cause RTPatchApply32 to ABORT the patch
				result = new System.IntPtr(0);

			return result;
		}

		//=========================================================================
		private delegate IntPtr CallBack(int id, IntPtr parm);
		// Sample CallBack function.  Note that there is very little
		//  functionality in this CallBack.  It is meant as a guide on how
		//  to define a CallBack that is of the form expected by our DLL
		//
		// The return value of this CallBack is often not used by RTPatchApply32.
		//  However, returning a NULL pointer will cause the patch application
		//  process to abort. 
		//
		// In many Id cases we simply abort the patch.  You would want to
		//  do something reasonable in these situations (ie ask user for
		//  some input and then set CallBack to the result of that query.
		//
		// Note that in some cases, you are required to return a string
		//  (e.g., CallBack 15 - password). To do so, use this syntax:
		//  RTPatchCallBack = Runtime.InteropServices.Marshal.StringToHGlobalAnsi("string")
	}
}

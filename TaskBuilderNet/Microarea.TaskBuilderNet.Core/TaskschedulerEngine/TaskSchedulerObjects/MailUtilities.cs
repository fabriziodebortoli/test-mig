using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Microarea.TaskBuilderNet.Core.TaskschedulerEngine.TaskSchedulerObjects
{
    /// <summary>
    /// This class implements RFC 822 compliant email validator routines.
    /// Very generally speaking, an email address, according to RFC 822, can be broken into 
    /// 3 primary parts.
    ///   - Quoted Identifier 
    ///     This is the part of the address that gives the greater amount of identification
    ///     of the email mailbox owner. It's usually the full name of the sender/recipient.
    ///   - Local Part
    ///     This is the actual mailbox/alias name hosted on the destination server. It's the 
    ///     part before the @ sign. 
    ///   - Domain 
    ///     This is typically the FQDN (Fully Qualified Domain Name) where the mailbox identified
    ///     by the local part is hosted. 
    /// </summary>
    //============================================================================================
    public class WTEEmailAddress
	{
		// static so that it's shared accross multiple instances.
		private static Regex oRegex;

		// Constants to combat backslashitis
		private const string Escape		= @"\\";
		private const string Period		= @"\.";
		private const string Space		= @"\040";
		private const string Tab		= @"\t";
		private const string OpenBr		= @"\[";
		private const string CloseBr	= @"\]";
		private const string OpenParen	= @"\(";
		private const string CloseParen = @"\)";
		private const string NonAscii	= @"\x80-\xff";
		private const string Ctrl		= @"\000-\037";
		private const string CRList		= @"\n\015"; // Should only really be \015
		
		private string	mailBox;
		private string	localPart;
		private string	domain;
		private string	quotedStr;
		private bool	isValid = false;

		//-------------------------------------------------------------------------------------------
		public string LocalPart
		{
			get { return localPart; }
		}

		//-------------------------------------------------------------------------------------------
		public string Domain
		{
			get { return domain; }
		}

		//-------------------------------------------------------------------------------------------
		public string QuotedString
		{
			get { return quotedStr; }
		}

		//-------------------------------------------------------------------------------------------
		public string Mailbox
		{
			get { return mailBox; }
		}

		//-------------------------------------------------------------------------------------------
		public bool IsValid
		{
			get { return isValid; }
		}

		//-------------------------------------------------------------------------------------------
		public WTEEmailAddress()
		{
			// initialise the regex... 
			initRegex();
		}

		//-------------------------------------------------------------------------------------------
		public WTEEmailAddress(string WTEEmailAddress)
		{
			// initialise the regex... 
			initRegex();

			Parse(WTEEmailAddress);
		}

		//-------------------------------------------------------------------------------------------
		public bool Parse(string email)
		{
			// Match against the regex...
			Match m = WTEEmailAddress.oRegex.Match(email);

			isValid = m.Success;
			domain = m.Groups["domain"].ToString();
			localPart = m.Groups["localpart"].ToString();
			mailBox = m.Groups["mailbox"].ToString();
			quotedStr = m.Groups["quotedstr"].ToString();

			return isValid;
		}

		/// <summary>
		/// Init regex initialises the huge regex and compiles it so that it runs a little faster.
		/// </summary>
		private void initRegex()
		{
		
			// for within "";
			string qtext = @"[^" + WTEEmailAddress.Escape + 
				WTEEmailAddress.NonAscii + 
				WTEEmailAddress.CRList + "\"]";
			string dtext = @"[^" + WTEEmailAddress.Escape + 
				WTEEmailAddress.NonAscii + 
				WTEEmailAddress.CRList + 
				WTEEmailAddress.OpenBr + 
				WTEEmailAddress.CloseBr + "\"]";

			string quoted_pair = " " + WTEEmailAddress.Escape + " [^" + WTEEmailAddress.NonAscii + "] ";

			// Impossible to do properly with a regex, I make do by allowing at most 
			// one level of nesting.
			string ctext = @" [^" + WTEEmailAddress.Escape + 
				WTEEmailAddress.NonAscii + 
				WTEEmailAddress.CRList + "()] ";
			
			// Nested quoted Pairs
			string Cnested = "";
			Cnested += WTEEmailAddress.OpenParen;
			Cnested += ctext + "*";
			Cnested += "(?:" + quoted_pair + " " + ctext + "*)*";
			Cnested += WTEEmailAddress.CloseParen;

			// A Comment Usually 
			string comment = "";
			comment += WTEEmailAddress.OpenParen;
			comment += ctext + "*";
			comment += "(?:";
			comment += "(?: " + quoted_pair + " | " + Cnested + ")";
			comment += ctext + "*";
			comment += ")*";
			comment += WTEEmailAddress.CloseParen;
			
			// *********************************************
			// X is optional whitespace/comments
			string X = "";
			X += "[" + WTEEmailAddress.Space + WTEEmailAddress.Tab + "]*";
			X += "(?: " + comment + " [" + WTEEmailAddress.Space + WTEEmailAddress.Tab + "]* )*";
			
			// an email address atom... it's not nuclear ;)
			string atom_char = @"[^(" + WTEEmailAddress.Space + ")<>\\@,;:\\\"." + WTEEmailAddress.Escape + WTEEmailAddress.OpenBr + 
				WTEEmailAddress.CloseBr +
				WTEEmailAddress.Ctrl +
				WTEEmailAddress.NonAscii + "]";
			string atom = "";
			atom += atom_char + "+";
			atom += "(?!" + atom_char + ")";

			// doublequoted string, unrolled.
			string quoted_str = "(?'quotedstr'";
			quoted_str += "\\\"";
			quoted_str += qtext + " *";
			quoted_str += "(?: " + quoted_pair + qtext + " * )*";
			quoted_str += "\\\")";

			// A word is an atom or quoted string
			string word = "";
			word += "(?:";
			word += atom;
			word += "|";
			word += quoted_str;
			word += ")";
			
			// A domain-ref is just an atom
			string domain_ref = atom;

			// A domain-literal is like a quoted string, but [...] instead of "..."
			string domain_lit = "";
			domain_lit += WTEEmailAddress.OpenBr;
			domain_lit += "(?: " + dtext + " | " + quoted_pair + " )*";
			domain_lit += WTEEmailAddress.CloseBr;

			// A sub-domain is a domain-ref or a domain-literal
			string  sub_domain = "";
			sub_domain += "(?:";
			sub_domain += domain_ref;
			sub_domain += "|";
			sub_domain += domain_lit;
			sub_domain += ")";
			sub_domain += X;

			// a domain is a list of subdomains separated by dots
			string domain = "(?'domain'";
			domain += sub_domain;
			domain += "(:?";
			domain += WTEEmailAddress.Period + " " + X + " " + sub_domain;
			domain += ")*)";

			// a a route. A bunch of "@ domain" separated by commas, followed by a colon.
			string route = "";
			route += "\\@ " + X + " " + domain;
			route += "(?: , " + X + " \\@ " + X + " " + domain + ")*";
			route += ":";
			route += X;
				
			// a local-part is a bunch of 'word' separated by periods
			string local_part = "(?'localpart'";
			local_part += word + " " + X;
			local_part += "(?:";
			local_part += WTEEmailAddress.Period + " " + X + " " + word + " " + X;
			local_part += ")*)";

			// an addr-spec is local@domain
			string addr_spec = local_part + " \\@ " + X + " " + domain;
            
			// a route-addr is <route? addr-spec>
			string route_addr = "";
			route_addr += "< " + X;
			route_addr += "(?: " + route + " )?";
			route_addr += addr_spec;
			route_addr += ">";

			// a phrase........
			string phrase_ctrl = @"\000-\010\012-\037";

			// Like atom-char, but without listing space, and uses phrase_ctrl.
			// Since the class is negated, this matches the same as atom-char plus space and tab
			
			string phrase_char = "[^()<>\\@,;:\\\"." + WTEEmailAddress.Escape + 
				WTEEmailAddress.OpenBr +
				WTEEmailAddress.CloseBr + 
				WTEEmailAddress.NonAscii + 
				phrase_ctrl + "]";
			
			// We've worked it so that word, comment, and quoted_str to not consume trailing X
			// because we take care of it manually
			string phrase = "";
			phrase += word;
			phrase += phrase_char;
			phrase += "(?:";
			phrase += "(?: " + comment + " | " + quoted_str + " )";
			phrase += phrase_char + " *";
			phrase += ")*";
			
			// A mailbox is an addr_spec or a phrase/route_addr
			string mailbox = "";
			mailbox += X;
			mailbox += "(?'mailbox'";
			mailbox += addr_spec;
			mailbox += "|";
			mailbox += phrase + " " + route_addr;
			mailbox += ")";
			
			// okay, now setup the object... We'll compile it since this is a rather large (euphemistically 
			// speaking) regex... We also need to IgnorePatternWhitespace unless it's escaped.

			WTEEmailAddress.oRegex = new Regex(mailbox,RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
		}
	}

	//============================================================================================================
	public class WTESimpleMAPIWrapper : IDisposable
	{
		//--------------------------------------------------------------------------------------------------------
		[DllImport("MAPI32.DLL", CharSet = CharSet.Ansi)]
		private static extern int MAPIInitialize(IntPtr ptrMapiInit);
		[DllImport("MAPI32.DLL", CharSet = CharSet.Ansi)]
		private static extern void MAPIUninitialize();
		[DllImport("MAPI32.DLL", CharSet = CharSet.Ansi)]
		private static extern int MAPILogon(IntPtr hwnd, string prf, string pw, int flag, int reserved, out IntPtr sess );
		[DllImport("MAPI32.DLL")]
		private static extern int MAPILogoff(IntPtr sess, IntPtr hwnd, int flag, int rsv );
		[DllImport("MAPI32.DLL")]
		private static extern int MAPISendMail(	IntPtr sess, IntPtr hwnd, WTEMapiMessage message, int flag, int rsv);
		[DllImport("MAPI32.DLL", CharSet = CharSet.Ansi)]
		private static extern int MAPIFindNext(IntPtr sess, IntPtr hwnd, string typ, string seed, int flag, int rsv, StringBuilder id );
		[DllImport("MAPI32.DLL", CharSet = CharSet.Ansi)]
		private static extern int MAPIReadMail( IntPtr sess, IntPtr hwnd, string id, int flag, int rsv, ref IntPtr ptrmsg );
		[DllImport("MAPI32.DLL")]
		private static extern int MAPIFreeBuffer( IntPtr ptr );
		[DllImport("MAPI32.DLL", CharSet = CharSet.Ansi)]
		private static extern int MAPIDeleteMail( IntPtr sess, IntPtr hwnd, string id, int flag, int rsv );
		[DllImport("MAPI32.DLL", CharSet = CharSet.Ansi)]
		private static extern int MAPIAddress(IntPtr sess, IntPtr hwnd, string caption, int editfld, string labels, int recipcount, IntPtr ptrRecips, int flag, int rsv, ref int newrec, ref IntPtr ptrnew);
		[DllImport("MAPI32.DLL", CharSet = CharSet.Ansi)]
		private static extern int MAPIResolveName(IntPtr sess, IntPtr hwnd, string name, int flag, int rsv, ref IntPtr ptrrecip);
		[DllImport("MAPI32.DLL", CharSet = CharSet.Ansi)]
		private static extern void HrGetOneProp(IntPtr pmp, uint ulPropTag, out IntPtr ppProp);

		private const int MAPI_MULTITHREAD_NOTIFICATIONS	= 0x00000001;
		private const int MAPI_NO_COINIT					= 0x00000008;

		private const int MAPI_LOGON_UI				= 0x00000001;	/* Display logon UI					*/
		private const int MAPI_NEW_SESSION			= 0x00000002;	/* Don't use shared session			*/
		private const int MAPI_ALLOW_OTHERS			= 0x00000008;	/* Make this a shared session		*/
		private const int MAPI_EXPLICIT_PROFILE		= 0x00000010;	/* Don't use default profile		*/
		private const int MAPI_EXTENDED				= 0x00000020;	/* Extended MAPI Logon				*/
		private const int MAPI_FORCE_DOWNLOAD		= 0x00001000;	/* Get new mail before return		*/
		private const int MAPI_SERVICE_UI_ALWAYS	= 0x00002000;	/* Do logon UI in all providers		*/
		private const int MAPI_NO_MAIL				= 0x00008000;	/* Do not activate transports		*/
		private const int MAPI_PASSWORD_UI			= 0x00020000;	/* Display password UI only			*/
		private const int MAPI_TIMEOUT_SHORT		= 0x00100000;	/* Minimal wait for logon resources	*/
		private const int MAPI_DIALOG				= 0x00000008;
		private const int MAPI_AB_NOMODIFY			= 0x00000400;
		private const int MAPI_PEEK					= 0x00000080;

		private const int MaxAttachmentsNumber = 100;

		public const int MapiORIG	= 0;
		public const int MapiTO		= 1;
		public const int MapiCC		= 2;
		public const int MapiBCC	= 3;
		
		private IntPtr session	= IntPtr.Zero;
		private IntPtr hwnd		= IntPtr.Zero;

		private WTEMapiRecipDesc	origin	= new WTEMapiRecipDesc();
		private ArrayList		recipients	= new ArrayList();
		private ArrayList		attachments = new ArrayList();

		//--------------------------------------------------------------------------------------------------------
		public WTESimpleMAPIWrapper()
		{
			int memorySize = Marshal.SizeOf(typeof(WTEMapiInit_0));
			IntPtr pointerToWTEMapiInit_0 = Marshal.AllocCoTaskMem(memorySize);
			WTEMapiInit_0 mapiInit = new WTEMapiInit_0();
			mapiInit.version = 0;
			mapiInit.flags = MAPI_NO_COINIT | MAPI_MULTITHREAD_NOTIFICATIONS;
			Marshal.StructureToPtr(mapiInit, pointerToWTEMapiInit_0, false);
			
			int mapiRc = MAPIInitialize(pointerToWTEMapiInit_0);

			Marshal.FreeCoTaskMem(pointerToWTEMapiInit_0);

			if (mapiRc != 0)
				throw new ApplicationException("Error initializing MAPI");
		}

		//--------------------------------------------------------------------------------------------------------
		private void Uninitialize()
		{
			MAPIUninitialize();
		}
		
		//--------------------------------------------------------------------------------------------------------
		public bool Logon(IntPtr aHwnd, bool logonUIIfNecessary)
		{
			hwnd = aHwnd;

			string defaultProfile = String.Empty;
			RegistryKey profilesKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion\\Windows Messaging Subsystem\\Profiles");
			if (profilesKey != null)
				defaultProfile = (string)profilesKey.GetValue("DefaultProfile");

			int mapiRc = 0;

			mapiRc = MAPILogon(aHwnd, defaultProfile, null, MAPI_NEW_SESSION , 0, out session);
			
			if (logonUIIfNecessary)
			{
				if (mapiRc == 0)
					Logoff();
				
				mapiRc = MAPILogon(aHwnd, defaultProfile, null, MAPI_LOGON_UI, 0, out session);
			}
			return (mapiRc == 0);
		}

		//--------------------------------------------------------------------------------------------------------
		public void Reset()
		{
			origin = new WTEMapiRecipDesc();

			foreach(WTEMapiRecipDesc recipientDesc in recipients)
				Marshal.FreeCoTaskMem(recipientDesc.entryID);
			recipients.Clear();
			
			attachments.Clear();
		}

		//--------------------------------------------------------------------------------------------------------
		public void Logoff()
		{ 
			if (session != IntPtr.Zero)
			{
				MAPILogoff(session, hwnd, 0, 0);
				session = IntPtr.Zero;
			}
		}

		//--------------------------------------------------------------------------------------------------------
		public bool IsLoggedOn	{ get { return (session != IntPtr.Zero); } }
			

		//--------------------------------------------------------------------------------------------------------
		public bool Send(string aSubject, string aText)
		{
			if (recipients == null || recipients.Count == 0)
				return true;

			WTEMapiMessage mailMessageTosend = new WTEMapiMessage();

			mailMessageTosend.reserved = 0;
			mailMessageTosend.subject	= aSubject;
			mailMessageTosend.noteText	= aText;

			mailMessageTosend.originator = AllocOrigin();
			mailMessageTosend.recips	 = AllocRecipients(out mailMessageTosend.recipCount);
			mailMessageTosend.files		 = AllocAttachments(out mailMessageTosend.fileCount);

			int error = MAPISendMail(session, hwnd, mailMessageTosend, 0, 0);
			Dealloc(mailMessageTosend);
			Reset();
			return error == 0;
		}

		//--------------------------------------------------------------------------------------------------------
		public void AddRecipient(string aName)
		{
			WTEMapiRecipDesc resolvedRecipient;
			if (ResolveName(aName, out resolvedRecipient))
			{
				resolvedRecipient.recipClass = MapiTO;
				recipients.Add(resolvedRecipient);
			}
		}

		//--------------------------------------------------------------------------------------------------------
		public void SetSender(string aName, string aAddress)
		{
			origin.name		= aName;
			origin.address	= aAddress;
		}

		//--------------------------------------------------------------------------------------------------------
		public void Attach(string filepath)
		{
			attachments.Add(filepath);
		}

		//--------------------------------------------------------------------------------------------------------
		private IntPtr AllocOrigin()
		{
			origin.recipClass = MapiORIG;

			int memorySize = Marshal.SizeOf(typeof(WTEMapiRecipDesc));
			IntPtr pointerToOrigin = Marshal.AllocCoTaskMem(memorySize);
			Marshal.StructureToPtr(origin, pointerToOrigin, false);
			return pointerToOrigin;
		}

		//--------------------------------------------------------------------------------------------------------
		private IntPtr AllocRecipients(out int recipientsCount)
		{
			recipientsCount = 0;
			if (recipients == null || recipients.Count == 0 )
				return IntPtr.Zero;

			IntPtr pointerToRecipients = Marshal.AllocCoTaskMem(recipients.Count * Marshal.SizeOf(typeof(WTEMapiRecipDesc)));

			int tmpPointer = (int) pointerToRecipients;
			foreach(WTEMapiRecipDesc recipientDesc in recipients)
			{
				Marshal.StructureToPtr(recipientDesc, (IntPtr)tmpPointer, false);
				tmpPointer += Marshal.SizeOf(recipientDesc);
			}

			recipientsCount = recipients.Count;
			return pointerToRecipients;
		}
		
		//--------------------------------------------------------------------------------------------------------
		private IntPtr AllocAttachments(out int fileCount)
		{
			fileCount = 0;
			if (attachments == null || attachments.Count <= 0 || attachments.Count > MaxAttachmentsNumber)
				return IntPtr.Zero;

			int memorySize = Marshal.SizeOf(typeof(WTEMapiFileDesc));
			IntPtr pointerToAttachment = Marshal.AllocCoTaskMem(attachments.Count * memorySize);

			WTEMapiFileDesc WTEMapiFileDescription = new WTEMapiFileDesc();
			WTEMapiFileDescription.position = -1;
			int tmpPointer = (int) pointerToAttachment;
			foreach (string path in attachments)
			{
				WTEMapiFileDescription.name = Path.GetFileName(path);
				WTEMapiFileDescription.path = path;
				Marshal.StructureToPtr(WTEMapiFileDescription, (IntPtr) tmpPointer, false);
				tmpPointer += memorySize;
			}
			fileCount = attachments.Count;
			return pointerToAttachment;
		}

		//--------------------------------------------------------------------------------------------------------
		private void Dealloc(WTEMapiMessage mailMessage)
		{
			if (mailMessage.originator != IntPtr.Zero)
			{
				Marshal.DestroyStructure(mailMessage.originator, typeof(WTEMapiRecipDesc));
				Marshal.FreeCoTaskMem(mailMessage.originator);
			}

			if (mailMessage.recips != IntPtr.Zero)
			{
				int memorySize = Marshal.SizeOf(typeof(WTEMapiRecipDesc));
				int tmpPointer = (int)mailMessage.recips;
				for( int i = 0; i < mailMessage.recipCount; i++ )
				{
					Marshal.DestroyStructure((IntPtr)tmpPointer, typeof(WTEMapiRecipDesc));
					tmpPointer += memorySize;
				}
				Marshal.FreeCoTaskMem(mailMessage.recips);
			}

			if (mailMessage.files != IntPtr.Zero)
			{
				int memorySize = Marshal.SizeOf(typeof(WTEMapiFileDesc));
				int tmpPointer = (int)mailMessage.files;
				for( int i = 0; i < mailMessage.fileCount; i++ )
				{
					Marshal.DestroyStructure((IntPtr)tmpPointer, typeof(WTEMapiFileDesc));
					tmpPointer += memorySize;
				}
				Marshal.FreeCoTaskMem(mailMessage.files);
			}
		}


		//--------------------------------------------------------------------------------------------------------
		public bool Delete( string id )
		{
			return MAPIDeleteMail(session, hwnd, id, 0, 0) == 0;
		}

		//--------------------------------------------------------------------------------------------------------
		public bool SaveAttachment(string id, string name, string savepath)
		{
			IntPtr pointerToMessage = IntPtr.Zero;
			if 
				(
				(MAPIReadMail(session, hwnd, id, MAPI_PEEK, 0, ref pointerToMessage) != 0) || 
				(pointerToMessage == IntPtr.Zero)
				)
				return false;

			WTEMapiMessage mailMessage = new WTEMapiMessage();
			Marshal.PtrToStructure(pointerToMessage, mailMessage);
			bool returnCode = false;
			if ((mailMessage.fileCount > 0) && (mailMessage.fileCount < 100) && (mailMessage.files != IntPtr.Zero))
				returnCode = SaveAttachmentByName(mailMessage, name, savepath);
			MAPIFreeBuffer(pointerToMessage);
			return returnCode;
		}

		//--------------------------------------------------------------------------------------------------------
		private void GetAttachmentNames(WTEMapiMessage mailMessage, out WTEMailAttach[] attachmentsArray)
		{
			attachmentsArray = new WTEMailAttach[mailMessage.fileCount];
			WTEMapiFileDesc WTEMapiFileDescription = new WTEMapiFileDesc();
			int memorySize = Marshal.SizeOf(typeof(WTEMapiFileDesc));
			int tmpPointer = (int) mailMessage.files;
			for (int i = 0; i < mailMessage.fileCount; i++)
			{
				Marshal.PtrToStructure((IntPtr)tmpPointer, WTEMapiFileDescription);
				tmpPointer += memorySize;
				attachmentsArray[i] = new WTEMailAttach();
				if( WTEMapiFileDescription.flags == 0 )
				{
					attachmentsArray[i].position = WTEMapiFileDescription.position;
					attachmentsArray[i].name	 = WTEMapiFileDescription.name;
					attachmentsArray[i].path	 = WTEMapiFileDescription.path;
				}
			}
		}

		//--------------------------------------------------------------------------------------------------------
		private bool SaveAttachmentByName(WTEMapiMessage mailMessage, string name, string savepath )
		{
			WTEMapiFileDesc WTEMapiFileDescription = new WTEMapiFileDesc();
			int memorySize = Marshal.SizeOf(typeof(WTEMapiFileDesc));
			int tmpPointer = (int) mailMessage.files;

			for( int i = 0; i < mailMessage.fileCount; i++ )
			{
				Marshal.PtrToStructure((IntPtr)tmpPointer, WTEMapiFileDescription);
				tmpPointer += memorySize;
				if (WTEMapiFileDescription.flags != 0)
					continue;
				if (WTEMapiFileDescription.name == null)
					continue;

				try 
				{
					if (name == WTEMapiFileDescription.name)
					{
						if( File.Exists(savepath))
							File.Delete(savepath);
						File.Move(WTEMapiFileDescription.path, savepath);
					}
					File.Delete(WTEMapiFileDescription.path);
				}
				catch (Exception exception)
				{
					Debug.Fail("Exception raised in WTESimpleMAPIWrapper.SaveAttachmentByName: " + exception.Message);
					return false;
				}
			}
			return true;
		}
		//--------------------------------------------------------------------------------------------------------
		public bool SelectAddresses(string label, WTEMapiRecipDesc[] oldRecipients, out WTEMapiRecipDesc[] newRecipients)
		{
			newRecipients = null;

			int newRecipientsNumber = 0;
			int oldRecipientsNumber = 0;
			IntPtr pointerToOldRecipients = IntPtr.Zero;
			IntPtr pointerToNewRecipients = IntPtr.Zero;

			try
			{
				int memorySize = Marshal.SizeOf(typeof(WTEMapiRecipDesc));
			
				if (oldRecipients != null && oldRecipients.Length > 0)
				{
					oldRecipientsNumber = oldRecipients.Length;
					pointerToOldRecipients = Marshal.AllocCoTaskMem(oldRecipientsNumber * memorySize);
					int tmpPointerToOldRecipients = (int) pointerToOldRecipients;
					foreach(WTEMapiRecipDesc oldRecipientDesc in oldRecipients)
					{
						if (oldRecipientDesc == null)
							continue;
						Marshal.StructureToPtr(oldRecipientDesc, (IntPtr)tmpPointerToOldRecipients, false);
						tmpPointerToOldRecipients += memorySize;
					}
				}

				int error = MAPIAddress(session, hwnd, null, 1, label, oldRecipientsNumber, pointerToOldRecipients, 0, 0, ref newRecipientsNumber, ref pointerToNewRecipients);
				if 
					(
					(error == 0) &&
					(newRecipientsNumber > 0) &&
					(pointerToNewRecipients != IntPtr.Zero) 
					)
				{		
					newRecipients = new WTEMapiRecipDesc[newRecipientsNumber];

					int tmpPointerToNewRecipients = (int)pointerToNewRecipients;
					memorySize = Marshal.SizeOf(typeof(WTEMapiRecipDesc));
					for (int i = 0; i < newRecipientsNumber; i++)
					{
						newRecipients[i] = new WTEMapiRecipDesc();
						Marshal.PtrToStructure((IntPtr)tmpPointerToNewRecipients, newRecipients[i]);
						tmpPointerToNewRecipients += memorySize;
					}
				}
			}
			catch(Exception)
			{
			}
			finally
			{
				if (pointerToOldRecipients != IntPtr.Zero)
					Marshal.FreeCoTaskMem(pointerToOldRecipients);

				if (pointerToNewRecipients != IntPtr.Zero)
					MAPIFreeBuffer(pointerToNewRecipients);
			}
            return (newRecipientsNumber > 0 && newRecipients != null && newRecipients.Length == newRecipientsNumber);
		}
		
		//--------------------------------------------------------------------------------------------------------
		public bool ResolveName(string name, out WTEMapiRecipDesc resolvedRecipient)
		{
			resolvedRecipient = null;
			IntPtr pointerToRecipient = IntPtr.Zero;
			
			int mapiRc = MAPIResolveName(session, hwnd, name, MAPI_DIALOG | MAPI_AB_NOMODIFY, 0, ref pointerToRecipient);
			if (mapiRc != 0)
				return false;

			resolvedRecipient = new WTEMapiRecipDesc();

			Marshal.PtrToStructure((IntPtr)pointerToRecipient, resolvedRecipient);
			
			// Tengo comunque il nome iniziale... quello su database
			resolvedRecipient.name = name;
			resolvedRecipient.recipClass = MapiTO;

			if (resolvedRecipient.eIDSize > 0)
			{
				byte[] byteArray = new byte[resolvedRecipient.eIDSize];
				Marshal.Copy(resolvedRecipient.entryID, byteArray, 0, resolvedRecipient.eIDSize);
				resolvedRecipient.entryID = Marshal.AllocCoTaskMem(resolvedRecipient.eIDSize);
				Marshal.Copy(byteArray, 0, resolvedRecipient.entryID, resolvedRecipient.eIDSize);
			}
			MAPIFreeBuffer(pointerToRecipient);

			return true;
		}
	
		//--------------------------------------------------------------------------------------------------------
		public bool GetAddressByDisplayName(string name, out string address)
		{
			address = null;
			WTEMapiRecipDesc resolvedRecipient;
			if (!ResolveName(name, out resolvedRecipient))
				return false;

			address = resolvedRecipient.address;

			// Devo trasformare un indirizzo EX in SMTP...
			// altrimenti non funziona l'invio via SMTP

			// 1. Cast AddressEntry.MAPIOBJECT to MAPIdefs.IAddrEntry
			// 2. Use HrGetOneProp to retrieve PR_EMS_AB_PROXY_ADDRESSES property
			// 3. Read TSPropValue.Value.MVszA 
			// 4. Call MAPIFreeBuffer to free teh memory (PSPropValue) returned by HrGetOneProp.
			// GetMAPIProperty(AddressEntry.MAPIOBJECT, PR_EMS_AB_PROXY_ADDRESSES);

			Marshal.FreeCoTaskMem(resolvedRecipient.entryID);

			return true;
		}

		//--------------------------------------------------------------------------------------------------------
		public void Dispose()
		{
			Uninitialize();
		}

	}
	// ********************************************* MAPI STRUCTURES *********************************************

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class WTEMapiInit_0
	{
		public int version = 0;
		public int flags = 0;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class WTEMapiMessage
	{
		public int		reserved;
		public string	subject;
		public string	noteText;
		public string	messageType;
		public string	dateReceived;
		public string	conversationID;
		public int		flags;
		public IntPtr	originator;		// WTEMapiRecipDesc* [1]
		public int		recipCount;
		public IntPtr	recips;			// WTEMapiRecipDesc* [n]
		public int		fileCount;
		public IntPtr	files;			// WTEMapiFileDesc*  [n]
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class WTEMapiRecipDesc
	{
		public int		reserved;
		public int		recipClass;
		public string	name;
		public string	address;
		public int		eIDSize;
		public IntPtr	entryID;
	}

	[StructLayout( LayoutKind.Sequential, CharSet = CharSet.Ansi )]
	public class WTEMapiFileDesc
	{
		public int		reserved;
		public int		flags;
		public int		position;
		public string	path;
		public string	name;
		public IntPtr	type;
	}


	// ********************************************* HELPER STRUCTURES *********************************************

	public class MailEnvelop
	{
		public string	id;
		public DateTime	date;
		public string	from;
		public string	subject;
		public bool		unread;
		public int		atts;
	}

	public class WTEMailAttach
	{
		public int		position;
		public string	path;
		public string	name;
	}
}
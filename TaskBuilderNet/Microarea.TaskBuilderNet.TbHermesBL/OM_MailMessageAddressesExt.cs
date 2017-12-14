using System;
using System.Collections.Generic;
using Limilabs.Mail.Headers;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
	public partial class OM_MailMessageAddresses
	{
		//-------------------------------------------------------------------------------
		public Microarea.TaskBuilderNet.TbHermesBL.OM_MailMessages.MailAddressType EnumMailAddressType
		{
			get { return (Microarea.TaskBuilderNet.TbHermesBL.OM_MailMessages.MailAddressType)Enum.Parse(typeof(Microarea.TaskBuilderNet.TbHermesBL.OM_MailMessages.MailAddressType), this.MailAddressType.ToString(), true); }
            set { this.MailAddressType = (Int32)value; }
		}

		//-------------------------------------------------------------------------------
		public static List<OM_MailMessageAddresses> FilterByType(List<OM_MailMessageAddresses> addresses, Microarea.TaskBuilderNet.TbHermesBL.OM_MailMessages.MailAddressType addrType)
		{
			int addrTypeInt = (int) addrType;
            return addresses.FindAll(x => x.MailAddressType == addrTypeInt);
		}

		//-------------------------------------------------------------------------------
		public static List<MailBox> ToMailboxes(List<OM_MailMessageAddresses> addresses)
		{
			List<MailBox> list = new List<MailBox>();
			//string mask = "{0} <{1}>";
			foreach (OM_MailMessageAddresses addr in addresses)
			{
				// TODO trova un modo più furbo per crearle! così sto formattando per poi parsare, bleahh! :-P (puke)
				//string addrTxt = string.Format(CultureInfo.InvariantCulture, mask, addr.MailAddress, addr.MailLongName);
				//Limilabs.Mail.Headers.MailAddress mailAddr = new MailAddressParser().ParseOne(addrTxt);
				MailBox mailAddr = new MailBox(addr.MailAddress, addr.MailLongName);
				list.Add(mailAddr);
			}
			return list;
		}
	}
}

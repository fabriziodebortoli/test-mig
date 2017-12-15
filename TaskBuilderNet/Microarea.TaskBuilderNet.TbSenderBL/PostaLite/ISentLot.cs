﻿
namespace Microarea.TaskBuilderNet.TbSenderBL.PostaLite
{
	public interface ISentLot
	{
		int IdLot { get; set; }
		decimal Total { get; set; }
		decimal AmountPrint { get; set; }
		decimal AmountPostage { get; set; }
		//int IdZone { get; set; }
		string DescriptionZone { get; set; }
		bool Incongruous { get; set; }
		int Error { get; set; }
		string[] Details { get; set; }
	}
}
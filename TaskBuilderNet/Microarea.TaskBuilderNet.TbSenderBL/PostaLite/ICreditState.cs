using System;
namespace Microarea.TaskBuilderNet.TbSenderBL.PostaLite
{
	public interface ICreditState
	{
		int CodeState { get; set; }
		decimal Credit { get; set; }
		int Error { get; set; }
		DateTime ExpiryDate { get; set; }
	}
}

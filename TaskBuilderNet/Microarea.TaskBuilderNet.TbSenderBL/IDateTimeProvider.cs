using System;
namespace Microarea.TaskBuilderNet.TbSenderBL
{
	public interface IDateTimeProvider
	{
		DateTime Now { get; }
	}
}

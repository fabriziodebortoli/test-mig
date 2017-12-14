using System.Collections;

namespace Microarea.TaskBuilderNet.Interfaces.Model
{
	[System.ComponentModel.TypeConverter(typeof(System.ComponentModel.TypeConverter))]
	public interface IItemsSourceConsumer
	{
		IList ItemsSource { get; set; }
		bool IsItemsSourceEditable { get; }
		void RefreshContentByDataType();
		
	}
}

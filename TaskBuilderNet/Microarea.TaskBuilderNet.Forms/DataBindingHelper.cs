using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.ComponentModel;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.TaskBuilderNet.Forms
{
	//=========================================================================
	public static class DataBindingHelper
	{
		//---------------------------------------------------------------------------
		public static TBBindingList<TBBindingListItem> CreateEnumBindingList(IDataObj dataEnum)
		{
			if (!dataEnum.DataType.IsEnum)
				return null;

			TBBindingList<TBBindingListItem> list = new TBBindingList<TBBindingListItem>();
			EnumTag enumTag = BasePathFinder.BasePathFinderInstance.Enums.Tags.GetTag((ushort)dataEnum.DataType.Tag);
			if (enumTag == null || enumTag.EnumItems == null || enumTag.EnumItems.Count == 0)
				return list;

			foreach (EnumItem enumItem in enumTag.EnumItems)
				list.Add(new TBBindingListItem(enumItem.Stored, enumItem.LocalizedName));

			return list;
		}

		//---------------------------------------------------------------------------
		public static TBBindingList<TBBindingListItem> CreateHotlinkBindingList(IMHotLink hotlink)
		{
			TBBindingList<TBBindingListItem> list = new TBBindingList<TBBindingListItem>();
			if (hotlink == null)
				return list;

			foreach (TBBindingListItem item in hotlink.SearchComboQueryData(-1))
				list.Add(item);

			return list;
		}
	}
}

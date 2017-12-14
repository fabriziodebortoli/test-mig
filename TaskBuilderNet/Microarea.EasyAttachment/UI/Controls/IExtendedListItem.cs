using System;

namespace Microarea.EasyAttachment.UI.Controls
{
    /// <summary>
    /// This interface, when implemented by a list item class, allows that 
    /// class to handle certain changes to the item or the items position
    /// in the list (like displaying different information when the item
    /// is selected or changing background color depending row).
    /// Note that it is not required by an list item in a ExtendedListView to
    /// implement this interface.
    /// </summary>
	
	
    public interface IExtendedListItem
    {
		event EventHandler ListItemResizing;
	
        void SelectedChanged(bool isSelected);
        void PositionChanged(int index);  
    }
}

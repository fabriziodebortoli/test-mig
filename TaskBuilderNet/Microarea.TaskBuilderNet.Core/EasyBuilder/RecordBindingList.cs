// -----------------------------------------------------------------------
// <copyright file="RecordCollection.cs" company="Microarea S.p.A.">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{
	using System.ComponentModel;
	using Microarea.TaskBuilderNet.Interfaces.Model;

	//=========================================================================
	/// <summary>
	/// TaskBuilder.Net SqlRecord collection supporting standard .Net data binding
	/// </summary>
	/// <typeparam name="T">MSqlRecord</typeparam>
	public class RecordBindingList<T> : BindingList<T>, ITypedList
		where T: IRecord
	{
		PropertyDescriptorCollection propertyDescriptors;
		string listName;

		//---------------------------------------------------------------------
		/// <summary>
		/// Returns the property descriptors collection
		/// </summary>
		public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
		{
			if (propertyDescriptors == null)
			{
				propertyDescriptors = TypeDescriptor.GetProperties(typeof(T));
			}
			return propertyDescriptors;
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Returns the list name
		/// </summary>
		public string GetListName(PropertyDescriptor[] listAccessors)
		{
			if (listName == null)
			{
				listName = typeof(T).Name;
			}
			return listName;
		}
	}
}

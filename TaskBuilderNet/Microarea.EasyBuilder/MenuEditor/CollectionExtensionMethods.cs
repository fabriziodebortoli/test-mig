using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microarea.EasyBuilder.MenuEditor
{
	//=========================================================================
	internal static class CollectionExtensionMethods
	{
		//---------------------------------------------------------------------
		public static int CompareTo(
			this IList<MenuApplication> thisColl,
			IList<MenuApplication> other,
			bool ignoreCase,
			CultureInfo culture
			)
		{
			if (other == null)
				return 1;

			if (thisColl.Count != other.Count)
				return (thisColl.Count > other.Count) ? 1 : -1;

			for (int itemIdx = 0; itemIdx < thisColl.Count; itemIdx++)
			{
				int result = thisColl[itemIdx].CompareTo(other[itemIdx], ignoreCase, culture);
				if (result != 0)
					return result;
			}

			return 0;
		}

		//---------------------------------------------------------------------
		public static int CompareTo(
			this IList<MenuGroup> thisColl,
			IList<MenuGroup> other,
			bool ignoreCase,
			CultureInfo culture
			)
		{
			if (other == null)
				return 1;

			if (thisColl.Count != other.Count)
				return (thisColl.Count > other.Count) ? 1 : -1;

			for (int itemIdx = 0; itemIdx < thisColl.Count; itemIdx++)
			{
				int result = thisColl[itemIdx].CompareTo(other[itemIdx], ignoreCase, culture);
				if (result != 0)
					return result;
			}

			return 0;
		}

		//---------------------------------------------------------------------
		public static MenuBranch GetCommandMenuBranch(this List<MenuBranch> thisColl, MenuCommand aMenuCommand)
		{
			if (thisColl.Count == 0)
				return null;

			for (int itemIdx = 0; itemIdx < thisColl.Count; itemIdx++)
			{
				if (thisColl[itemIdx].Commands != null && thisColl[itemIdx].Commands.Contains(aMenuCommand))
					return thisColl[itemIdx];

				if (thisColl[itemIdx].Menus != null)
				{
					MenuBranch commandMenuBranch = thisColl[itemIdx].Menus.GetCommandMenuBranch(aMenuCommand);
					if (commandMenuBranch != null)
						return commandMenuBranch;
				}
			}

			return null;
		}

		//---------------------------------------------------------------------
		public static string GetMenuBranchPathHierarchy(this List<MenuBranch> thisColl,	MenuBranch aMenuBranch)
		{
			if (thisColl.Contains(aMenuBranch))
				return aMenuBranch.GetNodePathHierarchyElement();

			if (thisColl.Count > 0)
			{
				for (int itemIdx = 0; itemIdx < thisColl.Count; itemIdx++)
				{
					string menuBranchHierarchy = thisColl[itemIdx].Menus.GetMenuBranchPathHierarchy(aMenuBranch);
					if (menuBranchHierarchy != null && menuBranchHierarchy.Length > 0)
						return String.Concat(thisColl[itemIdx].GetNodePathHierarchyElement(), "/", menuBranchHierarchy);
				}
			}
			return String.Empty;
		}

		//---------------------------------------------------------------------
		public static string GetCommandPathHierarchy(this List<MenuBranch> thisColl, MenuCommand aMenuCommand)
		{
			if (thisColl.Count == 0)
				return null;

			for (int itemIdx = 0; itemIdx < thisColl.Count; itemIdx++)
			{
				if (thisColl[itemIdx].Commands != null && thisColl[itemIdx].Commands.Contains(aMenuCommand))
					return String.Concat(thisColl[itemIdx].GetNodePathHierarchyElement(), "/", aMenuCommand.GetNodePathHierarchyElement());

				if (thisColl[itemIdx].Menus != null)
				{
					string commandHierarchy = thisColl[itemIdx].Menus.GetCommandPathHierarchy(aMenuCommand);
					if (commandHierarchy != null && commandHierarchy.Length > 0)
						return String.Concat(thisColl[itemIdx].GetNodePathHierarchyElement(), "/", commandHierarchy);
				}
			}

			return null;
		}

		//---------------------------------------------------------------------
		public static bool Contains(this List<MenuBranch> thisColl, MenuBranch aMenuBranch, bool searchSubBranches)
		{
			if (thisColl.Contains(aMenuBranch))
				return true;

			if (thisColl.Count > 0 && searchSubBranches)
			{
				for (int itemIdx = 0; itemIdx < thisColl.Count; itemIdx++)
				{
					if (thisColl[itemIdx].Menus != null && thisColl[itemIdx].Menus.Contains(aMenuBranch, true))
						return true;
				}
			}
			return false;
		}

		//---------------------------------------------------------------------
		public static int CompareTo(this IList<MenuBranch> thisColl, IList<MenuBranch> other)
		{
			return CompareTo(thisColl, other, true, CultureInfo.InvariantCulture);
		}

		//---------------------------------------------------------------------
		public static int CompareTo(
			this IList<MenuBranch> thisColl,
			IList<MenuBranch> other,
			bool ignoreCase,
			CultureInfo culture
			)
		{
			if (other == null)
				return 1;

			if (thisColl.Count != other.Count)
				return (thisColl.Count > other.Count) ? 1 : -1;

			for (int itemIdx = 0; itemIdx < thisColl.Count; itemIdx++)
			{
				int result = thisColl[itemIdx].CompareTo(other[itemIdx], ignoreCase, culture);
				if (result != 0)
					return result;
			}

			return 0;
		}

		//---------------------------------------------------------------------
		public static int CompareTo(this IList<MenuCommand> thisColl, IList<MenuCommand> other)
		{
			if (other == null)
				return 1;

			if (thisColl.Count != other.Count)
				return (thisColl.Count > other.Count) ? 1 : -1;

			for (int itemIdx = 0; itemIdx < thisColl.Count; itemIdx++)
			{
				int result = thisColl[itemIdx].CompareTo(other[itemIdx], false, CultureInfo.InvariantCulture);
				if (result != 0)
					return result;
			}

			return 0;
		}
	}
}
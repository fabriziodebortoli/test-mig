using System;
using System.Diagnostics;

namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{

    /// <summary>
    /// Used from Security for get assembly resources without use namespace as string
    /// </summary>
    public class Dummy
    {

    }
    
    /// <summary>
	/// Summary description for CommandImageInfo.
	/// </summary>
	//============================================================================
	internal class CommandImageInfo
	{
		private string	fileName;
		private int		listIndex;

		public CommandImageInfo(string aFileName, int aListIndex)
		{
			fileName		= aFileName;
			listIndex		= aListIndex;
		}

		public string	FileName	{ get { return fileName; } }
		public int		ListIndex	{ get { return listIndex; } set { listIndex = value; }}
	}

	/// <summary>
	/// Summary description for CommandsImageInfos.
	/// </summary>
	//============================================================================
	internal class CommandsImageInfos : System.Collections.ArrayList
	{
		public CommandsImageInfos()
		{
		}

		//---------------------------------------------------------------------------
		public new CommandImageInfo this[int index] 
		{
			get
			{
				if (!(base[index] is CommandImageInfo))
				{
					Debug.Fail("CommandsImageInfos.[] Error: invalid element.");
					return null;
				}
				return (CommandImageInfo)(base[index]);
			}
			set
			{
				base[index] = value;
			}
		}
		
		//---------------------------------------------------------------------------
		override public int Add(object aCommandImageInfo)
		{
			if (!(aCommandImageInfo is CommandImageInfo))
			{
				Debug.Fail("CommandsImageInfos.Add Error: invalid element.");
				return -1;
			}
			return base.Add(aCommandImageInfo);
		}

		//---------------------------------------------------------------------------
		public int AddCommandImageInfo(string aFileName, int aListIndex)
		{
			if 
				(
				aFileName == null ||
				aFileName.Length == 0
				)
				return -1;
			
			int objBmpInfoIdx = FindImage(aFileName);
			if (objBmpInfoIdx >= 0)
			{
				this[objBmpInfoIdx].ListIndex = aListIndex;
				return objBmpInfoIdx;
			}
				
			return Add(new CommandImageInfo(aFileName, aListIndex));
		}
		
		//---------------------------------------------------------------------------
		public int FindImage(string aFileName)
		{
			if (this.Count <= 0)
				return -1;
			
			for (int i = 0; i < this.Count; i++)
			{
				if 
					(
					this[i] != null && 
					String.Compare(((CommandImageInfo)this[i]).FileName, aFileName) == 0 
					)
				{
					return i;
				}
			}
			return -1;
		}
		
	}
}

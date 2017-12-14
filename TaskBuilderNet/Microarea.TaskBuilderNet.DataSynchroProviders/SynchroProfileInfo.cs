using System.Collections.Generic;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.DataSynchroProviders
{
	//================================================================================
	internal class SynchroProfileInfo
	{
		public List<BaseDocumentToSync> Documents = new List<BaseDocumentToSync>();

		//--------------------------------------------------------------------------------
		public SynchroProfileInfo()
		{
		}
	}

    //================================================================================
    public class BaseDocumentToSync : IDocumentToSync
    {
        //--------------------------------------------------------------------------------
        public string Name { get; private set; }
		//--------------------------------------------------------------------------------
		public string AddOnAppName { get; private set; }

        public string ActionsAttribute { get; protected set; }

        //--------------------------------------------------------------------------------
        public BaseDocumentToSync(string name)
        {
            this.Name = name;
        }
		
		//--------------------------------------------------------------------------------
		public BaseDocumentToSync(string nameSpace, string addOnAppName) 
			: this (nameSpace)
		{
			this.AddOnAppName = addOnAppName;
		}
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microarea.TaskBuilderNet.Interfaces
{
    public interface IDocumentToSync
    {
        string Name { get; }
		
		string AddOnAppName { get; }

        string ActionsAttribute { get; }
    }
}

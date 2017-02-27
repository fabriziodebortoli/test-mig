using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskBuilderNetCore.Interfaces
{
    public interface IDocumentToSync
    {
        string Name { get; }
		
		string AddOnAppName { get; }

        string ActionsAttribute { get; }
    }
}

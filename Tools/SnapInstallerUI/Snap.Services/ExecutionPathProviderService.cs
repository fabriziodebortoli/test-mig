using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.Services
{
    internal class ExecutionPathProviderService : IPathProviderService
    {
        readonly System.Reflection.Assembly executionAsm;
        public ExecutionPathProviderService()
        {
            this.executionAsm = System.Reflection.Assembly.GetExecutingAssembly();
        }
        public string RetrievePathToCalculateOn()
        {
            return new Uri(this.executionAsm.CodeBase).LocalPath;
        }
    }
}

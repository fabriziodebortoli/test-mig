
using TaskBuilderNetCore.Documents.Diagnostic;

namespace TaskBuilderNetCore.Documents.Controllers
{
    //====================================================================================    
    public class OrchestratorMessages
    {
        public static readonly Message DocumentNotFound = new Message(1, OrchestratorMessagesStrings.DocumentNotFound);
        public static readonly Message IsNotActivityDocument = new Message(2, OrchestratorMessagesStrings.IsNotActivityDocument);
    }
}

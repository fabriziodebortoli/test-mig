using TaskBuilderNetCore.Documents.Diagnostic;

namespace TaskBuilderNetCore.Documents.Controllers
{

    //====================================================================================    
    public class StateSerializerMessages
    {
        public static readonly Message ErrorLoadingState = new Message(1, StateSerializerMessagesStrings.ErrorLoadingState);
    }
}

namespace Microarea.RSWeb.Models
{
    public class RSEngine
    {
        public string nameSpace { get; set; }

        public RSEngine(string nameSpace)
        {
            this.nameSpace = nameSpace;
        }

        public Message GetResponseFor(Message msg)
        {
            Message nMsg = new Message();
            nMsg.commandType = msg.commandType;
            nMsg.message = msg.message;
            nMsg.response = "This Is Response for " + msg.message;
            return nMsg;
        }
    }
}

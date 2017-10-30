namespace Microarea.AdminServer.Services.PostMan
{
	//================================================================================
	public interface IPostManActuator
	{
		OperationResult Send(string to, string subject, string body);
	}

	//================================================================================
	public class PostMan
    {
		IPostManActuator postManActuator;

		public PostMan(IPostManActuator iPostManActuator)
		{
			this.postManActuator = iPostManActuator;
		}

		public OperationResult Send(string to, string subject, string body)
		{
			return this.postManActuator.Send(to, subject, body);
		}
	}
}

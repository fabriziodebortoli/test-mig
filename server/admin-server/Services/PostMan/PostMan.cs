namespace Microarea.AdminServer.Services.PostMan
{
	//================================================================================
	public interface IPostManActuator
	{
		OperationResult Send();
	}

	//================================================================================
	public class PostMan
    {
		IPostManActuator postManActuator;

		public PostMan(IPostManActuator iPostManActuator)
		{
			this.postManActuator = iPostManActuator;
		}

		public OperationResult Send()
		{
			return this.postManActuator.Send();
		}
	}
}

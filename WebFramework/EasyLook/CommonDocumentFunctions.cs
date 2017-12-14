using System.Web.SessionState;

using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;

namespace Microarea.Web.EasyLook
{
	public class CommonDocumentFunctions
	{

		///<summary>
		/// Metodo statico per ottenere la TbLoaderClientInterface dalla ReportSession 
		///</summary>
		//--------------------------------------------------------------------------------------
		public static TbLoaderClientInterface GetTbInterface()
		{
			UserInfo ui = UserInfo.FromSession();
			if (ui != null)
				return ui.GetTbLoaderInterface() as TbLoaderClientInterface;
			return null;
		}

		///<summary>
		/// Metodo statico per sapere se ci sono dei tbloader attivi sul server
		///</summary>
		//--------------------------------------------------------------------------------------
		public static bool IsTbLoaderLoaded()
		{
			UserInfo ui = UserInfo.FromSession();
			if (ui == null || ui.TbServices == null)
				return false;

			return ui.IsTbLoaderInstantiated(ui.ApplicationKey);
		}
	}
}

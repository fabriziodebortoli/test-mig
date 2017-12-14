//
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SecurityAdmin
{
	public partial class ViewSecurityPlugInInfo : System.Windows.Forms.Form
	{

		public ViewSecurityPlugInInfo(BrandLoader aBrandLoader)
		{
			InitializeComponent();

			if (aBrandLoader != null)
			{
				string brandedCompany = aBrandLoader.GetCompanyName();

				if (brandedCompany != null && brandedCompany.Length > 0)
				{
					lblDescription.Text = lblDescription.Text.Replace(NameSolverStrings.Microarea, brandedCompany);
					lblDescription2.Text = lblDescription2.Text.Replace(NameSolverStrings.Microarea, brandedCompany);
				}
			}
		}
	}
}
using System.Windows.Forms;

namespace HttpNamespaceManager.UI
{
	public partial class NamespaceMngForm : Form
	{
		public NamespaceMngForm ()
		{
			InitializeComponent();
		}
		public NamespaceMngForm (NamespaceManagerAction action, string url)
		{
			InitializeComponent();
			this.nsControl = new NamespaceMngControl(action, url);

		}
	}
}

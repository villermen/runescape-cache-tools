using System.Windows.Forms;

namespace RunescapeCacheToolsGUI
{
	public partial class ExtractJobUserControl : UserControl
	{
		public ExtractJobUserControl()
		{
			InitializeComponent();
		}

		private void ExtractJobUserControl_Load(object sender, System.EventArgs e)
		{
			Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
		}
	}
}

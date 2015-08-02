using System;
using System.Windows.Forms;

namespace RunescapeCacheToolsGUI
{
	static class Program
	{
		[STAThread]
		static void Main()
		{
			//show a message for all unhandled exceptions
			AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
				MessageBox.Show(
					$"The application encountered an unhandled {((Exception)args.ExceptionObject).GetType().FullName}. Stability is not guaranteed.");

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}

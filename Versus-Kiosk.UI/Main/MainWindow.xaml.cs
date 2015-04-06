using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace VersusKiosk.UI.Main
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

#if DEBUG
			ShowOnMonitor(0, this);
#endif
		}

		private void ShowOnMonitor(int monitor, Window window)
		{
			var screen = ScreenHandler.GetScreen(monitor);
			var currentScreen = ScreenHandler.GetCurrentScreen(this);
			window.WindowState = WindowState.Normal;
			window.Left = screen.WorkingArea.Left;
			window.Top = screen.WorkingArea.Top;
			window.Width = screen.WorkingArea.Width;
			window.Height = screen.WorkingArea.Height;
			this.Loaded += (s, e) => (s as Window).WindowState = WindowState.Maximized;
		}
		
	}

	public static class ScreenHandler
	{
		public static Screen GetCurrentScreen(Window window)
		{
			var parentArea = new System.Drawing.Rectangle((int)window.Left, (int)window.Top, (int)window.Width, (int)window.Height);
			return Screen.FromRectangle(parentArea);
		}

		public static Screen GetScreen(int requestedScreen)
		{
			var screens = Screen.AllScreens;
			var mainScreen = 0;
			if (screens.Length > 1 && mainScreen < screens.Length)
			{
				return screens[requestedScreen];
			}
			return screens[0];
		}
	}

}

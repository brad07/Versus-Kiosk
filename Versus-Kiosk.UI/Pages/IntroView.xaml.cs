using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VersusKiosk.UI.Pages
{
	/// <summary>
	/// Interaction logic for IntroView.xaml
	/// </summary>
	public partial class IntroView : UserControl
	{
		public IntroView()
		{
			InitializeComponent();
		}

		private void mediaPlayer_Initialized(object sender, EventArgs e)
		{
			// TODO: ARGH!!! MVVM pattern is broken, move this to the view model! - MJF
			var media = sender as MediaElement;
			media.Play();
		}
		
		private void mediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
		{
			// TODO: ARGH!!! MVVM pattern is broken, move this to the view model! - MJF
			var media = sender as MediaElement;
			media.Position = TimeSpan.Zero;
			media.LoadedBehavior = MediaState.Play;
		}

		
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
using System.Windows.Threading;

namespace VersusKiosk.UI.Pages
{
	/// <summary>
	/// Interaction logic for WeighInView.xaml
	/// </summary>
	public partial class WeighInView : UserControl
	{
		private readonly IEnumerable<BitmapImage> Images;
		private IEnumerator<BitmapImage> ImageEnumerator;

		public WeighInView()
		{
			InitializeComponent();

			string exe = Assembly.GetExecutingAssembly().Location;
			string exeDir = System.IO.Path.GetDirectoryName(exe);
			string imgDir = System.IO.Path.Combine(exeDir, "Media/WeighAnim");
			this.Images =
				from file in Directory.GetFiles(imgDir, "*.png")
				orderby FrameNum(file)
				let uri = new Uri(file, UriKind.Absolute)
				select new BitmapImage(uri);
		}

		private static int FrameNum(string filename)
		{
			filename = filename.ToLower();

			var prefix = "weight-icon-";
			filename = filename.Substring(filename.IndexOf(prefix) + prefix.Length);

			var postfix = ".png";
			filename = filename.Substring(0, filename.IndexOf(postfix));

			return Convert.ToInt32(filename);
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			foreach (var weightBox in FindVisualChildren<TextBox>(this))
			{
				if (weightBox.Name == "WeightBox")
					weightBox.Focus();
			}

			var timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromMilliseconds(50);
			timer.Tick += timer_Tick;
			timer.Start();
		}

		void timer_Tick(object sender, EventArgs e)
		{
			var weighImage = FindVisualChildren<Image>(this).FirstOrDefault();
			if (weighImage == null)
				return;

			if ((this.ImageEnumerator == null) || !this.ImageEnumerator.MoveNext())
			{
				this.ImageEnumerator = this.Images.GetEnumerator();
				this.ImageEnumerator.MoveNext();
			}

			weighImage.Source = this.ImageEnumerator.Current;
		}

		public IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
		{
			if (depObj != null)
			{
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
				{
					DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
					if (child != null && child is T)
					{
						yield return (T)child;
					}

					foreach (T childOfChild in FindVisualChildren<T>(child))
					{
						yield return childOfChild;
					}
				}
			}
		}

	}
}

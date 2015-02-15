using GalaSoft.MvvmLight.Command;
using Ninject;
using Ninject.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using VersusKiosk.Domain;
using VersusKiosk.UI.Main;

namespace VersusKiosk.UI.Pages
{
	public class IntroViewModel : PageViewModel
	{
		// number of seconds the user has to hold the admin button to bring up the log-in page
		private const int RequiredAdminHoldTime = 1;

		public override void Initialize()
		{
			base.Initialize();
		}

		protected override void OnTick(TimeSpan elapsed)
		{
			// override this method so we don't keep trying to restart ourselves
		}

		public ICommand ScanCommand { get { return new RelayCommand(OnScan); } }
		private void OnScan()
		{
			if (this.Parent.NumAvailableArcades > 0)
			{
				var session = this.Injector.Get<Session>();
				this.Parent.SetPage(this.Injector.Get<ChooseWorkoutViewModel>(new ConstructorArgument("session", session)));
			}
		}

		private DispatcherTimer AdminButtonTimer = null;

		private void StartAdminTimer()
		{
			StopAdminTimer();
			this.AdminButtonTimer = new DispatcherTimer();
			this.AdminButtonTimer.Interval = TimeSpan.FromSeconds(RequiredAdminHoldTime);
			this.AdminButtonTimer.Tick += (s, e) =>
			{
				this.AdminButtonTimer.Stop();
				this.Parent.SetPage(this.Injector.Get<AdminLoginViewModel>());
			};
			this.AdminButtonTimer.Start();
		}

		private void StopAdminTimer()
		{
			if (this.AdminButtonTimer != null)
			{
				this.AdminButtonTimer.Stop();
				this.AdminButtonTimer = null;
			}
		}

		public ICommand AdminButtonPressCommand { get { return new RelayCommand<MouseButtonEventArgs>(OnAdminButtonPress); } }
		private void OnAdminButtonPress(MouseButtonEventArgs args)
		{
			args.Handled = true;
			StartAdminTimer();
		}

		public ICommand PreviewMouseUpCommand { get { return new RelayCommand(OnPreviewMouseUp); } }
		private void OnPreviewMouseUp()
		{
			StopAdminTimer();
		}

	}
}

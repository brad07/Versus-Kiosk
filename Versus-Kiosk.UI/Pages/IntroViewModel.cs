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

		public ICommand SelectBodyAreaCommand { get { return new RelayCommand<BodyArea>(OnSelectBodyArea); } }
		private void OnSelectBodyArea(BodyArea area)
		{
			if (this.Parent.NumAvailableArcades > 0)
			{
				var session = this.Injector.Get<Session>(new PropertyValue("BodyArea", area));
				if ((area == BodyArea.WorkoutOfTheWeek) && (this.Parent.NumAvailableArcades == 1))
				{
					session.SetNumPlayers(1);
					this.Parent.SetPage(this.Injector.Get<EnterEmailViewModel>(new ConstructorArgument("session", session), new ConstructorArgument("playerNum", 0)));
				}
				else
				{
					this.Parent.SetPage(this.Injector.Get<ChooseWorkoutViewModel>(new ConstructorArgument("session", session)));
				}
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
				if (String.IsNullOrEmpty(Properties.Settings.Default.AdminPassword))
					this.Parent.SetPage(this.Injector.Get<AdminViewModel>());
				else
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

		public ICommand AdminButton_PreviewMouseDownCommand { get { return new RelayCommand<MouseButtonEventArgs>(OnAdminButton_PreviewMouseDown); } }
		private void OnAdminButton_PreviewMouseDown(MouseButtonEventArgs args)
		{
			args.Handled = true;
			StartAdminTimer();
			//this.Parent.SetPage(this.Injector.Get<AdminLoginViewModel>());
		}

		public ICommand AdminButton_PreviewMouseUpCommand { get { return new RelayCommand(OnAdminButton_PreviewMouseUp); } }
		private void OnAdminButton_PreviewMouseUp()
		{
			StopAdminTimer();
		}

		public ICommand ResetButton_PreviewMouseDownCommand { get { return new RelayCommand<MouseButtonEventArgs>(OnResetButton_PreviewMouseDown); } }
		private void OnResetButton_PreviewMouseDown(MouseButtonEventArgs args)
		{
			args.Handled = true;
			this.Parent.ResetAllStations();
		}

		public ICommand ResetButton_PreviewMouseUpCommand { get { return new RelayCommand(OnResetButton_PreviewMouseUpCommand); } }
		private void OnResetButton_PreviewMouseUpCommand()
		{
		}

		public ICommand SkipWarmUpsButton_PreviewMouseDownCommand { get { return new RelayCommand<MouseButtonEventArgs>(OnSkipWarmUpsButton_PreviewMouseDown); } }
		private void OnSkipWarmUpsButton_PreviewMouseDown(MouseButtonEventArgs args)
		{
			args.Handled = true;
			this.Parent.SkipAllWarmUps();
		}

	}
}

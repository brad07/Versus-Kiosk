using GalaSoft.MvvmLight.Command;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;
using VersusKiosk.UI.Main;
using VersusKiosk.UI.ViewModels;
using VersusKiosk.UI.Helpers;
using Newtonsoft.Json;
using Ninject.Parameters;
using MvvmDialogs.ViewModels;
using System.Windows;
using VersusKiosk.UI.SetUp;

namespace VersusKiosk.UI.Pages
{
	public class AdminViewModel : PageViewModel
	{
		private IList<StationViewModel> _Stations;
		public IList<StationViewModel> Stations
		{
			get { return this._Stations; }
			set { this._Stations = value; RaisePropertyChanged(() => this.Stations); }
		}

		public AdminViewModel()
		{
		}

		public override void Initialize()
		{
			base.Initialize();
		}

		protected override void OnActivating()
		{
			base.OnActivating();
			this.Parent.RequestArcadeStations();
		}

		protected override void OnDeactivating()
		{
			base.OnDisconnected();
		}

		public override void OnConnected()
		{
			base.OnConnected();
			this.Parent.RequestArcadeStations();
		}

		public override void OnDisconnected()
		{
			base.OnDisconnected();
		}

		protected override void OnTick(TimeSpan elapsed)
		{
		}

		public ICommand LogOutCommand { get { return new RelayCommand(OnLogOut); } }
		private void OnLogOut()
		{
			var result = new MessageBoxViewModel
			{
				Caption = "Quit?",
				Message = "Exit Versus Kiosk?",
				Image = MessageBoxImage.Question,
				Buttons = MessageBoxButton.YesNo
			}.Show(this.Parent.Dialogs);

			if (result == MessageBoxResult.Yes)
				this.Parent.ShutDown();
		}

		public ICommand SettingsCommand { get { return new RelayCommand(OnSettings); } }
		private void OnSettings()
		{
			var result = Injector.Get<InitialSetupViewModel>().Show();
			if (result)
			{
				new MessageBoxViewModel
				{
					Message = "Settings applied, please restart Kiosk.",
					Image = MessageBoxImage.Information,
					Buttons = MessageBoxButton.OK
				}.Show(this.Parent.Dialogs);
			}
		}

		public ICommand OkCommand { get { return new RelayCommand(OnOk); } }
		private void OnOk()
		{
			this.Parent.SetPage(this.Injector.Get<IntroViewModel>());
		}

		public ICommand PowerOnCommand { get { return new RelayCommand(OnPowerOn); } }
		private void OnPowerOn()
		{
			var result = new MessageBoxViewModel
			{
				Caption = "Power On?",
				Message = "Power on all versus stations?",
				Image = MessageBoxImage.Question,
				Buttons = MessageBoxButton.YesNo
			}.Show(this.Parent.Dialogs);

			if (result == MessageBoxResult.Yes)
				this.Parent.SendStationCommand("power_on");
		}

		public ICommand PowerOffCommand { get { return new RelayCommand(OnPowerOff); } }
		private void OnPowerOff()
		{
			var result = new MessageBoxViewModel
			{
				Caption = "Power Off?",
				Message = "Power off all versus stations?",
				Image = MessageBoxImage.Question,
				Buttons = MessageBoxButton.YesNo
			}.Show(this.Parent.Dialogs);

			if (result == MessageBoxResult.Yes)
				this.Parent.SendStationCommand("power_off");
		}

		public ICommand StopAllCommand { get { return new RelayCommand(OnStopAll); } }
		private void OnStopAll()
		{
			var result = new MessageBoxViewModel
			{
				Caption = "Stop All?",
				Message = "Stop all workouts on versus stations?",
				Image = MessageBoxImage.Question,
				Buttons = MessageBoxButton.YesNo
			}.Show(this.Parent.Dialogs);

			if (result == MessageBoxResult.Yes)
				this.Parent.SendStationCommand("stop_all");
		}

		public ICommand RebootAllCommand { get { return new RelayCommand(OnRebootAll); } }
		private void OnRebootAll()
		{
			var result = new MessageBoxViewModel
			{
				Caption = "Reboot All?",
				Message = "Stop all workouts and reboot all\nmachines including kiosk?",
				Image = MessageBoxImage.Question,
				Buttons = MessageBoxButton.YesNo
			}.Show(this.Parent.Dialogs);

			if (result == MessageBoxResult.Yes)
			{
				this.Parent.RebootAll();
			}
		}

		public override void ProcessNetworkMessage(dynamic msg)
		{
			if (msg.cmd == "arcade_stations")
				this.Stations = JsonConvert.DeserializeObject<List<StationViewModel>>(msg.stations.ToString());
		}

		public ICommand StationCommand { get { return new RelayCommand<dynamic>(OnStation); } }
		public void OnStation(dynamic station)
		{
			StationActionViewModel dlg = this.Injector.Get<StationActionViewModel>(new ConstructorArgument("station", station));
			this.Parent.Dialogs.Add(dlg);
		}


	}
}

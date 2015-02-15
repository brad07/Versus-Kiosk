using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MvvmDialogs.ViewModels;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VersusKiosk.UI.Main;
using VersusKiosk.UI.ViewModels;

namespace VersusKiosk.UI.Pages
{
	public class StationActionViewModel : ViewModelBase, IUserDialogViewModel
	{
		public virtual bool IsModal { get { return true; } }
		public virtual void RequestClose() { this.DialogClosing(this, null); }
		public virtual event EventHandler DialogClosing;

		[Inject]
		public MainViewModel Parent { get; set; }

		private StationViewModel _Station;
		public StationViewModel Station
		{
			get { return _Station; }
			set { _Station = value; RaisePropertyChanged(() => this.Station); }
		}

		private string _Title;
		public string Title
		{
			get { return _Title; }
			set { _Title = value; RaisePropertyChanged(() => this.Title); }
		}

		public StationActionViewModel(dynamic station)
		{
			this.Station = station;
			this.Title = "Actions for " + this.Station;
		}

		public ICommand RestartCommand { get { return new RelayCommand(OnRestart); } }
		private void OnRestart()
		{
			this.Parent.SendStationCommand("restart_station", this.Station.station_no);
			RequestClose();
		}

		public ICommand RebootCommand { get { return new RelayCommand(OnReboot); } }
		private void OnReboot()
		{
			this.Parent.SendStationCommand("reboot_station", this.Station.station_no);
			RequestClose();
		}

		public ICommand EnableCommand { get { return new RelayCommand(OnEnable); } }
		private void OnEnable()
		{
			this.Parent.SendStationCommand("enable_station", this.Station.station_no);
			RequestClose();
		}

		public ICommand DisableCommand { get { return new RelayCommand(OnDisable); } }
		private void OnDisable()
		{
			this.Parent.SendStationCommand("disable_station", this.Station.station_no);
			RequestClose();
		}

		public ICommand StopSessionCommand { get { return new RelayCommand(OnStopSession); } }
		private void OnStopSession()
		{
			this.Parent.SendStationCommand("stop_station_session", this.Station.station_no);
			RequestClose();
		}

		public ICommand CloseCommand { get { return new RelayCommand(() => Close()); } }
		private void Close()
		{
			RequestClose();
		}
	}
}

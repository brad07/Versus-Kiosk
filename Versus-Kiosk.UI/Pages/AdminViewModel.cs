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

		public ICommand OkCommand { get { return new RelayCommand(OnOk); } }
		private void OnOk()
		{
			this.Parent.SetPage(this.Injector.Get<IntroViewModel>());
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

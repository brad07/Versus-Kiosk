using GalaSoft.MvvmLight.Command;
using Ninject;
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
	public class StartViewModel : PageViewModel
	{
		public Session Session { get; private set; }
		public int NumPlayers { get { return this.Session.Players.Count(); } }

		[Inject]
		public MainViewModel MainViewModel { get; set; }

		private bool _RequestingSessionStart = true;
		public bool RequestingSessionStart
		{
			get { return this._RequestingSessionStart; }
			set { this._RequestingSessionStart = value; RaisePropertyChanged(() => RequestingSessionStart); }
		}

		private int _SecondsRemaining;
		public int SecondsRemaining
		{
			get { return this._SecondsRemaining; }
			set { this._SecondsRemaining = value; RaisePropertyChanged(() => SecondsRemaining); }
		}

		public StationInfo[] Stations { get; private set; }
		
		public StartViewModel(Session session)
		{
			this.Session = session;
		}

		public override void Initialize()
		{			
 			base.Initialize();
		}

		protected override void OnActivating()
		{
			this.Stations = new StationInfo[this.Session.Players.Count()];
			for (int i=0; i<this.Session.Players.Count(); i++)
				this.Stations[i] = new StationInfo { Player = this.Session.Players[i], StationName = ""};
			
			// request session start from CC in a seperate thread
			this.MainViewModel.RequestSessionStart(this.Session);
		}

		protected override void OnTick(TimeSpan elapsed)
		{
			if (!this.RequestingSessionStart)
			{
				var remaining = this.StartTime.AddSeconds(this.Session.Workout.StartTime) - DateTime.Now;
				this.SecondsRemaining = (int)Math.Max(0, Math.Min(this.Session.Workout.StartTime, remaining.TotalSeconds));
				var elapsed_since_start = DateTime.Now - this.StartTime;
				if (elapsed_since_start.TotalSeconds >= 15)
					this.Parent.SetPage(this.Injector.Get<IntroViewModel>());
			}
		}

		public override void ProcessNetworkMessage(dynamic msg)
		{
			// server has responded telling us a session is available
			if (msg.cmd == "session_starting")
			{
				this.Session.session_no = msg.session_no;
				for (int i = 0; i < this.Stations.Count(); i++)
					this.Stations[i].StationName = msg.stations[i];
				this.RequestingSessionStart = false;
				this.StartTime = DateTime.Now;
				this.SecondsRemaining = this.Session.Workout.StartTime;
			}
			else if (msg.cmd == "session_cancelled")
				this.Parent.SetPage(this.Injector.Get<IntroViewModel>()); // todo: notify the user - MJF
		}

		

		public ICommand OkCommand { get { return new RelayCommand(OnOk); } }
		private void OnOk()
		{
			this.Parent.SetPage(this.Injector.Get<IntroViewModel>());
		}

		public ICommand SkipWarmUpCommand { get { return new RelayCommand(OnSkipWarmup); } }
		private void OnSkipWarmup()
		{
			this.Parent.SkipWarmUp(this.Session);
			this.Parent.SetPage(this.Injector.Get<IntroViewModel>());
		}
		
	}

	public class StationInfo
	{
		public Player Player { get; set; }
		public string StationName { get; set; }

		public string DisplayName
		{
			get
			{
				var str = this.Player.FirstName;
				if (!String.IsNullOrEmpty(this.Player.LastName.Trim()))
					str += " " + this.Player.LastName[0] + ".";
				return str;
			}

		}
	}
}

﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MvvmDialogs.ViewModels;
using Ninject;
using Ninject.Parameters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using VersusKiosk.Domain;
using VersusKiosk.Domain.IoC;
using VersusKiosk.Domain.Network;
using VersusKiosk.UI.Hardware;
using VersusKiosk.UI.Pages;

namespace VersusKiosk.UI.Main
{
	public class MainViewModel : ViewModelBase, IInitializable, IDisposable
	{
		[Inject]
		public Injector Injector { get; set; }

		[Inject]
		public Comms Comms { get; set; }

		[Inject]
		public Scales Scales { get; set; }

		private ObservableCollection<IDialogViewModel> _Dialogs = new ObservableCollection<IDialogViewModel>();
		public ObservableCollection<IDialogViewModel> Dialogs { get { return _Dialogs; } }

		private ObservableCollection<PageViewModel> _Pages = new ObservableCollection<PageViewModel>();
		public ObservableCollection<PageViewModel> Pages
		{
			get { return _Pages; }
			set { _Pages = value; RaisePropertyChanged(() => this.Pages); }
		}
		
		/*
		private PageViewModel _CurrentPage = null;
		public PageViewModel CurrentPage
		{
			get { return _CurrentPage; }
			set { _CurrentPage = value; RaisePropertyChanged(() => this.CurrentPage); }
		}
		 * */

		private int _NumAvailableArcades = 0;
		public int NumAvailableArcades
		{
			get { return _NumAvailableArcades; }
			set { _NumAvailableArcades = value; RaisePropertyChanged(() => this.NumAvailableArcades); }
		}

		private DateTime _NextArcadeAvailable = DateTime.Now;
		public DateTime NextArcadeAvailable
		{
			get { return _NextArcadeAvailable; }
			set
			{
				_NextArcadeAvailable = value;
				RaisePropertyChanged(() => this.NextArcadeAvailable);
				if (!this.IsInDesignMode)
					UpdateNextArcadeAvailable();
			}
		}

		private string _NextArcadeAvailableString = "00:00";
		public string NextArcadeAvailableString
		{
			get { return _NextArcadeAvailableString; }
			set
			{
				_NextArcadeAvailableString = value;
				RaisePropertyChanged(() => this.NextArcadeAvailableString);
			}
		}

		private bool _Connected = false;
		public bool Connected
		{
			get { return _Connected; }
			set { _Connected = value; RaisePropertyChanged(() => this.Connected); }
		}

		private IPAddress control_center_ip;
		private DateTime last_connect_attempt = DateTime.Now.AddDays(-1);
		private int connect_retry_delay = 1;
		private DateTime last_autodiscovery_request = DateTime.Now.AddDays(-1);

		private DispatcherTimer UpdateTimer;
		
		public void Initialize()
		{
			if (this.IsInDesignMode)
				return;

			LoadUserSettings();

			this.Comms.BroadcastListener += ProcessNetworkBroadcast;
			this.Comms.MessageListener += ProcessNetworkMessage;
			var networkName = Environment.GetEnvironmentVariable("VERSUS_NETWORK", EnvironmentVariableTarget.User) ?? Properties.Settings.Default.NetworkName;
			this.Comms.startUDPListener(networkName); // todo: replace this
			this.Comms.startTCPSender();

			SetPage(this.Injector.Get<IntroViewModel>());

			// testing/development
			//var session = this.Injector.Get<Session>();
			//session.SetNumPlayers(1);
			//SetPage(this.Injector.Get<AdminLoginViewModel>());
			//SetPage(this.Injector.Get<AdminViewModel>());
			//SetPage(this.Injector.Get<ChooseWorkoutViewModel>());
			//SetPage(this.Injector.Get<EnterEmailViewModel>(new ConstructorArgument("session", session), new ConstructorArgument("playerNum", 0)));
			//SetPage(this.Injector.Get<PlayerDetailsViewModel>());
			//SetPage(this.Injector.Get<NicknameViewModel>(new ConstructorArgument("session", session), new ConstructorArgument("playerNum", 0)));
			//SetPage(this.Injector.Get<WeighInViewModel>(new ConstructorArgument("session", session), new ConstructorArgument("playerNum", 0)));

			requestAutoDiscovery();

			this.UpdateTimer = new DispatcherTimer();
			this.UpdateTimer.Interval = TimeSpan.FromSeconds(1);
			this.UpdateTimer.Tick += UpdateTimer_Tick;
			this.UpdateTimer.Start();
		}

		void UpdateTimer_Tick(object sender, EventArgs e)
		{
			if (control_center_ip == null)
				requestAutoDiscovery();
			UpdateNextArcadeAvailable();
		}

		public void Dispose()
		{
			SaveUserSettings();
		}

		#region Networking

		void ProcessNetworkBroadcast(dynamic msg)
		{
			var cmd = (string)msg.cmd;
			switch (cmd)
			{
				case "auto_discover":
					Console.WriteLine("*** RECEIVED auto_discover***");
					if (!this.Connected /* && (msg.network_name == config.network_name.Value)*/)
					{
						// disable remote logging
						if (control_center_ip == null)
						{
							control_center_ip = IPAddress.Parse(msg.ip_address.ToString());
						}

						//log("Setting Control Center IP = " + control_center_ip);
						connect();
					}
					break;
			}
		}

		private void ProcessNetworkMessage(dynamic msg)
		{
			Application.Current.Dispatcher.Invoke(new Action(() => {

				var cmd = msg.cmd.ToString();

				if (cmd == "available_arcades")
				{
					this.NumAvailableArcades = msg.num_available;
					this.NextArcadeAvailable = DateTime.Now.AddSeconds((int)msg.next_available);
				}

				// don't know what this message is so pass it to the currently active page
				else
					//this.CurrentPage.ProcessNetworkMessage(msg);
					this.Pages.Last().ProcessNetworkMessage(msg);
			}));
		}

		private void connect()
		{
			try
			{
				if ((DateTime.Now - last_connect_attempt).TotalSeconds > connect_retry_delay)
				{
					if (control_center_ip != null)
					{
						//log("Connecting to " + control_center_ip);

						// re-connection now happens automatically, calling this function more than once will cause multiple threads to be started 
						Debug.Assert(!this.Connected);
						if (this.Connected)
							return;

						try
						{
							this.Comms.StationClient.OnConnectedToServer += StationClient_OnConnectedToServer;
							this.Comms.StationClient.OnDisconnectedFromServer += StationClient_OnDisconnectedFromServer;
							this.Comms.startStationClient(control_center_ip);
						}
						catch (Exception)
						{
							//log("Exception starting websocket connection to server: " + ex.Message);
						}
					}
				}
			}
			catch (Exception)
			{
				//log("Exception in connect: " + ex.Message);
			}
		}

		void StationClient_OnDisconnectedFromServer(object context)
		{
			Application.Current.Dispatcher.Invoke(new Action(() =>
			{
				this.Connected = false;
				this.control_center_ip = null;
				SetPage(this.Injector.Get<IntroViewModel>());
			}));
		}

		void StationClient_OnConnectedToServer(object context)
		{
			Application.Current.Dispatcher.Invoke(new Action(() =>
			{
				this.Connected = true;
				dynamic msg = new System.Dynamic.ExpandoObject();
				msg.cmd = "station_id";
				msg.station_no = VersusKiosk.UI.Properties.Settings.Default.Port;
				msg.ip_address = LocalIPAddress().ToString();
				if (control_center_ip != null)
				{
					Console.WriteLine("*** SENDING STATION ID ***");
					this.Comms.sendMsg(msg, control_center_ip, true);
				}
			}));
		}

		private IPAddress LocalIPAddress()
		{
			IPHostEntry host;
			IPAddress localIP = new IPAddress(0);
			host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (IPAddress ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					localIP = ip;
					break;
				}
			}
			return localIP;
		}

		private void requestAutoDiscovery()
		{
			if ((DateTime.Now - last_autodiscovery_request).TotalSeconds >= 5)
			{
				//log("Requesting AutoDiscovery");
				dynamic msg = new System.Dynamic.ExpandoObject();
				msg.cmd = "request_auto_discover";
				msg.station_no = VersusKiosk.UI.Properties.Settings.Default.Port;
				//log("Sending auto_discover msg using UDP");
				Console.WriteLine("*** SENDING request_auto_discover ***");
				this.Comms.sendUDPMsg(msg);

				last_autodiscovery_request = DateTime.Now;
			}
		}

		#endregion Networking

		public ICommand LoadedCommand { get { return new RelayCommand(OnLoaded); } }
		private void OnLoaded()
		{
		}

		public ICommand ClosedCommand { get { return new RelayCommand(OnClosed); } }
		private void OnClosed()
		{
			this.Dispose();
		}

		private void LoadUserSettings()
		{
		}

		private void SaveUserSettings()
		{
		}

		/*
		public void SetPage(PageViewModel page)
		{
			if (this.CurrentPage != null)
				this.CurrentPage.Active = false;
			this.CurrentPage = page;
			page.Active = true;
			}
		 * */

		public void SetPage(PageViewModel page)
		{
			foreach (var p in this.Pages)
				p.Active = false;
			this.Pages.Clear();
			this.Pages.Add(page);
			page.Active = true;
		}

		/*
		public void AddPage(PageViewModel page)
		{
			var oldPage = this.Pages.Last();
			this.Pages.Add(page);
			page.Active = true;
		}
		 * */

		public void SkipWarmUp(Session session)
		{
			dynamic msg = new System.Dynamic.ExpandoObject();
			msg.cmd = "skip_warmup";
			msg.session_no = session.session_no;
			msg.session = session;
			if (control_center_ip != null)
			{
				Console.WriteLine("*** SKIPPING WARM UP ***");
				this.Comms.sendMsg(msg, control_center_ip, true);
			}
		}

		public void ResetAllStations()
		{
			dynamic msg = new System.Dynamic.ExpandoObject();
			msg.cmd = "reset_all_stations";
			if (control_center_ip != null)
			{
				Console.WriteLine("*** RESETTING ALL STATIONS ***");
				this.Comms.sendMsg(msg, control_center_ip, true);
			}
		}

		public void SkipAllWarmUps()
		{
			dynamic msg = new System.Dynamic.ExpandoObject();
			msg.cmd = "skip_all_warmups";
			if (control_center_ip != null)
			{
				Console.WriteLine("*** SKIPPING ALL WARMUPS ***");
				this.Comms.sendMsg(msg, control_center_ip, true);
			}
		}

		#region Sessions

		public void RequestSessionAvailable(int numPlayers)
		{
			dynamic msg = new System.Dynamic.ExpandoObject();
			msg.cmd = "request_arcade_session";
			msg.station_no = VersusKiosk.UI.Properties.Settings.Default.Port;
			msg.ip_address = LocalIPAddress().ToString();
			msg.num_players = numPlayers;
			if (control_center_ip != null)
				this.Comms.sendMsg(msg, control_center_ip, true);
		}

		public void RequestSessionStart(Session session)
		{
			dynamic msg = new System.Dynamic.ExpandoObject();
			msg.cmd = "start_arcade_session";
			msg.station_no = VersusKiosk.UI.Properties.Settings.Default.Port;
			msg.ip_address = LocalIPAddress().ToString();
			msg.session = session;
			if (control_center_ip != null)
			{
				Console.WriteLine("*** SENDING STATION ID ***");
				this.Comms.sendMsg(msg, control_center_ip, true);
			}
		}

		#endregion Sessions

		public void RequestPlayerDetails(Player player)
		{
			dynamic msg = new System.Dynamic.ExpandoObject();
			msg.cmd = "request_player_details";
			msg.email_address = player.Email;
			msg.ip_address = LocalIPAddress().ToString();
			if (control_center_ip != null)
				this.Comms.sendMsg(msg, control_center_ip, true);
		}

		public void RequestArcadeStations()
		{
			dynamic msg = new System.Dynamic.ExpandoObject();
			msg.cmd = "request_arcade_stations";
			msg.ip_address = LocalIPAddress().ToString();
			if (control_center_ip != null)
				this.Comms.sendMsg(msg, control_center_ip, true);
		}

		public void SendStationCommand(string command, int station_no)
		{
			dynamic msg = new System.Dynamic.ExpandoObject();
			msg.cmd = command;
			msg.station_no = station_no;
			msg.ip_address = LocalIPAddress().ToString();
			if (control_center_ip != null)
				this.Comms.sendMsg(msg, control_center_ip, true);
		}

		private void UpdateNextArcadeAvailable()
		{
			int seconds = (int)Math.Round((this.NextArcadeAvailable - DateTime.Now).TotalSeconds);
			seconds = Math.Max(0, seconds);
			int minutes = seconds / 60;
			seconds %= 60;
			this.NextArcadeAvailableString = minutes.ToString() + ":" + seconds.ToString("D2");
		}

	}
}

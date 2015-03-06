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
using VersusKiosk.UI.Hardware;

namespace VersusKiosk.UI.Pages
{
	public class WeighInViewModel : PageViewModel
	{
		private Session Session;
		private int PlayerNum;

		[Inject]
		public Scales Scales { get; set; }

		public int DisplayPlayerNum { get { return this.PlayerNum + 1; } }
		public int NumPlayers { get { return this.Session.Players.Count(); } }
		public Player Player {get; private set;}

		private DispatcherTimer WeighTimer = new DispatcherTimer();
		private double StartWeight = -100;
		private DateTime WeightTime = DateTime.Now;

		private string _WeightError = "";
		public string WeightError
		{
			get { return this._WeightError; }
			private set { this._WeightError = value; RaisePropertyChanged(() => this.WeightError); }
		}

		private bool _ValidWeight = false;
		public bool ValidWeight
		{
			get { return this._ValidWeight; }
			private set { this._ValidWeight = value; RaisePropertyChanged(() => this.ValidWeight); }
		}

		// true when scales are present, false otherwise
		private WeighMode _WeighMode = WeighMode.Unknown;
		public WeighMode WeighMode
		{
			get { return this._WeighMode; }
			private set { this._WeighMode = value; RaisePropertyChanged(() => this.WeighMode); }
		}

		public WeighInViewModel(Session session, int playerNum)
		{
			this.Session = session;
			this.PlayerNum = playerNum;
			this.Player = session.Players[playerNum];			
		}

		public override void Initialize()
		{
			base.Initialize();
		}

		protected override void OnActivating()
		{
 			base.OnActivating();

			this.WeighTimer.Interval = TimeSpan.FromMilliseconds(250);
			this.WeighTimer.Tick += (s, e) => WeighPlayer();
			WeighPlayer();
			this.WeighTimer.Start();
		}

		protected override void OnDeactivating()
		{
			if (this.Scales.Connected)
				this.Scales.Disconnect();
			this.WeighTimer.Stop();
 			base.OnDeactivating();
		}

		private void WeighPlayer()
		{
			if (!this.Scales.Connected)
				this.Scales.Connect();

			if (this.Scales.Connected)
			{
				this.WeighMode = WeighMode.Scales;
				var weight = this.Scales.Read();
				this.Player.Weight = Math.Round(weight).ToString();
				this.ValidWeight = IsValidWeight(weight);

				// if we've drifted by more than a few kilos then restart the weight timer
				if (Math.Abs(weight - this.StartWeight) > 3)
				{
					this.StartTime = DateTime.Now;
					this.StartWeight = weight;
				}

				// otherwise if we've been stable for at least 2 seconds then accept this weight
				else if ((DateTime.Now - this.StartTime).TotalSeconds >= 2)
					OnNext();

				this.WeightError = "";
			}
			else
			{
				this.WeighMode = WeighMode.Manual;
				this.ValidWeight = false;
			}
		}

		public ICommand BackCommand { get { return new RelayCommand(OnBack); } }
		private void OnBack()
		{
			this.Parent.SetPage(this);
		}

		public ICommand NextCommand { get { return new RelayCommand(OnNext); } }
		private void OnNext()
		{
			if (ValidateInput())
			{
				int nextPlayer = this.PlayerNum + 1;
				if (nextPlayer < this.Session.Players.Count())
					this.Parent.SetPage(this.Injector.Get<EnterEmailViewModel>(new ConstructorArgument("session", this.Session), new ConstructorArgument("playerNum", nextPlayer)));
				else
					this.Parent.SetPage(this.Injector.Get<StartViewModel>(new ConstructorArgument("session", this.Session)));
			}
		}

		public bool ValidateInput()
		{
			var result = true;
			this.WeightError = "";
			
			int weight;
			if (!Int32.TryParse(this.Player.Weight.Trim(), out weight) || !IsValidWeight(weight))
			{
				this.WeightError = "Please enter a valid weight";
				result = false;
			}

			return result;
		}

		private static bool IsValidWeight(double weight)
		{
			return (weight >= 40) && (weight <= 200);
		}
		
	}

	public enum WeighMode
	{
		Unknown,
		Manual,
		Scales
	}

}

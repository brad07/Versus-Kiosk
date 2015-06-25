using GalaSoft.MvvmLight.Command;
using Ninject;
using Ninject.Parameters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VersusKiosk.Domain;
using VersusKiosk.Domain.Network;
using VersusKiosk.UI.Main;

namespace VersusKiosk.UI.Pages
{
	public class ChooseWorkoutViewModel : PageViewModel
	{
		public Session Session { get; private set; }

		private WorkoutType[] _Workouts = new WorkoutType[]
		{
			new WorkoutType{Title = "Cardio", Prefix = "Cardio_", Description = "A fast-paced, challenging routine that will get your heart rate going.", MediaFile = @".\Media\high_knees.wmv", StartTime=15},
			new WorkoutType{Title = "Strength", Prefix = "Strength_", Description = "A slower-paced routine that concentrates on building all-round strength.", MediaFile = @".\Media\upright_row.wmv", StartTime=15},
			new WorkoutType{Title = "Mixed (Cardio & Strength)", Prefix = "Mixed_", Description = "An all-round routine that exercises all major muscle groups.", MediaFile = @".\Media\thrusters.wmv", StartTime=15},
		};

		public WorkoutType[] Workouts
		{
			get { return _Workouts; }
			set { _Workouts = value; RaisePropertyChanged(() => this.Workouts); }
		}

		private Dictionary<int, string[]> WorkoutMap {get; set;}

		private int _CurrentWorkoutIndex = 1;
		public int CurrentWorkoutIndex
		{
			get { return this._CurrentWorkoutIndex; }
			set
			{
				this._CurrentWorkoutIndex = value;
				RaisePropertyChanged(() => this.CurrentWorkoutIndex);
				this.CurrentWorkout = this.Workouts[value];
				SetAvailableWorkouts();
			}
		}

		public WorkoutType CurrentWorkout
		{
			get { return this.Session.WorkoutType; }
			set { this.Session.WorkoutType = value; RaisePropertyChanged(() => this.CurrentWorkout); }
		}

		private int _NumPlayers = 1;
		public int NumPlayers
		{
			get { return _NumPlayers; }
			set { _NumPlayers = value; RaisePropertyChanged(() => this.NumPlayers); }
		}

		private int _Duration = 12;
		public int Duration
		{
			get { return this._Duration; }
			set { this._Duration = value; RaisePropertyChanged(() => this.Duration); }
		}

		private int _FirstDuration;
		public int FirstDuration
		{
			get { return this._FirstDuration; }
			set { this._FirstDuration = value; RaisePropertyChanged(() => this.FirstDuration); }
		}

		private int _LastDuration;
		public int LastDuration
		{
			get { return this._LastDuration; }

			set { this._LastDuration = value; RaisePropertyChanged(() => this.LastDuration); }
		}

		private int[] _AvailableDurations;
		public int[] AvailableDurations
		{
			get { return this._AvailableDurations; }
			set
			{
				if (this.AvailableDurations != value)
				{
					this._AvailableDurations = value;
					RaisePropertyChanged(() => this.AvailableDurations);
					this.FirstDuration = this._AvailableDurations.First();
					this.LastDuration = this._AvailableDurations.Last();
					if (!this._AvailableDurations.Contains(this.Duration))
						this.Duration = this._AvailableDurations.Last();
				}
			}
		}
		
		public bool MultiplayerAvailable
		{
			get
			{
				return this.Parent.NumAvailableArcades > 1;
			}
		}

		public ChooseWorkoutViewModel(Session session)
		{
			this.Session = session;			
		}

		public override void Initialize()
		{
			base.Initialize();

			this.CurrentWorkoutIndex = 0;
			SetAvailableWorkouts();
		}

		private void SetAvailableWorkouts()
		{
			var prefix = GetWorkoutPrefix();

			// find all workouts that start with this prefix
			var availableWorkouts = this.Parent.Workouts.Where(w => w.name.StartsWith(prefix)).ToArray();

			// group by durations
			var groups = availableWorkouts
				.Select(w =>
				{
					var fields = w.name.Split('_');
					int duration = Int32.Parse(fields[2]);
					return new { duration, w.name };
				})
				.GroupBy(w => w.duration);

			// create the WorkoutMap (list of available workouts for any given duration)
			this.WorkoutMap = new Dictionary<int, string[]>();
			foreach (var g in groups)
				this.WorkoutMap[g.Key] = g.Select(x => x.name).ToArray();

			// get the available durations
			this.AvailableDurations = groups.Select(g => g.Key).OrderBy(duration => duration).ToArray();

			if (!this.AvailableDurations.Contains(this.Duration))
				this.Duration = this.AvailableDurations.Last();
		}

		private string GetWorkoutPrefix()
		{
			var prefix = this.CurrentWorkout.Prefix;
			if (this.Session.BodyArea == BodyArea.FullBody)
				prefix += "FB_";
			else if (this.Session.BodyArea == BodyArea.UpperBody)
				prefix += "UB_";
			else if (this.Session.BodyArea == BodyArea.LowerBody)
				prefix += "LB_";
			return prefix;
		}

		public ICommand SetDurationCommand { get { return new RelayCommand<int>(OnSetDuration); } }
		private void OnSetDuration(int duration)
		{
			this.Duration = duration;
		}

		public ICommand SetNumPlayersCommand { get { return new RelayCommand<int>(OnSetNumPlayers); } }
		private void OnSetNumPlayers(int numPlayers)
		{
			this.NumPlayers = numPlayers;
		}

		static Random rng = new Random();

		public ICommand NextCommand { get { return new RelayCommand(OnNext); } }
		private void OnNext()
		{
			var appLog = new EventLog() { Source = "Kiosk" };
			try { appLog.WriteEntry("Next command pressed."); } catch { }
			var list = this.WorkoutMap[this.Duration];
			this.Session.Workout = list[rng.Next(list.Length)];
			this.Session.SetNumPlayers(this.NumPlayers);
			try { appLog.WriteEntry(String.Format("Requesting session for {0} players.", this.NumPlayers)); } catch { }
			this.Parent.RequestSessionAvailable(this.NumPlayers);
		}

		public override void ProcessNetworkMessage(dynamic msg)
		{
			var appLog = new EventLog() { Source = "Kiosk" };
			try { appLog.WriteEntry(String.Format("Processing network message during choose workout: {0}.", msg.cmd.ToString())); } catch { }

			// server has responded telling us a session is available			
			if (msg.cmd == "session_available")
			{
				if (VersusKiosk.UI.Properties.Settings.Default.DemoMode)
					StartDemoMode();
				else
					this.Parent.SetPage(this.Injector.Get<EnterEmailViewModel>(new ConstructorArgument("session", this.Session), new ConstructorArgument("playerNum", 0)));
			}
			/*
			else
				this.Parent.SetPage(this.Injector.Get<IntroViewModel>()); // todo: notify the user - MJF
			*/
		}

		private void StartDemoMode()
		{
			for (int playerNum = 0; playerNum < this.Session.Players.Count(); playerNum++)
			{
				var player = this.Session.Players[playerNum];
				player.FirstName = String.Format("Player {0}", playerNum + 1);
				player.LastName = "";
				player.Email = "";
				player.Weight = "70";
			}
			this.Parent.SetPage(this.Injector.Get<StartViewModel>(new ConstructorArgument("session", this.Session)));
		}

		public ICommand LessCommand { get { return new RelayCommand(OnLess); } }
		private void OnLess()
		{
			this.NumPlayers = Math.Max(1, this.NumPlayers - 1);
		}

		public ICommand MoreCommand { get { return new RelayCommand(OnMore); } }
		private void OnMore()
		{
			this.NumPlayers = Math.Min(this.Parent.NumAvailableArcades, this.NumPlayers + 1);
		}

	}

}

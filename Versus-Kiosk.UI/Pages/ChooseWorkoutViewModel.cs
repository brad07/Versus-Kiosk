using GalaSoft.MvvmLight.Command;
using Ninject;
using Ninject.Parameters;
using System;
using System.Collections.Generic;
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

		private Workout[] _Workouts = new Workout[]
		{
			new Workout{Title = "Full Body", Name = "Arcade 1", Description = "An all-round routine that exercises all major muscle groups.", MediaFile = @".\Media\fullbody.wmv", StartTime=30},
			new Workout{Title = "High Intensity", Name = "Arcade 1", Description = "A fast-paced, challenging routine that will get your heart rate going.", MediaFile = @".\Media\intensity.wmv", StartTime=30},
			new Workout{Title = "Upper Body", Name = "Arcade 1", Description = "Specifically designed to build strength and overall upper-body fitness.", MediaFile = @".\Media\upperbody.wmv", StartTime=30},
			new Workout{Title = "Strength", Name = "Arcade 1", Description = "A slower-paced routine that concentrates on building all-round strength.", MediaFile = @".\Media\strength.wmv", StartTime=30},
		};

		public Workout[] Workouts
		{
			get { return _Workouts; }
			set { _Workouts = value; RaisePropertyChanged(() => this.Workouts); }
		}

		public Workout CurrentWorkout
		{
			get { return this.Session.Workout; }
			set { this.Session.Workout = value; RaisePropertyChanged(() => this.CurrentWorkout); }
		}

		private int[] _NumPlayerOptions = new int[] {1, 2, 3, 4};
		public int[] NumPlayerOptions
		{
			get { return _NumPlayerOptions; }
			set { _NumPlayerOptions = value; RaisePropertyChanged(() => this.NumPlayerOptions); }
		}

		private int _NumPlayers = 1;
		public int NumPlayers
		{
			get { return _NumPlayers; }
			set { _NumPlayers = value; RaisePropertyChanged(() => this.NumPlayers); }
		}
		

		public ChooseWorkoutViewModel(Session session)
		{
			this.Session = session;			
		}

		public override void Initialize()
		{
			base.Initialize();
			this.Session.Workout = this.Workouts.FirstOrDefault();
		}

		public ICommand NextCommand { get { return new RelayCommand(OnNext); } }
		private void OnNext()
		{
			this.Session.SetNumPlayers(this.NumPlayers);
			this.Parent.RequestSessionAvailable(this.NumPlayers);
		}

		public override void ProcessNetworkMessage(dynamic msg)
		{
			// server has responded telling us a session is available
			if (msg.cmd == "session_available")
				this.Parent.SetPage(this.Injector.Get<EnterEmailViewModel>(new ConstructorArgument("session", this.Session), new ConstructorArgument("playerNum", 0)));
			else
				this.Parent.SetPage(this.Injector.Get<IntroViewModel>()); // todo: notify the user - MJF
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

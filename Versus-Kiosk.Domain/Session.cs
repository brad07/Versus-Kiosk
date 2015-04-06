using GalaSoft.MvvmLight;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VersusKiosk.Domain.IoC;

namespace VersusKiosk.Domain
{
	public class Session : ViewModelBase
	{
		[Inject]
		public Injector Injector { get; set; }

		public int session_no {get; set;}
	
		private Player[] _Players;
		public Player[] Players
		{
			get { return _Players; }
			set { _Players = value; RaisePropertyChanged(() => this.Players); }
		}

		private BodyArea _BodyArea = BodyArea.FullBody;
		public BodyArea BodyArea
		{
			get { return _BodyArea; }
			set { _BodyArea = value; RaisePropertyChanged(() => this.BodyArea); }
		}

		private WorkoutType _WorkoutType;
		public WorkoutType WorkoutType
		{
			get { return _WorkoutType; }
			set { _WorkoutType = value; RaisePropertyChanged(() => this.WorkoutType); }
		}

		private string _Workout;
		public string Workout
		{
			get { return _Workout; }
			set { _Workout = value; RaisePropertyChanged(() => this.Workout); }
		}

		public void SetNumPlayers(int numPlayers)
		{
			this.Players = Enumerable.Range(1, numPlayers)
				.Select(playerNum => this.Injector.Get<Player>())
				.ToArray();
		}


	}
}

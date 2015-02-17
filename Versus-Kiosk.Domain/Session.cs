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

		private Player[] _Players;
		public Player[] Players
		{
			get { return _Players; }
			set { _Players = value; RaisePropertyChanged(() => this.Players); }
		}

		private Workout _Workout;
		public Workout Workout
		{
			get { return _Workout; }
			set { _Workout = value; RaisePropertyChanged(() => this.Workout); }
		}

		public Session()
		{
		}

		public void SetNumPlayers(int numPlayers)
		{
			this.Players = Enumerable.Range(1, numPlayers)
				.Select(playerNum => this.Injector.Get<Player>())
				.ToArray();

			/*
			this.Players = new Player[numPlayers];
			for (int i = 0; i < numPlayers; i++)
			{
				this.Players[i] = this.Injector.Get<Player>();
				switch (i)
				{
					case 0: this.Players[i].FirstName = "Matthew"; this.Players[i].LastName = "Michaelson"; break;
					case 1: this.Players[i].FirstName = "Mark"; this.Players[i].LastName = "Markson"; break;
					case 2: this.Players[i].FirstName = "Luke"; this.Players[i].LastName = "Lukeson"; break;
					case 3: this.Players[i].FirstName = "John"; this.Players[i].LastName = "Johnson"; break;
				}
			}
			 * */
		}


	}
}

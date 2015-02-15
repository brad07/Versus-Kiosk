using GalaSoft.MvvmLight.Command;
using Ninject.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VersusKiosk.Domain;
using VersusKiosk.Domain.IoC;

namespace VersusKiosk.UI.Pages
{
	public class PlayerDetailsViewModel : PageViewModel
	{
		private Session Session;
		private int PlayerNum;

		public int DisplayPlayerNum { get { return this.PlayerNum + 1; } }
		public Player Player {get; private set;}

		private string _FirstNameError = "";
		public string FirstNameError
		{
			get { return this._FirstNameError; }
			private set { this._FirstNameError = value; RaisePropertyChanged(() => this.FirstNameError); }
		}

		private string _LastNameError = "";
		public string LastNameError
		{
			get { return this._LastNameError; }
			private set { this._LastNameError = value; RaisePropertyChanged(() => this.LastNameError); }
		}

		private string _WeightError = "";
		public string WeightError
		{
			get { return this._WeightError; }
			private set { this._WeightError = value; RaisePropertyChanged(() => this.WeightError); }
		}

		public PlayerDetailsViewModel(Session session, int playerNum)
		{
			this.Session = session;
			this.PlayerNum = playerNum;
			this.Player = session.Players[playerNum];
		}

		public override void Initialize()
		{
			base.Initialize();
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
			this.FirstNameError = "";
			this.LastNameError = "";
			this.WeightError = "";
			
			if (String.IsNullOrEmpty(this.Player.FirstName.Trim()))
			{
				this.FirstNameError = "Please enter a first name";
				result = false;
			}

			if (String.IsNullOrEmpty(this.Player.LastName.Trim()))
			{
				this.LastNameError = "Please enter at least one surname initial";
				result = false;
			}

			int weight;
			if (!Int32.TryParse(this.Player.Weight.Trim(), out weight) || (weight < 40) || (weight > 200))
			{
				this.WeightError = "Please enter a valid weight";
				result = false;
			}

			return result;
		}
		
	}
}

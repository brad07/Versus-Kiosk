using GalaSoft.MvvmLight.Command;
using Ninject.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VersusKiosk.Domain;

namespace VersusKiosk.UI.Pages
{
	public class NicknameViewModel : PageViewModel
	{
		private Session Session;
		private int PlayerNum;

		public int DisplayPlayerNum { get { return this.PlayerNum + 1; } }
		public int NumPlayers { get { return this.Session.Players.Count(); } }
		public Player Player { get; private set; }

		private string _FirstNameError = "";
		public string FirstNameError
		{
			get { return this._FirstNameError; }
			private set { this._FirstNameError = value; RaisePropertyChanged(() => this.FirstNameError); }
		}

		public NicknameViewModel(Session session, int playerNum)
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
				this.Player.FirstName = this.Player.FirstName.Trim();
				this.Player.FirstName = this.Player.FirstName[0].ToString().ToUpper() + this.Player.FirstName.Substring(1);
				this.Player.LastName = " "; // todo: fix this in cc/station
				this.Parent.SetPage(this.Injector.Get<WeighInViewModel>(new ConstructorArgument("session", this.Session), new ConstructorArgument("playerNum", this.PlayerNum)));
			}
		}

		public bool ValidateInput()
		{
			var result = true;
			this.FirstNameError = "";

			if (String.IsNullOrEmpty(this.Player.FirstName.Trim()))
			{
				this.FirstNameError = "Please enter a nickname";
				result = false;
			}

			return result;
		}
	}
}

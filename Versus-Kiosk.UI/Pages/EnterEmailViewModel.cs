using GalaSoft.MvvmLight.Command;
using Ninject.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using VersusKiosk.Domain;
using VersusKiosk.UI.Helpers;

namespace VersusKiosk.UI.Pages
{
	public class EnterEmailViewModel : PageViewModel
	{
		private Session Session;
		private int PlayerNum;

		public int DisplayPlayerNum { get { return this.PlayerNum + 1; } }
		public Player Player { get { return this.Session.Players[this.PlayerNum]; } }

		private string _EmailError = "";
		public string EmailError
		{
			get { return this._EmailError;}
			private set { this._EmailError = value; RaisePropertyChanged(() => this.EmailError); }
		}

		public EnterEmailViewModel(Session session, int playerNum)
		{
			this.Session = session;
			this.PlayerNum = playerNum;
		}

		public ICommand OkCommand { get { return new RelayCommand(OnOk); } }
		private void OnOk()
		{
			if (this.ValidateInput())
				this.Parent.RequestPlayerDetails(this.Player);
		}

		public override void ProcessNetworkMessage(dynamic msg)
		{
			// server has responded telling us a session is available
			if (msg.cmd == "player_details")
			{
				// is this user in our database?
				if (msg.member != null)
				{
					// yes, so store their details
					this.Player.FirstName = msg.member.first_name;
					this.Player.LastName = msg.member.last_name;
					this.Player.Weight = msg.member.weight;

					// if there are more players to get then move on to the next one
					int nextPlayer = this.PlayerNum + 1;
					if (nextPlayer < this.Session.Players.Count())
						this.Parent.SetPage(this.Injector.Get<EnterEmailViewModel>(new ConstructorArgument("session", this.Session), new ConstructorArgument("playerNum", nextPlayer)));

					// otherwise start the session
					else
						this.Parent.SetPage(this.Injector.Get<StartViewModel>(new ConstructorArgument("session", this.Session)));
				}
				else
				{
					// user isn't in our database so ask them for their details
					this.Parent.SetPage(this.Injector.Get<PlayerDetailsViewModel>(new ConstructorArgument("session", this.Session), new ConstructorArgument("playerNum", this.PlayerNum)));
				}
			}
		}

		public bool ValidateInput()
		{
			var result = true;
			this.EmailError = "";
			var regex = new RegexUtilities();

			if ((!String.IsNullOrEmpty(Player.Email)) && (!regex.IsValidEmail(Player.Email)))
			{
				this.EmailError = "Please enter a valid email address";
				result = false;
			}

			// check if user is already in session
			if (!String.IsNullOrEmpty(this.Player.Email.ToLower().Trim()))
				for (int i = 0; i < this.PlayerNum; i++)
					if (this.Session.Players[i].Email.ToLower().Trim() == this.Player.Email.ToLower().Trim())
					{
						this.EmailError = "User already in session";
						result = false;
					}

			return result;
		}

	}
}

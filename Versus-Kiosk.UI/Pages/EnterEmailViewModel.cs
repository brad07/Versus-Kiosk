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
		public int NumPlayers { get { return this.Session.Players.Count(); } }
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

		public override void Initialize()
		{
			base.Initialize();
		}

		public ICommand OkCommand { get { return new RelayCommand(OnOk); } }
		private void OnOk()
		{
			if (this.ValidateInput())
			{
				// if these user has left the email field blank then they wish to remain anonymous, so ask for a nickname
				if (String.IsNullOrEmpty(this.Player.Email))
					this.Parent.SetPage(this.Injector.Get<NicknameViewModel>(new ConstructorArgument("session", this.Session), new ConstructorArgument("playerNum", this.PlayerNum)));

				// otherwise go through the signup/retrieval phase
				else
					this.Parent.RequestPlayerDetails(this.Player);
			}
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

					// get their latest weight
					this.Parent.SetPage(this.Injector.Get<WeighInViewModel>(new ConstructorArgument("session", this.Session), new ConstructorArgument("playerNum", this.PlayerNum)));
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

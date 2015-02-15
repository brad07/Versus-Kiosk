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
	public class ScanViewModel : PageViewModel
	{
		private Session Session;
		private int PlayerNum;

		public int DisplayPlayerNum { get { return this.PlayerNum + 1; } }

		public ScanViewModel(Session session, int playerNum)
		{
			this.Session = session;
			this.PlayerNum = playerNum;
		}

		public ICommand ScanCommand { get { return new RelayCommand(OnScan); } }
		private void OnScan()
		{
			this.Parent.SetPage(this.Injector.Get<PlayerDetailsViewModel>(new ConstructorArgument("session", this.Session), new ConstructorArgument("playerNum", this.PlayerNum)));
		}

	}

}

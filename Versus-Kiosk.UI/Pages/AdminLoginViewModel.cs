using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VersusKiosk.UI.Pages
{
	public class AdminLoginViewModel : PageViewModel
	{
		private string _Password = "";
		public string Password
		{
			get { return _Password; }
			set { _Password = value; RaisePropertyChanged(() => this.Password); }
		}

		public AdminLoginViewModel()
		{
		}

		public override void Initialize()
		{
			base.Initialize();
		}

		protected override void OnTick(TimeSpan elapsed)
		{
		}
		
		public ICommand NextCommand { get { return new RelayCommand(OnNext); } }
		private void OnNext()
		{
			if (this.Password == VersusKiosk.UI.Properties.Settings.Default.AdminPassword)
				this.Parent.SetPage(this.Injector.Get<AdminViewModel>());
			else
				this.Parent.SetPage(this.Injector.Get<IntroViewModel>());
		}

		public ICommand DigitCommand { get { return new RelayCommand<string>(OnDigit); } }
		private void OnDigit(string digit)
		{
			if (digit == "*")
			{
				if (this.Password.Count() > 0)
					this.Password = this.Password.Substring(0, this.Password.Count() - 1);
			}
			else if (digit == "#")
				this.Password = "";
			else
				this.Password += digit;
		}

	}
}

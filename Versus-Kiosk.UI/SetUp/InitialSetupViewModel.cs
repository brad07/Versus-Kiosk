using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MvvmDialogs.ViewModels;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VersusKiosk.UI.Main;
using VersusKiosk.UI.Properties;

namespace VersusKiosk.UI.SetUp
{
	public class InitialSetupViewModel : ViewModelBase, IUserDialogViewModel
	{
		public virtual bool IsModal { get { return true; } }
		public virtual void RequestClose() { this.DialogClosing(this, null); }
		public virtual event EventHandler DialogClosing;
		private bool Result = false;

		[Inject]
		public MainViewModel Parent { get; set; }

		private string _NetworkName = "Versus";
		public string NetworkName
		{
			get { return this._NetworkName; }
			set { this._NetworkName = value; RaisePropertyChanged(() => this.NetworkName); }
		}

		private string _AdminPassword = "2580";
		public string AdminPassword
		{
			get { return this._AdminPassword; }
			set { this._AdminPassword = value; RaisePropertyChanged(() => this.AdminPassword); }
		}

		private string _Mode = "Normal";
		public string Mode
		{
			get { return this._Mode; }
			set { this._Mode = value; RaisePropertyChanged(() => this.Mode); }
		}

		private string[] _Modes = new string[] {"Normal", "Demo"};
		public string[] Modes
		{
			get { return this._Modes; }
		}

		public InitialSetupViewModel()
		{
			this.NetworkName = Settings.Default.NetworkName;
			this.AdminPassword = Settings.Default.AdminPassword;
			this.Mode = Settings.Default.DemoMode ? "Demo" : "Normal";
		}

		public ICommand OkCommand { get { return new RelayCommand(OnOk); } }
		private void OnOk()
		{
			Settings.Default.NetworkName = this.NetworkName;
			Settings.Default.AdminPassword = this.AdminPassword;
			Settings.Default.DemoMode = (this.Mode == "Demo");
			Settings.Default.Save();
			this.Result = true;
			this.RequestClose();
		}

		public bool Show()
		{
			this.Parent.Dialogs.Add(this);
			return this.Result;
		}
	}
}

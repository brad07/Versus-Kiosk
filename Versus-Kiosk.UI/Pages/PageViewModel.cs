using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Ninject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using VersusKiosk.Domain.IoC;
using VersusKiosk.Domain.Network;
using VersusKiosk.UI.Main;

namespace VersusKiosk.UI.Pages
{
	public class PageViewModel : ViewModelBase, IInitializable
	{
		[Inject]
		public MainViewModel Parent { get; set; }

		[Inject]
		public Injector Injector { get; set; }

		private string _State;
		public string State
		{
			get { return _State; }
			set { _State = value; RaisePropertyChanged(() => this.State); }
		}

		private bool _Active = false;
		public bool Active
		{
			get
			{
				return this._Active;
			}
			set
			{
				if (this._Active != value)
				{
					this._Active = value;
					if (value)
					{
						this.StartTime = DateTime.Now;
						OnTick(TimeSpan.Zero);

						this.Timer = new DispatcherTimer();
						this.Timer.Interval = TimeSpan.FromSeconds(1);
						this.Timer.Tick += Timer_Tick;
						this.Timer.Start();
						OnActivating();
					}
					else
					{
						OnDeactivating();
						this.Timer.Stop();
						this.Timer.Tick -= Timer_Tick;
						this.Timer = null;
					}
				}
			}
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			OnTick(DateTime.Now - this.StartTime);
		}

		protected DateTime StartTime;
		protected DispatcherTimer Timer;

		public virtual void Initialize()
		{
		}

		public delegate void ActivatingDelegate();
		public event ActivatingDelegate Activating;

		protected virtual void OnActivating()
		{
			if (this.Activating != null)
				this.Activating();
			this.Parent.PropertyChanged += Parent_PropertyChanged;
		}

		public delegate void DeactivagingDelegate();
		public event DeactivagingDelegate Deactivating;

		protected virtual void OnDeactivating()
		{
			if (this.Deactivating != null)
				this.Deactivating();
			this.Parent.PropertyChanged -= Parent_PropertyChanged;
		}

		void Parent_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Connected")
			{
				if (this.Parent.Connected)
					OnConnected();
				else
					OnDisconnected();
			}
		}

		public virtual void OnConnected()
		{
		}

		public virtual void OnDisconnected()
		{
		}

		protected virtual void OnTick(TimeSpan elapsed)
		{
 			if (elapsed > TimeSpan.FromMinutes(1))
				this.Parent.SetPage(this.Injector.Get<IntroViewModel>());
		}

		public ICommand CancelCommand { get { return new RelayCommand(OnCancel); } }
		private void OnCancel()
		{
			this.Parent.SetPage(this.Injector.Get<IntroViewModel>());
		}

		public virtual void ProcessNetworkMessage(dynamic msg)
		{
		}
		
	}
}

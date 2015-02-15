using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VersusKiosk.UI.ViewModels
{
	// proivided for the benefit of the StationActionWindow class
	[Serializable]
	public class StationViewModel : ViewModelBase
	{
		[XmlElement("name")]
		public string name
		{
			get { return this._name; }
			set {
				this._name = value;
				RaisePropertyChanged(() => this.name);
			}
		}
		private string _name = "";

		[XmlElement("station_no")]
		public int station_no
		{
			get { return this._station_no; }
			set
			{
				this._station_no = value;
				RaisePropertyChanged(() => this.station_no);
			}
		}
		private int _station_no = 0;

		[XmlElement("disabled")]
		public bool disabled
		{
			get { return this._disabled; }
			set
			{
				this._disabled = value;
				RaisePropertyChanged(() => this.disabled);
			}
		}
		private bool _disabled = false;

		[XmlElement("disable_scoring")]
		public bool disable_scoring
		{
			get { return this._disable_scoring; }
			set
			{
				this._disable_scoring = value;
				RaisePropertyChanged(() => this.disable_scoring);
			}
		}
		private bool _disable_scoring = false;

		[XmlElement("connected")]
		public bool connected
		{
			get { return this._connected; }
			set
			{
				this._connected = value;
				RaisePropertyChanged(() => this.connected);
			}
		}
		private bool _connected = false;

	}
}

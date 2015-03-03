using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersusKiosk.Domain
{
	public class Player : ViewModelBase
	{
		private string _Email = "";
		public string Email
		{
			get { return _Email; }
			set { _Email = value; RaisePropertyChanged(() => this.Email); }
		}

		private string _FirstName = "";
		public string FirstName
		{
			get { return _FirstName; }
			set { _FirstName = value; RaisePropertyChanged(() => this.FirstName); }
		}

		private string _LastName = "";
		public string LastName
		{
			get { return _LastName; }
			set { _LastName = value; RaisePropertyChanged(() => this.LastName); }
		}

		private string _Weight = "";
		public string Weight
		{
			get { return _Weight; }
			set { _Weight = value; RaisePropertyChanged(() => this.Weight); }
		}

		public string FullName
		{
			get
			{
				var name = this.FirstName;
				if (!String.IsNullOrEmpty(this.LastName.Trim()))
					name += " " + this.LastName[0];
				return name;
			}
		}

		public override string ToString()
		{
			var result = String.Join(" ", new string[] { this.FirstName, this.LastName }).Trim();
			if (String.IsNullOrEmpty(result))
				result = "<Empty>";
			return result;
		}
	}
}

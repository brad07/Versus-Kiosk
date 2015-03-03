using HidLibrary;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersusKiosk.UI.Hardware
{
	public class Scales : IInitializable
	{
		private HidDevice Device;

		public bool Connected { get { return (this.Device != null) && this.Device.IsConnected; } }

		public void Initialize()
		{
			Connect();
		}

		public void Connect()
		{
			if (this.Connected)
				return;

			try
			{
				// find the scales
				var hidDeviceList = HidDevices.Enumerate(0x922, 0x800B);

				// add any leostick raw-hid devices that we find (this allows a leostick to be used as a dummy scale during development and testing)
				hidDeviceList = hidDeviceList.Concat(
					HidDevices.Enumerate(0x2341, 0x8036).Where(d => d.Capabilities.Usage == 0x0C00)	// 0x0C00==RAWHID_USAGE
					);

				this.Device = hidDeviceList.FirstOrDefault();
				if (this.Device == null)
					return;
				this.Device.Removed += () => this.Device = null;
				this.Device.OpenDevice();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception in connect scales: " + ex.Message);
				this.Device = null;
			}
		}

		public double Read()
		{
			try
			{
				if (this.Connected)
				{
					var inData = this.Device.Read(50);
					var weight = ((inData.Data[5] * 255) + inData.Data[4]) / 10.0f;
					return weight;
				}
				else
					return 0.0;
			}
			catch
			{
				return 0.0;
			}
		}
	}
}

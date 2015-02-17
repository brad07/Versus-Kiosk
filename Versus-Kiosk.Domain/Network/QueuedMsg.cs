using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersusKiosk.Domain.Network
{
	public class QueuedMsg
	{

		public dynamic msg { get; set; }
		public string ip_address { get; set; }
		public int delay { get; set; }
		public int ttl { get; set; }
		public DateTime submitted_at { get; set; }

		public QueuedMsg()
		{
			submitted_at = DateTime.Now;
		}

	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersusKiosk.Domain
{
	public class WorkoutType
	{
		public string Title { get; set; }
		public string Prefix { get; set; }
		public string Description { get; set; }
		public string MediaFile { get; set; }
		public int StartTime { get; set; }			
	}
}
